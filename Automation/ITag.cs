using System;
using System.ComponentModel;

namespace Automation
{
    public interface ITag : INotifyPropertyChanged
    {
        String Name { get; set; }
        Object Value { get; }
        Object PLC { get; }
        Object Address { get; }
    }

    public interface ITag<T> : ITag
        where T : struct
    {
        new T Value { get; set; }
    }

    public interface ITagBoolean : ITag<Boolean> { }
    public interface ITagInt16 : ITag<Int16> { }
    public interface ITagUInt16 : ITag<UInt16> { }
    public interface ITagInt32 : ITag<Int32> { }
    public interface ITagUInt32 : ITag<UInt32> { }
    public interface ITagInt64 : ITag<Int64> { }
    public interface ITagUInt64 : ITag<UInt64> { }
    public interface ITagSingle : ITag<Single> { }
    public interface ITagDouble : ITag<Double> { }
}
