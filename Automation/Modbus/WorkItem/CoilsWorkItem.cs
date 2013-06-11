using System;
using System.Collections.Generic;
using System.Linq;
using Automation.Modbus.Tag;

namespace Automation.Modbus.WorkItem
{
    class CoilsWorkItem : IWorkItem
    {
        #region Fields
        private Int32 _MaxQuantityOfCoils = 2000;
        private List<IModbusTag> _Tags;
        private Address _Address = new Address { Table = Table.Coils, Index = 0 };
        private UInt16 _QuantityOfCoils = 0;
        #endregion

        #region Private/Protected Properties
        private IList<IModbusTag> Tags
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
        private Int32 MaxQuantityOfCoils
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
        public UInt16 QuantityOfCoils
        {
            get { return _QuantityOfCoils; }
            private set
            {
                if (_QuantityOfCoils != value)
                {
                    _QuantityOfCoils = value;
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
        private CoilsWorkItem() { }
        private CoilsWorkItem(Int32 MaxQuantityOfCoils)
        {
            this.MaxQuantityOfCoils = MaxQuantityOfCoils;
        }
        #endregion

        #region Private Methods
        private Boolean Add(IModbusCoilsTag tag)
        {
            if (tag.Address == null)
                return false;
            if (tag.Address.Table != Table.Coils)
                return false;
            if (tag.QuantityOfCoils > MaxQuantityOfCoils)
                return false;
            if (Tags.Count == 0)
            {
                Tags.Add(tag);
                this.Address = tag.Address;
                this.QuantityOfCoils = tag.QuantityOfCoils;
                return true;
            }
            Address NewAddress = this.Address < tag.Address ? this.Address : tag.Address;
            UInt16 FirstIndex = NewAddress.Index;
            UInt16 LastIndex = Convert.ToUInt16(Math.Max(this.Address.Index + this.QuantityOfCoils - 1, tag.Address.Index + tag.QuantityOfCoils - 1));
            UInt16 NewQuantityOfCoils = Convert.ToUInt16(LastIndex - FirstIndex + 1);

            if (NewQuantityOfCoils > MaxQuantityOfCoils)
                return false;

            Tags.Add(tag);
            this.Address = NewAddress;
            this.QuantityOfCoils = NewQuantityOfCoils;
            return true;
        }
        private void Update(IEnumerable<TagBoolean> Tags)
        {
            foreach (var tag in Tags)
            {
                Update(tag);
            }
        }
        private void Update(TagBoolean Tag)
        {
            int index = Tag.Address.Index - this.Address.Index;
            Boolean value = BitConverter.ToBoolean(Buffer, index);
            Tag.ValueSetter(value);
        }
        #endregion

        #region Public/Internal Methods
        public static IEnumerable<CoilsWorkItem> GetWorkItems(IEnumerable<IModbusCoilsTag> Tags, UInt16 MaxQuantityOfCoils = 2000)
        {
            IList<IModbusCoilsTag> TagList = Tags.Where(tag => tag.Address.Table == Table.Coils).OrderBy(tag => tag.Address.Index).ToList();

            List<CoilsWorkItem> WorkItems = new List<CoilsWorkItem>();

            CoilsWorkItem WorkItem = null;
            foreach (IModbusCoilsTag tag in TagList)
            {
                if (WorkItem == null)
                {
                    WorkItem = new CoilsWorkItem(MaxQuantityOfCoils);
                    WorkItems.Add(WorkItem);
                }
                if (!(WorkItem.Add(tag)))
                {
                    WorkItem = new CoilsWorkItem(MaxQuantityOfCoils);
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

            Update(Tags.OfType<TagBoolean>());
        }
        #endregion
    }
}
