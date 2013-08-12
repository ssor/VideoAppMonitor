using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Threading;
using VideoAppMonitor;

namespace Server
{
    //public delegate void invokeCtlString(string str);

    public class UDPServer
    {
        public static ManualResetEvent Manualstate = new ManualResetEvent(true);
        public static StringBuilder sbuilder = new StringBuilder();
        public static Socket serverSocket;
        static byte[] byteData = new byte[1024];
        public static void startUDPListening()
        {
            try
            {
                //We are using UDP sockets
                serverSocket = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);
                IPAddress ip = IPAddress.Parse(GetLocalIP4());
                IPEndPoint ipEndPoint = new IPEndPoint(ip, 4999);
                //                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, port);

                //Bind this address to the server
                serverSocket.Bind(ipEndPoint);
                //防止客户端强行中断造成的异常
                long IOC_IN = 0x80000000;
                long IOC_VENDOR = 0x18000000;
                long SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;

                byte[] optionInValue = { Convert.ToByte(false) };
                byte[] optionOutValue = new byte[4];
                serverSocket.IOControl((int)SIO_UDP_CONNRESET, optionInValue, optionOutValue);

                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                //The epSender identifies the incoming clients
                EndPoint epSender = (EndPoint)ipeSender;

                //Start receiving data
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length,
                    SocketFlags.None, ref epSender, new AsyncCallback(OnReceive), epSender);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    string.Format("UDPServer.startUDPListening  -> error = {0}"
                    , ex.Message));
            }
        }
        public static void OnReceive(IAsyncResult ar)
        {
            try
            {
                IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint epSender = (EndPoint)ipeSender;

                serverSocket.EndReceiveFrom(ar, ref epSender);

                string strReceived = Encoding.UTF8.GetString(byteData);
                strReceived = strReceived.Substring(0, strReceived.IndexOf("\0"));

                string echo = ErrorManager.GetCurrentState(strReceived);
                if (echo != null)
                {
                    serverSocket.SendTo(Encoding.UTF8.GetBytes(echo), epSender);
                }

                Debug.WriteLine(
                    string.Format("UDPServer.OnReceive  -> received = {0}"
                    , strReceived));

                Array.Clear(byteData, 0, byteData.Length);
                //int i = strReceived.IndexOf("\0");
                //Manualstate.WaitOne();
                //Manualstate.Reset();
                ////todo here should deal with the received string
                //sbuilder.Append(strReceived.Substring(0, i));
                //Manualstate.Set();

                //Start listening to the message send by the user
                serverSocket.BeginReceiveFrom(byteData, 0, byteData.Length, SocketFlags.None, ref epSender,
                    new AsyncCallback(OnReceive), epSender);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    string.Format("UDPServer.OnReceive  -> error = {0}"
                    , ex.Message));
            }
        }
        static string GetLocalIP4()
        {
            IPAddress ipAddress = null;
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            for (int i = 0; i < ipHostInfo.AddressList.Length; i++)
            {
                ipAddress = ipHostInfo.AddressList[i];
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    break;
                }
                else
                {
                    ipAddress = null;
                }
            }
            if (null == ipAddress)
            {
                return null;
            }
            return ipAddress.ToString();
        }
    }
}
