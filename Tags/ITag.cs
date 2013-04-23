using System;
using System.ComponentModel;
using Automation.PLC;

namespace Automation.Tags
{
    public interface ITag : INotifyPropertyChanged
    {
        UInt16 Address { get; set; }
        UInt16 NoOfPoints { get; }
        String Name { get; set; }
        IPLC PLC { get; set; }
    }

    public abstract class TagBase : ITag
    {
        #region Constructor(s)
        public TagBase(UInt16 Address)
        {
            this.Address = Address;
        }
        #endregion

        #region Properties
        public IPLC PLC { get; set; }
        public String Name { get; set; }
        public UInt16 Address { get; set; }
        public abstract UInt16 NoOfPoints { get; }
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

    public interface ITagCoil : ITag
    {
        Boolean Value { get; set; }
        void Update(Boolean[] value);
    }

    public interface ITagRegister : ITag
    {
        void Update(UInt16[] value);
    }

    public class TagBoolean : TagBase, ITagCoil
    {
        #region Fields
        private Boolean _Value;
        #endregion

        #region Constructor(s)
        public TagBoolean(UInt16 Address) : base(Address) { }
        public TagBoolean(String Name, UInt16 Address)
            : this(Address)
        {
            this.Name = Name;
        }
        #endregion

        #region Properties
        public Boolean Value
        { get { return this._Value; } set { PLC.Write(this.Address, value); } }
        public override UInt16 NoOfPoints
        {
            get { return 1; }
        }
        #endregion

        #region Methods
        public void Update(bool[] value)
        {
            if (this._Value != value[0])
            {
                this._Value = value[0];
                OnPropertyChanged("Value");
            }
        }
        #endregion
    }

    public class TagInt16 : TagBase, ITagRegister
    {
        #region Fields
        private Int16 _Value;
        #endregion

        #region Constructor(s)
        public TagInt16(UInt16 Address) : base(Address) { }
        public TagInt16(String Name, UInt16 Address)
            : this(Address)
        {
            this.Name = Name;
        }
        #endregion

        #region Properties
        public Int16 Value
        {
            get { return _Value; }
            set { PLC.Write(this.Address, value); }
        }
        public override UInt16 NoOfPoints
        {
            get { return sizeof(Int16) / 2; }
        }
        #endregion

        #region Methods
        public void Update(UInt16[] value)
        {
            Int16[] newValue = new Int16[1];
            Buffer.BlockCopy(value, 0, newValue, 0, sizeof(Int16));
            if (_Value != newValue[0])
            {
                _Value = newValue[0];
                OnPropertyChanged("Value");
            }
        }
        #endregion
    }

    public class TagInt32 : TagBase, ITagRegister
    {
        #region Fields
        private Int32 _Value;
        #endregion

        #region Constructor(s)
        public TagInt32(UInt16 Address) : base(Address) { }
        public TagInt32(String Name, UInt16 Address)
            : this(Address)
        {
            this.Name = Name;
        }
        #endregion

        #region Properties
        public Int32 Value
        {
            get { return _Value; }
            set
            {
                PLC.Write(this.Address, value);
            }
        }
        public override UInt16 NoOfPoints
        {
            get { return sizeof(Int32) / 2; }
        }
        #endregion

        #region Methods
        public void Update(UInt16[] value)
        {
            Int32[] newValue = new Int32[1];
            Buffer.BlockCopy(value, 0, newValue, 0, sizeof(Int32));
            if (_Value != newValue[0])
            {
                _Value = newValue[0];
                OnPropertyChanged("Value");
            }
        }
        #endregion
    }

    public class TagInt64 : TagBase, ITagRegister
    {
        #region Fields
        private Int64 _Value;
        #endregion

        #region Constructor(s)
        public TagInt64(UInt16 Address) : base(Address) { }
        public TagInt64(String Name, UInt16 Address)
            : this(Address)
        {
            this.Name = Name;
        }
        #endregion

        #region Properties
        public Int64 Value
        { get { return _Value; } set { PLC.Write(this.Address, value); } }
        public override UInt16 NoOfPoints
        {
            get { return sizeof(Int64) / 2; }
        }
        #endregion

        #region Methods
        public void Update(UInt16[] value)
        {
            Int64[] newValue = new Int64[1];
            Buffer.BlockCopy(value, 0, newValue, 0, sizeof(Int64));
            if (_Value != newValue[0])
            {
                _Value = newValue[0];
                OnPropertyChanged("Value");
            }
        }
        #endregion
    }

    public class TagSingle : TagBase, ITagRegister
    {
        #region Fields
        private Single _Value;
        #endregion

        #region Constructor(s)
        public TagSingle(UInt16 Address) : base(Address) { }
        public TagSingle(String Name, UInt16 Address)
            : this(Address)
        {
            this.Name = Name;
        }
        #endregion

        #region Properties
        public Single Value
        { get { return _Value; } set { PLC.Write(this.Address, value); } }
        public override UInt16 NoOfPoints
        {
            get { return sizeof(Single) / 2; }
        }
        #endregion

        #region Methods
        public void Update(UInt16[] value)
        {
            Single[] newValue = new Single[1];
            Buffer.BlockCopy(value, 0, newValue, 0, sizeof(Single));
            if (_Value != newValue[0])
            {
                _Value = newValue[0];
                OnPropertyChanged("Value");
            }
        }
        #endregion
    }

    public class TagDouble : TagBase, ITagRegister
    {
        #region Fields
        private Double _Value;
        #endregion

        #region Constructor(s)
        public TagDouble(UInt16 Address) : base(Address) { }
        public TagDouble(String Name, UInt16 Address)
            : this(Address)
        {
            this.Name = Name;
        }
        #endregion

        #region Properties
        public Double Value
        { get { return _Value; } set { PLC.Write(this.Address, value); } }
        public override UInt16 NoOfPoints
        {
            get { return sizeof(Double) / 2; }
        }
        #endregion

        #region Methods
        public void Update(UInt16[] value)
        {
            Double[] newValue = new Double[1];
            Buffer.BlockCopy(value, 0, newValue, 0, sizeof(Double));
            if (_Value != newValue[0])
            {
                _Value = newValue[0];
                OnPropertyChanged("Value");
            }
        }
        #endregion
    }
}
