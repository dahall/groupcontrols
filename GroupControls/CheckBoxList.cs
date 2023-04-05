#pragma warning disable GlobalUsingsAnalyzer // Using should be in global file
using System.Windows.Forms.VisualStyles;
#pragma warning restore GlobalUsingsAnalyzer // Using should be in global file

namespace GroupControls;

/// <summary>Represents a windows control that displays a list of checkbox items with optional subtext entries.</summary>
[ToolboxItem(true), ToolboxBitmap(typeof(CheckBoxList)), DefaultProperty("Items"), DefaultEvent("ItemCheckStateChanged")]
[Description("Displays a list of checkbox items with optional subtext.")]
public class CheckBoxList : ButtonListBase<CheckBoxState>
{
	private readonly CheckBoxListItemCollection items;
	private readonly VisualStyleRenderer renderer;

	/// <summary>Creates a new instance of a <see cref="CheckBoxList"/>.</summary>
	public CheckBoxList()
	{
		items = new CheckBoxListItemCollection(this);
		items.ItemAdded += ItemsChanged;
		items.ItemDeleted += ItemsChanged;
		items.ItemChanged += ItemsChanged;
		items.Reset += ItemsChanged;
		items.ItemPropertyChanged += ItemPropertyChanged;

		try { renderer = new VisualStyleRenderer("BUTTON", 3, 0); } catch { }
	}

	/// <summary>Occurs when item check state changed.</summary>
	[Category("Behavior"), Description("Occurs when the value of any item's CheckState property changes.")]
	public event EventHandler<CheckBoxListItemCheckStateChangedEventArgs> ItemCheckStateChanged;

	/// <summary>Gets the list of <see cref="CheckBoxListItem"/> associated with the control.</summary>
	[MergableProperty(false), Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
	Localizable(true), Description("List of checkboxes with optional subtext")]
	public virtual CheckBoxListItemCollection Items => items;

