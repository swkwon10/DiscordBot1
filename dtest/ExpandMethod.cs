using System;
using System.Collections.Generic;
using System.Text;

namespace dtest
{
	public static class ExpandMethod
	{
		public static int GetLine(this string a)
		{
			return a.Split("\n").Length;
		}

		public static bool Equals_IgnCase(this string a , string b)
		{
			return a.Equals(b, StringComparison.OrdinalIgnoreCase);
		}
	}
}
