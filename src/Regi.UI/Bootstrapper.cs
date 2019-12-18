using Caliburn.Micro;
using Regi.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Regi.UI
{
    public class Bootstrapper : BootstrapperBase
    {
        public Bootstrapper()
        {
            Initialize();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<HomeViewModel>();
        }
    }
}
