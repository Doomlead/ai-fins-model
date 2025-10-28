﻿using System;

namespace KSoft.T4
{
	/// <summary>Type definition for a code primitive</summary>
	public class PrimitiveCodeDefinition
	{
		public string Keyword { get; private set; }
		public TypeCode Code { get; private set; }

		public string SimpleDesc { get; private set; }

		protected PrimitiveCodeDefinition(string keyword, TypeCode typeCode)
		{
			this.Keyword = keyword;
			this.Code = typeCode;

			this.SimpleDesc = "NO DESC";
		}

		public PrimitiveCodeDefinition SetupDescription(string simpleDesc)
		{
			this.SimpleDesc = simpleDesc;

			return this;
		}

		public virtual bool IsInteger { get {
			return false;
		} }

		public virtual int SizeOfInBytes { get {
			return 0;
		} }
		public virtual int SizeOfInBits { get {
			return this.SizeOfInBytes * 8;
		} }
	};
}
