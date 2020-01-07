using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Models.LearningNetcoreWebApp
{
    public class BaseAutoCompleteDto
    {

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }

    public abstract class PayLoad : BaseAutoCompleteDto { }

    public class Suggest
    {
        [JsonProperty("result")]
        public string Result { get; set; }
        [JsonProperty("payload")]
        public PayLoad Payload { get; set; }
    }

    public class CarResultsDTO : PayLoad
    {
        [JsonProperty("makeId")]
        public string MakeId { get; set; }
        [JsonProperty("makeName")]
        public string MakeName { get; set; }
        [JsonProperty("modelId")]
        public string ModelId { get; set; }
        [JsonProperty("modelName")]
        public string ModelName { get; set; }
    }
}
