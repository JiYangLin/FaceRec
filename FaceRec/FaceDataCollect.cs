using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FaceRec
{
    class FaceDataCollect
    {
        CascadeClassifier faceCascade = null;
        int count = 1;
        int faceId = 0;
        Scalar clr = new Scalar(255, 0, 0);
        public bool Init()
        {
            count = 1;
            ParamInputWnd wnd = new ParamInputWnd(0);
            if (true != wnd.ShowDialog()) return false;
            faceId = wnd.id;
            if (null == faceCascade) faceCascade = new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + "haarcascade_frontalface_default.xml");
            return true;
        }
        public void SaveImg(Mat img)
        {
            Thread.Sleep(500);
            Mat gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.RGB2GRAY);

            Rect[] faces = faceCascade.DetectMultiScale(gray, 1.3, 5);
            foreach (var pos in faces)
            {
                Cv2.Rectangle(img, pos, clr);

                try
                {
                    string path = System.AppDomain.CurrentDomain.BaseDirectory + "Facedata\\" + faceId.ToString() + "\\";
                    System.IO.Directory.CreateDirectory(path);
                    gray = gray[pos.Y, pos.Y + pos.Height, pos.X, pos.X + pos.Width];

                    string pathName = path + count.ToString() + ".jpg";
                    gray.ImWrite(pathName);

                    count += 1;

                    MainWindow.obj.ShowInf("存储文件成功: "+ pathName);
                }
                catch (Exception e) { }
         
            }
        }
    }
}
