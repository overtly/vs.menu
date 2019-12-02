using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using VS.Menu.Helper;
using VS.Menu.Model;

namespace VS.Menu
{
    /// <summary>
    /// ServiceDependence.xaml 的交互逻辑
    /// </summary>
    public partial class ServiceDependence : Window
    {
        private string _type;
        private List<DependenceModel> dependencies;
        delegate void threadDelegate();
        public ServiceDependence(string type)
        {
            _type = type;

            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            #region 获取已经保存的依赖
            dependencies = DependenceHelper.Get(_type) ?? new List<DependenceModel>();
            lvDependencies.ItemsSource = dependencies;
            #endregion
        }

        private void btnSure_Click(object sender, RoutedEventArgs e)
        {
            var packageId = tbPackageId.Text.Trim();
            var version = tbVersion.Text.Trim();
            var namespaces = tbNamespace.Text.Trim();
            if (string.IsNullOrEmpty(packageId))
            {
                tbPackageId.Focus();
                MessageBox.Show("请输入依赖包名称");
                return;
            }
            if (string.IsNullOrEmpty(version))
            {
                tbVersion.Focus();
                MessageBox.Show("请输入依赖包版本");
                return;
            }

            if (dependencies.Any(oo => oo.PackageId == packageId && oo.Version == version))
            {
                tbPackageId.Focus();
                MessageBox.Show("依赖包已存在");
                return;
            }

            if (dependencies.Any(oo => oo.PackageId == packageId))
            {
                var dependency = dependencies.FirstOrDefault(oo => oo.PackageId == packageId);
                dependency.Version = version;
                dependency.Namespace = namespaces;
            }
            else
            {
                dependencies.Add(new DependenceModel()
                {
                    PackageId = packageId,
                    Version = version,
                    Namespace = namespaces,
                });
            }
            DependenceHelper.Save(dependencies, _type);

            // 清除
            tbPackageId.Text = string.Empty;
            tbVersion.Text = string.Empty;
            tbNamespace.Text = string.Empty;
            tbPackageId.Focus();

            CrossThreadExecuteUI(() =>
            {
                var a = dependencies.Select(oo => new { oo.PackageId, oo.Version, oo.Namespace });
                lvDependencies.ItemsSource = a;
            });
        }


        private void cmiDelete_OnClick(object sender, RoutedEventArgs e)
        {
            if (lvDependencies.SelectedItem == null)
                return;

            var packageId = ((dynamic)lvDependencies.SelectedItem).PackageId;
            var index = dependencies.FindIndex(oo => oo.PackageId == packageId);
            if (index > -1)
            {
                dependencies.RemoveAt(index);
                DependenceHelper.Save(dependencies, _type);
            }
            CrossThreadExecuteUI(() =>
            {
                var a = dependencies.Select(oo => new { oo.PackageId, oo.Version, oo.Namespace });
                lvDependencies.ItemsSource = a;
            });
        }


        /// <summary>
        /// 跨线程更改UI
        /// </summary>
        /// <param name="action"></param>
        private void CrossThreadExecuteUI(Action action)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new threadDelegate(() =>
            {
                action();
            }));
        }

        private void LvDependencies_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (lvDependencies.SelectedItem == null)
                return;

            var packageId = ((dynamic)lvDependencies.SelectedItem).PackageId;
            var item = dependencies.FirstOrDefault(oo => oo.PackageId == packageId);
            CrossThreadExecuteUI(() =>
            {
                tbPackageId.Text = item.PackageId;
                tbNamespace.Text = item.Namespace;
                tbVersion.Text = item.Version;
            });
        }
    }
}
