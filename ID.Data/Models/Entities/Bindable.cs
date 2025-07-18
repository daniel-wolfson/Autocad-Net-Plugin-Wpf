using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Intellidesk.Data.Models.Entities
{
    public class Bindable : INotifyPropertyChanged
    {
        //private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        public event PropertyChangedEventHandler PropertyChanged;

        //protected T Get<T>([CallerMemberName] string name = null)
        //{
        //    Debug.Assert(name != null, "name != null");
        //    object value = null;
        //    if (_properties.TryGetValue(name, out value))
        //        return value == null ? default(T) : (T)value;
        //    return default(T);
        //}

        ///// <remarks>Use this overload when implicitly naming the property</remarks>
        //protected void Set<T>(T value, [CallerMemberName] string name = null)
        //{
        //    Debug.Assert(name != null, "name != null");
        //    if (Equals(value, Get<T>(name)))
        //        return;
        //    _properties[name] = value;
        //    OnPropertyChanged(name);
        //}
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}