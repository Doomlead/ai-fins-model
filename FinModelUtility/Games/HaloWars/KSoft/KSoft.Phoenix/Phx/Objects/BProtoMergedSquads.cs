﻿using System.Collections.Generic;

using BProtoSquadID = System.Int32;

namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoMergedSquads
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		public static readonly XML.BListXmlParams kBListXmlParams = new XML.BListXmlParams
		{
			ElementName = "MergedSquads",
		};
		#endregion

		BProtoSquadID mToMergeSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public BProtoSquadID ToMergeSquadID
		{
			get { return this.mToMergeSquadID; }
			set { this.mToMergeSquadID = value; }
		}

		[Meta.BProtoSquadReference]
		public List<BProtoSquadID> BaseSquadIDs { get; private set; } = [];

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mToMergeSquadID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceCursor);
			s.StreamElements("MergedSquad", this.BaseSquadIDs, xs, XML.BXmlSerializerInterface.StreamSquadID);
		}
		#endregion
	};
}
