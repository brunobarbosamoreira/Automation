using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Automation.Tags;
using Modbus.Device;

namespace Automation.PLC
{
    public interface IPLC : INotifyPropertyChanged
    {
        void Subscribe(ITagCoil tag);
        void Subscribe(ITagRegister tag);
        void Unsubscribe(ITagCoil tag);
        void Unsubscribe(ITagRegister tag);
        void Write(UInt16 Address, Int16 value);
        void Write(UInt16 Address, Int32 value);
        void Write(UInt16 Address, Int64 value);
        void Write(UInt16 Address, Single value);
        void Write(UInt16 Address, Double value);
        void Write(UInt16 Address, Boolean value);
        TimeSpan PollRate { get; }
        Boolean AllowGaps { get; }
        Boolean Connected { get; }
        Exception Exception { get; }
    }

    public class PLC : IPLC
    {
        #region Fields
        object WorkItemListLock = new object();
        TimeSpan _PollRate = TimeSpan.FromMilliseconds(250);
        System.Net.Sockets.TcpClient TcpClient;
        String IpAddress;
        Boolean _AllowGaps = true;
        Boolean _Connected;
        Exception _Exception;
        #endregion

        #region Constructor(s)
        public PLC(String IpAddress)
        {
            Initialize(IpAddress);
        }
        public PLC(String IpAddress, Boolean AllowGaps)
        {
            this.AllowGaps = AllowGaps;
            Initialize(IpAddress);
        }
        public PLC(String IpAddress, TimeSpan PollRate)
        {
            this.PollRate = PollRate;
            Initialize(IpAddress);
        }
        public PLC(String IpAddress, Boolean AllowGaps, TimeSpan PollRate)
        {
            this.AllowGaps = AllowGaps;
            this.PollRate = PollRate;
            Initialize(IpAddress);
        }
        #endregion

        #region Destructor
        ~PLC()
        {
            if (BW is BackgroundWorker)
            {
                if (BW.IsBusy && BW.WorkerSupportsCancellation)
                {
                    BW.CancelAsync();
                }
            }
        }
        #endregion

        #region Properties
        public Boolean AllowGaps
        {
            get { return _AllowGaps; }
            private set { _AllowGaps = value; }
        }
        public TimeSpan PollRate
        {
            get { return _PollRate; }
            private set { _PollRate = value; }
        }
        List<ITag> TagList { get; set; }
        ModbusIpMaster Driver { get; set; }
        BackgroundWorker BW { get; set; }
        List<IWorkItem> WorkItemList { get; set; }
        public Boolean Connected
        {
            get { return _Connected; }
            private set
            {
                if (_Connected != value)
                {
                    _Connected = value;
                    OnPropertyChanged("Connected");
                }
            }
        }
        public Exception Exception
        {
            get { return _Exception; }
            private set
            {
                if (_Exception != value)
                {
                    _Exception = value;
                    OnPropertyChanged("Exception");
                }
            }
        }
        #endregion

