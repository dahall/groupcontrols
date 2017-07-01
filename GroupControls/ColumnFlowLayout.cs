using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;

namespace GroupControls
{
	internal class FlowLayout : LayoutEngine
	{
		// Fields
		private static readonly int _flowDirectionProperty = PropertyStore.CreateKey();
		private static readonly int _wrapContentsProperty = PropertyStore.CreateKey();
		internal static readonly FlowLayout Instance = new FlowLayout();

		// Methods
		private static ContainerProxy CreateContainerProxy(Control container, FlowDirection flowDirection)
		{
			switch (flowDirection)
			{
				case FlowDirection.TopDown:
					return new TopDownProxy(container);

				case FlowDirection.RightToLeft:
					return new RightToLeftProxy(container);

				case FlowDirection.BottomUp:
					return new BottomUpProxy(container);
			}
			return new ContainerProxy(container);
		}

		internal static FlowLayoutSettings CreateSettings(Control owner)
		{
			return new FlowLayoutSettings(owner);
		}

		[Conditional("DEBUG_VERIFY_ALIGNMENT")]
		private void Debug_VerifyAlignment(Control container, FlowDirection flowDirection)
		{
		}

		public static FlowDirection GetFlowDirection(Control container)
		{
			return (FlowDirection)container.Properties.GetInteger(_flowDirectionProperty);
		}

		internal override Size GetPreferredSize(Control container, Size proposedConstraints)
		{
			Rectangle displayRect = new Rectangle(new Point(0, 0), proposedConstraints);
			Size size = this.xLayout(container, displayRect, true);
			if ((size.Width <= proposedConstraints.Width) && (size.Height <= proposedConstraints.Height))
			{
				return size;
			}
			displayRect.Size = size;
			return this.xLayout(container, displayRect, true);
		}

		public static bool GetWrapContents(Control container)
		{
			return (container.Properties.GetInteger(_wrapContentsProperty) == 0);
		}

		internal override bool LayoutCore(Control container, LayoutEventArgs args)
		{
			CommonProperties.SetLayoutBounds(container, this.xLayout(container, container.DisplayRectangle, false));
			return container.AutoSize;
		}

		private void LayoutRow(ContainerProxy containerProxy, ElementProxy elementProxy, int startIndex, int endIndex, Rectangle rowBounds)
		{
			int num;
			this.xLayoutRow(containerProxy, elementProxy, startIndex, endIndex, rowBounds, out num, false);
		}

		private Size MeasureRow(ContainerProxy containerProxy, ElementProxy elementProxy, int startIndex, Rectangle displayRectangle, out int breakIndex)
		{
			return this.xLayoutRow(containerProxy, elementProxy, startIndex, containerProxy.Container.Children.Count, displayRectangle, out breakIndex, true);
		}

		public static void SetFlowDirection(Control container, FlowDirection value)
		{
			if (!ClientUtils.IsEnumValid(value, (int)value, 0, 3))
			{
				throw new InvalidEnumArgumentException("value", (int)value, typeof(FlowDirection));
			}
			container.Properties.SetInteger(_flowDirectionProperty, (int)value);
			LayoutTransaction.DoLayout(container, container, PropertyNames.FlowDirection);
		}

		public static void SetWrapContents(Control container, bool value)
		{
			container.Properties.SetInteger(_wrapContentsProperty, value ? 0 : 1);
			LayoutTransaction.DoLayout(container, container, PropertyNames.WrapContents);
		}

		private Size xLayout(Control container, Rectangle displayRect, bool measureOnly)
		{
			int num2;
			FlowDirection flowDirection = GetFlowDirection(container);
			bool wrapContents = GetWrapContents(container);
			ContainerProxy containerProxy = CreateContainerProxy(container, flowDirection);
			containerProxy.DisplayRect = displayRect;
			displayRect = containerProxy.DisplayRect;
			ElementProxy elementProxy = containerProxy.ElementProxy;
			Size empty = Size.Empty;
			if (!wrapContents)
			{
				displayRect.Width = 0x7fffffff - displayRect.X;
			}
			for (int i = 0; i < container.Children.Count; i = num2)
			{
				Size size2 = Size.Empty;
				Rectangle displayRectangle = new Rectangle(displayRect.X, displayRect.Y, displayRect.Width, displayRect.Height - empty.Height);
				size2 = this.MeasureRow(containerProxy, elementProxy, i, displayRectangle, out num2);
				if (!measureOnly)
				{
					Rectangle rowBounds = new Rectangle(displayRect.X, empty.Height + displayRect.Y, size2.Width, size2.Height);
					this.LayoutRow(containerProxy, elementProxy, i, num2, rowBounds);
				}
				empty.Width = Math.Max(empty.Width, size2.Width);
				empty.Height += size2.Height;
			}
			if (container.Children.Count != 0)
			{
			}
			return LayoutUtils.FlipSizeIf((flowDirection == FlowDirection.TopDown) || (GetFlowDirection(container) == FlowDirection.BottomUp), empty);
		}

