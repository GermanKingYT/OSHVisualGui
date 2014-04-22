﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Globalization;

namespace OSHVisualGui
{
	class ColorTextBox : TextBox
	{
		public enum ColorStyle
		{
			RGB,
			ARGB,
			HEX
		}

		private ColorStyle style;
		public ColorStyle Style { get { return style; } set { style = value; ColorToText(Color); } }
		private Color color;
		public Color Color
		{
			get
			{
				return color;
			}
			set
			{
				if (color != value)
				{
					color = value;
					BackColor = Color.FromArgb(value.R, value.G, value.B); //filter alpha
					ColorToText(color);
					ForeColor = value.GetBrightness() > 0.5f ? Color.Black : Color.White;
					OnColorChanged();
				}
			}
		}

		private Button switchStyle;
		private Button openColorPicker;
		private Color buttonForeColor;
		private Color buttonBackColor;
		private PoperContainer poperContainer;
		private ColorPicker colorPicker;

		public event EventHandler ColorChanged;
		public delegate void ColorPickerHoverEventHandler(Color color);
		public event ColorPickerHoverEventHandler ColorPickerHover;

		public ColorTextBox()
		{
			style = ColorStyle.RGB;

			switchStyle = new Button();
			switchStyle.FlatStyle = FlatStyle.Flat;
			switchStyle.Width = switchStyle.Height = Height - 2;
			switchStyle.TextAlign = ContentAlignment.TopCenter;
			switchStyle.Text = "Y";
			switchStyle.Cursor = Cursors.Default;
			switchStyle.Dock = DockStyle.Right;
			switchStyle.Click += new EventHandler(switchStyle_Click);
			Controls.Add(switchStyle);

			openColorPicker = new Button();
			openColorPicker.FlatStyle = FlatStyle.Flat;
			openColorPicker.Width = switchStyle.Height = Height - 2;
			openColorPicker.Dock = DockStyle.Right;
			openColorPicker.Text = "°";
			openColorPicker.Cursor = Cursors.Default;
			openColorPicker.Click += new EventHandler(openColorPicker_Click);
			Controls.Add(openColorPicker);

			colorPicker = new ColorPicker();
			colorPicker.MouseMove += new MouseEventHandler(delegate(object sender, MouseEventArgs e)
			{
				if (ColorPickerHover != null)
				{
					ColorPickerHover(colorPicker.HoverColor);
				}
			});
			colorPicker.ColorPicked += new EventHandler(delegate(object sender, EventArgs e)
			{
				this.Color = colorPicker.SelectedColor;
				poperContainer.Hide();
			});

			buttonForeColor = switchStyle.ForeColor;
			buttonBackColor = switchStyle.BackColor;
		}

		private void ColorToText(Color color)
		{
			switch (style)
			{
				case ColorStyle.RGB:
					base.Text = string.Format("{0}/{1}/{2}", color.R, color.G, color.B);
					break;
				case ColorStyle.ARGB:
					base.Text = string.Format("{0}/{1}/{2}/{3}", color.A, color.R, color.G, color.B);
					break;
				case ColorStyle.HEX:
					base.Text = string.Format("{0:X08}", color.ToArgb());
					break;
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Return)
			{
				e.Handled = true;

				Color colorByName = Text.ToLower() == "none" || Text.ToLower() == "empty" ? Color.Empty : Color.FromName(Text);
				if (colorByName.IsKnownColor || colorByName == Color.Empty)
				{
					Color = colorByName;
				}
				else
				{
					Regex colorRegex = new Regex(@"\b(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)/){2,3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b|([0-9a-fA-F]{8})", RegexOptions.Compiled);
					if (colorRegex.IsMatch(Text))
					{
						string[] seperated = Text.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
						if (seperated.Length == 3 || seperated.Length == 4)
						{
							style = ColorStyle.RGB;

							int index = 0;
							int a = 255;

							if (seperated.Length == 4)
							{
								style = ColorStyle.ARGB;
								a = int.Parse(seperated[index]);
								index = 1;
							}

							int[] rgb = new int[3];
							for (int i = 0; i < 3; ++i, ++index)
							{
								rgb[i] = int.Parse(seperated[index]);
							}

							Color = Color.FromArgb(a, rgb[0], rgb[1], rgb[2]);
						}
						else
						{
							int argb = int.Parse(Text, NumberStyles.HexNumber, CultureInfo.CurrentCulture);
							style = ColorStyle.HEX;
							Color = Color.FromArgb(argb);
						}
					}
					else
					{
						ColorToText(BackColor);
					}
				}
			}

			base.OnKeyDown(e);
		}

		protected override void OnSizeChanged(EventArgs e)
		{
			base.OnSizeChanged(e);

			switchStyle.Width = switchStyle.Height = Height - 2;
		}

		protected override void OnForeColorChanged(EventArgs e)
		{
			base.OnForeColorChanged(e);
			switchStyle.ForeColor = openColorPicker.ForeColor = buttonForeColor;
		}

		protected override void OnBackColorChanged(EventArgs e)
		{
			base.OnBackColorChanged(e);
			switchStyle.BackColor = openColorPicker.BackColor = buttonBackColor;
		}

		private void switchStyle_Click(object sender, EventArgs e)
		{
			switch (Style)
			{
				case ColorStyle.RGB:
					Style = ColorStyle.ARGB;
					break;
				case ColorStyle.ARGB:
					Style = ColorStyle.HEX;
					break;
				case ColorStyle.HEX:
					Style = ColorStyle.RGB;
					break;
			}
		}

		private void openColorPicker_Click(object sender, EventArgs e)
		{
			poperContainer = new PoperContainer(colorPicker);
			poperContainer.Show(openColorPicker.PointToScreen(new Point(0, openColorPicker.Height)));
		}

		private void OnColorChanged()
		{
			if (ColorChanged != null)
			{
				ColorChanged(this, EventArgs.Empty);
			}
		}
	}
}
