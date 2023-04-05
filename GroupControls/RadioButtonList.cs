#pragma warning disable GlobalUsingsAnalyzer // Using should be in global file
using System.Windows.Forms.VisualStyles;
#pragma warning restore GlobalUsingsAnalyzer // Using should be in global file

namespace GroupControls;

/// <summary>Represents a windows control that displays a list of radio button items with optional subtext entries.</summary>
[ToolboxItem(true), ToolboxBitmap(typeof(RadioButtonList)), DefaultProperty("Items"), DefaultEvent("SelectedIndexChanged")]
[Description("Displays a list of radio buttons with optional subtext.")]
public class RadioButtonList : ButtonListBase<RadioButtonState>
{
	private readonly RadioButtonListItemCollection items;
	private readonly VisualStyleRenderer renderer;
	private int selectedIndex = -1;

	/// <summary>Creates a new instance of a <see cref="RadioButtonList"/>.</summary>
	public RadioButtonList()
	{
		items = new RadioButtonListItemCollection(this);
		items.ItemAdded += ItemsChanged;
		items.ItemDeleted += ItemsChanged;
		items.ItemChanged += ItemsChanged;
		items.Reset += ItemsChanged;
		items.ItemPropertyChanged += ItemPropertyChanged;

		try { renderer = new VisualStyleRenderer("BUTTON", 2, 0); } catch { }
	}

	/// <summary>Occurs when the selected index has changed.</summary>
	[Category("Behavior"), Description("Occurs when the value of the SelectedIndex property changes.")]
	public event EventHandler SelectedIndexChanged;

	/// <summary>Gets the list of <see cref="RadioButtonListItem"/> associated with the control.</summary>
	[MergableProperty(false), Category("Data"),
	DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
	Localizable(true), Description("List of radio buttons with optional subtext")]
	public virtual RadioButtonListItemCollection Items => items;

