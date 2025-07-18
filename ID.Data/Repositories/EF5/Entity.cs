using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using AcadNet.Data.Models;
using AcadNet.Data.Repositories.Infrastructure;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.ComponentModel;

namespace AcadNet.Data.Repositories.Ef5
{
    public abstract class BaseEntity : IObjectState, INotifyPropertyChanged
    {
        [NotMapped]
        public ObjectState ObjectState { get; set; }

        [field: NonSerialized]
        public virtual event EventHandler<EntityChangedArgs> EntityChangedEvent;

        [field: NonSerialized]
        public virtual event PropertyChangedEventHandler PropertyChanged;

        //private Dictionary<string, object> _properties = new Dictionary<string, object>();
        private List<string> _changedProperties = new List<string>();
        private List<string> _invalidProperties = new List<string>();

        [NotMapped]
        [XmlIgnore]
        [Browsable(false)]
        public virtual List<string> ChangedProperties
        {
            get { return _changedProperties; }
            set
            {
                _changedProperties = value;
            }
        }

        [NotMapped]
        [XmlIgnore]
        [Browsable(false)]
        public virtual List<string> InvalidProperties
        {
            get { return _invalidProperties; }
            set
            {
                _invalidProperties = value;
            }
        }

        #region "INotifyPropertyChanged Members"

        [NotMapped]
        [XmlIgnore]
        [Browsable(false)]
        protected bool AllowRaiseEvent { get; set; }

        public virtual void OnPropertyChanged(String propertyName = "") //[CallerMemberName]
        {
            if (this.AllowRaiseEvent)
            {
                if (!object.ReferenceEquals(PropertyChanged, null))
                {

                    if (PropertyChanged != null)
                    {
                        if (!ChangedProperties.Contains(propertyName))
                            ChangedProperties.Add(propertyName);

                        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                        
                        if (EntityChangedEvent != null)
                        {
                            EntityChangedEvent(this, new EntityChangedArgs(this, propertyName, (InvalidProperties.Count == 0)));
                        }
                    }
                }
            }
        }

        //protected void OnPropertyChanged<T>(Expression<Func<T>> action)
        //{
        //    var propertyName = GetPropertyName(action);
        //    OnPropertyChanged(propertyName);
        //}

        //private static string GetPropertyName<T>(Expression<Func<T>> action)
        //{
        //    var expression = (MemberExpression)action.Body;
        //    var propertyName = expression.Member.Name;
        //    return propertyName;
        //}

        #endregion
    }

    [Serializable]
    public abstract class PropertyNotifier : INotifyPropertyChanged
    {
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public PropertyNotifier() : base()
        {
            this.AllowRaiseEvent = true;
        }

        [XmlIgnore]
        protected bool AllowRaiseEvent { get; set; }

        #region INotifyPropertyChanged Members

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.AllowRaiseEvent)
            {
                if (!object.ReferenceEquals(this.PropertyChanged, null))
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }

        #endregion
    }
}
