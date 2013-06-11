using System;

namespace Automation.Modbus.Tag
{
    public abstract class ModbusTag<T> : Tag<T>, IModbusTag<T>
        where T : struct
    {
        #region Fields
        private Address _Address;
        private IModbusMaster _PLC;
        #endregion

        #region Public/Internal Properties
        /// <summary>
        /// Modbus Address
        /// </summary>
        public Address Address
        {
            get { return _Address; }
            internal set
            {
                if (_Address != value)
                {
                    _Address = value;
                    OnPropertyChanged("Address");
                }
            }
        }

        /// <summary>
        /// Communication driver to PLC
        /// </summary>        
        public IModbusMaster PLC
        {
            get { return _PLC; }
            internal set
            {
                if (_PLC != value)
                {
                    _PLC = value;
                    OnPropertyChanged("PLC");
                }
            }
        }
        /// <summary>
        /// Size of Value in bytes
        /// </summary>
        public abstract Int32 Size { get; }
        #endregion          

        #region ITag
        Object ITag.Address
        {
            get { return Address; }
        }
        Object ITag.PLC
        {
            get { return this.PLC; }
        }
        Object ITag.Value
        {
            get
            { return this.Value; }
        }
        #endregion
    }

    public class TagBoolean : ModbusTag<Boolean>, ITagBoolean, IModbusCoilsTag
    {
        public override int Size
        {
            get { return sizeof(Boolean); }
        }

        public UInt16 QuantityOfCoils
        {
            get { return 1; }
        }

        protected override void WriteToPLC(Boolean Value)
        {
            PLC.Write(this, Value);
        }
    }
    public class TagInt16 : ModbusTag<Int16>, ITagInt16, IModbusHoldingRegistersTag
    {
        public override int Size
        {
            get { return sizeof(Int16); }
        }

        public UInt16 QuantityOfRegisters { get { return 1; } }

        protected override void WriteToPLC(short Value)
        {
            PLC.Write(this, Value);
        }
    }
    public class TagUInt16 : ModbusTag<UInt16>, ITagUInt16, IModbusHoldingRegistersTag
    {
        public override int Size
        {
            get { return sizeof(UInt16); }
        }

        public UInt16 QuantityOfRegisters { get { return 1; } }

        protected override void WriteToPLC(ushort Value)
        {
            PLC.Write(this, Value);
        }
    }
    public class TagInt32 : ModbusTag<Int32>, ITagInt32, IModbusHoldingRegistersTag
    {
        public override int Size
        {
            get { return sizeof(Int32); }
        }

        public UInt16 QuantityOfRegisters { get { return 2; } }

        protected override void WriteToPLC(int Value)
        {
            PLC.Write(this, Value);
        }
    }
    public class TagUInt32 : ModbusTag<UInt32>, ITagUInt32, IModbusHoldingRegistersTag
    {
        public override int Size
        {
            get { return sizeof(UInt32); }
        }

        public UInt16 QuantityOfRegisters { get { return 2; } }

        protected override void WriteToPLC(uint Value)
        {
            PLC.Write(this, Value);
        }
    }
    public class TagInt64 : ModbusTag<Int64>, ITagInt64, IModbusHoldingRegistersTag
    {
        public override int Size
        {
            get { return sizeof(Int64); }
        }

        public UInt16 QuantityOfRegisters { get { return 4; } }

        protected override void WriteToPLC(long Value)
        {
            PLC.Write(this, Value);
        }
    }
    public class TagUInt64 : ModbusTag<UInt64>, ITagUInt64, IModbusHoldingRegistersTag
    {
        public override int Size
        {
            get { return sizeof(UInt64); }
        }

        public UInt16 QuantityOfRegisters { get { return 4; } }

        protected override void WriteToPLC(ulong Value)
        {
            PLC.Write(this, Value);
        }
    }
    public class TagSingle : ModbusTag<Single>, ITagSingle, IModbusHoldingRegistersTag
    {
        public override int Size
        {
            get { return sizeof(Single); }
        }

        public UInt16 QuantityOfRegisters { get { return 2; } }

        protected override void WriteToPLC(float Value)
        {
            PLC.Write(this, Value);
        }
    }
    public class TagDouble : ModbusTag<Double>, ITagDouble, IModbusHoldingRegistersTag
    {
        public override int Size
        {
            get { return sizeof(Double); }
        }

        public UInt16 QuantityOfRegisters { get { return 4; } }

        protected override void WriteToPLC(double Value)
        {
            PLC.Write(this, Value);
        }
    }
}
