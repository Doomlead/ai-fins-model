using System;
using System.Collections;
using System.Collections.Generic;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public sealed class InvertedComparer : IComparer
	{
		readonly IComparer mComparer;

		public InvertedComparer(IComparer comparer)
		{
			Contract.Requires<ArgumentNullException>(comparer != null);
			this.mComparer = comparer;
		}

		#region IComparer Members
		public int Compare(object x, object y)
		{
			return -this.mComparer.Compare(x, y);
		}
		#endregion
	};

	public sealed class InvertedComparer<T> : IComparer<T>
	{
		readonly IComparer<T> mComparer;

		public InvertedComparer(IComparer<T> comparer)
		{
			Contract.Requires<ArgumentNullException>(comparer != null);
			this.mComparer = comparer;
		}

		#region IComparer<T> Members
		public int Compare(T x, T y)
		{
			return -this.mComparer.Compare(x, y);
		}
		#endregion
	};
}