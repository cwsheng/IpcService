using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;

namespace IpcServer
{
    class AonymousPipeServer
    {
        public static void Run()
        {
            //客户端进程
            Process pipeClient = new Process();
            pipeClient.StartInfo.FileName = "IpcClient.exe";
            //创建输出匿名管道、指定句柄由子进程继承
            using (AnonymousPipeServerStream pipeServer = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
            {
                Console.WriteLine("[SERVER] 管道传输模式: {0}.", pipeServer.TransmissionMode);
                //将客户端进程的句柄传递给服务器。
                pipeClient.StartInfo.Arguments = pipeServer.GetClientHandleAsString();
                pipeClient.StartInfo.UseShellExecute = false;
                pipeClient.Start();
                pipeServer.DisposeLocalCopyOfClientHandle();
                try
                {
                    // 读取服务端输入内容，发送到客户端进程
                    using (StreamWriter sw = new StreamWriter(pipeServer))
                    {
                        sw.AutoFlush = true;
                        // 发送“同步消息”并等待客户端接收。
                        sw.WriteLine("SYNC");
                        //等待客户端读取所有内容
                        pipeServer.WaitForPipeDrain();
                        // 发送控制台数据到子进程
                        Console.Write("[SERVER] 输入文本: ");
                        sw.WriteLine(Console.ReadLine());
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("[SERVER] 异常: {0}", e.Message);
                }
            }
            pipeClient.WaitForExit();
            pipeClient.Close();
            Console.WriteLine("[SERVER] 客户端退出，服务端终止.");
        }
    }
}
