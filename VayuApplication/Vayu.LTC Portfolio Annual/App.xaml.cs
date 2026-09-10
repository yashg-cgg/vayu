using Prism.Ioc;
using System.Windows;
using Vayu.LTC_PortfolioAnnual.Views;

namespace Vayu.LTC_PortfolioAnnual
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
