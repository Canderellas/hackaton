namespace Backend_hackaton.Models.DTOs
{
    public class DeviceResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name_model { get; set; } = string.Empty;
        public string Name_type { get; set; } = string.Empty;
        public string Description_model { get; set; } = string.Empty;
        public string Description_type { get; set; } = string.Empty;
        public List<OperationLogDTO> Operation_logs { get; set; } = new();
        public List<PropertyDTO> Properties { get; set; } = new();
    }
}
