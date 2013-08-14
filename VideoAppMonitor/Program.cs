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
            importData(@"./config/config_monitor.txt");

            process_list.Add(new ExternalExe("视频图片监控", @"./videoApp/Bmp2Png/视频图片监控.exe"));
            process_list.Add(new ExternalExe("视频DAV文件监控", @"./videoApp/DavReporter/视频DAV文件监控.exe"));

            //process_list.Add(new ExternalExe("Player10001", @"./videoApp/Player/Player10001.exe"));
            //process_list.Add(new ExternalExe("Player10002", @"./videoApp/Player/Player10002.exe"));
            //process_list.Add(new ExternalExe("Player10003", @"./videoApp/Player/Player10003.exe"));

            process_list.Add(new ExternalExe("Client10001", @"./videoApp/Client/Client10001.exe", ErrorManager.clientReboot));
            //process_list.Add(new ExternalExe("Client10002", @"./videoApp/Client/Client10002.exe", ErrorManager.clientReboot));
            //process_list.Add(new ExternalExe("Client10003", @"./videoApp/Client/Client10003.exe", ErrorManager.clientReboot));

            Func<bool> func = () =>
            {
                List<Func<bool>> func_list = new List<Func<bool>> 
                { 
                     createEnvironmentCheckFun(environment_monitored_fold1, fold1_max_file_count),
                     createEnvironmentCheckFun(environment_monitored_fold2, fold2_max_file_count)
                };
                return checkEnvironment(func_list, true);
            };
            //启动应用，并同时启动监控timer
            start_process_monitor(15000, process_list, func)();

            UDPServer.startUDPListening(4999);

        LabelRead: string line = Console.ReadLine();
            string echo = ErrorManager.GetCurrentState(line);
            Console.WriteLine("CurrentState => " + echo);
            goto LabelRead;
        }
        static Action start_process_monitor(int interval, List<ExternalExe> _process_list, Func<bool> predictor)
        {
            System.Timers.Timer timer = new System.Timers.Timer(interval);
            Action dele = () =>
            {
                if ((predictor == null) || (predictor != null && predictor()))
                {
                    //检测是否有应用异常退出，如果有则将其启动
                    start_all_process(_process_list);
                }
                else
                {
                    kill_all_process(_process_list);
                    ErrorManager.AddError(ErrorManager.FirstLevelError);
                }
            };
            timer.Elapsed += (sender, e) =>
            {
                dele();
            };

            timer.Enabled = true;
            return dele;
        }

        static bool checkEnvironment(List<Func<bool>> predictor_list, bool pre_predictor_result)
        {
            if (pre_predictor_result == false) return false;
            if (predictor_list.Count <= 0) return pre_predictor_result;

            int count = predictor_list.Count;
            return checkEnvironment(predictor_list.GetRange(0, count - 1), predictor_list[count - 1]());
        }

        static Action createStartExeFunc(ExternalExe exe)
        {
            return () =>
            {
                Process[] finded_processes = Process.GetProcessesByName(exe.name);
                if (finded_processes.Length > 0)
                {
                }
                else
                {
                    Console.WriteLine(exe.name + " 已经退出，即将启动...");
                    exe.start_process();//多次启动无效咋办？
                }
            };
        }

        static Func<bool> createEnvironmentCheckFun(string path, int count)
        {
            Func<bool> checkFileCount =
                () =>
                {
                    DirectoryInfo TheFolder = new DirectoryInfo(path);

                    FileInfo[] all_files = TheFolder.GetFiles();
                    if (all_files.Length > count)
                    {
                        return false;
                    }
                    return true;
                };
            return checkFileCount;
        }

        static void start_all_process(List<ExternalExe> list)
        {
            foreach (ExternalExe exe in list)
            {
                createStartExeFunc(exe)();
            }
        }
        //关闭所有启动的程序
        static void kill_all_process(List<ExternalExe> list)
        {
            foreach (ExternalExe exe in list)
            {
                exe.kill_process();
            }
        }
        static void importData(string path)
        {
            StreamReader srReadFile1 = new StreamReader(path);
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

        static void createExternalExe(string _name, string _full_path)
        { 
            
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
