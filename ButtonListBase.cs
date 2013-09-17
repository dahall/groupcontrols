using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GroupControls
{
	/// <summary>
	/// Abstract base class for lists of owner-drawn buttons.
	/// </summary>
	[DefaultProperty("Items")]
	public abstract class ButtonListBase : ControlListBase
	{
		internal const int lrPadding = 3, tPadding = 2;

		private const TextFormatFlags baseTextFormatFlags = TextFormatFlags.WordBreak | TextFormatFlags.PreserveGraphicsTranslateTransform;

		private ContentAlignment checkAlign = ContentAlignment.TopLeft;
		private int focusedIndex = -1;
		private Size imageSize = Size.Empty;
		private Font subtextFont;
		private Color subtextForeColor = Color.Empty;
		private int subtextSeparatorHeight = 5;
		private ContentAlignment textAlign = ContentAlignment.TopLeft;
		private TextFormatFlags textFormatFlags = baseTextFormatFlags;

		/// <summary>
		/// Initializes a new instance of the <see cref="ButtonListBase"/> class.
		/// </summary>
		public ButtonListBase() : base()
		{
		}

		/// <summary>
		/// Occurs when SubtextForeColor changed.
		/// </summary>
		[Category("Property Changed"), Description("Occurs when the value of the SubtextForeColor property changes.")]
		public event PropertyChangedEventHandler SubtextForeColorChanged;

		/// <summary>
		/// Occurs when SubtextSeparatorHeight changed.
		/// </summary>
		[Category("Property Changed"), Description("Occurs when the value of the SubtextSeparatorHeight property changes.")]
		public event PropertyChangedEventHandler SubtextSeparatorHeightChanged;

		/// <summary>
		/// Gets or sets the alignment of the check box in relation to the text.
		/// </summary>
		[DefaultValue(typeof(ContentAlignment), "TopLeft"), Category("Appearance"), Localizable(true)]
		[Description("The alignment of the check box in relation to the text.")]
		public ContentAlignment CheckAlign
		{
			get { return checkAlign; }
			set { checkAlign = value; ResetListLayout("CheckAlign"); Refresh(); }
		}

		/// <summary>
		/// Gets or sets the font used to render the subtext of each item.
		/// </summary>
		[Description("The font used to display the item subtext."),
		Category("Appearance")]
		public Font SubtextFont
		{
			get { return subtextFont == null ? this.Font : subtextFont; }
			set { if (this.Font.Equals(value)) subtextFont = null; else subtextFont = value; ResetListLayout("SubtextFont"); Refresh(); }
		}

		/// <summary>
		/// Gets or sets the color of an item's subtext.
		/// </summary>
		[Category("Appearance"),
		Description("The color used to display the item subtext.")]
		public Color SubtextForeColor
		{
			get { return subtextForeColor == Color.Empty ? this.ForeColor : subtextForeColor; }
			set { subtextForeColor = value; OnSubtextForeColorChanged(new PropertyChangedEventArgs("SubtextForeColor")); Refresh(); }
		}

		/// <summary>
		/// Gets or sets the number of pixels used to separate the text from the subtext within an item.
		/// </summary>
		[DefaultValue(5),
		Description("Number of pixels separating item text and subtext."),
		Category("Appearance")]
		public int SubtextSeparatorHeight
		{
			get { return subtextSeparatorHeight; }
			set { subtextSeparatorHeight = value; OnSubtextSeparatorHeightChanged(new PropertyChangedEventArgs("SubtextSeparatorHeight")); ResetListLayout("SubtextSeparatorHeight"); Refresh(); }
		}

		/// <summary>
		/// Text for the control. This property is not available for this control.
		/// </summary>
		[Browsable(false)]
		public override string Text
		{
			get { return base.Text; }
			set { base.Text = value; }
		}

		/// <summary>
		/// Gest or sets the alignment of the text in relation to the bounds of the item.
		/// </summary>
		[DefaultValue(typeof(ContentAlignment), "TopLeft"),
		Description("Aligment of the text in relation to the item."),
		Category("Appearance"),
		Localizable(true)]
		public ContentAlignment TextAlign
		{
			get { return textAlign; }
			set
			{
				textAlign = value;
				RebuildTextFormatFlags();
				ResetListLayout("TextAlign");
				Refresh();
			}
		}

		/// <summary>
		/// Gets the index of the focused item.
		/// </summary>
		/// <value>The index of the focused item.</value>
		protected int FocusedIndex
		{
			get { return focusedIndex; }
		}

		/// <summary>
		/// Gets or sets the focused item.
		/// </summary>
		/// <value>The focused item.</value>
		protected ButtonListItem FocusedItem
		{
			get
			{
				return this.focusedIndex == -1 ? null : this.BaseItems[this.focusedIndex] as ButtonListItem;
			}
			set
			{
				focusedIndex = this.BaseItems.IndexOf(value);
			}
		}

		/// <summary>
		/// Gets the TextFormatFlags based on alignments.
		/// </summary>
		/// <value>The TextFormatFlags.</value>
		protected TextFormatFlags TextFormatFlags
		{
			get { return this.RightToLeft == RightToLeft.Yes ? textFormatFlags | TextFormatFlags.RightToLeft : textFormatFlags; }
		}

		/// <summary>
		/// Ensures that the specified item is visible within the control, scrolling the contents of the control if necessary.
		/// </summary>
		/// <param name="index">The zero-based index of the item to scroll into view.</param>
		public override void EnsureVisible(int index)
		{
			Rectangle r = GetItemRect(index);
			Rectangle scrollRect = new Rectangle(-this.AutoScrollPosition.X, -this.AutoScrollPosition.Y, this.ClientRectangle.Size.Width, this.ClientRectangle.Size.Height);
			if (!scrollRect.Contains(r))
			{
				this.AutoScrollPosition = r.Location;
				Refresh();
			}
		}

		internal void ResetSubtextFont()
		{
			subtextFont = null;
		}

		internal bool ShouldSerializeSubtextFont()
		{
			return (subtextFont != null && !subtextFont.Equals(this.Font));
		}

		internal bool ShouldSerializeSubtextForeColor()
		{
			return (subtextForeColor != Color.Empty && subtextForeColor != this.ForeColor);
		}

		/// <summary>
		/// Focuses the next item.
		/// </summary>
		/// <param name="i">The current item.</param>
		/// <param name="forward">if set to <c>true</c>, moves to the next item, otherwise moves to the previous item.</param>
		/// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
		protected bool FocusNextItem(ButtonListItem i, bool forward)
		{
			if (this.BaseItems.Count > 0)
			{
				int idx = -1;
				if (i != null && (idx = this.BaseItems.IndexOf(i)) == -1)
					throw new IndexOutOfRangeException();
				idx = GetNextEnabledItemIndex(idx, forward);
				if (idx != -1)
				{
					SetFocused(idx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets the index of the next enabled item.
		/// </summary>
		/// <param name="startIndex">The start index.</param>
		/// <param name="forward">if set to <c>true</c> find subsequent item, prior item if <c>false</c>.</param>
		/// <returns>Index of next enabled item, or -1 if not found.</returns>
		protected int GetNextEnabledItemIndex(int startIndex, bool forward)
		{
			int idx = startIndex == -1 && !forward ? this.BaseItems.Count : startIndex;
			while ((forward && ++idx < this.BaseItems.Count) || (!forward && --idx >= 0))
			{
				if (((ButtonListItem)this.BaseItems[idx]).Enabled)
					return idx;
			}
			return -1;
		}

		/// <summary>
		/// Gets the size of the image used to display the button.
		/// </summary>
		/// <param name="g">Current <see cref="Graphics"/> context.</param>
		/// <returns>The size of the image.</returns>
		protected abstract Size GetButtonSize(Graphics g);

		/// <summary>
		/// Gets the specified item's tooltip text.
		/// </summary>
		/// <param name="index">The index of the item.</param>
		/// <returns>
		/// Tooltip text to display. <c>null</c> or <see cref="String.Empty"/> to display no tooltip.
		/// </returns>
		protected override string GetItemToolTipText(int index)
		{
			ButtonListItem item = this.BaseItems[index] as ButtonListItem;
			return item == null ? string.Empty : item.ToolTipText;
		}

		/// <summary>
		/// Determines whether the specified item is enabled.
		/// </summary>
		/// <param name="index">The item index.</param>
		/// <returns>
		/// 	<c>true</c> if item is enabled; otherwise, <c>false</c>.
		/// </returns>
		protected override bool IsItemEnabled(int index)
		{
			ButtonListItem item = this.BaseItems[index] as ButtonListItem;
			return item == null ? true : item.Enabled;
		}

		/// <summary>
		/// Measures the specified item.
		/// </summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="maxSize">Maximum size of the item. Usually only constrains the width.</param>
		/// <returns>Minimum size for the item.</returns>
		protected override Size MeasureItem(System.Drawing.Graphics g, int index, Size maxSize)
		{
			ContentAlignment chkAlign = this.CheckAlign; // base.RtlTranslateContent(this.CheckAlign);
			ContentAlignment txtAlign = this.TextAlign; // base.RtlTranslateContent(this.TextAlign);

			ButtonListItem item = this.BaseItems[index] as ButtonListItem;
			if (item == null)
				return Size.Empty;

			// Get glyph size
			if (imageSize == Size.Empty)
				imageSize = GetButtonSize(g);
			int glyphWithPadding = imageSize.Width + (lrPadding * 2);

			// Calculate text size
			Size textSize = new Size(maxSize.Width, Int32.MaxValue);
			if ((chkAlign & anyCenterAlignment) == (ContentAlignment)0)
				textSize.Width -= glyphWithPadding;

			Size tsz = TextRenderer.MeasureText(g, item.Text, this.Font, textSize, TextFormatFlags);
			item.TextRect = new Rectangle(0, 0, textSize.Width, tsz.Height);
			Size stsz = Size.Empty;
			item.SubtextRect = Rectangle.Empty;
			if (!string.IsNullOrEmpty(item.Subtext))
			{
				stsz = TextRenderer.MeasureText(g, item.Subtext, this.SubtextFont, textSize, TextFormatFlags);
				item.SubtextRect = new Rectangle(0, tsz.Height + subtextSeparatorHeight, textSize.Width, stsz.Height);
			}

			// Calculate minimum item height
			int minHeight = item.TextRect.Height;
			if (!item.SubtextRect.IsEmpty)
				minHeight += (item.SubtextRect.Height + subtextSeparatorHeight);
			int textHeight = minHeight;
			if ((chkAlign == ContentAlignment.TopCenter) || (chkAlign == ContentAlignment.BottomCenter))
				minHeight += (imageSize.Height + tPadding);

			Size itemSize = new Size(maxSize.Width, minHeight);

			// Set relative position of glyph
			item.GlyphPosition = Point.Empty;
			if ((chkAlign & anyBottomAlignment) != (ContentAlignment)0)
			{
				item.GlyphPosition.Y = itemSize.Height - imageSize.Height;
			}
			else if ((chkAlign & anyMiddleAlignment) != (ContentAlignment)0)
			{
				item.GlyphPosition.Y = (itemSize.Height - imageSize.Height) / 2;
			}
			else
			{
				if (chkAlign == ContentAlignment.TopCenter)
					item.OffsetText(0, imageSize.Height + tPadding);
			}
			if ((chkAlign & anyRightAlignment) != (ContentAlignment)0)
			{
				item.GlyphPosition.X = itemSize.Width - imageSize.Width;
				item.OffsetText(lrPadding, 0);
			}
			else if ((chkAlign & anyCenterAlignment) != (ContentAlignment)0)
			{
				item.GlyphPosition.X = (itemSize.Width - imageSize.Width) / 2;
			}
			else
			{
				item.OffsetText(imageSize.Width + lrPadding, 0);
			}

			// Set text focus rectangle
			item.FocusRect = new Rectangle(item.TextRect.Location, new Size(Math.Min(Math.Max(tsz.Width, stsz.Width), maxSize.Width), textHeight));
			if ((txtAlign & anyCenterAlignment) != (ContentAlignment)0)
				item.FocusRect.X += (itemSize.Width - item.FocusRect.Width) / 2;
			else if ((txtAlign & anyRightAlignment) != (ContentAlignment)0)
				item.FocusRect.X = itemSize.Width - item.FocusRect.Width - imageSize.Width - lrPadding;

			// Adjust text position for bottom or middle
			/*if ((txtAlign & anyBottomAlignment) != (ContentAlignment)0)
				item.OffsetText(0, itemSize.Height - item.TextRect.Height);
			else if ((txtAlign & anyMiddleAlignment) != (ContentAlignment)0)
				item.OffsetText(0, (itemSize.Height - item.TextRect.Height) / 2);*/

			return itemSize;
		}

		/// <summary>
		/// Raises the <see cref="Control.GotFocus"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (focusedIndex != -1)
			{
				EnsureVisible(focusedIndex);
				InvalidateItem(focusedIndex);
			}
		}

		/// <summary>
		/// Raises the <see cref="Control.LostFocus"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			if (focusedIndex != -1)
				InvalidateItem(focusedIndex);
		}

		/// <summary>
		/// Raises the <see cref="Control.RightToLeftChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnRightToLeftChanged(EventArgs e)
		{
			base.OnRightToLeftChanged(e);
			RebuildTextFormatFlags();
		}

		/// <summary>
		/// Raises the <see cref="SubtextForeColorChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
		protected virtual void OnSubtextForeColorChanged(PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler1 = this.SubtextForeColorChanged;
			if (handler1 != null)
				handler1(this, e);
		}

		/// <summary>
		/// Raises the <see cref="SubtextSeparatorHeightChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
		protected virtual void OnSubtextSeparatorHeightChanged(PropertyChangedEventArgs e)
		{
			PropertyChangedEventHandler handler1 = this.SubtextSeparatorHeightChanged;
			if (handler1 != null)
				handler1(this, e);
		}

		/// <summary>
		/// Paints the button.
		/// </summary>
		/// <param name="g">A <see cref="Graphics" /> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="bounds">The bounds in which to paint the item.</param>
		protected abstract void PaintButton(Graphics g, int index, Rectangle bounds);

		/// <summary>
		/// Paints the specified item.
		/// </summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="bounds">The bounds in which to paint the item.</param>
		protected override void PaintItem(System.Drawing.Graphics g, int index, Rectangle bounds)
		{
			ButtonListItem li = this.BaseItems[index] as ButtonListItem;
			System.Diagnostics.Debug.WriteLine(string.Format("PaintItem: {0}[{1}], Bounds=({2},{3},{4},{5}), GlPos=({6},{7}), TPos=({8},{9},{10},{11})", this.Name, index,
				bounds.X, bounds.Y, bounds.Width, bounds.Height, li.GlyphPosition.X, li.GlyphPosition.Y, li.TextRect.X, li.TextRect.Y, li.TextRect.Width, li.TextRect.Height));

			// Draw glyph
			PaintButton(g, index, bounds);

			// Draw text
			Rectangle tr = li.TextRect;
			tr.Offset(bounds.Location);
			TextRenderer.DrawText(g, li.Text, this.Font, tr, li.Enabled ? this.ForeColor : SystemColors.GrayText, TextFormatFlags);

			Rectangle str = li.SubtextRect;
			bool hasSubtext = !string.IsNullOrEmpty(li.Subtext);
			if (hasSubtext)
			{
				str.Offset(bounds.Location);
				TextRenderer.DrawText(g, li.Subtext, this.SubtextFont, str, li.Enabled ? this.SubtextForeColor : SystemColors.GrayText, TextFormatFlags);
			}

			// Draw focus rect
			if (index == FocusedIndex && this.Focused)
			{
				Rectangle fr = li.FocusRect;
				fr.Offset(bounds.Location);
				ControlPaint.DrawFocusRectangle(g, fr);
			}
		}

		/// <summary>
		/// Sets the focus to the specified item.
		/// </summary>
		/// <param name="itemIndex">Index of the item.</param>
		protected void SetFocused(int itemIndex)
		{
			if (itemIndex != -1 && !IsItemEnabled(itemIndex))
				return;

			int oldSelect = focusedIndex;
			focusedIndex = itemIndex;
			// clear old selected item
			if (oldSelect > -1)
			{
				if (this.Focused)
					InvalidateItem(oldSelect);
			}
			// Set new item
			if (itemIndex > -1)
			{
				if (this.Focused)
					InvalidateItem(focusedIndex);
			}
		}

		private void RebuildTextFormatFlags()
		{
			ContentAlignment txtAlign = this.TextAlign; // base.RtlTranslateContent(this.TextAlign);
			textFormatFlags = baseTextFormatFlags;
			if ((txtAlign & anyRightAlignment) != (ContentAlignment)0)
				textFormatFlags |= TextFormatFlags.Right;
			else if ((txtAlign & anyCenterAlignment) != (ContentAlignment)0)
				textFormatFlags |= TextFormatFlags.HorizontalCenter;
		}
	}

	/// <summary>
	/// Base button item type.
	/// </summary>
	public class ButtonListItem : IEquatable<ButtonListItem>
	{
		internal Point GlyphPosition;
		internal Rectangle TextRect, SubtextRect, FocusRect;

		/// <summary>
		/// Initializes a new instance of the <see cref="ButtonListItem"/> class.
		/// </summary>
		public ButtonListItem()
		{
			this.Enabled = true;
			this.Checked = false;
			this.Text = string.Empty;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ButtonListItem"/> class.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="subtext">The subtext.</param>
		/// <param name="tooltiptext">The tooltip text.</param>
		public ButtonListItem(string text, string subtext = null, string tooltiptext = null)
			: this()
		{
			this.Text = text;
			this.Subtext = subtext;
			this.ToolTipText = tooltiptext;
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ButtonListItem"/> is checked.
		/// </summary>
		/// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
		[DefaultValue(false), Description("Indicates whether this item is checked."), Category("Appearance")]
		[BindableAttribute(true)]
		public virtual bool Checked { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ButtonListItem"/> is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		[DefaultValue(true), Category("Behavior")]
		[BindableAttribute(true)]
		public bool Enabled { get; set; }

		/// <summary>
		/// Gets or sets the subtext.
		/// </summary>
		/// <value>The subtext.</value>
		[DefaultValue((string)null), Category("Appearance")]
		[BindableAttribute(true)]
		public string Subtext { get; set; }

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <value>The tag.</value>
		[DefaultValue((object)null), Category("Data")]
		[BindableAttribute(true)]
		[TypeConverterAttribute(typeof(StringConverter))]
		public object Tag { get; set; }

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		[DefaultValue(""), Category("Appearance")]
		[BindableAttribute(true)]
		public string Text { get; set; }

		/// <summary>
		/// Gets or sets the tool tip text.
		/// </summary>
		/// <value>The tool tip text.</value>
		[DefaultValue((string)null), Category("Appearance")]
		[BindableAttribute(true)]
		public string ToolTipText { get; set; }

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
		/// <returns>
		/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
		/// </returns>
		/// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is ButtonListItem))
				return false;
			return this.Equals(obj as ButtonListItem);
		}

		/// <summary>
		/// Determines whether the specified <see cref="ButtonListItem"/> is equal to the current <see cref="ButtonListItem"/>.
		/// </summary>
		/// <param name="b2">The <see cref="ButtonListItem"/> to compare with the current <see cref="ButtonListItem"/>.</param>
		/// <returns>
		/// true if the specified <see cref="ButtonListItem"/> is equal to the current <see cref="ButtonListItem"/>; otherwise, false.
		/// </returns>
		public bool Equals(ButtonListItem b2)
		{
			if (b2 == null) return false;
			return (this.Checked == b2.Checked) && (this.Enabled == b2.Enabled) &&
				(this.Subtext == b2.Subtext) && (this.Text == b2.Text) && (this.ToolTipText == b2.ToolTipText);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			int ret = this.Checked.GetHashCode() ^ this.Enabled.GetHashCode() ^ this.Text.GetHashCode();
			if (this.Subtext != null) ret ^= this.Subtext.GetHashCode();
			if (this.ToolTipText != null) ret ^= this.ToolTipText.GetHashCode();
			return ret;
		}

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString()
		{
			return System.Text.RegularExpressions.Regex.Replace(this.Text, @"\&([^\&])", "$1");
		}

		internal void OffsetText(int x, int y)
		{
			this.TextRect.Offset(x, y);
			this.SubtextRect.Offset(x, y);
		}
	}
}