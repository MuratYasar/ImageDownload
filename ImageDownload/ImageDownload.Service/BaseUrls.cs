using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageDownload.Service
{
    [JsonObject("baseUrls")]
    public class BaseUrls
    {
        [JsonProperty("UrlName")]
        public string UrlName { get; set; }

        [JsonProperty("Count")]
        public int Count { get; set; }

        [JsonProperty("Parallelism")]
        public int Parallelism { get; set; }

        [JsonProperty("SavePath")]
        public string SavePath { get; set; }
    }
}
