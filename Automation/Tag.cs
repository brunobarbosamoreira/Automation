using System;
using System.ComponentModel;

namespace Automation
{
    public abstract class Tag<T> : NotifyPropertyChanged
         where T : struct
    {
        #region Fields
        private String _Name;
        private T _Value;
        #endregion

        /// <summary>
        /// Tag Value
        /// </summary>
        public T Value
        {
            get
            {
                return _Value;
            }
            set
            {
                if (!(_Value.Equals(value)))
                {
                    OnPropertyChanged("Value");
                    WriteToPLC(value);
                }
            }
        }

        /// <summary>
        /// Tag Name
        /// </summary>
        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        /// <summary>
        /// Write Value to PLC
        /// </summary>
        protected abstract void WriteToPLC(T Value);

        /// <summary>
        /// Value setter. Method used by the communication driver, to set the "Value" property with the updated value returned from the PLC.
        /// </summary>
        internal void ValueSetter(T Value)
        {
            if (!(_Value.Equals(Value)))
            {
                _Value = Value;
                OnPropertyChanged("Value");
            }
        }
    }
}