	/// <summary>Gets or sets the selected items in the list based on bits. Limited to lists of 64 items or less.</summary>
	[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public long SelectedIndicies
	{
		get
		{
			if (items.Count > sizeof(long) * 8)
				throw new ArgumentOutOfRangeException(nameof(SelectedIndicies), @"Too many items to retrieve");
			long ret = 0;
			for (var i = 0; i < items.Count; i++)
				if (items[i].Checked)
					ret |= 1L << i;
			return ret;
		}
		set
		{
			if (items.Count > sizeof(long) * 8)
				throw new ArgumentOutOfRangeException(nameof(SelectedIndicies), @"Too many items to set");
			for (var i = 0; i < items.Count; i++)
				items[i].Checked = (value & (1L << i)) > 0L;
			Refresh();
		}
	}

	/// <summary>Gets or sets whether the checkboxes will use three states rather than two.</summary>
	[DefaultValue(false),
	Description("Indicates whether the checkboxes will use three states rather than two."),
	Category("Behavior")]
	public bool ThreeState { get; set; }

	/// <summary>Gets the base list of items.</summary>
	/// <value>Any list supporting and <see cref="System.Collections.IList"/> interface.</value>
	protected internal override System.Collections.IList BaseItems => items;

	/// <summary>Gets the background renderer for this type of control.</summary>
	/// <value>The background renderer.</value>
	protected override PaintBackgroundMethod BackgroundRenderer => CheckBoxRenderer.DrawParentBackground;

	/// <summary>Gets the size of the image used to display the button.</summary>
	/// <param name="g">Current <see cref="Graphics"/> context.</param>
	/// <returns>The size of the image.</returns>
	protected override Size GetButtonSize(Graphics g) => CheckBoxRenderer.GetGlyphSize(g, CheckBoxState.CheckedNormal);

	/// <summary>Determines whether this list has the specified mnemonic in its members.</summary>
	/// <param name="charCode">The mnemonic character.</param>
	/// <returns><c>true</c> if list has the mnemonic; otherwise, <c>false</c>.</returns>
	protected override bool ListHasMnemonic(char charCode)
	{
		foreach (var item in items)
		{
			if (IsMnemonic(charCode, item.Text))
			{
				var idx = items.IndexOf(item);
				SetFocused(idx);
				ToggleItem(idx);
				return true;
			}
		}
		return false;
	}

	/// <summary>Called when item check state changed.</summary>
	/// <param name="e">The <see cref="CheckBoxListItemCheckStateChangedEventArgs"/> instance containing the event data.</param>
	protected virtual void OnItemCheckStateChanged(CheckBoxListItemCheckStateChangedEventArgs e) => ItemCheckStateChanged?.Invoke(this, e);

	/// <summary>Raises the <see cref="Control.KeyDown"/> event.</summary>
	/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Space)
		{
			PressingItem = FocusedIndex;
			InvalidateItem(PressingItem);
			Update();
			e.Handled = true;
		}
		base.OnKeyDown(e);
	}

	/// <summary>Raises the <see cref="Control.MouseClick"/> event.</summary>
	/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (HoverItem != -1)
		{
			SetFocused(HoverItem);
			ToggleItem(HoverItem);
		}
	}

	/// <summary>Paints the button.</summary>
	/// <param name="g">A <see cref="Graphics"/> reference.</param>
	/// <param name="index">The index of the item.</param>
	/// <param name="bounds">The bounds in which to paint the item.</param>
	/// <param name="newState"></param>
	protected override void PaintButton(Graphics g, int index, Rectangle bounds, bool newState)
	{
		var li = BaseItems[index] as CheckBoxListItem;
		if (li == null) throw new ArgumentOutOfRangeException(nameof(index));
		// Get current state
		var curState = (CheckBoxState)((int)li.CheckState * 4 + 1);
		if (!Enabled || !li.Enabled)
			curState += 3;
		else if (index == PressingItem)
			curState += 2;
		else if (index == HoverItem)
			curState++;
		li.State = curState;
		// Draw glyph
		var gp = li.GlyphPosition;
		gp.Offset(bounds.Location);
		CheckBoxRenderer.DrawCheckBox(g, gp, newState ? li.State : li.PrevState);
	}

	/// <summary>Processes a keyboard event.</summary>
	/// <param name="ke">The <see cref="KeyEventArgs"/> associated with the key press.</param>
	/// <returns><c>true</c> if the key was processed by the control; otherwise, <c>false</c>.</returns>
	protected override bool ProcessKey(KeyEventArgs ke)
	{
		var ret = false;
		switch (ke.KeyCode)
		{
			case Keys.Down:
			case Keys.Right:
				if (FocusNextItem(FocusedItem, true))
				{
					EnsureVisible(FocusedIndex);
					ret = true;
				}
				break;

			case Keys.Enter:
				break;

			case Keys.Escape:
				break;

			case Keys.Up:
			case Keys.Left:
				if (FocusNextItem(FocusedItem, false))
				{
					EnsureVisible(FocusedIndex);
					ret = true;
				}
				break;

			case Keys.Space:
				ToggleItem(FocusedIndex);
				break;

			case Keys.Tab:
				if (FocusNextItem(FocusedItem, !ke.Shift))
				{
					EnsureVisible(FocusedIndex);
					ret = true;
				}
				break;
		}
		if (ret) ke.SuppressKeyPress = true;
		return ret;
	}

	private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e) => OnListChanged();

	private void ItemsChanged(object sender, EventedList<CheckBoxListItem>.ListChangedEventArgs<CheckBoxListItem> e)
	{
		if (e.ListChangedType != ListChangedType.ItemChanged || !e.Item.Equals(e.OldItem))
			OnListChanged();
	}

	/// <summary>Flips the indicated items check state.</summary>
	/// <param name="itemIndex">Index of the item to toggle.</param>
	private void ToggleItem(int itemIndex)
	{
		if (itemIndex >= 0 && itemIndex < items.Count && items[itemIndex].Enabled)
		{
			switch (items[itemIndex].CheckState)
			{
				case CheckState.Checked:
					items[itemIndex].CheckState = ThreeState ? CheckState.Indeterminate : CheckState.Unchecked;
					break;

				case CheckState.Unchecked:
					items[itemIndex].CheckState = CheckState.Checked;
					break;

				default:
					items[itemIndex].CheckState = CheckState.Unchecked;
					break;
			}
			InvalidateItem(itemIndex);
			OnItemCheckStateChanged(new CheckBoxListItemCheckStateChangedEventArgs(items[itemIndex], itemIndex));
		}
	}
}

/// <summary>An item associated with a <see cref="CheckBoxList"/>.</summary>
[DefaultProperty("Text")]
public class CheckBoxListItem : ButtonListItem<CheckBoxState>
{
	/// <summary>Creates a new instance of a <c>CheckBoxListItem</c>.</summary>
	public CheckBoxListItem() => State = PrevState = CheckBoxState.UncheckedNormal;

	/// <summary>Creates a new instance of a <c>CheckBoxListItem</c>.</summary>
	/// <param name="text">Text displayed next to checkbox.</param>
	/// <param name="subtext">Subtext displayed under text.</param>
	/// <param name="tooltipText">Tooltip displayed for the item.</param>
	public CheckBoxListItem(string text, string subtext, string tooltipText = null)
		: base(text, subtext, tooltipText) => State = PrevState = CheckBoxState.UncheckedNormal;

	/// <summary>Occurs when the CheckState value changes.</summary>
	[Category("Behavior"), Description("Occurs when the value of the CheckState property changes.")]
	public event EventHandler CheckStateChanged;

	/// <summary>Gets or sets a value indicating whether this <see cref="CheckBoxListItem"/> is checked.</summary>
	/// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override bool Checked
	{
		get => CheckState == CheckState.Checked;
		set { if (Checked != value) CheckState = value ? CheckState.Checked : CheckState.Unchecked; }
	}