		private Size xLayoutRow(ContainerProxy containerProxy, ElementProxy elementProxy, int startIndex, int endIndex, Rectangle rowBounds, out int breakIndex, bool measureOnly)
		{
			Point location = rowBounds.Location;
			Size empty = Size.Empty;
			int num = 0;
			breakIndex = startIndex;
			bool wrapContents = GetWrapContents(containerProxy.Container);
			bool flag2 = false;
			ArrangedElementCollection children = containerProxy.Container.Children;
			int num2 = startIndex;
			while (num2 < endIndex)
			{
				elementProxy.Element = children[num2];
				if (elementProxy.ParticipatesInLayout)
				{
					Size preferredSize;
					if (elementProxy.AutoSize)
					{
						Size b = new Size(0x7fffffff, rowBounds.Height - elementProxy.Margin.Size.Height);
						if (num2 == startIndex)
						{
							b.Width = (rowBounds.Width - empty.Width) - elementProxy.Margin.Size.Width;
						}
						b = LayoutUtils.UnionSizes(new Size(1, 1), b);
						preferredSize = elementProxy.GetPreferredSize(b);
					}
					else
					{
						preferredSize = elementProxy.SpecifiedSize;
						if (elementProxy.Stretches)
						{
							preferredSize.Height = 0;
						}
						if (preferredSize.Height < elementProxy.MinimumSize.Height)
						{
							preferredSize.Height = elementProxy.MinimumSize.Height;
						}
					}
					Size size4 = preferredSize + elementProxy.Margin.Size;
					if (!measureOnly)
					{
						Rectangle rect = new Rectangle(location, new Size(size4.Width, rowBounds.Height));
						rect = LayoutUtils.DeflateRect(rect, elementProxy.Margin);
						AnchorStyles anchorStyles = elementProxy.AnchorStyles;
						containerProxy.Bounds = LayoutUtils.AlignAndStretch(preferredSize, rect, anchorStyles);
					}
					location.X += size4.Width;
					if ((num > 0) && (location.X > rowBounds.Right))
					{
						return empty;
					}
					empty.Width = location.X - rowBounds.X;
					empty.Height = Math.Max(empty.Height, size4.Height);
					if (wrapContents)
					{
						if (flag2)
						{
							return empty;
						}
						if (((num2 + 1) < endIndex) && CommonProperties.GetFlowBreak(elementProxy.Element))
						{
							if (num == 0)
							{
								flag2 = true;
							}
							else
							{
								breakIndex++;
								return empty;
							}
						}
					}
					num++;
				}
				num2++;
				breakIndex++;
			}
			return empty;
		}

		// Nested Types
		private class BottomUpProxy : FlowLayout.ContainerProxy
		{
			// Methods
			public BottomUpProxy(Control container)
				: base(container)
			{
			}

			// Properties
			public override Rectangle Bounds
			{
				set
				{
					base.Bounds = base.RTLTranslateNoMarginSwap(value);
				}
			}

			protected override bool IsVertical
			{
				get
				{
					return true;
				}
			}
		}

		private class ContainerProxy
		{
			// Fields
			private Control _container;
			private Rectangle _displayRect;
			private FlowLayout.ElementProxy _elementProxy;
			private bool _isContainerRTL;

			// Methods
			public ContainerProxy(Control container)
			{
				this._container = container;
				this._isContainerRTL = false;
				if (this._container is Control)
				{
					this._isContainerRTL = ((Control)this._container).RightToLeft == RightToLeft.Yes;
				}
			}

			protected Rectangle RTLTranslateNoMarginSwap(Rectangle bounds)
			{
				Rectangle rectangle = bounds;
				rectangle.X = (((this.DisplayRect.Right - bounds.X) - bounds.Width) + this.ElementProxy.Margin.Left) - this.ElementProxy.Margin.Right;
				FlowLayoutPanel container = this.Container as FlowLayoutPanel;
				if (container != null)
				{
					Point autoScrollPosition = container.AutoScrollPosition;
					if (!(autoScrollPosition != Point.Empty))
					{
						return rectangle;
					}
					Point point2 = new Point(rectangle.X, rectangle.Y);
					if (this.IsVertical)
					{
						point2.Offset(autoScrollPosition.Y, 0);
					}
					else
					{
						point2.Offset(autoScrollPosition.X, 0);
					}
					rectangle.Location = point2;
				}
				return rectangle;
			}

