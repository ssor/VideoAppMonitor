using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoAppMonitor
{

    public class ErrorManager
    {
        public const string RunningWell = "yes";
        public const string StillLive = "live?";
        public const string FirstLevelError = "all_closed_error";
        public const string clientReboot = "client_reboot";
        public const string ClearError = "clear_error";//优先处理


        public static string CurrentState = RunningWell;

        public static void AddError(string error)
        {
            CurrentState = error;
            Console.WriteLine("New State => " + CurrentState);
        }
        public static string GetCurrentState(string request)
        {
            if (request == ClearError)
            {
                CurrentState = RunningWell;
                Console.WriteLine("New State => " + CurrentState);
                return CurrentState;
            }

            if (RunningWell != CurrentState)
            {
                return CurrentState;
            }

            return RunningWell;
        }
    }
}
