extern alias NModbus;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Automation.Modbus.Tag;
using Automation.Modbus.WorkItem;

namespace Automation.Modbus
{
    public class ModbusTcpMaster : NotifyPropertyChanged, IModbusIPMaster
    {
        #region Fields
        private String _Name;
        private Boolean _Connected;
        private IPAddress _IP;
        private NModbus::Modbus.Device.ModbusIpMaster _Driver;
        private Object _DriverLock = new Object();
        private BackgroundWorker _Worker;
        private List<IModbusTag> _Tags = new List<IModbusTag>();
        private IList<IWorkItem> _WorkItems = new List<IWorkItem>();
        private TimeSpan _PollingPeriod = TimeSpan.FromMilliseconds(250);
        private UInt16 _MaxQuantityOfCoils = 2000;
        private UInt16 _MaxQuantityOfRegisters = 125;
        #endregion

        #region Constructor(s)
        public ModbusTcpMaster() { }
        /// <summary>
        /// ModbusTcpMaster driver
        /// </summary>
        /// <param name="MaxQuantityOfCoils">Maximum quantity of coils in a single request. This value can´t be above 2000</param>
        /// <param name="MaxQuantityOfRegisters">Maximum quantity of registers in a single request. This value can´t be above 125</param>
        public ModbusTcpMaster(UInt16 MaxQuantityOfCoils, UInt16 MaxQuantityOfRegisters)
        {
            this.MaxQuantityOfCoils = MaxQuantityOfCoils;
            this.MaxQuantityOfRegisters = MaxQuantityOfRegisters;
        }
        #endregion

