using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Tanks
{
	public partial class Form1 : Form
	{
		private List<Tank> tanks = new List<Tank>();
		private NetworkStream stream;
		private string id;
		private bool closed;
		public Form1()
		{
			InitializeComponent();
			Load += Form1_Load;
		}
		public static void ThreadExceptionHandler(object sender, ThreadExceptionEventArgs e)
		{
			MessageBox.Show(e.Exception.Message);
			Environment.Exit(1);
		}
		private async void Form1_Load(object sender, EventArgs e)
		{
			try
			{
				TcpClient client = new TcpClient();
				client.Connect(new IPEndPoint(IPAddress.Loopback, 8888));
				stream = client.GetStream();
				FormClosed += (s1, e1) => { closed = true; client.Close(); };

				while (true)
					using (MemoryStream ms = new MemoryStream())
					{
						byte[] data = new byte[4096];
						do
						{
							int bytes = await stream.ReadAsync(data, 0, data.Length);
							ms.Write(data, 0, bytes);
						} while (stream.DataAvailable);

						foreach (XElement element in XElement.Parse($"<r>{Encoding.Unicode.GetString(ms.ToArray())}</r>").Elements())
							switch (element.Name.ToString())
							{
								case "Id":
									id = element.Value;
									tanks.Add(new Tank(id, this, tanks, stream));
									break;
								case "Players":
									foreach (XElement playerId in element.Elements())
										tanks.Add(new Tank(playerId.Value, this, tanks));
									break;
								case "Player":
									switch (element.Attribute("Action").Value)
									{
										case "Join":
											tanks.Add(new Tank(element.Attribute("Id").Value, this, tanks));
											break;
										case "Leave":
											tanks.First(x => x.Id == element.Attribute("Id").Value).Dispose();
											break;
										case "Move":
											Tank tank = tanks.First(x => x.Id == element.Attribute("Id").Value);
											tank.Picture.Location = new Point(
												int.Parse(element.Attribute("X").Value),
												int.Parse(element.Attribute("Y").Value));
											tank.Direction = int.Parse(element.Attribute("Direction").Value);
											break;
										case "Shoot":
											tanks.First(x => x.Id == element.Attribute("Id").Value).Shoot();
											break;
									}
									break;
							}
					}
			}
			catch when (closed) { }
		}
	}
}
