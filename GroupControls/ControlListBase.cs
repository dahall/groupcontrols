using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace GroupControls
{
	/// <summary>
	/// Abstract class that handles the display of numerous control items.
	/// </summary>
	public abstract class ControlListBase : ScrollableControl
	{
		internal const int lrPadding = 3, tPadding = 2;

		internal static readonly ContentAlignment anyRightAlignment, anyCenterAlignment, anyBottomAlignment, anyMiddleAlignment;
		internal static ToolTip toolTip;

		/// <summary>Number of displayed columns within the control. Not used.</summary>
		protected int columns = 1;
		/// <summary></summary>
		protected int hoverItem = -1;
		/// <summary></summary>
		protected int idealHeight = 100;
		/// <summary></summary>
		protected SparseArray<Rectangle> itemBounds = new SparseArray<Rectangle>();
		/// <summary></summary>
		protected int pressingItem = -1;
		/// <summary></summary>
		protected bool spaceEvenly = false;

		private Timer hoverTimer;
		private bool mouseTracking = false;
		private bool showToolTips = true;
		private int timedHoverItem = -1;

		static ControlListBase()
		{
			anyRightAlignment = ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight;
			anyCenterAlignment = ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter;
			anyBottomAlignment = ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight;
			anyMiddleAlignment = ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight;
			toolTip = new ToolTip();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ControlListBase"/> class.
		/// </summary>
		public ControlListBase()
		{
			base.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.CacheText | ControlStyles.SupportsTransparentBackColor | ControlStyles.StandardClick | ControlStyles.ResizeRedraw | ControlStyles.Opaque, true);
			this.SuspendLayout();
			this.Size = new System.Drawing.Size(100, 100);
			this.AutoSize = true;
			this.ResumeLayout(false);
			hoverTimer = new Timer();
			hoverTimer.Interval = SystemInformation.MouseHoverTime;
			hoverTimer.Tick += new EventHandler(hoverTimer_Tick);
		}

		/// <summary>
		/// Gets the base list of items.
		/// </summary>
		/// <value>Any list supportting and <see cref="System.Collections.IList"/> interface.</value>
		protected abstract System.Collections.IList BaseItems
		{
			get;
		}

		/// <summary>
		/// Gets or sets a value that determines whether the control resizes based on its content.
		/// </summary>
		/// <value>true if enabled; otherwise, false.</value>
		[DefaultValue(true),
		Category("Layout"),
		Browsable(true),
		Description("Autosizes the control to fit the contents"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override bool AutoSize
		{
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		/// <summary>
		/// Gets or sets a value that determines whether a tooltip is displayed for each item in the list.
		/// </summary>
		/// <value><c>true</c> if tooltips are shown; otherwise, <c>false</c>.</value>
		[DefaultValue(true),
		Description("Indicates whether to show the tooltip for each item."),
		Category("Appearance")]
		public bool ShowItemToolTip
		{
			get { return showToolTips; }
			set { showToolTips = value; }
		}

		/// <summary>
		/// Gets or sets a value that determines if the items are spaced evenly based on the height of the largest item or if they are spaced according to the height of each item.
		/// </summary>
		/// <value><c>true</c> if items are spaced evenly; otherwise, <c>false</c>.</value>
		[DefaultValue(false),
		Description("Spaces items evenly."),
		Category("Appearance")]
		public bool SpaceEvenly
		{
			get { return spaceEvenly; }
			set { spaceEvenly = value; ResetListLayout(); Refresh(); }
		}

		private int GetItemAtLocation(Point pt)
		{
			for (int i = 0; i < itemBounds.Count; i++)
			{
				if (itemBounds[i].Contains(pt))
					return i;
			}
			return -1;
		}

		private void hoverTimer_Tick(object sender, EventArgs e)
		{
			hoverTimer.Stop();
			if (mouseTracking && timedHoverItem == hoverItem)
			{
				UpdateToolTip(GetItemToolTipText(timedHoverItem));
			}
		}

		private void SetHover(int itemIndex)
		{
			if (itemIndex != hoverItem)
			{
				hoverTimer.Stop();
				UpdateToolTip(null);
				int oldHover = hoverItem;
				hoverItem = itemIndex;
				// clear hover item
				if (oldHover != -1)
					InvalidateItem(oldHover);
				// Set hover item
				if (itemIndex != -1 && IsItemEnabled(itemIndex))
				{
					InvalidateItem(hoverItem);
					if (!string.IsNullOrEmpty(GetItemToolTipText(hoverItem)))
					{
						timedHoverItem = hoverItem;
						hoverTimer.Start();
					}
				}
			}
		}

		private void UpdateToolTip(string tiptext)
		{
			if (showToolTips)
			{
				toolTip.Hide(this);
				toolTip.Active = false;
				Cursor currentInternal = Cursor.Current;
				if (currentInternal != null)
				{
					toolTip.Active = true;
					Point position = Cursor.Position;
					position.Y += this.Cursor.Size.Height - currentInternal.HotSpot.Y;
					toolTip.Show(tiptext, this, base.PointToClient(position), toolTip.AutoPopDelay);
				}
			}
		}

		/// <summary>
		/// Gets the specified item's tooltip text.
		/// </summary>
		/// <param name="index">The index of the item.</param>
		/// <returns>Tooltip text to display. <c>null</c> or <see cref="String.Empty"/> to display no tooltip.</returns>
		protected abstract string GetItemToolTipText(int index);

		/// <summary>
		/// Invalidates the specified item.
		/// </summary>
		/// <param name="index">The item index.</param>
		protected virtual void InvalidateItem(int index)
		{
			base.Invalidate(itemBounds[index]);
		}

		/// <summary>
		/// Determines whether the specified item is enabled.
		/// </summary>
		/// <param name="index">The item index.</param>
		/// <returns><c>true</c> if item is enabled; otherwise, <c>false</c>.</returns>
		protected virtual bool IsItemEnabled(int index) { return true; }

		/// <summary>
		/// Determines whether this list has the specified mnemonic in its members.
		/// </summary>
		/// <param name="charCode">The mnumonic character.</param>
		/// <returns><c>true</c> if list has the mnemonic; otherwise, <c>false</c>.</returns>
		protected abstract bool ListHasMnemonic(char charCode);

		/// <summary>
		/// Measures the specified item.
		/// </summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="maxSize">Maximum size of the item. Usually only constrains the width.</param>
		/// <returns>Minimum size for the item.</returns>
		protected abstract Size MeasureItem(Graphics g, int index, Size maxSize);

		/// <summary>
		/// Raises the <see cref="Control.AutoSizeChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnAutoSizeChanged(EventArgs e)
		{
			base.OnAutoSizeChanged(e);
		}

		/// <summary>
		/// Raises the <see cref="Control.EnabledChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnEnabledChanged(EventArgs e)
		{
			base.OnEnabledChanged(e);
		}

		/// <summary>
		/// Raises the <see cref="Control.Layout"/> event.
		/// </summary>
		/// <param name="e">An <see cref="LayoutEventArgs"/> that contains the event data.</param>
		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);
			ResetListLayout();
			Refresh();
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseDown"/> event.
		/// </summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			int i = GetItemAtLocation(e.Location);
			if (i == -1 || !IsItemEnabled(i))
				return;
			pressingItem = i;
			this.Focus();
			InvalidateItem(pressingItem);
			base.Update();
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseEnter"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			mouseTracking = true;
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseLeave"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			mouseTracking = false;
			SetHover(-1);
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseMove"/> event.
		/// </summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (mouseTracking)
				SetHover(GetItemAtLocation(e.Location));
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseUp"/> event.
		/// </summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (pressingItem != -1)
			{
				int ci = pressingItem;
				pressingItem = -1;
				InvalidateItem(ci);
			}
			base.OnMouseUp(e);
		}

		/// <summary>
		/// Raises the <see cref="Control.Paint"/> event.
		/// </summary>
		/// <param name="pe">An <see cref="PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs pe)
		{
			for (int i = 0; i < this.BaseItems.Count; i++)
			{
				PaintItem(pe.Graphics, i, itemBounds[i]);
			}
		}

		/// <summary>
		/// Raises the <see cref="Control.StyleChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnStyleChanged(EventArgs e)
		{
			ResetListLayout();
			base.OnStyleChanged(e);
		}

		/// <summary>
		/// Paints the specified item.
		/// </summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="bounds">The bounds in which to paint the item.</param>
		protected abstract void PaintItem(Graphics g, int index, Rectangle bounds);

		/// <summary>
		/// Processes a dialog key.
		/// </summary>
		/// <param name="keyData">One of the <see cref="Keys"/> values that represents the key to process.</param>
		/// <returns><c>true</c> if the key was processed by the control; otherwise, <c>false</c>.</returns>
		protected override bool ProcessDialogKey(Keys keyData)
		{
			System.Diagnostics.Debug.WriteLine(string.Format("ProcessDialogKey: {0}, {1}", keyData, keyData & Keys.KeyCode));
			switch ((keyData & Keys.KeyCode))
			{
				case Keys.Space:
				case Keys.Tab:
				case Keys.Prior:
				case Keys.Next:
				case Keys.End:
				case Keys.Home:
				case Keys.Left:
				case Keys.Up:
				case Keys.Right:
				case Keys.Down:
				case Keys.Return:
					KeyEventArgs args1 = new KeyEventArgs(keyData);
					if (this.ProcessKey(args1))
						return true;
					break;
				default:
					break;
			}
			return base.ProcessDialogKey(keyData);
		}

		/// <summary>
		/// Processes a keyboard event.
		/// </summary>
		/// <param name="ke">The <see cref="KeyEventArgs"/> associated with the key press.</param>
		/// <returns><c>true</c> if the key was processed by the control; otherwise, <c>false</c>.</returns>
		protected abstract bool ProcessKey(KeyEventArgs ke);

		/// <summary>
		/// Previews a keyboard message.
		/// </summary>
		/// <param name="m">A <see cref="Message"/>, passed by reference, that represents the window message to process.</param>
		/// <returns><c>true</c> if the key was processed by the control; otherwise, <c>false</c>.</returns>
		protected override bool ProcessKeyPreview(ref Message m)
		{
			if (m.Msg == 0x100)
			{
				KeyEventArgs args1 = new KeyEventArgs(((Keys)((int)((long)m.WParam))) | Control.ModifierKeys);
				System.Diagnostics.Debug.WriteLine(string.Format("ProcessKeyPreview 0x100: {0}", args1));
				switch (args1.KeyCode)
				{
					case Keys.Space:
					case Keys.Prior:
					case Keys.Next:
					case Keys.End:
					case Keys.Home:
					case Keys.Left:
					case Keys.Up:
					case Keys.Right:
					case Keys.Down:
					case Keys.Return:
						return this.ProcessKey(args1);
					default:
						break;
				}
			}
			else if (m.Msg == 0x101)
			{
				KeyEventArgs args2 = new KeyEventArgs(((Keys)((int)((long)m.WParam))) | Control.ModifierKeys);
				System.Diagnostics.Debug.WriteLine(string.Format("ProcessKeyPreview 0x101: {0}", args2));
				if (args2.KeyCode == Keys.Tab)
				{
					return this.ProcessKey(args2);
				}
			}

			return base.ProcessKeyPreview(ref m);
		}

		/// <summary>
		/// Processes a mnemonic.
		/// </summary>
		/// <param name="charCode">The character code.</param>
		/// <returns><c>true</c> if the mnemonic was processed by the control; otherwise, <c>false</c>.</returns>
		protected override bool ProcessMnemonic(char charCode)
		{
			if (this.Enabled && this.Visible && (this.Focused || base.ContainsFocus))
				return ListHasMnemonic(charCode);
			return false;
		}

		/// <summary>
		/// Resets the list's layout.
		/// </summary>
		protected virtual void ResetListLayout()
		{
			itemBounds.Clear();
			using (Graphics g = this.CreateGraphics())
			{
				// First get the base height of all items and the max height
				int maxItemHeight = 0;
				idealHeight = 0;
				Size maxSize = new Size((ClientSize.Width - (Padding.Horizontal * columns)) / columns, ClientSize.Height - Padding.Vertical);
				for (int i = 0; i < this.BaseItems.Count; i++)
				{
					Size sz = this.MeasureItem(g, i, maxSize);
					this.itemBounds[i] = new Rectangle(Point.Empty, sz);

					// Calculate minimum item height
					int minHeight = sz.Height;
					maxItemHeight = Math.Max(maxItemHeight, minHeight);
					idealHeight += minHeight;
				}
				if (spaceEvenly) idealHeight = maxItemHeight * this.BaseItems.Count;
				// Add in padding between items
				idealHeight += ((this.BaseItems.Count - 1) * this.Margin.Vertical);
				idealHeight += this.Padding.Vertical;

				// Position the text and glyph of each item
				Point loc = new Point(this.Padding.Left, this.Padding.Top);
				for (int i = 0; i < this.BaseItems.Count; i++)
				{
					// Set bounds of the item
					this.itemBounds[i] = new Rectangle(loc, this.itemBounds[i].Size);

					// Set top position of next item
					loc.Y += (spaceEvenly ? maxItemHeight : this.itemBounds[i].Height) + this.Margin.Vertical;
				}
			}

			// Set scroll height and autosize to ideal height
			this.AutoScrollMinSize = new Size(this.ClientRectangle.Width, idealHeight);
			if (this.AutoSize) this.Height = idealHeight;
		}

		internal void OnListChanged()
		{
			ResetListLayout();
			Refresh();
		}

		/// <summary>
		/// Retrieves the size of a rectangular area into which a control can be fitted.
		/// </summary>
		/// <param name="proposedSize">The custom-sized area for a control.</param>
		/// <returns>
		/// An ordered pair of type <see cref="T:System.Drawing.Size"/> representing the width and height of a rectangle.
		/// </returns>
		public override Size GetPreferredSize(Size proposedSize)
		{
			return new Size(this.Width, idealHeight);
		}
	}
}