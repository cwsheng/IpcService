using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

namespace IpcServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //匿名管道
            //AonymousPipeServer.Run();
            //命名管道
            NamedPipeServer.Run();
        }
    }
}
