using System;
using System.Collections.Generic;
using System.Linq;
using Automation.Modbus.Tag;

namespace Automation.Modbus.WorkItem
{
    class HoldingRegistersWorkItem : IWorkItem
    {
        #region Fields
        private Int32 _MaxQuantityOfRegisters = 125;
        private List<IModbusTag> _Tags;
        private Address _Address;
        private UInt16 _QuantityOfRegisters = 0;
        #endregion

        #region Private/Protected Properties
        private List<IModbusTag> Tags
        {
            get
            {
                if (_Tags == null)
                {
                    _Tags = new List<IModbusTag>();
                }
                return _Tags;
            }
        }
        private Int32 MaxQuantityOfRegisters
        {
            get { return _MaxQuantityOfRegisters; }
            set
            {
                if (_MaxQuantityOfRegisters != value)
                {
                    if (_MaxQuantityOfRegisters <= 250)
                    {
                        _MaxQuantityOfRegisters = value;
                    }
                }
            }
        }
        #endregion

        #region Public Properties
        public Address Address
        {
            get { return _Address; }
            private set
            {
                if (_Address != value)
                {
                    _Address = value;
                }
            }
        }
        /// <summary>
        /// Size in bytes
        /// </summary>
        public UInt16 QuantityOfRegisters
        {
            get { return _QuantityOfRegisters; }
            private set
            {
                if (_QuantityOfRegisters != value)
                {
                    _QuantityOfRegisters = value;
                }
            }
        }
        public byte[] Buffer
        {
            get;
            set;
        }
        #endregion

        #region Constructor(s)
        private HoldingRegistersWorkItem() { }
        private HoldingRegistersWorkItem(Int32 MaxQuantityOfRegisters)
        {
            this.MaxQuantityOfRegisters = MaxQuantityOfRegisters;
        }
        #endregion

        #region Private Methods
        private Boolean Add(IModbusHoldingRegistersTag tag)
        {
            if (tag.Address == null)
                return false;
            if (tag.Address.Table != Table.HoldingRegisters)
                return false;
            if (tag.QuantityOfRegisters > MaxQuantityOfRegisters)
                return false;
            if (Tags.Count == 0)
            {
                Tags.Add(tag);
                this.Address = tag.Address;
                this.QuantityOfRegisters = tag.QuantityOfRegisters;
                return true;
            }
            Address NewAddress = this.Address < tag.Address ? this.Address : tag.Address;
            UInt16 FirstIndex = NewAddress.Index;
            UInt16 LastIndex = Convert.ToUInt16(Math.Max(this.Address.Index + this.QuantityOfRegisters - 1, tag.Address.Index + tag.QuantityOfRegisters - 1));
            UInt16 NewQuantityOfRegisters = Convert.ToUInt16(LastIndex - FirstIndex + 1);

            if (NewQuantityOfRegisters > MaxQuantityOfRegisters)
                return false;

            Tags.Add(tag);
            this.Address = NewAddress;
            this.QuantityOfRegisters = NewQuantityOfRegisters;
            return true;
        }
        private void Update(IEnumerable<TagInt16> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }
        private void Update(IEnumerable<TagUInt16> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }
        private void Update(IEnumerable<TagInt32> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }
        private void Update(IEnumerable<TagUInt32> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }
        private void Update(IEnumerable<TagInt64> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }
        private void Update(IEnumerable<TagUInt64> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }
        private void Update(IEnumerable<TagSingle> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }
        private void Update(IEnumerable<TagDouble> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }

        private void Update(TagInt16 Tag)
        {
            int index = (Tag.Address.Index - this.Address.Index) * sizeof(UInt16);
            Int16 value = BitConverter.ToInt16(Buffer, index);
            Tag.ValueSetter(value);
        }
        private void Update(TagUInt16 Tag)
        {
            int index = (Tag.Address.Index - this.Address.Index) * sizeof(UInt16);
            UInt16 value = BitConverter.ToUInt16(Buffer, index);
            Tag.ValueSetter(value);
        }
        private void Update(TagInt32 Tag)
        {
            int index = (Tag.Address.Index - this.Address.Index) * sizeof(UInt16);
            Int32 value = BitConverter.ToInt32(Buffer, index);
            Tag.ValueSetter(value);
        }
        private void Update(TagUInt32 Tag)
        {
            int index = (Tag.Address.Index - this.Address.Index) * sizeof(UInt16);
            UInt32 value = BitConverter.ToUInt32(Buffer, index);
            Tag.ValueSetter(value);
        }
        private void Update(TagInt64 Tag)
        {
            int index = (Tag.Address.Index - this.Address.Index) * sizeof(UInt16);
            Int64 value = BitConverter.ToInt64(Buffer, index);
            Tag.ValueSetter(value);
        }
        private void Update(TagUInt64 Tag)
        {
            int index = (Tag.Address.Index - this.Address.Index) * sizeof(UInt16);
            UInt64 value = BitConverter.ToUInt64(Buffer, index);
            Tag.ValueSetter(value);
        }
        private void Update(TagSingle Tag)
        {
            int index = (Tag.Address.Index - this.Address.Index) * sizeof(UInt16);
            Single value = BitConverter.ToSingle(Buffer, index);
            Tag.ValueSetter(value);
        }
        private void Update(TagDouble Tag)
        {
            int index = (Tag.Address.Index - this.Address.Index) * sizeof(UInt16);
            Double value = BitConverter.ToDouble(Buffer, index);
            Tag.ValueSetter(value);
        }
        #endregion

        #region Public/Internal Methods
        public static IEnumerable<HoldingRegistersWorkItem> GetWorkItems(IEnumerable<IModbusHoldingRegistersTag> Tags, UInt16 MaxQuantityOfRegisters = 125)
        {
            IList<IModbusHoldingRegistersTag> TagList = Tags.Where(tag => tag.Address.Table == Table.HoldingRegisters).OrderBy(tag => tag.Address.Index).ToList();

            List<HoldingRegistersWorkItem> WorkItems = new List<HoldingRegistersWorkItem>();

            HoldingRegistersWorkItem WorkItem = null;
            foreach (IModbusHoldingRegistersTag tag in TagList)
            {
                if (WorkItem == null)
                {
                    WorkItem = new HoldingRegistersWorkItem(MaxQuantityOfRegisters);
                    WorkItems.Add(WorkItem);
                }
                if (!(WorkItem.Add(tag)))
                {
                    WorkItem = new HoldingRegistersWorkItem(MaxQuantityOfRegisters);
                    WorkItem.Add(tag);
                    WorkItems.Add(WorkItem);
                }
            }
            return WorkItems;
        }
        public void UpdateTags()
        {
            if (Tags.Count == 0)
                return;

            Update(Tags.OfType<TagInt16>());
            Update(Tags.OfType<TagUInt16>());
            Update(Tags.OfType<TagInt32>());
            Update(Tags.OfType<TagUInt32>());
            Update(Tags.OfType<TagInt64>());
            Update(Tags.OfType<TagUInt64>());
            Update(Tags.OfType<TagSingle>());
            Update(Tags.OfType<TagDouble>());
        }
        #endregion
    }
}
