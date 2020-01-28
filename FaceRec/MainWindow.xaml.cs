using Comm;
using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace FaceRec
{
    public delegate void CamFrameOpt(Mat img);
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow
    {
        #region threadctl
        bool m_Runing = false;
        bool isRunning()
        {
            if (m_Runing)
            {
                OptWaitWnd wnd = new OptWaitWnd("", "正在运行,请稍后操作");
                wnd.ShowDialog();
                return true;
            }
            return false;
        }
        delegate void thrRun();
        void Run(thrRun fun)
        {
            if (isRunning()) return;
            m_Runing = true;
            Thread thread = new Thread(() =>
            {
                try
                {
                    fun();
                    m_Runing = false;
                }
                catch (Exception ex)
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        OptWaitWnd wnd = new OptWaitWnd("", ex.ToString());
                        wnd.ShowDialog();
                    });
                    m_Runing = false;
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }
        #endregion


        void ShowRt(bool show = true)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (show)
                {
                    rt.Visibility = Visibility.Visible;
                    img.Visibility = Visibility.Hidden;
                }
                else
                {
                    rt.Visibility = Visibility.Hidden;
                    img.Visibility = Visibility.Visible;
                }
                rt.Document.Blocks.Clear();
            });
        }
        public void ShowRtInfo(string inf)
        {
            inf = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ":" + inf;
            this.Dispatcher.Invoke(() =>
            {
                rt.Document.Blocks.Add(new Paragraph(new Run(inf) { Foreground = System.Windows.Media.Brushes.Green }));
                rt.ScrollToEnd();
            });
        }


        public static MainWindow obj = null;
        FaceDetection m_FaceDetection = new FaceDetection();
        FaceDataCollect m_FaceDataCollect = new FaceDataCollect();
        FaceRecognition m_FaceRecognition = new FaceRecognition();
        FaceTraining m_FaceTraining = new FaceTraining();
        public MainWindow()
        {
            InitializeComponent();
            obj = this;
        }
        public void ShowInf(string inf)
        {
            this.Dispatcher.Invoke(() =>
            {
                status.Content = inf;
            });
        }
        #region 相机操作
        bool startCamOpt = false;
        public int FrameWidth = 0;
        public int FrameHeight = 0;
        public void CamOpt(CamFrameOpt Fun)
        {
            ShowRt(false);
            startCamOpt = true;
            VideoCapture _capture = new VideoCapture(0);
            FrameWidth = (int)_capture.Get(VideoCaptureProperties.FrameWidth);
            FrameHeight = (int)_capture.Get(VideoCaptureProperties.FrameHeight);
            Mat image = new Mat();
            while (_capture.Read(image))
            {
                if (!startCamOpt)
                {
                    image.Release();
                    _capture.Release();
                    return;
                }
                Fun(image);
                ShowImg(image);
                image.Release();
                image = new Mat();
            }
            image.Release();
        }
        public void StopCamOpt()
        {
            startCamOpt = false;
        }
        void ShowImg(Mat frame)
        {
            this.Dispatcher.Invoke(() =>
            {
                Bitmap bitmap = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                using (MemoryStream stream = new MemoryStream())
                {
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
                    stream.Position = 0;
                    BitmapImage result = new BitmapImage();
                    result.BeginInit();
                    result.CacheOption = BitmapCacheOption.OnLoad;
                    result.StreamSource = stream;
                    result.EndInit();
                    result.Freeze();
                    img.Source = result;
                }
            });
        }
        #endregion




        #region 人脸检测
        private void FaceDetecStartBtn_Click(object sender, RoutedEventArgs e)
        {
            Run(() => CamOpt(m_FaceDetection.RecImg));
        }
        private void FaceDetecStopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopCamOpt();
        }
        #endregion


        #region 人脸识别
        private void FaceRecStartBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning()) return;
            Run(() =>
            {
                if (!m_FaceRecognition.Init()) return;
                CamOpt(m_FaceRecognition.RecImg);
            });
        }
        private void FaceRecStopBtn_Click(object sender, RoutedEventArgs e)
        {
            StopCamOpt();
        }
        #endregion


        #region 人脸数据采集
        private void StartCollectBtn_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning()) return;
            if (!m_FaceDataCollect.Init()) return;
            Run(() => CamOpt(m_FaceDataCollect.SaveImg));
        }
        private void StopCollectBtn_Click(object sender, RoutedEventArgs e)
        {
            StopCamOpt();
        }
        #endregion

        #region 训练
        private void StartTrainBtn_Click(object sender, RoutedEventArgs e)
        {
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "Facedata\\";
            if(!Directory.Exists(path))
            {
                OptWaitWnd dlg = new OptWaitWnd("文件夹不存在", "不存在文件夹：" + path);
                dlg.ShowDialog();
                return;
            }

            ShowRt(true);
            Run(()=> {
                m_FaceTraining.Train(path);
            });
          
        }
        #endregion


    }
}
