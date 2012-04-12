using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GroupControls
{
	/// <summary>
	/// Represents a windows control that displays a list of radio button items with optional subtext entries.
	/// </summary>
	[ToolboxBitmap(typeof(RadioButtonList)), DefaultProperty("Items"), DefaultEvent("SelectedIndexChanged")] 
	public class RadioButtonList : ButtonListBase
	{
		private RadioButtonListItemCollection items;
		private int selectedIndex = -1;

		/// <summary>
		/// Creates a new instance of a <see cref="RadioButtonList"/>.
		/// </summary>
		public RadioButtonList() : base()
		{
			items = new RadioButtonListItemCollection(this);
		}

		/// <summary>
		/// Occurs when the selected index has changed.
		/// </summary>
		[Category("Behavior"), Description("Occurs when the value of the SelectedIndex property changes.")]
		public event EventHandler SelectedIndexChanged;

		/// <summary>
		/// Gets or sets the alignment of the check box in relation to the text.
		/// </summary>
		[DefaultValue(typeof(ContentAlignment), "TopLeft"), Category("Appearance"), Localizable(true),
		Description("Determines the location of the check box in relation to the text.")]
		public ContentAlignment CheckAlign
		{
			get { return ImageAlign; }
			set { ImageAlign = value; ResetListLayout(); Refresh(); }
		}

		/// <summary>
		/// Gets the list of <see cref="RadioButtonListItem"/> associated with the control.
		/// </summary>
		[MergableProperty(false), Category("Data"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		Localizable(true), Description("List of radio buttons with optional subtext")]
		public virtual RadioButtonListItemCollection Items
		{
			get { return items; }
		}

		/// <summary>
		/// Gets or sets the index specifying the currently selected item.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		Bindable(true), DefaultValue(-1), Category("Data"),
		Description("Gets or sets the index specifying the currently selected item.")]
		public int SelectedIndex
		{
			get { return selectedIndex; }
			set
			{
				if (selectedIndex != value)
				{
					if (value < -1 || value >= items.Count)
						throw new IndexOutOfRangeException();

					// Clear old selected item
					int oldSelect = selectedIndex;
					if (oldSelect > -1 && oldSelect < items.Count)
					{
						Items[oldSelect].Checked = false;
						InvalidateItem(oldSelect);
						if (this.Focused)
							Invalidate(Items[oldSelect].TextRect);
					}

					// Set new item
					selectedIndex = value;
					if (selectedIndex > -1)
					{
						Items[selectedIndex].Checked = true;
						InvalidateItem(selectedIndex);
						if (this.Focused)
							Invalidate(Items[selectedIndex].TextRect);
					}
					SetFocused(selectedIndex);

					OnSelectedIndexChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Gets or sets currently selected item in the list.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		Description("Gets or sets currently selected item in the list."),
		Browsable(false), Bindable(true), DefaultValue((string)null), Category("Data")]
		public RadioButtonListItem SelectedItem
		{
			get { return this.selectedIndex == -1 ? null : items[this.selectedIndex]; }
			set { SelectedIndex = items.IndexOf(value); }
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
		/// Gets the size of the image used to display the button.
		/// </summary>
		/// <param name="g">Current <see cref="Graphics"/> context.</param>
		/// <returns>The size of the image.</returns>
		protected override Size GetImageSize(Graphics g)
		{
			return RadioButtonRenderer.GetGlyphSize(g, System.Windows.Forms.VisualStyles.RadioButtonState.CheckedNormal);
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
			foreach (RadioButtonListItem item in items)
			{
				if (Control.IsMnemonic(charCode, item.Text))
				{
					SetSelected(items.IndexOf(item));
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Raises the <see cref="Control.GotFocus"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			if (selectedIndex != -1)
				InvalidateItem(selectedIndex);
			else if (FocusedIndex == -1 && items.Count > 0)
				SetFocused(GetNextEnabledItemIndex(-1, true));
		}

		/// <summary>
		/// Raises the <see cref="Control.KeyDown"/> event.
		/// </summary>
		/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				PressingItem = this.selectedIndex;
				InvalidateItem(PressingItem);
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
		/// Raises the <see cref="Control.LostFocus"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			if (selectedIndex != -1)
				InvalidateItem(selectedIndex);
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseClick"/> event.
		/// </summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			if (HoverItem != -1)
				SetSelected(HoverItem);
		}

		/// <summary>
		/// Raises the <see cref="Control.Paint"/> event.
		/// </summary>
		/// <param name="pe">An <see cref="PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs pe)
		{
			if (Application.RenderWithVisualStyles)
				RadioButtonRenderer.DrawParentBackground(pe.Graphics, pe.ClipRectangle, this);
			else
				pe.Graphics.Clear(this.BackColor);
			base.OnPaint(pe);
		}

		/// <summary>
		/// Raises the <see cref="E:SelectedIndexChanged"/> event.
		/// </summary>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		protected virtual void OnSelectedIndexChanged(EventArgs e)
		{
			EventHandler handler1 = this.SelectedIndexChanged;
			if (handler1 != null)
				handler1(this, e);
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
			RadioButtonListItem li = items[index];
			System.Windows.Forms.VisualStyles.RadioButtonState rbs = li.Checked ? System.Windows.Forms.VisualStyles.RadioButtonState.CheckedNormal : System.Windows.Forms.VisualStyles.RadioButtonState.UncheckedNormal;
			int idx = items.IndexOf(li);
			if (!this.Enabled || !li.Enabled)
				rbs += 3;
			else if (idx == PressingItem)
				rbs += 2;
			else if (idx == HoverItem)
				rbs++;
			Point gp = li.GlyphPosition;
			gp.Offset(bounds.Location);
			RadioButtonRenderer.DrawRadioButton(g, gp, rbs);

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
			if (idx == FocusedIndex && this.Focused)
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
		protected override bool ProcessKey(System.Windows.Forms.KeyEventArgs ke)
		{
			switch (ke.KeyCode)
			{
				case Keys.Down:
				case Keys.Right:
					if (SelectNextItem(this.FocusedItem as RadioButtonListItem, true))
						return true;
					break;
				case Keys.Up:
				case Keys.Left:
					if (SelectNextItem(this.FocusedItem as RadioButtonListItem, false))
						return true;
					break;
				case Keys.Space:
					if (this.FocusedItem != null && !this.FocusedItem.Checked)
						SetSelected(this.FocusedIndex);
					break;
				case Keys.Tab:
				case Keys.Enter:
				case Keys.Escape:
				default:
					break;
			}
			return false;
		}

		/// <summary>
		/// Resets the list's layout.
		/// </summary>
		protected override void ResetListLayout()
		{
			base.ResetListLayout();
			// Get the change to the selected index based on item Checked property
			this.SelectedIndex = items.CheckedItemIndex;
		}

		private bool SelectNextItem(RadioButtonListItem i, bool forward)
		{
			if (items.Count > 0)
			{
				int idx = -1;
				if (i != null && (idx = this.BaseItems.IndexOf(i)) == -1)
					throw new IndexOutOfRangeException();
				idx = GetNextEnabledItemIndex(idx, forward);
				if (idx != -1)
				{
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
			this.SelectedIndex = itemIndex;
		}
	}

	/// <summary>
	/// An item associated with a <see cref="RadioButtonList"/>.
	/// </summary>
	[DefaultProperty("Text")]
	public class RadioButtonListItem : ButtonListItem
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="RadioButtonListItem"/> class.
		/// </summary>
		public RadioButtonListItem() : base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RadioButtonListItem"/> class.
		/// </summary>
		/// <param name="text">Text displayed next to radio button.</param>
		/// <param name="subtext">Subtext displayed under text.</param>
		/// <param name="tooltiptext">Tooltip displayed for the item.</param>
		public RadioButtonListItem(string text, string subtext = null, string tooltiptext = null)
			: base(text, subtext, tooltiptext)
		{
		}
	}

	/// <summary>
	/// Collection of <see cref="RadioButtonListItem"/> items.
	/// </summary>
	[Editor(typeof(System.ComponentModel.Design.CollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public class RadioButtonListItemCollection : EventedList<RadioButtonListItem>
	{
		private RadioButtonList parent;

		internal RadioButtonListItemCollection(RadioButtonList list)
		{
			parent = list;
		}

		internal int CheckedItemIndex
		{
			get { return base.FindIndex(delegate(RadioButtonListItem item) { return item.Checked; }); }
		}

		/// <summary>
		/// Adds a new item to the collection.
		/// </summary>
		/// <param name="text">Item text.</param>
		/// <param name="subtext">Item subtext.</param>
		public void Add(string text, string subtext)
		{
			base.Add(new RadioButtonListItem(text, subtext));
		}

		/// <summary>
		/// Called when [item added].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		protected override void OnItemAdded(int index, RadioButtonListItem value)
		{
			base.OnItemAdded(index, value);
			parent.OnListChanged();
		}

		/// <summary>
		/// Called when [item changed].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="oldValue">The old value.</param>
		/// <param name="newValue">The new value.</param>
		protected override void OnItemChanged(int index, RadioButtonListItem oldValue, RadioButtonListItem newValue)
		{
			base.OnItemChanged(index, oldValue, newValue);
			if (!oldValue.Equals(newValue))
				parent.OnListChanged();
		}

		/// <summary>
		/// Called when [item deleted].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		protected override void OnItemDeleted(int index, RadioButtonListItem value)
		{
			base.OnItemDeleted(index, value);
			parent.OnListChanged();
		}
	}
}