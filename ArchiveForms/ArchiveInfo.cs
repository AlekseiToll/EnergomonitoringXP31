using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;
using System.Threading;
using EmServiceLib;
using DbServiceLib;
using EmDataSaver;

namespace EnergomonitoringXP
{
	class ArchiveInfo
	{
		private Settings settings_;
		private int curPgServerIndex_;
		private string parentFolderName_;	// = object name
		private EmDeviceType devType_;
		private ConnectScheme connectScheme_;
		private float uNomLinear_;
		private float uNomPhase_;
		private float fNom_;
		private DateTime start_;
		private DateTime end_;
		private AvgTypes avgType_;
		private AvgTypes_PQP_A avgType_PQP_A_;
		private Int64 object_id_;
		private bool currentWithTR_;
		//private string objectName_;
		private string devVersion_;
		private short t_fliker_;
		private short constraintType_;
		private float iLimit_;
		private float uLimit_;
		private DateTime mlStartDateTime1_;
		private DateTime mlEndDateTime1_;
		private DateTime mlStartDateTime2_;
		private DateTime mlEndDateTime2_;
		//private bool marked_on_off_;			// for EtPQP-A only

		public ArchiveInfo(ref Settings settins)
		{
			settings_ = settins;
		}

		/// <summary>
		/// Common information (NOT for EtPQP-A)
		/// </summary>
		public void SetCommonInfo(
			int curPgServerIndex,
			string parentFolderName,
			DateTime start, DateTime end,
			ConnectScheme connectScheme,
			float uNomLinear, float uNomPhase, float fNom,
			AvgTypes avgType,
			Int64 object_id,
			bool currentWithTR,
			EmDeviceType devType,
			string devVersion, short t_fliker, short constraintType,
			DateTime mlStartDateTime1, DateTime mlEndDateTime1,
			DateTime mlStartDateTime2, DateTime mlEndDateTime2)
		{

			curPgServerIndex_ = curPgServerIndex;
			parentFolderName_ = parentFolderName;
			start_ = start;
			end_ = end;
			connectScheme_ = connectScheme;
			uNomLinear_ = uNomLinear;
			uNomPhase_ = uNomPhase;
			fNom_ = fNom;
			avgType_ = avgType;
			object_id_ = object_id;
			currentWithTR_ = currentWithTR;
			devType_ = devType;
			devVersion_ = devVersion;
			t_fliker_ = t_fliker;
			constraintType_ = constraintType;
			mlStartDateTime1_ = mlStartDateTime1;
			mlEndDateTime1_ = mlEndDateTime1;
			mlStartDateTime2_ = mlStartDateTime2;
			mlEndDateTime2_ = mlEndDateTime2;
		}

		/// <summary>
		/// Common information (for EtPQP-A)
		/// </summary>
		public void SetCommonInfo(
			int curPgServerIndex,
			string parentFolderName,
			DateTime start, DateTime end,
			ConnectScheme connectScheme,
			float uNomLinear, float uNomPhase, float fNom,
			AvgTypes_PQP_A avgType,
			Int64 object_id,
			string devVersion, short t_fliker, 
			short constraintType, float iLimit, float uLimit)
		{
			curPgServerIndex_ = curPgServerIndex;
			parentFolderName_ = parentFolderName;
			start_ = start;
			end_ = end;
			connectScheme_ = connectScheme;
			uNomLinear_ = uNomLinear;
			uNomPhase_ = uNomPhase;
			fNom_ = fNom;
			avgType_PQP_A_ = avgType;
			object_id_ = object_id;
			devType_ = EmDeviceType.ETPQP_A;
			devVersion_ = devVersion;
			t_fliker_ = t_fliker;
			constraintType_ = constraintType;
			iLimit_ = iLimit;
			uLimit_ = uLimit;
		}

