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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.IO;
using System.Threading;
using System.Configuration;

namespace SerialTool2._0
{
    /// <summary>
    /// mainpage.xaml 的交互逻辑
    /// </summary>
    public partial class mainpage : Page
    {
        public mainpage()
        {
            InitializeComponent();
        }
        public SerialPort serialPort;
        MainWindow mw2;
        private void Page_Loaded(object sender, RoutedEventArgs e)//初始化
        {
            mw2 = Application.Current.Windows.Cast<Window>().FirstOrDefault(w => w is MainWindow) as MainWindow;
            tb_SerialRecv_TEXT_OUTPUT_SHOWMSG("Ready:) :) :) :) \r\n", false); //欢迎
            recvSendIMGWindow.SetRecvSendIMGCHMode(RecvSendIMGWindow.RecvSendIMGCHMode.SerialMode);

            #region 初始化combobox
            string[] serialcheck = { "None", "Odd", "Even", "Mark", "Space"};
            string[] serialstopbit = { "None", "One", "Two", "OnePointFive" };
            int[] serialdatabit = { 5, 6, 7, 8, 9 };
            foreach (var item in serialstopbit)
            {
                cb_SerialStopBit.Items.Add(item);
            }
            foreach (var item in serialcheck)
            {
                cb_Serialcheck.Items.Add(item);
            }
            foreach (var item in serialdatabit)
            {
                cb_SerialData.Items.Add(item);
            }
            cb_Serialcheck.SelectedValue = "None";
            cb_SerialStopBit.SelectedValue = "One";
            cb_SerialData.SelectedValue = 8;
            #endregion

            #region 波特率配置文件读取
            foreach (var item in ConfigurationManager.AppSettings["BaudValue"].Split('|'))
            {
                cb_SerialBaud.Items.Add(item);
            }
            cb_SerialBaud.SelectedValue = ConfigurationManager.AppSettings["LastSetBaud"];
            #endregion

            #region 自动扫描串口
            new Thread(() =>
            {
                bool IsFirst = true;
                string[] New_serialportname;
                string[] Old_SerialPorName = new string[9];
                while (true)
                {
                    if (IsFirst)//初次启动
                    {
                        try
                        {
                            string[] str = SerialPort.GetPortNames();
                            Old_SerialPorName = str;
                            foreach (var item in str)
                            {
                                Dispatcher.Invoke(() =>
                                {
                                    cb_SerialPort.Items.Add(item);
                                });

                            }
                            IsFirst = false;
                        }
                        catch(Exception ex)
                        {
                            ToolClass.WriteLog(ex.ToString());
                        }
                        
                    }
                    else//持续扫描
                    {
                        try
                        {
                            New_serialportname = SerialPort.GetPortNames();
                            if (Old_SerialPorName != null)
                            {
                                List<string> NS_List = New_serialportname.ToList<string>();
                                List<string> OS_List = Old_SerialPorName.ToList<string>();
                                if (!OS_List.SequenceEqual(NS_List))
                                {
                                    Dispatcher.Invoke(() => { cb_SerialPort.Items.Clear(); });
                                    foreach (var ITEM2 in New_serialportname)
                                    {
                                        Dispatcher.Invoke(() =>
                                        {
                                            cb_SerialPort.Items.Add(ITEM2);
                                        });
                                    }
                                }
                                Old_SerialPorName = New_serialportname;
                            }
                        }
                        catch (Exception ex)
                        {
                            ToolClass.WriteLog(ex.ToString());
                        }
                        
                    }
                    Thread.Sleep(10);
                }
            })
            { IsBackground = true }.Start();
            #endregion
        }

