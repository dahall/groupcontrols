using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace GroupControls
{
	internal class ColumnLayoutEngine : LayoutEngine
	{
		public static readonly Size DefaultSize = new Size(100, 100);
		private Size idealSize = DefaultSize;
		private readonly Size lastLayout = Size.Empty;

		public Size IdealSize => idealSize;

		public SparseArray<Rectangle> ItemBounds { get; } = new SparseArray<Rectangle>();

		/// <summary>
		/// Retrieves the size of a rectangular area into which a control can be fitted.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="proposedSize">The custom-sized area for a control.</param>
		/// <returns>An ordered pair of type <see cref="Size"/> representing the width and height of a rectangle.</returns>
		public Size GetPreferredSize(ControlListBase container, Size proposedSize)
		{
			if (lastLayout != proposedSize)
				Layout(container, new LayoutEventArgs(container, null), proposedSize);
			return idealSize;
		}

		/// <summary>
		/// Layouts the specified container.
		/// </summary>
		/// <param name="container">The container.</param>
		/// <param name="layoutEventArgs">The <see cref="System.Windows.Forms.LayoutEventArgs" /> instance containing the event data.</param>
		/// <returns></returns>
		public override bool Layout(object container, LayoutEventArgs layoutEventArgs) => Layout(container as ControlListBase, layoutEventArgs, Size.Empty);

		private bool Layout(ControlListBase parent, LayoutEventArgs layoutEventArgs, Size proposedSize)
		{
			if (parent == null)
				throw new InvalidCastException(nameof(ColumnLayoutEngine) + " can only be used to layout controls derived from " + nameof(ControlListBase));

			if (proposedSize == Size.Empty)
				proposedSize = parent.ClientSize;

			if (parent.BaseItems != null && parent.BaseItems.Count > 0)
			{
				System.Diagnostics.Debug.WriteLine(parent.Name + ": Layout:");
				System.Diagnostics.Debug.WriteLine($"  ClientSize:{proposedSize}, Margin:{parent.Margin}, Padding:{parent.Padding}, Cols:{parent.RepeatColumns}, Spacing:{parent.ItemSpacing}, Items:{parent.BaseItems.Count}, Dir:{parent.RepeatDirection}, Even:{parent.SpaceEvenly}");
				ItemBounds.Clear();
				using (var g = parent.CreateGraphics())
				{
					// Determine the start coordinate of each column
					var colWidth = (parent.ClientSize.Width - parent.Padding.Horizontal - ((parent.RepeatColumns - 1) * parent.ItemSpacing.Width)) / parent.RepeatColumns;
					var colPos = new Point[parent.RepeatColumns];
					for (var x = 0; x < parent.RepeatColumns; x++)
						colPos[x] = new Point(parent.Padding.Left + (x * (colWidth + parent.ItemSpacing.Width)), parent.Padding.Top);

					// Get the base height of all items and the max height
					var maxItemHeight = 0;
					idealSize.Height = 0;
					var maxSize = new Size(colWidth, 0);
					for (var i = 0; i < parent.BaseItems.Count; i++)
					{
						var sz = parent.MeasureItem(g, i, maxSize);
						ItemBounds[i] = new Rectangle(Point.Empty, sz);

						// Calculate maximum item height
						maxItemHeight = Math.Max(maxItemHeight, sz.Height);
					}

					// Calculate the positions of each item
					var curCol = 0;
					for (var i = 0; i < parent.BaseItems.Count; i++)
					{
						// Set bounds of the item
						ItemBounds[i] = new Rectangle(colPos[curCol], ItemBounds[i].Size);
						// Set top position of next item
						colPos[curCol].Y += (parent.SpaceEvenly ? maxItemHeight : ItemBounds[i].Height) + parent.ItemSpacing.Height;
						if (parent.RepeatDirection == RepeatDirection.Horizontal)
							if (++curCol == parent.RepeatColumns) curCol = 0;
						// If parent.ItemSpacing evenly we can determine all locations now by changing column count at pure divisions
						/*if (parent.SpaceEvenly && parent.RepeatDirection == GroupControls.RepeatDirection.Vertical && i > 0)
						{
							if (i % (parent.BaseItems.Count / parent.RepeatColumns) == 0 && curCol <= (parent.BaseItems.Count % parent.RepeatColumns))
								curCol++;
						}*/
					}

					// Split vertical parent.RepeatColumns and reset positions of items
					if (parent.RepeatDirection == RepeatDirection.Vertical && parent.RepeatColumns > 1)
					{
						var idealColHeight = colPos[0].Y / parent.RepeatColumns;
						var thisColBottom = idealColHeight;
						var y = parent.Padding.Top + parent.Margin.Top;
						for (var i = 0; i < parent.BaseItems.Count; i++)
						{
							var iBounds = ItemBounds[i];
							var nBounds = Rectangle.Empty;
							if ((i + 1) < parent.BaseItems.Count)
								nBounds = ItemBounds[i + 1];

							if (curCol > 0)
								ItemBounds[i] = new Rectangle(new Point(colPos[curCol].X, y), ItemBounds[i].Size);
							colPos[curCol].Y = ItemBounds[i].Bottom + parent.ItemSpacing.Height;

							if ((iBounds.Bottom > thisColBottom || nBounds.Bottom > thisColBottom) && (curCol + 1 < parent.RepeatColumns))
							{
								if (Math.Abs(iBounds.Bottom - idealColHeight) < Math.Abs(nBounds.Bottom - idealColHeight))
								{
									y = parent.Padding.Top;
									curCol++;
									thisColBottom = iBounds.Bottom + parent.ItemSpacing.Height + idealColHeight;
								}
							}
							else
							{
								y += (parent.SpaceEvenly ? maxItemHeight : ItemBounds[i].Height) + parent.ItemSpacing.Height;
							}
						}
					}

					// Set ideal height
					idealSize.Height = 0;
					for (var c = 0; c < parent.RepeatColumns; c++)
						if (idealSize.Height < colPos[c].Y) idealSize.Height = colPos[c].Y;
					idealSize.Height = idealSize.Height - parent.ItemSpacing.Height + parent.Padding.Bottom;
				}

				// Set scroll height and autosize to ideal height
				parent.AutoScrollMinSize = new Size(parent.ClientRectangle.Width, idealSize.Height);
				if (parent.AutoSize) parent.Height = idealSize.Height;

				System.Diagnostics.Debug.WriteLine("  " + string.Join(" ", Array.ConvertAll(ItemBounds.ToArray(), r => $"({r})")));
			}

			return parent.AutoSize;
		}

		private bool NewLayout(ControlListBase parent, LayoutEventArgs layoutEventArgs, Size proposedSize)
		{
			if (parent == null)
				throw new InvalidCastException(nameof(ColumnLayoutEngine) + " can only be used to layout controls derived from " + nameof(ControlListBase));

			if (proposedSize == Size.Empty)
				proposedSize = parent.ClientSize;

			if (parent.BaseItems != null && parent.BaseItems.Count > 0)
			{
				var parentWidthPinned = parent.Dock != DockStyle.None || (parent.Anchor & (AnchorStyles.Left | AnchorStyles.Right)) != 0;
				System.Diagnostics.Debug.WriteLine(parent.Name + ": Layout: ");
				System.Diagnostics.Debug.WriteLine($"  ClientSize:{proposedSize}, Margin:{parent.Margin}, Padding:{parent.Padding}, Cols:{parent.RepeatColumns}, Spacing:{parent.ItemSpacing}, Items:{parent.BaseItems.Count}, Dir:{parent.RepeatDirection}, Even:{parent.SpaceEvenly}");
				ItemBounds.Clear();
				using (var g = parent.CreateGraphics())
				{
					idealSize = proposedSize;

					// Determine starting width of column
					var colWidth = (proposedSize.Width - parent.Padding.Horizontal - ((parent.RepeatColumns - 1) * parent.ItemSpacing.Width)) / parent.RepeatColumns;

					// Get the min width of all items
					var maxMinW = 0;
					var minSz = new Size[parent.BaseItems.Count];
					var wsz = new Size(1, int.MaxValue);
					for (var i = 0; i < parent.BaseItems.Count; i++)
					{
						minSz[i] = parent.MeasureItem(g, i, wsz);
						maxMinW = Math.Max(maxMinW, minSz[i].Width);
					}

					// Determine the column for each item
					var itemCol = new int[parent.BaseItems.Count];
					for (var i = 0; i < parent.BaseItems.Count; i++)
					{
						if (parent.RepeatDirection == RepeatDirection.Horizontal)
							itemCol[i] = i % parent.RepeatColumns;
						else
							itemCol[i] = i / (int)Math.Ceiling(parent.BaseItems.Count / (decimal)parent.RepeatColumns);
					}

					// If not spaced evenly, then balance the columns based on height
					if (!parent.SpaceEvenly)
					{
						// Determine total height of all items
						// Determine max height of each column
						// Working backwards, fit each item into columns with spillover on Column 1
					}

					// Get the min width of each column
					var minColWidth = new int[parent.RepeatColumns];
					var maxColWidth = 0;
					var totMinW = 0;
					if (parent.VariableColumnWidths)
					{
						for (var i = 0; i < parent.BaseItems.Count; i++)
						{
							var col = itemCol[i];
							minColWidth[col] = Math.Max(minColWidth[col], minSz[i].Width);
							maxColWidth = Math.Max(maxColWidth, minColWidth[col]);
						}
						for (var i = 0; i < parent.RepeatColumns; i++)
							totMinW += minColWidth[i];
					}
					else
					{
						for (var i = 0; i < parent.RepeatColumns; i++)
							minColWidth[i] = maxMinW;
						maxColWidth = Math.Min(maxMinW, colWidth);
						totMinW = parent.RepeatColumns * maxColWidth;
					}
					totMinW += ((parent.RepeatColumns - 1) * parent.ItemSpacing.Width) + parent.Padding.Horizontal;

					// Determine the start coordinate of each column
					var colPos = new Point[parent.RepeatColumns];
					var xPos = 0;
					for (var x = 0; x < parent.RepeatColumns; x++)
					{
						var cWidth = maxColWidth;
						if (parent.VariableColumnWidths && parent.AutoSize && !parentWidthPinned)
							cWidth = minColWidth[x];
						else if (!parent.VariableColumnWidths && (parentWidthPinned || parent.AutoSize))
							cWidth = maxColWidth;
						colPos[x] = new Point(parent.Padding.Left + xPos, parent.Padding.Top);
						xPos += cWidth + parent.ItemSpacing.Width;
					}

					// Set the width value based on AutoSize, Dock and SpaceEvenly properties
					for (var i = 0; i < parent.BaseItems.Count; i++)
					{
						minSz[i].Width = Math.Min(minSz[i].Width, minColWidth[itemCol[i]]);
					}

					/*int totMinW = 0;
					if (parent.AutoSize && !parentWidthPinned)
					{
						for (int i = 0; i < parent.BaseItems.Count; i++)
						{
							if (!parent.VariableColumnWidths)
								minSz[i].Width = minColWidth[itemCol[i]];
							totMinW += minSz[i].Width;
						}
					}
					else
					{
						for (int i = 0; i < parent.BaseItems.Count; i++)
						{
							minSz[i].Width = parentWidthPinned ? colWidth : Math.Max(colWidth, maxMinCol);
							totMinW += minSz[i].Width;
						}
					}
					totMinW += ((parent.RepeatColumns - 1) * parent.ItemSpacing.Width) + parent.Padding.Horizontal;*/

					/*// Get min width for all parent.RepeatColumns
					int maxMinCol = 0;
					int totMinW = 0;
					for (int i = 0; i < parent.RepeatColumns; i++)
					{
						maxMinCol = Math.Max(maxMinCol, minColWidth[i]);
						totMinW += minColWidth[i];
					}*/

					// Reset ideal width if needed
					if (!parentWidthPinned)
						idealSize.Width = totMinW;

					// Get the base height of all items and the max height
					var maxItemHeight = 0;
					idealSize.Height = 0;
					for (var i = 0; i < parent.BaseItems.Count; i++)
					{
						minSz[i].Height = 0;
						var sz = parent.MeasureItem(g, i, minSz[i]);
						ItemBounds[i] = new Rectangle(Point.Empty, sz);
						maxItemHeight = Math.Max(maxItemHeight, sz.Height);
					}

					// Calculate the positions of each item
					for (var i = 0; i < parent.BaseItems.Count; i++)
					{
						var curCol = itemCol[i];
						// Set bounds of the item
						ItemBounds[i] = new Rectangle(colPos[curCol], ItemBounds[i].Size);
						// Set top position of next item
						colPos[curCol].Y += (parent.SpaceEvenly ? maxItemHeight : ItemBounds[i].Height) + parent.ItemSpacing.Height;
					}

					// Split vertical parent.RepeatColumns and reset positions of items
					/*if (parent.RepeatDirection == GroupControls.RepeatDirection.Vertical && parent.RepeatColumns > 1)
					{
						int idealColHeight = colPos[0].Y / parent.RepeatColumns;
						int thisColBottom = idealColHeight;
						int y = parent.Padding.Top + parent.Margin.Top;
						for (int i = 0; i < parent.BaseItems.Count; i++)
						{
							Rectangle iBounds = itemBounds[i];
							Rectangle nBounds = Rectangle.Empty;
							if ((i + 1) < parent.BaseItems.Count)
								nBounds = itemBounds[i + 1];

							if (curCol > 0)
								itemBounds[i] = new Rectangle(new Point(colPos[curCol].X, y), itemBounds[i].Size);
							colPos[curCol].Y = itemBounds[i].Bottom + parent.ItemSpacing.Height;

							if ((iBounds.Bottom > thisColBottom || nBounds.Bottom > thisColBottom) && (curCol + 1 < parent.RepeatColumns))
							{
								if (Math.Abs(iBounds.Bottom - idealColHeight) < Math.Abs(nBounds.Bottom - idealColHeight))
								{
									y = parent.Padding.Top;
									curCol++;
									thisColBottom = iBounds.Bottom + parent.ItemSpacing.Height + idealColHeight;
								}
							}
							else
							{
								y += (parent.SpaceEvenly ? maxItemHeight : itemBounds[i].Height) + parent.ItemSpacing.Height;
							}
						}
					}*/

					// Set ideal height
					idealSize.Height = 0;
					for (var c = 0; c < parent.RepeatColumns; c++)
						idealSize.Height = Math.Max(idealSize.Height, colPos[c].Y);
					idealSize.Height = idealSize.Height - parent.ItemSpacing.Height + parent.Padding.Bottom;
				}

				// Set scroll height and autosize to ideal height
				parent.AutoScrollMinSize = new Size(proposedSize.Width, idealSize.Height);
				if (parentWidthPinned && parent.AutoSize)
					parent.Size = idealSize;

				System.Diagnostics.Debug.WriteLine("  " + string.Join(" ", Array.ConvertAll(ItemBounds.ToArray(), r => $"({r})")));
			}

			return parent.AutoSize;
		}
	}
}
