
using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace ChatServer
{
	class Program
	{
		static void Main(string[] args)
		{
			try
			{
				using (Server server = new Server(8888))
					server.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}
		}
	}
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
    class Server : IDisposable
    {
        public List<Client> Clients { get; set; }
        private TcpListener listener;
        public Server(int port)
        {
            listener = new TcpListener(IPAddress.Any, port);
            Clients = new List<Client>();
        }
        public void Start()
        {
            listener.Start();
            Console.WriteLine("Server started.");

            while (true)
                new Thread(new ThreadStart(new Client(listener.AcceptTcpClient(), this).Process)).Start();
        }
        public void Broadcast(string message, string name)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            foreach (Client client in Clients)
                if (client.UserName != name)
                    client.Stream.Write(data, 0, data.Length);
        }
        
        public void Dispose()
        {
            listener.Stop();
            foreach (Client client in Clients)
                client.Dispose();
            Console.WriteLine("Server stoped.");
        }
    }
}