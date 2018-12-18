using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using Vanara.Interop;

namespace GroupControls
{
	/// <summary>Specifies the direction in which items of a list control are displayed.</summary>
	public enum RepeatDirection
	{
		/// <summary>
		/// Items of a list are displayed vertically in columns from top to bottom, and then left to right, until all items are rendered.
		/// </summary>
		Vertical,

		/// <summary>Items of a list are displayed horizontally in rows from left to right, then top to bottom, until all items are rendered.</summary>
		Horizontal
	}

	/// <summary>Abstract class that handles the display of numerous control items.</summary>
	[System.ComponentModel.Design.Serialization.DesignerSerializer(typeof(Design.DesignerLayoutCodeDomSerializer), typeof(System.ComponentModel.Design.Serialization.CodeDomSerializer))]
	[Designer(typeof(ControlListBaseDesigner))]
	public abstract class ControlListBase : ScrollableControl
	{
		internal const ContentAlignment anyBottomAlignment = (ContentAlignment)((int)ContentAlignment.BottomLeft | (int)ContentAlignment.BottomCenter | (int)ContentAlignment.BottomRight);
		internal const ContentAlignment anyCenterAlignment = (ContentAlignment)((int)ContentAlignment.TopCenter | (int)ContentAlignment.MiddleCenter | (int)ContentAlignment.BottomCenter);
		internal const ContentAlignment anyMiddleAlignment = (ContentAlignment)((int)ContentAlignment.MiddleLeft | (int)ContentAlignment.MiddleCenter | (int)ContentAlignment.MiddleRight);
		internal const ContentAlignment anyRightAlignment = (ContentAlignment)((int)ContentAlignment.TopRight | (int)ContentAlignment.MiddleRight | (int)ContentAlignment.BottomRight);
		internal static readonly ToolTip toolTip = new ToolTip();

		private readonly Timer hoverTimer;
		private BorderStyle borderStyle;
		private int columns = 1;
		private int hoverItem = -1;
		private int hoverItemOld = -1;
		private ColumnLayoutEngine layoutEngine;
		private bool mouseTracking;
		private int pressingItem = -1;
		private int pressingItemOld = -1;
		private RepeatDirection repeatDirection = RepeatDirection.Vertical;
		private bool spaceEvenly;
		private Size spacing = new Size(0, 6);
		private int timedHoverItem = -1;
		private bool variableColWidth;

		/// <summary>Initializes a new instance of the <see cref="ControlListBase"/> class.</summary>
		protected ControlListBase()
		{
			ResizeRedraw = true;
			SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.SupportsTransparentBackColor | ControlStyles.StandardClick, true);
			SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
			base.DoubleBuffered = true;
			SuspendLayout();
			base.AutoScroll = true;
			Size = ColumnLayoutEngine.DefaultSize;
			base.AutoSize = true;
			ResumeLayout(false);
			hoverTimer = new Timer { Interval = SystemInformation.MouseHoverTime };
			hoverTimer.Tick += hoverTimer_Tick;
			PaddingChanged += OnLayoutPropertyChanged;
			SizeChanged += OnLayoutPropertyChanged;
		}

		/// <summary>Method that will draw a control's background in a specified area.</summary>
		/// <param name="g">The Graphics object used to draw.</param>
		/// <param name="bounds">The bounds.</param>
		/// <param name="childControl">The child control.</param>
		protected delegate void PaintBackgroundMethod(Graphics g, Rectangle bounds, Control childControl);

