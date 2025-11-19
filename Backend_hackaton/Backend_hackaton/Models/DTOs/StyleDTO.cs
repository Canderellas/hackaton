using System.Text.Json.Serialization;

namespace Backend_hackaton.Models.DTOs
{
    public class StyleDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
}