	/// <summary>Gets or sets the state of the checkbox.</summary>
	[DefaultValue(typeof(CheckState), "Unchecked"),
	Description("State of the checkbox for the item."),
	Category("Appearance")]
	public CheckState CheckState
	{
		get
		{
			var disp = ((int)state - 1) / 4; return disp == 0 ? CheckState.Unchecked : (disp == 1 ? CheckState.Checked : CheckState.Indeterminate);
		}
		set
		{
			if (value == CheckState) return;
			var wasChecked = Checked;
			State = (CheckBoxState)(((int)state - 1) % 4 + 1 + (value == CheckState.Unchecked ? 0 : (value == CheckState.Checked ? 4 : 8)));
			if (Checked != wasChecked)
				OnNotifyPropertyChanged(nameof(Checked));
			OnCheckStateChanged(EventArgs.Empty);
			OnNotifyPropertyChanged(nameof(CheckState));
		}
	}

	/// <summary>Gets or sets a value indicating whether this <see cref="CheckBoxListItem"/> is enabled.</summary>
	/// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
	public override bool Enabled
	{
		get => (int)state % 4 != 0;
		set
		{
			if (Enabled == value) return;
			State = (CheckBoxState)(((int)state - 1) % 4) + (value ? 1 : 4);
			OnNotifyPropertyChanged(nameof(Enabled));
		}
	}

	internal override bool Focused { get; set; }

	/// <summary>Determines whether the specified <see cref="CheckBoxListItem"/> is equal to the current <see cref="CheckBoxListItem"/>.</summary>
	/// <param name="other">The <see cref="CheckBoxListItem"/> to compare with the current <see cref="CheckBoxListItem"/>.</param>
	/// <returns>
	/// true if the specified <see cref="CheckBoxListItem"/> is equal to the current <see cref="CheckBoxListItem"/>; otherwise, false.
	/// </returns>
	public bool Equals(CheckBoxListItem other) => base.Equals(other) && CheckState == other.CheckState;

	/// <summary>Raises the <see cref="E:CheckStateChanged"/> event.</summary>
	/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
	protected virtual void OnCheckStateChanged(EventArgs e) => CheckStateChanged?.Invoke(this, e);
}

/// <summary>Provides data for the <see cref="E:CheckBoxList.ItemCheckStateChanged"/> event of the <see cref="CheckBoxList"/> control.</summary>
[DefaultProperty("Item")]
public class CheckBoxListItemCheckStateChangedEventArgs : EventArgs
{
	/// <summary>Initializes a new instance of the <see cref="CheckBoxListItemCheckStateChangedEventArgs"/> class.</summary>
	/// <param name="item">The item.</param>
	/// <param name="index">The index.</param>
	public CheckBoxListItemCheckStateChangedEventArgs(CheckBoxListItem item, int index)
	{
		Item = item;
		ItemIndex = index;
	}

	/// <summary>Gets the <see cref="CheckBoxListItem"/> whose checked state is changing.</summary>
	/// <value>The <see cref="CheckBoxListItem"/> whose checked state is changing.</value>
	public CheckBoxListItem Item { get; }

	/// <summary>Gets the index of the item.</summary>
	/// <value>The index of the item.</value>
	public int ItemIndex { get; }
}

/// <summary></summary>
[Editor("System.Windows.Forms.Design.CollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
	"System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
public class CheckBoxListItemCollection : EventedList<CheckBoxListItem>
{
	private readonly CheckBoxList parent;

	internal CheckBoxListItemCollection(CheckBoxList list) => parent = list;

	/// <summary>Adds a new item to the collection.</summary>
	/// <param name="text">Item text.</param>
	/// <param name="subtext">Item subtext.</param>
	public void Add(string text, string subtext) => base.Add(new CheckBoxListItem(text, subtext));

	/// <summary>Adds the specified text values to the collection.</summary>
	/// <param name="textValues">The text value pairs representing matching text and subtext.</param>
	/// <exception cref="System.ArgumentException">
	/// List of values must contain matching text/subtext entries for an even count of strings.;textValues
	/// </exception>
	public void Add(params string[] textValues)
	{
		if (textValues.Length % 2 != 0)
			throw new ArgumentException(@"List of values must contain matching text/subtext entries for an even count of strings.", nameof(textValues));
		parent.SuspendLayout();
		for (var i = 0; i < textValues.Length; i += 2)
			Add(textValues[i], textValues[i + 1]);
		parent.ResumeLayout();
	}

	/// <summary>Called when an item is added.</summary>
	/// <param name="index">The item index.</param>
	/// <param name="value">The item value.</param>
	protected override void OnItemAdded(int index, CheckBoxListItem value)
	{
		base.OnItemAdded(index, value);
		if (value != null && string.IsNullOrEmpty(value.Text) && parent != null && parent.IsDesignerHosted)
			value.Text = $"checkBoxItem{Count}";
	}
}