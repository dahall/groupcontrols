using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace GroupControls
{
	/// <summary>
	/// Abstract class that handles the display of numerous control items.
	/// </summary>
	[System.ComponentModel.Design.Serialization.DesignerSerializer(typeof(Design.DesignerLayoutCodeDomSerializer), typeof(System.ComponentModel.Design.Serialization.CodeDomSerializer))]
	[System.ComponentModel.Designer(typeof(ControlListBaseDesigner))]
	public abstract class ControlListBase : ScrollableControl
	{
		internal static readonly ContentAlignment anyRightAlignment = ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight;
		internal static readonly ContentAlignment anyCenterAlignment = ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter;
		internal static readonly ContentAlignment anyBottomAlignment = ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight;
		internal static readonly ContentAlignment anyMiddleAlignment = ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight;
		internal static ToolTip toolTip = new ToolTip();

		private BorderStyle borderStyle;
		private int columns = 1;
		private Timer hoverTimer;
		private int idealHeight = 100;
		private SparseArray<Rectangle> itemBounds = new SparseArray<Rectangle>();
		private bool mouseTracking = false;
		private RepeatDirection repeatDirection = RepeatDirection.Vertical;
		private bool spaceEvenly = false;
		private Size spacing = new Size(0, 6);
		private int timedHoverItem = -1;

		/// <summary>
		/// Initializes a new instance of the <see cref="ControlListBase"/> class.
		/// </summary>
		protected ControlListBase()
		{
			DoubleBuffered = true;
			ResizeRedraw = true;
			HoverItem = PressingItem = -1;
			ShowItemToolTip = true;
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.StandardClick | ControlStyles.OptimizedDoubleBuffer, true);
			SuspendLayout();
			base.AutoScroll = true;
			base.Size = new System.Drawing.Size(100, 100);
			base.AutoSize = true;
			ResumeLayout(false);
			hoverTimer = new Timer() { Interval = SystemInformation.MouseHoverTime };
			hoverTimer.Tick += hoverTimer_Tick;
			PaddingChanged += OnLayoutPropertyChanged;
			SizeChanged += OnLayoutPropertyChanged;
		}

		/// <summary>
		/// Gets or sets a value indicating whether the container enables the user to scroll to any controls placed outside of its visible boundaries.
		/// </summary>
		/// <returns>true if the container enables auto-scrolling; otherwise, false. The default value is false. </returns>
		[DefaultValue(true), Category("Layout"), Browsable(true), 
		Description("Indicates whether scroll bars automatically appear when the control contents are larger than its visible area."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override bool AutoScroll
		{
			get { return base.AutoScroll; }
			set { base.AutoScroll = value; }
		}

		/// <summary>
		/// Gets or sets a value that determines whether the control resizes based on its content.
		/// </summary>
		/// <value>true if enabled; otherwise, false.</value>
		[DefaultValue(true), Category("Layout"), Browsable(true), Description("Autosizes the control to fit the contents"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override bool AutoSize
		{
			get { return base.AutoSize; }
			set { base.AutoSize = value; }
		}

		/// <summary>
		/// Gets or sets the border style of the list control.
		/// </summary>
		/// <value>
		/// One of the <see cref="BorderStyle"/> values. The default is <c>BorderStyle:None</c>.
		/// </value>
		[DefaultValue(0), Description("Border style of the list control."), Category("Appearance")]
		public BorderStyle BorderStyle
		{
			get
			{
				return borderStyle;
			}
			set
			{
				if (borderStyle != value)
				{
					borderStyle = value;
					base.UpdateStyles();
				}
			}
		}

		/// <summary>
		/// Gets the spacing in between items.
		/// </summary>
		/// <value>The <see cref="Size"/> representing the horizontal and vertical spacing between items.</value>
		[DefaultValue(typeof(Size), "0,6"), Category("Layout"), Description("Spacing between items")]
		public virtual Size ItemSpacing
		{
			get { return spacing; }
			set { spacing = value; ResetListLayout("ItemSpacing"); Refresh(); }
		}

		/// <summary>
		/// Gets or sets the number of columns to display in the control.
		/// </summary>
		/// <value>The repeat columns.</value>
		[DefaultValue(1), Category("Layout"), Description("Number of columns to display")]
		public virtual int RepeatColumns
		{
			get { return columns; }
			set { columns = value; ResetListLayout("RepeatColumns"); Refresh(); }
		}

		/// <summary>
		/// Gets or sets the direction in which the items within the group are displayed.
		/// </summary>
		/// <value>One of the <see cref="RepeatDirection"/> values. The default is <c>Vertical</c>.</value>
		[DefaultValue(typeof(RepeatDirection), "Vertical"), Category("Layout"), Description("Direction items are displayed")]
		public virtual RepeatDirection RepeatDirection
		{
			get { return repeatDirection; }
			set { repeatDirection = value; ResetListLayout("RepeatDirection"); Refresh(); }
		}

		/// <summary>
		/// Gets or sets a value that determines whether a tooltip is displayed for each item in the list.
		/// </summary>
		/// <value><c>true</c> if tooltips are shown; otherwise, <c>false</c>.</value>
		[DefaultValue(true), Category("Appearance"), Description("Indicates whether to show the tooltip for each item.")]
		public bool ShowItemToolTip { get; set; }

		/// <summary>
		/// Gets or sets a value that determines if the items are spaced evenly based on the height of the largest item or if they are spaced according to the height of each item.
		/// </summary>
		/// <value><c>true</c> if items are spaced evenly; otherwise, <c>false</c>.</value>
		[DefaultValue(false), Description("Spaces items evenly."), Category("Appearance")]
		public bool SpaceEvenly
		{
			get { return spaceEvenly; }
			set { spaceEvenly = value; ResetListLayout("SpaceEvenly"); Refresh(); }
		}

		/// <summary>
		/// Gets or sets the text associated with this control.
		/// </summary>
		/// <returns>The text associated with this control.</returns>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		/// <summary>
		/// Gets the base list of items.
		/// </summary>
		/// <value>Any list supporting and <see cref="System.Collections.IList"/> interface.</value>
		protected abstract System.Collections.IList BaseItems { get; }

		/// <summary>
		/// Gets the required creation parameters when the control handle is created.
		/// </summary>
		/// <returns>A <see cref="T:System.Windows.Forms.CreateParams"/> that contains the required creation parameters when the handle to the control is created.</returns>
		protected override CreateParams CreateParams
		{
			get
			{
				const int WS_EX_CONTROLPARENT = 0x10000;
				const int WS_EX_CLIENTEDGE = 0x200;
				const int WS_BORDER = 0x800000;
				const int WS_EX_LAYOUTRTL = 0x400000;
				const int WS_EX_NOINHERITLAYOUT = 0x100000;
				const int WS_EX_RIGHT = 0x1000;
				const int WS_EX_RTLREADING = 0x2000;
				const int WS_EX_LEFTSCROLLBAR = 0x4000;

				CreateParams createParams = base.CreateParams;
				createParams.ExStyle &= ~WS_EX_CONTROLPARENT;

				// Set border
				createParams.ExStyle &= ~WS_EX_CLIENTEDGE;
				createParams.Style &= ~WS_BORDER;
				switch (borderStyle)
				{
					case BorderStyle.FixedSingle:
						createParams.Style |= WS_BORDER;
						break;
					case BorderStyle.Fixed3D:
						createParams.ExStyle |= WS_EX_CLIENTEDGE;
						break;
				}

				// Set right to left layout
				Form parent = FindForm();
				bool parentRightToLeftLayout = parent != null ? parent.RightToLeftLayout : false;
				if ((RightToLeft == RightToLeft.Yes) && parentRightToLeftLayout)
				{
					createParams.ExStyle |= WS_EX_LAYOUTRTL | WS_EX_NOINHERITLAYOUT;
					createParams.ExStyle &= ~(WS_EX_RIGHT | WS_EX_RTLREADING | WS_EX_LEFTSCROLLBAR);
				}

				return createParams;
			}
		}

		/// <summary>
		/// Gets the hover item's index.
		/// </summary>
		/// <value>The hover item index.</value>
		protected int HoverItem { get; private set; }

		/// <summary>
		/// Gets or sets the index of the item being pressing.
		/// </summary>
		/// <value>The pressed item index.</value>
		protected int PressingItem { get; set; }

		/// <summary>
		/// Method that will draw a control's background in a specified area.
		/// </summary>
		/// <param name="g">The Graphics object used to draw.</param>
		/// <param name="bounds">The bounds.</param>
		/// <param name="childControl">The child control.</param>
		protected delegate void PaintBackgroundMethod(Graphics g, Rectangle bounds, Control childControl);

		/// <summary>
		/// Gets the background renderer for this type of control.
		/// </summary>
		/// <value>
		/// The background renderer.
		/// </value>
		protected virtual PaintBackgroundMethod BackgroundRenderer => ButtonRenderer.DrawParentBackground;

		internal void OnListChanged()
		{
			ResetListLayout("Items");
			Refresh();
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing">
		///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			hoverTimer.Tick -= hoverTimer_Tick;
			hoverTimer.Dispose();
			base.Dispose(disposing);
		}

		/// <summary>
		/// Ensures that the specified item is visible within the control, scrolling the contents of the control if necessary.
		/// </summary>
		/// <param name="index">The zero-based index of the item to scroll into view.</param>
		public abstract void EnsureVisible(int index);

		/// <summary>
		/// Retrieves the bounding rectangle for a specific item within the list control.
		/// </summary>
		/// <param name="index">The zero-based index of the item whose bounding rectangle you want to return.</param>
		/// <returns>A <see cref="Rectangle"/> that represents the bounding rectangle of the specified item.</returns>
		public Rectangle GetItemRect(int index)
		{
			if (index < 0 || index >= BaseItems.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			return itemBounds[index];
		}

		/// <summary>
		/// Gets the specified item's tooltip text.
		/// </summary>
		/// <param name="index">The index of the item.</param>
		/// <returns>Tooltip text to display. <c>null</c> or <see cref="String.Empty"/> to display no tooltip.</returns>
		protected virtual string GetItemToolTipText(int index) => null;

		/// <summary>
		/// Invalidates the specified item.
		/// </summary>
		/// <param name="index">The item index.</param>
		protected virtual void InvalidateItem(int index)
		{
			base.Invalidate(OffsetForScroll(itemBounds[index]));
			//base.Invalidate();
		}

		/// <summary>
		/// Determines whether the specified item is enabled.
		/// </summary>
		/// <param name="index">The item index.</param>
		/// <returns><c>true</c> if item is enabled; otherwise, <c>false</c>.</returns>
		protected virtual bool IsItemEnabled(int index) => true;

		/// <summary>
		/// Determines whether this list has the specified mnemonic in its members.
		/// </summary>
		/// <param name="charCode">The mnemonic character.</param>
		/// <returns><c>true</c> if list has the mnemonic; otherwise, <c>false</c>.</returns>
		protected virtual bool ListHasMnemonic(char charCode) => false;

		/// <summary>
		/// Measures the specified item.
		/// </summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="maxSize">Maximum size of the item. Usually only constrains the width.</param>
		/// <returns>Minimum size for the item.</returns>
		protected abstract Size MeasureItem(Graphics g, int index, Size maxSize);

		/// <summary>
		/// Offsets the client point for scrolling.
		/// </summary>
		/// <returns>Offset point</returns>
		protected Point OffsetForScroll(Point pt)
		{
			System.Diagnostics.Debug.Write($"OffsetForScroll: pt={pt}; scPos={AutoScrollPosition}; ");
			/*//Creates the drawing matrix with the right zoom;
			Matrix mx = new Matrix(1, 0, 0, 1, 0, 0);
			//pans it according to the scroll bars
			mx.Translate(this.AutoScrollPosition.X, this.AutoScrollPosition.Y);
			//inverts it
			mx.Invert();

			//uses it to transform the current mouse position
			Point[] pa = new Point[] { pt };
			mx.TransformPoints(pa);
			System.Diagnostics.Debug.WriteLine($"outPt={pa[0]};");

			return pa[0];*/
			pt.Offset(-AutoScrollPosition.X, -AutoScrollPosition.Y);
			System.Diagnostics.Debug.WriteLine($"outPt={pt};");
			return pt;
		}

		/// <summary>
		/// Offsets the client rectangle for scrolling.
		/// </summary>
		/// <returns>Offset rectangle</returns>
		protected Rectangle OffsetForScroll(Rectangle rect)
		{
			System.Diagnostics.Debug.Write($"OffsetForScroll: rect={rect};");
			Rectangle outRect = rect;
			outRect.Offset(AutoScrollPosition);
			System.Diagnostics.Debug.WriteLine($"outrect={outRect};");
			return outRect;
		}

		/// <summary>
		/// Raises the <see cref="Control.KeyUp"/> event.
		/// </summary>
		/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (PressingItem != -1)
			{
				int ci = PressingItem;
				PressingItem = -1;
				InvalidateItem(ci);
			}
			// Handle button press on Space
			base.OnKeyUp(e);
		}

		/// <summary>
		/// Raises the <see cref="Control.Layout"/> event.
		/// </summary>
		/// <param name="e">An <see cref="LayoutEventArgs"/> that contains the event data.</param>
		protected override void OnLayout(LayoutEventArgs e)
		{
			base.OnLayout(e);

			if (BaseItems == null || BaseItems.Count == 0)
				return;

			System.Diagnostics.Debug.WriteLine(Name + ": OnLayout: " + e.AffectedProperty);
			System.Diagnostics.Debug.WriteLine($"  ClientSize:{ClientSize}, Margin:{Margin}, Padding:{Padding}, Cols:{columns}, Spacing:{spacing}, Items:{BaseItems.Count}, Dir:{RepeatDirection}, Even:{spaceEvenly}");
			itemBounds.Clear();
			using (Graphics g = CreateGraphics())
			{
				// Determine the start coordinate of each column
				int colWidth = (ClientSize.Width - Padding.Horizontal - ((columns - 1) * spacing.Width)) / columns;
				Point[] colPos = new Point[columns];
				for (int x = 0; x < columns; x++)
					colPos[x] = new Point(Padding.Left + (x * (colWidth + spacing.Width)), Padding.Top);

				// Get the base height of all items and the max height
				int maxItemHeight = 0;
				idealHeight = 0;
				Size maxSize = new Size(colWidth, 0);
				for (int i = 0; i < BaseItems.Count; i++)
				{
					Size sz = MeasureItem(g, i, maxSize);
					itemBounds[i] = new Rectangle(Point.Empty, sz);

					// Calculate maximum item height
					maxItemHeight = Math.Max(maxItemHeight, sz.Height);
				}

				// Calculate the positions of each item
				int curCol = 0;
				for (int i = 0; i < BaseItems.Count; i++)
				{
					// Set bounds of the item
					itemBounds[i] = new Rectangle(colPos[curCol], itemBounds[i].Size);
					// Set top position of next item
					colPos[curCol].Y += (spaceEvenly ? maxItemHeight : itemBounds[i].Height) + spacing.Height;
					if (RepeatDirection == GroupControls.RepeatDirection.Horizontal)
						if (++curCol == columns) curCol = 0;
					// If spacing evenly we can determine all locations now by changing column count at pure divisions
					/*if (spaceEvenly && RepeatDirection == GroupControls.RepeatDirection.Vertical && i > 0)
					{
						if (i % (this.BaseItems.Count / columns) == 0 && curCol <= (this.BaseItems.Count % columns))
							curCol++;
					}*/
				}

				// Split vertical columns and reset positions of items
				if (RepeatDirection == GroupControls.RepeatDirection.Vertical && columns > 1)
				{
					int idealColHeight = colPos[0].Y / columns;
					int thisColBottom = idealColHeight;
					int y = Padding.Top + Margin.Top;
					for (int i = 0; i < BaseItems.Count; i++)
					{
						Rectangle iBounds = itemBounds[i];
						Rectangle nBounds = Rectangle.Empty;
						if ((i + 1) < BaseItems.Count)
							nBounds = itemBounds[i + 1];

						if (curCol > 0)
							itemBounds[i] = new Rectangle(new Point(colPos[curCol].X, y), itemBounds[i].Size);
						colPos[curCol].Y = itemBounds[i].Bottom + spacing.Height;

						if ((iBounds.Bottom > thisColBottom || nBounds.Bottom > thisColBottom) && (curCol + 1 < columns))
						{
							if (Math.Abs(iBounds.Bottom - idealColHeight) < Math.Abs(nBounds.Bottom - idealColHeight))
							{
								y = Padding.Top;
								curCol++;
								thisColBottom = iBounds.Bottom + spacing.Height + idealColHeight;
							}
						}
						else
						{
							y += (spaceEvenly ? maxItemHeight : itemBounds[i].Height) + spacing.Height;
						}
					}
				}

				// Set ideal height
				idealHeight = 0;
				for (int c = 0; c < columns; c++)
					if (idealHeight < colPos[c].Y) idealHeight = colPos[c].Y;
				idealHeight = idealHeight - spacing.Height + Padding.Bottom;
			}

			// Set scroll height and autosize to ideal height
			AutoScrollMinSize = new Size(ClientRectangle.Width, idealHeight);
			if (AutoSize) Height = idealHeight;

#if DEBUG
			var sb = new System.Text.StringBuilder();
			for (int i = 0; i < itemBounds.Count; i++)
				sb.AppendFormat("({0}),", itemBounds[i]);
			System.Diagnostics.Debug.WriteLine("  " + sb.ToString());
#endif
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseDown"/> event.
		/// </summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			int i = GetItemAtLocation(OffsetForScroll(e.Location));
			if (i == -1 || !IsItemEnabled(i))
				return;
			PressingItem = i;
			Focus();
			InvalidateItem(PressingItem);
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
				SetHover(GetItemAtLocation(OffsetForScroll(e.Location)));
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseUp"/> event.
		/// </summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (PressingItem != -1)
			{
				int ci = PressingItem;
				PressingItem = -1;
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
			System.Diagnostics.Debug.WriteLine($"OnPaint: {pe.ClipRectangle}");
			Point pt = AutoScrollPosition;
			pe.Graphics.TranslateTransform(pt.X, pt.Y);

			pe.Graphics.Clear(BackColor);
			if (Application.RenderWithVisualStyles)
				BackgroundRenderer(pe.Graphics, pe.ClipRectangle, this);

			for (int i = 0; i < BaseItems.Count; i++)
			{
				if (pe.ClipRectangle.IntersectsWith(OffsetForScroll(itemBounds[i])))
				{
					PaintItem(pe.Graphics, i, itemBounds[i]);
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ScrollableControl.Scroll"/> event.
		/// </summary>
		/// <param name="se">A <see cref="T:System.Windows.Forms.ScrollEventArgs"/> that contains the event data.</param>
		protected override void OnScroll(ScrollEventArgs se)
		{
			base.OnScroll(se);
			Invalidate();
		}

		/// <summary>
		/// Raises the <see cref="Control.StyleChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnStyleChanged(EventArgs e)
		{
			ResetListLayout("Style");
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
					if (ProcessKey(args1))
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
		protected virtual bool ProcessKey(KeyEventArgs ke) => false;

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
						return ProcessKey(args1);
					default:
						break;
				}
			}
			else if (m.Msg == 0x101)
			{
				KeyEventArgs args2 = new KeyEventArgs(((Keys)((int)((long)m.WParam))) | Control.ModifierKeys);
				if (args2.KeyCode == Keys.Tab)
				{
					return ProcessKey(args2);
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
			if (Enabled && Visible && (Focused || base.ContainsFocus))
				return ListHasMnemonic(charCode);
			return false;
		}

		/// <summary>
		/// Resets the list's layout.
		/// </summary>
		/// <param name="propertyName">Name of the property forcing the layout.</param>
		protected virtual void ResetListLayout(string propertyName)
		{
			PerformLayout(this, propertyName);
		}

		/// <summary>
		/// Gets the item at location within the control.
		/// </summary>
		/// <param name="pt">The location.</param>
		/// <returns></returns>
		protected int GetItemAtLocation(Point pt)
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
			if (mouseTracking && timedHoverItem == HoverItem)
			{
				UpdateToolTip(GetItemToolTipText(timedHoverItem));
			}
		}

		private void OnLayoutPropertyChanged(object sender, EventArgs e)
		{
			ResetListLayout("Layout");
		}

		private void SetHover(int itemIndex)
		{
			if (itemIndex != HoverItem)
			{
				System.Diagnostics.Debug.WriteLine($"SetHover: old={HoverItem}, new={itemIndex}");
				hoverTimer.Stop();
				UpdateToolTip(null);
				int oldHover = HoverItem;
				HoverItem = itemIndex;
				// If no new item, invalidate everything
				if (HoverItem == -1)
				{
					base.Invalidate();
				}
				// Set hover item
				else
				{
					// clear old hover item
					if (oldHover != -1)
						InvalidateItem(oldHover);

					InvalidateItem(HoverItem);

					if (!string.IsNullOrEmpty(GetItemToolTipText(HoverItem)))
					{
						timedHoverItem = HoverItem;
						hoverTimer.Start();
					}
				}
				base.Update();
			}
		}

		private void UpdateToolTip(string tiptext)
		{
			toolTip.Hide(this);
			toolTip.Active = false;
			if (ShowItemToolTip && !string.IsNullOrEmpty(tiptext))
			{
				if (Cursor.Current != null)
				{
					toolTip.Active = true;

					Point position = Microsoft.Win32.NativeMethods.MapPointToClient(this, Cursor.Position);
					position.Offset(0, Cursor.Current.Bounds().Bottom);
					if (RightToLeft == System.Windows.Forms.RightToLeft.Yes)
						position.X = Width - position.X;
					toolTip.Show(tiptext, this, position, toolTip.AutoPopDelay);
				}
			}
		}
	}

	/// <summary>
	/// Specifies the direction in which items of a list control are displayed.
	/// </summary>
	public enum RepeatDirection
	{
		/// <summary>Items of a list are displayed vertically in columns from top to bottom, and then left to right, until all items are rendered.</summary>
		Vertical,
		/// <summary>Items of a list are displayed horizontally in rows from left to right, then top to bottom, until all items are rendered.</summary>
		Horizontal
	}
}