		/// <summary>
		/// Gets or sets a value indicating whether the container enables the user to scroll to any controls placed outside of its visible boundaries.
		/// </summary>
		/// <returns>true if the container enables auto-scrolling; otherwise, false. The default value is false.</returns>
		[DefaultValue(true), Category("Layout"), Browsable(true),
		Description("Indicates whether scroll bars automatically appear when the control contents are larger than its visible area."),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override bool AutoScroll
		{
			get => base.AutoScroll;
			set => base.AutoScroll = value;
		}

		/// <summary>Gets or sets a value that determines whether the control resizes based on its content.</summary>
		/// <value>true if enabled; otherwise, false.</value>
		[DefaultValue(true), Category("Layout"), Browsable(true), Description("Autosizes the control to fit the contents"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
		public override bool AutoSize
		{
			get => base.AutoSize;
			set => base.AutoSize = value;
		}

		/// <summary>Gets or sets the border style of the list control.</summary>
		/// <value>One of the <see cref="BorderStyle"/> values. The default is <c>BorderStyle:None</c>.</value>
		[DefaultValue(0), Description("Border style of the list control."), Category("Appearance")]
		public BorderStyle BorderStyle
		{
			get => borderStyle;
			set
			{
				if (borderStyle != value)
				{
					borderStyle = value;
					UpdateStyles();
				}
			}
		}

		/// <summary>Gets the spacing in between items.</summary>
		/// <value>The <see cref="Size"/> representing the horizontal and vertical spacing between items.</value>
		[DefaultValue(typeof(Size), "0,6"), Category("Layout"), Description("Spacing between items")]
		public virtual Size ItemSpacing
		{
			get => spacing;
			set { spacing = value; ResetListLayout(nameof(ItemSpacing)); Refresh(); }
		}

		/// <summary>Gets or sets the layout engine.</summary>
		/// <value>The layout engine.</value>
		public override LayoutEngine LayoutEngine => MyLayoutEngine;

		/// <summary>Gets or sets the number of columns to display in the control.</summary>
		/// <value>The repeat columns.</value>
		[DefaultValue(1), Category("Layout"), Description("Number of columns to display")]
		public virtual int RepeatColumns
		{
			get => columns;
			set { columns = value; ResetListLayout(nameof(RepeatColumns)); Refresh(); }
		}

		/// <summary>Gets or sets the direction in which the items within the group are displayed.</summary>
		/// <value>One of the <see cref="RepeatDirection"/> values. The default is <c>Vertical</c>.</value>
		[DefaultValue(typeof(RepeatDirection), "Vertical"), Category("Layout"), Description("Direction items are displayed")]
		public virtual RepeatDirection RepeatDirection
		{
			get => repeatDirection;
			set { repeatDirection = value; ResetListLayout(nameof(RepeatDirection)); Refresh(); }
		}

		/// <summary>Gets or sets a value that determines whether a tooltip is displayed for each item in the list.</summary>
		/// <value><c>true</c> if tooltips are shown; otherwise, <c>false</c>.</value>
		[DefaultValue(true), Category("Appearance"), Description("Indicates whether to show the tooltip for each item.")]
		public bool ShowItemToolTip { get; set; } = true;

		/// <summary>
		/// Gets or sets a value that determines if the items are spaced evenly based on the height of the largest item or if they are spaced
		/// according to the height of each item.
		/// </summary>
		/// <value><c>true</c> if items are spaced evenly; otherwise, <c>false</c>.</value>
		[DefaultValue(false), Description("Spaces items evenly."), Category("Appearance")]
		public bool SpaceEvenly
		{
			get => spaceEvenly;
			set { spaceEvenly = value; ResetListLayout(nameof(SpaceEvenly)); Refresh(); }
		}

		/// <summary>Gets or sets the text associated with this control.</summary>
		/// <returns>The text associated with this control.</returns>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Bindable(false)]
		public override string Text
		{
			get => base.Text;
			set => base.Text = value;
		}

		internal bool IsDesignerHosted
		{
			get
			{
				Control ctrl = this;
				while (ctrl != null)
				{
					if ((ctrl.Site != null) && ctrl.Site.DesignMode)
						return true;
					ctrl = ctrl.Parent;
				}
				return false;
			}
		}

		internal ColumnLayoutEngine MyLayoutEngine => layoutEngine ?? (layoutEngine = new ColumnLayoutEngine());

		/// <summary>
		/// Gets or sets a value that determines if the columns are allowed to be a variable width or if they are to be all the same width.
		/// </summary>
		/// <value><c>true</c> if the columns can be a variable width; otherwise, <c>false</c>.</value>
		[DefaultValue(false), Description("Make columns variable width."), Category("Appearance")]
		internal bool VariableColumnWidths
		{
			get => variableColWidth;
			set { variableColWidth = value; ResetListLayout(nameof(VariableColumnWidths)); Refresh(); }
		}

		/// <summary>Gets the base list of items.</summary>
		/// <value>Any list supporting and <see cref="System.Collections.IList"/> interface.</value>
		protected internal abstract System.Collections.IList BaseItems { get; }

		/// <summary>Gets the background renderer for this type of control.</summary>
		/// <value>The background renderer.</value>
		protected virtual PaintBackgroundMethod BackgroundRenderer => ButtonRenderer.DrawParentBackground;

		/// <summary>Gets the required creation parameters when the control handle is created.</summary>
		/// <returns>
		/// A <see cref="T:System.Windows.Forms.CreateParams"/> that contains the required creation parameters when the handle to the control
		/// is created.
		/// </returns>
		protected override CreateParams CreateParams
		{
			get
			{
				// ReSharper disable InconsistentNaming
				const int WS_EX_CONTROLPARENT = 0x10000;
				const int WS_EX_CLIENTEDGE = 0x200;
				const int WS_BORDER = 0x800000;
				const int WS_EX_LAYOUTRTL = 0x400000;
				const int WS_EX_NOINHERITLAYOUT = 0x100000;
				const int WS_EX_RIGHT = 0x1000;
				const int WS_EX_RTLREADING = 0x2000;
				const int WS_EX_LEFTSCROLLBAR = 0x4000;
				// ReSharper restore InconsistentNaming

				var createParams = base.CreateParams;
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
				var parent = FindForm();
				var parentRightToLeftLayout = parent?.RightToLeftLayout ?? false;
				if ((RightToLeft == RightToLeft.Yes) && parentRightToLeftLayout)
				{
					createParams.ExStyle |= WS_EX_LAYOUTRTL | WS_EX_NOINHERITLAYOUT;
					createParams.ExStyle &= ~(WS_EX_RIGHT | WS_EX_RTLREADING | WS_EX_LEFTSCROLLBAR);
				}

				return createParams;
			}
		}

		/// <summary>Gets the hover item's index.</summary>
		/// <value>The hover item index.</value>
		protected virtual int HoverItem
		{
			get => hoverItem;
			set
			{
				if (hoverItem == value) return;
				hoverItemOld = hoverItem;
				hoverItem = value;
			}
		}

		protected int HoverItemOld => hoverItemOld;

		/// <summary>Gets or sets the index of the item being pressing.</summary>
		/// <value>The pressed item index.</value>
		protected virtual int PressingItem
		{
			get => pressingItem;
			set
			{
				if (pressingItem == value) return;
				pressingItemOld = pressingItem;
				pressingItem = value;
			}
		}

		protected int PressingItemOld => pressingItemOld;

		/// <summary>Retrieves the bounding rectangle for a specific item within the list control.</summary>
		/// <param name="index">The zero-based index of the item whose bounding rectangle you want to return.</param>
		/// <returns>A <see cref="Rectangle"/> that represents the bounding rectangle of the specified item.</returns>
		public Rectangle GetItemRect(int index)
		{
			if (index < 0 || index >= BaseItems.Count)
				throw new ArgumentOutOfRangeException(nameof(index));
			return MyLayoutEngine.ItemBounds[index];
		}

		/// <summary>Retrieves the size of a rectangular area into which a control can be fitted.</summary>
		/// <param name="proposedSize">The custom-sized area for a control.</param>
		/// <returns>An ordered pair of type <see cref="Size"/> representing the width and height of a rectangle.</returns>
		public override Size GetPreferredSize(Size proposedSize) => MyLayoutEngine.GetPreferredSize(this, proposedSize);

		internal void OnListChanged()
		{
			ResetListLayout("Items");
			Refresh();
		}

		/// <summary>Measures the specified item.</summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="maxSize">Maximum size of the item. Usually only constrains the width.</param>
		/// <returns>Minimum size for the item.</returns>
		protected internal abstract Size MeasureItem(Graphics g, int index, Size maxSize);

		/// <summary>Releases unmanaged and - optionally - managed resources.</summary>
		/// <param name="disposing">
		/// <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.
		/// </param>
		protected override void Dispose(bool disposing)
		{
			hoverTimer.Tick -= hoverTimer_Tick;
			hoverTimer.Dispose();
			base.Dispose(disposing);
		}

		/// <summary>Gets the item at location within the control.</summary>
		/// <param name="pt">The location.</param>
		/// <returns></returns>
		protected int GetItemAtLocation(Point pt)
		{
			for (var i = 0; i < MyLayoutEngine.ItemBounds.Count; i++)
			{
				if (MyLayoutEngine.ItemBounds[i].Contains(pt))
					return i;
			}
			return -1;
		}

		/// <summary>Gets the specified item's tooltip text.</summary>
		/// <param name="index">The index of the item.</param>
		/// <returns>Tooltip text to display. <c>null</c> or <see cref="string.Empty"/> to display no tooltip.</returns>
		protected virtual string GetItemToolTipText(int index) => null;

		/// <summary>Invalidates the specified item.</summary>
		/// <param name="index">The item index.</param>
		protected virtual void InvalidateItem(int index) => Invalidate(OffsetForScroll(MyLayoutEngine.ItemBounds[index]));//base.Invalidate();

		/// <summary>Determines whether the specified item is enabled.</summary>
		/// <param name="index">The item index.</param>
		/// <returns><c>true</c> if item is enabled; otherwise, <c>false</c>.</returns>
		protected virtual bool IsItemEnabled(int index) => true;

		/// <summary>Determines whether this list has the specified mnemonic in its members.</summary>
		/// <param name="charCode">The mnemonic character.</param>
		/// <returns><c>true</c> if list has the mnemonic; otherwise, <c>false</c>.</returns>
		protected virtual bool ListHasMnemonic(char charCode) => false;

		/// <summary>Offsets the client point for scrolling.</summary>
		/// <returns>Offset point</returns>
		protected Point OffsetForScroll(Point pt)
		{
			//System.Diagnostics.Debug.Write($"OffsetForScroll: pt={pt}; scPos={AutoScrollPosition}; ");
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

		/// <summary>Offsets the client rectangle for scrolling.</summary>
		/// <returns>Offset rectangle</returns>
		protected Rectangle OffsetForScroll(Rectangle rect)
		{
			System.Diagnostics.Debug.Write($"OffsetForScroll: rect={rect};");
			var outRect = rect;
			outRect.Offset(AutoScrollPosition);
			System.Diagnostics.Debug.WriteLine($"outrect={outRect};");
			return outRect;
		}

		/// <summary>Raises the <see cref="Control.KeyUp"/> event.</summary>
		/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (PressingItem != -1)
			{
				var ci = PressingItem;
				PressingItem = -1;
				InvalidateItem(ci);
			}
			// Handle button press on Space
			base.OnKeyUp(e);
		}

		/// <summary>Raises the <see cref="Control.MouseDown"/> event.</summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);
			var i = GetItemAtLocation(OffsetForScroll(e.Location));
			if (i == -1 || !IsItemEnabled(i))
				return;
			PressingItem = i;
			Focus();
			InvalidateItem(PressingItem);
			Update();
		}

		/// <summary>Raises the <see cref="Control.MouseEnter"/> event.</summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			mouseTracking = true;
		}

		/// <summary>Raises the <see cref="Control.MouseLeave"/> event.</summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnMouseLeave(EventArgs e)
		{
			base.OnMouseLeave(e);
			mouseTracking = false;
			SetHover(-1);
		}

		/// <summary>Raises the <see cref="Control.MouseMove"/> event.</summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			if (mouseTracking)
				SetHover(GetItemAtLocation(OffsetForScroll(e.Location)));
		}

