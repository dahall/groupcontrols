using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GroupControls;

/// <summary>Abstract base class for lists of owner-drawn buttons.</summary>
[DefaultProperty("Items")]
public abstract class ButtonListBase<T> : ControlListBase
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

	/// <summary>Occurs when SubtextForeColor changed.</summary>
	[Category("Property Changed"), Description("Occurs when the value of the SubtextForeColor property changes.")]
	public event PropertyChangedEventHandler SubtextForeColorChanged;

	/// <summary>Occurs when SubtextSeparatorHeight changed.</summary>
	[Category("Property Changed"), Description("Occurs when the value of the SubtextSeparatorHeight property changes.")]
	public event PropertyChangedEventHandler SubtextSeparatorHeightChanged;

	/// <summary>Gets or sets the alignment of the check box in relation to the text.</summary>
	[DefaultValue(typeof(ContentAlignment), "TopLeft"), Category("Appearance"), Localizable(true)]
	[Description("The alignment of the check box in relation to the text.")]
	public ContentAlignment CheckAlign
	{
		get => checkAlign;
		set { checkAlign = value; ResetListLayout(nameof(CheckAlign)); Refresh(); }
	}

	/// <summary>Gets or sets the font used to render the subtext of each item.</summary>
	[Description("The font used to display the item subtext."),
	Category("Appearance")]
	public Font SubtextFont
	{
		get => subtextFont ?? Font;
		set { subtextFont = Font.Equals(value) ? null : value; ResetListLayout(nameof(SubtextFont)); Refresh(); }
	}

	/// <summary>Gets or sets the color of an item's subtext.</summary>
	[Category("Appearance"),
	Description("The color used to display the item subtext.")]
	public Color SubtextForeColor
	{
		get => subtextForeColor == Color.Empty ? ForeColor : subtextForeColor;
		set { subtextForeColor = value; OnSubtextForeColorChanged(new PropertyChangedEventArgs(nameof(SubtextForeColor))); Refresh(); }
	}

	/// <summary>Gets or sets the number of pixels used to separate the text from the subtext within an item.</summary>
	[DefaultValue(5),
	Description("Number of pixels separating item text and subtext."),
	Category("Appearance")]
	public int SubtextSeparatorHeight
	{
		get => subtextSeparatorHeight;
		set { subtextSeparatorHeight = value; OnSubtextSeparatorHeightChanged(new PropertyChangedEventArgs(nameof(SubtextSeparatorHeight))); ResetListLayout(nameof(SubtextSeparatorHeight)); Refresh(); }
	}

	/// <summary>Text for the control. This property is not available for this control.</summary>
	[Browsable(false)]
	public override string Text
	{
		get => base.Text;
		set => base.Text = value;
	}

	/// <summary>Gets or sets the alignment of the text in relation to the bounds of the item.</summary>
	[DefaultValue(typeof(ContentAlignment), "TopLeft"),
	Description("Alignment of the text in relation to the item."),
	Category("Appearance"),
	Localizable(true)]
	public ContentAlignment TextAlign
	{
		get => textAlign;
		set
		{
			textAlign = value;
			RebuildTextFormatFlags();
			ResetListLayout(nameof(TextAlign));
			Refresh();
		}
	}

	/// <summary>Gets the index of the focused item.</summary>
	/// <value>The index of the focused item.</value>
	protected int FocusedIndex => focusedIndex;

	/// <summary>Gets or sets the focused item.</summary>
	/// <value>The focused item.</value>
	protected ButtonListItem<T> FocusedItem
	{
		get => focusedIndex == -1 ? null : BaseItems[focusedIndex] as ButtonListItem<T>;
		set => focusedIndex = BaseItems.IndexOf(value);
	}

	/// <summary>Gets the TextFormatFlags based on alignments.</summary>
	/// <value>The TextFormatFlags.</value>
	protected TextFormatFlags TextFormatFlags => RightToLeft == RightToLeft.Yes ? textFormatFlags | TextFormatFlags.RightToLeft : textFormatFlags;

	/// <summary>Ensures that the specified item is visible within the control, scrolling the contents of the control if necessary.</summary>
	/// <param name="index">The zero-based index of the item to scroll into view.</param>
	public virtual void EnsureVisible(int index)
	{
		var r = GetItemRect(index);
		var scrollRect = new Rectangle(-AutoScrollPosition.X, -AutoScrollPosition.Y, ClientRectangle.Size.Width, ClientRectangle.Size.Height);
		if (scrollRect.Contains(r)) return;
		AutoScrollPosition = r.Location;
		Refresh();
	}

	internal void ResetSubtextFont() => subtextFont = null;

	internal bool ShouldSerializeSubtextFont() => subtextFont != null && !subtextFont.Equals(Font);

	internal bool ShouldSerializeSubtextForeColor() => subtextForeColor != Color.Empty && subtextForeColor != ForeColor;

	/// <summary>Measures the specified item.</summary>
	/// <param name="g">A <see cref="Graphics"/> reference.</param>
	/// <param name="index">The index of the item.</param>
	/// <param name="maxSize">Maximum size of the item. Usually only constrains the width.</param>
	/// <returns>Minimum size for the item.</returns>
	protected internal override Size MeasureItem(Graphics g, int index, Size maxSize)
	{
		var chkAlign = new EnumFlagIndexer<ContentAlignment>(CheckAlign, false); // base.RtlTranslateContent(this.CheckAlign);
		var txtAlign = new EnumFlagIndexer<ContentAlignment>(TextAlign, false); // base.RtlTranslateContent(this.TextAlign);

		var item = BaseItems[index] as ButtonListItem<T>;
		if (item == null)
			return Size.Empty;

		// Get glyph size
		if (imageSize == Size.Empty)
			imageSize = GetButtonSize(g);
		var glyphWithPadding = imageSize.Width + lrPadding * 2;

		// Calculate text size
		var textSize = new Size(maxSize.Width, int.MaxValue);
		if (!chkAlign[anyCenterAlignment])
			textSize.Width -= glyphWithPadding;

		var tsz = TextRenderer.MeasureText(g, item.Text, Font, textSize, TextFormatFlags);
		item.TextRect = new Rectangle(0, 0, textSize.Width, tsz.Height);
		// NEW: item.TextRect = new Rectangle(Point.Empty, tsz);
		var stsz = Size.Empty;
		item.SubtextRect = Rectangle.Empty;
		if (!string.IsNullOrEmpty(item.Subtext))
		{
			stsz = TextRenderer.MeasureText(g, item.Subtext, SubtextFont, textSize, TextFormatFlags);
			item.SubtextRect = new Rectangle(0, tsz.Height + subtextSeparatorHeight, textSize.Width, stsz.Height);
			// NEW: item.SubtextRect = new Rectangle(0, tsz.Height + subtextSeparatorHeight, stsz.Width, stsz.Height);
		}

		// Calculate minimum item height
		var minHeight = item.TextRect.Height;
		if (!item.SubtextRect.IsEmpty)
			minHeight += item.SubtextRect.Height + subtextSeparatorHeight;
		var textHeight = minHeight;
		if (chkAlign[ContentAlignment.TopCenter] || chkAlign[ContentAlignment.BottomCenter])
			minHeight += imageSize.Height + tPadding;

		// Calculate minimum item width
		var minWidth = Math.Max(tsz.Width, stsz.Width);
		if (imageSize.Width > 0 && !chkAlign[anyCenterAlignment])
			minWidth += imageSize.Width + lrPadding;

		var itemSize = new Size(maxSize.Width, minHeight);
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

	/// <summary>Focuses the next item.</summary>
	/// <param name="item">The current item.</param>
	/// <param name="forward">if set to <c>true</c>, moves to the next item, otherwise moves to the previous item.</param>
	/// <returns><c>true</c> on success, <c>false</c> otherwise.</returns>
	protected bool FocusNextItem(ButtonListItem<T> item, bool forward)
	{
		if (BaseItems.Count > 0)
		{
			var idx = -1;
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

	/// <summary>Gets the size of the image used to display the button.</summary>
	/// <param name="g">Current <see cref="Graphics"/> context.</param>
	/// <returns>The size of the image.</returns>
	protected abstract Size GetButtonSize(Graphics g);

	/// <summary>Gets the specified item's tooltip text.</summary>
	/// <param name="index">The index of the item.</param>
	/// <returns>Tooltip text to display. <c>null</c> or <see cref="string.Empty"/> to display no tooltip.</returns>
	protected override string GetItemToolTipText(int index)
	{
		var item = BaseItems[index] as ButtonListItem<T>;
		return item == null ? string.Empty : item.ToolTipText;
	}

	/// <summary>Gets the index of the next enabled item.</summary>
	/// <param name="startIndex">The start index.</param>
	/// <param name="forward">if set to <c>true</c> find subsequent item, prior item if <c>false</c>.</param>
	/// <returns>Index of next enabled item, or -1 if not found.</returns>
	protected int GetNextEnabledItemIndex(int startIndex, bool forward)
	{
		var idx = startIndex == -1 && !forward ? BaseItems.Count : startIndex;
		while ((forward && ++idx < BaseItems.Count) || (!forward && --idx >= 0))
		{
			if (((ButtonListItem<T>)BaseItems[idx]).Enabled)
				return idx;
		}
		return -1;
	}

	/// <summary>Determines whether the specified item is enabled.</summary>
	/// <param name="index">The item index.</param>
	/// <returns><c>true</c> if item is enabled; otherwise, <c>false</c>.</returns>
	protected override bool IsItemEnabled(int index)
	{
		var item = BaseItems[index] as ButtonListItem<T>;
		return item == null || item.Enabled;
	}

	/// <summary>Raises the <see cref="Control.GotFocus"/> event.</summary>
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

	/// <summary>Raises the <see cref="Control.LostFocus"/> event.</summary>
	/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		if (focusedIndex != -1)
			InvalidateItem(focusedIndex);
	}

	/// <summary>Raises the <see cref="Control.RightToLeftChanged"/> event.</summary>
	/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
	protected override void OnRightToLeftChanged(EventArgs e)
	{
		base.OnRightToLeftChanged(e);
		RebuildTextFormatFlags();
	}

	/// <summary>Raises the <see cref="SubtextForeColorChanged"/> event.</summary>
	/// <param name="e">An <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
	protected virtual void OnSubtextForeColorChanged(PropertyChangedEventArgs e) => SubtextForeColorChanged?.Invoke(this, e);

	/// <summary>Raises the <see cref="SubtextSeparatorHeightChanged"/> event.</summary>
	/// <param name="e">An <see cref="PropertyChangedEventArgs"/> that contains the event data.</param>
	protected virtual void OnSubtextSeparatorHeightChanged(PropertyChangedEventArgs e) => SubtextSeparatorHeightChanged?.Invoke(this, e);

	/// <summary>Paints the button.</summary>
	/// <param name="g">A <see cref="Graphics"/> reference.</param>
	/// <param name="index">The index of the item.</param>
	/// <param name="bounds">The bounds in which to paint the item.</param>
	/// <param name="newState">Then new state of the button.</param>
	protected abstract void PaintButton(Graphics g, int index, Rectangle bounds, bool newState);

	/// <summary>Paints the specified item.</summary>
	/// <param name="g">A <see cref="Graphics"/> reference.</param>
	/// <param name="index">The index of the item.</param>
	/// <param name="bounds">The bounds in which to paint the item.</param>
	/// <param name="newState">Then new state of the button.</param>
	protected override void PaintItem(Graphics g, int index, Rectangle bounds, bool newState)
	{
		var li = BaseItems[index] as ButtonListItem<T>;
		if (li == null) throw new ArgumentOutOfRangeException(nameof(index));
		System.Diagnostics.Debug.WriteLine($"PaintItem: {Name}[{index}], Bounds=({bounds.X},{bounds.Y},{bounds.Width},{bounds.Height}), " +
			$"GlPos=({li.GlyphPosition.X},{li.GlyphPosition.Y}), TPos=({li.TextRect.X},{li.TextRect.Y},{li.TextRect.Width},{li.TextRect.Height})");

		// Draw glyph
		PaintButton(g, index, bounds, newState);

		// Draw text
		var tr = li.TextRect;
		tr.Offset(bounds.Location);
		TextRenderer.DrawText(g, li.Text, Font, tr, li.Enabled ? ForeColor : SystemColors.GrayText, TextFormatFlags);

		var str = li.SubtextRect;
		var hasSubtext = !string.IsNullOrEmpty(li.Subtext);
		if (hasSubtext)
		{
			str.Offset(bounds.Location);
			TextRenderer.DrawText(g, li.Subtext, SubtextFont, str, li.Enabled ? SubtextForeColor : SystemColors.GrayText, TextFormatFlags);
		}

		// Draw focus rectangle
		if (index == FocusedIndex && Focused)
		{
			var fr = li.FocusRect;
			fr.Offset(bounds.Location);
			ControlPaint.DrawFocusRectangle(g, fr);
		}
	}

	/// <summary>Sets the focus to the specified item.</summary>
	/// <param name="itemIndex">Index of the item.</param>
	protected void SetFocused(int itemIndex)
	{
		if (itemIndex != -1 && !IsItemEnabled(itemIndex))
			return;

		var oldSelect = focusedIndex;
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
		var txtAlign = (int)TextAlign; // base.RtlTranslateContent(this.TextAlign);
		textFormatFlags = baseTextFormatFlags;
		if ((txtAlign & (int)anyRightAlignment) != 0)
			textFormatFlags |= TextFormatFlags.Right;
		else if ((txtAlign & (int)anyCenterAlignment) != 0)
			textFormatFlags |= TextFormatFlags.HorizontalCenter;
	}
}

/// <summary>Base button item type.</summary>
public abstract class ButtonListItem<T> : IEquatable<ButtonListItem<T>>, INotifyPropertyChanged
{
	internal Point GlyphPosition;
	internal T state;
	internal Rectangle TextRect, SubtextRect, FocusRect;
	private bool focused;
	private object tag;
	private string text, subtext, tooltip;

	/// <summary>Initializes a new instance of the <see cref="ButtonListItem{T}"/> class.</summary>
	/// <param name="text">The text.</param>
	/// <param name="subtext">The subtext.</param>
	/// <param name="tooltipText">The tooltip text.</param>
	protected ButtonListItem(string text = "", string subtext = null, string tooltipText = null)
	{
		Text = text;
		Subtext = subtext;
		ToolTipText = tooltipText;
	}

	/// <summary>Occurs when a property value has changed.</summary>
	public event PropertyChangedEventHandler PropertyChanged;

	/// <summary>Gets or sets a value indicating whether this <see cref="ButtonListItem{T}"/> is checked.</summary>
	/// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
	[DefaultValue(false), Description("Indicates whether this item is checked."), Category("Appearance")]
	[BindableAttribute(true)]
	public abstract bool Checked { get; set; }

	/// <summary>Gets or sets a value indicating whether this <see cref="ButtonListItem{T}"/> is enabled.</summary>
	/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
	[DefaultValue(true), Category("Behavior")]
	[BindableAttribute(true)]
	public abstract bool Enabled { get; set; }

	/// <summary>Gets or sets the subtext.</summary>
	/// <value>The subtext.</value>
	[DefaultValue(null), Category("Appearance")]
	[BindableAttribute(true)]
	public string Subtext
	{
		get => subtext;
		set
		{
			if (value == subtext) return;
			subtext = value;
			OnNotifyPropertyChanged(nameof(Subtext));
		}
	}

	/// <summary>Gets or sets the tag.</summary>
	/// <value>The tag.</value>
	[DefaultValue((object)null), Category("Data")]
	[BindableAttribute(true)]
	[TypeConverterAttribute(typeof(StringConverter))]
	public object Tag
	{
		get => tag;
		set
		{
			if (value == tag) return;
			tag = value;
			OnNotifyPropertyChanged(nameof(Tag));
		}
	}

	/// <summary>Gets or sets the text.</summary>
	/// <value>The text.</value>
	[DefaultValue(""), Category("Appearance")]
	[BindableAttribute(true)]
	public string Text
	{
		get => text;
		set
		{
			if (value == text) return;
			text = value;
			OnNotifyPropertyChanged(nameof(Text));
		}
	}

	/// <summary>Gets or sets the tool tip text.</summary>
	/// <value>The tool tip text.</value>
	[DefaultValue(null), Category("Appearance")]
	[BindableAttribute(true)]
	public string ToolTipText
	{
		get => tooltip;
		set
		{
			if (value == tooltip) return;
			tooltip = value;
			OnNotifyPropertyChanged(nameof(ToolTipText));
		}
	}

	/// <summary>Gets or sets a value indicating whether this <see cref="ButtonListItem{T}"/> is focused.</summary>
	/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
	[DefaultValue(false), Category("Behavior")]
	[BindableAttribute(true)]
	internal virtual bool Focused
	{
		get => focused;
		set
		{
			if (focused == value) return;
			focused = value;
			OnNotifyPropertyChanged(nameof(Focused));
		}
	}

	internal T PrevState { get; set; }

	internal T State
	{
		get => state;
		set { PrevState = state; state = value; }
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.</summary>
	/// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
	/// <returns>
	/// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
	/// </returns>
	/// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
	public override bool Equals(object obj)
	{
		var bli = obj as ButtonListItem<T>;
		if (obj == null || bli == null)
			return false;
		return Equals(bli);
	}

	/// <summary>Determines whether the specified <see cref="ButtonListItem{T}"/> is equal to the current <see cref="ButtonListItem{T}"/>.</summary>
	/// <param name="other">The <see cref="ButtonListItem{T}"/> to compare with the current <see cref="ButtonListItem{T}"/>.</param>
	/// <returns>
	/// true if the specified <see cref="ButtonListItem{T}"/> is equal to the current <see cref="ButtonListItem{T}"/>; otherwise, false.
	/// </returns>
	public bool Equals(ButtonListItem<T> other)
	{
		if (other == null) return false;
		return (Checked == other.Checked) && (Enabled == other.Enabled) &&
			(Subtext == other.Subtext) && (Text == other.Text) && (ToolTipText == other.ToolTipText);
	}

	/// <summary>Returns a hash code for this instance.</summary>
	/// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
	public override int GetHashCode() => new { C = text, D = subtext, E = tooltip }.GetHashCode();

	/// <summary>Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.</summary>
	/// <returns>A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.</returns>
	public override string ToString() => System.Text.RegularExpressions.Regex.Replace(Text, @"\&([^\&])", "$1");

	internal void OffsetText(int x, int y)
	{
		TextRect.Offset(x, y);
		SubtextRect.Offset(x, y);
	}

	/// <summary>Called when a property value has changed.</summary>
	/// <param name="propertyName">Name of the property.</param>
	protected virtual void OnNotifyPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}