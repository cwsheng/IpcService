using IpcServiceLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Threading.Tasks;

namespace IpcServiceFxServer
{
    class Program
    {
        static void Main(string[] args)
        {

            // 创建服务器通道
            IpcChannel serverChannel = new IpcChannel("localhost:9090");

            // 注册服务器通道
            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(serverChannel, false);

            // 显示通道名称
            Console.WriteLine("通道名称：{0}.", serverChannel.ChannelName);

            // 显示通道优先级
            Console.WriteLine("通道优先级：{0}.", serverChannel.ChannelPriority);

            // 显示通道关联的URL
            System.Runtime.Remoting.Channels.ChannelDataStore channelData = (System.Runtime.Remoting.Channels.ChannelDataStore)serverChannel.ChannelData;
            foreach (string uri in channelData.ChannelUris)
            {
                Console.WriteLine("通道URL：{0}.", uri);
            }

            //注册RemoteObject类型为已知类型
            System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownServiceType(typeof(RemoteObject), "RemoteObject.rem", System.Runtime.Remoting.WellKnownObjectMode.SingleCall);

            // Parse the channel's URI.
            string[] urls = serverChannel.GetUrlsForUri("RemoteObject.rem");
            if (urls.Length > 0)
            {
                string objectUrl = urls[0];
                string channelUri = serverChannel.Parse(objectUrl, out string objectUri);
                Console.WriteLine("The object URI is {0}.", objectUri);
                Console.WriteLine("The channel URI is {0}.", channelUri);
                Console.WriteLine("The object URL is {0}.", objectUrl);
            }

            // Wait for the user prompt.
            Console.WriteLine("Press ENTER to exit the server.");
            Console.ReadLine();
            Console.WriteLine("The server is exiting.");

        }
    }
}
