using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleModbus
{
    public class SimpleModbusCore
    {
        public enum PublicFunctionCodes
        {
            ReadCoils = 1,
            ReadDiscreteInput = 2,
            ReadHoldingRegisters = 3,
            ReadInputRegister = 4,
            WriteSingleCoil = 5,
            WriteSingleRegister = 6,
            ReadExceptionStatus = 7,
            Diagnostic_CheckSubCode = 8,
            GetComEventCounter = 11,
            GetComEventLog = 12,
            WriteMultipleCoils = 15,
            WriteMultipleRegisters = 16,
            ReportServerID = 17,
            ReadFileRecord = 20,
            WriteFileRecord = 21,
            MaskWriteRegister = 22,
            ReadWriteMultipleRegisters = 23,
            ReadFIFOQueue = 24,
            CheckSubCode = 43,
        }
        public enum PublicFunctionSubCodes
        {
            Diag1 = 0,
            Diag2 = 18,
            Diag3 = 20,
            SubCode1 = 13,
            SubCode2 = 14
        }
        public class MBAP
        {
            //Public Read Only
            public int TransactionIdentifier => (Data[0] << 8) | Data[1];
            public int ProtocolIdentifier => (Data[2] << 8) | Data[3];
            public int Length => (Data[4] << 8) | Data[5];
            public byte UnitIdentifier => Data[6];
            public PDU PDU { get; private set; }
            public string HEXString => BitConverter.ToString(Data.ToArray()).Replace("-", " ");
            public string MessageHEXString
            {
                get
                {
                    List<byte> b = new List<byte>();
                    b.AddRange(Data);
                    b.AddRange(PDU.Data);

                    return BitConverter.ToString(b.ToArray()).Replace("-", " ");
                }
            }
            public byte[] Message
            {
                get
                {
                    List<byte> b = new List<byte>();
                    b.AddRange(Data);
                    b.AddRange(PDU.Data);
                    return b.ToArray();
                }
            }

            //Public
            public MBAP(PDU pdu, byte unitIdentifier = 1, int transactionIdentifier = 1, int protocolIdentifier = 0)
            {
                PDU = pdu;

                Data.Add(HighByte(transactionIdentifier));
                Data.Add(LowByte(transactionIdentifier));

                Data.Add(HighByte(protocolIdentifier));
                Data.Add(LowByte(protocolIdentifier));

                Data.Add(HighByte(pdu.Data.Count + 1));
                Data.Add(LowByte(pdu.Data.Count + 1));

                Data.Add(unitIdentifier);
            }
            public MBAP(PDU pdu, byte[] data)
            {
                PDU = pdu;

                if (data.Length < 10)
                {
                    Data.AddRange(new byte[7]);

                    PDU.Data.AddRange(new byte[7]);
                    PDU.Data[0] = 255;
                    return;
                }

                List<byte> data_list = new List<byte>(data);
                Data.AddRange(data_list.GetRange(0, 7));
                PDU.Data.AddRange(data_list.GetRange(7, this.Length - 1));

                if (PDU.Data.Count < this.Length - 1)
                {

                }
            }

            //Protected
            protected List<byte> Data { get; set; } = new List<byte>();

            //Private
            private byte LowByte(int i) => (byte)(i & 0b_0000_0000_1111_1111);
            private byte HighByte(int i) => (byte)((i & 0b_1111_1111_0000_0000) >> 8);
        }

        public abstract class PDU
        {
            public enum PDUType
            {
                Request,
                Response
            }

            //Public
            public PDUType Type { get; private set; }
            public PublicFunctionCodes FunctionCode => (PublicFunctionCodes)Data[0];

            public List<byte> Data { get; private set; } = new List<byte>();
            public string HEXString => BitConverter.ToString(Data.ToArray()).Replace("-", " ");

            //Protected
            protected void Create(PDUType type, PublicFunctionCodes functionCode, int startAddress, object value)
            {
                if (value.GetType() == typeof(short[]))
                {
                    short[] val = (short[])value;

                    Type = type;

                    Data.Add((byte)functionCode);

                    Data.Add(HighByte(startAddress));
                    Data.Add(LowByte(startAddress));

                    Data.Add(HighByte(val.Length));
                    Data.Add(LowByte(val.Length));

                    Data.Add((byte)(val.Length * 2));

                    for (int i = 0; i < val.Length; i++)
                    {
                        Data.Add(HighByte(val[i]));
                        Data.Add(LowByte(val[i]));
                    }
                }
                else if (value.GetType() == typeof(int[]))
                {
                    int[] val = (int[])value;

                    Type = type;

                    Data.Add((byte)functionCode);

                    Data.Add(HighByte(startAddress));
                    Data.Add(LowByte(startAddress));

                    Data.Add(HighByte(val.Length * 2));
                    Data.Add(LowByte(val.Length * 2));

                    Data.Add((byte)(val.Length * 4));

                    for (int i = 0; i < val.Length; i++)
                    {
                        Data.Add(HighByteHighWord(val[i]));
                        Data.Add(LowByteHighWord(val[i]));
                        Data.Add(HighByte(val[i]));
                        Data.Add(LowByte(val[i]));
                    }
                }
                else if (value.GetType() == typeof(float[]))
                {
                    float[] val = (float[])value;

                    Type = type;

                    Data.Add((byte)functionCode);

                    Data.Add(HighByte(startAddress));
                    Data.Add(LowByte(startAddress));

                    Data.Add(HighByte(val.Length * 2));
                    Data.Add(LowByte(val.Length * 2));

                    Data.Add((byte)(val.Length * 4));

                    for (int i = 0; i < val.Length; i++)
                    {
                        byte[] b = BitConverter.GetBytes(val[i]);
                        b = Reverse(b);
                        Data.AddRange(b);
                    }
                }
                else if (value.GetType() == typeof(int))
                {
                    Type = type;

                    Data.Add((byte)functionCode);

                    Data.Add(HighByte(startAddress));
                    Data.Add(LowByte(startAddress));

                    Data.Add(HighByte((int)value));
                    Data.Add(LowByte((int)value));
                }
                else if (value.GetType() == typeof(bool))
                {
                    Type = type;

                    Data.Add((byte)functionCode);

                    Data.Add(HighByte(startAddress));
                    Data.Add(LowByte(startAddress));

                    if ((bool)value)
                        Data.Add(0xFF);
                    else
                        Data.Add(0xFF);
                    Data.Add(0);
                }
            }
            protected void Create(PDUType type) => Type = type;

            //Private
            private byte LowByte(int i) => (byte)(i & 0b_1111_1111);
            private byte HighByte(int i) => (byte)((i & 0b_1111_1111_0000_0000) >> 8);
            private byte LowByteHighWord(int i) => (byte)((i & 0b_1111_1111_0000_0000__0000_0000) >> 16);
            private byte HighByteHighWord(int i) => (byte)((i & 0b_1111_1111_0000_0000_0000_0000__0000_0000) >> 24);
            private byte[] Reverse(byte[] arr)
            {
                int i = 0;
                int j = arr.Length - 1;
                while (i < j)
                {
                    var temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                    i++;
                    j--;
                }
                return arr;
            }

            //private int SwapBytes(int i) => (i >> 8) | (i << 8);
            //private byte LowNibble(byte b) => (byte)(b & 0b_0000_1111);
            //private byte HighNibble(byte b) => (byte)((b & 0b_1111_0000) >> 4);
            //public bool IsUserFunctionCode => (Data[0] >= 65 & Data[0] <= 72) | (Data[0] >= 100 & Data[0] <= 110);
        }

        public class ADU_FunctionRequest : PDU
        {
            //Public Read Only
            public int Address => (Data[1] << 8) | Data[2];
            public int Quantity => (Data[3] << 8) | Data[4];

            //Public
            public ADU_FunctionRequest(PublicFunctionCodes functionCode, int address, object value) => Create(PDU.PDUType.Request, functionCode, address, value);
        }

        public class ADU_FunctionResponse : PDU
        {
            public bool IsExceptionFunctionCode => (Data[0] >> 7) == 1;
            public int Address => (Data[1] << 8) | Data[2];
            public int Value => (Data[3] << 8) | Data[4];

            public int ExceptionCode
            {
                get
                {
                    return Data[1] + 0x80;
                }
            }
            public byte ByteCount => Data[1];
            public int Status => (Data[3] << 8) | Data[4];

            public bool Bool => Convert.ToBoolean(Data[2]);
            public int Int16 => (Data[2] << 8) | Data[3];
            public int Int32 => System.BitConverter.ToInt32(Reverse(Data.GetRange(2, 4).ToArray()), 0); //((Data[2] << 8) | Data[3]) << 16 | ((Data[4] << 8) | Data[5]);
            public float Float => System.BitConverter.ToSingle(Reverse(Data.GetRange(2, 4).ToArray()), 0);

            private byte[] Reverse(byte[] arr)
            {
                int i = 0;
                int j = arr.Length - 1;
                while (i < j)
                {
                    var temp = arr[i];
                    arr[i] = arr[j];
                    arr[j] = temp;
                    i++;
                    j--;
                }
                return arr;
            }

            public List<object> Values
            {
                get
                {
                    List<byte> data = Data.GetRange(2, Data.Count - 2);
                    List<object> ret = new List<object>();
                    for (int i = 0; i < data.Count; i += 2)
                        ret.Add((data[i] << 8) | data[i + 1]);
                    return ret;
                }
            }
            public ADU_FunctionResponse() => Create(PDU.PDUType.Response);
        }
    }
}
