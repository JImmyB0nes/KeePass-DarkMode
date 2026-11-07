using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using KeePass.App;
using KeePass.UI;
using KeePassDarkMode.Decorators;
using KeePassDarkMode.Options;
using KeePassDarkMode.Theme;

namespace KeePassDarkMode
{
	internal class DarkMode
	{
		private readonly DarkModeOptions _options;
		private readonly CustomTheme _defaultTheme;

		private readonly Dictionary<ToolStrip, CenteredSearchDecorator> _centeredSearch =
			new Dictionary<ToolStrip, CenteredSearchDecorator>();

		private readonly Dictionary<Button, ShieldButtonWindow> _shieldButtons =
			new Dictionary<Button, ShieldButtonWindow>();

		private ITheme _customTheme;
		private ITheme _theme;
		private bool _enabled;
		private bool _lightMode;

		// True while the current effective theme is the light variant
		// (auto-sync + light Windows) - drives template and title bar choices.
		internal bool LightMode
		{
			get { return _lightMode; }
		}

		public bool Enabled
		{
			get { return _enabled; }
			set { SetEnable(value); }
		}

		public string Name
		{
			get { return _customTheme.Name; }
		}

		public DarkMode(DarkModeOptions options)
		{
			_options = options;
			_defaultTheme = CustomTheme.GetDefaultTheme();
			_customTheme = GetCustomTheme();
			_theme = _defaultTheme;
		}

		private void SetEnable(bool enable)
		{
			_enabled = enable;

			if (_enabled)
				_customTheme = GetCustomTheme();
			else
				CleanupCenteredSearch();

			foreach (var shield in _shieldButtons.Values)
				shield.SetEnabled(_enabled);

			_theme = _enabled ? _customTheme : _defaultTheme;

			ToolStripManager.Renderer = _theme.ToolStripRenderer;
			ObjectListViewDecorator.Initialize();
			KnownColorsDecorator.Apply(_theme, _enabled);

			ApplyOther();
		}

		// Releases all centered-search decorators (theme off or plugin shutdown).
		public void CleanupCenteredSearch()
		{
			foreach (var decorator in _centeredSearch.Values)
				decorator.Dispose();
			_centeredSearch.Clear();
		}

		// Restores native shield buttons and releases the interceptors.
		public void CleanupShieldButtons()
		{
			foreach (var pair in _shieldButtons)
			{
				pair.Value.SetEnabled(false);
				pair.Key.Paint -= HandleShieldButtonPaint;
			}
			_shieldButtons.Clear();
		}

		// Re-read the template and swap in the new theme without toggling
		// Enabled - avoids the double repaint (flicker) of disable/enable.
		public void Reload()
		{
			if (!_enabled)
				return;

			_customTheme = GetCustomTheme();
			_theme = _customTheme;

			ToolStripManager.Renderer = _theme.ToolStripRenderer;
			ObjectListViewDecorator.Initialize();
			KnownColorsDecorator.Apply(_theme, true);

			ApplyOther();
		}

		private ITheme GetCustomTheme()
		{
			var templateFile = TemplateReader.Get(ResolveTemplatePath()) ?? TemplateReader.GetDefaultTemplate();
			// Last resort: if even the embedded default cannot be read, fall back to
			// the empty (light) theme instead of throwing during plugin load.
			if (templateFile == null)
				return _defaultTheme;

			var themeTemplate = new CustomThemeTemplate(templateFile);

			if (_options.AccentFromWindows)
				CustomThemeTemplate.ApplySystemAccent(themeTemplate.Palette);

			return new CustomTheme(themeTemplate);
		}

		// In auto-sync + auto-light mode the effective template follows the
		// current Windows app mode: dark template for dark, light template for light.
		private string ResolveTemplatePath()
		{
			_lightMode = false;
			if (_options.Enabled && _options.AutoSyncWithWin10Theme && _options.AutoLightTheme &&
			    Win10ThemeMonitor.IsDarkThemeEnabled() == false)
			{
				_lightMode = true;
				return _options.LightTemplate;
			}

			return _options.Template;
		}

