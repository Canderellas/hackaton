using Backend_hackaton.Models.DTOs;
using Backend_hackaton.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend_hackaton.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeviceController : Controller
    {
        private readonly IDeviceService _deviceService;

        public DeviceController(IDeviceService deviceService)
        {
            _deviceService = deviceService;
        }
        [HttpGet("{deviceId}")]
        public async Task<ActionResult<DeviceResponse>> GetDevice(string deviceId)
        {
            DeviceResponse? device = await _deviceService.GetDevice(deviceId);
            if (device == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(device);
            }
        }
    }
}
