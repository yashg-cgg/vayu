using Prism.Ioc;
using System.Windows;
using Vayu.TemperatureHistoryDetails.Views;

namespace Vayu.TemperatureHistoryDetails
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

        }
    }
}
