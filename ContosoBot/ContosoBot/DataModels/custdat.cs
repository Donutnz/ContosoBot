using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace ContosoBot.DataModels {
    public class custdat {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }

        [JsonProperty(PropertyName = "createdat")]
        public string createdAt { get; set; }

        [JsonProperty(PropertyName = "updatedat")]
        public string updatedAt { get; set; }

        [JsonProperty(PropertyName = "version")]
        public string version { get; set; }

        [JsonProperty(PropertyName = "deleted")]
        public bool deleted { get; set; }

        [JsonProperty(PropertyName = "title")]
        public object title { get; set; }

        [JsonProperty(PropertyName = "firstname")]
        public string firstname { get; set; }

        [JsonProperty(PropertyName = "lastname")]
        public string lastname { get; set; }

        [JsonProperty(PropertyName = "balance")]
        public int balance { get; set; }

        [JsonProperty(PropertyName = "username")]
        public string username { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string password { get; set; }

        [JsonProperty(PropertyName = "occupation")]
        public string occupation { get; set; }
    }
}