using Prism.Ioc;
using System.Windows;
using Vayu.ERCOT_Market_Overview.Views;

namespace Vayu.ERCOT_Market_Overview
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
