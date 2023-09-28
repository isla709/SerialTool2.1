using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialTool2._0
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ToolClass.RunStartWindow(this, new StartWindow());
        }
        public ToolClass tools = new ToolClass();
        public LineChartWindow lineChartWindow = new LineChartWindow();
        public mainpage mpage = new mainpage();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            ToolClass.CrateLogFile();
            PagePL.Content = new Frame() { Content = mpage };

        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ToolClass.WriteLog("App Is Close");
        }

        #region 窗口基础功能
        private void Title_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void Btn_Close_MouseEnter(object sender, MouseEventArgs e)
        {
            //bool IsThreadEnd = false;
            new Thread(() =>
            {
                byte RVal = 255;
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            Btn_Close.Fill = new SolidColorBrush(Color.FromArgb(255, RVal, 0, 0));
                        }
                        catch(Exception ex) 
                        {
                            ToolClass.WriteLog(ex.ToString());
                        }
                        
                    });
                    RVal--;
                    //Console.WriteLine(RVal.ToString());
                    if (RVal < 100)
                    {
                        break;
                        
                    }
                    //Thread.Sleep(1);

                }
                Dispatcher.Invoke(() =>
                {
                    //IsThreadEnd = true;
                });
            })
            { IsBackground = true }.Start(); 
            
        }

        private void Btn_Close_MouseLeave(object sender, MouseEventArgs e)
        {
            //bool IsThreadEnd = false;
            new Thread(() => 
            {
                byte RVal = 255;
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            Btn_Close.Fill = new SolidColorBrush(Color.FromArgb(255, RVal, 0, 0));
                        }
                        catch (Exception ex) 
                        {
                            ToolClass.WriteLog(ex.ToString());
                        }
                    });
                    RVal++;
                    //Console.WriteLine(RVal.ToString());
                    if (RVal >= 255)
                    {
                        break;

                    }
                    //Thread.Sleep(1);
                }
                Dispatcher.Invoke(() =>
                {
                    //IsThreadEnd = true;
                });
            })
            { IsBackground = true }.Start();
        }

        private void Btn_Close_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        private void menu_mainpage_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
