using Modbus.Models;
using Modbus.Shared;
using System.Net;

namespace Modbus.Slave;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IModbusServer _modbusServer;
    private readonly IModbusClient _modbusClient;
    private readonly IPiDevicePerformanceInfo _devicePerformance;
    private byte _unitId = 1;
    public Worker(ILogger<Worker> logger, IModbusServer modbusServer,
        IModbusClient modbusClient,
        IPiDevicePerformanceInfo piDevicePerformance)
    {
        _modbusClient = modbusClient;
        _modbusServer = modbusServer;
        _devicePerformance = piDevicePerformance;
        _logger = logger;

    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var host = Dns.GetHostEntry(Dns.GetHostName());
        var ipAddress = host.AddressList.Where(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork).FirstOrDefault() ?? IPAddress.Parse("127.0.0.1");
        var port = 10502;
        _modbusServer.CreateServer(ipAddress, port);
        _modbusServer.AddSlave(_unitId);
        _modbusServer.StartServer();
        _modbusClient.CreateMaster(ipAddress, port);
        return base.StartAsync(cancellationToken);
    }
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _modbusClient.Dispose();
        _modbusServer.Dispose();
        return base.StopAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

            var performanceData = _devicePerformance.GetPerformanceInfo();
            WriteData(performanceData);
            _logger.LogInformation($"Cpu Usage:{performanceData.CpuUsage} -- Cpu Temperature:{performanceData.CpuTemperature} -- Ram Usage:{performanceData.MemoryUsage} -- TimeStamp:{DateTime.Now}");
            await Task.Delay(5000, stoppingToken);
        }
    }
    private void WriteData(DevicePerformance performance)
    {
        var props = typeof(DevicePerformance).GetProperties();
        ushort address = 18000;
        foreach (var prop in props)
        {
            Console.WriteLine(prop.PropertyType.ToString());
            if (!prop.PropertyType.ToString().Contains( "DateTime"))
            {
                var value = Convert.ToSingle(prop.GetValue(performance));
                var convertedValue = value.ToUnsignedShortArray();
                _modbusClient.Write(_unitId, address, convertedValue);
                address += 1;
            }
        }
    }
}

