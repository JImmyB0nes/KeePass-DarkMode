using System.Collections;
using System.Windows.Forms;

namespace KeePassDarkMode
{
	public class FormsArrayList : ArrayList
	{
		public event FormAddedEventHandler Added; 
		
		public override int Add(object value)
		{
			var args = new FormAddedEventArgs((Form) value);
			if (Added != null)
				Added.Invoke(this, args);
			return base.Add(value);
		}
	}
}