		private void ApplyOther()
		{
			var colorControlNormalField =
				typeof(AppDefs).GetField("ColorControlNormal", BindingFlags.Static | BindingFlags.Public);
			var colorControlDisabledField =
				typeof(AppDefs).GetField("ColorControlDisabled", BindingFlags.Static | BindingFlags.Public);
			var colorEditError =
				typeof(AppDefs).GetField("ColorEditError", BindingFlags.Static | BindingFlags.Public);

			if (colorControlNormalField != null)
				colorControlNormalField.SetValue(null, _theme.Other.ControlNormalColor);

			if (colorControlDisabledField != null)
				colorControlDisabledField.SetValue(null, _theme.Other.ControlDisabledColor);

			if (colorEditError != null)
				colorEditError.SetValue(null, _theme.Other.ColorEditError);
		}

		public void Apply(Control control)
		{
			if (control.InvokeRequired)
			{
				control.Invoke(new MethodInvoker(() => Apply(control)));
				return;
			}

			if (!(control is ToolStrip))
			{
				control.BackColor = _theme.Control.BackColor;
				control.ForeColor = _theme.Control.ForeColor;
			}

			var form = control as Form;
			if (form != null) Apply(form);

			var userControl = control as UserControl;
			if (userControl != null) Apply(userControl);

			var dataGridView = control as DataGridView;
			if (dataGridView != null) Apply(dataGridView);

			var button = control as Button;
			if (button != null) Apply(button);

			var treeView = control as TreeView;
			if (treeView != null) Apply(treeView);

			var richTextBox = control as RichTextBox;
			if (richTextBox != null) Apply(richTextBox);

			var linkLabel = control as LinkLabel;
			if (linkLabel != null) Apply(linkLabel);

			var listView = control as ListView;
			if (listView != null) Apply(listView);

			var secureTextBoxEx = control as SecureTextBoxEx;
			if (secureTextBoxEx != null) Apply(secureTextBoxEx);

			var hotKeyControlEx = control as HotKeyControlEx;
			if (hotKeyControlEx != null) Apply(hotKeyControlEx);

			var toolStrip = control as ToolStrip;
			if (toolStrip != null) Apply(toolStrip);

			var menuStrip = control as MenuStrip;
			if (menuStrip != null) Apply(menuStrip);

			var contextMenuStrip = control as ContextMenuStrip;
			if (contextMenuStrip != null) Apply(contextMenuStrip);

			var statusStrip = control as StatusStrip;
			if (statusStrip != null) Apply(statusStrip);

			var tabControl = control as TabControl;
			if (tabControl != null) Apply(tabControl);

			var tabPage = control as TabPage;
			if (tabPage != null) Apply(tabPage);

			var qualityProgressBar = control as QualityProgressBar;
			if (qualityProgressBar != null) Apply(qualityProgressBar);

			var comboBox = control as ComboBox;
			if (comboBox != null) Apply(comboBox);

			var dateTimePicker = control as DateTimePicker;
			if (dateTimePicker != null) Apply(dateTimePicker);

			var checkBox = control as CheckBox;
			if (checkBox != null) Apply(checkBox);

			var propertyGrid = control as PropertyGrid;
			if (propertyGrid != null) Apply(propertyGrid);

			OverrideResetBackground(control);
			OverrideScrollBarsSetExplorerTheme(control);
		}

		private void OverrideScrollBarsSetExplorerTheme(Control control)
		{
			if (!CanHaveScrollBars(control))
				return;

			var useExplorerDarkMode = _theme.ScrollBar.UseExplorerDarkMode;
			TrySetWindowTheme(control.Handle, _enabled && useExplorerDarkMode);

			control.HandleCreated -= HandleControlCreated;
			control.HandleCreated += HandleControlCreated;

			var keePassForm = control as Form;
			if (keePassForm != null && keePassForm.GetType().Namespace.StartsWith("KeePass"))
			{
				keePassForm.Load -= HandleKeePassFormLoad;
				keePassForm.Load += HandleKeePassFormLoad;
			}
		}

