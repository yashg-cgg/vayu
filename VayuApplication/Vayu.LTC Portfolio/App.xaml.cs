using Prism.Ioc;
using System.Windows;
using Vayu.LTC_Portfolio.Views;

namespace Vayu.LTC_Portfolio
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
