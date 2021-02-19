using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Tanks
{
	class Bullet : IDisposable
	{
		public int Speed { get; set; }
		public PictureBox Picture { get; set; }
		public bool IsDisposed { get; private set; }
		private Form form;
		private int direction;
		private List<Tank> tanks;
		private Timer moveTimer;
		public Bullet(Form form, Tank shooter, List<Tank> tanks)
		{
			Speed = 6;
			direction = shooter.Direction;
			this.form = form;
			this.tanks = tanks;

			form.Controls.Add(Picture = new PictureBox()
			{
				Image = Properties.Resources.Bullet,
				SizeMode = PictureBoxSizeMode.Zoom,
				Size = new Size(26, 26),
				BackColor = Color.Transparent
			});
			Point location = new Point(
				shooter.Picture.Location.X + shooter.Picture.Width / 2 - Picture.Width / 2,
				shooter.Picture.Location.Y + shooter.Picture.Height / 2 - Picture.Height / 2);
			switch (direction)
			{
				case 0: location = new Point(location.X, location.Y - shooter.Picture.Height / 2 - Picture.Height / 2); break;
				case 1: location = new Point(location.X + shooter.Picture.Width / 2 + Picture.Width / 2, location.Y); break;
				case 2: location = new Point(location.X, location.Y + shooter.Picture.Height / 2 + Picture.Height / 2); break;
				case 3: location = new Point(location.X - shooter.Picture.Width / 2 - Picture.Width / 2, location.Y); break;
			}
			Picture.Location = location;
			Picture.Image.RotateFlip((RotateFlipType)direction);

			moveTimer = new Timer() { Interval = 25 };
			moveTimer.Tick += Move_Tick;
			moveTimer.Start();
		}
		private void Move_Tick(object sender, EventArgs e)
		{
			Point point = Point.Empty;
			switch (direction)
			{
				case 0: point = new Point(Picture.Location.X, Picture.Location.Y - Speed); break;
				case 1: point = new Point(Picture.Location.X + Speed, Picture.Location.Y); break;
				case 2: point = new Point(Picture.Location.X, Picture.Location.Y + Speed); break;
				case 3: point = new Point(Picture.Location.X - Speed, Picture.Location.Y); break;
			}

			if (point.Y < 0 || point.Y + Picture.Height > form.ClientSize.Height ||
				point.X < 0 || point.X + Picture.Width > form.ClientSize.Width)
			{
				Dispose();
				return;
			}

			foreach (Tank tank in tanks)
				if (point.X <= tank.Picture.Location.X + tank.Picture.Width && point.X + Picture.Width >= tank.Picture.Location.X &&
					point.Y <= tank.Picture.Location.Y + tank.Picture.Height && point.Y + Picture.Height >= tank.Picture.Location.Y)
				{
					tank.Kill();
					Dispose();
					return;
				}
			Picture.Location = point;
		}
		public void Dispose()
		{
			IsDisposed = true;
			moveTimer.Stop();
			form.Controls.Remove(Picture);
		}
	}
}
