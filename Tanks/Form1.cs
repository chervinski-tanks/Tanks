using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tanks.Properties;

namespace Tanks
{
	public partial class Form1 : Form
	{
        Dictionary<string, Tank> tanks = new Dictionary<string, Tank>();
        TcpClient client { get; set; }
        NetworkStream Stream { get; set; }
		public Form1()
		{
			InitializeComponent();
            //TcpClient client = new TcpClient(new IPEndPoint(IPAddress.Loopback, 8888));
			tanks.Add("", new Tank(this));
		}
	}
    class Tank
    {
        private int direction;

        public NetworkStream Stream { get; set; }
        public PictureBox Picture { get; set; }
        public Form1 Form { get; set; }
        public int Speed { get; set; }
        public int Direction {
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
        public Tank(Form1 form)
        {
            Speed = 7;
            Direction = 0;
            Form = form;
            Picture = new PictureBox()
            {
                Image = Properties.Resources.tank,
                SizeMode = PictureBoxSizeMode.Zoom,
                Size = new Size(50, 50),
                BackColor = Color.Transparent
            };
            form.Controls.Add(Picture);
            form.KeyDown += Form_KeyDown;
        }
        private void Form_KeyDown(object sender, KeyEventArgs e)
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
            
            Picture.Location = point;
        }
    }
}
