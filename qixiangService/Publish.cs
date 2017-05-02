using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using Microsoft.Office.Interop.Word;
using System.Drawing;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using Spire.Doc;
using System.Web;

namespace qixiangService
{
    class Publish
    {
        private string ConfigPath = "";
        //发送数据
        public void posthData(string URL,string path, string fileType)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(path);
                string fileName = fileInfo.Name.Substring(0, fileInfo.Name.IndexOf("."));
                //保存
                wordToHtml(path, "test");
                //获取
                string data = "";
                data = changeRoute(ConfigPath + "_images", ConfigPath + ".html");
                string encodeStr = HttpUtility.UrlEncode(data);
                byte[] BData = Encoding.UTF8.GetBytes("data=" + encodeStr + "&fileName=" + fileName + "&fileType=" + fileType);

                HttpWebRequest request = WebRequest.Create(URL) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.KeepAlive = true;
                request.ContentLength = BData.Length;

                Stream dataStream = request.GetRequestStream();

                dataStream.Write(BData, 0, BData.Length);
                dataStream.Close();

                //接受返回值
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                string value = reader.ReadToEnd();
                if (System.Configuration.ConfigurationManager.AppSettings["aimURL"].ToString() != value)
                {
                    System.Configuration.ConfigurationManager.AppSettings["aimURL"] = value;
                }

                reader.Close();
                stream.Close();
                response.Close();
            }
            catch (Exception e)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter("c:\\qixiangtext.txt",true))
                {
                    file.WriteLine(e.Message);
                }
                    throw;
            }
            
            
        }

        public void sendImage(string URL, string path)
        {
            new WebClient().UploadFile(URL, path);
        }

        public void sendDocument(string URL, string path)
        {

        }


        //解析数据
        public void wordToHtml(string filePath,string makeRoute)
        {

            Spire.Doc.Document document = new Spire.Doc.Document();
            document.LoadFromFile(filePath);

            //创建目录
            string TempDPath = System.Configuration.ConfigurationManager.AppSettings["TempDPath"];
            if (!Directory.Exists(TempDPath))
            {
                Directory.CreateDirectory(TempDPath);
            }
            string filename = "";
            if (makeRoute == null)
            {
                filename = "test";
            }
            else
            {
                filename = makeRoute;
            }

            //保存位置(暂时的)
            ConfigPath = TempDPath +"\\"+ filename;
            string saveFileName = ConfigPath + ".html";

            document.SaveToFile(saveFileName,FileFormat.Html);
            document.Close();
        }


        public string ImageToBase64(string imageFilename)
        {
            try
            {
                Bitmap bmp = new Bitmap(imageFilename);
                BinaryFormatter binFormatter = new BinaryFormatter();
                MemoryStream memStream = new MemoryStream();
                bmp.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                byte[] arr = new byte[memStream.Length];
                memStream.Position = 0;
                memStream.Read(arr, 0, (int)memStream.Length);
                memStream.Close();
                string strbase64 = Convert.ToBase64String(arr);
                
                return strbase64;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return "";
            }
        }

        //
        public string changeRoute(string imediaPath, string htmlRoute)
        {
            // 图片格式转换
            //DirectoryInfo folder = new DirectoryInfo(imediaPath);

            WebClient wc = new WebClient();
            string str = wc.DownloadString(htmlRoute);
            //先提取body
            int bodyStartIndex = str.IndexOf("<body");
            int divFirstIndex = str.IndexOf("<div", bodyStartIndex);
            int bodyEndIndex = str.IndexOf("</body>");
            try
            {
                str = str.Substring(divFirstIndex, bodyEndIndex - divFirstIndex);
                /*foreach (FileInfo file in folder.GetFiles())
                {
                    string tempBase64 = ImageToBase64(file.FullName);
                    string srcRoute = file.Directory.Name + "\\" + file.Name;
                    str = str.Replace(srcRoute, "data:image/png;base64,"+tempBase64);
                }*/
                return str;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return "";
        }



    }
}