		private void HandleControlCreated(object sender, EventArgs e)
		{
			var createdControl = sender as Control;
			if (createdControl != null && createdControl.IsHandleCreated)
				TrySetWindowTheme(createdControl.Handle, _enabled && _theme.ScrollBar.UseExplorerDarkMode);
		}

		private void HandleKeePassFormLoad(object sender, EventArgs e)
		{
			var privateCustomListsField = sender.GetType()
				.GetFields(BindingFlags.Instance | BindingFlags.NonPublic);

			foreach (var fieldInfo in privateCustomListsField)
			{
				if (fieldInfo.FieldType == typeof(CustomListViewEx))
				{
					var customListView = fieldInfo.GetValue(sender) as CustomListViewEx;
					if (customListView != null && customListView.IsHandleCreated)
					{
						TrySetWindowTheme(customListView.Handle, _enabled && _theme.ScrollBar.UseExplorerDarkMode);
					}
				}
			}
		}

		private bool CanHaveScrollBars(Control control)
		{
			return control is TextBoxBase ||
			       control is ListBox ||
			       control is ListView ||
			       control is TreeView ||
			       control is DataGridView ||
			       control is Panel ||
			       control is UserControl ||
			       control is Form ||
			       control is ScrollableControl ||
			       control is TabControl;
		}

		private void OverrideResetBackground(Control control)
		{
			// Named handler instead of a lambda: a fresh closure per Visit() would
			// defeat -=, so re-applied themes would stack handlers on every reload.
			if ((control.Name == "m_cmbStringName" && control.Parent.Name == "EditStringForm") ||
			    (control.Name == "m_tbSearch" && control.Parent.Name == "OptionsForm"))
			{
				control.BackColorChanged -= HandleSystemColorReset;
				control.BackColorChanged += HandleSystemColorReset;
			}
		}

		private void HandleSystemColorReset(object sender, EventArgs args)
		{
			var control = (Control) sender;
			if (control.BackColor.IsSystemColor)
				control.BackColor = _theme.Control.BackColor;
		}

		private void Apply(DataGridView dataGridView)
		{
			dataGridView.BackgroundColor = _theme.Control.BackColor;
			dataGridView.RowsDefaultCellStyle.BackColor = _theme.Control.BackColor;
			dataGridView.RowsDefaultCellStyle.ForeColor = _theme.Control.ForeColor;
		}

		private void Apply(CheckBox checkBox)
		{
			var checkBoxLook = checkBox.Appearance == Appearance.Button
				? _theme.CheckBoxButton
				: _theme.CheckBox;

			checkBox.BackColor = checkBoxLook.BackColor;
			checkBox.ForeColor = checkBoxLook.ForeColor;
			checkBox.FlatStyle = checkBoxLook.FlatStyle;
			checkBox.FlatAppearance.BorderColor = checkBoxLook.BorderColor;
			checkBox.FlatAppearance.CheckedBackColor = checkBoxLook.CheckedBackColor;
			checkBox.FlatAppearance.MouseDownBackColor = checkBoxLook.MouseDownBackColor;
			checkBox.FlatAppearance.MouseOverBackColor = checkBoxLook.MouseOverBackColor;

			checkBox.EnabledChanged -= HandleCheckBoxEnabledChanged;
			checkBox.EnabledChanged += HandleCheckBoxEnabledChanged;
		}

		private void HandleCheckBoxEnabledChanged(object sender, EventArgs e)
		{
			var checkBox = (CheckBox) sender;
			if (checkBox.Enabled)
			{
				checkBox.Paint -= HandleCheckBoxPaint;
				checkBox.Invalidate();
			}
			else
			{
				checkBox.Paint += HandleCheckBoxPaint;
			}
		}

