using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace IpcServer
{
    public class NamedPipeServer
    {
        private static int numThreads = 4;
        public static void Run()
        {
            int i;
            Thread[] servers = new Thread[numThreads];

            Console.WriteLine("\n*** 命名管道服务端示例 ***\n");
            Console.WriteLine("等待客户端连接...\n");
            for (i = 0; i < numThreads; i++)
            {
                servers[i] = new Thread(ServerThread);
                servers[i].Start();
            }
            Thread.Sleep(250);
            while (i > 0)
            {
                for (int j = 0; j < numThreads; j++)
                {
                    if (servers[j] != null)
                    {
                        if (servers[j].Join(250))
                        {
                            Console.WriteLine("Server thread[{0}] 结束.", servers[j].ManagedThreadId);
                            servers[j] = null;
                            i--;    // 减少线程数量
                        }
                    }
                }
            }
            Console.WriteLine("\n服务器线程已完成，正在退出.");
            Console.ReadLine();
        }

        private static void ServerThread(object data)
        {
            //管道名称、管道方向
            NamedPipeServerStream pipeServer = new NamedPipeServerStream("testpipe", PipeDirection.InOut, numThreads);
            int threadId = Thread.CurrentThread.ManagedThreadId;
            // 等待客户端连接
            pipeServer.WaitForConnection();
            Console.WriteLine("客户端连接成功 thread[{0}].", threadId);
            try
            {
                //读取客户的请求。客户端写入管道后，其安全令牌将可用
                StreamString ss = new StreamString(pipeServer);
                //使用客户端预期的字符串向连接的客户端验证我们的身份。
                ss.WriteString("I am the one true server!");
                string filename = ss.ReadString();
                //在模拟客户端时读入文件的内容。
                ReadFileToStream fileReader = new ReadFileToStream(ss, filename);
                Console.WriteLine("读取文件: {0} 线程[{1}] 管道用户: {2}.", filename, threadId, pipeServer.GetImpersonationUserName());
                pipeServer.RunAsClient(fileReader.Start);
            }
            catch (IOException e)
            {
                Console.WriteLine("异常: {0}", e.Message);
            }
            pipeServer.Close();
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
            int len = 0;

            len = ioStream.ReadByte() * 256;
            len += ioStream.ReadByte();
            byte[] inBuffer = new byte[len];
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

    // 包含在模拟用户的上下文中执行的方法
    public class ReadFileToStream
    {
        private string fn;
        private StreamString ss;
        public ReadFileToStream(StreamString str, string filename)
        {
            fn = filename;
            ss = str;
        }

        public void Start()
        {
            string contents = File.ReadAllText(fn);
            ss.WriteString(contents);
        }
    }
}
