using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Tanks
{
	class Tank : IDisposable
	{
		public string Id { get; set; }
		public int Speed { get; set; }
		public PictureBox Picture { get; set; }
		private Form form;
		private List<Tank> tanks;
		private NetworkStream stream;
		private bool spawned;
		private int direction;
		public int Direction
		{
			get => direction;
			set
			{
				int nRotates = 0;
				while (direction != value)
				{
					if (++direction == 4)
						direction = 0;
					++nRotates;
				}
				Picture?.Image.RotateFlip((RotateFlipType)nRotates);
			}
		}
		public Tank(string id, Form form, List<Tank> tanks, NetworkStream stream = null)
		{
			Speed = 7;
			Id = id;
			this.form = form;
			this.tanks = tanks;
			form.Controls.Add(Picture = new PictureBox() {
				Image = Properties.Resources.Tank,
				SizeMode = PictureBoxSizeMode.Zoom,
				Size = new Size(50, 50),
				BackColor = Color.Transparent
			});
			if (stream != null)
			{
				this.stream = stream;
				form.KeyDown += Form_KeyDown;
			}
		}
		public void Form_KeyDown(object sender, KeyEventArgs e)
		{
			Point point;
			switch (e.KeyCode)
			{
				case Keys.Up:
					Direction = 0;
					point = new Point(Picture.Location.X, Picture.Location.Y - Speed);
					break;
				case Keys.Right:
					Direction = 1;
					point = new Point(Picture.Location.X + Speed, Picture.Location.Y);
					break;
				case Keys.Down:
					Direction = 2;
					point = new Point(Picture.Location.X, Picture.Location.Y + Speed);
					break;
				case Keys.Left:
					Direction = 3;
					point = new Point(Picture.Location.X - Speed, Picture.Location.Y);
					break;
				default: return;
			}

			bool found = false;
			foreach (Tank tank in tanks)
				if (point.X <= tank.Picture.Location.X + tank.Picture.Width && point.X + Picture.Width >= tank.Picture.Location.X &&
					point.Y <= tank.Picture.Location.Y + tank.Picture.Height && point.Y + Picture.Height >= tank.Picture.Location.Y && tank != this)
				{
					found = true;
					if (spawned)
						switch (Direction)
						{
							case 0: point = new Point(point.X, tank.Picture.Location.Y + tank.Picture.Height + 1); break;
							case 1: point = new Point(tank.Picture.Location.X - Picture.Width - 1, point.Y); break;
							case 2: point = new Point(point.X, tank.Picture.Location.Y - Picture.Height - 1); break;
							case 3: point = new Point(tank.Picture.Location.X + tank.Picture.Width + 1, point.Y); break;
						}
				}
			if (!found)
			{
				spawned = true;

				if (point.X + Picture.Width > form.ClientSize.Width)
					point = new Point(form.ClientSize.Width - Picture.Width, point.Y);
				if (point.X < 0)
					point = new Point(0, point.Y);
				if (point.Y + Picture.Height > form.ClientSize.Height)
					point = new Point(point.X, form.ClientSize.Height - Picture.Height);
				if (point.Y < 0)
					point = new Point(point.X, 0);
			}
			Picture.Location = point;

			if (stream != null)
			{
				byte[] data = Encoding.Unicode.GetBytes(new XElement("Player",
					new XAttribute("Id", Id),
					new XAttribute("Action", "Move"),
					new XAttribute("X", point.X),
					new XAttribute("Y", point.Y),
					new XAttribute("Direction", Direction)).ToString());
				stream.Write(data, 0, data.Length);
			}
		}
		public void Dispose()
		{
			form.Controls.Remove(Picture);
			if (stream != null)
				form.KeyDown -= Form_KeyDown;
		}
	}
}
