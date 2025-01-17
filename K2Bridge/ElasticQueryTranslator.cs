// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge;

using System;
using System.Collections.Generic;
using System.Linq;
using K2Bridge.Models;
using K2Bridge.Models.Request;
using K2Bridge.Models.Request.Aggregations.Bucket;
using K2Bridge.Models.Request.Queries;
using K2Bridge.Telemetry;
using K2Bridge.Visitors;
using Lucene.Net.QueryParsers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

/// <summary>
/// ElasticQueryTranslator provides the functionality for translating a Kibana query into Kusto query.
/// </summary>
internal class ElasticQueryTranslator : ITranslator
{
    private readonly IVisitor visitor;

    /// <summary>
    /// Initializes a new instance of the <see cref="ElasticQueryTranslator"/> class.
    /// </summary>
    /// <param name="visitor">The visitor to accept the translation request.</param>
    /// <param name="logger">Logger.</param>
    public ElasticQueryTranslator(IVisitor visitor, ILogger<ElasticQueryTranslator> logger)
    {
        this.visitor = visitor;
        Logger = logger;
    }

    private ILogger Logger { get; set; }

    /// <summary>
    /// Translate a given request into QueryData.
    /// </summary>
    /// <param name="header">A header.</param>
    /// <param name="query">A query.</param>
    /// <returns>A <see cref="QueryData"/>.</returns>
    /// <exception cref="TranslateException">Throws a TranslateException on error.</exception>
    public QueryData TranslateQuery(string header, string query)
    {
        Ensure.IsNotNullOrEmpty(header, nameof(header));

        try
        {
            Logger.LogDebug("Translate params: header:{@header}, query:{@query}", header, query.ToSensitiveData());

            // Prepare the esDSL object, except some fields such as the query field which will be built later
            var elasticSearchDsl = JsonConvert.DeserializeObject<ElasticSearchDSL>(query);

            // deserialize the headers and extract the index name
            // Todo: Consolidate json (de)serializations framework
            var headerDictionary = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(header);

            Ensure.IsNotNull(elasticSearchDsl.Query, nameof(elasticSearchDsl.Query));

            elasticSearchDsl.IndexName = headerDictionary["index"];
            elasticSearchDsl.HighlightText = new Dictionary<string, string>();

            List<string> sortFields = null;
            List<string> docValueFields = null;

            if (elasticSearchDsl.Query.Bool != null)
            {
                Ensure.IsNotNull(elasticSearchDsl.Query.Bool.Must, nameof(elasticSearchDsl.Query.Bool.Must));
                Ensure.IsNotNull(elasticSearchDsl.Query.Bool.Filter, nameof(elasticSearchDsl.Query.Bool.Filter));

                foreach (var element in elasticSearchDsl.Query.Bool.Must.Concat(elasticSearchDsl.Query.Bool.Filter))
                {
                    switch (element)
                    {
                        case QueryStringClause queryStringClause:
                            elasticSearchDsl.HighlightText.Add("*", queryStringClause.Phrase);
                            break;
                        case MatchPhraseClause matchPhraseClause:
                            elasticSearchDsl.HighlightText.Add(matchPhraseClause.FieldName, QueryParser.Escape(matchPhraseClause.Phrase.ToString()));
                            break;
                    }
                }

                sortFields = new List<string>();
                elasticSearchDsl.Sort?.ForEach(clause => sortFields.Add(clause.FieldName));
            }
            else if (elasticSearchDsl.Query.Ids != null)
            {
                EnsureClause.IsNotNull(elasticSearchDsl.Query.Ids.Id, nameof(elasticSearchDsl.Query.Ids.Id));
                Ensure.ConditionIsMet(elasticSearchDsl.Query.Ids.Id.Length == 1, $"{nameof(elasticSearchDsl.Query.Ids.Id)} must include exactly one value");
            }
            else
            {
                throw new IllegalClauseException("Either Bool or Ids clauses must not be null");
            }

            docValueFields = new List<string>();
            elasticSearchDsl.DocValueFields?.ForEach(item => docValueFields.Add(item.Field));

            // Use the visitor and build the KustoQL string from the esDSL object
            elasticSearchDsl.Accept(visitor);

            var queryData = new QueryData(
                elasticSearchDsl.KustoQL,
                elasticSearchDsl.IndexName,
                sortFields,
                docValueFields,
                elasticSearchDsl.HighlightText);

            if (elasticSearchDsl.Highlight != null)
            {
                Ensure.IsNotNullOrEmpty(elasticSearchDsl.Highlight.PreTags, nameof(elasticSearchDsl.Highlight.PreTags));
                Ensure.IsNotNullOrEmpty(elasticSearchDsl.Highlight.PostTags, nameof(elasticSearchDsl.Highlight.PostTags));

                queryData.HighlightPreTag = elasticSearchDsl.Highlight.PreTags[0];
                queryData.HighlightPostTag = elasticSearchDsl.Highlight.PostTags[0];
            }

            var aggregations = elasticSearchDsl.Aggregations;

            // If PrimaryAggregation is BucketAggregation, we add its key and type to QueryData
            if (aggregations?.Count == 1 && aggregations.First().Value.PrimaryAggregation is BucketAggregation)
            {
                var (key, aggregationContainer) = aggregations.First();
                queryData.PrimaryAggregation = KeyValuePair.Create(key, aggregationContainer.PrimaryAggregation.GetType().Name);
            }

            return queryData;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to execute translate operation.");
            throw new TranslateException("Failed translating elasticsearch query", ex);
        }
    }
}