			// Properties
			public virtual Rectangle Bounds
			{
				set
				{
					if (this.IsContainerRTL)
					{
						if (this.IsVertical)
						{
							value.Y = this.DisplayRect.Bottom - value.Bottom;
						}
						else
						{
							value.X = this.DisplayRect.Right - value.Right;
						}
						FlowLayoutPanel container = this.Container as FlowLayoutPanel;
						if (container != null)
						{
							Point autoScrollPosition = container.AutoScrollPosition;
							if (autoScrollPosition != Point.Empty)
							{
								Point point2 = new Point(value.X, value.Y);
								if (this.IsVertical)
								{
									point2.Offset(0, autoScrollPosition.X);
								}
								else
								{
									point2.Offset(autoScrollPosition.X, 0);
								}
								value.Location = point2;
							}
						}
					}
					this.ElementProxy.Bounds = value;
				}
			}

			public Control Container
			{
				get
				{
					return this._container;
				}
			}

			public Rectangle DisplayRect
			{
				get
				{
					return this._displayRect;
				}
				set
				{
					if (this._displayRect != value)
					{
						this._displayRect = LayoutUtils.FlipRectangleIf(this.IsVertical, value);
					}
				}
			}

			public FlowLayout.ElementProxy ElementProxy
			{
				get
				{
					if (this._elementProxy == null)
					{
						this._elementProxy = this.IsVertical ? new FlowLayout.VerticalElementProxy() : new FlowLayout.ElementProxy();
					}
					return this._elementProxy;
				}
			}

			protected bool IsContainerRTL
			{
				get
				{
					return this._isContainerRTL;
				}
			}

			protected virtual bool IsVertical
			{
				get
				{
					return false;
				}
			}
		}

		private class ElementProxy
		{
			// Fields
			private Control _element;

			// Methods
			public virtual Size GetPreferredSize(Size proposedSize)
			{
				return this._element.GetPreferredSize(proposedSize);
			}

			// Properties
			public virtual AnchorStyles AnchorStyles
			{
				get
				{
					AnchorStyles unifiedAnchor = LayoutUtils.GetUnifiedAnchor(this.Element);
					bool flag = (unifiedAnchor & (AnchorStyles.Bottom | AnchorStyles.Top)) == (AnchorStyles.Bottom | AnchorStyles.Top);
					bool flag2 = (unifiedAnchor & AnchorStyles.Top) != AnchorStyles.None;
					bool flag3 = (unifiedAnchor & AnchorStyles.Bottom) != AnchorStyles.None;
					if (flag)
					{
						return (AnchorStyles.Bottom | AnchorStyles.Top);
					}
					if (flag2)
					{
						return AnchorStyles.Top;
					}
					if (flag3)
					{
						return AnchorStyles.Bottom;
					}
					return AnchorStyles.None;
				}
			}

			public bool AutoSize
			{
				get
				{
					return CommonProperties.GetAutoSize(this._element);
				}
			}

			public virtual Rectangle Bounds
			{
				set
				{
					this._element.SetBounds(value, BoundsSpecified.None);
				}
			}

			public Control Element
			{
				get
				{
					return this._element;
				}
				set
				{
					this._element = value;
				}
			}

			public virtual Padding Margin
			{
				get
				{
					return CommonProperties.GetMargin(this.Element);
				}
			}

			public virtual Size MinimumSize
			{
				get
				{
					return CommonProperties.GetMinimumSize(this.Element, Size.Empty);
				}
			}

			public bool ParticipatesInLayout
			{
				get
				{
					return this._element.ParticipatesInLayout;
				}
			}

			public virtual Size SpecifiedSize
			{
				get
				{
					return CommonProperties.GetSpecifiedBounds(this._element).Size;
				}
			}

			public bool Stretches
			{
				get
				{
					AnchorStyles anchorStyles = this.AnchorStyles;
					return (((AnchorStyles.Bottom | AnchorStyles.Top) & anchorStyles) == (AnchorStyles.Bottom | AnchorStyles.Top));
				}
			}
		}

