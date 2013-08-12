using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Server;

namespace VideoAppMonitor
{
    class Program
    {
        static List<ExternalExe> process_list = new List<ExternalExe>();
        static string environment_monitored_fold1 = @"C:\davs";
        static int fold1_max_file_count = 50;
        static string environment_monitored_fold2 = @"C:\pics";
        static int fold2_max_file_count = 50;



        static void Main(string[] args)
        {
            importData();

            process_list.Add(new ExternalExe("视频图片监控", @"./videoApp/Bmp2Png/视频图片监控.exe"));
            process_list.Add(new ExternalExe("视频DAV文件监控", @"./videoApp/DavReporter/视频DAV文件监控.exe"));

            process_list.Add(new ExternalExe("Player10001", @"./videoApp/Player/Player10001.exe"));
            //process_list.Add(new ExternalExe("Player10002", @"./videoApp/Player/Player10002.exe"));
            //process_list.Add(new ExternalExe("Player10003", @"./videoApp/Player/Player10003.exe"));

            process_list.Add(new ExternalExe("Client10001", @"./videoApp/Client/Client10001.exe", ErrorManager.clientReboot));
            //process_list.Add(new ExternalExe("Client10002", @"./videoApp/Client/Client10002.exe"));
            //process_list.Add(new ExternalExe("Client10003", @"./videoApp/Client/Client10003.exe"));


            foreach (ExternalExe exe in process_list)
            {
                exe.start_process();
            }



            start_process_monitor();


            UDPServer.startUDPListening();
        LabelRead: string line = Console.ReadLine();
            string echo = ErrorManager.GetCurrentState(line);
            Console.WriteLine("CurrentState => " + echo);
            goto LabelRead;
            //Thread thread = new Thread(new ThreadStart(start_exe));
            //thread.Start();
            //start_exe();
        }
        static void start_process_monitor()
        {
            System.Timers.Timer timer = new System.Timers.Timer(15000);
            timer.Elapsed += (sender, e) =>
            {
                if (environmentOk())
                {
                    //检测是否有应用异常退出，如果有则将其启动
                    foreach (ExternalExe exe in process_list)
                    {
                        Process[] finded_processes = Process.GetProcessesByName(exe.name);
                        if (finded_processes.Length > 0)
                        {
                        }
                        else
                        {
                            Console.WriteLine(exe.name + " 已经退出，即将重启...");
                            exe.start_process();//多次启动无效咋办？
                        }
                    }
                }
                else
                {
                    kill_all_process();
                    ErrorManager.AddError(ErrorManager.FirstLevelError);
                }
            };
            timer.Enabled = true;
            //Process[] processes = Process.GetProcessesByName("视频图片监控");
        }
        static bool environmentOk()
        {
            Func<string, int, bool> checkFileCount =
                 (path, count) =>
                 {
                     DirectoryInfo TheFolder = new DirectoryInfo(path);

                     FileInfo[] all_files = TheFolder.GetFiles();
                     if (all_files.Length > count)
                     {
                         return false;
                     }
                     return true;
                 };

            if (checkFileCount(environment_monitored_fold1, fold1_max_file_count)
                && checkFileCount(environment_monitored_fold2, fold2_max_file_count))
            {
                return true;

            }
            else
                return false;
        }
        //关闭所有启动的程序
        static void kill_all_process()
        {
            foreach (ExternalExe exe in process_list)
            {
                exe.kill_process();
            }
        }
        static void start_exe()
        {
            string file_name = "./videoApp/Player/Player10001.exe";
            //string file_name = "./videoApp/Bmp2Png/视频图片监控.exe";
            ProcessStartInfo info = new ProcessStartInfo(file_name);
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.CreateNoWindow = true;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
            Process pro = Process.Start(info);

            //pro.WaitForExit();
            //Process.Start(file_name);

            using (StreamReader reader = pro.StandardOutput)
            {
                string line = reader.ReadLine();
                while (line != null)
                {
                    Console.WriteLine(line);
                    line = reader.ReadLine();
                }
            }
        }
        static void importData()
        {
            string strReadFilePath1 = @"./config/config_monitor.txt";
            StreamReader srReadFile1 = new StreamReader(strReadFilePath1);
            string strConfig = srReadFile1.ReadToEnd();
            srReadFile1.Close();
            // eg. {"src_file_path":"C:\\Users\\ssor\\Desktop\\pics","dest_file_path":"C:\\Users\\ssor\\Desktop\\picpng","max_file_count":5}
            Debug.WriteLine(strConfig);
            Config cfg = (Config)JsonConvert.DeserializeObject<Config>(strConfig);
            if (cfg != null)
            {
                environment_monitored_fold1 = cfg.src_file_path1;
                environment_monitored_fold2 = cfg.src_file_path2;
                fold1_max_file_count = cfg.max_file_count1;
                fold2_max_file_count = cfg.max_file_count2;
            }
        }
    }
    class ExternalExe
    {
        public string name;
        string full_path;
        Process process;
        ProcessStartInfo info;
        public string error_log = null;//当启动时报告的信息

        public ExternalExe(string _name, string _full_path)
        {
            this.name = _name;
            this.full_path = _full_path;

            info = new ProcessStartInfo(_full_path);
            info.WindowStyle = ProcessWindowStyle.Normal;
            info.CreateNoWindow = false;
            info.RedirectStandardOutput = true;
            info.UseShellExecute = false;
        }
        public ExternalExe(string _name, string _full_path, string _error_log)
            : this(_name, _full_path)
        {
            this.error_log = _error_log;
        }
        public void start_process()
        {
            Thread thread = new Thread(new ThreadStart(() =>
            {
                try
                {
                    process = Process.Start(info);

                    Console.WriteLine(this.name + " 已经启动...");
                    if (error_log != null)
                    {
                        ErrorManager.AddError(this.error_log);
                    }
                    using (StreamReader reader = process.StandardOutput)
                    {
                        string line = reader.ReadLine();
                        while (line != null)
                        {
                            Console.WriteLine(line);
                            line = reader.ReadLine();
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }));
            thread.Start();
        }
        public void kill_process()
        {
            if (null != process)
            {
                process.Kill();
                process = null;
            }
        }
    }
    public class Config
    {
        public string src_file_path1;
        public int max_file_count1;
        public string src_file_path2;
        public int max_file_count2;

        public Config(string _src_file_path1, int _max_file_count1, string _src_file_path2, int _max_file_count2)
        {
            this.src_file_path1 = _src_file_path1;
            this.max_file_count1 = _max_file_count1;

            this.src_file_path2 = _src_file_path2;
            this.max_file_count2 = _max_file_count2;
        }


    }
}