        #region Public Properties
        public IPAddress IP
        {
            get { return _IP; }
            private set
            {
                if (_IP != value)
                {
                    _IP = value;
                    OnPropertyChanged("IP");
                }
            }
        }
        public Int32 Port
        {
            get { return 502; }
        }
        public String Name
        {
            get { return _Name; }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        public Boolean Connected
        {
            get { return _Connected; }
            protected set
            {
                if (_Connected != value)
                {
                    _Connected = value;
                    OnPropertyChanged("Connected");
                }
            }
        }
        /// <summary>
        /// Period of data samples
        /// </summary>
        public TimeSpan PollingPeriod
        {
            get { return _PollingPeriod; }
            set
            {
                if (_PollingPeriod != value)
                {
                    _PollingPeriod = value;
                    OnPropertyChanged("PollingPeriod");
                }
            }
        }
        #endregion

        #region Private/Protected Properties
        private BackgroundWorker Worker
        {
            get
            {
                if (_Worker == null)
                {
                    _Worker = new BackgroundWorker()
                    {
                        WorkerSupportsCancellation = true,
                        WorkerReportsProgress = true
                    };
                    _Worker.ProgressChanged += new ProgressChangedEventHandler(BW_ProgressChanged);
                    _Worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(BW_RunWorkerCompleted);
                    _Worker.DoWork += new DoWorkEventHandler(BW_DoWork);
                }
                return _Worker;
            }
        }
        private Object DriverLock { get { return _DriverLock; } }
        private IList<IWorkItem> WorkItems
        {
            get
            {
                return _WorkItems;
            }
            set
            {
                _WorkItems = value;
            }
        }
        private List<IModbusTag> Tags
        {
            get { return _Tags; }
        }
        private NModbus::Modbus.Device.ModbusIpMaster Driver
        {
            get { return _Driver; }
            set
            {
                if (_Driver != value)
                {
                    _Driver = value;
                }
            }
        }
        private UInt16 MaxQuantityOfCoils
        {
            get { return _MaxQuantityOfCoils; }
            set
            {
                if (_MaxQuantityOfCoils != value)
                {
                    if (_MaxQuantityOfCoils <= 2000)
                    {
                        _MaxQuantityOfCoils = value;
                    }
                }
            }
        }
        private UInt16 MaxQuantityOfRegisters
        {
            get { return _MaxQuantityOfRegisters; }
            set
            {
                if (_MaxQuantityOfRegisters != value)
                {
                    if (_MaxQuantityOfRegisters <= 125)
                    {
                        _MaxQuantityOfRegisters = value;
                    }
                }
            }
        }
        #endregion

        #region Private/Protected Methods
        private IList<IWorkItem> GetWorkItems(IEnumerable<IModbusTag> Tags)
        {
            IList<IModbusTag> TagList = Tags.ToList();

            IEnumerable<CoilsWorkItem> Coils = CoilsWorkItem.GetWorkItems(TagList.OfType<IModbusCoilsTag>().ToList(), MaxQuantityOfCoils);
            IEnumerable<HoldingRegistersWorkItem> HoldingRegisters = HoldingRegistersWorkItem.GetWorkItems(TagList.OfType<IModbusHoldingRegistersTag>().ToList(), MaxQuantityOfRegisters);

            List<IWorkItem> WorkItems = new List<IWorkItem>();
            WorkItems.AddRange(Coils.Cast<IWorkItem>());
            WorkItems.AddRange(HoldingRegisters.Cast<IWorkItem>());

            return WorkItems;
        }
        private void Connect()
        {
            lock (DriverLock)
            {
                if (Driver is NModbus.Modbus.Device.ModbusIpMaster)
                {
                    Driver.Dispose();
                }
                TcpClient Client = new TcpClient
                {
                    ReceiveTimeout = 1000,
                    SendTimeout = 1000
                };

                if (Worker.IsBusy)
                    return;
                Client.Connect(IP, Port);
                Driver = NModbus.Modbus.Device.ModbusIpMaster.CreateIp(Client);
                Worker.RunWorkerAsync();
                this.Connected = true;
            }

        }
        private void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker Worker = sender as BackgroundWorker;
            if (Worker is BackgroundWorker)
            {
                try
                {
                    using (Driver)
                    {
                        Stopwatch stopWatch = new Stopwatch();
                        do
                        {
                            // Start timer
                            stopWatch.Start();

                            // Process Work Items
                            IList<IWorkItem> WorkItems = this.WorkItems.ToList();
                            foreach (var workItem in WorkItems)
                            {
                                this.Read(workItem);
                                Worker.ReportProgress(0, workItem);
                            }

                            // Stop timer
                            stopWatch.Stop();

                            // Get the time that the thread has to sleep
                            // in order to make requests periodically acording to
                            // PollingPeriod
                            TimeSpan delay = PollingPeriod.Subtract(stopWatch.Elapsed);
                            if (delay > TimeSpan.Zero)
                            {
                                Thread.Sleep(delay);
                            }

                            // Reset timer
                            stopWatch.Reset();
                        } while (!(Worker.CancellationPending));
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
        private void BW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // If background worker is reporting progress => We are connected
            this.Connected = true;

            if (e.UserState is IWorkItem)
            {
                IWorkItem workItem = e.UserState as IWorkItem;
                workItem.UpdateTags();
            }
        }
        private void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // If background worker completed it's work => We are disconnected
            this.Connected = false;

            if (e.Error is Exception)
            {
                // To do
            }
        }
        private void Read(IWorkItem workItem)
        {
            if (workItem is HoldingRegistersWorkItem)
                Read((HoldingRegistersWorkItem)workItem);
            else if (workItem is CoilsWorkItem)
                Read((CoilsWorkItem)workItem);
            //else if (workItem is blablaWorkItem)
            //    Read(workItem as blablaWorkItem);
            else
                throw new NotSupportedException();
        }
        private void Read(HoldingRegistersWorkItem workItem)
        {
            UInt16 address = workItem.Address.Index;
            UInt16 length = workItem.QuantityOfRegisters;
            UInt16[] reply;

            lock (DriverLock)
            {
                reply = Driver.ReadHoldingRegisters(1, address, length);
            }

            Byte[] buffer = new Byte[reply.Length * sizeof(UInt16)];
            Buffer.BlockCopy(reply, 0, buffer, 0, buffer.Length);
            workItem.Buffer = buffer;
        }
        private void Read(CoilsWorkItem workItem)
        {
            UInt16 address = workItem.Address.Index;
            UInt16 length = workItem.QuantityOfCoils;
            Boolean[] reply;

            lock (DriverLock)
            {
                reply = Driver.ReadCoils(1, address, length);
            }

            Byte[] buffer = new Byte[reply.Length * sizeof(Boolean)];
            Buffer.BlockCopy(reply, 0, buffer, 0, buffer.Length);
            workItem.Buffer = buffer;
        }
        private void WriteHoldingRegisters(UInt16 Index, Byte[] Data)
        {
            int Words_Length = Data.Length / sizeof(UInt16) + Data.Length % sizeof(UInt16);
            UInt16[] Words = new UInt16[Words_Length];
            Buffer.BlockCopy(Data, 0, Words, 0, Data.Length);

            lock (DriverLock)
            {
                Driver.WriteMultipleRegisters(1, Index, Words);
            }
        }
        private void WriteCoils(UInt16 Index, Byte[] Data)
        {
            int Coils_Length = Data.Length / sizeof(Boolean) + Data.Length % sizeof(Boolean);
            Boolean[] Coils = new Boolean[Coils_Length];
            Buffer.BlockCopy(Data, 0, Coils, 0, Data.Length);

            lock (DriverLock)
            {
                Driver.WriteMultipleCoils(1, Index, Coils);
            }
        }
        private void Add(IModbusTag Tag)
        {
            this.Tags.Add(Tag);
            this.WorkItems = GetWorkItems(Tags);
        }
        #endregion

        #region Public Methods
        public void Connect(IPAddress IP)
        {
            this.IP = IP;
            this.Connect();
        }
        public void Disconnect()
        {
            if (Worker is BackgroundWorker)
            {
                if (Worker.IsBusy)
                {
                    if (Worker.WorkerSupportsCancellation)
                    {
                        Worker.CancelAsync();
                    }
                }
            }
        }
        public void Add(TagBoolean Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.Coils, Index = Index };
            Add(Tag);
        }
        public void Add(TagInt16 Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.HoldingRegisters, Index = Index };
            Add(Tag);
        }
        public void Add(TagUInt16 Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.HoldingRegisters, Index = Index };
            Add(Tag);
        }
        public void Add(TagInt32 Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.HoldingRegisters, Index = Index };
            Add(Tag);
        }
        public void Add(TagUInt32 Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.HoldingRegisters, Index = Index };
            Add(Tag);
        }
        public void Add(TagInt64 Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.HoldingRegisters, Index = Index };
            Add(Tag);
        }
        public void Add(TagUInt64 Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.HoldingRegisters, Index = Index };
            Add(Tag);
        }
        public void Add(TagSingle Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.HoldingRegisters, Index = Index };
            Add(Tag);
        }
        public void Add(TagDouble Tag, UInt16 Index)
        {
            Tag.PLC = this;
            Tag.Address = new Address { Table = Table.HoldingRegisters, Index = Index };
            Add(Tag);
        }

        public void Write(TagBoolean Tag, Boolean Value)
        {
            if (Tag.Address.Table == Table.Coils)
            {
                WriteCoils(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        public void Write(TagInt16 Tag, Int16 Value)
        {
            if (Tag.Address.Table == Table.HoldingRegisters)
            {
                WriteHoldingRegisters(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        public void Write(TagUInt16 Tag, UInt16 Value)
        {
            if (Tag.Address.Table == Table.HoldingRegisters)
            {
                WriteHoldingRegisters(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        public void Write(TagInt32 Tag, Int32 Value)
        {
            if (Tag.Address.Table == Table.HoldingRegisters)
            {
                WriteHoldingRegisters(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        public void Write(TagUInt32 Tag, UInt32 Value)
        {
            if (Tag.Address.Table == Table.HoldingRegisters)
            {
                WriteHoldingRegisters(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        public void Write(TagInt64 Tag, Int64 Value)
        {
            if (Tag.Address.Table == Table.HoldingRegisters)
            {
                WriteHoldingRegisters(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        public void Write(TagUInt64 Tag, UInt64 Value)
        {
            if (Tag.Address.Table == Table.HoldingRegisters)
            {
                WriteHoldingRegisters(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        public void Write(TagSingle Tag, Single Value)
        {
            if (Tag.Address.Table == Table.HoldingRegisters)
            {
                WriteHoldingRegisters(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        public void Write(TagDouble Tag, Double Value)
        {
            if (Tag.Address.Table == Table.HoldingRegisters)
            {
                WriteHoldingRegisters(Tag.Address.Index, BitConverter.GetBytes(Value));
            }
        }
        #endregion

        #region IPLC
        ReadOnlyCollection<ITag> IPLC.Tags
        {
            get { return Tags.Cast<ITag>().ToList().AsReadOnly(); }
        }
        #endregion
    }
}
