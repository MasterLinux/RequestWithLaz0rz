using Newtonsoft.Json;

namespace Jamendo.Request.Model
{
    public class BaseRequestModel
    {
        [JsonProperty("headers")]
        public BaseRequestHeaderModel Headers { get; set; }
    }

    public class BaseRequestHeaderModel
    {
        [JsonProperty("status")]
        public string Status { get; set; }
        public int Code { get; set; }
        public string ErrorMessage { get; set; }
        public string Warnings { get; set; }
        public int ResultCount { get; set; }
    }
}
