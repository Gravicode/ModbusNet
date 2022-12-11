using System;
using System.Net;
using System.Net.Sockets;
using NModbus;
using System.Net;
using System.Net.Sockets;

namespace Modbus.Shared
{
    public interface IModbusClient
    {

        void CreateMaster(IPAddress ip, int port);
        void Write(byte unitId, ushort address, ushort[] data);
        void Dispose();
    }
    public class ModbusClient:IDisposable,IModbusClient
    {
        private readonly IModbusFactory _modbusFactory;
        private IModbusMaster _master;
        private TcpClient _tcpClient;
        public ModbusClient()
        {
            _modbusFactory = new ModbusFactory();
        }

        public void CreateMaster(IPAddress ip, int port)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(ip, port);
            _master = _modbusFactory.CreateMaster(_tcpClient);
        }
        public void Write(byte unitId, ushort address, ushort[] data)
        {
            _master.WriteMultipleRegisters(unitId, address, data);
        }

        public void Dispose()
        {
            _master?.Dispose();
            _master = null;
            _tcpClient?.Close();
            _tcpClient = null;
        }


    }
}

