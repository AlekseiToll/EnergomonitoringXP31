using System;
using System.Collections.Generic;

using EmServiceLib;

namespace DeviceIO
{
	/// <summary>
	/// Stricture of common device information for EM32
	/// </summary>
	public class DeviceCommonInfoEm32 : BaseDeviceCommonInfo
	{
		#region Fields

		public short ConstraintType;

		// PQP out info
		public bool PqpExists = false;
		public PqpSetEm32[] PqpSet;
		// fliker
		public short t_fliker;  // всегда 10

		// Dips & Swells out info
		//public bool DnsExists;
		//public DnsSet32[] DnsSet;

		//public Int64[] PointersDip = new Int64[EmService.CountPhases];
		//public Int64[] PointersSwell = new Int64[EmService.CountPhases];

		// pointers to current dips & swells
		public DnsPointerData[] CurPointersDip = new DnsPointerData[EmService.CountPhases];
		public DnsPointerData[] CurPointersSwell = new DnsPointerData[EmService.CountPhases];

		// buffer to store info about current dips & swells
		public byte[] BufCurDipSwellData;

		public DateTime DateStartDipSwell = DateTime.MinValue;
		public DateTime DateEndDipSwell = DateTime.MinValue;
		// в этом массиве хранятся промежутки времени, за которые нужно считывать
		// архивы DNS. эти промежутки указаны юзером
		//public List<EmService.Pair<DateTime>> ListDnsPeriods;

		// Average values out info
		public DateTime DateStartAvg3sec = DateTime.MinValue;
		public DateTime DateEndAvg3sec = DateTime.MinValue;
		public DateTime DateStartAvg1min = DateTime.MinValue;
		public DateTime DateEndAvg1min = DateTime.MinValue;
		public DateTime DateStartAvg30min = DateTime.MinValue;
		public DateTime DateEndAvg30min = DateTime.MinValue;

		public bool AvgExists = false;

		#endregion

		#region Constructors

		public DeviceCommonInfoEm32()
		{
			for (int i = 0; i < EmService.CountPhases; ++i)
			{
				//PointersDip[i] = -1;
				//PointersSwell[i] = -1;
				CurPointersDip[i] = new DnsPointerData();
				CurPointersSwell[i] = new DnsPointerData();
			}
		}

		#endregion

		#region Methods

		public bool DnsExists()
		{
			//for (int i = 0; i < EmService.CountPhases; ++i)
			//{
			//    if (PointersDip[i] > 0 || PointersSwell[i] > 0 ||
			//        CurPointersDip[i].Pointer != -1 || CurPointersSwell[i].Pointer != -1)
			//        return true;
			//}
			if (DateStartDipSwell != DateTime.MinValue && DateEndDipSwell != DateTime.MinValue)
				return true;
			return false;
		}

		public bool CurDnsExists()
		{
			for (int i = 0; i < EmService.CountPhases; ++i)
			{
				if (CurPointersDip[i].Pointer != -1 || CurPointersSwell[i].Pointer != -1)
					return true;
			}
			return false;
		}

		public void ResetAllDns()
		{
			for (int i = 0; i < EmService.CountPhases; ++i)
			{
				//PointersDip[i] = -1; 
				//PointersSwell[i] = -1;
				CurPointersDip[i].Pointer = -1;
				CurPointersSwell[i].Pointer = -1;
			}
		}

		#endregion
	}

	///// <summary>
	///// Structure of one Dns record
	///// </summary>
	//public struct DnsSet32
	//{
	//    public DateTime DnsStart;
	//    public DateTime DnsEnd;
	//    //public ushort Year;
	//    //public byte Month;
	//    //public byte Day;
	//}

	/// <summary>
	/// Structure of one PQP record
	/// </summary>
	public struct PqpSetEm32
	{
		public DateTime PqpStart;
		public DateTime PqpEnd;
		public ushort Year;
		public byte Month;
		public byte Day;
	}
}