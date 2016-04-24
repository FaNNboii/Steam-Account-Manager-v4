using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AccountManager.Views
{
    /// <summary>
    /// Interaktionslogik für TwoFactorWindow.xaml
    /// </summary>
    public partial class TwoFactorWindow : MVVMLibv2.MVVMWindow<Viewmodels.TwoFactorViewModel>
    {
        public TwoFactorWindow(string key)
        {
            InitializeComponent();
            ViewModel.Secret = key;
        }
    }
}
