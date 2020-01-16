﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace VisitorsTests
{
    using K2Bridge.Models.Request;
    using K2Bridge.Visitors;
    using NUnit.Framework;

    [TestFixture]
    public class TestSortClauseVisitor
    {
        [TestCase(ExpectedResult = "")]
        public string IgnoresClausesWithUnderscore()
        {
            var sortClause = new SortClause() { FieldName = "_wibble" };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(sortClause);

            return sortClause.KQL;
        }

        [TestCase(ExpectedResult = "wibble asc")]
        public string GeneratesClauseQuery()
        {
            var sortClause = new SortClause() { FieldName = "wibble", Order = "asc" };

            var visitor = new ElasticSearchDSLVisitor();
            visitor.Visit(sortClause);

            return sortClause.KQL;
        }
    }
}