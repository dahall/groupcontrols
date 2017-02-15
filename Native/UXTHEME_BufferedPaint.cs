using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Vanara.Interop
{
	internal static partial class NativeMethods
	{
		[Flags]
		public enum BufferedPaintAnimationStyle
		{
			None = 0,
			Linear = 1,
			Cubic = 2,
			Sine = 3
		}

		public enum BufferedPaintBufferFormat
		{
			CompatibleBitmap,
			Dib,
			TopDownDib,
			TopDownMonoDib
		}

		[Flags]
		public enum BufferedPaintParamsFlags
		{
			None = 0,
			Erase = 1,
			NoClip = 2,
			NonClient = 4,
		}

		[DllImport("uxtheme.dll", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr BeginBufferedAnimation(HandleRef hwnd, SafeDCHandle hdcTarget, [In] ref RECT rcTarget, BufferedPaintBufferFormat dwFormat,
			[In] BufferedPaintParams pPaintParams, [In] ref BufferedPaintAnimationParams pAnimationParams, out IntPtr phdcFrom, out IntPtr phdcTo);

		[DllImport("uxtheme.dll", ExactSpelling = true, SetLastError = true)]
		public static extern IntPtr BeginBufferedPaint(SafeDCHandle hdcTarget, [In] ref RECT prcTarget, BufferedPaintBufferFormat dwFormat, [In] BufferedPaintParams pPaintParams, out IntPtr phdc);

		[DllImport("uxtheme.dll", ExactSpelling = true, PreserveSig = false)]
		public static extern void BufferedPaintInit();

		[DllImport("uxtheme.dll", ExactSpelling = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool BufferedPaintRenderAnimation(HandleRef hwnd, SafeDCHandle hdcTarget);

		[DllImport("uxtheme.dll", ExactSpelling = true, PreserveSig = false)]
		public static extern void BufferedPaintStopAllAnimations(HandleRef hwnd);

		[DllImport("uxtheme.dll", ExactSpelling = true, PreserveSig = false)]
		public static extern void BufferedPaintUnInit();

		[DllImport("uxtheme.dll", ExactSpelling = true, PreserveSig = false)]
		public static extern void EndBufferedAnimation(IntPtr hbpAnimation, [MarshalAs(UnmanagedType.Bool)] bool fUpdateTarget);

		[DllImport("uxtheme.dll", ExactSpelling = true, PreserveSig = false)]
		public static extern void EndBufferedPaint(IntPtr hbp, [MarshalAs(UnmanagedType.Bool)] bool fUpdateTarget);

		[StructLayout(LayoutKind.Sequential)]
		public struct BufferedPaintAnimationParams
		{
			private int cbSize, dwFlags;
			private BufferedPaintAnimationStyle style;
			private int dwDuration;

			public BufferedPaintAnimationParams(int duration, BufferedPaintAnimationStyle animStyle = BufferedPaintAnimationStyle.Linear)
			{
				cbSize = Marshal.SizeOf(typeof(BufferedPaintAnimationParams));
				dwFlags = 0;
				dwDuration = duration;
				style = animStyle;
			}

			public BufferedPaintAnimationStyle AnimationStyle
			{
				get { return style; }
				set { style = value; }
			}

			public int Duration
			{
				get { return dwDuration; }
				set { dwDuration = value; }
			}

			public static BufferedPaintAnimationParams Empty => new BufferedPaintAnimationParams { cbSize = Marshal.SizeOf(typeof(BufferedPaintAnimationParams)) };
		}

		[StructLayout(LayoutKind.Sequential)]
		public class BufferedPaintParams : IDisposable
		{
			private int cbSize;
			public BufferedPaintParamsFlags Flags;
			private IntPtr prcExclude;
			private IntPtr pBlendFunction;

			public BufferedPaintParams(BufferedPaintParamsFlags flags = BufferedPaintParamsFlags.None)
			{
				cbSize = Marshal.SizeOf(typeof(BufferedPaintParams));
				Flags = flags;
				prcExclude = pBlendFunction = IntPtr.Zero;
			}

			public Rectangle? Exclude
			{
				get { return prcExclude.PtrToStructure<Rectangle>(); }
				set { InteropUtil.StructureToPtr(value, ref prcExclude, t => t.IsEmpty); }
			}

			public BLENDFUNCTION? BlendFunction
			{
				get { return pBlendFunction.PtrToStructure<BLENDFUNCTION>(); }
				set { InteropUtil.StructureToPtr(value, ref pBlendFunction, t => t.IsEmpty); }
			}

			public void Dispose()
			{
				if (prcExclude != IntPtr.Zero) Marshal.FreeHGlobal(prcExclude);
				if (pBlendFunction != IntPtr.Zero) Marshal.FreeHGlobal(pBlendFunction);
			}

			public static BufferedPaintParams NoClip => new BufferedPaintParams(BufferedPaintParamsFlags.NoClip);
			public static BufferedPaintParams ClearBg => new BufferedPaintParams(BufferedPaintParamsFlags.Erase);
		}
	}
}
