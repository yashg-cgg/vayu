using Vayu.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;
using Vayu.ViewModels;
using Prism.Mvvm;

namespace Vayu
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
             containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
        }
        protected override void ConfigureViewModelLocator()
        {
            base.ConfigureViewModelLocator();
            ViewModelLocationProvider.Register(typeof(MainWindow).ToString(), typeof(MainWindowViewModel));
        }
    }
}
