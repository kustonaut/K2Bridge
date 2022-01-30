// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// See LICENSE file in the project root for full license information.

using K2Bridge.Visitors;

namespace K2Bridge.Models.Request.Aggregations.Bucket;

/// <summary>
/// Default bucket aggregation.
/// </summary>
internal class DefaultAggregation : BucketAggregation
{
    /// <inheritdoc/>
    public override void Accept(IVisitor visitor)
    {
        visitor.Visit(this);
    }
}
