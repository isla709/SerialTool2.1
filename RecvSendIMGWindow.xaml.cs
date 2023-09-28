using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SerialTool2._0
{
    /// <summary>
    /// RecvSendIMGWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RecvSendIMGWindow : Window
    {
        public RecvSendIMGWindow()
        {
            InitializeComponent();
        }

        public enum RecvSendIMGCHMode
        {
            None,
            SerialMode,
            NetWorkMode
        }

        RecvSendIMGCHMode SetRunMode = RecvSendIMGCHMode.None;
        public void SetRecvSendIMGCHMode(RecvSendIMGCHMode arg)//模式设置
        {
            SetRunMode = arg;
        }

        MainWindow mw1;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mw1 = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w is MainWindow) as MainWindow;
            if (SetRunMode == RecvSendIMGCHMode.SerialMode)//串口模式
            {

                new Thread(() =>
                {
                    while(true)
                    {
                        if (mw1.mpage.serialPort != null)
                        {
                            Dispatcher.Invoke(() =>
                            {
                                try
                                {
                                    if (mw1.mpage.serialPort.IsOpen == true)
                                    {
                                        lb_conTarget.Content = mw1.mpage.serialPort.PortName;
                                    }
                                    else
                                    {
                                        lb_conTarget.Content = "未连接";
                                    }
                                }
                                catch
                                {
                                    lb_conTarget.Content = "未连接";
                                }

                            });
                        }
                        else
                        {
                            Dispatcher.Invoke(() => {
                                lb_conTarget.Content = "NULL";
                            });
                            
                        }

                        Thread.Sleep(10);
                    }
                })
                { IsBackground = true }.Start();


            }
            if (SetRunMode == RecvSendIMGCHMode.NetWorkMode)//网络模式
            {

            }

        }

        private void btn_Img_Send_Path_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog Img_sendPathFileDialog = new OpenFileDialog()
                {
                    Filter = "图像文件(*.jpg;*.png;*bmp)|*.jpg;*.png;*bmp",
                    AddExtension = true,
                };
                Img_sendPathFileDialog.ShowDialog();
                tb_Img_Send_Path.Text = Img_sendPathFileDialog.FileName;
            }
            catch
            { 
                
            }
        }

        SaveFileDialog Img_RecvPathFileDialog = new SaveFileDialog()
        {
            Filter = "图像文件(*.jpg;*.png;*bmp)|*.jpg;*.png;*bmp",
            AddExtension = true,
        };
        private void btn_Img_Recv_Path_Click(object sender, RoutedEventArgs e)
        {
            
            Img_RecvPathFileDialog.ShowDialog();
            Img_RecvPathFileDialog.FileOk += Img_RecvPathFileDialog_FileOk;
        }

        private void Img_RecvPathFileDialog_FileOk(object sender, System.ComponentModel.CancelEventArgs e)
        {
            File.WriteAllBytes(Img_RecvPathFileDialog.FileName, mw1.mpage.imgData.ToArray());
        }

        private void btn_Send_Click(object sender, RoutedEventArgs e)
        {
            if (lb_conTarget.Content.ToString() != "NULL")
            {
                if (mw1.mpage.serialPort.IsOpen != false)
                {
                    FileStream fileStream = new FileStream(tb_Img_Send_Path.Text,FileMode.Open);
                    byte[] imgbytedata = new byte[fileStream.Length];
                    fileStream.Read(imgbytedata,0,imgbytedata.Length);
                    fileStream.Close();
                    mw1.mpage.serialPort.Write(imgbytedata, 0, imgbytedata.Length);
                }
            }
        }

        public bool IsImgRecv = false;
        private void btn_Recv_Click(object sender, RoutedEventArgs e)
        {
            lb_recvSta.Content = "就绪";
            mw1.mpage.imgData = new List<byte>();
            IsImgRecv = true;
        }

        private void title_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void tb_Img_Send_Path_TextChanged(object sender, TextChangedEventArgs e)
        {
            FileStream fileStream = new FileStream(tb_Img_Send_Path.Text, FileMode.Open);
            byte[] imgbytedata = new byte[fileStream.Length];
            fileStream.Read(imgbytedata, 0, imgbytedata.Length);
            fileStream.Close();
            var imgdata = (Bitmap)System.Drawing.Image.FromStream(new MemoryStream(imgbytedata));
            IMG_IMGPL.Source = ToolClass.BitmapToBitmapImage(imgdata);
        }
    }
}
