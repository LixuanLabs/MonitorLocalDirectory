using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Runtime.InteropServices;
using System.Web;
using System.Net;
using System.IO;
using System.Threading;

namespace qixiangService
{
    public partial class qixiangService : ServiceBase
    {
        public qixiangService()
        {
            InitializeComponent();
        }
      
        protected override void OnStart(string[] args)
        {
            string watchDir = System.Configuration.ConfigurationManager.AppSettings["watchDir"];
            string tempDir = System.Configuration.ConfigurationManager.AppSettings["TempDPath"];
            //创建一个临时目录
            Directory.CreateDirectory(tempDir);
            //监控文件夹
            WatchAssistant WatchDirs = ConfigurationManager.GetSection("WatchDirs") as WatchAssistant;
            
            foreach (string key in WatchDirs.watchDir.Settings.Keys)
            {
                Watch watch = new Watch();
                string filter = "*.*";
                watch.setProp(key, WatchDirs.watchDir.Settings[key], filter);
                watch.WatchStart();
                //Thread thread = new Thread(watch.WatchStart);
                //thread.Start();
                //thread.Join();
                
            }

            //二十小时自动重新启动服务程序
            //System.Timers.Timer reBoot = new System.Timers.Timer();
            //reBoot.Interval = 24 * 60 * 60 * 1000;
            //reBoot.Elapsed += reBootManager.reBoot;
            //reBoot.AutoReset = true;
            //reBoot.Enabled = true;

            //每隔一个小时爬去一次数据
            //System.Timers.Timer t = new System.Timers.Timer();
            //t.Interval = 1 * 60 * 60 * 1000;
            //t.Elapsed += radarManager.getRadar;
            //t.AutoReset = true;
            //t.Enabled = true;
            
        }
        protected override void OnStop()
        {
            
        }
    }

    public class reBootManager
    {
        public static void reBoot(Object source, System.Timers.ElapsedEventArgs e)
        {
            System.Diagnostics.Process.Start("shutdown", "/r/f/t 0");
        }
    }

    public class radarManager
    {
        
        public static void getRadar(Object source,System.Timers.ElapsedEventArgs e)
        {
            

            Publish publish = new Publish();
            string tempDir = System.Configuration.ConfigurationManager.AppSettings["TempDPath"];
            string radarSend = System.Configuration.ConfigurationManager.AppSettings["radarSend"];
            
            WebClient wc = new WebClient();
            DateTime dateTime = DateTime.Now;
            string year = dateTime.Year.ToString();
            string month = addZero(dateTime.Month);
            string day = "";
            int hourInt = dateTime.Hour;
            string hour = "";
            if(hourInt < 9)
            {
                hour = addZero(hourInt+15);
                day = addZero(dateTime.Day-1);
            }
            else
            {
                day = addZero(dateTime.Day);
                hour = addZero(hourInt-9);
            }
            string minute = addZero(dateTime.Minute/6*6);
            string radarURL = "http://10.56.5.119/LOCAL/rad/nmcpt/Z_RADA_C_BABJ_"+year+month+day+hour+minute+"00_P_DOR_RDCP_CR_ANCN.PNG";
            //string radarURL = "http://115.28.64.105:8008/images/wx.jpg";
            //using (StreamWriter file = new StreamWriter("c:\\qixiangtest.txt", true))
            //{
            //    file.WriteLine("调用了1  {0}",radarURL);
            //}
            string downloadFileRoute = tempDir + "\\radar.PNG";
            //从内网下载
            wc.DownloadFile(radarURL, downloadFileRoute);
            //上传到服务器
            postRadarImage(radarSend, "&classType=雷达拼图&content="+downloadFileRoute, downloadFileRoute);
            wc.Dispose();

            //string base64Img = publish.ImageToBase64(tempDir + "\\radar.PNG");
           // string encodeStr = HttpUtility.UrlEncode(base64Img);
            //postRadar(encodeStr, radarSend);
        }

        public static void postRadarImage(string URL, string postStr, string filePath)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            string boundary = "boundary";
            request.ContentType = "multipart/form-data;boundary="+boundary;
            request.Method = "POST";
            //构造发送数据
            StringBuilder str = new StringBuilder();
            //发送除文件之外的数据
            foreach (string split in postStr.Split('&'))
            {
                string[] item = split.Split('=');
                if (item.Length != 2)
                {
                    break;
                }
                string name = item[0];
                string value = item[1];
                str.Append("--" + boundary);
                str.Append("\r\n");
                str.Append("Content-Dispositon:form-data;name="+name);
                str.Append("\r\n\r\n");
                str.Append(value);
                str.Append("\r\n");
            }
            //文件数据
            str.Append("--"+boundary);
            str.Append("\r\n");
            str.Append("Content-Disposition:form-data;name=\"radarImage\";filename="+filePath+"");
            str.Append("\r\n");
            str.Append("Content-Type:image/png");
            str.Append("\r\n\r\n");

            //Header
            string postHeader = str.ToString();
            byte[] postHeaderBytes = Encoding.UTF8.GetBytes(postHeader);

            //构造尾部数据
            byte[] boundaryBytes = Encoding.ASCII.GetBytes("\r\n--"+boundary+"--\r\n");

            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            long requestLength = postHeaderBytes.Length + fileStream.Length + boundaryBytes.Length;
            request.ContentLength = requestLength;

            Stream requestStream = request.GetRequestStream();
            //输入头部
            requestStream.Write(postHeaderBytes, 0, postHeaderBytes.Length);
            //输入文件流数据
            byte[] buffer = new Byte[checked((uint)Math.Min(4096, (int)fileStream.Length))];
            int bytesRead = 0;
            while((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                requestStream.Write(buffer, 0, bytesRead);
            }
            //输入尾部数据
            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length);

            fileStream.Close();
            requestStream.Close();

            WebResponse response = request.GetResponse();
            Stream resStream = response.GetResponseStream();
            StreamReader resStrReader = new StreamReader(resStream);

            resStrReader.Close();
            resStream.Close();
            response.Close();

        }



        public static void postRadar(string base64Img,string URL)
        {
            
            byte[] BData = Encoding.UTF8.GetBytes("radar="+base64Img);

            HttpWebRequest request = WebRequest.Create(URL) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.KeepAlive = true;
            request.ContentLength = BData.Length;

            Stream dataStream = request.GetRequestStream();

            dataStream.Write(BData, 0, BData.Length);
            dataStream.Close();
        }

        public static string addZero(int source)
        {
            if(source < 10)
            {
                return "0" + source.ToString();
            }else
            {
                return source.ToString();
            }
        }
    }
}
