using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ID.Common.Model
{
    public abstract class BaseEntity : INotifyPropertyChanged, IObjectState, IEntity
    {
        [field: NonSerialized]
        [Browsable(false)]
        protected ValidationHandler ValidationHandler;

        [field: NonSerialized]
        [Browsable(false)]
        public event EventHandler<EntityChangedArgs> EntityChangedEvent;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [NotMapped, Browsable(false)]
        public ObjectState ObjectState { get; set; }

        protected T Get<T>([CallerMemberName] string propertyName = null)
        {
            //Debug.Assert(propertyName != null, "name != null");
            //object value = null;
            //(ValidationHandler != null && ValidationHandler.InvalidPropertyExist(propertyName)
            //   ? default(T): (T)value;
            //return value == null ? default(T) : (T)value;
            return default(T);
        }

        /// <remarks>Use this overload when implicitly naming the property</remarks>
        protected void Set<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(value, storage)) return;

            SetProperty(ref storage, value); //OnPropertyChanged(propertyName);

            var inValidMessage = PropertyValid(propertyName);
            if (inValidMessage == "")
                this.ObjectState = ObjectState.Modified;

            if (EntityChangedEvent != null && ValidationHandler != null)
                EntityChangedEvent(this, new EntityChangedArgs(this, propertyName, !ValidationHandler.InvalidPropertiesExist()));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed"),
         System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        protected bool SetProperty<T>(ref T storage, T value, params string[] propertyNames)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            if (propertyNames != null && propertyNames.Any())
            {
                foreach (var item in propertyNames)
                {
                    this.OnPropertyChanged(item);
                }
            }
            return true;
        }

        public virtual string PropertyValid([CallerMemberName] string propertyName = null)
        {
            return "";
        }

        public virtual T Default<T>() where T : BaseEntity, new()
        {
            return new T();  //Activator.CreateInstance(this.GetType(), null);
        }

        public virtual T Clone<T>() where T : BaseEntity, new()
        {
            return new T();
        }

        public bool IsValid()
        {
            return (ValidationHandler == null || (ValidationHandler != null && !ValidationHandler.InvalidPropertiesExist()));
            //return InvalidProperties != null && InvalidProperties.Value.Count == 0;
        }

        public bool IsModified()
        {
            return this.ObjectState == ObjectState.Modified;
            //ChangedProperties != null && ChangedProperties.Count > 0;
        }

        //public void ClearChangedProperties()
        //{
        //    ChangedProperties.Value.Clear();
        //}

        //public void ClearInvalidProperties()
        //{
        //    InvalidProperties.Value.Clear();
        //}

        //[NotMapped, XmlIgnore, Browsable(false)]
        //protected Lazy<Dictionary<string, object>> ChangedProperties = new Lazy<Dictionary<string, object>>();

        //[NotMapped, XmlIgnore, Browsable(false)]
        //protected Lazy<Dictionary<string, string>> InvalidProperties = new Lazy<Dictionary<string, string>>();
    }
}
