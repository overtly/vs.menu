using System.Windows;
using VS.Menu.Helper;
using VS.Menu.Model;

namespace VS.Menu
{
    /// <summary>
    /// ServiceNugetServer.xaml 的交互逻辑
    /// </summary>
    public partial class ServiceNugetServer : Window
    {
        public ServiceNugetServer()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var data = NugetServerHelper.Get();
            tbAddress.Text = data.Address;
            tbKey.Text = data.Key;
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            var address = tbAddress.Text.Trim();
            var key = tbKey.Text.Trim();
            if (string.IsNullOrEmpty(address))
            {
                tbAddress.Focus();
                MessageBox.Show("请输入服务器地址");
                return;
            }

            var data = new NugetServerModel()
            {
                Address = address,
                Key = key
            };
            NugetServerHelper.Save(data);
            Close();
        }
    }
}
