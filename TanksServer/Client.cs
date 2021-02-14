using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace ChatServer
{
    class Client : IDisposable
    {
        public string Id { get; private set; }
        public string UserName { get; private set; }
        public NetworkStream Stream { get; private set; }

        private TcpClient tcpClient;
        private Server server;

        public Client(TcpClient tcpClient, Server server)
        {
            this.tcpClient = tcpClient;
            this.server = server;
        }
        public void Process()
        {
            Stream = tcpClient.GetStream();
            Id  = Guid.NewGuid().ToString();
            
            Byte[] data = Encoding.Unicode.GetBytes(new XElement("Id",a));

            Stream.Write(data, 0,data.Length);
            server.Broadcast(new XElement("Player", new XAttribute("Action", "Join"), new XAttribute("Id", Id )).ToString());
            while (true)
                try
                {
                    string message = GetMessage();                      
                    server.Broadcast(message, UserName);  
                    
                
                }
                catch
                {
                    server.Clients.Remove(this);
                    server.Broadcast(new XElement("Player", new XAttribute("Action", "Leave"), new XAttribute("Id", Id)).ToString());

                    Dispose();
                    break;
                }
        }
        private string GetMessage()
        {
            byte[] data = new byte[4096];
            using (MemoryStream ms = new MemoryStream())
            {
                do
                {
                    int bytes = Stream.Read(data, 0, data.Length);
                    ms.Write(data, 0, bytes);
                } while (Stream.DataAvailable);
                data = ms.ToArray();
            }

            if (data.Length == 0)
                throw new EndOfStreamException();
            return Encoding.Unicode.GetString(data, 0, data.Length);
        }
        public void Dispose()
        {
            Stream?.Close();
            tcpClient.Close();
        }
    }
}