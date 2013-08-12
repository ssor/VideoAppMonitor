using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Server;

namespace VideoAppMonitor
{
    class Program
    {
        static List<ExternalExe> process_list = new List<ExternalExe>();
        static void Main(string[] args)
        {

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
            System.Timers.Timer timer = new System.Timers.Timer(5000);
            timer.Elapsed += (sender, e) =>
            {
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

            };
            timer.Enabled = true;
            //Process[] processes = Process.GetProcessesByName("视频图片监控");
        }
        static void kill_all_process
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
}
