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
    class FaceTraining
    {
       
        string dstPathName = System.AppDomain.CurrentDomain.BaseDirectory + "trainer-ing.yml";
        public void Train(string path)
        {
            MainWindow.obj.ShowRtInfo("开始训练");

            LBPHFaceRecognizer recognizer = LBPHFaceRecognizer.Create();

            List<Mat> mat = new List<Mat>();
            List<int> lable = new List<int>();

            DirectoryInfo TheFolder = new DirectoryInfo(path);
            foreach (DirectoryInfo NextFolder in TheFolder.GetDirectories())
            {
                int lb = Convert.ToInt32(NextFolder.Name);
                foreach (FileInfo NextFile in NextFolder.GetFiles("*.jpg"))
                {
                    lable.Add(lb);
                    mat.Add(new Mat(NextFile.FullName, ImreadModes.Grayscale));
                }
            }

            MainWindow.obj.ShowRtInfo("训练数据" + mat.Count + "条") ;
            recognizer.Train(mat,lable);

            mat.Clear();
            lable.Clear();

            recognizer.Save(dstPathName);


            MainWindow.obj.ShowRtInfo("训练结束！ " + dstPathName);
        }
    }
}
