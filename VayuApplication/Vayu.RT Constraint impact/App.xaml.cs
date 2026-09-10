using Prism.Ioc;
using System.Windows;
using Vayu.RT_Constraint_impact.Views;

namespace Vayu.RT_Constraint_impact
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