		private void HandleCheckBoxPaint(object sender, PaintEventArgs e)
		{
			var checkBox = (CheckBox) sender;
			var disabledForeColor = ControlPaint.Dark(_theme.Button.ForeColor, 0.25f);
			var glyphSize = CheckBoxRenderer.GetGlyphSize(e.Graphics, CheckBoxState.UncheckedNormal);
			var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine;
			var clientRectangle = new Rectangle(glyphSize.Width, -1,
				checkBox.ClientRectangle.Size.Width - glyphSize.Width, checkBox.ClientRectangle.Size.Height);

			TextRenderer.DrawText(e.Graphics, checkBox.Text, checkBox.Font, clientRectangle, disabledForeColor, flags);
		}

		private void Apply(ComboBox comboBox)
		{
			if (comboBox.DropDownStyle == ComboBoxStyle.DropDownList)
				comboBox.FlatStyle = FlatStyle.Popup;

			comboBox.BackColorChanged -= HandleComboBoxBackColorChanged;
			comboBox.BackColorChanged += HandleComboBoxBackColorChanged;
		}

		private void Apply(DateTimePicker dateTimePicker)
		{
			dateTimePicker.BackColor = _theme.Control.BackColor;
			dateTimePicker.ForeColor = _theme.Control.ForeColor;

			// The themed display field is painted by the OS and ignores
			// BackColor, so the value text gets overpainted dark (Paint runs
			// after the OS painting). Only the popup calendar is colorized
			// natively via CalendarColorizer.
			dateTimePicker.Paint -= HandleDateTimePickerPaint;
			dateTimePicker.Paint += HandleDateTimePickerPaint;

			// Month calendar popup gets our palette when it opens (see CalendarColorizer).
			dateTimePicker.DropDown -= HandleDateTimePickerDropDown;
			dateTimePicker.DropDown += HandleDateTimePickerDropDown;
			dateTimePicker.CloseUp -= HandleDateTimePickerCloseUp;
			dateTimePicker.CloseUp += HandleDateTimePickerCloseUp;
		}

