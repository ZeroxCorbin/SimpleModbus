using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleModbus
{
    public class SimpleModbusTCP
    {
        public delegate void ErrorEventHandler(object sender, Exception data);
        public event ErrorEventHandler Error;

        public delegate void MessageEventHandler(string message);
        public event MessageEventHandler Message;

        public AsyncSocket.ASocket Socket { get; private set; }
        public SimpleModbusTCP(AsyncSocket.ASocket socket) => Socket = socket;

        private object SocketLock { get; } = new object();

        private SimpleModbusCore.MBAP PreWrite(SimpleModbusCore.PublicFunctionCodes functionCode, int addr, object value) => Write(new SimpleModbusCore.MBAP(new SimpleModbusCore.ADU_FunctionRequest(functionCode, addr, value)));
        private SimpleModbusCore.MBAP Write(SimpleModbusCore.MBAP mbap)
        { 
            Message?.Invoke($"W: {mbap.MessageHEXString}");

            lock (SocketLock)
            {
                Socket.Send(mbap.Message);
                byte[] b = Socket.ReceiveBytes(1000);

                if (b == null)
                    return null;

                mbap = new SimpleModbusCore.MBAP(new SimpleModbusCore.ADU_FunctionResponse(), b);
            } 
            
            Message?.Invoke($"R: {mbap.MessageHEXString}");
            return mbap;
        }

        //public T Get<T>(SimpleModbusCore.PublicFunctionCodes code, int address, int quantity)
        //{
        //    if (typeof(T) == typeof(short))
        //        return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Int16, typeof(T));
        //    if (typeof(T) == typeof(int))
        //        return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Int32, typeof(T));
        //    if (typeof(T) == typeof(bool))
        //        return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Bool, typeof(T));
        //    if (typeof(T) == typeof(float))
        //        return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Float, typeof(T));

        //    return default;
        //}

        //public bool Set<T>(int address, T value)
        //{
        //    if (typeof(T) == typeof(short))
        //        return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteSingleRegister, address, value).PDU).IsExceptionFunctionCode;
        //    //if (typeof(T) == typeof(Int32))
        //    //    return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteMultipleRegisters, address, quantity).PDU).Int32, typeof(T));
        //    if (typeof(T) == typeof(bool))
        //        return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteSingleCoil, address, value).PDU).IsExceptionFunctionCode;
        //    //if (typeof(T) == typeof(float))
        //    //    return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Float, typeof(T));

        //    return default;
        //}


        //public T Set<T>(SimpleModbusCore.PublicFunctionCodes code, int address, int quantity)
        //{
        //    if (typeof(T) == typeof(Int16))
        //        return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Int16, typeof(T));
        //    if (typeof(T) == typeof(Int32))
        //        return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Int32, typeof(T));
        //    if (typeof(T) == typeof(bool))
        //        return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Bool, typeof(T));
        //    if (typeof(T) == typeof(float))
        //        return (T)Convert.ChangeType(((SimpleModbusCore.ADU_FunctionResponse)PreWrite(code, address, quantity).PDU).Float, typeof(T));

        //    return default;
        //}
        public bool ReadDiscreteInput(int addr)
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
        public bool ReadCoils(int addr)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadCoils, addr, 1).PDU).Bool;
            }

            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
        public int GetInt16(int addr, int quantityOfInt16 = 1)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadInputRegister, addr, quantityOfInt16).PDU).Int16;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0;
            }
        }
        public int GetInt16Hr(int addr, int quantityOfInt16 = 1)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadHoldingRegisters, addr, quantityOfInt16).PDU).Int16;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0;
            }
        }
        public int GetInt32(int addr, int quantityOfInt32 = 1)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadInputRegister, addr, quantityOfInt32 * 2).PDU).Int32;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0;
            }
        }
        public int GetInt32Hr(int addr, int quantityOfInt32 = 1)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadHoldingRegisters, addr, quantityOfInt32 * 2).PDU).Int32;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0;
            }
        }
        public float GetFloat(int addr, int quantityOfFloat = 1)
        {
            try
            {

                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadInputRegister, addr, quantityOfFloat * 2).PDU).Float;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0.0f;
            }
        }
        public float GetFloatHr(int addr, int quantityOfFloat = 1)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadHoldingRegisters, addr, quantityOfFloat * 2).PDU).Float;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return 0.0f;
            }
        }
        public string GetString(int addr, int quantityOfChars)
        {
            try
            {
                return ((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.ReadInputRegister, addr, quantityOfChars).PDU).String;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return "";
            }
        }

        public bool WriteSingleCoil(int addr, bool value)
        {
            try
            {
                return !((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteSingleCoil, addr, value).PDU).IsExceptionFunctionCode;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
        public bool SetByte(int addr, byte[] values)
        {
            try
            {
                return !((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteMultipleRegisters, addr, values).PDU).IsExceptionFunctionCode;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
        public bool SetInt16(int addr, short[] values)
        {
            try
            {
                return !((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteMultipleRegisters, addr, values).PDU).IsExceptionFunctionCode;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
        public bool SetInt32(int addr, int[] values)
        {
            try
            {
                return !((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteMultipleRegisters, addr, values).PDU).IsExceptionFunctionCode;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
        public bool SetFloat(int addr, float[] values)
        {
            try
            {
                return !((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteMultipleRegisters, addr, values).PDU).IsExceptionFunctionCode;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
        public bool SetString(int addr, string value)
        {
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);

                return !((SimpleModbusCore.ADU_FunctionResponse)PreWrite(SimpleModbusCore.PublicFunctionCodes.WriteMultipleRegisters, addr, bytes).PDU).IsExceptionFunctionCode;
            }
            catch (Exception ex)
            {
                Error?.Invoke(this, ex);
                return false;
            }
        }
    }
}
