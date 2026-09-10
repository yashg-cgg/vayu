using Prism.Ioc;
using System.Windows;
using Vayu.ProfitLossDaily.Views;

namespace Vayu.ProfitLossDaily
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
            // containerRegistry.RegisterForNavigation<MainWindow, MainWindowViewModel>();
        }
        //protected override void ConfigureViewModelLocator()
        //{
        //    base.ConfigureViewModelLocator();
        //    ViewModelLocationProvider.Register(typeof(MainWindow).ToString(), typeof(MainWindowViewModel));
        //}
        public static void Cleanup()
        {
        }
    }
}
