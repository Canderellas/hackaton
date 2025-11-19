using System.Text.Json.Serialization;

namespace Backend_hackaton.Models.DTOs
{
    public class OperationLogDTO
    {
        [JsonPropertyName("place")]
        public string Place { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTimeOffset Date { get; set; }

        [JsonPropertyName("comment")]
        public string Comment { get; set; } = string.Empty;
    }
}
