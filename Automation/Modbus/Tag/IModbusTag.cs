using System;

namespace Automation.Modbus.Tag
{
    public interface IModbusTag : ITag
    {
        new Address Address { get; }
        new IModbusMaster PLC { get; }
    }

    interface IModbusHoldingRegistersTag : IModbusTag
    {
        /// <summary>
        /// Quantity of Holding Registers
        /// </summary>
        UInt16 QuantityOfRegisters { get; }
    }

    interface IModbusCoilsTag : IModbusTag
    {
        /// <summary>
        /// Quantity of Coils
        /// </summary>
        UInt16 QuantityOfCoils { get; }
    }

    public interface IModbusTag<T> : ITag<T>, IModbusTag
          where T : struct
    {

    }
}
