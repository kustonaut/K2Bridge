// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

namespace K2Bridge.Models.Request.Aggregations
{
    using K2Bridge.Visitors;

    internal abstract class Aggregation : KustoQLBase, IVisitable
    {
        public string Key { get; set; }

        public abstract void Accept(IVisitor visitor);
    }
}
