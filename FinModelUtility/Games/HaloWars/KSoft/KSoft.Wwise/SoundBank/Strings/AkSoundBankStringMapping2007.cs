﻿#if CONTRACTS_FULL_SHIM
using Contract = System.Diagnostics.ContractsShim.Contract;
#else
using Contract = System.Diagnostics.Contracts.Contract; // SHIM'D
#endif

namespace KSoft.Wwise.SoundBank
{
	sealed class AkSoundBankStringMapping2007
		: AkSoundBankStringMappingBase
	{
		public struct StringHashEntry
			: IO.IEndianStreamSerializable
		{
			public uint Offset; // if this is -1, we have to skip it. TODO: serialize entries into memory stream?
			public uint Key, ID;

			public string Value;

			#region IEndianStreamSerializable Members
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.Offset);
			}
			#endregion
		};
		public sealed class StringGroup
			: IO.IEndianStreamSerializable
		{
			AKBKHashHeader mHeader;
			public StringHashEntry[] Entries;

			public uint ID { get { return this.mHeader.Hash; } }

			#region IEndianStreamSerializable Members
			void SerializeGroupEntries(IO.EndianStream s)
			{
				s.StreamArrayInt32(ref this.Entries);
				for (int x = 0; x < this.Entries.Length; x++)
				{
					s.Stream(ref this.Entries[x].ID);
					s.Stream(ref this.Entries[x].Value, Memory.Strings.StringStorage.CStringAscii);
					s.Stream(ref this.Entries[x].Key);
				}
			}
			public void Serialize(IO.EndianStream s)
			{
				s.Stream(ref this.mHeader);

				long eos = EndOfStream(s, this.mHeader);
				this.SerializeGroupEntries(s);
				Contract.Assert(s.BaseStream.Position == eos);
			}
			#endregion
		};

		public StringHashEntry[] Events;

		static void SerializeEntries(IO.EndianStream s, ref StringHashEntry[] entries)
		{
			s.StreamArrayInt32(ref entries);
			for (int x = 0; x < entries.Length; x++)
			{
				s.Stream(ref entries[x].Value, Memory.Strings.StringStorage.CStringAscii);
				s.Stream(ref entries[x].Key);
			}
		}

	void SerializeStringType(IO.EndianStream s, AKBKHashHeader hdr, AkSoundBank bank)
		{
			switch (hdr.Type)
			{
				case StringType.OldEvents:
					SerializeEntries(s, ref this.Events);
					break;
			}
		}

		public override void Serialize(IO.EndianStream s, AkSubchunkHeader header)
		{
			Contract.Assert(s.IsReading);

			var bank = s.Owner as AkSoundBank;

			long eos = EndOfStream(s, header);

			while (s.BaseStream.Position != eos)
			{
				AKBKHashHeader hdr = new AKBKHashHeader();
				hdr.Serialize(s);

				this.SerializeStringType(s, hdr, bank);
			}
		}
	};
}