	/// <summary>Gets or sets the index specifying the currently selected item.</summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
	Bindable(true), DefaultValue(-1), Category("Data"),
	Description("Gets or sets the index specifying the currently selected item.")]
	public int SelectedIndex
	{
		get => selectedIndex;
		set
		{
			if (selectedIndex != value)
			{
				if (value < -1 || value >= items.Count)
					throw new ArgumentOutOfRangeException(nameof(SelectedIndex));

				// Clear old selected item
				var oldSelect = selectedIndex;
				if (oldSelect > -1 && oldSelect < items.Count)
				{
					Items[oldSelect].Checked = false;
					InvalidateItem(oldSelect);
					//if (Focused) Invalidate(Items[oldSelect].TextRect);
				}

				// Set new item
				selectedIndex = value;
				if (selectedIndex > -1)
				{
					Items[selectedIndex].Checked = true;
					InvalidateItem(selectedIndex);
					//if (Focused) Invalidate(Items[selectedIndex].TextRect);
				}
				SetFocused(selectedIndex);

				OnSelectedIndexChanged(EventArgs.Empty);
			}
		}
	}

	/// <summary>Gets or sets currently selected item in the list.</summary>
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
	Description("Gets or sets currently selected item in the list."),
	Browsable(false), Bindable(true), DefaultValue(null), Category("Data")]
	public RadioButtonListItem SelectedItem
	{
		get => selectedIndex == -1 ? null : items[selectedIndex];
		set => SelectedIndex = items.IndexOf(value);
	}

	/// <summary>Gets the base list of items.</summary>
	/// <value>Any list supporting and <see cref="System.Collections.IList"/> interface.</value>
	protected internal override System.Collections.IList BaseItems => items;

	/// <summary>Gets the background renderer for this type of control.</summary>
	/// <value>The background renderer.</value>
	protected override PaintBackgroundMethod BackgroundRenderer => RadioButtonRenderer.DrawParentBackground;

	/// <summary>Gets the size of the image used to display the button.</summary>
	/// <param name="g">Current <see cref="Graphics"/> context.</param>
	/// <returns>The size of the image.</returns>
	protected override Size GetButtonSize(Graphics g) => RadioButtonRenderer.GetGlyphSize(g, RadioButtonState.CheckedNormal);

	/// <summary>Determines whether this list has the specified mnemonic in its members.</summary>
	/// <param name="charCode">The mnemonic character.</param>
	/// <returns><c>true</c> if list has the mnemonic; otherwise, <c>false</c>.</returns>
	protected override bool ListHasMnemonic(char charCode)
	{
		foreach (var item in items)
		{
			if (IsMnemonic(charCode, item.Text))
			{
				SetSelected(items.IndexOf(item));
				return true;
			}
		}
		return false;
	}

	/// <summary>Raises the <see cref="Control.GotFocus"/> event.</summary>
	/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		if (selectedIndex != -1)
			InvalidateItem(selectedIndex);
		else if (FocusedIndex == -1 && items.Count > 0)
		{
			SetFocused(GetNextEnabledItemIndex(-1, true));
			if (FocusedIndex != -1)
				EnsureVisible(FocusedIndex);
		}
	}

	/// <summary>Raises the <see cref="Control.KeyDown"/> event.</summary>
	/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
	protected override void OnKeyDown(KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Space)
		{
			PressingItem = selectedIndex;
			InvalidateItem(PressingItem);
			Update();
			e.Handled = true;
		}
		base.OnKeyDown(e);
	}

	/// <summary>Raises the <see cref="Control.LostFocus"/> event.</summary>
	/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
	protected override void OnLostFocus(EventArgs e)
	{
		base.OnLostFocus(e);
		if (selectedIndex != -1)
			InvalidateItem(selectedIndex);
	}

	/// <summary>Raises the <see cref="Control.MouseClick"/> event.</summary>
	/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
	protected override void OnMouseClick(MouseEventArgs e)
	{
		base.OnMouseClick(e);
		if (HoverItem != -1)
			SetSelected(HoverItem);
	}

	/// <summary>Raises the <see cref="E:SelectedIndexChanged"/> event.</summary>
	/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
	protected virtual void OnSelectedIndexChanged(EventArgs e) => SelectedIndexChanged?.Invoke(this, e);

	/// <summary>Paints the button.</summary>
	/// <param name="g">A <see cref="Graphics"/> reference.</param>
	/// <param name="index">The index of the item.</param>
	/// <param name="bounds">The bounds in which to paint the item.</param>
	/// <param name="newState"></param>
	protected override void PaintButton(Graphics g, int index, Rectangle bounds, bool newState)
	{
		var li = BaseItems[index] as RadioButtonListItem;
		if (li == null) throw new ArgumentOutOfRangeException(nameof(index));
		// Get current state
		var curState = li.Checked ? RadioButtonState.CheckedNormal : RadioButtonState.UncheckedNormal;
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
		RadioButtonRenderer.DrawRadioButton(g, gp, newState ? li.State : li.PrevState);
		System.Diagnostics.Debug.WriteLine($"PaintRadioButton[{index}]: tx='{li.Text}'; r={bounds}; st[{newState}]={(newState ? li.State : li.PrevState)}");
	}

	/// <summary>Processes a keyboard event.</summary>
	/// <param name="ke">The <see cref="KeyEventArgs"/> associated with the key press.</param>
	/// <returns><c>true</c> if the key was processed by the control; otherwise, <c>false</c>.</returns>
	protected override bool ProcessKey(KeyEventArgs ke)
	{
		switch (ke.KeyCode)
		{
			case Keys.Down:
			case Keys.Right:
				if (SelectNextItem(FocusedItem as RadioButtonListItem, true))
				{
					EnsureVisible(selectedIndex);
					return true;
				}
				break;

			case Keys.Up:
			case Keys.Left:
				if (SelectNextItem(FocusedItem as RadioButtonListItem, false))
				{
					EnsureVisible(selectedIndex);
					return true;
				}
				break;

			case Keys.Space:
				if (FocusedItem != null && !FocusedItem.Checked)
				{
					EnsureVisible(selectedIndex);
					SetSelected(FocusedIndex);
				}
				break;
		}
		return false;
	}

	/// <summary>Resets the list's layout.</summary>
	protected override void ResetListLayout(string property)
	{
		base.ResetListLayout(property);
		// Get the change to the selected index based on item Checked property
		SelectedIndex = items.CheckedItemIndex;
	}

	private void ItemPropertyChanged(object sender, PropertyChangedEventArgs e) => OnListChanged();

	private void ItemsChanged(object sender, EventedList<RadioButtonListItem>.ListChangedEventArgs<RadioButtonListItem> e)
	{
		if (e.ListChangedType != ListChangedType.ItemChanged || !e.Item.Equals(e.OldItem))
			OnListChanged();
	}

	private bool SelectNextItem(RadioButtonListItem i, bool forward)
	{
		if (items.Count > 0)
		{
			var idx = -1;
			if (i != null && (idx = BaseItems.IndexOf(i)) == -1)
				throw new ArgumentOutOfRangeException(nameof(i));
			idx = GetNextEnabledItemIndex(idx, forward);
			if (idx != -1)
			{
				EnsureVisible(idx);
				SetSelected(idx);
				return true;
			}
		}
		return false;
	}

	private void SetSelected(int itemIndex)
	{
		// Ignore if item is disabled
		if (itemIndex != -1 && !items[itemIndex].Enabled)
			return;
		SelectedIndex = itemIndex;
	}
}

