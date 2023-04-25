// (c) Mars-Energo Ltd.
// 
// EM-3.3T Device Data Structures
//
// Author			:	Andrew A. Golyakov 
// Version			:	2.0.0
// Last revision	:	01.03.2006 12:04
// version history	:
// 1.0.1	:	Changes in names and all inner structure. 
//				Now Content lines involve all-pages arrays with
//				taking into account
// 2.0.0	:	Memory map changes.

using System;

using EmServiceLib;

namespace DeviceIO
{
	/// <summary>
	/// Class of any one Main Record with real existing data
	/// </summary>
	[Serializable]
	public class ContentsLineEm33 : BaseDeviceCommonInfo
	{
		// PQP out info
		public bool PqpExists;
		public PqpSetEm33[] PqpSet;
		// fliker
		public short t_fliker;  // 1, 5 или 10

		// Dips & Overs out info
		public bool DnsExists;
		public DateTime DnsStart;
		public DateTime DnsEnd;
		public uint DnsNum;
		public uint DnsTimer;
		public ushort[] DnsPagesNAND;

		// Average values out info
		public bool AvgExists;
		public DateTime AvgBegin;
		public DateTime AvgEnd;
		public ushort AvgTime;
		public uint AvgNum;
		public ushort[] AvgPagesNAND;
	}

	/// <summary>
	/// Structure of one PQP record
	/// </summary>
	public struct PqpSetEm33
	{
		public ushort PqpPageNAND;		// адрес первой из восьми страниц NAND Flash, на которой 
										// располагаетс€ архив ѕ Ё	
		public DateTime PqpStart;
		public DateTime PqpEnd;
		public ushort[] UnfPagesNAND;	// адреса страниц с данными архивов напр€жений и частот
		public int UnfRecords;			// количество записей в архиве Ќапр€жений и „астот
		public int PqpZone;				// номер зоны ѕ Ё, в которой расположен данных архив
	}

	/// <summary>
	/// Stricture of common device information for EM33 and EM31
	/// </summary>
	public class DeviceCommonInfoEm33
	{
		public long SerialNumber;
		public ushort InternalType;		// 1 Ц Ём 3.1 , 3 Ц Ём 3.3“ и Ём 3.3“1
		public string Name;
		public string Version;
		public EmDeviceType DeviceType;

		public ContentsLineEm33[] Content;
	}

	public struct PqpZonesEm33
	{
		public ushort sPage;
		public ushort ePage;
		public bool Valid;		
	}
}