using Modbus.Shared;
using Modbus.Slave;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddSingleton<IModbusServer>(new ModbusServer());
        services.AddSingleton<IModbusClient>(new ModbusClient());
        services.AddSingleton<IPiDevicePerformanceInfo>(new PiDevicePerformanceInfo());

        services.AddHostedService<Worker>();
    })
    .Build();

host.Run();

