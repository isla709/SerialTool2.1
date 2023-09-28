using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SerialTool2._0
{
    public class ToolClass
    {
        public static string LogFilePath = null;
        
        public ToolClass() { }

        public static void RunStartWindow(MainWindow mw,StartWindow startWindow)
        {
            mw.Hide();
            startWindow = new StartWindow();
            startWindow.Show();
            Thread.Sleep(3000);
            startWindow.Close();
            mw.Show();
            Console.WriteLine();
        }

        public static void CrateLogFile()
        {
            if (!Directory.Exists("./Log"))
            { 
                Directory.CreateDirectory("./Log");
            }
            string Time = DateTime.Now.Year.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Day.ToString() + " " + DateTime.Now.Hour + "." + DateTime.Now.Minute;
            string FileName ="./Log/" + Time + ".log";
            File.AppendAllText(FileName,"[" + DateTime.Now.ToString() + "]: Log Is Crate \n\r");
            LogFilePath = FileName;
            
        }
        public static void WriteLog(string str)
        {
            if (LogFilePath != null)
            {
                File.AppendAllText(LogFilePath, "[" + DateTime.Now.ToString() + "]: " + str + "\n\r");
            }

            
        }


        // Bitmap --> BitmapImage
        public static BitmapImage BitmapToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png); // 坑点：格式选Bmp时，不带透明度

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                // According to MSDN, "The default OnDemand cache option retains access to the stream until the image is needed."
                // Force the bitmap to load right now so we can dispose the stream.
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }


        // BitmapImage --> Bitmap
        public static Bitmap BitmapImageToBitmap(BitmapImage bitmapImage)
        {
            // BitmapImage bitmapImage = new BitmapImage(new Uri("../Images/test.png", UriKind.Relative));

            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }


        // ImageSource --> Bitmap
        public static Bitmap ImageSourceToBitmap(ImageSource imageSource)
        {
            BitmapSource m = (BitmapSource)imageSource;

            System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(m.PixelWidth, m.PixelHeight, System.Drawing.Imaging.PixelFormat.Format32bppPArgb); // 坑点：选Format32bppRgb将不带透明度

            System.Drawing.Imaging.BitmapData data = bmp.LockBits(
            new System.Drawing.Rectangle(System.Drawing.Point.Empty, bmp.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

            m.CopyPixels(Int32Rect.Empty, data.Scan0, data.Height * data.Stride, data.Stride);
            bmp.UnlockBits(data);

            return bmp;
        }

        // BitmapImage --> byte[]
        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
        {
            byte[] bytearray = null;
            try
            {
                Stream smarket = bmp.StreamSource; ;
                if (smarket != null && smarket.Length > 0)
                {
                    //设置当前位置
                    smarket.Position = 0;
                    using (BinaryReader br = new BinaryReader(smarket))
                    {
                        bytearray = br.ReadBytes((int)smarket.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return bytearray;
        }


        // byte[] --> BitmapImage
        public static BitmapImage ByteArrayToBitmapImage(byte[] array)
        {
            using (var ms = new MemoryStream(array))
            {
                BitmapImage bmp = null;
                try
                {
                    bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = new MemoryStream(array);
                    bmp.EndInit();
                    bmp.Freeze();
                }
                catch(Exception ex)
                {
                    bmp = null;
                }
                return bmp;
            }
        }
    }
}
