using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace IpcClient
{
    class NamedPipeClient
    {
        private static int numClients = 4;

        public static void Run(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "spawnclient")
                {
                    //.代表本机(可替换为具体IP)
                    var pipeClient = new NamedPipeClientStream(".", "testpipe", PipeDirection.InOut, PipeOptions.None, TokenImpersonationLevel.Impersonation);
                    Console.WriteLine("连接服务端...\n");
                    pipeClient.Connect();
                    var ss = new StreamString(pipeClient);
                    // Validate the server's signature string.
                    if (ss.ReadString() == "I am the one true server!")
                    {
                        //发送文件路径，服务端读取后返回
                        ss.WriteString(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "namedpipeTxt.txt"));

                        //输出文件内容
                        Console.Write($"文件内容：{ss.ReadString()}\r\n");
                    }
                    else
                    {
                        Console.WriteLine("服务端未验证通过");
                    }
                    pipeClient.Close();
                    Console.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("\n*** 命名管道客户端示例 ***\n");
                StartClients();
            }
        }

        // 启动客户端
        private static void StartClients()
        {
            string currentProcessName = Environment.CommandLine;

            // Remove extra characters when launched from Visual Studio
            currentProcessName = currentProcessName.Trim('"', ' ');

            currentProcessName = Path.ChangeExtension(currentProcessName, ".exe");
            Process[] plist = new Process[numClients];

            Console.WriteLine("生成客户端进程...\n");

            if (currentProcessName.Contains(Environment.CurrentDirectory))
            {
                currentProcessName = currentProcessName.Replace(Environment.CurrentDirectory, String.Empty);
            }

            //兼容处理
            currentProcessName = currentProcessName.Replace("\\", String.Empty);
            currentProcessName = currentProcessName.Replace("\"", String.Empty);

            int i;
            for (i = 0; i < numClients; i++)
            {
                //启动客户端进程，使用同一个命名管道
                plist[i] = Process.Start(currentProcessName, "spawnclient");
            }
            while (i > 0)
            {
                for (int j = 0; j < numClients; j++)
                {
                    if (plist[j] != null)
                    {
                        if (plist[j].HasExited)
                        {
                            Console.WriteLine($"客户端进程[{plist[j].Id}]已经退出.");
                            plist[j] = null;
                            i--;
                        }
                        else
                        {
                            Thread.Sleep(250);
                        }
                    }
                }
            }
            Console.WriteLine("\n客户端进程完成，退出中");
        }
    }

    //定义用于在流上读取和写入字符串的数据协议
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            int len;
            len = ioStream.ReadByte();
            len = len * 256;
            len += ioStream.ReadByte();
            var inBuffer = new byte[len];
            ioStream.Read(inBuffer, 0, len);

            return streamEncoding.GetString(inBuffer);
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            ioStream.WriteByte((byte)(len / 256));
            ioStream.WriteByte((byte)(len & 255));
            ioStream.Write(outBuffer, 0, len);
            ioStream.Flush();

            return outBuffer.Length + 2;
        }
    }
}
