using System;
using System.Net.Sockets;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace ChatServer
{
	class Client : IDisposable
	{
		public NetworkStream Stream { get; private set; }

		private string id;
		private TcpClient tcpClient;
		private Server server;
		private Tank tank;

		public Client(TcpClient tcpClient, Server server)
		{
			this.tcpClient = tcpClient;
			this.server = server;
		}
		public void Process()
		{
			Stream = tcpClient.GetStream();
			tank = new Tank();

			// send his ID
			byte[] data = Encoding.Unicode.GetBytes(new XElement("Id", id = Guid.NewGuid().ToString()).ToString());
			Stream.Write(data, 0, data.Length);

			// and other palyers' ID
			XElement players = new XElement("Players");
			foreach (var client in server.Clients)
				players.Add(new XElement("Id", client.id));
			data = Encoding.Unicode.GetBytes(players.ToString());
			Stream.Write(data, 0, data.Length);

			// and move all players to their real locations
			foreach (var client in server.Clients)
			{
				data = Encoding.Unicode.GetBytes(new XElement("Player",
					new XAttribute("Id", client.id),
					new XAttribute("Action", "Move"),
					new XAttribute("X", client.tank.X),
					new XAttribute("Y", client.tank.Y),
					new XAttribute("Direction", client.tank.Direction)).ToString());
				Stream.Write(data, 0, data.Length);
			}

			server.Clients.Add(this);
			server.Broadcast(new XElement("Player", new XAttribute("Id", id), new XAttribute("Action", "Join")).ToString(), this);
			while (true)
				try
				{
					string message = GetMessage();
					XElement x = XElement.Parse(message);
					if (x.Attribute("Action").Value == "Move")
					{
						tank.Direction = int.Parse(x.Attribute("Direction").Value);
						tank.X = int.Parse(x.Attribute("X").Value);
						tank.Y = int.Parse(x.Attribute("Y").Value);
					}
					server.Broadcast(message, this);
				}
				catch
				{
					server.Clients.Remove(this);
					server.Broadcast(new XElement("Player", new XAttribute("Id", id), new XAttribute("Action", "Leave")).ToString(), this);

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
	class Tank
	{
		public int Direction { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
	}
}