        bool IsSerialCon = false;
        private void btn_ConSerial_Click(object sender, RoutedEventArgs e)//连接串口
        {
            if (IsSerialCon == false)//连接
            {
                if (cb_SerialBaud.SelectedIndex != -1 && cb_SerialPort.SelectedIndex != -1 && cb_Serialcheck.SelectedIndex != -1
                && cb_SerialData.SelectedIndex != -1 && cb_SerialStopBit.SelectedIndex != -1)
                {
                    serialPort = new SerialPort();
                    serialPort.WriteBufferSize = 2097152000;
                    serialPort.ReadBufferSize  = 2097152000;
                    serialPort.PinChanged += SerialPort_PinChanged;
                    serialPort.ErrorReceived += SerialPort_ErrorReceived;
                    serialPort.PortName = cb_SerialPort.SelectedValue.ToString();
                    serialPort.BaudRate = int.Parse(cb_SerialBaud.SelectedValue.ToString());
                    serialPort.DataBits = int.Parse(cb_SerialData.SelectedValue.ToString());
                    switch (cb_Serialcheck.SelectedValue.ToString())
                    {
                        case "None":
                            serialPort.Parity = Parity.None;
                            break;
                        case "Odd":
                            serialPort.Parity = Parity.Odd;
                            break;
                        case "Even":
                            serialPort.Parity = Parity.Even;
                            break;
                        case "Mark":
                            serialPort.Parity = Parity.Mark;
                            break;
                        case "Space":
                            serialPort.Parity = Parity.Space;
                            break;
                    }
                    switch (cb_SerialStopBit.SelectedValue.ToString())
                    {
                        case "None":
                            serialPort.StopBits = StopBits.None;
                            break;
                        case "One":
                            serialPort.StopBits = StopBits.One;
                            break;
                        case "Two":
                            serialPort.StopBits = StopBits.Two;
                            break;
                        case "OnePointFive":
                            serialPort.StopBits = StopBits.OnePointFive;
                            break;
                    }
                    try
                    {
                        serialPort.Open();
                        serialPort.DataReceived += SerialPort_DataReceived;
                        tb_SerialRecv_TEXT_OUTPUT_SHOWMSG("Serial Is Open! \r\n",true);
                        btn_ConSerial.Content = "断开";
                        IsSerialCon = true;
                    }
                    catch(Exception ex)
                    {
                        ToolClass.WriteLog(ex.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("选项不能为空", "提示", MessageBoxButton.OK, MessageBoxImage.Stop);
                    ToolClass.WriteLog("选项不能为空");
                }
            }
            else                     //断开
            {
                if (serialPort != null && serialPort.IsOpen == true)
                {
                    try
                    {
                        serialPort.DataReceived -= SerialPort_DataReceived;
                        serialPort.PinChanged += SerialPort_PinChanged;
                        serialPort.Close();
                        serialPort.Dispose();
                        serialPort = null;
                        
                    }
                    catch(Exception ex) 
                    {
                        ToolClass.WriteLog(ex.ToString());
                    }
                }
                else if (serialPort != null && serialPort.IsOpen == false)
                {
                    serialPort.Dispose();
                    serialPort = null;
                }
                tb_SerialRecv_TEXT_OUTPUT_SHOWMSG("Serial Is Close! \r\n", true);
                btn_ConSerial.Content = "连接";
                IsSerialCon = false;
            }
        }

        UInt64 RXCount = 0;

        public List<byte> imgData;
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)//数据接收事件
        {
            RXCount += (UInt64)serialPort.BytesToRead;
            Dispatcher.Invoke(() => {
                tb_RX_Count.Text = RXCount.ToString();
            });


            if (recvSendIMGWindow.IsImgRecv == true)
            {
                Dispatcher.Invoke(() =>
                {
                    recvSendIMGWindow.lb_recvSta.Content = "正在接收";

                });
                byte[] imgrecvbytedata;
                int recvcount = serialPort.BytesToRead;
                imgrecvbytedata = new byte[recvcount];
                serialPort.Read(imgrecvbytedata, 0, recvcount);
                foreach (var it in imgrecvbytedata)
                {
                    imgData.Add(it);
                }
                Dispatcher.Invoke(() =>
                {
                    recvSendIMGWindow.IMG_IMGPL.Source = ToolClass.ByteArrayToBitmapImage(imgData.ToArray());
                    recvSendIMGWindow.lb_recvSta.Content = "已完成";
                });
                
            }
            else
            {
                if (IsHEXShowEnable == false)
                {
                    int readcount = serialPort.BytesToRead;
                    byte[] recvbyte = new byte[readcount];
                    serialPort.Read(recvbyte, 0, readcount);

                    string recvtmp = Encoding.UTF8.GetString(recvbyte);

                    Dispatcher.Invoke(() =>
                    {

                        tb_SerialRecv_TEXT_OUTPUT_SHOWMSG(recvtmp, IsAddTimeEnable);
                    });

                }
                else
                {
                    string recvtmp = serialPort.ReadExisting();
                    byte[] recvtmpbyte = Encoding.UTF8.GetBytes(recvtmp);
                    string recvtmpbytestr = "";
                    foreach (var item in recvtmpbyte)
                    {
                        recvtmpbytestr += item.ToString("X2") + " ";
                    }
                    Dispatcher.Invoke(() =>
                    {
                        tb_SerialRecv_TEXT_OUTPUT_SHOWMSG(recvtmpbytestr, IsAddTimeEnable);
                    });

                }
                Dispatcher.Invoke(() =>
                {
                    tb_SerialRecv_TEXT_OUTPUT.ScrollToEnd();
                });
            }
        }

        UInt64 TXCount = 0;
        private void btn_SerialSend_Click(object sender, RoutedEventArgs e)//串口发送事件
        {
            if (serialPort != null && serialPort.IsOpen == true)
            {
                if (IsHEXSendEnable == false)
                {
                    string Str = tb_SerialSend_TEXT_INPUT.Text;
                    string Str1 =  Str.Replace("\\r","\r");
                    string SendStr = Str1.Replace("\\n", "\n");
                    if (IsEndaddrn)
                    {
                        SendStr += "\r\n";
                    }
                    byte[] sendbyte = Encoding.UTF8.GetBytes(SendStr);
                    serialPort.Write(sendbyte,0,sendbyte.Length);
                    TXCount += (UInt64)tb_SerialSend_TEXT_INPUT.Text.Length;
                }
                else
                {
                    string[] strtmp = tb_SerialSend_TEXT_INPUT.Text.Split(' ');
                    byte[] bytetmp = new byte[strtmp.Length];

                    for (int ict = 0; ict < strtmp.Length-1; ++ict)
                    {
                        bytetmp[ict] = Convert.ToByte(strtmp[ict], 16);
                    }
                    if (IsEndaddrn)
                    {
                        List<byte> BytetmpList = bytetmp.ToList();
                        BytetmpList.Add(0x0A);
                        BytetmpList.Add(0x0D);
                        serialPort.Write(BytetmpList.ToArray(), 0, BytetmpList.ToArray().Length);
                    }
                    else
                    {
                        serialPort.Write(bytetmp, 0, bytetmp.Length);
                    }
                    string[] str_count = tb_SerialSend_TEXT_INPUT.Text.Split(' ');
                    TXCount += (UInt64)str_count.Length - 1;
                }
                Dispatcher.Invoke(() => {
                    tb_TX_Count.Text = TXCount.ToString();
                });
            }
        }

        private void btn_clsRecvcount_Click(object sender, RoutedEventArgs e)//计数清0事件
        {
            TXCount = 0;
            RXCount = 0;
            tb_RX_Count.Text = "0";
            tb_TX_Count.Text = "0";
        }

        public void tb_SerialRecv_TEXT_OUTPUT_SHOWMSG(string str,bool isShowTime)//接收页显示字符串
        {
            if (isShowTime == false)
            {
                tb_SerialRecv_TEXT_OUTPUT.Text += str;
            }
            else
            {
                tb_SerialRecv_TEXT_OUTPUT.Text +="[" + DateTime.Now + "] " +  str;
            }
        }

        bool IsHEXShowEnable = false;
        private void ckb_HEXShow_Click(object sender, RoutedEventArgs e)
        {
            IsHEXShowEnable = ckb_HEXShow.IsChecked.Value;
        }//HEX显示模式切换事件

        bool IsHEXSendEnable = false;
        bool NeedDropOnebyte = false;
        bool NeddDrop5Cbyte = false;
        private void ckb_HEXSend_Click(object sender, RoutedEventArgs e)//HEX发送模式切换事件
        {
            IsHEXSendEnable = ckb_HEXSend.IsChecked.Value;
            if (tb_SerialSend_TEXT_INPUT.Text != "")
            {
                if (IsHEXSendEnable == true)
                {
                    byte[] bytetmp = Encoding.UTF8.GetBytes(tb_SerialSend_TEXT_INPUT.Text);
                    string strtmp = "";
                    
                    foreach (var item in bytetmp)
                    {
                        if (item.ToString("X2") == "5C" && NeddDrop5Cbyte == false)
                        {
                            NeedDropOnebyte = true;
                            NeddDrop5Cbyte = true;
                            bool IsRFinish = false;
                            bool IsNFinish = false;
                            foreach (var item2 in bytetmp)
                            {
                                if (!(IsNFinish && IsRFinish))
                                {
                                    if (item2.ToString("X2") == "6E")
                                    {
                                        strtmp += "0D ";
                                        IsNFinish = true;
                                    }
                                    else if (item2.ToString("X2") == "72")
                                    {
                                        strtmp += "0A ";
                                        IsRFinish = true;
                                    }
                                }
                            }
                            
                        }
                        else
                        {
                            if (NeedDropOnebyte == false)
                            {
                                if (NeddDrop5Cbyte)
                                {
                                    if (item.ToString("X2") != "5C")
                                    {
                                        NeddDrop5Cbyte = false;
                                    }
                                }
                                else
                                {
                                    strtmp += item.ToString("X2") + " ";
                                    NeddDrop5Cbyte = false;
                                }    
                            }
                            else
                            {
                                NeedDropOnebyte = false;
                            }
                            
                        }
                        
                    }
                    tb_SerialSend_TEXT_INPUT.Text = strtmp;
                }
                else
                {
                    string strsrc = tb_SerialSend_TEXT_INPUT.Text;
                    string[] strsrcarr = strsrc.Split(' ');
                    List<byte> bytelisttmp = new List<byte>();
                    foreach (var it in strsrcarr)
                    {
                        try
                        {
                            if (it == "0A")
                            {
                                bytelisttmp.Add(Convert.ToByte("5C", 16));
                                bytelisttmp.Add(Convert.ToByte("72", 16));
                            }
                            else if (it == "0D")
                            {
                                bytelisttmp.Add(Convert.ToByte("5C", 16));
                                bytelisttmp.Add(Convert.ToByte("6E", 16));
                            }
                            else
                            {
                                bytelisttmp.Add(Convert.ToByte(it, 16));
                            }
                            
                        }
                        catch { }
                    }
                    byte[] bytearr = bytelisttmp.ToArray();
                    tb_SerialSend_TEXT_INPUT.Text = Encoding.UTF8.GetString(bytearr);
                }
            }
        }

        bool IsAddTimeEnable = false;
        private void ckb_AddTime_Click(object sender, RoutedEventArgs e)
        {
            IsAddTimeEnable = ckb_AddTime.IsChecked.Value;
        }//是否加入时间戳

        //bool isLineChartEnable = false;
        //private void ckb_LineChart_Click(object sender, RoutedEventArgs e)
        //{
        //    isLineChartEnable = ckb_LineChart.IsChecked.Value;
        //    if (isLineChartEnable)
        //    {
        //        mw2.lineChartWindow.Show();
        //    }
        //    else
        //    {
        //        mw2.lineChartWindow.Hide();
        //    }

        //}//是否折线图

        private void btn_clsRecvData_Click(object sender, RoutedEventArgs e)
        {
            tb_SerialRecv_TEXT_OUTPUT.Text = "";
        }//清除接收区

        bool isKeepSendEnable = false;
        Thread KeepSendThread;
        private void ckb_Keepsend_Click(object sender, RoutedEventArgs e)//自动发送
        {
            isKeepSendEnable = ckb_Keepsend.IsChecked.Value;
            if (isKeepSendEnable)
            {
                int SleepTime = 1;
                if (!int.TryParse(tb_KeepSendTime.Text, out SleepTime))
                {
                    MessageBox.Show("间隔时间只能输入Int类型");
                }
                KeepSendThread = new Thread(() =>
                {
                    while (isKeepSendEnable)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            btn_SerialSend_Click(null, null);
                        });
                        Thread.Sleep(SleepTime);
                    }
                })
                { IsBackground = true };
                KeepSendThread.Start();
            }
            else
            {
                if (KeepSendThread != null)
                {
                    try
                    {
                        KeepSendThread.Abort();
                        KeepSendThread = null;
                    }
                    catch
                    {
                        KeepSendThread = null;
                    }
                    
                }
            }
        }

        bool IsEndaddrn = false;
        private void ckb_endaddrn_Click(object sender, RoutedEventArgs e)
        {
            IsEndaddrn = ckb_endaddrn.IsChecked.Value;
        }

        bool IsEnableImgRS = false;
        RecvSendIMGWindow recvSendIMGWindow = new RecvSendIMGWindow();
        private void ckb_ImgRS_Click(object sender, RoutedEventArgs e)
        {
            IsEnableImgRS = ckb_ImgRS.IsChecked.Value;
            if (IsEnableImgRS)
            {

                recvSendIMGWindow.Show();
            }
            else
            {
                recvSendIMGWindow.Hide();
                recvSendIMGWindow.IsImgRecv = false;
            }
        }

        #region 弃用
        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {

        }

        private void SerialPort_PinChanged(object sender, SerialPinChangedEventArgs e)
        {

        }
        #endregion


    }
}
