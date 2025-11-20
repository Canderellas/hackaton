using System.Text.Json.Serialization;

namespace Backend_hackaton.Models.DTOs
{
    public class PropertyDTO
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
        [JsonPropertyName("styles")]
        public List<StyleDTO> Styles { get; set; } = new List<StyleDTO>();
    }
}
