using Newtonsoft.Json;
using System;

namespace MyCommonLibrary
{
    public class BaseModel
    {
        public BaseModel()
        {
            Id = Guid.NewGuid().ToString();
        }

        [JsonProperty("id")]
        public string Id { get; set; }
    }
}
