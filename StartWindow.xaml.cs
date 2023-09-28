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
using System.Threading;
using System.IO;

namespace SerialTool2._0
{
    /// <summary>
    /// StartWindow.xaml 的交互逻辑
    /// </summary>
    public partial class StartWindow : Window
    {
        public StartWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Random random = new Random();
            int imgseed = random.Next(0,10);
            if (imgseed > 4)
            {
                try
                {
                    BG_IMG.ImageSource = new BitmapImage(new Uri("./src/bg/IMG1.png", UriKind.RelativeOrAbsolute));
                }
                catch (Exception ex) 
                {
                    ToolClass.WriteLog(ex.Message);
                }
                
            }

        }
    }
}
