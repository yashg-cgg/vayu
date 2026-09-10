using Prism.Ioc;
using System.Windows;
using Vayu.Node_Price_Hourly.Views;

namespace Vayu.Node_Price_Hourly
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
