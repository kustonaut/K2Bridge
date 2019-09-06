﻿namespace K2Bridge.KustoConnector
{
    using System;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class ElasticResponse
    {
        public Response[] responses { get; set; }
    }

    public class Response
    {
        public int took { get; set; }
        public bool timed_out { get; set; }
        public _Shards _shards { get; set; }
        public Hits hits { get; set; }
        public Aggregations aggregations { get; set; }
        public int status { get; set; }
    }

    public class _Shards
    {
        public int total { get; set; }
        public int successful { get; set; }
        public int skipped { get; set; }
        public int failed { get; set; }
    }

    public class Hits
    {
        public int total { get; set; }
        public object max_score { get; set; }
        public Hit[] hits { get; set; }
    }

    public class Hit
    {
        public string _index { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string _type { get; set; }
        public string _id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int _version { get; set; }
        public object _score { get; set; }
        public JObject _source { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Fields fields { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public long[] sort { get; set; }
    }

    public class IndexPattern
    {
        public string title { get; set; }

        public string timeFieldName { get; set; }

        public string fieldFormatMap { get; set; }

        public string fields { get; set; }
    }

    public class Source
    {
        [JsonProperty(PropertyName = "index-pattern")]
        public IndexPattern index_pattern { get; set; }
        public string type { get; set; }
        public string updated_at { get; set; }
        public MigrationVersion migrationVersion { get; set; }
    }

    public class Fields
    {
        public int[] hour_of_day { get; set; }
        public DateTime[] timestamp { get; set; }
    }

    public class MigrationVersion
    {
        [JsonProperty(PropertyName = "index-pattern")]
        public string index_pattern { get; set; }
    }

    public class Aggregations
    {
        [JsonProperty(PropertyName = "2")]
        public _2 _2 { get; set; }
    }

    public class _2
    {
        public DateHistogramBucket[] buckets { get; set; }
    }

    public class Bucket
    {
        public DateTime key_as_string { get; set; }
        public long key { get; set; }
        public int doc_count { get; set; }
    }

}