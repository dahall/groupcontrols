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
	[ToolboxBitmap(typeof(RadioButtonList)), DefaultProperty("Items")]
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
		/// Gets or sets the alignment of the check box in relation to the text.
		/// </summary>
		[DefaultValue(typeof(ContentAlignment), "TopLeft"),
		Description("Determines the location of the check box in relation to the text."),
		Category("Appearance"),
		Localizable(true)]
		public ContentAlignment CheckAlign
		{
			get { return imageAlign; }
			set { imageAlign = value; ResetListLayout(); Refresh(); }
		}

		/// <summary>
		/// Gets the list of <see cref="RadioButtonListItem"/> associated with the control.
		/// </summary>
		[MergableProperty(false),
		Category("Data"),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		Localizable(true),
		Description("List of radio buttons with optional subtext")]
		public virtual RadioButtonListItemCollection Items
		{
			get { return items; }
		}

		/// <summary>
		/// Gets or sets the index specifying the currently selected item.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		Bindable(true),
		DefaultValue(-1),
		Category("Data"),
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
					if (selectedIndex > -1)
						Items[selectedIndex].Checked = false;
					selectedIndex = value;
					Items[selectedIndex].Checked = true;
					OnSelectedIndexChanged(EventArgs.Empty);
				}
			}
		}

		/// <summary>
		/// Gets or sets currently selected item in the list.
		/// </summary>
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
		Description("Gets or sets currently selected item in the list."),
		Browsable(false),
		Bindable(true),
		DefaultValue((string)null),
		Category("Data")]
		public RadioButtonListItem SelectedItem
		{
			get
			{
				return this.selectedIndex == -1 ? null : items[this.selectedIndex];
			}
			set
			{
				SelectedIndex = items.IndexOf(value);
			}
		}

		private bool SelectNextItem(RadioButtonListItem i, bool forward)
		{
			if (items.Count > 0)
			{
				if (i == null)
				{
					if (forward)
						SetSelected(0);
					else
						SetSelected(items.Count - 1);
					return true;
				}
				else
				{
					int idx = items.IndexOf(i);
					if (idx == -1)
						throw new IndexOutOfRangeException();
					if ((idx == 0 && !forward) || (idx == (items.Count - 1) && forward))
						return false;
					if (forward)
						SetSelected(idx + 1);
					else
						SetSelected(idx - 1);
					return true;
				}
			}
			return false;
		}

		private void SetSelected(int itemIndex)
		{
			if (itemIndex != -1 && !items[itemIndex].Enabled)
				return;

			int oldSelect = selectedIndex;
			this.SelectedIndex = itemIndex;
			// clear old selected item
			if (oldSelect > -1)
			{
				InvalidateItem(oldSelect);
				if (this.Focused)
					Invalidate(Items[oldSelect].TextRect);
			}
			// Set new item
			if (itemIndex > -1)
			{
				InvalidateItem(selectedIndex);
				if (this.Focused)
					Invalidate(Items[selectedIndex].TextRect);
			}

			SetFocused(itemIndex);
		}

		#region Protected Methods

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
			else if (focusedIndex == -1 && items.Count > 0)
			{
				SetFocused(0);
			}
		}

		/// <summary>
		/// Raises the <see cref="Control.KeyDown"/> event.
		/// </summary>
		/// <param name="e">An <see cref="KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Space)
			{
				pressingItem = this.selectedIndex;
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
		/// Raises the <see cref="Control.LostFocus"/> event.
		/// </summary>
		/// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
		protected override void OnLostFocus(EventArgs e)
		{
			base.OnLostFocus(e);
			if (selectedIndex != -1)
				Invalidate(this.itemBounds[selectedIndex]);
		}

		/// <summary>
		/// Raises the <see cref="Control.MouseClick"/> event.
		/// </summary>
		/// <param name="e">An <see cref="MouseEventArgs"/> that contains the event data.</param>
		protected override void OnMouseClick(MouseEventArgs e)
		{
			base.OnMouseClick(e);
			if (hoverItem != -1)
				SetSelected(hoverItem);
		}

		/// <summary>
		/// Raises the <see cref="Control.Paint"/> event.
		/// </summary>
		/// <param name="pe">An <see cref="PaintEventArgs"/> that contains the event data.</param>
		protected override void OnPaint(PaintEventArgs pe)
		{
			RadioButtonRenderer.DrawParentBackground(pe.Graphics, pe.ClipRectangle, this);
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
			else if (idx == pressingItem)
				rbs += 2;
			else if (idx == hoverItem)
				rbs++;
			Point gp = li.GlyphPosition;
			gp.Offset(bounds.Location);
			RadioButtonRenderer.DrawRadioButton(g, gp, rbs);

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
		protected override bool ProcessKey(System.Windows.Forms.KeyEventArgs ke)
		{
			switch (ke.KeyCode)
			{
				case Keys.Down:
				case Keys.Right:
					if (SelectNextItem(this.FocusedItem as RadioButtonListItem, true))
						return true;
					break;
				case Keys.Enter:
					break;
				case Keys.Escape:
					break;
				case Keys.Up:
				case Keys.Left:
					if (SelectNextItem(this.FocusedItem as RadioButtonListItem, false))
						return true;
					break;
				case Keys.Space:
					if (!this.FocusedItem.Checked)
						SetSelected(this.focusedIndex);
					break;
				case Keys.Tab:
					break;
				default:
					break;
			}
			return false;
		}

		#endregion Protected Methods

		#region Events

		/// <summary>
		/// Occurs when the selected index has changed.
		/// </summary>
		public event EventHandler SelectedIndexChanged;

		#endregion Events
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
		public RadioButtonListItem () { }

		/// <summary>
		/// Initializes a new instance of the <see cref="RadioButtonListItem"/> class.
		/// </summary>
		/// <param name="text">Text displayed next to radio button.</param>
		/// <param name="subtext">Subtext displayed under text.</param>
		public RadioButtonListItem(string text, string subtext)
			: base(text, subtext, null) { }

		/// <summary>
		/// Initializes a new instance of the <see cref="RadioButtonListItem"/> class.
		/// </summary>
		/// <param name="text">Text displayed next to radio button.</param>
		/// <param name="subtext">Subtext displayed under text.</param>
		/// <param name="tooltiptext">Tooltip displayed for the item.</param>
		public RadioButtonListItem(string text, string subtext, string tooltiptext)
			: base(text, subtext, tooltiptext) { }
	}

	/// <summary>
	/// 
	/// </summary>
	[Editor(typeof(System.ComponentModel.Design.CollectionEditor), typeof(System.Drawing.Design.UITypeEditor))]
	public class RadioButtonListItemCollection : EventedList<RadioButtonListItem>
	{
		private RadioButtonList parent;

		internal RadioButtonListItemCollection(RadioButtonList list)
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
		protected override void OnInsert(int index, RadioButtonListItem value)
		{
			base.OnInsert(index, value);
			parent.OnListChanged();
		}

		/// <summary>
		/// Called when [remove].
		/// </summary>
		/// <param name="index">The index.</param>
		/// <param name="value">The value.</param>
		protected override void OnRemove(int index, RadioButtonListItem value)
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
		protected override void OnSet(int index, RadioButtonListItem oldValue, RadioButtonListItem newValue)
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
			base.Add(new RadioButtonListItem(text, subtext));
		}
	}
}