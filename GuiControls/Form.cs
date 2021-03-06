﻿using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Xml.Linq;

namespace OSHVisualGui.GuiControls
{
	public class Form : ContainerControl
	{
		#region Properties

		private readonly Panel panel;

		private string text;
		public string Text
		{
			get => text;
			set => text = value ?? string.Empty;
		}

		public override Point Location
		{
			get => base.Location;
			set
			{
			}
		}

		internal override List<Control> Controls => panel.Controls;

		internal override Point ContainerLocation => base.ContainerLocation.Add(panel.Location);

		internal override Point ContainerAbsoluteLocation => panel.ContainerAbsoluteLocation;

		internal override Size ContainerSize => panel.ContainerSize;

		public override Size Size
		{
			get => base.Size;
			set
			{
				var tempSize = value;
				if (tempSize.Width < 80 || tempSize.Height < 50)
				{
					tempSize = new Size(Math.Max(80, tempSize.Width), Math.Max(50, tempSize.Height));
				}
				base.Size = tempSize;
				panel.Size = new Size(value.Width - 2 * 6, value.Height - 17 - 2 * 6);
			}
		}

		[Category("Events")]
		public ConstructorEvent ConstructorEvent { get; set; }

		[Category("Events")]
		public FormClosingEvent FormClosingEvent { get; set; }
		
		#endregion

		public Form(Point location)
			: this()
		{
			base.Location = location;
		}

		public Form()
		{
			Type = ControlType.Form;

			Parent = null;

			Mode = DragMode.GrowOnly;

			panel = new Panel
			{
				Location = new Point(6, 6 + 17),
				IsSubControl = true
			};
			AddSubControl(panel);

			Size = DefaultSize = new Size(300, 300);

			ForeColor = DefaultForeColor = Color.White;
			BackColor = DefaultBackColor = Color.FromArgb(unchecked((int)0xFF7C7B79));

			FormClosingEvent = new FormClosingEvent(this);
			ConstructorEvent = new ConstructorEvent(this);
		}

		public override IEnumerable<KeyValuePair<string, ChangedProperty>> GetChangedProperties()
		{
			foreach (var pair in base.GetChangedProperties())
			{
				yield return pair;
			}
			yield return new KeyValuePair<string, ChangedProperty>("text", new ChangedProperty(Text));
		}

		public override void AddControl(Control control)
		{
			panel.AddControl(control);
		}

		public override void RemoveControl(Control control)
		{
			panel.RemoveControl(control);
		}

		public override void Render(Graphics graphics)
		{
			graphics.FillRectangle(new SolidBrush(BackColor.Substract(Color.FromArgb(0, 100, 100, 100))), new Rectangle(AbsoluteLocation, Size));

			var rect = new Rectangle(AbsoluteLocation.Add(1, 1), Size.Substract(2, 2));
			var linearBrush = new LinearGradientBrush(rect, BackColor, BackColor.Substract(Color.FromArgb(0, 90, 90, 90)), LinearGradientMode.Vertical);
			graphics.FillRectangle(linearBrush, rect);

			graphics.DrawString(text, Font, foreBrush, new Point(AbsoluteLocation.X + 2, AbsoluteLocation.Y + 3));
			graphics.FillRectangle(new SolidBrush(BackColor.Substract(Color.FromArgb(0, 50, 50, 50))), AbsoluteLocation.X + 5, AbsoluteLocation.Y + 17 + 2, Size.Width - 10, 1);

			var crossLocation = new Point(AbsoluteLocation.X + Size.Width - 16, AbsoluteLocation.Y + 6);
			for (var i = 0; i < 4; ++i)
			{
				graphics.FillRectangle(foreBrush, crossLocation.X + i, crossLocation.Y + i, 3, 1);
				graphics.FillRectangle(foreBrush, crossLocation.X + 6 - i, crossLocation.Y + i, 3, 1);
				graphics.FillRectangle(foreBrush, crossLocation.X + i, crossLocation.Y + 7 - i, 3, 1);
				graphics.FillRectangle(foreBrush, crossLocation.X + 6 - i, crossLocation.Y + 7 - i, 3, 1);
			}

			panel.Render(graphics);

			if (IsHighlighted)
			{
				using (var pen = new Pen(Color.Orange, 1))
				{
					graphics.DrawRectangle(pen, AbsoluteLocation.X - 2, AbsoluteLocation.Y - 2, Size.Width + 3, Size.Height + 3);
				}

				IsHighlighted = false;
			}
		}

		public override Control Copy()
		{
			throw new NotImplementedException();
		}

		public override string ToString()
		{
			return Name + " - Form";
		}

		public override void ReadPropertiesFromXml(XElement element)
		{
			base.ReadPropertiesFromXml(element);

			if (element.HasAttribute("text"))
				Text = Text.FromXMLString(element.Attribute("text").Value.Trim());
		}
	}
}
