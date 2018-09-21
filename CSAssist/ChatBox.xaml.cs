using System;
using System.Collections.Generic;
using System.Globalization;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CSAssist
{
    /// <summary>
    /// ChatBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChatBox : UserControl
    {
        public event Action AnywayMouseDown;
        public ChatBox()
        {
            InitializeComponent();

            msg.Loaded += Msg_Loaded;
        }

        private void Msg_Loaded(object sender, RoutedEventArgs e)
        {
            var gap = this.ActualHeight - msg.ActualHeight;
            this.Height = msg.Height + gap;
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AnywayMouseDown?.Invoke();
        }
    }
}