        #region Methods
        void Initialize(String IpAddress)
        {
            TagList = new List<ITag>();
            WorkItemList = new List<IWorkItem>();
            this.IpAddress = IpAddress;
            BW = new BackgroundWorker();
            BW.WorkerReportsProgress = true;
            BW.WorkerSupportsCancellation = true;
            BW.ProgressChanged += new ProgressChangedEventHandler(BW_ProgressChanged);
            BW.DoWork += new DoWorkEventHandler(BW_DoWork);
            BW.RunWorkerAsync();
        }
        void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker BW;
            if (sender is BackgroundWorker)
            {
                BW = sender as BackgroundWorker;
                while (!BW.CancellationPending)
                {
                    try
                    {
                        if (this.TcpClient == null || Driver == null || !(this.TcpClient.Connected))
                        {
                            this.TcpClient = new System.Net.Sockets.TcpClient()
                            {
                                ReceiveTimeout = 1000,
                                SendTimeout = 1000
                            };
                            this.TcpClient.Connect(IpAddress, 502);
                            Driver = ModbusIpMaster.CreateIp(TcpClient);
                            Driver.Transport.Retries = 3;
                        }
                        Stopwatch stopWatch = new Stopwatch();
                        lock (WorkItemListLock)
                        {
                            stopWatch.Start();
                            foreach (CoilsWorkItem workItem in WorkItemList.OfType<CoilsWorkItem>())
                            {
                                workItem.Coils = Driver.ReadCoils(1, workItem.Address, workItem.Length);
                                foreach (ITagCoil tag in workItem.TagList)
                                {
                                    BW.ReportProgress(0, new object[] { tag, workItem.Coils.Skip(tag.Address - workItem.Address).Take(tag.NoOfPoints).ToArray() });
                                }
                            }

                            foreach (HoldingRegistersWorkItem workItem in WorkItemList.OfType<HoldingRegistersWorkItem>())
                            {
                                workItem.HoldingRegisters = Driver.ReadHoldingRegisters(1, workItem.Address, workItem.Length);
                                foreach (ITagRegister tag in workItem.TagList)
                                {
                                    BW.ReportProgress(0, new object[] { tag, workItem.HoldingRegisters.Skip(tag.Address - workItem.Address).Take(tag.NoOfPoints).ToArray() });
                                }
                            }
                        }
                        stopWatch.Stop();
                        TimeSpan delay = PollRate.Subtract(stopWatch.Elapsed);
                        if (delay > TimeSpan.Zero)
                        {
                            Thread.Sleep(delay);
                        }

                    }
                    catch (System.Net.Sockets.SocketException exc)
                    {
                        BW.ReportProgress(0, exc);
                        Thread.Sleep(1000);
                        //BW.CancelAsync();
                    }
                    catch (InvalidOperationException exc)
                    {
                        BW.ReportProgress(0, exc);
                        Thread.Sleep(1000);
                    }
                    catch (Exception exc)
                    {
                        BW.ReportProgress(0, exc);
                        Thread.Sleep(1000);
                    }
                }
            }
        }
        void BW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (sender is BackgroundWorker)
            {
                if (e.UserState is Exception)
                {
                    this.Exception = e.UserState as Exception;
                    this.Connected = false;
                }
                else
                {
                    this.Connected = true;
                }
                if (e.UserState is object[])
                {
                    object[] item = e.UserState as object[];
                    if (item[0] is ITagCoil)
                    {
                        ((ITagCoil)item[0]).Update((Boolean[])item[1]);
                    }
                    else if (item[0] is ITagRegister)
                    {
                        ((ITagRegister)item[0]).Update((UInt16[])item[1]);
                    }
                }
            }
        }
        public void Subscribe(ITagCoil tag)
        {
            Subscribe(tag as ITag);
        }
        public void Subscribe(ITagRegister tag)
        {
            Subscribe(tag as ITag);
        }
        private void Subscribe(ITag tag)
        {
            TagList.Add(tag);
            tag.PLC = this;
            BuildWorkItemsList();
        }
        public void Unsubscribe(ITagCoil tag)
        {
            Unsubscribe(tag as ITag);
        }
        public void Unsubscribe(ITagRegister tag)
        {
            Unsubscribe(tag as ITag);
        }
        private void Unsubscribe(ITag tag)
        {
            TagList.Remove(tag);
            BuildWorkItemsList();
        }
        private void BuildWorkItemsList()
        {
            TagList.Sort(delegate(ITag a, ITag b)
            {
                if (b == null)
                    return 0;
                if (a == null)
                    return -1;
                return a.Address.CompareTo(b.Address);
            });

            lock (WorkItemListLock)
            {
                WorkItemList.Clear();

                //Add to WorkItemList CoilsWorkItem
                foreach (ITagCoil tag in TagList.OfType<ITagCoil>())
                {
                    CoilsWorkItem CurrentWorkItem = WorkItemList.OfType<CoilsWorkItem>().LastOrDefault();
                    if (CurrentWorkItem == null)
                    {
                        CurrentWorkItem = new CoilsWorkItem(AllowGaps);
                        WorkItemList.Add(CurrentWorkItem);
                    }
                    if (!CurrentWorkItem.Add(tag))
                    {
                        CurrentWorkItem = new CoilsWorkItem(AllowGaps);
                        WorkItemList.Add(CurrentWorkItem);
                        CurrentWorkItem.Add(tag);
                    }
                }

                //Add to WorkItemList HoldingRegistersWorkItem
                foreach (ITagRegister tag in TagList.OfType<ITagRegister>())
                {
                    HoldingRegistersWorkItem CurrentWorkItem = WorkItemList.OfType<HoldingRegistersWorkItem>().LastOrDefault();
                    if (CurrentWorkItem == null)
                    {
                        CurrentWorkItem = new HoldingRegistersWorkItem(AllowGaps);
                        WorkItemList.Add(CurrentWorkItem);
                    }
                    if (!CurrentWorkItem.Add(tag))
                    {
                        CurrentWorkItem = new HoldingRegistersWorkItem(AllowGaps);
                        WorkItemList.Add(CurrentWorkItem);
                        CurrentWorkItem.Add(tag);
                    }
                }
            }
        }
        public void Write(UInt16 Address, Boolean value)
        {
            Driver.WriteSingleCoil(1, Address, value);
        }
        private void Write(UInt16 Address, UInt16[] values)
        {
            Driver.WriteMultipleRegisters(1, Address, values);
        }
        public void Write(UInt16 Address, Int16 value)
        {
            byte[] SourceArray = BitConverter.GetBytes(value);
            UInt16[] values = new UInt16[sizeof(Int16) / sizeof(UInt16)];
            Buffer.BlockCopy(SourceArray, 0, values, 0, SourceArray.Length);
            Write(Address, values);
        }
        public void Write(UInt16 Address, Int32 value)
        {
            byte[] SourceArray = BitConverter.GetBytes(value);
            UInt16[] values = new UInt16[sizeof(Int32) / sizeof(UInt16)];
            Buffer.BlockCopy(SourceArray, 0, values, 0, SourceArray.Length);
            Write(Address, values);
        }
        public void Write(UInt16 Address, Int64 value)
        {
            byte[] SourceArray = BitConverter.GetBytes(value);
            UInt16[] values = new UInt16[sizeof(Int64) / sizeof(UInt16)];
            Buffer.BlockCopy(SourceArray, 0, values, 0, SourceArray.Length);
            Write(Address, values);
        }
        public void Write(UInt16 Address, Single value)
        {
            byte[] SourceArray = BitConverter.GetBytes(value);
            UInt16[] values = new UInt16[sizeof(Single) / sizeof(UInt16)];
            Buffer.BlockCopy(SourceArray, 0, values, 0, SourceArray.Length);
            Write(Address, values);
        }
        public void Write(UInt16 Address, Double value)
        {
            byte[] SourceArray = BitConverter.GetBytes(value);
            UInt16[] values = new UInt16[sizeof(Double) / sizeof(UInt16)];
            Buffer.BlockCopy(SourceArray, 0, values, 0, SourceArray.Length);
            Write(Address, values);
        }
        #endregion

        #region WorkItem
        interface IWorkItem
        {
            UInt16 Address { get; }
            UInt16 Length { get; }
            Boolean AllowGaps { get; }
        }
        class HoldingRegistersWorkItem : IWorkItem
        {
            static readonly UInt16 MaxLength = 125;
            public UInt16 Address { get; private set; }
            public UInt16 Length { get; private set; }
            public Boolean AllowGaps { get; private set; }
            public List<ITagRegister> TagList { get; private set; }
            public UInt16[] HoldingRegisters { get; set; }
            public HoldingRegistersWorkItem()
            {
                TagList = new List<ITagRegister>();
            }
            public HoldingRegistersWorkItem(Boolean AllowGaps) : this() { this.AllowGaps = AllowGaps; }
            public Boolean Add(ITagRegister tag)
            {
                Boolean ans = false;
                if (Length == 0)
                {
                    if (tag.NoOfPoints <= MaxLength)
                    {
                        Address = tag.Address;
                        Length = tag.NoOfPoints;
                        TagList.Add(tag);
                        ans = true;
                    }
                }
                else
                {
                    // If Tag fits in this work item buffer
                    if (Address <= tag.Address && tag.Address <= Address + MaxLength - 1 && Address <= tag.Address + tag.NoOfPoints && tag.Address + tag.NoOfPoints <= Address + MaxLength - 1)
                    {
                        if (AllowGaps || (Address <= tag.Address && tag.Address <= Address + Length))
                        {
                            UInt16 NewLength = (UInt16)(tag.Address + tag.NoOfPoints - Address);
                            Length = NewLength > Length ? NewLength : Length;
                            TagList.Add(tag);
                            ans = true;
                        }
                    }
                }
                return ans;
            }
        }
        class CoilsWorkItem : IWorkItem
        {
            static readonly UInt16 MaxLength = 2000;
            public UInt16 Address { get; private set; }
            public UInt16 Length { get; private set; }
            public Boolean AllowGaps { get; private set; }
            public List<ITagCoil> TagList { get; private set; }
            public Boolean[] Coils { get; set; }
            public CoilsWorkItem()
            {
                TagList = new List<ITagCoil>();
            }
            public CoilsWorkItem(Boolean AllowGaps) : this() { this.AllowGaps = AllowGaps; }
            public Boolean Add(ITagCoil tag)
            {
                Boolean ans = false;
                if (Length == 0)
                {
                    if (tag.NoOfPoints <= MaxLength)
                    {
                        Address = tag.Address;
                        Length = tag.NoOfPoints;
                        TagList.Add(tag);
                        ans = true;
                    }
                }
                else
                {
                    // If Tag fits in this work item buffer
                    if (Address <= tag.Address && tag.Address <= Address + MaxLength - 1 && Address <= tag.Address + tag.NoOfPoints && tag.Address + tag.NoOfPoints <= Address + MaxLength - 1)
                    {
                        if (AllowGaps || (Address <= tag.Address && tag.Address <= Address + Length))
                        {
                            UInt16 NewLength = (UInt16)(tag.Address + tag.NoOfPoints - Address);
                            Length = NewLength > Length ? NewLength : Length;
                            TagList.Add(tag);
                            ans = true;
                        }
                    }
                }
                return ans;
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
