using IpcServiceLib;
using System;
using System.Runtime.Remoting.Channels.Ipc;

namespace IpcServiceFxClient
{
    class Program
    {
        static void Main(string[] args)
        {
            // 创建通道
            IpcChannel channel = new IpcChannel();

            // 注册通道
            System.Runtime.Remoting.Channels.ChannelServices.RegisterChannel(channel, false);

            // 注册类型RemoteObject为指定URL的已知类型
            System.Runtime.Remoting.WellKnownClientTypeEntry remoteType = new System.Runtime.Remoting.WellKnownClientTypeEntry(typeof(RemoteObject), "ipc://localhost:9090/RemoteObject.rem");
            System.Runtime.Remoting.RemotingConfiguration.RegisterWellKnownClientType(remoteType);

            // 创建消息接收器
            string objectUri;
            System.Runtime.Remoting.Messaging.IMessageSink messageSink = channel.CreateMessageSink("ipc://localhost:9090/RemoteObject.rem", null, out objectUri);
            Console.WriteLine("The URI of the message sink is {0}.", objectUri);
            if (messageSink != null)
            {
                Console.WriteLine("The type of the message sink is {0}.", messageSink.GetType().ToString());
            }

            // Create an instance of the remote object.
            RemoteObject service = new RemoteObject();

            // Invoke a method on the remote object.
            Console.WriteLine("The client is invoking the remote object.");
            Console.WriteLine("The remote object has been called {0} times.", service.GetCount());
            Console.ReadLine();
        }
    }
}
