using Backend_hackaton.Models;
using Backend_hackaton.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Backend_hackaton.Services
{
    public class DeviceService : IDeviceService
    {
        private readonly PostgresContext _context;
        public DeviceService(PostgresContext context)
        {
            _context = context;
        }
        public async Task<DeviceResponse?> GetDevice(string deviceId)
        {
            Guid? searchId = null;
            try
            {
               searchId = Guid.Parse(deviceId);
            }
            catch
            {
                return null;
            }
            Device? device = await _context.Devices
            .Include(c => c.FkModelNavigation)
                .ThenInclude(n => n.FkTypeNavigation)
            .Include(c => c.OperationalLogs.OrderBy(p => p.DateOperation))
            .Include(c => c.DevicePropertyValues)
                .ThenInclude(b => b.FkModelPropertyNavigation)
                    .ThenInclude(d => d.FkPropertyNavigation)
            .FirstOrDefaultAsync(d => d.Id == searchId);
            if (device != null)
            {
                return MapToResponse(device);
            }
            else
            {
                return null;
            }
        }
        private DeviceResponse MapToResponse(Device device)
        {
            return new DeviceResponse
            {
                Id = device.Id.ToString(),
                Name_model = device.FkModelNavigation.Name ?? "",
                Name_type = device.FkModelNavigation.FkTypeNavigation.Name ?? "",
                Description_model = device.FkModelNavigation.Description ?? "",
                Description_type = device.FkModelNavigation.FkTypeNavigation.Description ?? "",
                Operation_logs = device.OperationalLogs.Select(op => new OperationLogDTO
                {
                    Place = op.Place,
                    Date = op.DateOperation,
                    Comment = op.Comment
                }).ToList(),
                Properties = device.DevicePropertyValues?.Select(pv => new PropertyDTO
                {
                    Name = pv.FkModelPropertyNavigation.FkPropertyNavigation.Name,
                    Value = pv.Value
                }).ToList()
            };
        }
    }
}
