using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Resources;
using System.Windows.Forms;

using EmServiceLib;

namespace DeviceIO
{
	public class EtPQPDevice : EmDevice
	{
		#region Constants and enums

		// максимальное число записей в архиве DnS
		const int cntRecordsDnS_ = 16384;
		// максимальное число параметров при запросе архива усреденных
		const int maxNumAvgFieldsQuery_ = 2048;

		private const int avgArchiveLength_ = 4096;

		private const int COUNT_OBJ_NAMES = 8;

		#endregion

		#region Fields

		DeviceCommonInfoEtPQP devInfo_ = new DeviceCommonInfoEtPQP();

		static ushort id_avg_query_ = 1;

		#endregion

		#region Properties

		public DeviceCommonInfoEtPQP DeviceInfo
		{
			get { return this.devInfo_; }
		}

		#endregion

		#region Constructors

		public EtPQPDevice(EmPortType portType, ushort devAddr, bool auto, object[] port_params,
			IntPtr hMainWnd)
			: base(EmDeviceType.ETPQP, portType, devAddr, auto, port_params, hMainWnd)
		{
			byte[] temp_pswd = { 0x08, 0x01, 0x02, 0x03, 0x02, 0x07, 0x02, 0x01, 0x01, 0x01 };
			pswd_for_writing_ = new byte[temp_pswd.Length];
			temp_pswd.CopyTo(pswd_for_writing_, 0);
		}

		#endregion

		#region Public Methods

		public override int OpenDevice()
		{
			try
			{
				if (portManager_ != null) portManager_.ClosePort(true);

				portManager_ = new PortManager(hMainWnd_, portType_, devType_, ref portParams_, bAutoMode_);

				if (!portManager_.CreatePort()) return -1;

				if (!portManager_.OpenPort()) return -1;

				// для работы с EtPQP сначала на всякий случай посылаем команду - сброс обработки
				// запроса усредненных. эти запросы долго обрабатываются и могут приходить пакеты
				// от прошлых запросов
				ResetAllAvgQuery();

				long ser;
				if (!ReadDeviceSerialNumber(out ser))
				{
					portManager_.ClosePort(true);
					return -1;
				}
				Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 2, 0, 0);
				return (int)ser;
			}
			catch (EmDisconnectException)
			{
				portManager_.ClosePort(true);
				return -1;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in Open EtPQP device:");
				throw;
			}
		}

		public override bool IsSomeArchiveExist()
		{
			return (devInfo_.Content != null && devInfo_.Content.Count > 0);
		}

		public override ExchangeResult ReadDeviceInfo()
		{
			byte[] buffer = null;

			// creating DeviceCommonInfo object
			devInfo_ = new DeviceCommonInfoEtPQP();
			devInfo_.SerialNumber = serialNumber_;
			devInfo_.DevVersion = "0.0";
			int iNumOfPkeRecords = 0;

			try
			{
				// objects info
				if (Read(EmCommands.COMMAND_ReadObjectsEntrys, ref buffer, null) != 0)
					return ExchangeResult.Other_Error;
				int objInfoLen = 64;
				if (buffer == null || buffer.Length < objInfoLen)
				{
					throw new EmDeviceEmptyException();
				}
				// parse buffer

				#region objects

				int objCount = buffer.Length / objInfoLen;
				ContentsLineEtPQP[] mainRecords = new ContentsLineEtPQP[objCount];
				for (int iObj = 0; iObj < objCount; ++iObj)
				{
					mainRecords[iObj] = new ContentsLineEtPQP();

					// id
					mainRecords[iObj].GlobalObjectId =
						Conversions.bytes_2_ushort(ref buffer, 0 + iObj * objInfoLen);

					// object name
					mainRecords[iObj].ObjectName =
						Conversions.bytes_2_string(ref buffer, iObj * objInfoLen + 2, 16);
					if (mainRecords[iObj].ObjectName == "")
						mainRecords[iObj].ObjectName = "default object";

					// connection scheme
					ushort conScheme = Conversions.bytes_2_ushort(ref buffer, 18 + iObj * objInfoLen);
					switch (conScheme)
					{
						case 1: mainRecords[iObj].ConnectionScheme = ConnectScheme.Ph1W2; break;
						case 2: mainRecords[iObj].ConnectionScheme = ConnectScheme.Ph3W3; break;
						case 3: mainRecords[iObj].ConnectionScheme = ConnectScheme.Ph3W4; break;
						case 4: mainRecords[iObj].ConnectionScheme = ConnectScheme.Ph3W3_B_calc; break;
						case 5: mainRecords[iObj].ConnectionScheme = ConnectScheme.Ph3W4_B_calc; break;
						default: mainRecords[iObj].ConnectionScheme = ConnectScheme.Unknown; break;
					}

					// nominal f
					mainRecords[iObj].F_Nominal =
						Conversions.bytes_2_ushort(ref buffer, 20 + iObj * objInfoLen);

					// nominal U lin
					mainRecords[iObj].U_NominalLinear =
						Conversions.bytes_2_float2wIEEE754_old(ref buffer, 22 + iObj * objInfoLen, true);

					// nominal U ph
					mainRecords[iObj].U_NominalPhase =
						Conversions.bytes_2_float2wIEEE754_old(ref buffer, 26 + iObj * objInfoLen, true);

					// ml_start_time1
					mainRecords[iObj].MlStartTime1 =
						Conversions.bytes_2_TimeSpanHhMm(ref buffer, 30 + iObj * objInfoLen);

					// ml_end_time1
					mainRecords[iObj].MlEndTime1 =
						Conversions.bytes_2_TimeSpanHhMm(ref buffer, 32 + iObj * objInfoLen);

					// ml_start_time2
					mainRecords[iObj].MlStartTime2 =
						Conversions.bytes_2_TimeSpanHhMm(ref buffer, 34 + iObj * objInfoLen);

					// ml_end_time2
					mainRecords[iObj].MlEndTime2 =
						Conversions.bytes_2_TimeSpanHhMm(ref buffer, 36 + iObj * objInfoLen);

					// constraint_type
					mainRecords[iObj].ConstraintType =
						Conversions.bytes_2_short(ref buffer, 38 + iObj * objInfoLen);
					mainRecords[iObj].ConstraintType -= 1;

					// begin
					mainRecords[iObj].CommonBegin = Conversions.bytes_2_DateTimeSLIP(ref buffer, 40 + iObj * objInfoLen,
						"Start Main date in ReadDeviceInfo()");

					// end
					mainRecords[iObj].CommonEnd = Conversions.bytes_2_DateTimeSLIP(ref buffer, 52 + iObj * objInfoLen,
						"End Main date in ReadDeviceInfo()");
					if (mainRecords[iObj].CommonEnd == DateTime.MinValue)
						continue;

					devInfo_.Content.AddRecord(mainRecords[iObj]);
				}

				#endregion

				// device version
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)45 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 8)
				{
					EmService.WriteToLogFailed("Unable to read Device version!");
				}
				else
				{
					ushort hardwareVer = Conversions.bytes_2_ushort(ref buffer, 0);
					ushort programVer = Conversions.bytes_2_ushort(ref buffer, 2);
					uint buildDate = Conversions.bytes_2_uint_new(ref buffer, 4);
					string buildDateStr = buildDate.ToString();
					if (buildDateStr.Length < 6) buildDateStr = buildDateStr.Insert(0, "0");
					devInfo_.DevVersion = hardwareVer.ToString() + "." + programVer.ToString() + "." +
						buildDate.ToString();
					DateTime dtBuildDate = new DateTime(
						Int32.Parse(buildDateStr.Substring(4, 2)) + 2000,
						Int32.Parse(buildDateStr.Substring(2, 2)),
						Int32.Parse(buildDateStr.Substring(0, 2)));

					if (programVer < 6)
					{
						throw new EmDeviceOldVersionException();
					}
					if (dtBuildDate < new DateTime(2011, 1, 15))
					{
						throw new EmDeviceOldVersionException();
					}
				}
				// end of device version

