using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using KeePassDarkMode.Theme;

namespace KeePassDarkMode.Editor
{
	// Makes the Palette node expandable in the PropertyGrid: every palette
	// entry appears as a Color property with the regular color picker.
	internal class PaletteTypeConverter : TypeConverter
	{
		public override bool GetPropertiesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
		{
			var palette = (Palette) value;
			var descriptors = new List<PropertyDescriptor>();
			foreach (var name in palette.GetNames())
			{
				descriptors.Add(new PaletteColorDescriptor(name));
			}
			return new PropertyDescriptorCollection(descriptors.ToArray());
		}

		private class PaletteColorDescriptor : PropertyDescriptor
		{
			private readonly string _name;

			public PaletteColorDescriptor(string name) : base(name, null)
			{
				_name = name;
			}

			public override Type ComponentType
			{
				get { return typeof(Palette); }
			}

			public override bool IsReadOnly
			{
				get { return false; }
			}

			public override Type PropertyType
			{
				get { return typeof(Color); }
			}

			public override bool CanResetValue(object component)
			{
				return false;
			}

			public override object GetValue(object component)
			{
				return ((Palette) component).GetColor(_name);
			}

			public override void ResetValue(object component)
			{
			}

			public override void SetValue(object component, object value)
			{
				((Palette) component).SetColor(_name, (Color) value);
			}

			public override bool ShouldSerializeValue(object component)
			{
				return true;
			}
		}
	}
}
