using System.Configuration;
using System.Data;
using System.Windows;
using VkdpSales.ViewModels.Main;
using VkdpSales.Views.Main;

namespace VkdpSales
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Start(object sender, StartupEventArgs e)
        {
            // ✅ Не закрывать приложение при закрытии первого окна
            //ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var loginWin = new LoginWindow();
            loginWin.DataContext = new LoginViewModel();
            loginWin.Show();
        }
    }

}
