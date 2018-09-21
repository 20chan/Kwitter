using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace CSAssist
{
    /// <summary>
    /// ChatList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChatList : UserControl
    {
        public int MaxItems { get; set; } = 50;
        public event Action AnywayMouseDown;
        public ObservableCollection<ChatViewModel> Items { get; set; }
        public ChatList()
        {
            InitializeComponent();

            Items = new ObservableCollection<ChatViewModel>();

            this.DataContext = this;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;
        }
        
        public void Add(string name, string msg)
        {
            if (Items.Count == MaxItems)
                Items.RemoveAt(0);
            Items.Add(new ChatViewModel { Name = name, Message = msg });
            scroller.ScrollToEnd();
        }
        
        private void ChatBox_AnywayMouseDown()
        {
            AnywayMouseDown?.Invoke();
        }
    }
}
