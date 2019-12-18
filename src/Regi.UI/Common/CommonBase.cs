using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Regi.UI.Common
{
    public class CommonBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;

            if (handler != null)
            {
                PropertyChangedEventArgs args = new PropertyChangedEventArgs(propertyName);

                handler(this, args);
            }
        }
    }
}
