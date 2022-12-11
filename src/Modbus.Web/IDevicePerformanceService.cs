using Modbus.Models;

namespace Modbus.Web
{
    public interface IDevicePerformanceService
    {
        Task<DevicePerformance> CreateDevicePerformance(DevicePerformance devicePerformance);
    }
}