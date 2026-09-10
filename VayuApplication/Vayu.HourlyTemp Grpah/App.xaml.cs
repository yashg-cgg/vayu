using Prism.Ioc;
using System.Windows;
using Vayu.HourlyTemp_Grpah.Views;

namespace Vayu.HourlyTemp_Grpah
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
