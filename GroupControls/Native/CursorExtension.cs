using Vanara.Interop;
using System.Drawing;

namespace System.Windows.Forms
{
	internal static class CursorExtension
	{
		public static Size GetSize(this System.Windows.Forms.Cursor cursor)
		{
			Size size = Size.Empty;
			NativeMethods.ICONINFO info = new NativeMethods.ICONINFO();
			NativeMethods.GetIconInfo(cursor.Handle, info);
			if (info.hbmColor != IntPtr.Zero)
			{
				using (Bitmap bm = Bitmap.FromHbitmap(info.hbmColor))
					size = bm.Size;
			}
			else if (info.hbmMask != IntPtr.Zero)
			{
				using (Bitmap bm = Bitmap.FromHbitmap(info.hbmMask))
					size = new Size(bm.Width, bm.Height / 2);
			}
			return size;
		}

		public static Rectangle Bounds(this System.Windows.Forms.Cursor cursor)
		{
			using (Bitmap bmp = new Bitmap(cursor.Size.Width, cursor.Size.Height))
			{
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.Clear(Color.Transparent);
					cursor.Draw(g, new Rectangle(Point.Empty, bmp.Size));

					int xMin = bmp.Width;
					int xMax = -1;
					int yMin = bmp.Height;
					int yMax = -1;

					for (int x = 0; x < bmp.Width; x++)
					{
						for (int y = 0; y < bmp.Height; y++)
						{
							if (bmp.GetPixel(x, y).A > 0)
							{
								xMin = Math.Min(xMin, x);
								xMax = Math.Max(xMax, x);
								yMin = Math.Min(yMin, y);
								yMax = Math.Max(yMax, y);
							}
						}
					}
					return new Rectangle(new Point(xMin, yMin), new Size((xMax - xMin) + 1, (yMax - yMin) + 1));
				}
			}
		}
	}
}
