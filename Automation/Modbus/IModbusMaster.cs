using System;
using System.ComponentModel;
using System.Net;
using Automation.Modbus.Tag;
using System.Collections.ObjectModel;

namespace Automation.Modbus
{
    public interface IModbusMaster : IPLC
    {
        void Write(TagBoolean Tag, Boolean Value);
        void Write(TagInt16 tagInt16, Int16 Value);
        void Write(TagUInt16 tagUInt16, UInt16 Value);
        void Write(TagInt32 tagInt32, Int32 Value);
        void Write(TagUInt32 tagUInt32, UInt32 Value);
        void Write(TagInt64 tagInt64, Int64 Value);
        void Write(TagUInt64 tagUInt64, UInt64 Value);
        void Write(TagSingle tagSingle, Single Value);
        void Write(TagDouble tagDouble, Double Value);
    }

    interface IModbusIPMaster : IModbusMaster, IIP_PLC
    {
    }

    interface IModbusSerialMaster : IModbusMaster, ISerial_PLC
    {
    }
}
