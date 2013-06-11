using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO.Ports;
using System.Net;

namespace Automation
{
    public interface IPLC : INotifyPropertyChanged
    {
        String Name { get; set; }
        Boolean Connected { get; }
        ReadOnlyCollection<ITag> Tags { get; }
        TimeSpan PollingPeriod { get; set; }
    }

    public interface IIP_PLC : IPLC
    {
        IPAddress IP { get; }
        void Connect(IPAddress IP);
        void Disconnect();
    }

    public interface ISerial_PLC : IPLC
    {
        SerialPort SerialPort { get; }
        void Connect(SerialPort Port);
        void Disconnect();
    }
}
