using OpenCvSharp;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace FaceRec
{
    class FaceDetection
    {
        CascadeClassifier faceCascade = null;
        CascadeClassifier eyeCascade = null;
        Scalar clr1 = new Scalar(255, 0, 0);
        Scalar clr2 = new Scalar(0, 255, 0);
        public void RecImg(Mat img)
        {
            if (null == faceCascade) faceCascade = new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + "haarcascade_frontalface_default.xml");
            if (null == eyeCascade) eyeCascade = new CascadeClassifier(System.AppDomain.CurrentDomain.BaseDirectory + "haarcascade_eye.xml");


            Mat gray = new Mat();
            Cv2.CvtColor(img, gray, ColorConversionCodes.RGB2GRAY);

            Rect[] faces = faceCascade.DetectMultiScale(gray, 1.2,5, 0, new OpenCvSharp.Size(32, 32));
            foreach (var pos in faces)
            {
                Cv2.Rectangle(img, pos, clr1, 2);

                //在检测人脸的基础上检测眼睛
                Mat fac_gray = gray[pos.Y, (pos.Y + pos.Height), pos.X, (pos.X + pos.Width)];
                Rect[] eyes = eyeCascade.DetectMultiScale(fac_gray, 1.3, 2);
                foreach (var eyepos in eyes)
                {//眼睛坐标的换算，将相对位置换成绝对位置
                    Rect rt = new Rect(pos.X + eyepos.X, pos.Y + eyepos.Y, eyepos.Width, eyepos.Height);
                    Cv2.Rectangle(img, rt, clr2, 2);
                }
            }
        }
    }
}