				#region Current Limits

				EmService.WriteToLogGeneral("before reading current limits");
				// current current limits
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)186 }) != 0) return ExchangeResult.Other_Error;
				devInfo_.SD_CurrentRangeName_1 = Conversions.bytes_2_string(ref buffer, 0, 16);
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)187 }) != 0) return ExchangeResult.Other_Error;
				devInfo_.SD_CurrentRangeName_2 = Conversions.bytes_2_string(ref buffer, 0, 16);
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)188 }) != 0) return ExchangeResult.Other_Error;
				devInfo_.SD_CurrentRangeName_3 = Conversions.bytes_2_string(ref buffer, 0, 16);
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)189 }) != 0) return ExchangeResult.Other_Error;
				devInfo_.SD_CurrentRangeName_4 = Conversions.bytes_2_string(ref buffer, 0, 16);
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)190 }) != 0) return ExchangeResult.Other_Error;
				devInfo_.SD_CurrentRangeName_5 = Conversions.bytes_2_string(ref buffer, 0, 16);
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)191 }) != 0) return ExchangeResult.Other_Error;
				devInfo_.SD_CurrentRangeName_6 = Conversions.bytes_2_string(ref buffer, 0, 16);
				EmService.WriteToLogGeneral("after reading current limits");

				#endregion

				#region PQP

				EmService.WriteToLogGeneral("ReadDeviceInfo: start region PQP");

				if (Read(EmCommands.COMMAND_ReadQualityContents, ref buffer, null) != 0)
				{
					EmService.WriteToLogFailed("COMMAND_ReadQualityContents failed!");
					EmService.WriteToLogFailed("buffer length = " + buffer.Length);
					return ExchangeResult.Other_Error;
				}

				int recLen = 20;
				if (buffer != null)
				{
					iNumOfPkeRecords = buffer.Length / recLen;	// число архивов ПКЭ
					bool needAddDay = false;
					DateTime dtStart, dtEnd;
					ushort iYear;
					byte iMo, iDay, iHour, iMin, iSec;

					for (int i = 0; i < iNumOfPkeRecords; ++i)
					{
						if ((i * recLen + (recLen - 1)) >= buffer.Length)
							break;

						iYear = (ushort)(buffer[i * recLen + 0] + (buffer[i * recLen + 1] << 8));
						if (iYear < 2000) iYear += 2000; 
						iDay = buffer[i * recLen + 2];
						iMo = buffer[i * recLen + 3];
						iHour = buffer[i * recLen + 5];
						iMin = buffer[i * recLen + 4];
						iSec = buffer[i * recLen + 6];

						try
						{
							dtStart = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);
						}
						catch (ArgumentOutOfRangeException ex)
						{
							EmService.DumpException(ex, "Invalid start pqp date:");
							EmService.WriteToLogFailed("Year = " + iYear + ", Month = " + iMo +
								", Day = " + iDay + ", Hour = " + iHour + ", Min = " + iMin + 
								", Sec = " + iSec);
							continue;
						}

						iYear = (ushort)(buffer[i * recLen + 0 + 8] + (buffer[i * recLen + 1 + 8] << 8));
						if (iYear < 2000) iYear += 2000;
						iDay = buffer[i * recLen + 2 + 8];
						iMo = buffer[i * recLen + 3 + 8];
						iHour = buffer[i * recLen + 5 + 8];
						iMin = buffer[i * recLen + 4 + 8];
						iSec = buffer[i * recLen + 6 + 8];

						try
						{
							// если прибор возвращает 24 часа, то к конечной дате архива нужно прибавить 1 день
							if (iHour == 24)
							{
								needAddDay = true;
								iHour = 0;
							}
							else
								needAddDay = false;

							dtEnd = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);
							if (needAddDay)
								dtEnd = dtEnd.AddDays(1);

							if (dtStart > DateTime.Now)
							{
								EmService.WriteToLogFailed(
									"Invalid PQP Date (too large)!");
								EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
								iYear, iMo, iDay, iHour, iMin, iSec));
								//continue;
							}
						}
						catch (ArgumentOutOfRangeException ex)
						{
							EmService.DumpException(ex, "ArgumentOutOfRangeException in pqp dates:");
							EmService.WriteToLogFailed("Year = " + iYear + ", Month = " + iMo +
								", Day = " + iDay + ", Hour = " + iHour + ", Min = " + iMin +
								", Sec = " + iSec);
							continue;
						}
						PqpSetEtPQP tmpPqp = new PqpSetEtPQP();
						tmpPqp.PqpStart = dtStart;
						tmpPqp.PqpEnd = dtEnd;

						ushort globalObjId = Conversions.bytes_2_ushort(ref buffer, i * recLen + 16);
						ContentsLineEtPQP curObj = devInfo_.Content.FindRecord(globalObjId);

						if (curObj == null)
						{
							EmService.WriteToLogFailed("Error in pqp dates: Object was not found!");
							continue;
						}

						curObj.PqpSet.Add(tmpPqp);
						curObj.PqpExists = true;
					}
				}

				#region COMMAND_ReadQualityDatesByObject

				// даты ПКЭ =================================================
				/*for (int iObj = 0; iObj < devInfo_.Content.Count; ++iObj)
				{
					if (Read(COMMAND_ReadQualityDatesByObject, ref buffer,
							new object[] { devInfo_.Content[iObj].GlobalObjectId }) != 0)
						return -1;

					int recLen = 28;
					if (buffer != null)
					{
						iNumOfPkeRecords = buffer.Length / recLen;	// число архивов ПКЭ
						bool needAddDay = false;
						DateTime dtStart, dtEnd;
						ushort iYear;
						byte iMo, iDay;

						for (int i = 0; i < iNumOfPkeRecords; ++i)
						{
							if ((i * recLen + (recLen - 1)) >= buffer.Length)
								break;

							iYear = (ushort)(buffer[i * recLen + 0] + (buffer[i * recLen + 1] << 8));
							if (iYear < 2000) iYear += 2000; 
							iDay = buffer[i * recLen + 2];
							iMo = buffer[i * recLen + 3];

							try
							{
								dtStart = new DateTime(iYear, iMo, iDay,
												buffer[i * recLen + 4],
												buffer[i * recLen + 5],
												buffer[i * recLen + 7]);

								// если прибор возвращает 24 часа, то к конечной дате 
								// архива нужно прибавить 1 день
								if (buffer[i * recLen + 8] == 24)
								{
									needAddDay = true;
									buffer[i * recLen + 8] = 0;
								}
								else
									needAddDay = false;

								dtEnd = new DateTime(iYear, iMo, iDay,
												buffer[i * recLen + 8],
												buffer[i * recLen + 9],
												buffer[i * recLen + 11]);
								if (needAddDay)
									dtEnd = dtEnd.AddDays(1);

								if (dtStart > DateTime.Now)
								{
									EmService.WriteToLogFailed(
										"Invalid PQP Date (too large)!");
									EmService.WriteToLogFailed(
									string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
									iYear, iMo, iDay, iHour, iMin, iSec));
									//continue;
								}
							}
							catch (ArgumentOutOfRangeException ex)
							{
								EmService.DumpException(ex, "Invalid pqp dates:");
								EmService.WriteToLogFailed("Year = " + iYear + ", Month = " + iMo +
									", Day = " + iDay + ", Hour = " + iHour + ", Min = " + iMin +
									", Sec = " + iSec);
								continue;
							}
							PqpSetEtPQP tmpPqp = new PqpSetEtPQP();
							tmpPqp.PqpStart = dtStart;
							tmpPqp.PqpEnd = dtEnd;

							// object id
							//tmpPqp.ObjectId = (int)Conversions.bytes_2_ushort(ref buffer, 12 + i * recLen);
							//devInfo_.Content.AddPqpArchive(tmpPqp);

							devInfo_.Content[iObj].PqpSet.Add(tmpPqp);
							devInfo_.Content[iObj].PqpExists = true;
						}
					}
				}*/
				// end of даты ПКЭ ==========================================

				#endregion

				EmService.WriteToLogGeneral("ReadDeviceInfo: end region PQP");

				#endregion

				#region dip and swell pointers

				EmService.WriteToLogGeneral("ReadDeviceInfo: start region DNS");

				/////////////////////////////////////////////////////
				// dip and swell

				// at first read current dip/swell
				if (Read(EmCommands.COMMAND_ReadDipSwellStatus, ref buffer, null) != 0)
				{
					EmService.WriteToLogFailed("COMMAND_ReadDipSwellStatus failed!");
					EmService.WriteToLogFailed("buffer length = " + buffer.Length);
					return ExchangeResult.Other_Error;
				}
				// analyze result
				if (buffer != null && buffer.Length >= 172)
				{
					if (!ParseDipSwellStatusBuffer(ref buffer))
						devInfo_.ResetAllDeviceDns();
				}

				// read dip pointers
				//ushort startParam = 69;  // номер записи в системных данных прибора
				//for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				//{
				//    // по всем фазам читаем только 3пр или 4пр, а не обе сразу
				//    //if (devInfo_.ConnectionScheme == 2 && iPhase == 6) { startParam++; continue; }
				//    //if (devInfo_.ConnectionScheme == 1 && iPhase == 7) { startParam++; continue; }

				//    if (Read(COMMAND_ReadSystemData, ref buffer, new object[] { startParam++ }) != 0)
				//        return -1;
				//    if (buffer == null || buffer.Length < 4)
				//    {
				//        EmService.WriteToLogFailed("Unable to read dip pointer, phase = "
				//                                            + iPhase.ToString());
				//        devInfo_.PointersDip[iPhase] = -1;
				//    }
				//    else
				//    {
				//        devInfo_.PointersDip[iPhase] = Conversions.bytes_2_uint_new(ref buffer, 0);
				//    }
				//}

				// read swell pointers
				//startParam = 77;  // номер записи в системных данных прибора
				//for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				//{
				//    // по всем фазам читаем только 3пр или 4пр, а не обе сразу
				//    //if (devInfo_.ConnectionScheme == 2 && iPhase == 6) { startParam++; continue; }
				//    //if (devInfo_.ConnectionScheme == 1 && iPhase == 7) { startParam++; continue; }

				//    if (Read(COMMAND_ReadSystemData, ref buffer, new object[] { startParam++ }) != 0)
				//        return -1;
				//    if (buffer == null || buffer.Length < 4)
				//    {
				//        EmService.WriteToLogFailed("Unable to read swell pointer, phase = "
				//                                            + iPhase.ToString());
				//        devInfo_.PointersSwell[iPhase] = -1;
				//    }
				//    else
				//    {
				//        devInfo_.PointersSwell[iPhase] = Conversions.bytes_2_uint_new(ref buffer, 0);
				//    }
				//}

				if (!GetDipSwellStartEndDates())
				{
					devInfo_.ResetAllDeviceDns();
					EmService.WriteToLogDebug("ResetAllDeviceDns() was called!");
				}

				EmService.WriteToLogGeneral("ReadDeviceInfo: end region DNS");

				#endregion

				#region avg

				EmService.WriteToLogGeneral("ReadDeviceInfo: start region AVG");

				for (int iObj = 0; iObj < devInfo_.Content.Count; ++iObj)
				{
					if (Read(EmCommands.COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand, ref buffer,
										new object[] { devInfo_.Content[iObj].GlobalObjectId }) != 0)
					{
						EmService.WriteToLogFailed(
							"COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand failed!");
						EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						return ExchangeResult.Other_Error;
					}

					if (buffer == null)
					{
						EmService.WriteToLogFailed(
							"COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand failed!");
						EmService.WriteToLogFailed("buffer = null");
					}
					else
					if (buffer.Length < 48)
					{
						EmService.WriteToLogFailed(
							"COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand failed!");
						EmService.WriteToLogFailed("buffer < 48");
					}

					if (buffer != null && buffer.Length >= 48)
					{
						DateTime dtStart3sec, dtEnd3sec, dtStart1min, dtEnd1min, dtStart30min, dtEnd30min;
						ushort iYear = 0;
						byte iMo = 0, iDay = 0;
						byte iHour = 0, iMin = 0, iSec = 0;

						try
						{
							if (buffer[0] == 255)
							{
								dtStart3sec = DateTime.MinValue;
								dtEnd3sec = DateTime.MinValue;
							}
							else
							{
								iYear = (ushort)(buffer[0] + (buffer[1] << 8));
								if (iYear < 2008) iYear += 2000;
								iDay = buffer[2];
								iMo = buffer[3];
								iHour = buffer[5];
								iMin = buffer[4];
								iSec = buffer[7];
								dtStart3sec = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);

								iYear = (ushort)(buffer[8] + (buffer[9] << 8));
								if (iYear < 2008) iYear += 2000;
								iDay = buffer[10];
								iMo = buffer[11];
								iHour = buffer[13];
								iMin = buffer[12];
								iSec = buffer[15];
								dtEnd3sec = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);
							}
						
							if (dtStart3sec > DateTime.Now)
							{
								EmService.WriteToLogFailed("Invalid AVG Date (too large)!");
								EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
								iYear, iMo, iDay, iHour, iMin, iSec));
								//throw new EmException("Invalid AVG Date (too large)!");
							}

							devInfo_.Content[iObj].DateStartAvg3sec = dtStart3sec;
							devInfo_.Content[iObj].DateEndAvg3sec = dtEnd3sec;
						}
						catch (ArgumentOutOfRangeException aex)
						{
							EmService.DumpException(aex, "Error in avg dates 3 sec:");
							devInfo_.Content[iObj].DateStartAvg3sec = DateTime.MinValue;
							devInfo_.Content[iObj].DateEndAvg3sec = DateTime.MinValue;
							EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}", 
								iYear, iMo, iDay, iHour, iMin, iSec));
						}

						try
						{
							if (buffer[16] == 255)
							{
								dtStart1min = DateTime.MinValue;
								dtEnd1min = DateTime.MinValue;
							}
							else
							{
								iYear = (ushort)(buffer[0 + 16] + (buffer[1 + 16] << 8));
								if (iYear < 2008) iYear += 2000;
								iDay = buffer[2 + 16];
								iMo = buffer[3 + 16];
								iHour = buffer[5 + 16];
								iMin = buffer[4 + 16];
								iSec = buffer[7 + 16];
								dtStart1min = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);

								iYear = (ushort)(buffer[8 + 16] + (buffer[9 + 16] << 8));
								if (iYear < 2008) iYear += 2000;
								iDay = buffer[10 + 16];
								iMo = buffer[11 + 16];
								iHour = buffer[13 + 16];
								iMin = buffer[12 + 16];
								iSec = buffer[15 + 16];
								dtEnd1min = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);
							}

							if (dtStart1min > DateTime.Now)
							{
								EmService.WriteToLogFailed("Invalid AVG Date (too large)!");
								EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
								iYear, iMo, iDay, iHour, iMin, iSec));
								//throw new EmException("Invalid AVG Date (too large)!");
							}

							devInfo_.Content[iObj].DateStartAvg1min = dtStart1min;
							devInfo_.Content[iObj].DateEndAvg1min = dtEnd1min;
						}
						catch (ArgumentOutOfRangeException aex)
						{
							EmService.DumpException(aex, "Error in avg dates 1 min:");
							devInfo_.Content[iObj].DateStartAvg1min = DateTime.MinValue;
							devInfo_.Content[iObj].DateEndAvg1min = DateTime.MinValue;
							EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}", 
								iYear, iMo, iDay, iHour, iMin, iSec));
						}

						try
						{
							if (buffer[32] == 255)
							{
								dtStart30min = DateTime.MinValue;
								dtEnd30min = DateTime.MinValue;
							}
							else
							{
								iYear = (ushort)(buffer[0 + 32] + (buffer[1 + 32] << 8));
								if (iYear < 2008) iYear += 2000;
								iDay = buffer[2 + 32];
								iMo = buffer[3 + 32];
								iHour = buffer[5 + 32];
								iMin = buffer[4 + 32];
								iSec = buffer[7 + 32];
								dtStart30min = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);

								iYear = (ushort)(buffer[8 + 32] + (buffer[9 + 32] << 8));
								if (iYear < 2008) iYear += 2000;
								iDay = buffer[10 + 32];
								iMo = buffer[11 + 32];
								iHour = buffer[13 + 32];
								iMin = buffer[12 + 32];
								iSec = buffer[15 + 32];
								dtEnd30min = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);
							}
							if (dtStart30min > DateTime.Now)
							{
								EmService.WriteToLogFailed("Invalid AVG Date (too large)!");
								EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
								iYear, iMo, iDay, iHour, iMin, iSec));
								//throw new EmException("Invalid AVG Date (too large)!");
							}

							devInfo_.Content[iObj].DateStartAvg30min = dtStart30min;
							devInfo_.Content[iObj].DateEndAvg30min = dtEnd30min;
						}
						catch (ArgumentOutOfRangeException aex)
						{
							EmService.DumpException(aex, "Error in avg dates 30 min:");
							devInfo_.Content[iObj].DateStartAvg30min = DateTime.MinValue;
							devInfo_.Content[iObj].DateEndAvg30min = DateTime.MinValue;
							EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
								iYear, iMo, iDay, iHour, iMin, iSec));
						}

						if (devInfo_.Content[iObj].DateStartAvg3sec != DateTime.MinValue ||
								devInfo_.Content[iObj].DateStartAvg1min != DateTime.MinValue ||
								devInfo_.Content[iObj].DateStartAvg30min != DateTime.MinValue)
						{
							EmService.WriteToLogGeneral(
								"devInfo_.AvgExists = true; object = " + iObj.ToString());
							devInfo_.Content[iObj].AvgExists = true;
						}
						else
						{
							EmService.WriteToLogGeneral(
								"devInfo_.AvgExists = FALSE; object = " + iObj.ToString());
							devInfo_.Content[iObj].AvgExists = false;
						}
					}
				}

				#endregion
			}
			catch (System.Threading.ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in ReadDeviceInfo()");
				Thread.ResetAbort();
				return ExchangeResult.Other_Error;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDeviceInfo()");
				throw;
			}

			return ExchangeResult.OK;
		}

		public ExchangeResult ReadAvgArchive(ref byte[] buffer, DateTime dtStart, DateTime dtEnd, AvgTypes avgType,
									int globalObjectId)
		{
			try
			{
				ContentsLineEtPQP curObject = devInfo_.Content.FindRecord(globalObjectId);
				if (curObject == null) throw new EmException("Invalid object id!");

				DateTime dtTempStart = DateTime.MaxValue;
				DateTime dtTempEnd = DateTime.MinValue;
				int interval = 60;
				EmCommands curCommand = EmCommands.COMMAND_Read30minArchiveByTimestampObjectDemand;
				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] tempBuffer = null;
				List<byte> listTempBuffer = new List<byte>();

				switch (avgType)
				{
					case AvgTypes.ThreeSec: dtTempStart = curObject.DateStartAvg3sec;
						dtTempEnd = curObject.DateEndAvg3sec;
						curCommand = EmCommands.COMMAND_Read3secArchiveByTimestampObjectDemand;
						interval = 3;
						break;
					case AvgTypes.OneMin: dtTempStart = curObject.DateStartAvg1min;
						dtTempEnd = curObject.DateEndAvg1min;
						curCommand = EmCommands.COMMAND_Read1minArchiveByTimestampObjectDemand;
						interval = 60;
						break;
					case AvgTypes.ThirtyMin: dtTempStart = curObject.DateStartAvg30min;
						dtTempEnd = curObject.DateEndAvg30min;
						curCommand = EmCommands.COMMAND_Read30minArchiveByTimestampObjectDemand;
						interval = 1800;
						break;
				}
				while (dtTempStart < dtStart) dtTempStart = dtTempStart.AddSeconds(interval);

				while (dtTempStart <= dtEnd && dtTempStart <= dtTempEnd)
				{
					tempBuffer = null;

					errCode = Read(curCommand, ref tempBuffer, new object[] { (ushort)dtTempStart.Year, 
									(byte)dtTempStart.Month, (byte)dtTempStart.Day, (byte)dtTempStart.Hour, 
									(byte)dtTempStart.Minute, (byte)dtTempStart.Second, globalObjectId });
					//if (errCode != 0) return errCode;

					// если был дисконнект, то выходим!
					if (errCode == ExchangeResult.Disconnect_Error) throw new EmDisconnectException();

					// если буфер слишком большой (могло придти два пакета), то обрезаем лишнее
					if (tempBuffer != null && tempBuffer.Length > avgArchiveLength_)
					{
						byte[] oldBuffer = tempBuffer;
						tempBuffer = new byte[avgArchiveLength_];
						Array.Copy(oldBuffer, tempBuffer, avgArchiveLength_);
					}

					if (errCode == ExchangeResult.OK && tempBuffer != null)
						listTempBuffer.AddRange(tempBuffer);

					dtTempStart = dtTempStart.AddSeconds(interval);

					// считан очерендной пакет, поэтому меняем прогрессбар
					if (OnStepReading != null) OnStepReading(EmDeviceType.ETPQP);
				}

				if (listTempBuffer.Count > 0)
				{
					buffer = new byte[listTempBuffer.Count];
					listTempBuffer.CopyTo(buffer);
				}

				return errCode;
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in ReadAvgArchive(): " + emx.Message);
				return ExchangeResult.Other_Error;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadAvgArchive():");
				throw;
			}
		}

		public ExchangeResult ResetAllAvgQuery()
		{
			try
			{
				byte[] buffer = null;
				return Read(EmCommands.COMMAND_AverageArchiveQuery, ref buffer,
								new object[] { (ushort)QueryAvgType.AAQ_TYPE_ResetAll, GetIdAvgQuery() }, 1);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ResetAllAvgQuery:");
				throw;
			}
		}

		public ExchangeResult ReadAvgArchiveSelectively(ref byte[] buffer, DateTime dtStart, DateTime dtEnd,
						AvgTypes avgType, ref List<ushort> fields)
		{
			try
			{
				if (fields == null || fields.Count < 1 || fields.Count > maxNumAvgFieldsQuery_)
					throw new EmException("ReadAvgArchiveSelectively(): Invalid fields parameters count!  "
											+ fields.Count.ToString());

				ExchangeResult errCode = ExchangeResult.Other_Error;
			
				List<object> listQueryParams = new List<object>();

				listQueryParams.Add((ushort)QueryAvgType.AAQ_TYPE_Query);
				listQueryParams.Add((ushort)GetIdAvgQuery());
				listQueryParams.Add((ushort)avgType);
				listQueryParams.Add((ushort)dtStart.Year);
				listQueryParams.Add((byte)dtStart.Month);
				listQueryParams.Add((byte)dtStart.Day);
				listQueryParams.Add((byte)dtStart.Hour);
				listQueryParams.Add((byte)dtStart.Minute);
				listQueryParams.Add((byte)dtStart.Second);
				listQueryParams.Add((ushort)dtEnd.Year);
				listQueryParams.Add((byte)dtEnd.Month);
				listQueryParams.Add((byte)dtEnd.Day);
				listQueryParams.Add((byte)dtEnd.Hour);
				listQueryParams.Add((byte)dtEnd.Minute);
				listQueryParams.Add((byte)dtEnd.Second);

				for (int iField = 0; iField < fields.Count; ++iField)
				{
					listQueryParams.Add((ushort)fields[iField]);
				}

				object[] query_params = new object[listQueryParams.Count];
				listQueryParams.CopyTo(query_params);

				ushort status = (ushort)QueryAvgCurStatus.AAQ_STATE_Busy;

				errCode = Read(EmCommands.COMMAND_AverageArchiveQuery, ref buffer,
					new object[] { (ushort)QueryAvgType.AAQ_TYPE_ReadStatus, GetIdAvgQuery() });
				if (errCode != ExchangeResult.OK) return errCode;
				status = Conversions.bytes_2_ushort(ref buffer, 4);
				
				if (status != (ushort)QueryAvgCurStatus.AAQ_STATE_Idle)
				{
					errCode = Read(EmCommands.COMMAND_AverageArchiveQuery, ref buffer,
						new object[] { (ushort)QueryAvgType.AAQ_TYPE_ResetAll, GetIdAvgQuery() });
					Thread.Sleep(5000);
				}

				errCode = Read(EmCommands.COMMAND_AverageArchiveQuery, ref buffer, query_params);
				// поскольку был запрос усреденных с параметрами, то пришел "неполный" буфер и
				// надо его восстановить до полного, чтобы можно было класть в БД стандартными функциями
				if ((buffer != null) && (errCode == ExchangeResult.OK))
					MakeStandartAvgBuffer(query_params, ref buffer);

				return errCode;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadAvgArchiveSelectively():");
				throw;
			}
		}

		public ExchangeResult ReadDipSwellArchive(ref byte[] buffer, DateTime dtStart, DateTime dtEnd, 
										ref ContentsLineEtPQP cl)
		{
			try
			{
				Int64 pointerStart, pointerEnd;
				List<byte> listTempBuffer = new List<byte>();
				byte[] tempBuffer = null;
				ExchangeResult errCode = ExchangeResult.Other_Error;

				// read pointers for all phases
				long[] pointers = null;
				if (!ReadDipSwellIndexes(dtStart, dtEnd, out pointers, cl.GlobalObjectId))
					return ExchangeResult.Other_Error;

				// dips
				for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				{
					// по всем фазам читаем только 3пр или 4пр, а не обе сразу
					if ((cl.ConnectionScheme == ConnectScheme.Ph3W3 ||
						cl.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
						&& iPhase == 6) { continue; }
					if ((cl.ConnectionScheme == ConnectScheme.Ph3W4 ||
						cl.ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
						&& iPhase == 7) { continue; }

					//if (!ReadDipSwellIndex(dtStart, iPhase, 1 /*dip*/, "start", out pointerStart, 
					//                        cl.GlobalObjectId))
					//    continue;
					//if (!ReadDipSwellIndex(dtEnd, iPhase, 1 /*dip*/, "end", out pointerEnd,
					//                        cl.GlobalObjectId))
					//    continue;
					if(pointers.Length < (iPhase * 2 + 2)) break;
					pointerStart = pointers[iPhase * 2];
					pointerEnd = pointers[iPhase * 2 + 1];

					// не читать события, которые могли успеть произойти после того как были считаны 
					// данные по COMMAND_ReadDipSwellStatus, иначе будет путаница
					if (pointerEnd >= cl.CurPointersDip[iPhase].Pointer &&
						cl.CurPointersDip[iPhase].Pointer != -1)
						pointerEnd = cl.CurPointersDip[iPhase].Pointer - 1;

					if (pointerEnd < pointerStart)
					{
						EmService.WriteToLogFailed(string.Format(
							"ReadDipSwellArchive() dips: pointerEnd = {0} < pointerStart = {1}",
							pointerEnd, pointerStart));
						continue;
					}
					if (pointerEnd == -1 || pointerStart == -1)
						continue;

					long count = (int)(pointerEnd - pointerStart + 1);
					ushort curCount;
					while (count > 0)
					{
						if (count <= 256) curCount = (ushort)count;
						else curCount = 256;
						errCode = Read(EmCommands.COMMAND_ReadDipSwellArchiveByObject, ref tempBuffer,
										new object[] { PhaseToUshort(iPhase, 1 /*dip*/), pointerStart, 
													(ushort)curCount, cl.GlobalObjectId });
						if (errCode != ExchangeResult.OK) return errCode;

						if (tempBuffer != null)
							listTempBuffer.AddRange(tempBuffer);
						count -= curCount;
					}

					if (OnStepReading != null) OnStepReading(EmDeviceType.ETPQP);
				}

				// swells
				for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				{
					// по всем фазам читаем только 3пр или 4пр, а не обе сразу
					if ((cl.ConnectionScheme == ConnectScheme.Ph3W3 ||
						cl.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
						&& iPhase == 6) { continue; }
					if ((cl.ConnectionScheme == ConnectScheme.Ph3W4 ||
						cl.ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
						&& iPhase == 7) { continue; }

					//if (!ReadDipSwellIndex(dtStart, iPhase, 0 /*swell*/, "start", out pointerStart,
					//                        cl.GlobalObjectId))
					//    continue;
					//if (!ReadDipSwellIndex(dtEnd, iPhase, 0 /*swell*/, "end", out pointerEnd,
					//                        cl.GlobalObjectId))
					//    continue;
					if (pointers.Length < (iPhase * 2 + 2 + 16)) 
						break;
					// здесь 16 это 8 событий (А, В, С, АВ, ВС, СА, АВС, АВСN) по 2 индекса (start и end)
					pointerStart = pointers[iPhase * 2 + 16];
					pointerEnd = pointers[iPhase * 2 + 1 + 16];

					// не читать события, которые могли успеть произойти после того как были считаны 
					// данные по COMMAND_ReadDipSwellStatus, иначе будет путаница
					if (pointerEnd >= cl.CurPointersSwell[iPhase].Pointer &&
											cl.CurPointersSwell[iPhase].Pointer != -1)
						pointerEnd = cl.CurPointersSwell[iPhase].Pointer - 1;

					if (pointerEnd < pointerStart)
					{
						EmService.WriteToLogFailed(string.Format(
							"ReadDipSwellArchive() swells: pointerEnd = {0} < pointerStart = {1}",
							pointerEnd, pointerStart));
						continue;
					}
					if (pointerEnd == -1 || pointerStart == -1)
						continue;

					long count = (int)(pointerEnd - pointerStart + 1);
					ushort curCount;
					while (count > 0)
					{
						if (count <= 256) curCount = (ushort)count;
						else curCount = 256;
						errCode = Read(EmCommands.COMMAND_ReadDipSwellArchiveByObject, ref tempBuffer,
										new object[] { PhaseToUshort(iPhase, 0 /*swell*/), pointerStart, 
													(ushort)curCount, cl.GlobalObjectId });
						if (errCode != ExchangeResult.OK) return errCode;

						if (tempBuffer != null)
							listTempBuffer.AddRange(tempBuffer);
						count -= curCount;
					}

					if (OnStepReading != null) OnStepReading(EmDeviceType.ETPQP);
				}

				if (listTempBuffer.Count > 0)
				{
					buffer = new byte[listTempBuffer.Count];
					listTempBuffer.CopyTo(buffer);
				}

				return errCode;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDateOfDipSwellEvent():");
				throw;
			}
		}

		public ExchangeResult ReadObjectNames(ref string[] names)
		{
			try
			{
				byte[] buffer = null;
				ExchangeResult errCode = ExchangeResult.Other_Error;
				names = new string[COUNT_OBJ_NAMES];

				int curParamNumber = 143;
				for (int iName = 0; iName < COUNT_OBJ_NAMES; ++iName)
				{
					errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer,
						new object[] { (ushort)curParamNumber++ });
					if (errCode != ExchangeResult.OK)
						return errCode;
					names[iName] = Conversions.bytes_2_string(ref buffer, 0, 16);
				}
				return errCode;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadObjectNames:");
				throw;
			}
		}

		public ExchangeResult ReadNominalsAndTimes(ref object[] vals)
		{
			try
			{
				byte[] buffer = null;
				ExchangeResult errCode;

				ushort paramNumber = 4;   // frequency nominal
				errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { paramNumber });
				if (errCode != ExchangeResult.OK)
					return errCode;
				vals[0] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true);

				paramNumber = 6;   // linear voltage nominal
				errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { paramNumber });
				if (errCode != ExchangeResult.OK)
					return errCode;
				vals[1] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true);

				paramNumber = 5;   // phase voltage nominal
				errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { paramNumber });
				if (errCode != ExchangeResult.OK)
					return errCode;
				vals[2] = Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true);

				paramNumber = 7;   // start max loading mode 1
				errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { paramNumber });
				if (errCode != ExchangeResult.OK)
					return errCode;
				vals[3] = Conversions.bytes_2_TimeSLIP(ref buffer, 0).ToString("HH:mm");

				paramNumber = 8;   // end max loading mode 1
				errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { paramNumber });
				if (errCode != ExchangeResult.OK)
					return errCode;
				vals[4] = Conversions.bytes_2_TimeSLIP(ref buffer, 0).ToString("HH:mm");

				paramNumber = 9;   // start max loading mode 2
				errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { paramNumber });
				if (errCode != ExchangeResult.OK)
					return errCode;
				vals[5] = Conversions.bytes_2_TimeSLIP(ref buffer, 0).ToString("HH:mm");

				paramNumber = 10;   // end max loading mode 2
				errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { paramNumber });
				if (errCode != ExchangeResult.OK)
					return errCode;
				vals[6] = Conversions.bytes_2_TimeSLIP(ref buffer, 0).ToString("HH:mm");
			
				return errCode;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadNominalsAndTimes:");
				throw;
			}
		}

		/// <summary>
		/// Reading device serial number
		/// </summary>
		public bool ReadDeviceSerialNumber(out long serialNumber)
		{
			serialNumber = -1;

			byte[] buffer = null;

			try
			{
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer,
							new object[] { (ushort)0 }) != 0) return false;

				if (buffer == null || buffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read serial number!");
					serialNumber = -1;
					return false;
				}
				serialNumber = (long)(Conversions.bytes_2_ushort(ref buffer, 0));
				EmService.WriteToLogGeneral("SERIAL NUMBER = " + serialNumber);
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDeviceSerialNumber():");
				serialNumber = -1;
				return false;
			}
			finally
			{
				serialNumber_ = serialNumber;
			}
		}

		public bool ReadTime(ref byte[] buffer)
		{
			buffer = null;

			try
			{
				if (Read(EmCommands.COMMAND_ReadTime, ref buffer, null) != 0) return false;

				if (buffer == null || buffer.Length < 20)
				{
					EmService.WriteToLogFailed("Unable to read device time!");
					buffer = null;
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ReadTime(): " + ex.Message);
				buffer = null;
				throw;
			}
		}

		public ExchangeResult Read(EmCommands command, ref byte[] buffer, object[] request_params)
		{
			return Read(command, ref buffer, request_params, 3);
		}

		public ExchangeResult Read(EmCommands command, ref byte[] buffer, object[] request_params, int attempts)
		{
			try
			{
				int num = 0;
				ExchangeResult res = ExchangeResult.Other_Error;

				List<UInt32> listParams;
				MakeIntList(ref request_params, out listParams);

				List<byte> listBuffer = null;
				while (++num <= attempts)
				{
					listBuffer = new List<byte>();
					res = portManager_.ReadData(command, ref listBuffer, listParams);
					
					if (res != ExchangeResult.OK)
						EmService.WriteToLogFailed("Reading error. Attempt " + num);
					if (res == ExchangeResult.OK)
						break;
					if (res == ExchangeResult.Disconnect_Error) 
					{
						//ClosePort();
						throw new EmDisconnectException();
					}
					if (bCancelReading_)
						break;
				}
				if (listBuffer != null && listBuffer.Count > 0)
				{
					buffer = new byte[listBuffer.Count];
					listBuffer.CopyTo(buffer);
				}

				return res;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQPDevice::Read():");
				throw;
			}
		}

		/// <summary>Write Constraints To the Device</summary>
		/// <param name="buffer">Buffer Containing Constraints To Write</param>
		public ExchangeResult WriteSets(ref byte[] buffer)
		{
			try
			{
				List<byte> tempList = new List<byte>(buffer);
				return portManager_.WriteData(EmCommands.COMMAND_WriteSets, ref tempList);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQPDevice::WriteSets():");
				throw;
			}
		}

		public ExchangeResult WriteNominalsAndTimes(ref object[] vals)
		{
			try
			{
				byte[] tempBuf = null;
				float f = Single.Parse(vals[0].ToString());
				if (!Conversions.float2wIEEE754_old_2_bytes(ref tempBuf, f))
					return ExchangeResult.Other_Error;
				if (WriteSystemData(4, ref tempBuf) != 0)
					return ExchangeResult.Write_Error;
				f = Single.Parse(vals[1].ToString());
				if (!Conversions.float2wIEEE754_old_2_bytes(ref tempBuf, f))
					return ExchangeResult.Other_Error;
				if (WriteSystemData(6, ref tempBuf) != 0)
					return ExchangeResult.Write_Error;
				// фазное закомментировано, т.к. достаточно писать линейное, а
				// фазное перерасчитается в приборе
				//f = Single.Parse(vals[2].ToString());
				//if (!Conversions.float2wIEEE754_old_2_bytes(ref tempBuf, f))
				//    return -1;
				//if (WriteSystemData(5, ref tempBuf) != 0)
				//    return -2;

				if (!Conversions.TimeSLIP_2_bytes(ref tempBuf, (DateTime)vals[3]))
					return ExchangeResult.Other_Error;
				if (WriteSystemData(7, ref tempBuf) != 0)
					return ExchangeResult.Write_Error;
				if (!Conversions.TimeSLIP_2_bytes(ref tempBuf, (DateTime)vals[4]))
					return ExchangeResult.Other_Error;
				if (WriteSystemData(8, ref tempBuf) != 0)
					return ExchangeResult.Write_Error;
				if (!Conversions.TimeSLIP_2_bytes(ref tempBuf, (DateTime)vals[5]))
					return ExchangeResult.Other_Error;
				if (WriteSystemData(9, ref tempBuf) != 0)
					return ExchangeResult.Write_Error;
				if (!Conversions.TimeSLIP_2_bytes(ref tempBuf, (DateTime)vals[6]))
					return ExchangeResult.Other_Error;
				if (WriteSystemData(10, ref tempBuf) != 0)
					return ExchangeResult.Write_Error;

				return ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQPDevice::WriteNominalsAndTimes():");
				throw;
			}
		}

		public ExchangeResult WriteObjectNames(ref byte[] buffer)
		{
			try
			{
				byte[] tempBuf;
				int oneNameLen = 16;
				int curParamNumber = 143;
				int curShift = 0;
				for (int iName = 0; iName < COUNT_OBJ_NAMES; ++iName)
				{
					tempBuf = new byte[oneNameLen];
					Array.Copy(buffer, curShift, tempBuf, 0, tempBuf.Length);
					// вместо нулей вставляем пробелы, а то прибор выдает BAD_DATA
					for (int iByte = 0; iByte < tempBuf.Length; ++iByte)
						if (tempBuf[iByte] == 0) tempBuf[iByte] = 32;
					// write to device
					if (WriteSystemData(curParamNumber++, ref tempBuf) != 0)
						return ExchangeResult.Write_Error;
					curShift += oneNameLen;
				}

				return ExchangeResult.OK;
			}
			catch (ArgumentOutOfRangeException aex)
			{
				EmService.DumpException(aex, "ArgumentOutOfRangeException in WriteObjectNames():");
				return ExchangeResult.Other_Error;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQPDevice::WriteObjectNames():");
				throw;
			}
		}

		#endregion

		#region Private Methods

		/// <summary>Write System Data To the Device</summary>
		/// <param name="param">Number of Sytem Parameter To Write</param>
		/// <param name="buffer">Value of this Parameter in Bytes</param>
		protected ExchangeResult WriteSystemData(int param, ref byte[] buffer)
		{
			try
			{
				byte[] time_buf = new byte[20];
				if (!ReadTime(ref time_buf))
					return ExchangeResult.Other_Error;
				ushort[] hash_buf = new ushort[10];
				CalcHashForWriting(ref time_buf, ref hash_buf);
				byte[] buffer_to_write = new byte[66 + buffer.Length];
				for (int iBuf = 0; iBuf < 66 + buffer.Length; ++iBuf) buffer_to_write[iBuf] = 0;
				buffer_to_write[0] = (byte)(param & 0xFF);
				buffer_to_write[1] = (byte)((param >> 8) & 0xFF);
				Array.Copy(buffer, 0, buffer_to_write, 2, buffer.Length);
				EmService.CopyUShortArrayToByteArray(ref hash_buf, ref buffer_to_write, 
					2 + buffer.Length);

				List<byte> tempList = new List<byte>(buffer_to_write);
				return portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref tempList);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQPDevice::Write():");
				throw;
			}
		}

		private static ushort GetIdAvgQuery()
		{
			++id_avg_query_;
			if (id_avg_query_ > 50) id_avg_query_ = 1;
			return id_avg_query_;
		}

		private bool ReadDipSwellIndexes(DateTime dtStart, DateTime dtEnd, 
										out long[] pointers, int globalObjId)
		{
			pointers = null;
			try
			{
				byte[] buffer = null;
				if (Read(EmCommands.COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject, ref buffer,
									new object[] { (ushort)dtStart.Year, (byte)dtStart.Month, 
										(byte)dtStart.Day, (byte)dtStart.Hour, (byte)dtStart.Minute,
										(byte)dtStart.Second,
										(ushort)dtEnd.Year, (byte)dtEnd.Month, 
										(byte)dtEnd.Day, (byte)dtEnd.Hour, (byte)dtEnd.Minute,
										(byte)dtEnd.Second,
										(ushort)globalObjId }) != 0)
					return false;

				if (buffer != null && buffer.Length >= 128)
				{
					pointers = new long[32];

					for (int iPointer = 0; iPointer < 32; ++iPointer)
					{
						pointers[iPointer] = Conversions.bytes_2_int(ref buffer, iPointer * 4);
					}
				}
				else return false;

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDipSwellIndexes():");
				return false;
			}
		}

		//private bool ReadDipSwellIndex(DateTime dtStart, int phase, byte typeEvent, string typeDate,
		//                                out Int64 pointer, int globalObjId)
		//{
		//    pointer = -1;
		//    try
		//    {
		//        byte[] buffer = null;
		//        ushort uPhase = PhaseToUshort(phase, typeEvent);
		//        if (typeDate == "start")
		//        {
		//            if (Read(COMMAND_ReadDipSwellIndexByStartTimestampByObject, ref buffer,
		//                            new object[] { uPhase, (ushort)dtStart.Year, (byte)dtStart.Month, 
		//                            (byte)dtStart.Day, (byte)dtStart.Hour, (byte)dtStart.Minute,
		//                            (byte)dtStart.Second, (ushort)globalObjId }) != 0)
		//                return false;
		//        }
		//        else if (typeDate == "end")
		//        {
		//            if (Read(COMMAND_ReadDipSwellIndexByEndTimestampByObject, ref buffer,
		//                            new object[] { uPhase, (ushort)dtStart.Year, (byte)dtStart.Month, 
		//                            (byte)dtStart.Day, (byte)dtStart.Hour, (byte)dtStart.Minute,
		//                            (byte)dtStart.Second, (ushort)globalObjId }) != 0)
		//                return false;
		//        }
		//        else return false;

		//        if (buffer != null && buffer.Length >= 4)
		//        {
		//            //UInt16 command = Conversions.bytes_2_ushort(ref buffer, 0);
		//            //if (command != 0x1000)
		//            //    return false;

		//            pointer = Conversions.bytes_2_uint_new(ref buffer, 0);
		//        }
		//        else return false;

		//        return true;
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Error in ReadDipSwellIndex():");
		//        return false;
		//    }
		//}
		
		private bool ParseDipSwellStatusBuffer(ref byte[] buffer)
		{
			ContentsLineEtPQP curObj = null;
			try
			{
				int globalObjId = Conversions.bytes_2_ushort(ref buffer, 10);
				curObj = devInfo_.Content.FindRecord(globalObjId);
				if (curObj == null)
					throw new EmException("object was not found!");

				devInfo_.GlobalIdObjectOfCurDNS = globalObjId;
				curObj.BufCurDipSwellData = buffer;

				ushort status = Conversions.bytes_2_ushort(ref buffer, 8);

				ushort curStatus = 0x0001;
				ushort curOffset = 12;
				for (int iPhase = 0; iPhase < /*EmService.CountPhases*/ 6; ++iPhase)
				{
					if ((status & curStatus) != 0)		// dip
					{
						curObj.CurPointersDip[iPhase].Pointer =
								Conversions.bytes_2_uint_new(ref buffer, curOffset);
						ushort iYear = (ushort)(buffer[curOffset + 4] + (buffer[curOffset + 5] << 8));
						curObj.CurPointersDip[iPhase].Date =
								new DateTime(iYear, buffer[curOffset + 7],
										buffer[curOffset + 6], buffer[curOffset + 9],
										buffer[curOffset + 8], buffer[curOffset + 11]);
					}
					else if ((status & (curStatus * 2)) != 0)		// swell
					{
						curObj.CurPointersSwell[iPhase].Pointer =
							Conversions.bytes_2_uint_new(ref buffer, curOffset);
						ushort iYear = (ushort)(buffer[curOffset + 4] + (buffer[curOffset + 5] << 8));
						curObj.CurPointersSwell[iPhase].Date =
								new DateTime(iYear, buffer[curOffset + 7],
										buffer[curOffset + 6], buffer[curOffset + 9],
										buffer[curOffset + 8], buffer[curOffset + 11]);
					}
					curStatus *= 4;
					curOffset += 16;
				}

				if (curObj.ConnectionScheme == ConnectScheme.Ph3W3 ||
					curObj.ConnectionScheme == ConnectScheme.Ph3W3_B_calc) // 3пр
				{
					if ((status & 0x4000) != 0)
					{
						curObj.CurPointersDip[(int)Phase.ABC].Pointer =
												Conversions.bytes_2_uint_new(ref buffer, 140);
						ushort iYear = (ushort)(buffer[140 + 4] + (buffer[140 + 5] << 8));
						curObj.CurPointersDip[(int)Phase.ABC].Date =
								new DateTime(iYear, buffer[140 + 7],
										buffer[140 + 6], buffer[140 + 9],
										buffer[140 + 8], buffer[140 + 11]);
					}
					if ((status & 0x8000) != 0)
					{
						curObj.CurPointersSwell[(int)Phase.ABC].Pointer =
												Conversions.bytes_2_uint_new(ref buffer, 156);
						ushort iYear = (ushort)(buffer[156 + 4] + (buffer[156 + 5] << 8));
						curObj.CurPointersSwell[(int)Phase.ABC].Date =
								new DateTime(iYear, buffer[156 + 7],
										buffer[156 + 6], buffer[156 + 9],
										buffer[156 + 8], buffer[156 + 11]);
					}
				}
				else if (curObj.ConnectionScheme == ConnectScheme.Ph3W4 ||
							curObj.ConnectionScheme == ConnectScheme.Ph3W4_B_calc)  // 4 пр
				{
					if ((status & 0x1000) != 0)
					{
						curObj.CurPointersDip[(int)Phase.ABCN].Pointer =
												Conversions.bytes_2_uint_new(ref buffer, 108);
						ushort iYear = (ushort)(buffer[108 + 4] + (buffer[108 + 5] << 8));
						curObj.CurPointersDip[(int)Phase.ABCN].Date =
								new DateTime(iYear, buffer[108 + 7],
										buffer[108 + 6], buffer[108 + 9],
										buffer[108 + 8], buffer[108 + 11]);
					}
					if ((status & 0x2000) != 0)
					{
						curObj.CurPointersSwell[(int)Phase.ABCN].Pointer =
												Conversions.bytes_2_uint_new(ref buffer, 124);
						ushort iYear = (ushort)(buffer[124 + 4] + (buffer[124 + 5] << 8));
						curObj.CurPointersSwell[(int)Phase.ABCN].Date =
								new DateTime(iYear, buffer[124 + 7],
										buffer[124 + 6], buffer[124 + 9],
										buffer[124 + 8], buffer[124 + 11]);
					}
				}

				if (!curObj.CurDnsExists())
				{
					curObj.BufCurDipSwellData = null;
					devInfo_.GlobalIdObjectOfCurDNS = -1;
				}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ParseDipSwellStatusBuffer():");
				devInfo_.GlobalIdObjectOfCurDNS = -1;
				if (curObj != null) curObj.ResetCurDns();
				return false;
			}
		}

		/// <summary>
		/// Функция получает первую и последнюю дату архива DNS для каждого объекта
		/// </summary>
		/// <returns></returns>
		private bool GetDipSwellStartEndDates()
		{
			try
			{
				#region old code

				//byte[] buffer = null;
				//int recLength = 22;

				//if (Read(COMMAND_ReadEarliestAndLatestDipSwellTimestampsForEveryObject, 
				//ref buffer, null) != 0)
				//	return false;

				// если чтение неудачно
				//if (buffer == null || buffer.Length < recLength)
				//{
				//    bool res = false;
				//    for (int iObj = 0; iObj < devInfo_.Content.Count; ++iObj)
				//    {
				//        devInfo_.Content[iObj].DateStartDipSwell = DateTime.MaxValue;
				//        if (devInfo_.Content[iObj].CurDnsExists())
				//        {
				//            for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				//            {
				//                if (devInfo_.Content[iObj].CurPointersDip[iPhase].Pointer != -1)
				//                {
				//                    if (devInfo_.Content[iObj].CurPointersDip[iPhase].Date <
				//                                    devInfo_.Content[iObj].DateStartDipSwell)
				//                        devInfo_.Content[iObj].DateStartDipSwell =
				//                                    devInfo_.Content[iObj].CurPointersDip[iPhase].Date;
				//                    devInfo_.Content[iObj].DateEndDipSwell = DateTime.Now;
				//                }
				//                if (devInfo_.Content[iObj].CurPointersSwell[iPhase].Pointer != -1)
				//                {
				//                    if (devInfo_.Content[iObj].CurPointersSwell[iPhase].Date <
				//                                    devInfo_.Content[iObj].DateStartDipSwell)
				//                        devInfo_.Content[iObj].DateStartDipSwell =
				//                                    devInfo_.Content[iObj].CurPointersSwell[iPhase].Date;
				//                    devInfo_.Content[iObj].DateEndDipSwell = DateTime.Now;
				//                }
				//            }
				//            res = true;
				//        }
				//        //else res = false;
				//    }
				//	return res;
				//}

				// анализ полученного буфера
				//for (int iRec = 0; iRec < buffer.Length / recLength; ++iRec)
				//{
				//    DateTime startDate = DateTime.MaxValue;
				//    DateTime endDate = DateTime.MinValue;
				//    short globalObjId = Conversions.bytes_2_short(
				//                                ref buffer, 20 + iRec * recLength);
				//    ContentsLineEtPQP curObj = devInfo_.Content.FindRecord(globalObjId);

				//    if (curObj != null)
				//    {
				//        curObj.DateStartDipSwell = DateTime.MaxValue;
				//        try
				//        {
				//            ushort iYear = 
				//                (ushort)(buffer[0 + iRec * recLength] + (buffer[1 + iRec * recLength] << 8));
				//            if (iYear < 2008) iYear += 2000;
				//            byte iMo = buffer[3 + iRec * recLength];
				//            byte iDay = buffer[2 + iRec * recLength];
				//            startDate = new DateTime(iYear, iMo, iDay,
				//                            buffer[5 + iRec * recLength],
				//                            buffer[4 + iRec * recLength],
				//                            buffer[6 + iRec * recLength], 0);

				//            iYear = (ushort)(buffer[10 + iRec * recLength] + (buffer[11 + iRec * recLength] << 8));
				//            if (iYear < 2008) iYear += 2000;
				//            iMo = buffer[13 + iRec * recLength];
				//            iDay = buffer[12 + iRec * recLength];
				//            endDate = new DateTime(iYear, iMo, iDay,
				//                            buffer[15 + iRec * recLength],
				//                            buffer[14 + iRec * recLength],
				//                            buffer[16 + iRec * recLength], 0);
				//        }
				//        catch (Exception e)
				//        {
				//			  EmService.DumpException(ex, "Error in GetDipSwellStartEndDates():");
				//            return false;
				//        }

				//        curObj.DateStartDipSwell = startDate;
				//        curObj.DateEndDipSwell = endDate;
				//    }
				//}

				#endregion

				// исправляем для них даты с учетом текущих провалов
				for (int iObj = 0; iObj < devInfo_.Content.Count; ++iObj)
				{
					// выставляем даты по датам объекта в целом
					devInfo_.Content[iObj].DateStartDipSwell =
						devInfo_.Content[iObj].CommonBegin;
					devInfo_.Content[iObj].DateEndDipSwell =
						devInfo_.Content[iObj].CommonEnd;

					if (devInfo_.Content[iObj].CurDnsExists())
					{
						for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
						{
							// по всем фазам читаем только 3пр или 4пр, а не обе сразу
							if ((devInfo_.Content[iObj].ConnectionScheme == ConnectScheme.Ph3W3 ||
								devInfo_.Content[iObj].ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
								&& iPhase == 6) { continue; }
							if ((devInfo_.Content[iObj].ConnectionScheme == ConnectScheme.Ph3W4 ||
								devInfo_.Content[iObj].ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
								&& iPhase == 7) { continue; }

							if (devInfo_.Content[iObj].CurPointersDip[iPhase].Pointer != -1)
							{
								if (devInfo_.Content[iObj].DateStartDipSwell == DateTime.MinValue ||
									devInfo_.Content[iObj].DateStartDipSwell >
											devInfo_.Content[iObj].CurPointersDip[iPhase].Date)
									devInfo_.Content[iObj].DateStartDipSwell =
										devInfo_.Content[iObj].CurPointersDip[iPhase].Date;
							}
							if (devInfo_.Content[iObj].CurPointersSwell[iPhase].Pointer != -1)
							{
								if (devInfo_.Content[iObj].DateStartDipSwell == DateTime.MinValue ||
									devInfo_.Content[iObj].DateStartDipSwell >
											devInfo_.Content[iObj].CurPointersSwell[iPhase].Date)
									devInfo_.Content[iObj].DateStartDipSwell =
										devInfo_.Content[iObj].CurPointersSwell[iPhase].Date;
							}
						}
						devInfo_.Content[iObj].DateEndDipSwell = DateTime.Now;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetDipSwellStartEndDates():");
				return false;
			}

			return true;
		}

		#endregion
	}
}
