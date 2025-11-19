using Backend_hackaton.Models;
using Backend_hackaton.Models.DTOs;

namespace Backend_hackaton.Services
{
    public interface IDeviceService
    {
        Task<DeviceResponse> GetDevice(string deviceId);
    }
}
