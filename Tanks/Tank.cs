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
		public int Speed { get; set; } = 4;
		public PictureBox Picture { get; set; }
		private Bullet bullet;
		private Form form;
		private List<Tank> tanks;
		private NetworkStream stream;
		private Timer moveTimer, killTimer;
		private bool spawned;
		private int direction, nKillFlashes = 20;
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
				moveTimer = new Timer() { Interval = 25 };
				moveTimer.Tick += Move_Tick;
				form.KeyDown += Form_KeyDown;
				form.KeyUp += Form_KeyUp; ;
			}

			killTimer = new Timer() { Interval = 200 };
			killTimer.Tick += (s, e) => {
				if (nKillFlashes-- == 0)
				{
					(s as Timer).Stop();
					Picture.Show();
					nKillFlashes = 20;
					Speed = 4;
				}
				else Picture.Visible = !Picture.Visible;
			};
		}
		private void Move_Tick(object sender, EventArgs e)
		{
			Point point = Point.Empty;
			switch (Direction)
			{
				case 0: point = new Point(Picture.Location.X, Picture.Location.Y - Speed); break;
				case 1: point = new Point(Picture.Location.X + Speed, Picture.Location.Y); break;
				case 2: point = new Point(Picture.Location.X, Picture.Location.Y + Speed); break;
				case 3: point = new Point(Picture.Location.X - Speed, Picture.Location.Y); break;
			}

			bool found = false; // intersect
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

			byte[] data = Encoding.Unicode.GetBytes(new XElement("Player",
				new XAttribute("Id", Id),
				new XAttribute("Action", "Move"),
				new XAttribute("X", point.X),
				new XAttribute("Y", point.Y),
				new XAttribute("Direction", Direction)).ToString());
			stream.Write(data, 0, data.Length);
		}
		private void Form_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Up: Direction = 0; break;
				case Keys.Right: Direction = 1; break;
				case Keys.Down: Direction = 2; break;
				case Keys.Left: Direction = 3; break;
				case Keys.Space:
					byte[] data = Encoding.Unicode.GetBytes(new XElement("Player", new XAttribute("Id", Id), new XAttribute("Action", "Shoot")).ToString());
					stream.Write(data, 0, data.Length);
					Shoot();
					return;
				default: return;
			}
			moveTimer.Start();
		}
		private void Form_KeyUp(object sender, KeyEventArgs e)
		{
			if (moveTimer.Enabled && (
				e.KeyCode == Keys.Up && Direction == 0 ||
				e.KeyCode == Keys.Right && Direction == 1 ||
				e.KeyCode == Keys.Down && Direction == 2 ||
				e.KeyCode == Keys.Left && Direction == 3))
				moveTimer.Stop();
		}
		public void Shoot()
		{
			if (bullet == null || bullet.IsDisposed)
				bullet = new Bullet(form, this, tanks);
		}
		public void Kill()
		{
			if (!killTimer.Enabled)
			{
				Speed = 2;
				killTimer.Start();
			}
			else nKillFlashes = 20;
		}
		public void Dispose()
		{
			if (stream != null)
			{
				moveTimer.Stop();
				form.KeyDown -= Form_KeyDown;
			}
			form.Controls.Remove(Picture);
		}
	}
}