		public string GetArchiveInfo()
		{
			try
			{
				ResourceManager rm = new ResourceManager("EnergomonitoringXP.emstrings", this.GetType().Assembly);
				string strCurrentConnectionScheme = rm.GetString("name_con_scheme_" +
												connectScheme_.ToString() + "_short");
				//string strAvgTime;
				//if (devType_ != EmDeviceType.ETPQP_A) 
				//strAvgTime = rm.GetString(EmService.GetAvgTypeStringForRM(avgType_));
				//else strAvgTime = rm.GetString(EmService.GetAvgTypeStringForRM(avgType_PQP_A_));

				DbService dbService = new DbService(
					AVGDataGridWrapperBase.GetPgConnectionString(devType_, curPgServerIndex_, ref settings_));
				if (!dbService.Open())
				{
					MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					return string.Empty;
				}

				string CtrValue = string.Empty;
				string VtrValue = string.Empty;

				if (devType_ == EmDeviceType.ETPQP_A)
				{
					#region EtPQP-A

					//bool u_transformer_enable = false;
					//short i_transformer_usage = 0;
					//short u_transformer_type = 0;
					//int i_transformer_primary = 1;
					//int i_transformer_secondary = 1;
					float gps_latitude = 0;
					float gps_longitude = 0;
					float u_limit = 0;
					float i_limit = 0;
					bool marked_on_off = true;

					try
					{
						dbService.ExecuteReader("SELECT gps_latitude, gps_longitude, u_transformer_enable, u_transformer_type, i_sensor_type, i_transformer_usage, i_transformer_primary, u_limit, i_limit, marked_on_off FROM registrations WHERE reg_id = " + object_id_.ToString() + ";");

						while (dbService.DataReaderRead())
						{
							object tmpObj = dbService.DataReaderData("gps_latitude");
							if (tmpObj is DBNull) gps_latitude = 0;
							else gps_latitude = Conversions.object_2_float(tmpObj);

							tmpObj = dbService.DataReaderData("gps_longitude");
							if (tmpObj is DBNull) gps_longitude = 0;
							else gps_longitude = Conversions.object_2_float(tmpObj);

							tmpObj = dbService.DataReaderData("u_limit");
							if (tmpObj is DBNull) u_limit = 0;
							else u_limit = Conversions.object_2_float(tmpObj);

							tmpObj = dbService.DataReaderData("i_limit");
							if (tmpObj is DBNull) i_limit = 0;
							else i_limit = Conversions.object_2_float(tmpObj);

							tmpObj = dbService.DataReaderData("marked_on_off");
							if (tmpObj is DBNull) marked_on_off = false;
							else marked_on_off = (bool)tmpObj;
							//marked_on_off_ = marked_on_off;

							//object tmpObj = dbService.DataReaderData("u_transformer_enable");
							//if (tmpObj is DBNull) u_transformer_enable = false;
							//else u_transformer_enable = (bool)tmpObj;

							//tmpObj = dbService.DataReaderData("i_transformer_usage");
							//if (tmpObj is DBNull) i_transformer_usage = 0;
							//else i_transformer_usage = (short)tmpObj;

							//tmpObj = dbService.DataReaderData("u_transformer_type");
							//if (tmpObj is DBNull) u_transformer_type = 0;
							//else u_transformer_type = (short)tmpObj;

							//tmpObj = dbService.DataReaderData("i_transformer_primary");
							//if (tmpObj is DBNull) i_transformer_primary = 1;
							//else i_transformer_primary = (int)tmpObj;
						}
					}
					catch (Exception tmpEx)
					{
						MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
						EmService.DumpException(tmpEx, "Error in GetArchiveInfo() EtPQP-A Read:");
						return string.Empty;
					}
					finally
					{
						if (dbService != null) dbService.CloseConnect();
					}

					//string strCtrValue = string.Empty;
					//string strVtrValue = string.Empty;

					//if (u_transformer_enable)
					//{
					//    strVtrValue = "1:" +
					//        (DeviceIO.EtPQP_A_Device.GetUTransformerMultiplier(u_transformer_type)).ToString();
					//}
					//else
					//    strVtrValue = rm.GetString("name_peakload_none");

					//if (i_transformer_usage == 1 || i_transformer_usage == 2)
					//{
					//    if (i_transformer_usage == 1)
					//        i_transformer_secondary = 1;
					//    else if (i_transformer_usage == 2)
					//        i_transformer_secondary = 5;

					//    strCtrValue = string.Format("{0}, {1}", i_transformer_primary, i_transformer_secondary);
					//}
					//else
					//    strCtrValue = rm.GetString("name_peakload_none");

					string strMarked;
					if (Thread.CurrentThread.CurrentCulture.ToString().Equals("ru-RU"))
					{
						if (marked_on_off) strMarked = "включен";
						else strMarked = "выключен";
					}
					else
					{
						if (marked_on_off) strMarked = "on";
						else strMarked = "off";
					}

					string res;
					if (connectScheme_ != ConnectScheme.Ph3W3 && connectScheme_ != ConnectScheme.Ph3W3_B_calc)
					{
						if (gps_latitude != 0 || gps_longitude != 0)
						{
							#region Letters for latitude and longitude

							string str_latitude = "˚ N";
							if (settings_.CurrentLanguage == "ru") str_latitude = "˚ с.ш.";
							string str_longitude = "˚ E";
							if (settings_.CurrentLanguage == "ru") str_longitude = "˚ в.д.";

							if (gps_latitude < 0)
							{
								str_latitude = "˚ S";
								if (settings_.CurrentLanguage == "ru") str_latitude = "˚ ю.ш.";
							}

							if (gps_longitude < 0)
							{
								str_longitude = "˚ W";
								if (settings_.CurrentLanguage == "ru") str_longitude = "˚ з.д.";
							}

							str_latitude = gps_latitude.ToString() + str_latitude;
							str_longitude = gps_longitude.ToString() + str_longitude;

							#endregion

							res = String.Format(
								rm.GetString("window_prefix_common_archive_etpqp_a_gps"),
								parentFolderName_,
								start_, end_,
								strCurrentConnectionScheme,
								uNomLinear_, uNomPhase_, fNom_, u_limit, i_limit,
								strMarked,
								str_latitude, str_longitude);
								//strAvgTime, strCtrValue, strVtrValue);
						}
						else
						{
							res = String.Format(
								rm.GetString("window_prefix_common_archive_etpqp_a"),
								parentFolderName_,
								start_, end_,
								strCurrentConnectionScheme,
								uNomLinear_, uNomPhase_, fNom_, u_limit, i_limit,
								strMarked);
								//strAvgTime, strCtrValue, strVtrValue);
						}
					}
					else
					{
						if (gps_latitude != 0 || gps_longitude != 0)
						{
							res = String.Format(
								rm.GetString("window_prefix_common_archive_3ph3w_etpqp_a_gps"),
								parentFolderName_,
								start_, end_,
								strCurrentConnectionScheme,
								uNomLinear_, fNom_, u_limit, i_limit,
								strMarked,
								gps_latitude, gps_longitude);
						}
						else
						{
							res = String.Format(
								rm.GetString("window_prefix_common_archive_3ph3w_etpqp_a"),
								parentFolderName_,
								start_, end_,
								strCurrentConnectionScheme,
								uNomLinear_, fNom_, u_limit, i_limit,
								strMarked);
						}
					}
					return res;

					#endregion
				}
				else
				{
					#region NOT EtPQP-A

					//try
					//{
						//string commandText;
						//switch (devType_)
						//{
						//    case EmDeviceType.EM33T:
						//    case EmDeviceType.EM33T1:
						//    case EmDeviceType.EM31K:
						//        commandText = "SELECT * FROM turn_ratios WHERE database_id = " +
						//                                            object_id_.ToString() + ";";
						//        break;
						//    case EmDeviceType.EM32:
						//        commandText = "SELECT * FROM turn_ratios WHERE device_id = " +
						//                                    object_id_.ToString() + ";";
						//        break;
						//    case EmDeviceType.ETPQP:
						//        commandText = "SELECT * FROM turn_ratios WHERE object_id = " +
						//                                    object_id_.ToString() + ";";
						//        break;
						//    default:
						//        EmService.WriteToLogFailed("Invalid devType in SetCommonCaption1");
						//        EmService.WriteToLogFailed(devType_.ToString());
						//        return string.Empty;
						//}

						//dbService.ExecuteReader(commandText);
						//while (dbService.DataReaderRead())
						//{
							//short iType = 0;
							//if (devType_ != EmDeviceType.ETPQP)
							//    iType = (short)dbService.DataReaderData("type");
							//else iType = (short)dbService.DataReaderData("turn_type");
							//float fValue1 = (float)dbService.DataReaderData("value1");
							//float fValue2 = (float)dbService.DataReaderData("value2");

							//switch (iType)
							//{
							//    case 1: // voltage
							//        VtrValue = (fValue1 / fValue2).ToString();
							//        break;

							//    case 2: // current
							//        CtrValue = (fValue1 / fValue2).ToString();
							//        break;
							//} // switch
						//} // while
					//} // try
					//catch
					//{
					//    MessageBoxes.DbConnectError(this, dbService.Host, dbService.Port, dbService.Database);
					//    return string.Empty;
					//}
					//finally
					//{
					//    if (dbService != null) dbService.CloseConnect();
					//}

					//string strCtrValue = string.Empty;
					//string strVtrValue = string.Empty;

					//bool vtr_or_ctr_exists = CtrValue != string.Empty || VtrValue != string.Empty;
					//strCtrValue = CtrValue != string.Empty ?
					//    CtrValue : rm.GetString("name_peakload_none");

					//strVtrValue = (VtrValue == string.Empty) ? rm.GetString("name_peakload_none")
					//    : VtrValue;

					//if (!currentWithTR_ && vtr_or_ctr_exists) strVtrValue += rm.GetString("name_peakload_notapplied");

					string res = String.Format(
						rm.GetString("window_prefix_common_archive"),
						parentFolderName_,
						start_, end_,
						strCurrentConnectionScheme,
						uNomLinear_, uNomPhase_, fNom_);
						//strAvgTime, strCtrValue, strVtrValue);
					return res;

					#endregion
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetArchiveInfo():");
				throw;
			}
		}

		public ConnectScheme ConnectScheme
		{
			get { return connectScheme_; }
			set { connectScheme_ = value; }
		}

		public string ObjectName
		{
			get { return parentFolderName_; }
			//set { objectName_ = value; }
		}

		public int CurPgServerIndex
		{
			get { return curPgServerIndex_; }
			set { curPgServerIndex_ = value; }
		}

		public bool CurrentWithTR
		{
			get { return currentWithTR_; }
			set { currentWithTR_ = value; }
		}

		public float UNomPhase
		{
			get { return uNomPhase_; }
			set { uNomPhase_ = value; }
		}

		public float UNomLinear
		{
			get { return uNomLinear_; }
			set { uNomLinear_ = value; }
		}

		public EmDeviceType DevType
		{
			get { return devType_; }
			set { devType_ = value; }
		}

		public Int64 ObjectId
		{
			get { return object_id_; }
			set { object_id_ = value; }
		}

		public short T_fliker
		{
			get { return t_fliker_; }
			set { t_fliker_ = value; }
		}

		public short ConstraintType
		{
			get { return constraintType_; }
			set { constraintType_ = value; }
		}

		public string DevVersion
		{
			get { return devVersion_; }
			set { devVersion_ = value; }
		}

		public float ULimit
		{
			get { return uLimit_; }
			set { uLimit_ = value; }
		}

		public float ILimit
		{
			get { return iLimit_; }
			set { iLimit_ = value; }
		}

		public DateTime MlStartDateTime1
		{
			get { return mlStartDateTime1_; }
			set { mlStartDateTime1_ = value; }
		}

		public DateTime MlEndDateTime1
		{
			get { return mlEndDateTime1_; }
			set { mlEndDateTime1_ = value; }
		}

		public DateTime MlStartDateTime2
		{
			get { return mlStartDateTime2_; }
			set { mlStartDateTime2_ = value; }
		}

		public DateTime MlEndDateTime2
		{
			get { return mlEndDateTime2_; }
			set { mlEndDateTime2_ = value; }
		}
	}
}
