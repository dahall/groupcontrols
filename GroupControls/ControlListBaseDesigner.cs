﻿#pragma warning disable GlobalUsingsAnalyzer // Using should be in global file
using System.Windows.Forms.Design;
#pragma warning restore GlobalUsingsAnalyzer // Using should be in global file

namespace GroupControls;

internal class ControlListBaseDesigner : RichControlDesigner<ControlListBase, ControlListBaseDesigner.ActionList>
{
	public ControlListBaseDesigner() => base.AutoResizeHandles = true;

	protected override void OnPaintAdornments(PaintEventArgs pe)
	{
		if (Control != null && Control.Visible && Control.BorderStyle == BorderStyle.None)
		{
			var color = (Control.BackColor.GetBrightness() < 0.5) ? ControlPaint.Light(Control.BackColor) : ControlPaint.Dark(Control.BackColor);
			using var pen = new Pen(color) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
			var clientRectangle = Control.ClientRectangle;
			clientRectangle.Width--;
			clientRectangle.Height--;
			pe.Graphics.DrawRectangle(pen, clientRectangle);
		}
		base.OnPaintAdornments(pe);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1812:AvoidUninstantiatedInternalClasses")]
	public class ActionList : RichDesignerActionList<ControlListBaseDesigner, ControlListBase>
	{
		public ActionList(ControlListBaseDesigner d, ControlListBase c) : base(d, c)
		{
		}

		[DesignerActionProperty("Dock", 1)]
		public DockStyle Dock
		{
			get => Component.Dock;
			set { if (Component.Dock != value) { Component.Dock = value; } }
		}

		[DesignerActionProperty("Columns", 2)]
		public int RepeatColumns
		{
			get => Component.RepeatColumns;
			set { if (Component.RepeatColumns != value) { Component.RepeatColumns = value; } }
		}

		[DesignerActionProperty("Repeat Direction", 2)]
		public RepeatDirection RepeatDirection
		{
			get => Component.RepeatDirection;
			set { if (Component.RepeatDirection != value) { Component.RepeatDirection = value; } }
		}

#if NETFRAMEWORK
		[DesignerActionMethod("Edit Items...", 0, IncludeAsDesignerVerb = true)]
		public void EditItems() => ParentDesigner.EditValue(Component, "Items");
#endif
	}
}