using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GroupControls
{
	public class LabelList : ControlListBase
	{
		List<StringItem> list = new List<StringItem>();
		TextFormatFlags textFormatFlags = TextFormatFlags.WordBreak | TextFormatFlags.PreserveGraphicsTranslateTransform;

		protected override System.Collections.IList BaseItems
		{
			get { return list; }
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public List<StringItem> Items { get { return list; } }

		public override void EnsureVisible(int index)
		{
		}

		protected override string GetItemToolTipText(int index)
		{
			return list[index];
		}

		protected override Size MeasureItem(Graphics g, int index, Size maxSize)
		{
			return TextRenderer.MeasureText(g, list[index], this.Font, maxSize, TextFormatFlags);
		}

		protected override void PaintItem(Graphics g, int index, Rectangle bounds)
		{
			Font ft = this.Font;
			if (base.HoverItem == index)
				ft = new Font(ft, FontStyle.Bold);
			TextRenderer.DrawText(g, list[index], ft, bounds, this.ForeColor, this.BackColor, TextFormatFlags);
		}

		protected TextFormatFlags TextFormatFlags
		{
			get { return this.RightToLeft == RightToLeft.Yes ? textFormatFlags | TextFormatFlags.RightToLeft : textFormatFlags; }
		}
	}

	public class StringItem
	{
		public string Text { get; set; }
		public StringItem() { Text = null; }
		public static implicit operator System.String(StringItem i) { return i.Text; }
		public override string ToString() { return Text; }
	}
}
