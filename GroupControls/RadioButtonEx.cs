using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace GroupControls
{
	/// <summary>
	/// 
	/// </summary>
	public class RadioButtonEx : RadioButton
	{
		internal const int lrPadding = 3, tbPadding = 2, tPadding = 1;

		private Size cachedGlyphSize = Size.Empty;
		private Rectangle[] cachedRects;
		private bool hot = false;
		private int lastWidthBeforeAutoSize;
		private bool pressed = false;
		private string subtext;
		private Font subtextFont;
		private Color subtextForeColor = Color.Empty;
		private int subtextSeparatorHeight = 5;
		private TextFormatFlags tff = TextFormatFlags.WordBreak;

		/// <summary>
		/// Initializes a new instance of the <see cref="RadioButtonEx"/> class.
		/// </summary>
		public RadioButtonEx() : base()
		{
			AppearanceChanged += DisplayPropertyChanged;
			ForeColorChanged += DisplayPropertyChanged;
			Layout += LayoutPropertyChanged;
			RightToLeftChanged += LayoutPropertyChanged;
			StyleChanged += DisplayPropertyChanged;
			TextChanged += LayoutPropertyChanged;
			CheckAlign = TextAlign = ContentAlignment.TopLeft;
			lastWidthBeforeAutoSize = 200; // base.DefaultMinimumSize.Width;
		}

		public override bool AutoSize
		{
			get { return base.AutoSize; }
			set { if (base.AutoSize) lastWidthBeforeAutoSize = this.Width; base.AutoSize = value; }
		}

		/// <summary>
		/// Gets or sets the horizontal and vertical alignment of the check mark on a <see cref="T:System.Windows.Forms.RadioButton" /> control.
		/// </summary>
		/// <returns>One of the <see cref="T:System.Drawing.ContentAlignment" /> values. The default value is MiddleLeft.</returns>
		[Category("Appearance"), DefaultValue(typeof(ContentAlignment), "TopLeft"), Bindable(true), Description("Determines the location of the check box inside the control."), Localizable(true)]
		public new ContentAlignment CheckAlign
		{
			get { return base.CheckAlign; }
			set { base.CheckAlign = value; RefreshLayout(); }
		}

		/// <summary>
		/// Gets or sets the subtext.
		/// </summary>
		/// <value>The subtext.</value>
		[DefaultValue((string)null), Category("Appearance"), BindableAttribute(true)]
		public string Subtext
		{
			get { return subtext; }
			set { if (subtext != value) subtext = value; RefreshLayout(); }
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
			set { if (this.Font.Equals(value)) subtextFont = null; else subtextFont = value; RefreshLayout(); }
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
			set { subtextSeparatorHeight = value; RefreshLayout(); }
		}

		/// <summary>
		/// Gets or sets the alignment of the text on the <see cref="T:System.Windows.Forms.RadioButton" /> control.
		/// </summary>
		/// <returns>One of the <see cref="T:System.Drawing.ContentAlignment" /> values. The default is <see cref="F:System.Drawing.ContentAlignment.MiddleLeft" />.</returns>
		[Category("Appearance"), DefaultValue(typeof(ContentAlignment), "TopLeft"), Bindable(true), Description("The alignment of the text that will be displayed on the control."), Localizable(true)]
		public new ContentAlignment TextAlign
		{
			get { return base.TextAlign; }
			set { base.TextAlign = value; RefreshLayout(); }
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
				System.Diagnostics.Debug.WriteLine(string.Format("RadioButtonEx.GetPreferredSize: client={0}; proposed={1}; result={2}; autosize={3}", this.ClientRectangle, proposedSize, h, this.AutoSize));
				return new Size(Rectangle.Union(focusBounds, glyphBounds).Width, h);
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
			if (cachedGlyphSize.IsEmpty)
			{
				cachedGlyphSize = RadioButtonRenderer.GetGlyphSize(g, System.Windows.Forms.VisualStyles.RadioButtonState.UncheckedNormal);
				cachedGlyphSize.Height = (int)((float)cachedGlyphSize.Height * (g.DpiX / 96f));
				cachedGlyphSize.Width = (int)((float)cachedGlyphSize.Width * (g.DpiX / 96f));
			}
			return cachedGlyphSize;
		}

		protected void LayoutPropertyChanged(object sender, EventArgs e)
		{
			RefreshLayout();
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
			/*if (this.UseVisualStyleBackColor)
				RadioButtonRenderer.DrawParentBackground(pevent.Graphics, pevent.ClipRectangle, this);
			else*/
				pevent.Graphics.Clear(this.BackColor);
			Rectangle glyphBounds, focusBounds, textBounds, subtextBounds;
			int h = GetElementBounds(pevent.Graphics, pevent.ClipRectangle, out glyphBounds, out textBounds, out subtextBounds, out focusBounds);
			if (this.AutoSize)
				this.Size = new Size(Rectangle.Union(glyphBounds, focusBounds).Width, h);
			RadioButtonRenderer.DrawRadioButton(pevent.Graphics, glyphBounds.Location, GetRadioButtonState());
			TextRenderer.DrawText(pevent.Graphics, this.Text, this.Font, textBounds, this.ForeColor, this.TextFormatFlags);
			if (subtextBounds != Rectangle.Empty)
				TextRenderer.DrawText(pevent.Graphics, Subtext, this.SubtextFont, subtextBounds, this.SubtextForeColor, this.TextFormatFlags);
			//if (this.Focused)
				ControlPaint.DrawFocusRectangle(pevent.Graphics, focusBounds);
		}

		private System.Windows.Forms.VisualStyles.RadioButtonState GetRadioButtonState()
		{
			System.Windows.Forms.VisualStyles.RadioButtonState ret = System.Windows.Forms.VisualStyles.RadioButtonState.UncheckedNormal;
			if (!this.Enabled)
				ret = System.Windows.Forms.VisualStyles.RadioButtonState.UncheckedDisabled;
			else if (pressed)
				ret = System.Windows.Forms.VisualStyles.RadioButtonState.UncheckedPressed;
			else if (hot)
				ret = System.Windows.Forms.VisualStyles.RadioButtonState.UncheckedHot;

			if (this.Checked)
				ret += 4;

			return ret;
		}

		private int GetElementBounds(Graphics g, Rectangle bounds, out Rectangle glyphBounds, out Rectangle textBounds, out Rectangle subtextBounds, out Rectangle focusBounds)
		{
			ContentAlignment chkAlign = base.RtlTranslateContent(this.CheckAlign);
			ContentAlignment txtAlign = base.RtlTranslateContent(this.TextAlign);

			if (cachedRects == null || bounds.Width != cachedRects[0].Width)
			{
				System.Diagnostics.Debug.WriteLine("RadioButtonEx.GetElementBounds: Recalculating...");
				glyphBounds = textBounds = subtextBounds = focusBounds = Rectangle.Empty;

				// Get glyph size
				glyphBounds.Size = GetButtonSize(g);

				// Reset width if AutoSized
				if (this.AutoSize)
					bounds.Width = Math.Max(bounds.Width, lastWidthBeforeAutoSize);

				// Calculate text size
				Size textSize = new Size(bounds.Width, Int32.MaxValue);
				if (!IsAligned(chkAlign, SideAlignment.Center))
					textSize.Width -= (glyphBounds.Width + lrPadding);

				Size tsz = TextRenderer.MeasureText(g, this.Text, this.Font, textSize, TextFormatFlags);
				textBounds = new Rectangle(new Point(0, tPadding), tsz);
				Size stsz = Size.Empty;
				if (!string.IsNullOrEmpty(this.Subtext))
				{
					stsz = TextRenderer.MeasureText(g, this.Subtext, this.SubtextFont, textSize, TextFormatFlags);
					subtextBounds = new Rectangle(0, tsz.Height + subtextSeparatorHeight + tPadding, stsz.Width, stsz.Height);
					int wDiff = (stsz.Width - tsz.Width) / (IsAligned(txtAlign, SideAlignment.Center) ? 2 : 1);
					if (!IsAligned(txtAlign, SideAlignment.Left))
					{
						if (wDiff >= 0)
							textBounds.X = wDiff;
						else
							subtextBounds.X = -wDiff;
					}
				}
				Size totalTextSize = new Size(Math.Max(tsz.Width, stsz.Width), tsz.Height + (stsz.Height == 0 ? 0 : stsz.Height + subtextSeparatorHeight));

				// Calculate minimum item height
				int minHeight = textBounds.Height + tPadding;
				if (!subtextBounds.IsEmpty)
					minHeight += (subtextBounds.Height + subtextSeparatorHeight);
				if ((chkAlign == ContentAlignment.TopCenter) || (chkAlign == ContentAlignment.BottomCenter))
					minHeight += (glyphBounds.Height + tbPadding);

				Size itemSize = new Size(bounds.Width, this.AutoSize ? minHeight : bounds.Height);

				// Set relative position of glyph and text
				Rectangle textConstrainedRect = new Rectangle(0, 0, textSize.Width, itemSize.Height);
				if (IsAligned(chkAlign, SideAlignment.Left))
				{
					glyphBounds.Y += 2;
					textConstrainedRect.X = (glyphBounds.Width + lrPadding);
				}
				else if (IsAligned(chkAlign, SideAlignment.Center))
				{
					glyphBounds.X += (itemSize.Width - glyphBounds.Width) / 2;
					glyphBounds.Y -= 1;
				}
				else
				{
					glyphBounds.X += itemSize.Width - glyphBounds.Width;
					glyphBounds.Y += 2;
				}
				if (IsAligned(chkAlign, SideAlignment.Middle))
				{
					glyphBounds.Y += (itemSize.Height - glyphBounds.Height) / 2;
				}
				else if (IsAligned(chkAlign, SideAlignment.Bottom))
				{
					glyphBounds.Y += itemSize.Height - glyphBounds.Height;
					if (chkAlign == ContentAlignment.BottomCenter)
						textConstrainedRect.Height -= (glyphBounds.Height + tbPadding);
				}
				else if (chkAlign == ContentAlignment.TopCenter)
				{
					textConstrainedRect.Y += (glyphBounds.Height + tbPadding);
					textConstrainedRect.Height -= (glyphBounds.Height + tbPadding);
				}
				Point adj = Point.Empty;
				if (IsAligned(txtAlign, SideAlignment.Bottom))
					adj.Y = (textConstrainedRect.Height - totalTextSize.Height);
				else if (IsAligned(txtAlign, SideAlignment.Middle))
					adj.Y = (textConstrainedRect.Height - totalTextSize.Height) / 2;
				if (IsAligned(txtAlign, SideAlignment.Right))
					adj.X = (textConstrainedRect.Width - totalTextSize.Width);
				else if (IsAligned(txtAlign, SideAlignment.Center))
					adj.X = (textConstrainedRect.Width - totalTextSize.Width) / 2;
				adj.X += textConstrainedRect.X;
				adj.Y += textConstrainedRect.Y;
				textBounds.Offset(adj);
				subtextBounds.Offset(adj);

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
				if (IsAligned(chkAlign, SideAlignment.Bottom))
					gOff = this.Height - bounds.Height;
				else if (IsAligned(chkAlign, SideAlignment.Middle))
					gOff = (this.Height - bounds.Height) / 2;

				if (IsAligned(txtAlign, SideAlignment.Bottom))
					tOff = this.Height - bounds.Height;
				else if (IsAligned(chkAlign, SideAlignment.Middle))
					tOff = (this.Height - bounds.Height) / 2;
			}
			System.Diagnostics.Debug.WriteLine(string.Format("RadioButtonEx.GetElementBounds: client={4}; bounds={0}; focus={1}; autosize={2}; toff={3}", bounds, focusBounds, this.AutoSize, tOff, this.ClientRectangle));

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
			if (IsAligned(txtAlign, SideAlignment.Right))
				tff |= TextFormatFlags.Right;
			else if (IsAligned(txtAlign, SideAlignment.Center))
				tff |= TextFormatFlags.HorizontalCenter;
		}

		private void RefreshLayout()
		{
			RebuildTextFormatFlags();
			cachedRects = null;
			Refresh();
		}
/*	}

	internal static class LayoutEngine
	{
		internal class LayoutData
		{
			internal Rectangle checkArea;
			internal Rectangle checkBounds;
			internal Rectangle client;
			internal Rectangle face;
			internal Rectangle field;
			internal Rectangle focus;
			//internal Rectangle imageBounds;
			//internal Point imageStart;
			internal LayoutOptions options;
			internal Rectangle textBounds;

			internal LayoutData(LayoutOptions ops)
			{
				options = ops;
			}
		}

		internal class LayoutOptions
		{
			private static readonly TextImageRelation[] _imageAlignToRelation;
			internal int borderSize;
			internal ContentAlignment checkAlign;
			internal int checkPaddingSize;
			internal int checkSize;
			internal Rectangle client;
			private static readonly int combineCheck;
			//private static readonly int combineImageText;
			private bool disableWordWrapping;
			internal bool everettButtonCompat;
			internal bool focusOddEvenFixup;
			internal Font font;
			internal StringAlignment gdipAlignment;
			internal StringFormatFlags gdipFormatFlags;
			internal System.Drawing.Text.HotkeyPrefix gdipHotkeyPrefix;
			internal StringAlignment gdipLineAlignment;
			internal StringTrimming gdipTrimming;
			internal TextFormatFlags gdiTextFormatFlags;
			internal bool growBorderBy1PxWhenDefault;
			internal bool hintTextUp;
			//internal ContentAlignment imageAlign;
			//internal Size imageSize;
			internal bool isDefault;
			internal bool layoutRTL;
			internal bool maxFocus;
			internal Padding padding;
			internal int paddingSize;
			internal bool shadowedText;
			internal string text;
			internal ContentAlignment textAlign;
			//internal int textImageInset;
			//internal TextImageRelation textImageRelation;
			internal bool textOffset;
			internal bool useCompatibleTextRendering;
			internal bool verticalText;

			public LayoutData Layout(Graphics g, int fullCheckSize)
			{
				LayoutData layout = new LayoutData(this);

				Rectangle face = layout.client;
				CalcCheckmarkRectangle(g, layout, fullCheckSize);
				LayoutTextAndImage(layout);
				if (this.maxFocus)
				{
					layout.focus = layout.field;
					layout.focus.Inflate(-1, -1);
					layout.focus = LayoutUtils.InflateRect(layout.focus, this.padding);
				}
				else
				{
					Rectangle a = new Rectangle(layout.textBounds.X - 1, layout.textBounds.Y - 1, layout.textBounds.Width + 2, layout.textBounds.Height + 3);
					layout.focus = a;
				}
				if (this.focusOddEvenFixup)
				{
					if ((layout.focus.Height % 2) == 0)
					{
						layout.focus.Y++;
						layout.focus.Height--;
					}
					if ((layout.focus.Width % 2) == 0)
					{
						layout.focus.X++;
						layout.focus.Width--;
					}
				}

				return layout;
			}

			private void CalcCheckmarkRectangle(Graphics g, LayoutData layout, int fullCheckSize)
			{
				layout.face = LayoutUtils.InflateRect(layout.client, this.padding);
				layout.checkBounds = new Rectangle(layout.face.X, layout.face.Y, fullCheckSize, fullCheckSize);
				ContentAlignment alignment = this.RtlTranslateContent(this.checkAlign);
				Rectangle rectangle = layout.face;
				layout.field = rectangle;
				layout.checkArea = Rectangle.Empty;
				if (fullCheckSize > 0)
				{
					if (LayoutUtils.IsAligned(alignment, LayoutUtils.SideAlignment.Right))
						layout.checkBounds.X = (rectangle.X + rectangle.Width) - layout.checkBounds.Width;
					else if (LayoutUtils.IsAligned(alignment, LayoutUtils.SideAlignment.Center))
						layout.checkBounds.X = rectangle.X + ((rectangle.Width - layout.checkBounds.Width) / 2);
					if (LayoutUtils.IsAligned(alignment, LayoutUtils.SideAlignment.Bottom))
						layout.checkBounds.Y = (rectangle.Y + rectangle.Height) - layout.checkBounds.Height;
					else if (LayoutUtils.IsAligned(alignment, LayoutUtils.SideAlignment.Top))
						layout.checkBounds.Y = rectangle.Y + 2;
					else
						layout.checkBounds.Y = rectangle.Y + ((rectangle.Height - layout.checkBounds.Height) / 2);
					switch (alignment)
					{
						case ContentAlignment.TopLeft:
						case ContentAlignment.MiddleLeft:
						case ContentAlignment.BottomLeft:
							layout.checkArea = rectangle;
							layout.checkArea.Width = fullCheckSize + 1;
							layout.field.X += fullCheckSize + 1;
							layout.field.Width -= fullCheckSize + 1;
							break;

						case ContentAlignment.TopCenter:
							layout.checkArea = rectangle;
							layout.checkArea.Height = fullCheckSize;
							layout.field.Y += fullCheckSize;
							layout.field.Height -= fullCheckSize;
							break;

						case ContentAlignment.TopRight:
						case ContentAlignment.BottomRight:
						case ContentAlignment.MiddleRight:
							layout.checkArea.X = (rectangle.X + rectangle.Width) - fullCheckSize;
							layout.checkArea.Width = fullCheckSize + 1;
							layout.checkArea.Y = rectangle.Y;
							layout.checkArea.Height = rectangle.Height;
							layout.field.Width -= fullCheckSize + 1;
							break;

						case ContentAlignment.MiddleCenter:
							layout.checkArea = layout.checkBounds;
							break;

						case ContentAlignment.BottomCenter:
							layout.checkArea.X = rectangle.X;
							layout.checkArea.Width = rectangle.Width;
							layout.checkArea.Y = (rectangle.Y + rectangle.Height) - fullCheckSize;
							layout.checkArea.Height = fullCheckSize;
							layout.field.Height -= fullCheckSize;
							break;
					}
				}
			}

			private void LayoutTextAndImage(LayoutData layout)
			{
				const int textImageInset = 2;
				int num3;
				ContentAlignment alignment2 = this.RtlTranslateContent(this.textAlign);
				Rectangle withinThis = Rectangle.Inflate(layout.field, -textImageInset, -textImageInset);
				Rectangle textBounds;
				if (this.text == null || this.text.Length == 0)
				{
					Size textSize = TextRenderer.MeasureText(g, this.Text, this.Font, withinThis.Size, TextFormatFlags);
					textBounds = LayoutUtils.Align(textSize, withinThis, alignment2);
				}
				else
				{
					Size proposedSize = LayoutUtils.SubAlignedRegion(withinThis.Size, imageSize, relation);
					Size size4 = GetTextSize(proposedSize);
					Rectangle rectangle2 = withinThis;
					Size b = LayoutUtils.AddAlignedRegion(size4, imageSize, relation);
					rectangle2.Size = LayoutUtils.UnionSizes(rectangle2.Size, b);
					Rectangle bounds = LayoutUtils.Align(b, rectangle2, ContentAlignment.MiddleCenter);
					bool flag = (ImageAlignToRelation(align) & relation) != TextImageRelation.Overlay;
					bool flag2 = (TextAlignToRelation(alignment2) & relation) != TextImageRelation.Overlay;
					if (flag)
					{
						LayoutUtils.SplitRegion(rectangle2, imageSize, (AnchorStyles)relation, out imageBounds, out textBounds);
					}
					else if (flag2)
					{
						LayoutUtils.SplitRegion(rectangle2, size4, (AnchorStyles)LayoutUtils.GetOppositeTextImageRelation(relation), out textBounds, out imageBounds);
					}
					else
					{
						LayoutUtils.SplitRegion(bounds, imageSize, (AnchorStyles)relation, out imageBounds, out textBounds);
						LayoutUtils.ExpandRegionsToFillBounds(rectangle2, (AnchorStyles)relation, ref imageBounds, ref textBounds);
					}
					imageBounds = LayoutUtils.Align(imageSize, imageBounds, align);
					textBounds = LayoutUtils.Align(size4, textBounds, alignment2);
				}
				switch (relation)
				{
					case TextImageRelation.TextBeforeImage:
					case TextImageRelation.ImageBeforeText:
						{
							int num = Math.Min(textBounds.Bottom, field.Bottom);
							textBounds.Y = Math.Max(Math.Min(textBounds.Y, field.Y + ((field.Height - textBounds.Height) / 2)), field.Y);
							textBounds.Height = num - textBounds.Y;
							break;
						}
				}
				if ((relation == TextImageRelation.TextAboveImage) || (relation == TextImageRelation.ImageAboveText))
				{
					int num2 = Math.Min(textBounds.Right, field.Right);
					textBounds.X = Math.Max(Math.Min(textBounds.X, field.X + ((field.Width - textBounds.Width) / 2)), field.X);
					textBounds.Width = num2 - textBounds.X;
				}
				if ((relation == TextImageRelation.ImageBeforeText) && (imageBounds.Size.Width != 0))
				{
					imageBounds.Width = Math.Max(0, Math.Min(withinThis.Width - textBounds.Width, imageBounds.Width));
					textBounds.X = imageBounds.X + imageBounds.Width;
				}
				if ((relation == TextImageRelation.ImageAboveText) && (imageBounds.Size.Height != 0))
				{
					imageBounds.Height = Math.Max(0, Math.Min(withinThis.Height - textBounds.Height, imageBounds.Height));
					textBounds.Y = imageBounds.Y + imageBounds.Height;
				}
				textBounds = Rectangle.Intersect(textBounds, field);
				if (hintTextUp)
				{
					textBounds.Y--;
				}
				if (textOffset)
				{
					textBounds.Offset(1, 1);
				}
				if (options.everettButtonCompat)
				{
					imageStart = imageBounds.Location;
					imageBounds = Rectangle.Intersect(imageBounds, field);
				}
				else if (!Application.RenderWithVisualStyles)
				{
					textBounds.X++;
				}
				if (!useCompatibleTextRendering)
				{
					num3 = Math.Min(textBounds.Bottom, withinThis.Bottom);
					textBounds.Y = Math.Max(textBounds.Y, withinThis.Y);
				}
				else
				{
					num3 = Math.Min(textBounds.Bottom, field.Bottom);
					textBounds.Y = Math.Max(textBounds.Y, field.Y);
				}
				textBounds.Height = num3 - textBounds.Y;
			}

			internal ContentAlignment RtlTranslateContent(ContentAlignment align)
			{
				if (this.layoutRTL)
				{
					ContentAlignment[][] alignmentArray = new ContentAlignment[][] { new ContentAlignment[] { ContentAlignment.TopLeft, ContentAlignment.TopRight }, new ContentAlignment[] { ContentAlignment.MiddleLeft, ContentAlignment.MiddleRight }, new ContentAlignment[] { ContentAlignment.BottomLeft, ContentAlignment.BottomRight } };
					for (int i = 0; i < 3; i++)
					{
						if (alignmentArray[i][0] == align)
						{
							return alignmentArray[i][1];
						}
						if (alignmentArray[i][1] == align)
						{
							return alignmentArray[i][0];
						}
					}
				}
				return align;
			}
		}

		internal static class LayoutUtils
		{
*/
			public enum SideAlignment
			{
				Left = 0x111,
				Right = 0x444,
				Center = 0x222,
				Top = 0x7,
				Bottom = 0x700,
				Middle = 0x70
			}

			public static bool IsAligned(ContentAlignment ca, SideAlignment sa)
			{
				return ((int)ca & (int)sa) != 0;
			}
/*
			public static Rectangle Align(Size alignThis, Rectangle withinThis, ContentAlignment align)
			{
				return VAlign(alignThis, HAlign(alignThis, withinThis, align), align);
			}
 
			public static Rectangle HAlign(Size alignThis, Rectangle withinThis, ContentAlignment align)
			{
				if (IsAligned(align, SideAlignment.Right))
				{
					withinThis.X += withinThis.Width - alignThis.Width;
				}
				else if (IsAligned(align, SideAlignment.Center))
				{
					withinThis.X += (withinThis.Width - alignThis.Width) / 2;
				}
				withinThis.Width = alignThis.Width;
				return withinThis;
			}

			public static Rectangle VAlign(Size alignThis, Rectangle withinThis, ContentAlignment align)
			{
				if (IsAligned(align, SideAlignment.Bottom))
				{
					withinThis.Y += withinThis.Height - alignThis.Height;
				}
				else if (IsAligned(align, SideAlignment.Middle))
				{
					withinThis.Y += (withinThis.Height - alignThis.Height) / 2;
				}
				withinThis.Height = alignThis.Height;
				return withinThis;
			}

			internal static Rectangle InflateRect(Rectangle rectangle, Padding padding)
			{
				return Rectangle.FromLTRB(rectangle.Left + padding.Left, rectangle.Top + padding.Top,
					rectangle.Right - padding.Right, rectangle.Bottom - padding.Bottom);
			}
		}
 */
		}
}