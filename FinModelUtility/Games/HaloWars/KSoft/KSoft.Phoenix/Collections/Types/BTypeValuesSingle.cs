using System;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public sealed class BTypeValuesSingle
		: BTypeValuesBase<float>
	{
		public BTypeValuesSingle(BTypeValuesParams<float> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}

		public bool HasNonZeroItems { get {
			for (int x = 0; x < this.Count; x++)
				if (this[x] != 0.0f)
					return true;

			return false;
		} }
	};
}