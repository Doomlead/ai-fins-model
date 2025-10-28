﻿
namespace KSoft.Phoenix.Phx
{
	public sealed class BProtoShieldBubbleTypes
		: IO.ITagElementStringNameStreamable
	{
		#region Xml constants
		const string kXmlRoot = "ShieldBubbleTypes";
		#endregion

		int mDefaultShieldSquadID = TypeExtensions.kNone;
		[Meta.BProtoSquadReference]
		public int DefaultShieldSquadID
		{
			get { return this.mDefaultShieldSquadID; }
			set { this.mDefaultShieldSquadID = value; }
		}

		public Collections.BListArray<BProtoSquadShieldBubble> ProtoShieldIDs { get; private set; } = new Collections.BListArray<BProtoSquadShieldBubble>();

		public bool IsNotEmpty { get {
			return this.DefaultShieldSquadID.IsNotNone()
				|| !this.ProtoShieldIDs.IsEmpty;
		} }

		#region ITagElementStreamable<string> Members
		public void Serialize<TDoc, TCursor>(IO.TagElementStream<TDoc, TCursor, string> s)
			where TDoc : class
			where TCursor : class
		{
			var xs = s.GetSerializerInterface();

			using (var bm = s.EnterCursorBookmarkOpt(kXmlRoot)) if (bm.IsNotNull)
			{
				xs.StreamDBID(s, XML.XmlUtil.kNoXmlName, ref this.mDefaultShieldSquadID, DatabaseObjectKind.Squad, false, XML.XmlUtil.kSourceCursor);
				XML.XmlUtil.Serialize(s, this.ProtoShieldIDs, BProtoSquadShieldBubble.kBListXmlParams);
			}
		}
		#endregion
	};
}