		private void HandleDateTimePickerPaint(object sender, PaintEventArgs e)
		{
			if (!_enabled)
				return;

			var picker = (DateTimePicker) sender;
			var rect = picker.ClientRectangle;

			// Keep the leading check box and the trailing OS-drawn calendar/
			// drop-down buttons intact - only the value text area is repainted.
			var leftInset = picker.ShowCheckBox ? DpiUtil.ScaleIntX(16) : 0;
			var rightInset = DpiUtil.ScaleIntX(36);
			var field = Rectangle.FromLTRB(rect.X + leftInset, rect.Y,
				Math.Max(rect.X + leftInset + 1, rect.Right - rightInset), rect.Bottom);

			using (var background = new SolidBrush(_theme.Control.BackColor))
				e.Graphics.FillRectangle(background, field);

			var textColor = picker.Enabled
				? _theme.Control.ForeColor
				: Color.FromArgb(150, _theme.Control.ForeColor);

			var textRect = new Rectangle(field.X + DpiUtil.ScaleIntX(3), field.Y,
				field.Width - DpiUtil.ScaleIntX(6), field.Height);
			TextRenderer.DrawText(e.Graphics, picker.Text, picker.Font, textRect,
				textColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
		}

		private void HandleDateTimePickerDropDown(object sender, EventArgs e)
		{
			var picker = (DateTimePicker) sender;
			if (!_enabled)
				return;

			var calendar = CalendarColorizer.GetCalendar(picker.Handle);
			CalendarColorizer.Colorize(calendar,
				_theme.Control.BackColor,
				_theme.Control.ForeColor,
				_theme.ListView.HeaderBackColor);
		}

		private void HandleDateTimePickerCloseUp(object sender, EventArgs e)
		{
			var picker = (DateTimePicker) sender;
			var calendar = CalendarColorizer.GetCalendar(picker.Handle);
			CalendarColorizer.Restore(calendar);
		}

		private void HandleComboBoxBackColorChanged(object sender, EventArgs e)
		{
			if (!_enabled)
			{
				return;
			}

			var comboBox = (ComboBox) sender;
			if (comboBox.BackColor == SystemColors.Window)
				comboBox.BackColor = _theme.Control.BackColor;
		}

		private void Apply(QualityProgressBar qualityProgressBar)
		{
			// Paint runs after the control's own Visual-Styles painting, so the
			// overpaint below wins: dark track, gradient fill, light text.
			qualityProgressBar.Paint -= HandleQualityBarPaint;
			qualityProgressBar.Paint += HandleQualityBarPaint;
			qualityProgressBar.Invalidate();
		}

		private void HandleShieldButtonPaint(object sender, PaintEventArgs e)
		{
			if (!_enabled)
				return;

			var button = (Button) sender;
			ShieldButtonWindow shield;
			if (!_shieldButtons.TryGetValue(button, out shield) || !shield.ShieldRequested)
				return;

			// Native shield rendering was suppressed - draw it ourselves.
			ShieldButtonWindow.DrawShield(e.Graphics, button.ClientRectangle);
		}

		private void HandleQualityBarPaint(object sender, PaintEventArgs e)
		{
			if (!_enabled)
				return;

			var bar = (QualityProgressBar) sender;
			var rect = bar.ClientRectangle;
			var g = e.Graphics;

			using (var background = new SolidBrush(_theme.Control.BackColor))
				g.FillRectangle(background, rect);

			var range = bar.Maximum - bar.Minimum;
			if (range > 0)
			{
				var position = Math.Max(0, Math.Min(range, bar.Value - bar.Minimum));
				var width = (int) ((float) rect.Width * position / range);
				if (width > 0)
				{
					using (var gradient = new LinearGradientBrush(rect,
						AppDefs.ColorQualityLow, AppDefs.ColorQualityHigh,
						LinearGradientMode.Horizontal))
					{
						g.FillRectangle(gradient, rect.X, rect.Y, width, rect.Height);
					}
				}
			}

			if (!string.IsNullOrEmpty(bar.Text))
			{
				TextRenderer.DrawText(g, bar.Text, bar.Font, rect,
					_theme.Control.ForeColor,
					TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
			}
		}

		private void Apply(TabControl tabControl)
		{
			var decoratorName = tabControl.Name + "_decorator";
			var decorator =
				tabControl.Parent.Controls.Find(decoratorName, false).FirstOrDefault() as TabControlDecorator;

			if (decorator == null)
			{
				decorator = new TabControlDecorator(tabControl, _theme);
				decorator.Name = decoratorName;
			}

			tabControl.BackColor = _theme.TabControl.BackColor;
			tabControl.ForeColor = _theme.TabControl.ForeColor;

			decorator.EnableTheme(_enabled, _theme);

			tabControl.ControlAdded -= HandleTabControlAdded;
			tabControl.ControlAdded += HandleTabControlAdded;
		}

		private void Apply(TabPage tabPage)
		{
			tabPage.Invalidated -= HandleTabPageInvalidated;
			tabPage.Invalidated += HandleTabPageInvalidated;
		}

		private void HandleTabPageInvalidated(object sender, InvalidateEventArgs e)
		{
			var tabPage = (TabPage) sender;
			if (tabPage.UseVisualStyleBackColor)
			{
				tabPage.UseVisualStyleBackColor = false;
			}
		}

		private void HandleTabControlAdded(object sender, ControlEventArgs e)
		{
			if (e.Control is TabPage)
			{
				var visitor = new ControlVisitor(Apply);
				visitor.Visit(e.Control);
			}
		}

		private void Apply(StatusStrip statusStrip)
		{
			statusStrip.BackColor = _theme.MenuItem.BackColor;
			statusStrip.ForeColor = _theme.MenuItem.ForeColor;

			Apply(statusStrip.Items);
		}

		private void Apply(ContextMenuStrip contextMenuStrip)
		{
			contextMenuStrip.BackColor = _theme.MenuItem.BackColor;
			contextMenuStrip.ForeColor = _theme.MenuItem.ForeColor;

			Apply(contextMenuStrip.Items);
		}

		private void Apply(MenuStrip menuStrip)
		{
			menuStrip.BackColor = _theme.MenuItem.BackColor;
			menuStrip.ForeColor = _theme.MenuItem.ForeColor;

			Apply(menuStrip.Items);
		}

		private void Apply(ToolStrip toolStrip)
		{
			toolStrip.BackColor = _theme.MenuItem.BackColor;
			toolStrip.ForeColor = _theme.MenuItem.ForeColor;

			// Keep the quick-find box centered like a modern search bar.
			CenteredSearchDecorator centered;
			if (_centeredSearch.TryGetValue(toolStrip, out centered))
			{
				centered.Dispose();
				_centeredSearch.Remove(toolStrip);
			}

			Apply(toolStrip.Items);

			var search = toolStrip.Items.OfType<ToolStripComboBox>().FirstOrDefault(x =>
				x.Name == "m_tbQuickFind" || x.Name == "m_tbQuickSearch");
			var ownerForm = toolStrip.FindForm();
			if (_enabled && search != null && ownerForm != null &&
				ownerForm.GetType().FullName == "KeePass.Forms.MainForm")
			{
				_centeredSearch.Add(toolStrip, new CenteredSearchDecorator(toolStrip, search, _theme));
				toolStrip.Disposed -= HandleCenteredStripDisposed;
				toolStrip.Disposed += HandleCenteredStripDisposed;
			}
		}

		private void HandleCenteredStripDisposed(object sender, EventArgs e)
		{
			var strip = (ToolStrip) sender;
			CenteredSearchDecorator centered;
			if (_centeredSearch.TryGetValue(strip, out centered))
			{
				centered.Dispose();
				_centeredSearch.Remove(strip);
			}
		}

		private void Apply(ToolStripItemCollection toolStripItemCollection)
		{
			foreach (ToolStripItem item in toolStripItemCollection)
			{
				item.ForeColor = _theme.MenuItem.ForeColor;
				item.BackColor = _theme.MenuItem.BackColor;

				var menuItem = item as ToolStripMenuItem;
				if (menuItem != null)
				{
					menuItem.DropDownOpening -= HandleMenuItemOnDropDownOpening;
					menuItem.DropDownOpening += HandleMenuItemOnDropDownOpening;
				}
			}
		}

		private void HandleMenuItemOnDropDownOpening(object sender, EventArgs e)
		{
			var menuItem = (ToolStripMenuItem) sender;
			Apply(menuItem.DropDownItems);
		}

		private void Apply(SecureTextBoxEx secureTextBoxEx)
		{
			secureTextBoxEx.BackColorChanged -= HandleSecureTextBoxExOnBackColorChanged;
			secureTextBoxEx.BackColorChanged += HandleSecureTextBoxExOnBackColorChanged;
		}

		private void HandleSecureTextBoxExOnBackColorChanged(object sender, EventArgs e)
		{
			if (!_enabled)
			{
				return;
			}

			var textBox = (SecureTextBoxEx) sender;
			if (textBox.BackColor == SystemColors.Window)
				textBox.BackColor = _theme.SecureTextBox.BackColor;
		}

		private void Apply(HotKeyControlEx hotKeyControlEx)
		{
			hotKeyControlEx.BackColorChanged -= HandleHotKeyControlExOnBackColorChanged;
			hotKeyControlEx.BackColorChanged += HandleHotKeyControlExOnBackColorChanged;
		}

		private void HandleHotKeyControlExOnBackColorChanged(object sender, EventArgs e)
		{
			if (!_enabled)
			{
				return;
			}

			var textBox = (HotKeyControlEx) sender;
			if (textBox.BackColor == SystemColors.Window)
				textBox.BackColor = _theme.Control.BackColor;
		}

		private void Apply(Form form)
		{
			form.BackColor = _theme.Form.BackColor;
			form.ForeColor = _theme.Form.ForeColor;

			// Title bar in theme color (Win11 22H2+, no-op elsewhere).
			var caption = _theme.Form.BackColor;
			var dark = _enabled && caption.GetBrightness() < 0.5;
			CaptionTint.Apply(form, caption, _theme.Form.ForeColor, dark);

			foreach (var component in GetComponents(form))
			{
				Apply(component);
			}
		}

		private void Apply(UserControl userControl)
		{
			userControl.BackColor = _theme.Form.BackColor;
			userControl.ForeColor = _theme.Form.ForeColor;

			foreach (var component in GetComponents(userControl))
			{
				Apply(component);
			}
		}

		private IEnumerable<Control> GetComponents(ContainerControl containerControl)
		{
			var componentsField = containerControl.GetType()
				.GetField("components", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			if (componentsField != null)
			{
				var components = componentsField.GetValue(containerControl) as IContainer;
				if (components != null)
				{
					// Context menus (e.g. the tray icon's m_ctxTray) live in the
					// components container and are Components, not Controls - they
					// would be missed otherwise and stay light (tray menu issue).
					return components.Components
						.OfType<Control>()
						.Concat(components.Components.OfType<ToolStrip>()
							.Cast<Control>());
				}
			}

			return Enumerable.Empty<Control>();
		}

		private void Apply(Button button)
		{
			button.BackColor = _theme.Button.BackColor;
			button.ForeColor = _theme.Button.ForeColor;
			button.FlatAppearance.BorderColor = _theme.Button.BorderColor;
			button.FlatStyle = _theme.Button.FlatStyle;

			// UAC-shield buttons (OK on protected actions) are forced to
			// FlatStyle.System by KeePass, which renders them native white.
			ShieldButtonWindow shield;
			if (!_shieldButtons.TryGetValue(button, out shield))
			{
				shield = new ShieldButtonWindow(button);
				_shieldButtons.Add(button, shield);
				button.Disposed += (s, e) => _shieldButtons.Remove(button);
			}
			shield.SetEnabled(_enabled);

			button.Paint -= HandleShieldButtonPaint;
			if (_enabled)
				button.Paint += HandleShieldButtonPaint;

			if (button is SplitButtonEx)
			{
				var decorator = button.Controls.OfType<SplitButtonExDecorator>().FirstOrDefault()
				                ?? new SplitButtonExDecorator((SplitButtonEx) button, _theme);

				decorator.EnableTheme(_enabled, _theme);
			}

			button.EnabledChanged -= HandleButtonEnabledChanged;
			button.EnabledChanged += HandleButtonEnabledChanged;

			// Apply disabled state immediately (issue #146): buttons that are
			// disabled when the form opens never fire EnabledChanged.
			if (!button.Enabled)
				HandleButtonEnabledChanged(button, EventArgs.Empty);
		}

		private void HandleButtonEnabledChanged(object sender, EventArgs e)
		{
			var button = (Button) sender;
			if (button.Enabled)
			{
				button.Paint -= HandleButtonPaint;
			}
			else
			{
				button.Paint += HandleButtonPaint;
			}
		}

		private void HandleButtonPaint(object sender, PaintEventArgs e)
		{
			var button = (Button) sender;
			var disabledForeColor = ControlPaint.Dark(_theme.Button.ForeColor, 0.25f);
			if (button.Enabled)
			{
				disabledForeColor = _theme.Button.ForeColor;
			}

			var flags = TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
			TextRenderer.DrawText(e.Graphics, button.Text, button.Font, button.ClientRectangle, disabledForeColor,
				flags);
		}

		private void Apply(LinkLabel linkLabel)
		{
			linkLabel.LinkColor = _theme.LinkLabel.LinkColor;
		}

		private void Apply(TreeView treeView)
		{
			treeView.BorderStyle = _theme.TreeView.BorderStyle;
			treeView.BackColor = _theme.TreeView.BackColor;

			treeView.DrawMode = _theme.TreeViewDrawMode;
			treeView.DrawNode -= HandleTreeViewDrawNode;
			treeView.DrawNode += HandleTreeViewDrawNode;
		}

		[DllImport("UxTheme.dll", ExactSpelling = true, CharSet = CharSet.Unicode)]
		private static extern int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

		public static void TrySetWindowTheme(IntPtr hWnd, bool enable)
		{
			if (hWnd == IntPtr.Zero)
				return;

			try
			{
				SetWindowTheme(hWnd, enable ? "DarkMode_Explorer" : "explorer", null);
			}
			catch (Exception)
			{
				// ignored
			}
		}

		private void HandleTreeViewDrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			// DrawDefault = true does not have TextFormatFlags.NoPrefix flag set
			var node = e.Node;

			var isNodeSelected = (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
			var foreColor = isNodeSelected && node.TreeView.Focused
				? _theme.TreeView.SelectionColor
				: _theme.TreeView.ForeColor != Color.Empty
					? _theme.TreeView.ForeColor
					: node.TreeView.ForeColor;

			var backColor = isNodeSelected ? _theme.TreeView.SelectionBackColor : _theme.TreeView.BackColor;

			var font = node.NodeFont ?? node.TreeView.Font;
			var size = TextRenderer.MeasureText(node.Text, font, e.Bounds.Size, TextFormatFlags.NoPrefix);
			var rectangle = new Rectangle(new Point(node.Bounds.X - 1, node.Bounds.Y),
				new Size(size.Width, node.Bounds.Height));

			using (var backColorBrush = new SolidBrush(backColor))
				e.Graphics.FillRectangle(backColorBrush, rectangle);

			if (isNodeSelected && node.TreeView.Focused)
				ControlPaint.DrawFocusRectangle(e.Graphics, rectangle, foreColor, backColor);

			TextRenderer.DrawText(e.Graphics, node.Text, font, rectangle, foreColor, TextFormatFlags.NoPrefix);
		}

		private readonly Dictionary<RichTextBox, RichTextBoxDecorator> _richTextBoxDecorators =
			new Dictionary<RichTextBox, RichTextBoxDecorator>();

		private void Apply(RichTextBox richTextBox)
		{
			RichTextBoxDecorator decorator;
			if (!_richTextBoxDecorators.TryGetValue(richTextBox, out decorator))
			{
				decorator = new RichTextBoxDecorator(richTextBox, _theme);
				_richTextBoxDecorators.Add(richTextBox, decorator);
			}

			decorator.EnableTheme(_enabled, _theme);
		}

		private void Apply(ListView listView)
		{
			// Dark vector glyphs for the standard icon picker's preview list.
			var pickerForm = listView.FindForm();
			if (listView.Name == "m_lvIcons" && pickerForm != null &&
				pickerForm.GetType().FullName == "KeePass.Forms.IconPickerForm")
			{
				var picker = listView.Controls.OfType<StandardIconPickerDecorator>().FirstOrDefault()
				             ?? new StandardIconPickerDecorator(listView);
				picker.Apply(_enabled, _theme);
			}

			if (ObjectListViewDecorator.CanDecorate(listView))
			{
				ObjectListViewDecorator.Apply(listView, _theme);
				return;
			}

			var decorator = listView.Controls.OfType<ListViewDecorator>().FirstOrDefault()
			                ?? new ListViewDecorator(listView, _theme);

			decorator.EnableTheme(_enabled, _theme);
		}

		private void Apply(PropertyGrid propertyGrid)
		{
			propertyGrid.CategoryForeColor = _theme.PropertyGrid.CategoryForeColor;
			propertyGrid.LineColor = _theme.PropertyGrid.LineColor;
		}
	}
}