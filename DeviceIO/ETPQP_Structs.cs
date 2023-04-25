// (c) Mars-Energo Ltd.

using System;
using System.Collections.Generic;

using EmServiceLib;

namespace DeviceIO
{
	/// <summary>
	/// Class to store Main Records
	/// </summary>
	public class ContentsLineEtPQPStorage
	{
		#region Fields

		List<ContentsLineEtPQP> listRecords_ = new List<ContentsLineEtPQP>();

		#endregion

		#region Public Methods

		public bool AddRecord(ContentsLineEtPQP rec)
		{
			try
			{
				for (int iRec = 0; iRec < listRecords_.Count; ++iRec)
				{
					if (listRecords_[iRec].GlobalObjectId == rec.GlobalObjectId)
						return false;
				}
				listRecords_.Add(rec);
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ContentsLineEtPQPStorage.AddRecord()");
				return false;
			}
		}

		public void DeleteAll()
		{
			listRecords_.Clear();
		}

		public ContentsLineEtPQP FindRecord(int id)
		{
			try
			{
				for (int iRec = 0; iRec < listRecords_.Count; ++iRec)
				{
					if (listRecords_[iRec].GlobalObjectId == id)
						return listRecords_[iRec];
				}
				return null;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ContentsLineEtPQPStorage.FindRecord()" + ex.Message);
				return null;
			}
		}

		//public bool AddPqpArchive(PqpSetEtPQP pqp) 
		//{
		//    try
		//    {
		//        ContentsLineEtPQP rec = FindRecord(pqp.ObjectId);
		//        if (rec == null) return false;

		//        if (rec.PqpSet == null)
		//            rec.PqpSet = new List<PqpSetEtPQP>();

		//        rec.PqpSet.Add(pqp);
		//        rec.PqpExists = true;
		//        return true;
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.WriteToLogFailed(
		//            "Error in ContentsLineEtPQPStorage.AddPqpArchive()" + ex.Message);
		//        return false;
		//    }
		//}

		#endregion

		#region Properties

		public int Count
		{
			get { return listRecords_.Count; }
		}

		public ContentsLineEtPQP this[int index]
		{
			get {
				if(index >= 0 && index < listRecords_.Count)
					return listRecords_[index];
				else {
					EmService.WriteToLogFailed("Invalid Main Record index!");
					return null;
				}
			}
		}

		#endregion
	}

	/// <summary>
	/// Structure of any one Main Record with real existing data
	/// </summary>
	[Serializable]
	public class ContentsLineEtPQP : BaseDeviceCommonInfo
	{
		#region Fields

		public Int32 GlobalObjectId;
		public short ConstraintType;

		// PQP out info
		public bool PqpExists = false;
		public List<PqpSetEtPQP> PqpSet = new List<PqpSetEtPQP>();
		// fliker
		public short t_fliker = 10;

		// Dips & Overs out info
		//public bool DnsExists = false;
		public DateTime DateStartDipSwell = DateTime.MinValue;
		public DateTime DateEndDipSwell = DateTime.MinValue;

		public DnsPointerData[] CurPointersDip = new DnsPointerData[EmService.CountPhases];
		public DnsPointerData[] CurPointersSwell = new DnsPointerData[EmService.CountPhases];

		// buffer to store info about current dips & swells
		public byte[] BufCurDipSwellData;

		// Average values out info
		public DateTime DateStartAvg3sec = DateTime.MinValue;
		public DateTime DateEndAvg3sec = DateTime.MinValue;
		public DateTime DateStartAvg1min = DateTime.MinValue;
		public DateTime DateEndAvg1min = DateTime.MinValue;
		public DateTime DateStartAvg30min = DateTime.MinValue;
		public DateTime DateEndAvg30min = DateTime.MinValue;

		public bool AvgExists = false;

		#endregion

		#region Constructor

		public ContentsLineEtPQP()
		{
			for (int i = 0; i < EmService.CountPhases; ++i)
			{
				CurPointersDip[i] = new DnsPointerData();
				CurPointersSwell[i] = new DnsPointerData();
			}
		}

		#endregion

		#region Methods

		public bool CurDnsExists()
		{
			for (int i = 0; i < EmService.CountPhases; ++i)
			{
				if (CurPointersDip[i].Pointer != -1 || CurPointersSwell[i].Pointer != -1)
					return true;
			}
			return false;
		}

		public bool DnsExists()
		{
			// функция считывания данных из прибора написана так, что здесь будет MaxValue,
			// если нет данных по DNS для этого объекта
			return (DateStartDipSwell != DateTime.MaxValue && DateStartDipSwell != DateTime.MinValue);
		}

		public void ResetCurDns()
		{
			for (int i = 0; i < EmService.CountPhases; ++i)
			{
				CurPointersDip[i].Pointer = -1;
				CurPointersSwell[i].Pointer = -1;
			}
			BufCurDipSwellData = null;
		}

		public void ResetAllObjectDns()
		{
			for (int i = 0; i < EmService.CountPhases; ++i)
			{
				//PointersDip[i] = -1;
				//PointersSwell[i] = -1;
				CurPointersDip[i].Pointer = -1;
				CurPointersSwell[i].Pointer = -1;
			}
			DateStartDipSwell = DateTime.MaxValue;
		}

		#endregion
	}

	/// <summary>
	/// Structure of one PQP record
	/// </summary>
	public struct PqpSetEtPQP
	{
		public DateTime PqpStart;
		public DateTime PqpEnd;
		//public ushort Year;
		//public byte Month;
		//public byte Day;
		//public byte Hour;
		//public byte Minute;
		//public byte Second;
		public Int32 ObjectId;
		public ConnectScheme ConnectionScheme;
		public float F_Nominal;
		public float U_NominalLinear;
		public float U_NominalPhase;
		public float I_NominalPhase;
		public short ConstraintType;

		public TimeSpan MlStartTime1;
		public TimeSpan MlEndTime1;
		public TimeSpan MlStartTime2;
		public TimeSpan MlEndTime2;
	}

	/// <summary>
	/// Stricture of common device information for ETPQP
	/// </summary>
	public class DeviceCommonInfoEtPQP
	{
		public long SerialNumber = -1;
		public string DevVersion;

		// id объекта, которму принадлежат текущие DNS
		public int GlobalIdObjectOfCurDNS = -1;
		//public Int64[] PointersDip = new Int64[EmService.CountPhases];
		//public Int64[] PointersSwell = new Int64[EmService.CountPhases];

		// пределы по току
		public string SD_CurrentRangeName_1;
		public string SD_CurrentRangeName_2;
		public string SD_CurrentRangeName_3;
		public string SD_CurrentRangeName_4;
		public string SD_CurrentRangeName_5;
		public string SD_CurrentRangeName_6;

		public ContentsLineEtPQPStorage Content;

		public DeviceCommonInfoEtPQP()
		{
			Content = new ContentsLineEtPQPStorage();
		}

		public void ResetAllDeviceDns()
		{
			//for (int i = 0; i < EmService.CountPhases; ++i)
			//{
			//    PointersDip[i] = -1;
			//    PointersSwell[i] = -1;
			//}

			for (int iObj = 0; iObj < Content.Count; ++iObj)
			{
				Content[iObj].DateStartDipSwell = DateTime.MaxValue;
				for (int i = 0; i < EmService.CountPhases; ++i)
				{
					Content[iObj].CurPointersDip[i].Pointer = -1;
					Content[iObj].CurPointersSwell[i].Pointer = -1;
				}
			}
		}
	}
}