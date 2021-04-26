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

namespace Pomodoro
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // We force this because (apparently) a focused element won't update its source.
            uiWorkDuration.GetBindingExpression(TextBox.TextProperty).UpdateSource();
            uiBreakDuration.GetBindingExpression(TextBox.TextProperty).UpdateSource();

            Properties.Settings.Default.Save();
        }
    }
}
