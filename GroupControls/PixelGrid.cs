using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GroupControls
{
	public class PixelGrid : Control
	{
		Point lastClick = Point.Empty;

		public PixelGrid()
		{
			this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
			//this.SetStyle(ControlStyles.Opaque, false);
			this.BackColor = Color.Transparent;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			lastClick = this.PointToClient(e.Location);
			Refresh();
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint(e);
			//e.Graphics.Clear(this.BackColor);
			for (int x = 0; x < this.ClientRectangle.Width; x++)
			{
				if (x % 10 == 0)
					e.Graphics.DrawLine(SystemPens.ControlText, new Point(x, 0), new Point(x, this.ClientRectangle.Height));
				else if (x % 2 == 0)
					for (int y = 0; y < this.ClientRectangle.Height; y++)
						if (y % 10 == 5)
							e.Graphics.DrawPoint(SystemPens.ControlText, x, y);
			}
			for (int y = 0; y < this.ClientRectangle.Width; y++)
			{
				if (y % 10 == 0)
					e.Graphics.DrawLine(SystemPens.ControlText, new Point(0, y), new Point(this.ClientRectangle.Width, y));
				else if (y % 2 == 0)
					for (int x = 0; x < this.ClientRectangle.Height; x++)
						if (x % 10 == 5)
							e.Graphics.DrawPoint(SystemPens.ControlText, x, y);
			}
			e.Graphics.DrawPoint(Pens.Red, lastClick);
		}
	}

	internal static class GraphicsExt
	{
		public static void DrawPoint(this Graphics g, Pen pen, int x, int y)
		{
			Bitmap bm = new Bitmap(1, 1);
			bm.SetPixel(0, 0, pen.Color);
			g.DrawImageUnscaled(bm, x, y);
		}

		public static void DrawPoint(this Graphics g, Pen pen, Point pt)
		{
			DrawPoint(g, pen, pt.X, pt.Y);
		}
	}
}
