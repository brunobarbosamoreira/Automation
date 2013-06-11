using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Sockets;

namespace Automation
{
    public abstract class PLC : NotifyPropertyChanged, IPLC
    {
        #region Fields
        private String _Name;
        private Boolean _Connected;
        private List<ITag> _Tags = new List<ITag>();
        private List<ITag> _SubscribedTags = new List<ITag>();
        private TimeSpan _PollingPeriod = TimeSpan.FromMilliseconds(250);
        #endregion

        #region Public Properties
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

        #region Protected Properties
        protected List<ITag> Tags { get { return _Tags; } }
        protected List<ITag> SubscribedTags { get { return _SubscribedTags; } }
        #endregion

        #region IPLC
        ReadOnlyCollection<ITag> IPLC.Tags
        {
            get { return Tags.AsReadOnly(); }
        }
        #endregion
    }
}
