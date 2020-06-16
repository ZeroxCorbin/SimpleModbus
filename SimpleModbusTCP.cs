using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleModbus
{
    public class SimpleModbusTCP : IDisposable
    {
        public delegate void ErrorEventHandler(object sender, Exception data);
        public event ErrorEventHandler Error;

        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler Message;

        public SocketManagerNS.SocketManager Socket { get; private set; }
        public bool IsConnected => Socket.IsConnected;

        public bool Connect(string ip, int port = 502)
        {
            Socket = new SocketManagerNS.SocketManager($"{ip}:{port}");
            try
            {
                Socket.Connect(true);
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }

            if (Socket.IsConnected)
                return true;
            else
                return false;
        }
        public void Disconnect() => Socket?.Disconnect();

        private SimpleModbusCore.MBAP PreWrite(SimpleModbusCore.PublicFunctionCodes functionCode, int addr, int value) => Write(new SimpleModbusCore.MBAP(new SimpleModbusCore.ADU_FunctionRequest(functionCode, addr, value)));
        private SimpleModbusCore.MBAP PreWrite(SimpleModbusCore.PublicFunctionCodes functionCode, int addr, int length, int[] values) => Write(new SimpleModbusCore.MBAP(new SimpleModbusCore.ADU_FunctionRequest(functionCode, addr, length, values)));
        private SimpleModbusCore.MBAP PreWrite(SimpleModbusCore.PublicFunctionCodes functionCode, int addr, bool value) => Write(new SimpleModbusCore.MBAP(new SimpleModbusCore.ADU_FunctionRequest(functionCode, addr, value)));
        private SimpleModbusCore.MBAP Write(SimpleModbusCore.MBAP mbap)
        {
            Message?.Invoke($"W: {mbap.MessageHEXString}");

            byte[] b = Socket.WriteRead(mbap.Message);

            mbap = new SimpleModbusCore.MBAP(new SimpleModbusCore.ADU_FunctionResponse(), b);
            Message?.Invoke($"R: {mbap.MessageHEXString}");

            return mbap;
        }

        public bool GetBool(int addr)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadDiscreteInput, addr, 1).PDU).Bool;
            }

            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
        public int GetInt16(int addr, int qauntity = 1)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadInputRegister, addr, qauntity).PDU).Int16;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0;
            }
        }
        public int GetInt16Hr(int addr, int quantity = 1)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadHoldingRegisters, addr, quantity).PDU).Int16;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0;
            }
        }

        public int GetInt32(int addr)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadInputRegister, addr, 2).PDU).Int32;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0;
            }
        }
        public float GetFloat(int addr)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadInputRegister, addr, 2).PDU).Float;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0.0f;
            }
        }
        public string GetString(int addr)
        {
            return null;
        }

        public bool SetBool(int addr, bool value)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteSingleCoil, addr, value).PDU).IsExceptionFunctionCode;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
        public bool SetInt16(int addr, int value)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteMultipleRegisters, addr, 1, new int[] { value}).PDU).IsExceptionFunctionCode;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }


        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                Socket?.Disconnect();

                if (disposing)
                {
                    Socket?.Dispose();
                }

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        ~SimpleModbusTCP()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
