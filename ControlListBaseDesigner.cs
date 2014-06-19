using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace GroupControls
{
	internal class ControlListBaseDesigner : ControlDesigner
	{
		public ControlListBaseDesigner()
		{
			base.AutoResizeHandles = true;
		}

		protected override void OnPaintAdornments(PaintEventArgs pe)
		{
			ControlListBase component = base.Component as ControlListBase;
			if (component != null && component.Visible && component.BorderStyle == BorderStyle.None)
			{
				Color color = (this.Control.BackColor.GetBrightness() < 0.5) ? ControlPaint.Light(this.Control.BackColor) : ControlPaint.Dark(this.Control.BackColor);
				using (Pen pen = new Pen(color) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
				{
					Rectangle clientRectangle = this.Control.ClientRectangle;
					clientRectangle.Width--;
					clientRectangle.Height--;
					pe.Graphics.DrawRectangle(pen, clientRectangle);
				}
			}
			base.OnPaintAdornments(pe);
		}
	}
}