		private class RightToLeftProxy : FlowLayout.ContainerProxy
		{
			// Methods
			public RightToLeftProxy(Control container)
				: base(container)
			{
			}

			// Properties
			public override Rectangle Bounds
			{
				set
				{
					base.Bounds = base.RTLTranslateNoMarginSwap(value);
				}
			}
		}

		private class TopDownProxy : FlowLayout.ContainerProxy
		{
			// Methods
			public TopDownProxy(Control container)
				: base(container)
			{
			}

			// Properties
			protected override bool IsVertical
			{
				get
				{
					return true;
				}
			}
		}

		private class VerticalElementProxy : FlowLayout.ElementProxy
		{
			// Methods
			public override Size GetPreferredSize(Size proposedSize)
			{
				return LayoutUtils.FlipSize(base.GetPreferredSize(LayoutUtils.FlipSize(proposedSize)));
			}

			// Properties
			public override AnchorStyles AnchorStyles
			{
				get
				{
					AnchorStyles unifiedAnchor = LayoutUtils.GetUnifiedAnchor(base.Element);
					bool flag = (unifiedAnchor & (AnchorStyles.Right | AnchorStyles.Left)) == (AnchorStyles.Right | AnchorStyles.Left);
					bool flag2 = (unifiedAnchor & AnchorStyles.Left) != AnchorStyles.None;
					bool flag3 = (unifiedAnchor & AnchorStyles.Right) != AnchorStyles.None;
					if (flag)
					{
						return (AnchorStyles.Bottom | AnchorStyles.Top);
					}
					if (flag2)
					{
						return AnchorStyles.Top;
					}
					if (flag3)
					{
						return AnchorStyles.Bottom;
					}
					return AnchorStyles.None;
				}
			}

			public override Rectangle Bounds
			{
				set
				{
					base.Bounds = LayoutUtils.FlipRectangle(value);
				}
			}

			public override Padding Margin
			{
				get
				{
					return LayoutUtils.FlipPadding(base.Margin);
				}
			}

			public override Size MinimumSize
			{
				get
				{
					return LayoutUtils.FlipSize(base.MinimumSize);
				}
			}

			public override Size SpecifiedSize
			{
				get
				{
					return LayoutUtils.FlipSize(base.SpecifiedSize);
				}
			}
		}
	}

	internal sealed class LayoutTransaction : IDisposable
	{
		// Fields
		private Control _controlToLayout;
		private bool _resumeLayout;

		// Methods
		public LayoutTransaction(Control controlToLayout, Control controlCausingLayout, string property)
			: this(controlToLayout, controlCausingLayout, property, true)
		{
		}

		public LayoutTransaction(Control controlToLayout, Control controlCausingLayout, string property, bool resumeLayout)
		{
			CommonProperties.xClearPreferredSizeCache(controlCausingLayout);
			this._controlToLayout = controlToLayout;
			this._resumeLayout = resumeLayout;
			if (this._controlToLayout != null)
			{
				this._controlToLayout.SuspendLayout();
				CommonProperties.xClearPreferredSizeCache(this._controlToLayout);
				if (resumeLayout)
				{
					this._controlToLayout.PerformLayout(new LayoutEventArgs(controlCausingLayout, property));
				}
			}
		}

		public static IDisposable CreateTransactionIf(bool condition, Control controlToLayout, Control elementCausingLayout, string property)
		{
			if (condition)
			{
				return new LayoutTransaction(controlToLayout, elementCausingLayout, property);
			}
			CommonProperties.xClearPreferredSizeCache(elementCausingLayout);
			return new NullLayoutTransaction();
		}

		public void Dispose()
		{
			if (this._controlToLayout != null)
			{
				this._controlToLayout.ResumeLayout(this._resumeLayout);
			}
		}

		public static void DoLayout(Control elementToLayout, Control elementCausingLayout, string property)
		{
			if (elementCausingLayout != null)
			{
				CommonProperties.xClearPreferredSizeCache(elementCausingLayout);
				if (elementToLayout != null)
				{
					CommonProperties.xClearPreferredSizeCache(elementToLayout);
					elementToLayout.PerformLayout(elementCausingLayout, property);
				}
			}
		}

		public static void DoLayoutIf(bool condition, Control elementToLayout, Control elementCausingLayout, string property)
		{
			if (!condition)
			{
				if (elementCausingLayout != null)
				{
					CommonProperties.xClearPreferredSizeCache(elementCausingLayout);
				}
			}
			else
			{
				DoLayout(elementToLayout, elementCausingLayout, property);
			}
		}
	}
}