		/// <summary>Raises the <see cref="Control.MouseUp"/> event.</summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			if (PressingItem != -1)
			{
				var ci = PressingItem;
				PressingItem = -1;
				InvalidateItem(ci);
			}
			base.OnMouseUp(e);
		}

		/// <summary>Raises the <see cref="E:PaddingChanged"/> event.</summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected override void OnPaddingChanged(EventArgs e)
		{
			base.OnPaddingChanged(e);
			ResetListLayout("Padding");
		}

		/// <summary>Raises the <see cref="Control.Paint"/> event.</summary>
		/// <param name="pe">An <see cref="PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs pe)
		{
			System.Diagnostics.Debug.WriteLine($"OnPaint: {pe.ClipRectangle}");
			var pt = AutoScrollPosition;
			pe.Graphics.TranslateTransform(pt.X, pt.Y);
			//NativeMethods.BufferedPaint.PaintAnimation(pe.Graphics, this, ClientRectangle, PaintControl, false, true, 100, pe.ClipRectangle);
			PaintControl(pe.Graphics, ClientRectangle, true, pe.ClipRectangle);
		}

		/// <summary>Raises the <see cref="E:System.Windows.Forms.ScrollableControl.Scroll"/> event.</summary>
		/// <param name="se">A <see cref="T:System.Windows.Forms.ScrollEventArgs"/> that contains the event data.</param>
		protected override void OnScroll(ScrollEventArgs se)
		{
			base.OnScroll(se);
			Invalidate();
		}

		/// <summary>Raises the <see cref="Control.StyleChanged"/> event.</summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnStyleChanged(EventArgs e)
		{
			ResetListLayout("Style");
			base.OnStyleChanged(e);
		}

		/// <summary>Paints the specified item.</summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="bounds">The bounds in which to paint the item.</param>
		/// <param name="newState">if set to <c>true</c> [new state].</param>
		protected abstract void PaintItem(Graphics g, int index, Rectangle bounds, bool newState);

		/// <summary>Processes a dialog key.</summary>
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
					var args1 = new KeyEventArgs(keyData);
					if (ProcessKey(args1))
						return true;
					break;
			}
			return base.ProcessDialogKey(keyData);
		}

		/// <summary>Processes a keyboard event.</summary>
		/// <param name="ke">The <see cref="KeyEventArgs"/> associated with the key press.</param>
		/// <returns><c>true</c> if the key was processed by the control; otherwise, <c>false</c>.</returns>
		protected virtual bool ProcessKey(KeyEventArgs ke) => false;

		/// <summary>Previews a keyboard message.</summary>
		/// <param name="m">A <see cref="Message"/>, passed by reference, that represents the window message to process.</param>
		/// <returns><c>true</c> if the key was processed by the control; otherwise, <c>false</c>.</returns>
		protected override bool ProcessKeyPreview(ref Message m)
		{
			if (m.Msg == 0x100)
			{
				var args1 = new KeyEventArgs(((Keys)((int)((long)m.WParam))) | ModifierKeys);
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
				}
			}
			else if (m.Msg == 0x101)
			{
				var args2 = new KeyEventArgs(((Keys)((int)((long)m.WParam))) | ModifierKeys);
				if (args2.KeyCode == Keys.Tab)
				{
					return ProcessKey(args2);
				}
			}

			return base.ProcessKeyPreview(ref m);
		}

		/// <summary>Processes a mnemonic.</summary>
		/// <param name="charCode">The character code.</param>
		/// <returns><c>true</c> if the mnemonic was processed by the control; otherwise, <c>false</c>.</returns>
		protected override bool ProcessMnemonic(char charCode)
		{
			if (Enabled && Visible && (Focused || ContainsFocus))
				return ListHasMnemonic(charCode);
			return false;
		}

		/// <summary>Resets the list's layout.</summary>
		/// <param name="propertyName">Name of the property forcing the layout.</param>
		protected virtual void ResetListLayout(string propertyName) => PerformLayout(this, propertyName);

		private void hoverTimer_Tick(object sender, EventArgs e)
		{
			hoverTimer.Stop();
			if (mouseTracking && timedHoverItem == HoverItem)
			{
				UpdateToolTip(GetItemToolTipText(timedHoverItem));
			}
		}

		private void OnLayoutPropertyChanged(object sender, EventArgs e) => ResetListLayout("Layout");

		private void PaintControl(Graphics graphics, Rectangle bounds, bool newState, Rectangle clip)
		{
			graphics.Clear(BackColor);
			if (Application.RenderWithVisualStyles)
				BackgroundRenderer(graphics, clip, this);

			for (var i = 0; i < BaseItems.Count; i++)
			{
				if (clip.IntersectsWith(OffsetForScroll(MyLayoutEngine.ItemBounds[i])))
					PaintItem(graphics, i, MyLayoutEngine.ItemBounds[i], newState);
			}
		}

		private void SetHover(int itemIndex)
		{
			if (itemIndex != HoverItem)
			{
				System.Diagnostics.Debug.WriteLine($"SetHover: old={HoverItem}, new={itemIndex}");
				hoverTimer.Stop();
				UpdateToolTip(null);
				var oldHover = HoverItem;
				HoverItem = itemIndex;
				// If no new item, invalidate everything
				if (HoverItem == -1)
				{
					Invalidate();
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
				Update();
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

					var position = this.MapPointToClient(Cursor.Position);
					position.Offset(0, Cursor.Current.Bounds().Bottom);
					if (RightToLeft == RightToLeft.Yes)
						position.X = Width - position.X;
					toolTip.Show(tiptext, this, position, toolTip.AutoPopDelay);
				}
			}
		}
	}
}