/// <summary>An item associated with a <see cref="RadioButtonList"/>.</summary>
[DefaultProperty("Text")]
public class RadioButtonListItem : ButtonListItem<RadioButtonState>
{
	/// <summary>Initializes a new instance of the <see cref="RadioButtonListItem"/> class.</summary>
	public RadioButtonListItem() => State = PrevState = RadioButtonState.UncheckedNormal;

	/// <summary>Initializes a new instance of the <see cref="RadioButtonListItem"/> class.</summary>
	/// <param name="text">Text displayed next to radio button.</param>
	/// <param name="subtext">Subtext displayed under text.</param>
	/// <param name="tooltipText">Tooltip displayed for the item.</param>
	public RadioButtonListItem(string text, string subtext = null, string tooltipText = null)
		: base(text, subtext, tooltipText) => State = PrevState = RadioButtonState.UncheckedNormal;

	/// <summary>Gets or sets a value indicating whether this <see cref="RadioButtonListItem"/> is checked.</summary>
	/// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
	public override bool Checked
	{
		get => ((int)state - 1) / 4 == 1;
		set
		{
			if (value == Checked) return;
			State = (RadioButtonState)(((int)state - 1) % 4 + 1 + (value ? 4 : 0));
			OnNotifyPropertyChanged(nameof(Checked));
		}
	}

	/// <summary>Gets or sets a value indicating whether this <see cref="RadioButtonListItem"/> is enabled.</summary>
	/// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
	public override bool Enabled
	{
		get => state != RadioButtonState.CheckedDisabled && state != RadioButtonState.UncheckedDisabled;
		set
		{
			if (Enabled == value) return;
			if (value)
				State = Checked ? RadioButtonState.CheckedNormal : RadioButtonState.UncheckedNormal;
			else
				State = Checked ? RadioButtonState.CheckedDisabled : RadioButtonState.UncheckedDisabled;
			OnNotifyPropertyChanged(nameof(Enabled));
		}
	}
}

/// <summary>Collection of <see cref="RadioButtonListItem"/> items.</summary>
[Editor("System.Windows.Forms.Design.CollectionEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
	"System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
public class RadioButtonListItemCollection : EventedList<RadioButtonListItem>
{
	private readonly RadioButtonList parent;

	internal RadioButtonListItemCollection(RadioButtonList list) => parent = list;

	internal int CheckedItemIndex => Count > 0 ? FindIndex(item => item.Checked) : -1;

	/// <summary>Adds a new item to the collection.</summary>
	/// <param name="text">Item text.</param>
	/// <param name="subtext">Item subtext.</param>
	public void Add(string text, string subtext) => Add(new RadioButtonListItem(text, subtext));

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
	protected override void OnItemAdded(int index, RadioButtonListItem value)
	{
		base.OnItemAdded(index, value);
		if (value != null && string.IsNullOrEmpty(value.Text) && parent != null && parent.IsDesignerHosted)
			value.Text = "radioButtonItem" + Count.ToString();
	}
}