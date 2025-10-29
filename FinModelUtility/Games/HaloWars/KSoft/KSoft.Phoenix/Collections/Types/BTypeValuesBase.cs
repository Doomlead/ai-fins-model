using System;
using Contract = System.Diagnostics.Contracts.Contract;

namespace KSoft.Collections
{
	public abstract class BTypeValuesBase<T>
		: BListExplicitIndexBase<T>
	{
		internal BTypeValuesParams<T> TypeValuesParams { get { return this.Params as BTypeValuesParams<T>; } }

		protected BTypeValuesBase(BTypeValuesParams<T> @params) : base(@params)
		{
			Contract.Requires<ArgumentNullException>(@params != null);
		}
	};
}