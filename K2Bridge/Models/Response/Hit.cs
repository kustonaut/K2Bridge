namespace K2Bridge.Models.Response
{
    using System.Data;
    using K2Bridge.KustoConnector;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Collections.Generic;
    using System.Dynamic;
    using System;

    public class Hit
    {
        private const string TYPE = "_doc";
        private const int VERSION = 1;

        [JsonProperty("_index")]
        public string Index { get; set; }

        [JsonProperty("_type")]
        public string Type { get; } = TYPE;

        [JsonProperty("_id")]
        public string Id { get; set; }

        [JsonProperty("_version")]
        public int Version { get; set; } = VERSION;

        [JsonProperty("_score")]
        public object Score { get; set; }

        [JsonProperty("_source")]
        public JObject Source { get; } = new JObject();

        [JsonProperty("fields", NullValueHandling = NullValueHandling.Ignore)]
        public Fields Fields { get; set; }

        [JsonProperty("sort", NullValueHandling = NullValueHandling.Ignore)]
        public long[] Sort { get; set; }

        [JsonProperty("highlight", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, object> Highlight { get; set; }

        public void AddSource(string keyName, object value)
        {
            this.Source.Add(keyName, value == null ? null : JToken.FromObject(value));
        }

        public static Hit Create(IDataRecord record, QueryData query)
        {
            var hit = new Hit() { Index = query.IndexName };
            hit.Highlight = new Dictionary<string, object>();

            for (int index = 0; index < record.FieldCount; index++)
            {
                var name = record.GetName(index);
                var value = record.ReadValue(index);
                hit.AddSource(name, value);

                if (query.HighlightText == null) {
                    continue;
                }

                // Elastic only highlights string values, but we try to highlight everything we can here.
                // To mimic elastic: check for type of value here and skip if != string.
                if ((query.HighlightText.ContainsKey(name) && query.HighlightText[name].Equals(value.ToString(), StringComparison.OrdinalIgnoreCase)) ||
                    (query.HighlightText.ContainsKey("*") && query.HighlightText["*"].Equals(value.ToString(), StringComparison.OrdinalIgnoreCase))) {
                       hit.Highlight.Add(name, new List<string> { query.HighlightPreTag + value.ToString() + query.HighlightPostTag });
                }
            }

            return hit;
        }
    }
}