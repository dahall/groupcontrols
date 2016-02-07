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
		protected ButtonListBase() : base()
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
			get { return subtextFont == null ? Font : subtextFont; }
			set { if (Font.Equals(value)) subtextFont = null; else subtextFont = value; ResetListLayout("SubtextFont"); Refresh(); }
		}

		/// <summary>
		/// Gets or sets the color of an item's subtext.
		/// </summary>
		[Category("Appearance"),
		Description("The color used to display the item subtext.")]
		public Color SubtextForeColor
		{
			get { return subtextForeColor == Color.Empty ? ForeColor : subtextForeColor; }
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
		/// Gets or sets the alignment of the text in relation to the bounds of the item.
		/// </summary>
		[DefaultValue(typeof(ContentAlignment), "TopLeft"),
		Description("Alignment of the text in relation to the item."),
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
		protected int FocusedIndex => focusedIndex;

		/// <summary>
		/// Gets or sets the focused item.
		/// </summary>
		/// <value>The focused item.</value>
		protected ButtonListItem FocusedItem
		{
			get
			{
				return focusedIndex == -1 ? null : BaseItems[focusedIndex] as ButtonListItem;
			}
			set
			{
				focusedIndex = BaseItems.IndexOf(value);
			}
		}

		/// <summary>
		/// Gets the TextFormatFlags based on alignments.
		/// </summary>
		/// <value>The TextFormatFlags.</value>
		protected TextFormatFlags TextFormatFlags => RightToLeft == RightToLeft.Yes ? textFormatFlags | TextFormatFlags.RightToLeft : textFormatFlags;

		/// <summary>
		/// Ensures that the specified item is visible within the control, scrolling the contents of the control if necessary.
		/// </summary>
		/// <param name="index">The zero-based index of the item to scroll into view.</param>
		public override void EnsureVisible(int index)
		{
			Rectangle r = GetItemRect(index);
			Rectangle scrollRect = new Rectangle(-AutoScrollPosition.X, -AutoScrollPosition.Y, ClientRectangle.Size.Width, ClientRectangle.Size.Height);
			if (!scrollRect.Contains(r))
			{
				AutoScrollPosition = r.Location;
				Refresh();
			}
		}

		internal void ResetSubtextFont()
		{
			subtextFont = null;
		}

		internal bool ShouldSerializeSubtextFont() => (subtextFont != null && !subtextFont.Equals(Font));

		internal bool ShouldSerializeSubtextForeColor() => (subtextForeColor != Color.Empty && subtextForeColor != ForeColor);

		/// <summary>
		/// Focuses the next item.
		/// </summary>
		/// <param name="item">The current item.</param>
		/// <param name="forward">if set to <c>true</c>, moves to the next item, otherwise moves to the previous item.</param>
		/// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
		protected bool FocusNextItem(ButtonListItem item, bool forward)
		{
			if (BaseItems.Count > 0)
			{
				int idx = -1;
				if (item != null && (idx = BaseItems.IndexOf(item)) == -1)
					throw new ArgumentOutOfRangeException(nameof(item));
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
			int idx = startIndex == -1 && !forward ? BaseItems.Count : startIndex;
			while ((forward && ++idx < BaseItems.Count) || (!forward && --idx >= 0))
			{
				if (((ButtonListItem)BaseItems[idx]).Enabled)
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
			ButtonListItem item = BaseItems[index] as ButtonListItem;
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
			ButtonListItem item = BaseItems[index] as ButtonListItem;
			return item == null ? true : item.Enabled;
		}

		/// <summary>
		/// Measures the specified item.
		/// </summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="maxSize">Maximum size of the item. Usually only constrains the width.</param>
		/// <returns>Minimum size for the item.</returns>
		protected internal override Size MeasureItem(System.Drawing.Graphics g, int index, Size maxSize)
		{
			var chkAlign = new EnumFlagIndexer<ContentAlignment>(CheckAlign, false); // base.RtlTranslateContent(this.CheckAlign);
			var txtAlign = new EnumFlagIndexer<ContentAlignment>(TextAlign, false); // base.RtlTranslateContent(this.TextAlign);

			ButtonListItem item = BaseItems[index] as ButtonListItem;
			if (item == null)
				return Size.Empty;

			// Get glyph size
			if (imageSize == Size.Empty)
				imageSize = GetButtonSize(g);
			int glyphWithPadding = imageSize.Width + (lrPadding * 2);

			// Calculate text size
			Size textSize = new Size(maxSize.Width, Int32.MaxValue);
			if (!chkAlign[anyCenterAlignment])
				textSize.Width -= glyphWithPadding;

			Size tsz = TextRenderer.MeasureText(g, item.Text, Font, textSize, TextFormatFlags);
			item.TextRect = new Rectangle(0, 0, textSize.Width, tsz.Height);
			// NEW: item.TextRect = new Rectangle(Point.Empty, tsz);
			Size stsz = Size.Empty;
			item.SubtextRect = Rectangle.Empty;
			if (!string.IsNullOrEmpty(item.Subtext))
			{
				stsz = TextRenderer.MeasureText(g, item.Subtext, SubtextFont, textSize, TextFormatFlags);
				item.SubtextRect = new Rectangle(0, tsz.Height + subtextSeparatorHeight, textSize.Width, stsz.Height);
				// NEW: item.SubtextRect = new Rectangle(0, tsz.Height + subtextSeparatorHeight, stsz.Width, stsz.Height);
			}

			// Calculate minimum item height
			int minHeight = item.TextRect.Height;
			if (!item.SubtextRect.IsEmpty)
				minHeight += (item.SubtextRect.Height + subtextSeparatorHeight);
			int textHeight = minHeight;
			if (chkAlign[ContentAlignment.TopCenter] || chkAlign[ContentAlignment.BottomCenter])
				minHeight += (imageSize.Height + tPadding);

			// Calculate minimum item width
			int minWidth = Math.Max(tsz.Width, stsz.Width);
			if (imageSize.Width > 0 && !chkAlign[anyCenterAlignment])
				minWidth += (imageSize.Width + lrPadding);

			Size itemSize = new Size(maxSize.Width, minHeight);
			// NEW: Size itemSize = new Size(minWidth, minHeight);

			// Set relative position of glyph
			item.GlyphPosition = Point.Empty;
			if (chkAlign[anyBottomAlignment])
				item.GlyphPosition.Y = itemSize.Height - imageSize.Height;
			else if (chkAlign[anyMiddleAlignment])
				item.GlyphPosition.Y = (itemSize.Height - imageSize.Height) / 2;
			else if (chkAlign == ContentAlignment.TopCenter)
				item.OffsetText(0, imageSize.Height + tPadding);
			if (chkAlign[anyRightAlignment])
			{
				item.GlyphPosition.X = itemSize.Width - imageSize.Width;
				item.OffsetText(lrPadding, 0);
			}
			else if (chkAlign[anyCenterAlignment])
			{
				item.GlyphPosition.X = (itemSize.Width - imageSize.Width) / 2;
				if (item.TextRect.Width < item.SubtextRect.Width)
					item.TextRect.Offset((item.SubtextRect.Width - item.TextRect.Width) / 2, 0);
				else
					item.SubtextRect.Offset((item.TextRect.Width - item.SubtextRect.Width) / 2, 0);
			}
			else
			{
				item.OffsetText(imageSize.Width + lrPadding, 0);
			}

			// Set text focus rectangle
			item.FocusRect = Rectangle.Union(item.TextRect, item.SubtextRect);
			/*item.FocusRect = new Rectangle(item.TextRect.Location, new Size(Math.Min(Math.Max(tsz.Width, stsz.Width), maxSize.Width), textHeight));
			if ((txtAlign & anyCenterAlignment) != (ContentAlignment)0)
				item.FocusRect.X += (itemSize.Width - item.FocusRect.Width) / 2;
			else if ((txtAlign & anyRightAlignment) != (ContentAlignment)0)
				item.FocusRect.X = itemSize.Width - item.FocusRect.Width - imageSize.Width - lrPadding;*/

			// Adjust text position for bottom or middle
			if (txtAlign[anyBottomAlignment])
				item.OffsetText(0, itemSize.Height - item.TextRect.Height);
			else if (txtAlign[anyMiddleAlignment])
				item.OffsetText(0, (itemSize.Height - item.TextRect.Height) / 2);

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
			SubtextForeColorChanged?.Invoke(this, e);
		}

		/// <summary>
		/// Raises the <see cref="SubtextSeparatorHeightChanged"/> event.
		/// </summary>
		/// <param name="e">An <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
		protected virtual void OnSubtextSeparatorHeightChanged(PropertyChangedEventArgs e)
		{
			SubtextSeparatorHeightChanged?.Invoke(this, e);
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
			ButtonListItem li = BaseItems[index] as ButtonListItem;
			System.Diagnostics.Debug.WriteLine($"PaintItem: {Name}[{index}], Bounds=({bounds.X},{bounds.Y},{bounds.Width},{bounds.Height}), " +
				$"GlPos=({li.GlyphPosition.X},{li.GlyphPosition.Y}), TPos=({li.TextRect.X},{li.TextRect.Y},{li.TextRect.Width},{li.TextRect.Height})");

			// Draw glyph
			PaintButton(g, index, bounds);

			// Draw text
			Rectangle tr = li.TextRect;
			tr.Offset(bounds.Location);
			TextRenderer.DrawText(g, li.Text, Font, tr, li.Enabled ? ForeColor : SystemColors.GrayText, TextFormatFlags);

			Rectangle str = li.SubtextRect;
			bool hasSubtext = !string.IsNullOrEmpty(li.Subtext);
			if (hasSubtext)
			{
				str.Offset(bounds.Location);
				TextRenderer.DrawText(g, li.Subtext, SubtextFont, str, li.Enabled ? SubtextForeColor : SystemColors.GrayText, TextFormatFlags);
			}

			// Draw focus rectangle
			if (index == FocusedIndex && Focused)
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
				if (Focused)
					InvalidateItem(oldSelect);
			}
			// Set new item
			if (itemIndex > -1)
			{
				if (Focused)
					InvalidateItem(focusedIndex);
			}
		}

		private void RebuildTextFormatFlags()
		{
			ContentAlignment txtAlign = TextAlign; // base.RtlTranslateContent(this.TextAlign);
			textFormatFlags = baseTextFormatFlags;
			if ((txtAlign & anyRightAlignment) != 0)
				textFormatFlags |= TextFormatFlags.Right;
			else if ((txtAlign & anyCenterAlignment) != 0)
				textFormatFlags |= TextFormatFlags.HorizontalCenter;
		}
	}

	/// <summary>
	/// Base button item type.
	/// </summary>
	public class ButtonListItem : IEquatable<ButtonListItem>, INotifyPropertyChanged
	{
		internal Point GlyphPosition;
		internal Rectangle TextRect, SubtextRect, FocusRect;

		private bool hascheck = false;
		private bool enabled = true;
		private string text, subtext, tooltip;
		private object tag;

		/// <summary>
		/// Initializes a new instance of the <see cref="ButtonListItem"/> class.
		/// </summary>
		/// <param name="text">The text.</param>
		/// <param name="subtext">The subtext.</param>
		/// <param name="tooltipText">The tooltip text.</param>
		public ButtonListItem(string text = "", string subtext = null, string tooltipText = null)
		{
			Text = text;
			Subtext = subtext;
			ToolTipText = tooltipText;
		}

		/// <summary>
		/// Occurs when a property value has changed.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ButtonListItem"/> is checked.
		/// </summary>
		/// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
		[DefaultValue(false), Description("Indicates whether this item is checked."), Category("Appearance")]
		[BindableAttribute(true)]
		public virtual bool Checked
		{
			get { return hascheck; }
			set
			{
				if (value != hascheck)
				{
					hascheck = value;
					OnNotifyPropertyChanged(nameof(Checked));
				}
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="ButtonListItem"/> is enabled.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		[DefaultValue(true), Category("Behavior")]
		[BindableAttribute(true)]
		public bool Enabled
		{
			get { return enabled; }
			set
			{
				if (value != enabled)
				{
					enabled = value;
					OnNotifyPropertyChanged(nameof(Enabled));
				}
			}
		}

		/// <summary>
		/// Gets or sets the subtext.
		/// </summary>
		/// <value>The subtext.</value>
		[DefaultValue(null), Category("Appearance")]
		[BindableAttribute(true)]
		public string Subtext
		{
			get { return subtext; }
			set
			{
				if (value != subtext)
				{
					subtext = value;
					OnNotifyPropertyChanged(nameof(Subtext));
				}
			}
		}

		/// <summary>
		/// Gets or sets the tag.
		/// </summary>
		/// <value>The tag.</value>
		[DefaultValue((object)null), Category("Data")]
		[BindableAttribute(true)]
		[TypeConverterAttribute(typeof(StringConverter))]
		public object Tag
		{
			get { return tag; }
			set
			{
				if (value != tag)
				{
					tag = value;
					OnNotifyPropertyChanged(nameof(Tag));
				}
			}
		}

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>The text.</value>
		[DefaultValue(""), Category("Appearance")]
		[BindableAttribute(true)]
		public string Text
		{
			get { return text; }
			set
			{
				if (value != text)
				{
					text = value;
					OnNotifyPropertyChanged(nameof(Text));
				}
			}
		}

		/// <summary>
		/// Gets or sets the tool tip text.
		/// </summary>
		/// <value>The tool tip text.</value>
		[DefaultValue(null), Category("Appearance")]
		[BindableAttribute(true)]
		public string ToolTipText
		{
			get { return tooltip; }
			set
			{
				if (value != tooltip)
				{
					tooltip = value;
					OnNotifyPropertyChanged(nameof(ToolTipText));
				}
			}
		}

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
			var bli = obj as ButtonListItem;
			if (obj == null || bli == null)
				return false;
			return Equals(bli);
		}

		/// <summary>
		/// Determines whether the specified <see cref="ButtonListItem"/> is equal to the current <see cref="ButtonListItem"/>.
		/// </summary>
		/// <param name="other">The <see cref="ButtonListItem"/> to compare with the current <see cref="ButtonListItem"/>.</param>
		/// <returns>
		/// true if the specified <see cref="ButtonListItem"/> is equal to the current <see cref="ButtonListItem"/>; otherwise, false.
		/// </returns>
		public bool Equals(ButtonListItem other)
		{
			if (other == null) return false;
			return (Checked == other.Checked) && (Enabled == other.Enabled) &&
				(Subtext == other.Subtext) && (Text == other.Text) && (ToolTipText == other.ToolTipText);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode() => new { A = hascheck, B = enabled, C = text, D = subtext, E = tooltip }.GetHashCode();

		/// <summary>
		/// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
		/// </returns>
		public override string ToString() => System.Text.RegularExpressions.Regex.Replace(Text, @"\&([^\&])", "$1");

		/// <summary>
		/// Called when a property value has changed.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		protected virtual void OnNotifyPropertyChanged(string propertyName)
		{
			var h = PropertyChanged;
			if (h != null)
				h(this, new PropertyChangedEventArgs(propertyName));
		}

		internal void OffsetText(int x, int y)
		{
			TextRect.Offset(x, y);
			SubtextRect.Offset(x, y);
		}
	}
}