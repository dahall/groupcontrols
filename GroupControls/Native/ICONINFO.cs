using System;
using System.Runtime.InteropServices;

namespace Vanara.Interop
{
	internal static partial class NativeMethods
	{
		[DllImport("user32.dll", ExactSpelling = true, SetLastError = true)]
		public static extern bool GetIconInfo(IntPtr hIcon, [In, Out] NativeMethods.ICONINFO info);

		[StructLayout(LayoutKind.Sequential)]
		public class ICONINFO
		{
			public int fIcon;
			public int xHotspot;
			public int yHotspot;
			public IntPtr hbmMask = IntPtr.Zero;
			public IntPtr hbmColor = IntPtr.Zero;
		}
	}
}