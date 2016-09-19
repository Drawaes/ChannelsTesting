using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Channels;
using Channels.Networking.Libuv;
using Channels.Networking.Sockets;

namespace BytesBeingWrittenUnchanged
{
    public class Program
    {
        static uint data1 = 0xFFFFFFFF;
        static ulong data2 = 0xFFFFFFFFFFFFFFFF;
        static byte data3 = 0xFF;
        static ushort data4 = 0xFFFF;
        static byte empty = 0x0000;
        static SocketListener server;
        static IPEndPoint address = new IPEndPoint(IPAddress.Loopback, 7777);
        static int totalMessages = 50000;
        static SocketConnection client;
        static UvThread _thread = new UvThread();
        //static UvTcpListener server;

        public static void Main(string[] args)
        {
            server = new SocketListener();
            //server = new UvTcpListener(_thread,address);
            server.OnConnection(Server);
            //server.Start();
            server.Start(address);

            client = SocketConnection.ConnectAsync(address).Result;
            
            Task recieve = Task.Run(() => ClientQueue(client));

            Console.ReadLine();
        }

        static async void Server(IChannel channel)
        {
            for(int i = 0; i < totalMessages;i++)
            {
                var buff = channel.Output.Alloc(100);
                buff.Ensure(2);
                buff.Memory.Write(empty);
                buff.CommitBytes(2);

                buff.Ensure(8);
                buff.Memory.Write(data2);
                buff.CommitBytes(8);
                               
                await buff.FlushAsync();
            }
            var buff2 = channel.Output.Alloc(100);
            buff2.Ensure(2);
            buff2.Memory.Write(empty);
            buff2.CommitBytes(2);

            buff2.Ensure(8);
            buff2.Memory.Write(data2);
            buff2.CommitBytes(8);
            Console.ReadLine();
        }

        static async Task ClientQueue(IChannel channel)
        {
            var mcount = 0;

            while(true)
            {
                var buff = await channel.Input.ReadAsync();
                while(buff.Length >= 10)
                {
                    var b = buff.FirstSpan.Read<byte>();
                    //if(b != 0xFF)
                    //    throw new NotImplementedException();
                    buff = buff.Slice(10);
                    mcount += 10;
                }
                Console.WriteLine("Read " + mcount);
                buff.Consumed(buff.Start,buff.End);
                if(mcount == (10 * totalMessages))
                {
                    Console.WriteLine("Read all bytes!");
                }
                if(mcount > (10 * totalMessages))
                {
                    Console.WriteLine("read too many " + mcount);
                }
            }
            
        }
    }
}
