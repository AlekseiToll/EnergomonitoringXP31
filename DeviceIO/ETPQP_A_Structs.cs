using System.Xml.Serialization;
using System;
using System.Collections.Generic;

using EmServiceLib;
using DeviceIO.Constraints;

namespace DeviceIO
{
	/// <summary>
	/// Class to store Main Records
	/// </summary>
	public class ContentsLineEtPQP_A_Storage
	{
		#region Fields

		List<ContentsLineEtPQP_A> listRecords_ = new List<ContentsLineEtPQP_A>();

		#endregion

		#region Public Methods

		public bool AddRecord(ContentsLineEtPQP_A rec)
		{
			try
			{
				for (int iRec = 0; iRec < listRecords_.Count; ++iRec)
				{
					if (listRecords_[iRec].RegistrationId == rec.RegistrationId)
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

		public ContentsLineEtPQP_A FindRecord(int id)
		{
			try
			{
				for (int iRec = 0; iRec < listRecords_.Count; ++iRec)
				{
					if (listRecords_[iRec].RegistrationId == id)
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

		#endregion

		#region Properties

		public int Count
		{
			get { return listRecords_.Count; }
		}

		public ContentsLineEtPQP_A this[int index]
		{
			get
			{
				if (index >= 0 && index < listRecords_.Count)
					return listRecords_[index];
				else
				{
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
	public class ContentsLineEtPQP_A
	{
		#region Fields

		public long SerialNumber;
		public string DevVersion;
		public DateTime DevVersionDate;
		public string ObjectName;
		public ConnectScheme ConnectionScheme;
		public DateTime CommonBegin;
		public DateTime CommonEnd;

		public UInt32 RegistrationId;
		public short ConstraintType;
		public float[] Constraints = new float[EtPQPAConstraints.CntConstraints];

		public SystemInfoEtPQP_A SysInfo;

		// PQP out info
		public List<PqpSetEtPQP_A> PqpSet = new List<PqpSetEtPQP_A>();

		public ushort TimeZone;

        // Average values out info
		// в этих массивах хран€тс€ данные по трем типам архивов усредненных (3 сек, 10 мин, 2 часа)
		// в 1-ом 3 стартовых, во 2-ом 3 конечных
		public AVGDataEtPQP_A[] AvgDataStart;
		public AVGDataEtPQP_A[] AvgDataEnd;
		public bool AvgExists = false;

		#endregion

		#region Constructor

		public ContentsLineEtPQP_A()
		{
			AvgDataStart = new AVGDataEtPQP_A[3];
			AvgDataEnd = new AVGDataEtPQP_A[3];
			for (int i = 0; i < 3; ++i)
			{
				AvgDataStart[i] = new AVGDataEtPQP_A(0, DateTime.MinValue, DateTime.MinValue);
				AvgDataEnd[i] = new AVGDataEtPQP_A(0, DateTime.MinValue, DateTime.MinValue);
			}

			SysInfo = new SystemInfoEtPQP_A();
		}

		#endregion

		#region Methods

		public UInt32 GetAVGStartIndexByType(AvgTypes_PQP_A type)
		{
			switch (type)
			{
				case AvgTypes_PQP_A.ThreeSec: //return AvgIndexStart3sec;
					return AvgDataStart[(int)AvgTypes_PQP_A.ThreeSec - 1].Index;
				case AvgTypes_PQP_A.TenMin: //return AvgIndexStart10min;
					return AvgDataStart[(int)AvgTypes_PQP_A.TenMin - 1].Index;
				case AvgTypes_PQP_A.TwoHours: //return AvgIndexStart2hour;
					return AvgDataStart[(int)AvgTypes_PQP_A.TwoHours - 1].Index;
			}
			return 0;
		}

		public UInt32 GetAVGEndIndexByType(AvgTypes_PQP_A type)
		{
			switch (type)
			{
				case AvgTypes_PQP_A.ThreeSec: return AvgDataEnd[(int)AvgTypes_PQP_A.ThreeSec - 1].Index;
				case AvgTypes_PQP_A.TenMin: return AvgDataEnd[(int)AvgTypes_PQP_A.TenMin - 1].Index;
				case AvgTypes_PQP_A.TwoHours: return AvgDataEnd[(int)AvgTypes_PQP_A.TwoHours - 1].Index;
			}
			return 0;
		}

		#endregion
	}

	public class AVGDataEtPQP_A
	{
		public UInt32 Index;
		public DateTime dtStart = DateTime.MinValue;
		public DateTime dtEnd = DateTime.MinValue;

		public AVGDataEtPQP_A(UInt32 index, DateTime start, DateTime end)
		{
			Index = index; dtStart = start; dtEnd = end;
		}
	}

	/// <summary>
	/// Structure of one PQP record
	/// </summary>
	public struct PqpSetEtPQP_A
	{
		public UInt32 PqpIndex;
		public DateTime PqpStart;
		public DateTime PqpEnd;
		public UInt32 RegistrationId;
		public ConnectScheme ConnectionScheme;
		public float F_Nominal;
		public float U_NominalLinear;
		public float U_NominalPhase;
		public float I_NominalPhase;
		public short ConstraintType;

		public PqpSetEtPQP_A(UInt32 index, UInt32 regId)
		{
			PqpIndex = index;
			RegistrationId = regId;

			PqpStart = DateTime.MinValue;
			PqpEnd = DateTime.MinValue;
			ConnectionScheme = ConnectScheme.Unknown;
			F_Nominal = 0;
			U_NominalLinear = 0;
			U_NominalPhase = 0;
			I_NominalPhase = 0;
			ConstraintType = 0;
		}
	}

	/// <summary>
	/// Stricture of common device information for EM33 and EM31
	/// </summary>
	public class DeviceCommonInfoEtPQP_A
	{
		public long SerialNumber = -1;
		public string DevVersion;
		public DateTime DevVersionDate;

		public ContentsLineEtPQP_A_Storage Content;

		public DeviceCommonInfoEtPQP_A()
		{
			Content = new ContentsLineEtPQP_A_Storage();
		}
	}

	public class SystemInfoEtPQP_A
	{
		private float f_Nominal_;
		private float u_NominalLinear_;
		private float u_NominalPhase_;
		private float i_NominalPhase_;
		private float u_Limit_;
		private float i_Limit_;
		private int f_Limit_;
		
		private bool u_transformer_enable_;
		private short u_transformer_type_;
		private int u_consistent_;					// —огласованное напр€жение
		private short i_sensor_type_;
		private short i_transformer_usage_;			// “рансформатор тока Ц использование
		private short i_transformer_primary_;		// “рансформатор тока Ц первичный ток
		private short i_transformerSecondary_;		// “рансформатор тока Ц вторичный ток
		private bool synchro_zero_enable_;
		private bool autocorrect_time_gps_enable_;
		private short electro_system_;
		private short pqp_length_;
		private short start_mode_;
		private int registration_stop_cnt_;
		private int pqp_cnt_;
		private int cnt_dip_;
		private int cnt_swell_;
		private int cnt_interrupt_;
		private short t_fliker_;
		private bool marked_on_off_;				// ”чет маркированных данных

		private double gps_latitude_;
		private double gps_longitude_;

		#region Properties

		[XmlAttribute]
		public float F_Nominal
		{
			get { return f_Nominal_; }
			set { f_Nominal_ = value; }
		}

		[XmlAttribute]
		public float U_NominalLinear
		{
			get { return u_NominalLinear_; }
			set { u_NominalLinear_ = value; }
		}

		[XmlAttribute]
		public float U_NominalPhase
		{
			get { return u_NominalPhase_; }
			set { u_NominalPhase_ = value; }
		}

		[XmlAttribute]
		public float I_NominalPhase
		{
			get { return i_NominalPhase_; }
			set { i_NominalPhase_ = value; }
		}

		[XmlAttribute]
		public float U_Limit
		{
			get { return u_Limit_; }
			set { u_Limit_ = value; }
		}

		[XmlAttribute]
		public float I_Limit
		{
			get { return i_Limit_; }
			set { i_Limit_ = value; }
		}

		[XmlAttribute]
		public int F_Limit
		{
			get { return f_Limit_; }
			set { f_Limit_ = value; }
		}

		[XmlAttribute]
		public bool U_transformer_enable
		{
			get { return u_transformer_enable_; }
			set { u_transformer_enable_ = value; }
		}

		[XmlAttribute]
		public short U_transformer_type
		{
			get { return u_transformer_type_; }
			set { u_transformer_type_ = value; }
		}

		[XmlAttribute]
		public int U_consistent
		{
			get { return u_consistent_; }
			set { u_consistent_ = value; }
		}

		[XmlAttribute]
		public short I_sensor_type
		{
			get { return i_sensor_type_; }
			set { i_sensor_type_ = value; }
		}

		[XmlAttribute]
		public short I_transformer_usage
		{
			get { return i_transformer_usage_; }
			set { i_transformer_usage_ = value; }
		}

		[XmlAttribute]
		public short I_transformer_primary
		{
			get { return i_transformer_primary_; }
			set { i_transformer_primary_ = value; }
		}

		[XmlAttribute]
		public bool Synchro_zero_enable
		{
			get { return synchro_zero_enable_; }
			set { synchro_zero_enable_ = value; }
		}

		[XmlAttribute]
		public bool Autocorrect_time_gps_enable
		{
			get { return autocorrect_time_gps_enable_; }
			set { autocorrect_time_gps_enable_ = value; }
		}

		[XmlAttribute]
		public short Electro_system
		{
			get { return electro_system_; }
			set { electro_system_ = value; }
		}

		[XmlAttribute]
		public short Pqp_length
		{
			get { return pqp_length_; }
			set { pqp_length_ = value; }
		}

		[XmlAttribute]
		public short Start_mode
		{
			get { return start_mode_; }
			set { start_mode_ = value; }
		}

		[XmlAttribute]
		public int Registration_stop_cnt
		{
			get { return registration_stop_cnt_; }
			set { registration_stop_cnt_ = value; }
		}

		[XmlAttribute]
		public int Pqp_cnt
		{
			get { return pqp_cnt_; }
			set { pqp_cnt_ = value; }
		}

		[XmlAttribute]
		public int Cnt_dip
		{
			get { return cnt_dip_; }
			set { cnt_dip_ = value; }
		}

		[XmlAttribute]
		public int Cnt_swell
		{
			get { return cnt_swell_; }
			set { cnt_swell_ = value; }
		}

		[XmlAttribute]
		public int Cnt_interrupt
		{
			get { return cnt_interrupt_; }
			set { cnt_interrupt_ = value; }
		}

		[XmlAttribute]
		public short I_TransformerSecondary
		{
			get { return i_transformerSecondary_; }
			set { i_transformerSecondary_ = value; }
		}

		[XmlAttribute]
		public short T_fliker
		{
			get { return t_fliker_; }
			set { t_fliker_ = value; }
		}

		[XmlAttribute]
		public double Gps_Latitude
		{
			get { return gps_latitude_; }
			set { gps_latitude_ = value; }
		}

		[XmlAttribute]
		public double Gps_Longitude
		{
			get { return gps_longitude_; }
			set { gps_longitude_ = value; }
		}

		[XmlAttribute]
		public bool Marked_on_off
		{
			get { return marked_on_off_; }
			set { marked_on_off_ = value; }
		}

		#endregion

		public SystemInfoEtPQP_A Clone()
		{
			SystemInfoEtPQP_A obj = new SystemInfoEtPQP_A();
			obj.autocorrect_time_gps_enable_ = this.autocorrect_time_gps_enable_;
			obj.cnt_dip_ = this.cnt_dip_;
			obj.cnt_interrupt_ = this.cnt_interrupt_;
			obj.cnt_swell_ = this.cnt_swell_;
			obj.electro_system_ = this.electro_system_;
			obj.f_Limit_ = this.f_Limit_;
			obj.f_Nominal_ = this.f_Nominal_;
			obj.i_Limit_ = this.i_Limit_;
			obj.i_NominalPhase_ = this.i_NominalPhase_;
			obj.i_sensor_type_ = this.i_sensor_type_;
			obj.i_transformer_primary_ = this.i_transformer_primary_;
			obj.i_transformer_usage_ = this.i_transformer_usage_;
			obj.i_transformerSecondary_ = this.i_transformerSecondary_;
			obj.pqp_cnt_ = this.pqp_cnt_;
			obj.pqp_length_ = this.pqp_length_;
			obj.registration_stop_cnt_ = this.registration_stop_cnt_;
			obj.start_mode_ = this.start_mode_;
			obj.synchro_zero_enable_ = this.synchro_zero_enable_;
			obj.t_fliker_ = this.t_fliker_;
			obj.u_consistent_ = this.u_consistent_;
			obj.u_Limit_ = this.u_Limit_;
			obj.u_NominalLinear_ = this.u_NominalLinear_;
			obj.u_NominalPhase_ = this.u_NominalPhase_;
			obj.u_transformer_enable_ = this.u_transformer_enable_;
			obj.u_transformer_type_ = this.u_transformer_type_;
			obj.gps_latitude_ = this.gps_latitude_;
			obj.gps_longitude_ = this.gps_longitude_;
			obj.marked_on_off_ = this.marked_on_off_;
			return obj;
		}
	}
}