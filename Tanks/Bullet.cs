using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace Tanks
{
	class Bullet : IDisposable
	{
		private int direction;
		public int Speed { get; set; }
		public Form Form { get; set; }
		public PictureBox Picture { get; set; }
		public List<Tank> Tanks { get; set; }
		public Bullet(Point location, int direction, Form form, List<Tank> tanks)
		{
			Speed = 12;
			Form = form;
			Tanks = tanks;
			form.Controls.Add(Picture = new PictureBox() {
				Image = Properties.Resources.Bullet,
				SizeMode = PictureBoxSizeMode.Zoom,
				Location = location,
				Size = new Size(20, 10),
				BackColor = Color.Transparent
			});
			Timer timer = new Timer() { Interval = 1 };
			timer.Tick += Timer_Tick;
			timer.Start();
		}
		private void Timer_Tick(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
		public void Dispose()
		{
			Form.Controls.Remove(Picture);
		}
	}
}
