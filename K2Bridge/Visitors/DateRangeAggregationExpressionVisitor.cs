// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Visitors
{
    using System;
    using System.Text.RegularExpressions;
    using K2Bridge.Models.Request;
    using K2Bridge.Models.Request.Aggregations;

    /// <content>
    /// A visitor for the <see cref="RangeAggregationExpression"/> element.
    /// </content>
    internal partial class ElasticSearchDSLVisitor : IVisitor
    {
        /// <inheritdoc/>
        public void Visit(DateRangeAggregationExpression dateRangeExpression)
        {
            Ensure.IsNotNull(dateRangeExpression, nameof(dateRangeExpression));
            EnsureClause.StringIsNotNullOrEmpty(dateRangeExpression.Field, nameof(dateRangeExpression.Field));

            var fromExpr = dateRangeExpression.From != null ? DateMathParser.ParseDateMath(dateRangeExpression.From) : "''";
            var toExpr = dateRangeExpression.To != null ? DateMathParser.ParseDateMath(dateRangeExpression.To) : "''";

            var gteExpr = dateRangeExpression.From != null ? $"{dateRangeExpression.Field} >= {fromExpr}" : null;
            var ltExpr = dateRangeExpression.To != null ? $"{dateRangeExpression.Field} < {toExpr}" : null;

            dateRangeExpression.BucketNameKustoQL = $"strcat({fromExpr}, '_', {toExpr})";

            dateRangeExpression.KustoQL = (gteExpr, ltExpr) switch
            {
                (null, null) => string.Empty,
                (null, _) => ltExpr,
                (_, null) => gteExpr,
                (_, _) => $"{gteExpr} {KustoQLOperators.And} {ltExpr}",
            };
        }
    }
}