using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Interfaces;
using Intellidesk.Data.Models.Entities;
using System;
using System.ComponentModel;

namespace Intellidesk.AcadNet.Common.Core
{

    public abstract class PaletteBaseViewModel1<T> : BaseViewModel, INotifyPropertyChanged, IBaseViewModel
        where T : IPaletteElement
    {
        #region <props>

        public virtual bool IsAddTitleEnabled { get; set; } = false;

        private T _currentElement;
        public new T CurrentElement
        {
            get { return _currentElement; }
            set
            {
                _currentElement = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region <Methods>

        public CommandArgs StartCommand(string commandName)
        {
            var commandArgs = new CommandArgs(null, commandName);
            return commandArgs;
        }

        public virtual void Init()
        {
            ElementItems.Clear();
            CurrentElement = (T)Activator.CreateInstance(typeof(T));
        }

        #endregion

        #region <Callback>

        private void OnCommandCancelled(object sender, CommandEventArgs e)
        {
            _lastSelectAddedObjectId = ObjectId.Null;
        }

        protected void OnCurrentElementChanged(object sender, PropertyChangedEventArgs args)
        {
            OnPropertyChanged("IsAddTitleEnabled");
        }

        #endregion
    }
}
