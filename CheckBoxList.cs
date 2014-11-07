using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace GroupControls
{
	/// <summary>
	/// Represents a windows control that displays a list of checkbox items with optional subtext entries.
	/// </summary>
	[ToolboxItem(true), ToolboxBitmap(typeof(CheckBoxList)), DefaultProperty("Items"), DefaultEvent("ItemCheckStateChanged")]
	[Description("Displays a list of checkbox items with optional subtext.")]
	public class CheckBoxList : ButtonListBase
	{
		private CheckBoxListItemCollection items;
		private CheckBoxState lastState = CheckBoxState.UncheckedNormal;
		private VisualStyleRenderer renderer;

		/// <summary>
		/// Creates a new instance of a <see cref="CheckBoxList"/>.
		/// </summary>
		public CheckBoxList() : base()
		{
			items = new CheckBoxListItemCollection(this);
			items.ItemAdded += itemsChanged;
			items.ItemDeleted += itemsChanged;
			items.ItemChanged += itemsChanged;
			items.Reset += itemsChanged;
			items.ItemPropertyChanged += itemPropertyChanged;

			try { renderer = new VisualStyleRenderer("BUTTON", 3, 0); } catch { }
		}

		/// <summary>
		/// Occurs when item check state changed.
		/// </summary>
		[Category("Behavior"), Description("Occurs when the value of any item's CheckState property changes.")]
		public event EventHandler<CheckBoxListItemCheckStateChangedEventArgs> ItemCheckStateChanged;

		/// <summary>
		/// Gets the list of <see cref="CheckBoxListItem"/> associated with the control.
		/// </summary>
		[MergableProperty(false), Category("Data"), DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		Localizable(true), Description("List of checkboxes with optional subtext")]
		public virtual CheckBoxListItemCollection Items
		{
			get { return items; }
		}

		/// <summary>
		/// Gets or sets the selected items in the list based on bits. Limited to lists of 64 items or less.
		/// </summary>
		[Browsable(false),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public long SelectedIndicies
		{
			get
			{
				if (items.Count > sizeof(long) * 8)
					throw new IndexOutOfRangeException("Too many items to retrieve");
				long ret = 0;
				for (int i = 0; i < items.Count; i++)
					if (items[i].Checked)
						ret |= (1L << i);
				return ret;
			}
			set
			{
				if (items.Count > sizeof(long) * 8)
					throw new IndexOutOfRangeException("Too many items to set");
				for (int i = 0; i < items.Count; i++)
					items[i].Checked = ((value & (1L << i)) > 0L);
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets whether the checkboxes will use three states rather than two.
		/// </summary>
		[DefaultValue(false),
		Description("Indicates whether the checkboxes will use three states rather than two."),
		Category("Behavior")]
		public bool ThreeState { get; set; }

		/// <summary>
		/// Gets the background renderer for this type of control.
		/// </summary>
		/// <value>
		/// The background renderer.
		/// </value>
		protected override PaintBackgroundMethod BackgroundRenderer
		{
			get { return CheckBoxRenderer.DrawParentBackground; }
		}

		/// <summary>
		/// Gets the base list of items.
		/// </summary>
		/// <value>
		/// Any list supportting and <see cref="System.Collections.IList"/> interface.
		/// </value>
		protected override System.Collections.IList BaseItems
		{
			get { return items; }
		}

		/// <summary>
		/// If each items <c>Tag</c> property has been assigned a value of <c>T</c>, this method retrived the OR value of all items.
		/// </summary>
		/// <typeparam name="T">An enumerated type with a FlagsAttribute assigned to all item's <c>Tag</c> properties.</typeparam>
		/// <param name="defaultValue">The default value.</param>
		/// <returns>The OR value of all items</returns>
		public T GetFlagsValue<T>(T defaultValue) where T : struct, IConvertible
		{
			IsValidEnum<T>();
			long ret = Convert.ToInt64(defaultValue);
			foreach (var item in this.items)
			{
				if (item.Tag != null && item.Tag is T && item.Checked)
					ret |= Convert.ToInt64(item.Tag);
			}
			return (T)Enum.ToObject(typeof(T), ret);
		}

		private static void IsValidEnum<T>() where T : struct, IConvertible
		{
			if (!typeof(T).IsEnum)
				throw new ArgumentException(string.Format("Type '{0}' is not an enum", typeof(T).FullName));
			if (!Attribute.IsDefined(typeof(T), typeof(FlagsAttribute)))
				throw new ArgumentException(string.Format("Type '{0}' doesn't have the 'Flags' attribute", typeof(T).FullName));
		}

		/// <summary>
		/// If each items <c>Tag</c> property has been assigned a value of <c>T</c>, this method with set the item's Checked property appropriately.
		/// </summary>
		/// <typeparam name="T">An enumerated type with a FlagsAttribute assigned to all item's <c>Tag</c> properties.</typeparam>
		/// <param name="value">The composite flag value.</param>
		public void SetFlagsValue<T>(T value) where T : struct, IConvertible
		{
			IsValidEnum<T>();
			long lv = Convert.ToInt64(value);
			foreach (var item in this.items)
			{
				if (item.Tag != null && item.Tag is T)
				{
					long tlv = Convert.ToInt64(item.Tag);
					item.Checked = (lv & tlv) == tlv;
				}
			}
		}

		private static bool EnumHasValue<T>(object enumValue, object value) where T : struct, IConvertible
		{
			if (enumValue == null || value == null || !(enumValue is T) || !(value is T))
				return false;
			long val = Convert.ToInt64(value);
			return (Convert.ToInt64(enumValue) & val) == val;
		}

		/// <summary>
		/// Processes the flags on check state changed. Include this method in the event handler for ItemCheckStateChanged.
		/// </summary>
		/// <param name="item">The item that was changed.</param>
		public void ProcessFlagsOnCheckStateChanged<T>(CheckBoxListItem item) where T : struct, IConvertible
		{
			IsValidEnum<T>();

			bool set = item.Checked;
			T newVal = GetFlagsValue<T>(Activator.CreateInstance<T>());
			for (int i = 0; i < items.Count; i++)
			{
				CheckBoxListItem curItem = items[i];
				bool changed = false;
				if (set && (EnumHasValue<T>(newVal, curItem.Tag) || EnumHasValue<T>(curItem.Tag, item.Tag)) && !curItem.Checked)
				{
					curItem.Checked = true;
					changed = true;
				}
				if (!set && EnumHasValue<T>(curItem.Tag, item.Tag) && curItem.Checked)
				{
					curItem.Checked = false;
					changed = true;
				}
				if (changed)
					InvalidateItem(i);
			}
		}

		/// <summary>
		/// Gets the size of the image used to display the button.
		/// </summary>
		/// <param name="g">Current <see cref="Graphics"/> context.</param>
		/// <returns>The size of the image.</returns>
		protected override Size GetButtonSize(Graphics g)
		{
			return CheckBoxRenderer.GetGlyphSize(g, System.Windows.Forms.VisualStyles.CheckBoxState.CheckedNormal);
		}

		/// <summary>
		/// Determines whether this list has the specified mnemonic in its members.
		/// </summary>
		/// <param name="charCode">The mnumonic character.</param>
		/// <returns>
		/// 	<c>true</c> if list has the mnemonic; otherwise, <c>false</c>.
		/// </returns>
		protected override bool ListHasMnemonic(char charCode)
		{
			foreach (CheckBoxListItem item in items)
			{
				if (Control.IsMnemonic(charCode, item.Text))
				{
					int idx = items.IndexOf(item);
					SetFocused(idx);
					ToggleItem(idx);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Called when item check state changed.
		/// </summary>
		/// <param name="e">The <see cref="CheckBoxListItemCheckStateChangedEventArgs"/> instance containing the event data.</param>
		protected virtual void OnItemCheckStateChanged(CheckBoxListItemCheckStateChangedEventArgs e)
		{
			var h = this.ItemCheckStateChanged;
			if (h != null)
				h(this, e);
		}

		/// <summary>
		/// Raises the <see cref="Control.KeyDown"/> event.
		/// </summary>
		/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				PressingItem = this.FocusedIndex;
				InvalidateItem(PressingItem);
				base.Update();
				e.Handled = true;
			}
			base.OnKeyDown(e);
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseClick"/> event.
		/// </summary>
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

		/// <summary>
		/// Paints the button.
		/// </summary>
		/// <param name="g">A <see cref="Graphics" /> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="bounds">The bounds in which to paint the item.</param>
		protected override void PaintButton(Graphics g, int index, Rectangle bounds)
		{
			CheckBoxListItem li = this.BaseItems[index] as CheckBoxListItem;
			// Get current state
			CheckBoxState curState = (CheckBoxState)(((int)li.CheckState * 4) + 1);
			if (!this.Enabled || !li.Enabled)
				curState += 3;
			else if (index == PressingItem)
				curState += 2;
			else if (index == HoverItem)
				curState++;
			// Draw glyph
			Microsoft.Win32.NativeMethods.BufferedPaint.Paint<CheckBoxState, CheckBoxListItem>(g, this, bounds, PaintAnimatedButton, lastState, curState, GetTransition(curState, lastState), li);
			lastState = curState;
		}

		private void PaintAnimatedButton(Graphics g, Rectangle bounds, CheckBoxState curState, CheckBoxListItem li)
		{
			Point gp = li.GlyphPosition;
			gp.Offset(bounds.Location);
			CheckBoxRenderer.DrawCheckBox(g, gp, curState);
		}

		private uint GetTransition(CheckBoxState curState, CheckBoxState lastState)
		{
			if (renderer != null)
				return renderer.GetTransitionDuration((int)curState, (int)lastState);
			return 0;
		}

		/// <summary>
		/// Processes a keyboard event.
		/// </summary>
		/// <param name="ke">The <see cref="KeyEventArgs"/> associated with the key press.</param>
		/// <returns><c>true</c> if the key was processed by the control; otherwise, <c>false</c>.</returns>
		protected override bool ProcessKey(KeyEventArgs ke)
		{
			bool ret = false;
			switch (ke.KeyCode)
			{
				case Keys.Down:
				case Keys.Right:
					if (FocusNextItem(this.FocusedItem, true))
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
					if (FocusNextItem(this.FocusedItem, false))
					{
						EnsureVisible(FocusedIndex);
						ret = true;
					}
					break;
				case Keys.Space:
					ToggleItem(FocusedIndex);
					break;
				case Keys.Tab:
					if (FocusNextItem(this.FocusedItem, !ke.Shift))
					{
						EnsureVisible(FocusedIndex);
						ret = true;
					}
					break;
				default:
					break;
			}
			if (ret) ke.SuppressKeyPress = true;
			return ret;
		}

		private void itemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			this.OnListChanged();
		}

		private void itemsChanged(object sender, EventedList<CheckBoxListItem>.ListChangedEventArgs<CheckBoxListItem> e)
		{
			if (e.ListChangedType != ListChangedType.ItemChanged || !e.Item.Equals(e.OldItem))
				this.OnListChanged();
		}

		/// <summary>
		/// Flips the indicated items check state.
		/// </summary>
		/// <param name="itemIndex">Index of the item to toggle.</param>
		private void ToggleItem(int itemIndex)
		{
			if (itemIndex >= 0 && itemIndex < items.Count && items[itemIndex].Enabled)
			{
				switch (items[itemIndex].CheckState)
				{
					case CheckState.Checked:
						items[itemIndex].CheckState = this.ThreeState ? CheckState.Indeterminate : CheckState.Unchecked;
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

	/// <summary>
	/// Provides data for the <see cref="E:CheckBoxList.ItemCheckStateChanged"/> event of the <see cref="CheckBoxList"/> control.
	/// </summary>
	[DefaultProperty("Item")]
	public class CheckBoxListItemCheckStateChangedEventArgs : EventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="CheckBoxListItemCheckStateChangedEventArgs" /> class.
		/// </summary>
		/// <param name="item">The item.</param>
		/// <param name="index">The index.</param>
		public CheckBoxListItemCheckStateChangedEventArgs(CheckBoxListItem item, int index)
		{
			this.Item = item;
			this.ItemIndex = index;
		}

		/// <summary>
		/// Gets the <see cref="CheckBoxListItem"/> whose checked state is changing.
		/// </summary>
		/// <value>
		/// The <see cref="CheckBoxListItem"/> whose checked state is changing.
		/// </value>
		public CheckBoxListItem Item { get; private set; }

		/// <summary>
		/// Gets the index of the item.
		/// </summary>
		/// <value>
		/// The index of the item.
		/// </value>
		public int ItemIndex { get; private set; }
	}

	/// <summary>
	/// An item associated with a <see cref="CheckBoxList"/>.
	/// </summary>
	[DefaultProperty("Text")]
	public class CheckBoxListItem : ButtonListItem
	{
		private CheckState checkState = CheckState.Unchecked;

		/// <summary>
		/// Creates a new instance of a <c>CheckBoxListItem</c>.
		/// </summary>
		public CheckBoxListItem()
		{
		}

		/// <summary>
		/// Creates a new instance of a <c>CheckBoxListItem</c>.
		/// </summary>
		/// <param name="text">Text displayed next to checkbox.</param>
		/// <param name="subtext">Subtext displayed under text.</param>
		public CheckBoxListItem(string text, string subtext)
			: this(text, subtext, null)
		{
		}

		/// <summary>
		/// Creates a new instance of a <c>CheckBoxListItem</c>.
		/// </summary>
		/// <param name="text">Text displayed next to checkbox.</param>
		/// <param name="subtext">Subtext displayed under text.</param>
		/// <param name="tooltiptext">Tooltip displayed for the item.</param>
		public CheckBoxListItem(string text, string subtext, string tooltiptext)
			: base(text, subtext, tooltiptext)
		{
		}

		/// <summary>
		/// Occurs when the CheckState value changes.
		/// </summary>
		[Category("Behavior"), Description("Occurs when the value of the CheckState property changes.")]
		public event EventHandler CheckStateChanged;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="CheckBoxListItem"/> is checked.
		/// </summary>
		/// <value><c>true</c> if checked; otherwise, <c>false</c>.</value>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public override bool Checked
		{
			get
			{
				return this.CheckState == CheckState.Checked;
			}
			set
			{
				this.CheckState = value ? CheckState.Checked : CheckState.Unchecked;
				base.Checked = value;
			}
		}

		/*/// <summary>
		/// Gets or sets the value.
		/// </summary>
		/// <value>
		/// The value.
		/// </value>
		public virtual T Value { get; set; }*/

		/// <summary>
		/// Gets or sets the state of the checkbox.
		/// </summary>
		[DefaultValue(typeof(CheckState), "Unchecked"),
		Description("State of the checkbox for the item."),
		Category("Appearance")]
		public System.Windows.Forms.CheckState CheckState
		{
			get { return this.checkState; }
			set
			{
				if (value != this.CheckState)
				{
					this.checkState = value;
					OnCheckStateChanged(EventArgs.Empty);
					OnNotifyPropertyChanged("CheckState");
				}
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="CheckBoxListItem"/> is equal to the current <see cref="CheckBoxListItem"/>.
		/// </summary>
		/// <param name="cb2">The <see cref="CheckBoxListItem"/> to compare with the current <see cref="CheckBoxListItem"/>.</param>
		/// <returns>
		/// true if the specified <see cref="CheckBoxListItem"/> is equal to the current <see cref="CheckBoxListItem"/>; otherwise, false.
		/// </returns>
		public bool Equals(CheckBoxListItem cb2)
		{
			return base.Equals((ButtonListItem)cb2) && this.CheckState == cb2.CheckState;
		}

		/// <summary>
		/// Raises the <see cref="E:CheckStateChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void OnCheckStateChanged(EventArgs e)
		{
			EventHandler handler1 = this.CheckStateChanged;
			if (handler1 != null)
				handler1(this, e);
		}
	}

	/// <summary>
	/// 
	/// </summary>
	[Editor(typeof(System.ComponentModel.Design.CollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public class CheckBoxListItemCollection : EventedList<CheckBoxListItem>
	{
		private CheckBoxList parent;

		internal CheckBoxListItemCollection(CheckBoxList list)
		{
			parent = list;
		}

		/// <summary>
		/// Adds a new item to the collection.
		/// </summary>
		/// <param name="text">Item text.</param>
		/// <param name="subtext">Item subtext.</param>
		public void Add(string text, string subtext)
		{
			base.Add(new CheckBoxListItem(text, subtext));
		}

		/// <summary>
		/// Adds the specified text values to the collection.
		/// </summary>
		/// <param name="textValues">The text value pairs representing matching text and subtext.</param>
		/// <exception cref="System.ArgumentException">List of values must contain matching text/subtext entries for an even count of strings.;textValues</exception>
		public void Add(params string[] textValues)
		{
			if (textValues.Length % 2 != 0)
				throw new ArgumentException("List of values must contain matching text/subtext entries for an even count of strings.", "textValues");
			parent.SuspendLayout();
			for (int i = 0; i < textValues.Length; i += 2)
				this.Add(textValues[i], textValues[i + 1]);
			parent.ResumeLayout();
		}
	}
}