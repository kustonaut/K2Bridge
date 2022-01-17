// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace UnitTests.K2Bridge.Visitors
{
    using System.Collections.Generic;
    using global::K2Bridge.Models.Request.Aggregations;
    using global::K2Bridge.Models.Request.Queries;
    using global::K2Bridge.Tests.UnitTests.Visitors;
    using global::K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class FiltersVisitorTests
    {
        [TestCase("foo:bar", "bar:baz", ExpectedResult = "\nlet _extdata = _data\n| extend ['key%Zm9vOmJhcg--%YmFyOmJheg--'] = pack_array('foo:bar','bar:baz'), ['_filter_value'] = pack_array((foo has \"bar\"),(bar has \"baz\"))\n| mv-expand ['key%Zm9vOmJhcg--%YmFyOmJheg--'] to typeof(string), ['_filter_value']\n| where ['_filter_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key%Zm9vOmJhcg--%YmFyOmJheg--']\n| order by ['key%Zm9vOmJhcg--%YmFyOmJheg--'] asc;")]
        [TestCase("foo:(bar OR baz)", "bar:(baz OR foo)", ExpectedResult = "\nlet _extdata = _data\n| extend ['key%Zm9vOihiYXIgT1IgYmF6KQ--%YmFyOihiYXogT1IgZm9vKQ--'] = pack_array('foo:(bar OR baz)','bar:(baz OR foo)'), ['_filter_value'] = pack_array(((foo has \"bar\") or (foo has \"baz\")),((bar has \"baz\") or (bar has \"foo\")))\n| mv-expand ['key%Zm9vOihiYXIgT1IgYmF6KQ--%YmFyOihiYXogT1IgZm9vKQ--'] to typeof(string), ['_filter_value']\n| where ['_filter_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key%Zm9vOihiYXIgT1IgYmF6KQ--%YmFyOihiYXogT1IgZm9vKQ--']\n| order by ['key%Zm9vOihiYXIgT1IgYmF6KQ--%YmFyOihiYXogT1IgZm9vKQ--'] asc;")]
        [TestCase("foo", "*", ExpectedResult = "\nlet _extdata = _data\n| extend ['key%Zm9v%Kg--'] = pack_array('foo','*'), ['_filter_value'] = pack_array((* has \"foo\"),(true))\n| mv-expand ['key%Zm9v%Kg--'] to typeof(string), ['_filter_value']\n| where ['_filter_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key%Zm9v%Kg--']\n| order by ['key%Zm9v%Kg--'] asc;")]
        [TestCase("foo", "bar:*", ExpectedResult = "\nlet _extdata = _data\n| extend ['key%Zm9v%YmFyOio-'] = pack_array('foo','bar:*'), ['_filter_value'] = pack_array((* has \"foo\"),(bar matches regex \"(.)*\"))\n| mv-expand ['key%Zm9v%YmFyOio-'] to typeof(string), ['_filter_value']\n| where ['_filter_value'] == true;\nlet _summarizablemetrics = _extdata\n| summarize wibble by ['key%Zm9v%YmFyOio-']\n| order by ['key%Zm9v%YmFyOio-'] asc;")]
        public string FiltersVisit_WithAggregation_ReturnsValidResponse(string q1, string q2)
        {
            var rangeAggregation = new FiltersAggregation()
            {
                Metric = "wibble",
                Key = "key",
                Filters = new Dictionary<string, FiltersBoolQuery>()
                {
                    [q1] = new FiltersBoolQuery
                    {
                        BoolQuery = new BoolQuery()
                        {
                            Must = new List<QueryStringClause>()
                            {
                                new QueryStringClause()
                                {
                                    Phrase = q1,
                                    Wildcard = true,
                                    Default = "*",
                                },
                            },
                        },
                    },
                    [q2] = new FiltersBoolQuery
                    {
                        BoolQuery = new BoolQuery()
                        {
                            Must = new List<QueryStringClause>()
                            {
                                new QueryStringClause()
                                {
                                    Phrase = q2,
                                    Wildcard = true,
                                    Default = "*",
                                },
                            },
                        },
                    },
                },
            };

            var visitor = VisitorTestsUtils.CreateAndVisitRootVisitor("dayOfWeek", "double");
            visitor.Visit(rangeAggregation);

            return rangeAggregation.KustoQL;
        }
    }
}