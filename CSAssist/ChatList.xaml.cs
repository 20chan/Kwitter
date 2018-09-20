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
        public ObservableCollection<ChatViewModel> Items { get; set; }
        public ChatList()
        {
            InitializeComponent();

            Items = new ObservableCollection<ChatViewModel>()
            {
                new ChatViewModel { Name = "이름1", Message = "테스트" },
                new ChatViewModel { Name = "핵간지짱짱이름", Message = "나는 핵 간지 짱짱 이름을 가졋다" },
                new ChatViewModel { Name = "어", Message = "아주 긴 메시지 아주 긴 메시지 정말 메시지가 넘 길어" },
                new ChatViewModel { Name = "짱구", Message = "메시지 메시지" },
            };

            this.DataContext = this;

            if (DesignerProperties.GetIsInDesignMode(this))
                return;
        }
        
        public void Add(string name, string msg)
        {
            Items.Add(new ChatViewModel { Name = name, Message = msg });
            scroller.ScrollToEnd();
        }
    }
}
