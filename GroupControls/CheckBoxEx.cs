using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GroupControls
{
	/// <summary>
	/// 
	/// </summary>
	public class CheckBoxEx : CheckBox
	{
		internal const int lrPadding = 3, tPadding = 2;

		internal static readonly ContentAlignment anyBottomAlignment = ContentAlignment.BottomLeft | ContentAlignment.BottomCenter | ContentAlignment.BottomRight;
		internal static readonly ContentAlignment anyCenterAlignment = ContentAlignment.TopCenter | ContentAlignment.MiddleCenter | ContentAlignment.BottomCenter;
		internal static readonly ContentAlignment anyMiddleAlignment = ContentAlignment.MiddleLeft | ContentAlignment.MiddleCenter | ContentAlignment.MiddleRight;
		internal static readonly ContentAlignment anyRightAlignment = ContentAlignment.TopRight | ContentAlignment.MiddleRight | ContentAlignment.BottomRight;

		private Size cachedGlyphSize = Size.Empty;
		private Rectangle[] cachedRects;
		private bool hot = false;
		private bool pressed = false;
		private string subtext;
		private Font subtextFont;
		private Color subtextForeColor = Color.Empty;
		private int subtextSeparatorHeight = 5;
		private TextFormatFlags tff = TextFormatFlags.WordBreak;

		/// <summary>
		/// Initializes a new instance of the <see cref="CheckBoxEx"/> class.
		/// </summary>
		public CheckBoxEx() : base()
		{
			AppearanceChanged += DisplayPropertyChanged;
			ForeColorChanged += DisplayPropertyChanged;
			Layout += LayoutPropertyChanged;
			RightToLeftChanged += LayoutPropertyChanged;
			StyleChanged += DisplayPropertyChanged;
			TextChanged += LayoutPropertyChanged;
			CheckAlign = TextAlign = ContentAlignment.TopLeft;
		}

		/// <summary>
		/// Gets or sets the horizontal and vertical alignment of the check mark on a <see cref="T:System.Windows.Forms.CheckBox" /> control.
		/// </summary>
		/// <returns>One of the <see cref="T:System.Drawing.ContentAlignment" /> values. The default value is MiddleLeft.</returns>
		[Category("Appearance"), DefaultValue(typeof(ContentAlignment), "TopLeft"), Bindable(true), Description("Determines the location of the check box inside the control."), Localizable(true)]
		public new ContentAlignment CheckAlign
		{
			get { return base.CheckAlign; }
			set { base.CheckAlign = value; LayoutPropertyChanged(this, EventArgs.Empty); }
		}

		/// <summary>
		/// Gets or sets the subtext.
		/// </summary>
		/// <value>The subtext.</value>
		[DefaultValue((string)null), Category("Appearance"), BindableAttribute(true)]
		public string Subtext
		{
			get { return subtext; }
			set { if (subtext != value) subtext = value; Refresh(); }
		}

		/// <summary>
		/// Gets or sets the font used to render the subtext of each item.
		/// </summary>
		/// <value>The subtext font.</value>
		[Description("The font used to display the item subtext."), Category("Appearance")]
		[AmbientValue((Font)null)]
		public Font SubtextFont
		{
			get { return subtextFont == null ? this.Font : subtextFont; }
			set { if (this.Font.Equals(value)) subtextFont = null; else subtextFont = value; Refresh(); }
		}

		/// <summary>
		/// Gets or sets the color of an item's subtext.
		/// </summary>
		/// <value>The color of the subtext.</value>
		[Category("Appearance"), Description("The color used to display the item subtext.")]
		[AmbientValue(typeof(Color), "Empty")]
		public Color SubtextForeColor
		{
			get
			{
				if (subtextForeColor.IsEmpty)
				{
					var o = GetParentProperty(this, "SubtextForeColor");
					return (o != null) ? (Color)o : this.ForeColor;
				}
				return subtextForeColor;
			}
			set { subtextForeColor = value; Refresh(); }
		}

		/// <summary>
		/// Gets or sets the number of pixels used to separate the text from the subtext within an item.
		/// </summary>
		/// <value>The number of pixels used to separate the text from the subtext.</value>
		[DefaultValue(5), Description("Number of pixels separating item text and subtext."), Category("Appearance")]
		[AmbientValue(-1)]
		public int SubtextSeparatorHeight
		{
			get
			{
				if (subtextSeparatorHeight == -1)
				{
					var o = GetParentProperty(this, "SubtextSeparatorHeight");
					if (o != null) return (int)o;
				}
				return subtextSeparatorHeight;
			}
			set { subtextSeparatorHeight = value; Refresh(); }
		}

		/// <summary>
		/// Gets or sets the alignment of the text on the <see cref="T:System.Windows.Forms.CheckBox" /> control.
		/// </summary>
		/// <returns>One of the <see cref="T:System.Drawing.ContentAlignment" /> values. The default is <see cref="F:System.Drawing.ContentAlignment.MiddleLeft" />.</returns>
		[Category("Appearance"), DefaultValue(typeof(ContentAlignment), "TopLeft"), Bindable(true), Description("The alignment of the text that will be displayed on the control."), Localizable(true)]
		public new ContentAlignment TextAlign
		{
			get { return base.TextAlign; }
			set { base.TextAlign = value; LayoutPropertyChanged(this, EventArgs.Empty); }
		}

		/// <summary>
		/// Gets the TextFormatFlags based on alignments.
		/// </summary>
		/// <value>The TextFormatFlags.</value>
		private TextFormatFlags TextFormatFlags
		{
			get { return this.RightToLeft == RightToLeft.Yes ? tff | TextFormatFlags.RightToLeft : tff; }
		}

		/// <summary>
		/// Retrieves the size of a rectangular area into which a control can be fitted.
		/// </summary>
		/// <param name="proposedSize">The custom-sized area for a control.</param>
		/// <returns>
		/// An ordered pair of type <see cref="T:System.Drawing.Size" /> representing the width and height of a rectangle.
		/// </returns>
		public override Size GetPreferredSize(Size proposedSize)
		{
			using (Graphics g = this.CreateGraphics())
			{
				Rectangle bounds = new Rectangle(Point.Empty, proposedSize);
				if (bounds.Width > this.Width) bounds.Width = this.Width;
				Rectangle glyphBounds, focusBounds, textBounds, subtextBounds;
				int h = GetElementBounds(g, bounds, out glyphBounds, out textBounds, out subtextBounds, out focusBounds);
				System.Diagnostics.Debug.WriteLine(string.Format("CheckBoxEx.GetPreferredSize: client={0}; proposed={1}; result={2}; autosize={3}", this.ClientRectangle, proposedSize, h, this.AutoSize));
				return new Size(bounds.Width, h);
			}
		}

		internal static object GetParentProperty(Control ctrl, string property)
		{
			Control parent = ctrl.Parent;
			if (parent != null)
			{
				var prop = parent.GetType().GetProperty(property);
				if (prop != null)
					return prop.GetValue(parent, null);
				else
					return GetParentProperty(parent, property);
			}
			return null;
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

		protected void DisplayPropertyChanged(object sender, EventArgs e)
		{
			Refresh();
		}

		protected virtual Size GetButtonSize(Graphics g)
		{
			return cachedGlyphSize.IsEmpty ? cachedGlyphSize = CheckBoxRenderer.GetGlyphSize(g, System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal) : cachedGlyphSize;
		}

		protected void LayoutPropertyChanged(object sender, EventArgs e)
		{
			RebuildTextFormatFlags();
			cachedRects = null;
			Refresh();
		}

		protected override void OnMouseDown(MouseEventArgs mevent)
		{
			base.OnMouseDown(mevent);
			pressed = true;
			Refresh();
		}

		protected override void OnMouseEnter(EventArgs eventargs)
		{
			base.OnMouseEnter(eventargs);
			hot = true;
			Refresh();
		}

		protected override void OnMouseLeave(EventArgs eventargs)
		{
			base.OnMouseLeave(eventargs);
			hot = false;
			Refresh();
		}

		protected override void OnMouseUp(MouseEventArgs mevent)
		{
			base.OnMouseUp(mevent);
			pressed = false;
			Refresh();
		}

		/// <summary>
		/// Raises the <see cref="E:Paint"/> event.
		/// </summary>
		/// <param name="pevent">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		protected override void OnPaint(PaintEventArgs pevent)
		{
			if (this.UseVisualStyleBackColor)
				CheckBoxRenderer.DrawParentBackground(pevent.Graphics, pevent.ClipRectangle, this);
			else
				pevent.Graphics.Clear(this.BackColor);
			Rectangle glyphBounds, focusBounds, textBounds, subtextBounds;
			int h = GetElementBounds(pevent.Graphics, pevent.ClipRectangle, out glyphBounds, out textBounds, out subtextBounds, out focusBounds);
			if (this.AutoSize && h != this.Height)
				this.Height = h;
			CheckBoxRenderer.DrawCheckBox(pevent.Graphics, glyphBounds.Location, GetCheckBoxState());
			TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, textBounds, this.ForeColor, this.TextFormatFlags);
			if (subtextBounds != Rectangle.Empty)
				TextRenderer.DrawText(pevent.Graphics, Subtext, this.SubtextFont, subtextBounds, this.SubtextForeColor, this.TextFormatFlags);
			if (this.Focused)
				ControlPaint.DrawFocusRectangle(pevent.Graphics, focusBounds);
		}

		private System.Windows.Forms.VisualStyles.CheckBoxState GetCheckBoxState()
		{
			System.Windows.Forms.VisualStyles.CheckBoxState ret = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedNormal;
			if (!this.Enabled)
				ret = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedDisabled;
			else if (pressed)
				ret = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedPressed;
			else if (hot)
				ret = System.Windows.Forms.VisualStyles.CheckBoxState.UncheckedHot;

			if (this.Checked)
				ret += 4;
			else if (this.CheckState == System.Windows.Forms.CheckState.Indeterminate)
				ret += 8;

			return ret;
		}

		private int GetElementBounds(Graphics g, Rectangle bounds, out Rectangle glyphBounds, out Rectangle textBounds, out Rectangle subtextBounds, out Rectangle focusBounds)
		{
			ContentAlignment chkAlign = base.RtlTranslateContent(this.CheckAlign);
			ContentAlignment txtAlign = base.RtlTranslateContent(this.TextAlign);

			if (cachedRects == null || bounds.Width != cachedRects[0].Width)
			{
				System.Diagnostics.Debug.WriteLine("CheckBoxEx.GetElementBounds: Recalculating...");
				glyphBounds = textBounds = subtextBounds = focusBounds = Rectangle.Empty;

				// Get glyph size
				glyphBounds.Size = GetButtonSize(g);

				// Calculate text size
				Size textSize = new Size(bounds.Width, Int32.MaxValue);
				if ((chkAlign & anyCenterAlignment) == (ContentAlignment)0)
					textSize.Width -= (glyphBounds.Width + lrPadding);

				Size tsz = TextRenderer.MeasureText(g, this.Text, this.Font, textSize, TextFormatFlags);
				textBounds = new Rectangle(0, 0, textSize.Width, tsz.Height);
				Size stsz = Size.Empty;
				if (!string.IsNullOrEmpty(this.Subtext))
				{
					stsz = TextRenderer.MeasureText(g, this.Subtext, this.SubtextFont, textSize, TextFormatFlags);
					subtextBounds = new Rectangle(0, tsz.Height + subtextSeparatorHeight, textSize.Width, stsz.Height);
				}

				// Calculate minimum item height
				int minHeight = textBounds.Height;
				if (!subtextBounds.IsEmpty)
					minHeight += (subtextBounds.Height + subtextSeparatorHeight);
				if ((chkAlign == ContentAlignment.TopCenter) || (chkAlign == ContentAlignment.BottomCenter))
					minHeight += (glyphBounds.Height + tPadding);

				Size itemSize = new Size(bounds.Width, minHeight);

				// Set relative position of glyph and text
				if ((chkAlign & anyBottomAlignment) != (ContentAlignment)0)
				{
					glyphBounds.Y = itemSize.Height - glyphBounds.Height;
				}
				else if ((chkAlign & anyMiddleAlignment) != (ContentAlignment)0)
				{
					glyphBounds.Y = (itemSize.Height - glyphBounds.Height) / 2;
				}
				else
				{
					if (chkAlign == ContentAlignment.TopCenter)
					{
						textBounds.Offset(0, glyphBounds.Height + tPadding);
						subtextBounds.Offset(0, glyphBounds.Height + tPadding);
					}
				}
				if ((chkAlign & anyRightAlignment) != (ContentAlignment)0)
				{
					glyphBounds.X = itemSize.Width - glyphBounds.Width;
					textBounds.X = subtextBounds.X = 0;
				}
				else if ((chkAlign & anyCenterAlignment) != (ContentAlignment)0)
				{
					glyphBounds.X = (itemSize.Width - glyphBounds.Width) / 2;
				}
				else
				{
					textBounds.Offset(glyphBounds.Width + lrPadding, 0);
					subtextBounds.Offset(glyphBounds.Width + lrPadding, 0);
				}

				// Set text focus rectangle
				focusBounds = Rectangle.Union(textBounds, subtextBounds);

				bounds.Height = itemSize.Height;

				cachedRects = new Rectangle[] { bounds, glyphBounds, textBounds, subtextBounds, focusBounds };
			}
			else
			{
				bounds.Height = cachedRects[0].Height;
				glyphBounds = cachedRects[1];
				textBounds = cachedRects[2];
				subtextBounds = cachedRects[3];
				focusBounds = cachedRects[4];
			}

			int gOff = 0, tOff = 0;
			if (!this.AutoSize)
			{
				if ((chkAlign & anyBottomAlignment) != (ContentAlignment)0)
					gOff = this.Height - bounds.Height;
				else if ((chkAlign & anyMiddleAlignment) != (ContentAlignment)0)
					gOff = (this.Height - bounds.Height) / 2;

				if ((txtAlign & anyBottomAlignment) != (ContentAlignment)0)
					tOff = this.Height - bounds.Height;
				else if ((txtAlign & anyMiddleAlignment) != (ContentAlignment)0)
					tOff = (this.Height - bounds.Height) / 2;
			}
			System.Diagnostics.Debug.WriteLine(string.Format("CheckBoxEx.GetElementBounds: client={4}; bounds={0}; focus={1}; autosize={2}; toff={3}", bounds, focusBounds, this.AutoSize, tOff, this.ClientRectangle));

			Point offset = bounds.Location;
			glyphBounds.Offset(offset.X, offset.Y + gOff);
			focusBounds.Offset(offset.X, offset.Y + tOff);
			textBounds.Offset(offset.X, offset.Y + tOff);
			subtextBounds.Offset(offset.X, offset.Y + tOff);
			return bounds.Height;
		}

		private void RebuildTextFormatFlags()
		{
			ContentAlignment txtAlign = base.RtlTranslateContent(this.TextAlign);
			tff = TextFormatFlags.WordBreak | TextFormatFlags.PreserveGraphicsTranslateTransform;
			if ((txtAlign & anyRightAlignment) != (ContentAlignment)0)
				tff |= TextFormatFlags.Right;
			else if ((txtAlign & anyCenterAlignment) != (ContentAlignment)0)
				tff |= TextFormatFlags.HorizontalCenter;
		}
	}
}