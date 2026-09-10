using Prism.Ioc;
using System.Windows;
using Vayu.LMP15Mins.Views;

namespace Vayu.LMP15Mins
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
