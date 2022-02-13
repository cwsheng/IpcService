using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading;

namespace IpcClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //匿名管道
            //AonymousPipeClient.Run(args);
            //命名管道
            NamedPipeClient.Run(args);
        }
    }
}
