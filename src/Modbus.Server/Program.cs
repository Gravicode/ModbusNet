// See https://aka.ms/new-console-template for more information
using Modbus.Models;
using NModbus;
using System.Configuration;
using System.Net.Sockets;
using Modbus.Shared;
using System.Net.Http.Json;

Console.WriteLine($"{DateTime.Now} - Modbus Service Running, get data from modbus server and push to api..");

HttpClient _client = new HttpClient();

var linuxServiceIp = ConfigurationManager.AppSettings["LinuxServiceIp"];
var linuxServicePort = Convert.ToInt32(ConfigurationManager.AppSettings["LinuxServicePort"]);
var dataStoreServiceUrl = ConfigurationManager.AppSettings["DataStoreUrl"];

var modbusFactory = new ModbusFactory();
var tcpClient = new TcpClient();
IModbusMaster master = modbusFactory.CreateMaster(tcpClient);
tcpClient.ConnectAsync(linuxServiceIp, linuxServicePort);
master.Transport.Retries = 3;
master.Transport.WaitToRetryMilliseconds = 1000;
master.Transport.SlaveBusyUsesRetryCount = true;



while (true)
{
    if (!tcpClient.Connected)
    {
        Thread.Sleep(1000);
        continue;
    }

    var dataList = new List<float>();
    for (ushort i = 0; i < 3; i++)
    {
        var result = master.ReadHoldingRegisters(1, (ushort)(18000 + i), 2);
        dataList.Add(result.ToFloat());
    }
    var sysPerformData = new DevicePerformance()
    {
        CpuUsage = (float)Math.Round(dataList[0], 2),
        MemoryUsage = (float)Math.Round(dataList[1], 2),
        CpuTemperature = (float)Math.Round(dataList[2], 2),
        TimeStamp = DateTime.Now
    };

    _client.PostAsJsonAsync(dataStoreServiceUrl + "/deviceperformance", sysPerformData);
    Console.WriteLine($"Cpu Usage:{sysPerformData.CpuUsage} -- Cpu Temperature:{sysPerformData.CpuTemperature} -- Ram Usage:{sysPerformData.MemoryUsage} -- TimeStamp:{sysPerformData.TimeStamp}");
    Thread.Sleep(6000);
}
