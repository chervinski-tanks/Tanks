using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Tanks.Properties;

namespace Tanks
{
	public partial class Form1 : Form
	{
		List<Tank> tanks = new List<Tank>();
		TcpClient client;
		NetworkStream stream;
		string id;
		public Form1()
		{
			InitializeComponent();
			client = new TcpClient();
			client.Connect(new IPEndPoint(IPAddress.Loopback, 8888));
			stream = client.GetStream();
			Load += Form1_Load;
		}

		private async void Form1_Load(object sender, EventArgs e)
		{
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
					{
						if (element.Name == "Id")
						{
							id = element.Value;
							tanks.Add(new Tank(id, this, tanks, stream));
						}
						else if (element.Name == "Players")
						{
							foreach (var item in element.Elements())
								tanks.Add(new Tank(element.Value, this, tanks));
						}
						else if (element.Name == "Player")
						{
							string action = element.Attribute("Action").Value;
							switch (action)
                            {
								case "Join":
									tanks.Add(new Tank(element.Attribute("Id").Value, this, tanks));
									break;
								case "Leave":
									tanks.First(x => x.Id == element.Attribute("Id").Value).Dispose();
									break;
								case "Up": case "Right": case "Down": case "Left":
										tanks.First(x => x.Id == element.Attribute("Id").Value).Form_KeyDown(this, new KeyEventArgs(
											action == "Up" ? Keys.Up : (
											action == "Right" ? Keys.Right :(
											action == "Down" ? Keys.Down : Keys.Left))));
										break;
							}

                        }
					}
				}

		}
	}
	class Tank : IDisposable
	{
		private int direction;

		public string Id { get; set; }
		public NetworkStream Stream { get; set; }
		public PictureBox Picture { get; set; }
		public Form1 Form { get; set; }
		public int Speed { get; set; }
		public List<Tank> Tanks { get; set; }
		public int Direction
		{
			get => direction;
			set
			{
				if (direction != value)
					while (direction != value)
					{
						Picture?.Image.RotateFlip(RotateFlipType.Rotate90FlipNone);
						if (++direction == 4)
							direction = 0;
					}
			}
		}
		public Tank(string id, Form1 form, List<Tank> tanks, NetworkStream stream = null)
		{
			Speed = 7;
			Direction = 0;
			Id = id;
			Form = form;
			Tanks = tanks;
			Picture = new PictureBox()
			{
				Image = Properties.Resources.tank,
				SizeMode = PictureBoxSizeMode.Zoom,
				Size = new Size(50, 50),
				BackColor = Color.Transparent
			};
			form.Controls.Add(Picture);
			if (stream != null)
			{
				Stream = stream;
				form.KeyDown += Form_KeyDown;
			}
		}
		public void Form_KeyDown(object sender, KeyEventArgs e)
		{
			Point point;
			string action;
			switch (e.KeyCode)
			{
				case Keys.Up:
					Direction = 0;
					point = new Point(Picture.Location.X, Picture.Location.Y - Speed);
					action = "Up";
					break;
				case Keys.Right:
					Direction = 1;
					point = new Point(Picture.Location.X + Speed, Picture.Location.Y);
					action = "Right";
					break;
				case Keys.Down:
					Direction = 2;
					point = new Point(Picture.Location.X, Picture.Location.Y + Speed);
					action = "Down";
					break;
				case Keys.Left:
					Direction = 3;
					point = new Point(Picture.Location.X - Speed, Picture.Location.Y);
					action = "Left";
					break;
				default: return;
			}

			if (point.X + Picture.Width > Form.ClientSize.Width)
				point = new Point(Form.ClientSize.Width - Picture.Width, point.Y);
			if (point.X < 0)
				point = new Point(0, point.Y);
			if (point.Y + Picture.Height > Form.ClientSize.Height)
				point = new Point(point.X, Form.ClientSize.Height - Picture.Height);
			if (point.Y < 0)
				point = new Point(point.X, 0);
			Picture.Location = point;

			byte[] data = Encoding.Unicode.GetBytes(new XElement("Player", new XAttribute("Action", action), new XAttribute("Id", Id)).ToString());
			Stream.Write(data, 0, data.Length);
		}

        public void Dispose()
        {
			Form.Controls.Remove(Picture);
        }
    }
}
