using System;
using System.IO;
using System.IO.Pipes;

namespace IpcClient
{
    class AonymousPipeClient
    {
        public static void Run(string[] args)
        {
            if (args.Length > 0)
            {
                //创建输入类型匿名管道
                using (PipeStream pipeClient = new AnonymousPipeClientStream(PipeDirection.In, args[0]))
                {
                    Console.WriteLine("[CLIENT] 当前管道传输模式: {0}.", pipeClient.TransmissionMode);

                    //创建读取流，从管道中读取
                    using (StreamReader sr = new StreamReader(pipeClient))
                    {
                        string temp;
                        // 等待来着服务器的消息
                        do
                        {
                            Console.WriteLine("[CLIENT] 同步等待...");
                            temp = sr.ReadLine();
                        }
                        while (!temp.StartsWith("SYNC"));

                        // Read the server data and echo to the console.
                        while ((temp = sr.ReadLine()) != null)
                        {
                            Console.WriteLine("[CLIENT] 响应: " + temp);
                        }
                    }
                }
            }
            Console.Write("[CLIENT] 任意键退出...");
            Console.ReadLine();
        }
    }
}
