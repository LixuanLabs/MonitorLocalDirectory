using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
namespace qixiangService
{
    class Watch
    {
        private string dirType = "";
        private string dirPath = "";
        private string filter = "";
        public void setProp(string type, string path, string filter)
        {
            this.dirType = type;
            this.dirPath = path;
            this.filter = filter;
        }
        public void WatchStart()
        {
            
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = dirPath;
            watcher.Filter = filter;
            watcher.Changed += new FileSystemEventHandler(OnProcess);
            watcher.Created += new FileSystemEventHandler(OnProcess);
            watcher.Deleted += new FileSystemEventHandler(OnProcess);

            watcher.EnableRaisingEvents = true;
            watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.CreationTime | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size;
            watcher.IncludeSubdirectories = true;

            

            Console.WriteLine("开始监控了！！");


        }

        public void OnProcess(object source, FileSystemEventArgs e)
        {
            if (e.ChangeType == WatcherChangeTypes.Created && Regex.IsMatch(e.Name, ".+.(doc|docx|txt)"))
            {
                OnCreated(source, e);
            }
            else if (e.ChangeType == WatcherChangeTypes.Changed && Regex.IsMatch(e.Name, ".+.(doc|docx|txt)"))
            {
                OnChanged(source, e);
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted && Regex.IsMatch(e.Name, ".+.(doc|docx|txt)"))
            {
                OnDeleted(source, e);
            }
        }

        public void OnCreated(object source, FileSystemEventArgs e)
        {
            //调用发送事件
            string aimURL = System.Configuration.ConfigurationManager.AppSettings["aimURL"];
            string watchDir = dirPath;
            
            //DirectoryInfo folder = new DirectoryInfo(watchDir);
            
            Publish publish = new Publish();

            publish.posthData(aimURL, e.FullPath, dirType);
            
        }

        public void OnChanged(object source, FileSystemEventArgs e)
        {
            
        }

        public void OnDeleted(object source, FileSystemEventArgs e)
        {
            
        }
    }
}
