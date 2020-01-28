using Comm;
using OpenCvSharp;
using OpenCvSharp.Face;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceRec
{
    class FaceRecognition
    {
        CascadeClassifier faceCascade = null;
        LBPHFaceRecognizer recognizer = null;
        Scalar clr1 = new Scalar(255, 0, 0);
        Scalar clr2 = new Scalar(0, 0, 0);
        List<string> faceName;
        public bool Init()
        {
            if (null == faceName)
            {
                faceName = new List<string>();
                using (StreamReader sr = new StreamReader(new FileStream(System.AppDomain.CurrentDomain.BaseDirectory + "facename.txt", FileMode.Open), Encoding.UTF8))
                {
                    string strline;
                    while ((strline = sr.ReadLine()) != null)
                    {
                        faceName.Add(strline);
                    }
                }
            }


            if (null == faceCascade) faceCascade = new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + "haarcascade_frontalface_default.xml");
            
            if (null == recognizer)
            {
                recognizer = LBPHFaceRecognizer.Create();

                OptWaitWnd wnd = null;
                MainWindow.obj.Dispatcher.Invoke(()=> {
                    wnd = new OptWaitWnd("正在处理", "正在读取数据，请稍后");
                    wnd.Show();
                });
                recognizer.Read(System.AppDomain.CurrentDomain.BaseDirectory + "trainer.yml");
                MainWindow.obj.Dispatcher.Invoke(() => {
                    wnd.Close();
                });
            }
            return true;
        }

        public void RecImg(Mat img)
        {
            int minW = MainWindow.obj.FrameWidth / 10;
            int minH = MainWindow.obj.FrameHeight / 10;

            Mat gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.RGB2GRAY);

            Rect[] faces = faceCascade.DetectMultiScale(gray, 1.2, 5, 0, new OpenCvSharp.Size(minW, minH));

            foreach (var pos in faces)
            {
                Cv2.Rectangle(img, pos, new Scalar(255, 0, 0), 2);

                Mat temp = gray[pos.Y, pos.Y + pos.Height, pos.X, pos.X + pos.Width];
                int label;
                double confidence;
                recognizer.Predict(temp,out label,out confidence);

                string nameStr = "unknown";
                string confidenceStr = "";
                if (confidence < 100)
                {
                   
                    if (label < faceName.Count) nameStr = faceName[label];
                    confidenceStr = string.Format("{0}",(int)(100 - confidence + 38));
                }
                else
                {
                    confidenceStr = string.Format("{0}", (int)(100 - confidence));
                }

                OpenCvSharp.Point pt = new OpenCvSharp.Point(pos.X, pos.Y);
                Cv2.PutText(img, nameStr, pt, HersheyFonts.HersheyComplex, 1,clr1,1);
                pt = new OpenCvSharp.Point(pos.X+5, pos.Y + pos.Height-5);
                Cv2.PutText(img, confidenceStr, pt, HersheyFonts.HersheyComplex, 1, clr2, 1);
            }
        }
    }
}
