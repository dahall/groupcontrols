using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GroupControls
{
	/// <summary>
	/// Represents a windows control that displays a list of checkbox items with optional subtext entries.
	/// </summary>
	[ToolboxBitmap(typeof(CheckBoxList)), DefaultProperty("Items")]
	public class CheckBoxList : ButtonListBase
	{
		private CheckBoxListItemCollection items;

		/// <summary>
		/// Creates a new instance of a <see cref="CheckBoxList"/>.
		/// </summary>
		public CheckBoxList() : base()
		{
			items = new CheckBoxListItemCollection(this);
		}

		/// <summary>
		/// Occurs when item check state changed.
		/// </summary>
		public event EventHandler ItemCheckStateChanged;

		/// <summary>
		/// Called when item check state changed.
		/// </summary>
		protected virtual void OnItemCheckStateChanged()
		{
			EventHandler h = this.ItemCheckStateChanged;
			if (h != null)
				h(this, EventArgs.Empty);
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
		/// Gets or sets the alignment of the checkbox in relation to the text.
		/// </summary>
		[DefaultValue(typeof(ContentAlignment), "TopLeft"),
		Description("The alignment of the checkbox in relation to the text."),
		Category("Appearance"),
		Localizable(true)]
		public ContentAlignment CheckAlign
		{
			get { return imageAlign; }
			set { imageAlign = value; ResetListLayout(); Refresh(); }
		}

		/// <summary>
		/// Gets the list of <see cref="CheckBoxListItem"/> associated with the control.
		/// </summary>
		[MergableProperty(false),
		Category("Data"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		Localizable(true),
		Description("List of checkboxes with optional subtext")]
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
		public bool ThreeState
		{
			get;
			set;
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
				OnItemCheckStateChanged();
			}
		}

		/// <summary>
		/// Gets the size of the image used to display the button.
		/// </summary>
		/// <param name="g">Current <see cref="Graphics"/> context.</param>
		/// <returns>The size of the image.</returns>
		protected override Size GetImageSize(Graphics g)
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
		/// Raises the <see cref="Control.KeyDown"/> event.
		/// </summary>
		/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				pressingItem = this.focusedIndex;
				InvalidateItem(pressingItem);
				base.Update();
				e.Handled = true;
			}
			base.OnKeyDown(e);
		}

		/// <summary>
		/// Raises the <see cref="Control.KeyUp"/> event.
		/// </summary>
		/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyUp(KeyEventArgs e)
		{
			if (pressingItem != -1)
			{
				int ci = pressingItem;
				pressingItem = -1;
				InvalidateItem(ci);
			}
			// Handle button press on Space
			base.OnKeyUp(e);
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseClick"/> event.
		/// </summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			if (hoverItem != -1)
			{
				SetFocused(hoverItem);
				ToggleItem(hoverItem);
			}
		}

		/// <summary>
		/// Raises the <see cref="Control.Paint"/> event.
		/// </summary>
		/// <param name="pe">An <see cref="PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs pe)
		{
			CheckBoxRenderer.DrawParentBackground(pe.Graphics, pe.ClipRectangle, this);
			base.OnPaint(pe);
		}

		/// <summary>
		/// Paints the specified item.
		/// </summary>
		/// <param name="g">A <see cref="Graphics"/> reference.</param>
		/// <param name="index">The index of the item.</param>
		/// <param name="bounds">The bounds in which to paint the item.</param>
		protected override void PaintItem(System.Drawing.Graphics g, int index, Rectangle bounds)
		{
			// Draw glyph
			CheckBoxListItem li = items[index];
			System.Windows.Forms.VisualStyles.CheckBoxState rbs = (System.Windows.Forms.VisualStyles.CheckBoxState)(((int)li.CheckState * 4) + 1);
			int idx = items.IndexOf(li);
			if (!this.Enabled || !li.Enabled)
				rbs += 3;
			else if (idx == pressingItem)
				rbs += 2;
			else if (idx == hoverItem)
				rbs++;
			Point gp = li.GlyphPosition;
			gp.Offset(bounds.Location);
			CheckBoxRenderer.DrawCheckBox(g, gp, rbs);

			// Draw text
			Rectangle tr = li.TextRect;
			tr.Offset(bounds.Location);
			TextRenderer.DrawText(g, li.Text, this.Font, tr, li.Enabled ? this.ForeColor : SystemColors.GrayText, tff);

			Rectangle str = li.SubtextRect;
			bool hasSubtext = !string.IsNullOrEmpty(li.Subtext);
			if (hasSubtext)
			{
				str.Offset(bounds.Location);
				TextRenderer.DrawText(g, li.Subtext, this.SubtextFont, str, li.Enabled ? this.SubtextForeColor : SystemColors.GrayText, tff);
			}

			// Draw focus rect
			if (idx == focusedIndex && this.Focused)
			{
				if (hasSubtext)
					tr = Rectangle.Union(tr, str);
				ControlPaint.DrawFocusRectangle(g, tr);
			}
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
						ret = true;
					break;
				case Keys.Enter:
					break;
				case Keys.Escape:
					break;
				case Keys.Up:
				case Keys.Left:
					if (FocusNextItem(this.FocusedItem, false))
						ret = true;
					break;
				case Keys.Space:
					ToggleItem(focusedIndex);
					break;
				case Keys.Tab:
					if (FocusNextItem(this.FocusedItem, !ke.Shift))
						ret = true;
					break;
				default:
					break;
			}
			if (ret) ke.SuppressKeyPress = true;
			return ret;
		}

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
			: this(text, subtext, null) { }

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
		public event EventHandler CheckStateChanged;

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
				}
			}
		}

		/// <summary>
		/// Determines whether the specified <see cref="CheckBoxListItem"/> is equal to the current <see cref="CheckBoxListItem"/>.
		/// </summary>
		/// <param name="b2">The <see cref="CheckBoxListItem"/> to compare with the current <see cref="CheckBoxListItem"/>.</param>
		/// <returns>
		/// true if the specified <see cref="CheckBoxListItem"/> is equal to the current <see cref="CheckBoxListItem"/>; otherwise, false.
		/// </returns>
		public bool Equals(CheckBoxListItem cb2)
		{
			return base.Equals((ButtonListItem)cb2) && this.CheckState == cb2.CheckState;
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
		/// Called when [clear].
		/// </summary>
		protected override void OnClear()
		{
			base.OnClear();
			parent.OnListChanged();
		}

		/// <summary>
		/// Called when [insert].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		protected override void OnInsert(int index, CheckBoxListItem value)
		{
			base.OnInsert(index, value);
			parent.OnListChanged();
		}

		/// <summary>
		/// Called when [remove].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		protected override void OnRemove(int index, CheckBoxListItem value)
		{
			base.OnRemove(index, value);
			parent.OnListChanged();
		}

		/// <summary>
		/// Called when [set].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="oldValue">The old value.</param>
		/// <param name="newValue">The new value.</param>
		protected override void OnSet(int index, CheckBoxListItem oldValue, CheckBoxListItem newValue)
		{
			base.OnSet(index, oldValue, newValue);
			if (!oldValue.Equals(newValue))
				parent.OnListChanged();
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
	}
}
