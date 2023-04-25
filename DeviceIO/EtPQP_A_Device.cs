using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Resources;
using System.Windows.Forms;

using DeviceIO.Constraints;
using EmServiceLib;
using DeviceIOEmPortCSharp;

namespace DeviceIO
{
	public class EtPQP_A_Device : EmDevice
	{
		#region Constants and enums

		public const ushort CntPqpSegments = 256;
		public const uint PqpArchiveLength = 512 * 1024;
		public const ushort PqpSegmentLength = 2048;

		private const int registrationRecordLength_ = 2048;

		private const int COUNT_OBJ_NAMES = 11;
		
		#endregion

		#region Fields

		private DeviceCommonInfoEtPQP_A devInfo_ = new DeviceCommonInfoEtPQP_A();

		private static readonly int avgRecordLength_ = 16384;

		private string wifiProfileName_;
		private string wifiPassword_; 

		#endregion

		#region Properties

		public DeviceCommonInfoEtPQP_A DeviceInfo
		{
			get { return this.devInfo_; }
		}

		public static int AvgRecordLength_PQP_A
		{
			get { return avgRecordLength_; }
		}

		#endregion

		#region Constructors

		public EtPQP_A_Device(EmPortType portType, ushort devAddr, bool auto, object[] port_params,
			string wifiProfileName, string wifiPassword,
			IntPtr hMainWnd)
			: base(EmDeviceType.ETPQP_A, portType, devAddr, auto, port_params, hMainWnd)
		{
			byte[] temp_pswd = { 0x08, 0x01, 0x02, 0x03, 0x02, 0x07, 0x02, 0x01, 0x01, 0x01 };
			pswd_for_writing_ = new byte[temp_pswd.Length];
			temp_pswd.CopyTo(pswd_for_writing_, 0);

			wifiProfileName_ = wifiProfileName;
			wifiPassword_ = wifiPassword;
		}

		#endregion

		#region Public Methods

		public override int OpenDevice()
		{
			try
			{
				if(portManager_ != null) portManager_.ClosePort(true);

				if(portType_ != EmPortType.WI_FI)
					portManager_ = new PortManager(hMainWnd_, portType_, devType_, ref portParams_, bAutoMode_);
				else portManager_ = new PortManager(hMainWnd_, portType_, devType_, wifiProfileName_, wifiPassword_, 
					ref portParams_, bAutoMode_);

				if (!portManager_.CreatePort()) return -1;

				if (!portManager_.OpenPort()) return -1;

				// для работы с EtPQP сначала на всякий случай посылаем команду - сброс обработки
				// запроса усредненных. эти запросы долго обрабатываются и могут приходить пакеты
				// от прошлых запросов
				//ResetAllAvgQuery();

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
				EmService.DumpException(ex, "Error in Open EtPQP-A device:");
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
			byte[] bufferMainRecord = null;

			// creating DeviceCommonInfo object
			devInfo_ = new DeviceCommonInfoEtPQP_A();
			devInfo_.SerialNumber = serialNumber_;
			devInfo_.DevVersion = "0.0";

			try
			{
				// Чтение списка абсолютных индексов доступных Регистраций 
				if (Read(EmCommands.COMMAND_ReadRegistrationIndices, ref buffer, null) != 0)
				{
					EmService.WriteToLogFailed("COMMAND_ReadRegistrationIndices failed 1");
					return ExchangeResult.Other_Error;
				}
				if (buffer == null || buffer.Length < 4)	// 4 bytes is the length of 1 index
				{
					Thread.Sleep(1000);
					if (Read(EmCommands.COMMAND_ReadRegistrationIndices, ref buffer, null) != 0)
					{
						EmService.WriteToLogFailed("COMMAND_ReadRegistrationIndices failed 2");
						return ExchangeResult.Other_Error;
					}
					if (buffer == null || buffer.Length < 4)
						throw new EmDeviceEmptyException();
				}

				// parse buffer
				int regCount = buffer.Length / 4;
				ContentsLineEtPQP_A[] mainRecords = new ContentsLineEtPQP_A[regCount];

				UInt32[] regIndexes = new UInt32[regCount];
				for (int iInd = 0; iInd < regCount; ++iInd)
				{
					regIndexes[iInd] = Conversions.bytes_2_uint_new(ref buffer, iInd * 4);
				}

				// переменная указывает на архивы, сделанные со старой прошивкой
				// если есть старые и новые архивы, то старые игнорируем. если есть
				// только старые, то выдаем сообщение о необходимости перепрошить прибор
				bool oldArchiveExists = false;

				for (int iReg = 0; iReg < regCount; ++iReg)
				{
					mainRecords[iReg] = new ContentsLineEtPQP_A();
					mainRecords[iReg].RegistrationId = regIndexes[iReg];

					if (Read(EmCommands.COMMAND_ReadRegistrationByIndex, ref bufferMainRecord,
								new object[] { mainRecords[iReg].RegistrationId } ) != 0)
					{
						EmService.WriteToLogFailed("COMMAND_ReadRegistrationByIndex failed");
						return ExchangeResult.Other_Error;
					}
					if (bufferMainRecord.Length < registrationRecordLength_)
					{
						EmService.WriteToLogFailed("Error: reg buffer.Length = " +
							bufferMainRecord.Length.ToString());
						EmService.WriteToLogFailed("Registration number = " + iReg.ToString());
						return ExchangeResult.Other_Error;
					}

					// проверяем не слишком ли старая прошивка у прибора
					ushort ver_num = Conversions.bytes_2_ushort(ref bufferMainRecord, 122);
					if (ver_num < 1)
					{
						oldArchiveExists = true;
						EmService.WriteToLogFailed("Device version: " + ver_num.ToString());
						EmService.WriteToLogFailed(string.Format("Device version bytes: {0}, {1}", 
															bufferMainRecord[122], bufferMainRecord[123]));
						//throw new EmDeviceOldVersionException();
						continue;
					}

					// begin
					ushort zone;
					mainRecords[iReg].CommonBegin =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref bufferMainRecord, 8,
											"Start date for main record", out zone);
					mainRecords[iReg].TimeZone = zone;
					// end
					mainRecords[iReg].CommonEnd =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref bufferMainRecord, 36,
											"End date for main record");
					
					// object name
					mainRecords[iReg].ObjectName = Conversions.bytes_2_string(ref bufferMainRecord, 64, 16);
					if (mainRecords[iReg].ObjectName == "")
						mainRecords[iReg].ObjectName = "default object";

					// connection scheme
					ushort conScheme = Conversions.bytes_2_ushort(ref bufferMainRecord, 80);
					switch (conScheme)
					{
						case 0: mainRecords[iReg].ConnectionScheme = ConnectScheme.Ph1W2; break;
						case 1: mainRecords[iReg].ConnectionScheme = ConnectScheme.Ph3W4; break;
						case 2: mainRecords[iReg].ConnectionScheme = ConnectScheme.Ph3W3; break;
						case 3: mainRecords[iReg].ConnectionScheme = ConnectScheme.Ph3W3_B_calc; break;
						default: mainRecords[iReg].ConnectionScheme = ConnectScheme.Unknown; break;
					}

					// U limit
					mainRecords[iReg].SysInfo.U_Limit = Conversions.bytes_2_ushort(ref bufferMainRecord, 82);
					// I limit
					mainRecords[iReg].SysInfo.I_Limit = Conversions.bytes_2_ushort(ref bufferMainRecord, 84);
					// F limit
					mainRecords[iReg].SysInfo.F_Limit = Conversions.bytes_2_ushort(ref bufferMainRecord, 86);

					mainRecords[iReg].SysInfo.U_transformer_enable =
						(Conversions.bytes_2_ushort(ref bufferMainRecord, 88) == 0);
					mainRecords[iReg].SysInfo.U_transformer_type = 
						Conversions.bytes_2_short(ref bufferMainRecord, 90);
					mainRecords[iReg].SysInfo.I_transformer_usage = 
						Conversions.bytes_2_short(ref bufferMainRecord, 98);
					mainRecords[iReg].SysInfo.I_transformer_primary =
						Conversions.bytes_2_short(ref bufferMainRecord, 100);
					if (mainRecords[iReg].SysInfo.I_transformer_usage == 1)
						mainRecords[iReg].SysInfo.I_TransformerSecondary = 1;
					else if (mainRecords[iReg].SysInfo.I_transformer_usage == 2)
						mainRecords[iReg].SysInfo.I_TransformerSecondary = 5;

					mainRecords[iReg].SysInfo.Autocorrect_time_gps_enable = Conversions.bytes_2_ushort(ref bufferMainRecord, 104) != 0;
					mainRecords[iReg].SysInfo.Gps_Latitude = Conversions.bytes_2_double(ref bufferMainRecord, 1224);
					mainRecords[iReg].SysInfo.Gps_Longitude = Conversions.bytes_2_double(ref bufferMainRecord, 1232);

					// constraints /////////////////////////////////
					for (int iConstr = 0; iConstr < EtPQPAConstraints.CntConstraints;
						++iConstr)
					{
						mainRecords[iReg].Constraints[iConstr] =
							Conversions.bytes_2_signed_float_Q_15_16_new(ref bufferMainRecord, 128 + iConstr * 4);
					}

					// nominal f
					mainRecords[iReg].SysInfo.F_Nominal = Conversions.bytes_2_ushort(ref bufferMainRecord, 86);

					switch (mainRecords[iReg].ConnectionScheme)
					{
						case ConnectScheme.Ph1W2:
							mainRecords[iReg].SysInfo.U_NominalPhase = 220;
							mainRecords[iReg].SysInfo.U_NominalLinear = 381;
							break;
						//case ConnectScheme.Ph3W3:
						//    mainRecords[iReg].SysInfo.U_NominalPhase = 57.7F;
						//    mainRecords[iReg].SysInfo.U_NominalLinear = 100;
						//    break;
						//case ConnectScheme.Ph3W3_B_calc:
						//    mainRecords[iReg].SysInfo.U_NominalPhase = 219.4F;
						//    mainRecords[iReg].SysInfo.U_NominalLinear = 380;
						//    break;
						case ConnectScheme.Ph3W3:
						case ConnectScheme.Ph3W3_B_calc:
							if (mainRecords[iReg].SysInfo.U_transformer_enable)
							{
								mainRecords[iReg].SysInfo.U_NominalPhase = 57.7F;
								mainRecords[iReg].SysInfo.U_NominalLinear = 100;
							}
							else
							{
								mainRecords[iReg].SysInfo.U_NominalPhase = 219.4F;
								mainRecords[iReg].SysInfo.U_NominalLinear = 380;
							}
							break;
						case ConnectScheme.Ph3W4:
						case ConnectScheme.Ph3W4_B_calc:
							if (mainRecords[iReg].SysInfo.U_transformer_enable)
							{
								mainRecords[iReg].SysInfo.U_NominalPhase = 57.7F;
								mainRecords[iReg].SysInfo.U_NominalLinear = 100;
							}
							else
							{
								mainRecords[iReg].SysInfo.U_NominalPhase = 220;
								mainRecords[iReg].SysInfo.U_NominalLinear = 381;
							}
							break;
					}

					// constraint_type
					mainRecords[iReg].ConstraintType = Conversions.bytes_2_short(ref bufferMainRecord, 108);

					// marked on off
					mainRecords[iReg].SysInfo.Marked_on_off =
						(Conversions.bytes_2_ushort(ref bufferMainRecord, 1258) == 0);

					// device version //?????????????????????? зачем две версии?? (чуть ниже еще одна)
					uint verNum = Conversions.bytes_2_uint_new(ref bufferMainRecord, 124);
					string sVernum = verNum.ToString();
					mainRecords[iReg].DevVersion = sVernum;
					try
					{
						mainRecords[iReg].DevVersionDate = new DateTime(
							Int32.Parse(sVernum.Substring(sVernum.Length - 6, 2)) + 2000,
							Int32.Parse(sVernum.Substring(sVernum.Length - 4, 2)),
							Int32.Parse(sVernum.Substring(sVernum.Length - 2, 2)));
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "Error getting device version date!");
						mainRecords[iReg].DevVersionDate = DateTime.MinValue;
					}

                    #region PQP

                    // the length of pqp archives
					mainRecords[iReg].SysInfo.Pqp_length = Conversions.bytes_2_short(ref bufferMainRecord, 110);
					// count of pqp archives
					mainRecords[iReg].SysInfo.Pqp_cnt = Conversions.bytes_2_int(ref bufferMainRecord, 118);
					// pqp archive indexes
					for (int iPqp = 0; iPqp < 64; ++iPqp)
					{
						UInt32 index = Conversions.bytes_2_uint_new(ref bufferMainRecord, 944 + iPqp * 4);
						if (index == (UInt32)0xFFFFFFFF) continue;

						mainRecords[iReg].PqpSet.Add(new PqpSetEtPQP_A(index, 
							mainRecords[iReg].RegistrationId));
                        EmService.WriteToLogGeneral("PqP Index: " + index.ToString());
					}
					if (mainRecords[iReg].SysInfo.Pqp_cnt > mainRecords[iReg].PqpSet.Count)
					{
						EmService.WriteToLogFailed("PqpCount = " +
							mainRecords[iReg].SysInfo.Pqp_cnt.ToString() +
							" but PqpSet.Count = " + mainRecords[iReg].PqpSet.Count.ToString());
						mainRecords[iReg].SysInfo.Pqp_cnt = (ushort)mainRecords[iReg].PqpSet.Count;
					}

					// read the first segment of each pqp archive to get its start date
					for (int iPqpDate = 0; iPqpDate < mainRecords[iReg].PqpSet.Count; ++iPqpDate)
					{
						if (Read(EmCommands.COMMAND_ReadRegistrationArchiveByIndex, ref buffer,
							new object[] { mainRecords[iReg].PqpSet[iPqpDate].PqpIndex, 0 }) != 0)
						{
							EmService.WriteToLogFailed(
								"COMMAND_ReadRegistrationArchiveByIndex failed  " +
								iPqpDate.ToString());
							mainRecords[iReg].PqpSet.RemoveAt(iPqpDate);
							--iPqpDate;
							continue;
							//return -1;
						}
						if (buffer == null || buffer.Length < (EtPQP_A_Device.PqpSegmentLength + 6))
						{
							mainRecords[iReg].PqpSet.RemoveAt(iPqpDate);
							--iPqpDate;
							continue;
						}

						// убираем первые 6 байт, в которых archive_id и номер сегмента
						byte[] buffer_old = buffer;
						buffer = new byte[EtPQP_A_Device.PqpSegmentLength];
						Array.Copy(buffer_old, 6, buffer, 0, EtPQP_A_Device.PqpSegmentLength);

						PqpSetEtPQP_A curPQP = mainRecords[iReg].PqpSet[iPqpDate];
						curPQP.PqpStart =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 12,
									"PQP Start date");
						curPQP.PqpEnd =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 84,
									"PQP End date");
						mainRecords[iReg].PqpSet[iPqpDate] = curPQP;
                    }

                    #endregion

                    #region AVG info

					//mainRecords[iReg].AvgIndexStart3sec =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1192*/1200);
					//mainRecords[iReg].AvgCnt3sec =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1196*/1204);

					//// еще не прошли цикл
					//// UPD: про цикл закомментировано, т.к. Боря сказал, что пока мы живы до значения 0xFFFFFFFF
					//// дело не успеет дойти
					////if ((0xFFFFFFFF - mainRecords[iReg].AvgIndexStart3sec) > mainRecords[iReg].AvgCnt3sec)
					//mainRecords[iReg].AvgIndexEnd3sec =
					//        mainRecords[iReg].AvgIndexStart3sec + mainRecords[iReg].AvgCnt3sec - 1;
					////else // прошли цикл, нумерация началась сначала
					////{
					////    EmService.WriteToLogDebug("AVG new circle!!!");
					////    mainRecords[iReg].AvgIndexEnd3sec =
					////        mainRecords[iReg].AvgCnt3sec - (0xFFFFFFFF - mainRecords[iReg].AvgIndexStart3sec);
					////}

					//mainRecords[iReg].AvgIndexStart10min =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1200*/1208);
					//mainRecords[iReg].AvgCnt10min =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1204*/1212);
					//mainRecords[iReg].AvgIndexEnd10min =
					//        mainRecords[iReg].AvgIndexStart10min + mainRecords[iReg].AvgCnt10min - 1;

					//mainRecords[iReg].AvgIndexStart2hour =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1208*/1216);
					//mainRecords[iReg].AvgCnt2hour =
					//    Conversions.bytes_2_uint_new(ref bufferMainRecord, /*1212*/1220);
					//mainRecords[iReg].AvgIndexEnd2hour =
					//        mainRecords[iReg].AvgIndexStart2hour + mainRecords[iReg].AvgCnt2hour - 1;
                   
					// временный массив, чтобы можно было 3 типа усредненных считывать в цикле
					EmCommands[] avgCommandsDate = new EmCommands[] { EmCommands.COMMAND_ReadAverageArchive3SecByIndex,
					    EmCommands.COMMAND_ReadAverageArchive10MinByIndex,
					    EmCommands.COMMAND_ReadAverageArchive2HourByIndex };
					EmCommands[] avgCommandsRealIndex = new EmCommands[] { 
						EmCommands.COMMAND_ReadAverageArchive3SecMinMaxIndices,
					    EmCommands.COMMAND_ReadAverageArchive10MinMinMaxIndices,
					    EmCommands.COMMAND_ReadAverageArchive2HourMinMaxIndices };
					//UInt32[] indexesStart = new UInt32[] { mainRecords[iReg].AvgIndexStart3sec,
					//    mainRecords[iReg].AvgIndexStart10min, mainRecords[iReg].AvgIndexStart2hour };
					//UInt32[] indexesEnd = new UInt32[] { mainRecords[iReg].AvgIndexEnd3sec,
					//    mainRecords[iReg].AvgIndexEnd10min, mainRecords[iReg].AvgIndexEnd2hour };
					//UInt32[] cntAvg = new UInt32[] { mainRecords[iReg].AvgCnt3sec,
					//    mainRecords[iReg].AvgCnt10min, mainRecords[iReg].AvgCnt2hour };

					for (int iAvgType = 0; iAvgType < 3; ++iAvgType)
					{
						// сначала узнаем стартовый и конечный фактические индексы
						if (Read(avgCommandsRealIndex[iAvgType], ref buffer,
										new object[] { mainRecords[iReg].RegistrationId }) != 0)
						{
							EmService.WriteToLogFailed(
								"ReadDevInfo:COMMAND_ReadAverageArchiveXXXMinMaxIndices failed1! " +
								iAvgType.ToString());
							if (buffer != null) EmService.WriteToLogFailed("buffer length = " + buffer.Length);
							else EmService.WriteToLogFailed("buffer = null");
							continue;
						}

						if (buffer == null || buffer.Length < 120)
						{
							EmService.WriteToLogFailed(
								"ReadDevInfo:COMMAND_ReadAverageArchiveXXXMinMaxIndices failed2!  " +
								iAvgType.ToString());
							if (buffer != null) EmService.WriteToLogFailed("buffer length = " + buffer.Length);
							else EmService.WriteToLogFailed("buffer = null");
							continue;
						}

						// стартовый индекс
						UInt32 curIndex = Conversions.bytes_2_uint_new(ref buffer, 0);

						DateTime start_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 4, "AVG record start date1");
						DateTime end_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 32, "AVG record end date1");

						mainRecords[iReg].AvgDataStart[iAvgType] = new AVGDataEtPQP_A(curIndex,
							start_datetime, end_datetime);

						// конечный индекс
						curIndex = Conversions.bytes_2_uint_new(ref buffer, 60);

						start_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 64, "AVG record start date2");
						end_datetime =
							Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 92, "AVG record end date2");

						mainRecords[iReg].AvgDataEnd[iAvgType] = new AVGDataEtPQP_A(curIndex,
							start_datetime, end_datetime);

						//if (cntAvg[iAvgType] == 0) continue;

						//// start
						//if (Read(avgCommandsDate[iAvgType], ref buffer,
						//                new object[] { indexesStart[iAvgType] }) != 0)
						//{
						//    EmService.WriteToLogFailed("ReadDevInfo:COMMAND_ReadAverageArchiveXXXXByIndex failed1! " +
						//        iAvgType.ToString());
						//    EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						//    continue;
						//}
						//if (buffer == null || buffer.Length < avgRecordLength_)
						//{
						//    EmService.WriteToLogFailed("ReadDevInfo:COMMAND_ReadAverageArchiveXXXXByIndex failed2!  " +
						//        iAvgType.ToString());
						//    if (buffer != null) EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						//    else EmService.WriteToLogFailed("buffer = null");
						//    continue;
						//}

						//DateTime start_datetime =
						//    Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 12, "AVG record start date");
						//DateTime end_datetime =
						//    Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 40, "AVG record end date");
						//mainRecords[iReg].AvgDataStart[iAvgType] = new AVGDataEtPQP_A(indexesStart[iAvgType],
						//    start_datetime, end_datetime);
						//UInt32 curIndex = Conversions.bytes_2_uint_new(ref buffer, 0);
						//if (curIndex != indexesStart[iAvgType])
						//{
						//    EmService.WriteToLogFailed(string.Format("ReadDevInfo error 111: {0}, {1}",
						//        indexesStart[iAvgType], curIndex));
						//}

						//// end
						//if (Read(avgCommandsDate[iAvgType], ref buffer, new object[] { indexesEnd[iAvgType] }) != 0)
						//{
						//    EmService.WriteToLogFailed("ReadDevInfo:COMMAND_ReadAverageArchiveXXXXByIndex failed3! " +
						//        iAvgType.ToString());
						//    EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						//    continue;
						//}
						//if (buffer == null || buffer.Length < avgRecordLength_)
						//{
						//    EmService.WriteToLogFailed("ReadDevInfo:COMMAND_ReadAverageArchiveXXXXByIndex failed4!  " +
						//        iAvgType.ToString());
						//    if (buffer != null) EmService.WriteToLogFailed("buffer length = " + buffer.Length);
						//    else EmService.WriteToLogFailed("buffer = null");
						//    continue;
						//}

						//start_datetime = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 12, 
						//    "AVG record start date");
						//end_datetime = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 40, "AVG record end date");
						//mainRecords[iReg].AvgDataEnd[iAvgType] = new AVGDataEtPQP_A(indexesEnd[iAvgType],
						//    start_datetime, end_datetime);
						//curIndex = Conversions.bytes_2_uint_new(ref buffer, 0);
						//if (curIndex != indexesEnd[iAvgType])
						//{
						//    EmService.WriteToLogFailed(string.Format("ReadDevInfo error 222: {0}, {1}",
						//        indexesEnd[iAvgType], curIndex));
						//}
					}

					if (mainRecords[iReg].AvgDataStart.Length >= 3 && mainRecords[iReg].AvgDataEnd.Length >= 3)
					{
						if ((mainRecords[iReg].AvgDataStart[0].dtStart != DateTime.MinValue &&
							mainRecords[iReg].AvgDataEnd[0].dtStart != DateTime.MinValue) ||
							(mainRecords[iReg].AvgDataStart[1].dtStart != DateTime.MinValue &&
							mainRecords[iReg].AvgDataEnd[1].dtStart != DateTime.MinValue) ||
							(mainRecords[iReg].AvgDataStart[2].dtStart != DateTime.MinValue &&
							mainRecords[iReg].AvgDataEnd[2].dtStart != DateTime.MinValue))
							mainRecords[iReg].AvgExists = true;
					}

					#endregion

					devInfo_.Content.AddRecord(mainRecords[iReg]);
				}

				if (oldArchiveExists && devInfo_.Content.Count == 0)
				{
					throw new EmDeviceOldVersionException();
				}

				// device version
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)364 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read Device version!");
				}
				else
				{
					uint verNum = Conversions.bytes_2_uint_new(ref buffer, 0);
					string sVernum = verNum.ToString();
					devInfo_.DevVersion = sVernum;
					try
					{
						devInfo_.DevVersionDate = new DateTime(
							Int32.Parse(sVernum.Substring(sVernum.Length - 6, 2)) + 2000,
							Int32.Parse(sVernum.Substring(sVernum.Length - 4, 2)),
							Int32.Parse(sVernum.Substring(sVernum.Length - 2, 2)));
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "Error getting device version date!");
						devInfo_.DevVersionDate = DateTime.MinValue;
					}

					//if (dtBuildDate < new DateTime(2011, 1, 15))
					//{
					//    throw new EmDeviceOldVersionException();
					//}
				}
				// end of device version
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

		public ExchangeResult ReadAvgArchive(ref byte[] buffer, DateTime dtStart, DateTime dtEnd, 
									UInt32 indexStart, UInt32 indexEnd, AvgTypes_PQP_A avgType,
									ref ContentsLineEtPQP_A cl)
		{
			try
			{
                //DateTime dtTempStart = DateTime.MaxValue;
                //DateTime dtTempEnd = DateTime.MinValue;
                //int interval = 7200;
				EmCommands curCommand = EmCommands.COMMAND_ReadAverageArchive3SecByIndex;
                ExchangeResult errCode = ExchangeResult.Other_Error;
                byte[] tempBuffer = null;
                List<byte> listTempBuffer = new List<byte>();

                switch (avgType)
                {
                    case AvgTypes_PQP_A.ThreeSec:
						curCommand = EmCommands.COMMAND_ReadAverageArchive3SecByIndex;
                        //interval = 3;
                        break;
                    case AvgTypes_PQP_A.TenMin:
						curCommand = EmCommands.COMMAND_ReadAverageArchive10MinByIndex;
                        //interval = 600;
                        break;
                    case AvgTypes_PQP_A.TwoHours:
						curCommand = EmCommands.COMMAND_ReadAverageArchive2HourByIndex;
                        //interval = 7200;
                        break;
                }
                //while (dtTempStart < dtStart) dtTempStart = dtTempStart.AddSeconds(interval);

				int cnt_error = 0;
                for (UInt32 iIndex = indexStart; iIndex <= indexEnd; ++iIndex)
                {
					if (bCancelReading_) throw new EmDisconnectException();

                    tempBuffer = null;
					EmService.WriteToLogGeneral("Reading AVG record, Index =  " + iIndex.ToString());
					errCode = Read(curCommand, ref tempBuffer, new object[] { iIndex });

					if (errCode == ExchangeResult.OK && tempBuffer != null && tempBuffer.Length >= avgRecordLength_)
					{
						listTempBuffer.AddRange(tempBuffer);
						cnt_error = 0;
					}
					else
					{
						EmService.WriteToLogFailed("Error reading AVG record (EtPQP-A):  " + iIndex.ToString());
						if (cnt_error >= 5) break; //return -1;
						cnt_error++;
					}

                    // считана очерендная запись, поэтому меняем прогрессбар
                    if (OnStepReading != null) OnStepReading(EmDeviceType.ETPQP_A);
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

		public ExchangeResult ReadDipSwellArchive(ref byte[] buffer, UInt32 registrationId)
		{
			try
			{
				ExchangeResult errCode = Read(EmCommands.COMMAND_ReadDSIArchivesByRegistration, ref buffer,
									new object[] { registrationId });

				// если был дисконнект, то выходим
				//if (errCode == -2) throw new EmDisconnectException();

				if (errCode != ExchangeResult.OK)
				{
					EmService.WriteToLogFailed("Error reading DNS archive (EtPQP-A)");
				}
				return errCode;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDipSwellArchive():");
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

				int curParamNumber = 56;
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

		/// <summary>
		/// read momentary data
		/// </summary>
		/// <param name="buffer">buffer to receive data</param>
		/// <param name="type">1 - 3 sec, 2 - 1 min, 3 - 30 min</param>
		/// <param name="devInfo">device info</param>
		public ExchangeResult ReadMomentData(ref byte[] buffer, AvgTypes_PQP_A type, ref BaseDeviceCommonInfo devInfo)
		{
			try
			{
				byte[] tmpBuffer = null;
				// connection scheme
				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, new object[] { 
									(ushort)24 }) != ExchangeResult.OK)
				    return ExchangeResult.Other_Error;
				if (tmpBuffer == null)
				{
					EmService.WriteToLogFailed("Moment data: Unable to read curConnectScheme!");
				}
				else
				{
					ushort conScheme = Conversions.bytes_2_ushort(ref tmpBuffer, 0);
					switch (conScheme)
					{
						case 0: devInfo.ConnectionScheme = ConnectScheme.Ph1W2; break;
						case 1: devInfo.ConnectionScheme = ConnectScheme.Ph3W4; break;
						case 2: devInfo.ConnectionScheme = ConnectScheme.Ph3W3; break;
						case 3: devInfo.ConnectionScheme = ConnectScheme.Ph3W3_B_calc; break;
						default: devInfo.ConnectionScheme = ConnectScheme.Unknown; break;
					}
				}

				// read nominal values:

				#region F nominal

				// f_nominal
				//if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { 
									//(ushort)4 }) != ExchangeResult.OK)
				//    return ExchangeResult.Other_Error;
				//if (buffer == null || buffer.Length < 4)
				//{
				//    EmService.WriteToLogFailed("Unable to read f_nominal!");
				//    devInfo.F_Nominal = 0;
				//}
				//else
				//{
				//    devInfo.F_Nominal = (Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true));
				//}
				// end of f_nominal

				#endregion

				// u_nominal
				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, new object[] { 
									(ushort)158 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (tmpBuffer == null)
				{
					EmService.WriteToLogFailed("Moment data: Unable to read U_transformer_enable!");
				}
				else
				{
					devInfo.U_transformer_enable = Conversions.bytes_2_ushort(ref tmpBuffer, 0) != 0;
				}

				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, new object[] { 
									(ushort)159 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (tmpBuffer == null)
				{
					EmService.WriteToLogFailed("Moment data: Unable to read U_transformer_type!");
				}
				else
				{
					devInfo.U_transformer_type = Conversions.bytes_2_short(ref tmpBuffer, 0);
				}
				// calc u_nominal
				switch (devInfo.ConnectionScheme)
				{
					case ConnectScheme.Ph1W2:
						devInfo.U_NominalPhase = 220;
						devInfo.U_NominalLinear = 381;
						break;
					//case ConnectScheme.Ph3W3:
					//    mainRecords[iReg].SysInfo.U_NominalPhase = 57.7F;
					//    mainRecords[iReg].SysInfo.U_NominalLinear = 100;
					//    break;
					//case ConnectScheme.Ph3W3_B_calc:
					//    mainRecords[iReg].SysInfo.U_NominalPhase = 219.4F;
					//    mainRecords[iReg].SysInfo.U_NominalLinear = 380;
					//    break;
					case ConnectScheme.Ph3W3:
					case ConnectScheme.Ph3W3_B_calc:
						if (devInfo.U_transformer_enable)
						{
							devInfo.U_NominalPhase = 57.7F;
							devInfo.U_NominalLinear = 100;
						}
						else
						{
							devInfo.U_NominalPhase = 219.4F;
							devInfo.U_NominalLinear = 380;
						}
						break;
					case ConnectScheme.Ph3W4:
					case ConnectScheme.Ph3W4_B_calc:
						if (devInfo.U_transformer_enable)
						{
							devInfo.U_NominalPhase = 57.7F;
							devInfo.U_NominalLinear = 100;
						}
						else
						{
							devInfo.U_NominalPhase = 220;
							devInfo.U_NominalLinear = 381;
						}
						break;
				}
				// end of u_nominal

				#region I nominal

				// i_nominal_phase
				/*if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)17 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read i_nominal_phase!");
					devInfo.I_NominalPhase = 0;
				}
				else
				{
					devInfo.I_NominalPhase = (Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true));
				}*/
				// end of i_nominal_phase

				#endregion

				#region Object Name

				// object name
				/*if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)33 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (buffer == null)
				{
					devInfo.ObjectName = "";
					EmService.WriteToLogFailed("Unable to read Object name!");
				}
				else
				{
					Encoding enc = Encoding.GetEncoding(0);
					char[] cars = enc.GetChars(buffer);
					devInfo.ObjectName = new string(cars);
					devInfo.ObjectName = devInfo.ObjectName.Trim('\0');
				}*/
				// end of object name

				#endregion

				// read AVG archive
				EmCommands command;
				switch (type)
				{
					case AvgTypes_PQP_A.ThreeSec: command = EmCommands.COMMAND_ReadMeasurements3Sec; break;
					case AvgTypes_PQP_A.TenMin: command = EmCommands.COMMAND_ReadMeasurements10Min; break;
					case AvgTypes_PQP_A.TwoHours: command = EmCommands.COMMAND_ReadMeasurements2Hour; break;
					default: command = EmCommands.COMMAND_ReadMeasurements3Sec; break;
				}
				if (Read(command, ref buffer, null) != 0) return ExchangeResult.Other_Error;

				if (buffer == null)
					throw new EmDeviceEmptyException();
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return ExchangeResult.Other_Error;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadMomentData():  ");
				throw;
			}
			return ExchangeResult.OK;
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
				buffer = null;
				int num = 0;
				ExchangeResult res = ExchangeResult.Other_Error;

				List<UInt32> listParams;
				MakeIntList(ref request_params, out listParams);

				//List<byte> listBuffer = null;
				while (++num <= attempts)
				{
					if (bCancelReading_) throw new EmDisconnectException();

					//listBuffer = new List<byte>();
					//res = portA_.ReadData(command, listBuffer, listParams);
					res = portManager_.ReadData(command, ref buffer, listParams);

					if (res != 0)
					{
						EmService.WriteToLogFailed("Reading error. Attempt " + num);
						Thread.Sleep(1000);
					}
					if (res == ExchangeResult.OK)  // если успешно
						break;
					if (res == ExchangeResult.Disconnect_Error)  // если был дисконнект
					{
						portManager_.ClosePort(true);

						if (bCancelReading_)
							throw new EmDisconnectException();
						else
						{
							EmService.WriteToLogFailed("Error reading (EtPQP-A): OpenFast");
							if (!portManager_.OpenFast(true)) throw new EmDisconnectException();
							Thread.Sleep(1000);
						}
					}

					if (num >= 2)
					{
						if (portType_ == EmPortType.USB)
						{
							RestartUSB();
						}
						portManager_.OpenFast(false);
					}
				}
				//if (listBuffer != null && listBuffer.Count > 0)
				//{
				//    buffer = new byte[listBuffer.Count];
				//    listBuffer.CopyTo(buffer);
				//}
				return res;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQP_A_Device::Read():");
				throw;
			}
		}

		/// <summary>Write Constraints To the Device</summary>
		public ExchangeResult WriteSets(ref byte[] buffer, Int32 checkSum1, Int32 checkSum2)
		{
			try
			{
				if (buffer == null) return ExchangeResult.Other_Error;
				
				//List<byte> tempList = null;
				int shift = 1600;	//госты писать не надо! их пропускаем
				byte[] tempBuf = new byte[6];   // param (2) + data (4)
				int param_num = 164;

				///////////////////test//////////////
				//param_num = 166;
				//tempBuf[0] = (byte)(param_num & 0xFF);
				//tempBuf[1] = (byte)((param_num >> 8) & 0xFF);
				//tempBuf[2] = 204;
				//tempBuf[3] = 76;
				//tempBuf[4] = 0;
				//tempBuf[5] = 0;
				//tempList = new List<byte>(tempBuf);
				//if (((EmPortSLIPWrap)port_).WriteData(COMMAND_WriteSystemData, tempList,
				//        param_num /*dummy!*/) != 0)
				//{
				//    //EmService.WriteToLogFailed(
				//        //"Error while writing constraints Et-PQP-A: i = " + iConstr.ToString());
				//    return -1;
				//}

				//param_num = 262;
				//tempBuf[0] = (byte)(param_num & 0xFF);
				//tempBuf[1] = (byte)((param_num >> 8) & 0xFF);
				//tempBuf[2] = 106;
				//tempBuf[3] = 162;
				//tempBuf[4] = 211;
				//tempBuf[5] = 35;
				//tempList = new List<byte>(tempBuf);
				//if (((EmPortSLIPWrap)port_).WriteData(COMMAND_WriteSystemData, tempList,
				//        param_num /*dummy!*/) != 0)
				//{
				//    //EmService.WriteToLogFailed(
				//    //	"Error while writing constraints Et-PQP-A: i = " + iConstr.ToString());
				//    return -1;
				//}
				//return 0;
				///////////////////////////////////////

				for (int iConstr = 0; iConstr < (EtPQPAConstraints.CntConstraints * 2); ++iConstr)
				{
					// эта чехарда с номерами - оттого, что в приборе пришлось добавить 2
					// уставки для напряжения и теперь в приборе они идут не подряд
					if (iConstr == 10) { param_num = 386; }
					if (iConstr == 11) { param_num = 387; }
					if (iConstr == 12) { param_num = 174; }
					if (iConstr == 110) { param_num = 388; }
					if (iConstr == 111) { param_num = 389; }
					if (iConstr == 112) { param_num = 273; }

					tempBuf[0] = (byte)(param_num & 0xFF);
					tempBuf[1] = (byte)((param_num >> 8) & 0xFF);
					Array.Copy(buffer, shift, tempBuf, 2, 4);
					//tempList = new List<byte>(tempBuf);
					if (portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref tempBuf) != 0)
					{
						EmService.WriteToLogFailed(
							"Error while writing constraints Et-PQP-A: i = " + iConstr.ToString());
						return ExchangeResult.Write_Error;
					}

					shift += 4;
					++param_num;

					if (param_num == 262)		// check sum for user1
					{
						tempBuf[0] = (byte)(param_num & 0xFF);
						tempBuf[1] = (byte)((param_num >> 8) & 0xFF);
						Conversions.int_2_bytes(checkSum1, ref tempBuf, 2);
						//tempList = new List<byte>(tempBuf);
						if (portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref tempBuf) != 0)
						{
							EmService.WriteToLogFailed(
                                "Error while writing constraints Et-PQP-A: checkSum1");
							return ExchangeResult.Write_Error;
						}

						++param_num;
					}

					if (param_num == 361)		// check sum for user2
					{
						tempBuf[0] = (byte)(param_num & 0xFF);
						tempBuf[1] = (byte)((param_num >> 8) & 0xFF);
						Conversions.int_2_bytes(checkSum2, ref tempBuf, 2);
						//tempList = new List<byte>(tempBuf);
						if (portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref tempBuf) != 0)
						{
							EmService.WriteToLogFailed(
                                "Error while writing constraints Et-PQP-A: checkSum2");
							return ExchangeResult.Write_Error;
						}

						++param_num;
					}
				}

				return ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQP_A_Device::WriteSets():");
				throw;
			}
		}

		public ExchangeResult WriteObjectNames(ref byte[] buffer)
		{
			try
			{
				byte[] tempBuf;
				int oneNameLen = 16;
				int curParamNumber = 56;
				int curShift = 0;
				for (int iName = 0; iName < COUNT_OBJ_NAMES; ++iName)
				{
					tempBuf = new byte[oneNameLen];
					Array.Copy(buffer, curShift, tempBuf, 0, tempBuf.Length);
					// вместо нулей вставляем пробелы, а то прибор выдает BAD_DATA
					for (int iByte = 0; iByte < tempBuf.Length; ++iByte)
						if (tempBuf[iByte] == 0) tempBuf[iByte] = 32;

					// write to device
					ExchangeResult res = WriteSystemData(curParamNumber++, ref tempBuf);
					if (res != ExchangeResult.OK) return res;

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
				EmService.DumpException(ex, "Error in EtPQP_A_Device::WriteObjectNames():");
				throw;
			}
		}

		/// <summary>Write System Data To the Device</summary>
		/// <param name="param">Number of Sytem Parameter To Write</param>
		/// <param name="buffer">Value of this Parameter in Bytes</param>
		public ExchangeResult WriteSystemData(int param, ref byte[] buffer)
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

				//List<byte> tempList = new List<byte>(buffer_to_write);
				//return ((EtPqpAUSB)portA_).WriteData(COMMAND_WriteSystemData, tempList);
				return portManager_.WriteData(EmCommands.COMMAND_WriteSystemData, ref buffer_to_write);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQP_A_Device::Write():");
				throw;
			}
		}

		public static float GetUTransformerMultiplier(short type)
		{
			switch (type)
			{
				case 0: return 1;
				case 1: return 30;
				case 2: return 31.5F;
				case 3: return 33;
				case 4: return 60;
				case 5: return 66;
				case 6: return 100;
				case 7: return 105;
				case 8: return 110;
				case 9: return 138;
				case 10: return 150;
				case 11: return 157.5F;
				case 12: return 180;
				case 13: return 200;
				case 14: return 240;
				case 15: return 270;
				case 16: return 350;
				case 17: return 1100;
				case 18: return 1500;
				case 19: return 2200;
				case 20: return 3300;
				case 21: return 5000;
				case 22: return 7500;
				case 23: return 8;
				case 24: return 330;
			}

			return 1;
		}

		public void RestartUSB()
		{
			try
			{
				EmService.WriteToLogGeneral("!!!!!!!!!!! Restart USB !!!!!!!!!!!!!");

				//byte[] time_buf = new byte[20];
				//if (!ReadTime(ref time_buf))
				//{
				//    EmService.WriteToLogFailed("Error in RestartUSB 1");
				//    return;
				//}
				//ushort[] hash_buf = new ushort[10];
				//CalcHashForWriting(ref time_buf, ref hash_buf);
				//byte[] buffer_to_write = new byte[66];
				//for (int iBuf = 0; iBuf < 66; ++iBuf) buffer_to_write[iBuf] = 0;
				//EmService.CopyUShortArrayToByteArray(ref hash_buf, ref buffer_to_write, 0);

				//List<byte> tempList = new List<byte>(buffer_to_write);
				//return ((EtPqpAUSB)portA_).WriteData(COMMAND_WriteSystemData, tempList);

				byte[] buffer = null;
				portManager_.WriteData(EmCommands.COMMAND_RestartInterface, ref buffer);
				Thread.Sleep(3000);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPQP_A_Device::RestartUSB():");
				throw;
			}
		}

		#endregion

		#region Private Methods

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
			//ContentsLineEtPQP_A curObj = null;
			try
			{
				//int globalObjId = Conversions.bytes_2_ushort(ref buffer, 10);
				//curObj = devInfo_.Content.FindRecord(globalObjId);
				//if (curObj == null)
				//    throw new EmException("object was not found!");

				//devInfo_.GlobalIdObjectOfCurDNS = globalObjId;
				//curObj.BufCurDipSwellData = buffer;

				//ushort status = Conversions.bytes_2_ushort(ref buffer, 8);

				//ushort curStatus = 0x0001;
				//ushort curOffset = 12;
				//for (int iPhase = 0; iPhase < /*EmService.CountPhases*/ 6; ++iPhase)
				//{
				//    if ((status & curStatus) != 0)		// dip
				//    {
				//        curObj.CurPointersDip[iPhase].Pointer =
				//                Conversions.bytes_2_uint_new(ref buffer, curOffset);
				//        ushort iYear = (ushort)(buffer[curOffset + 4] + (buffer[curOffset + 5] << 8));
				//        curObj.CurPointersDip[iPhase].Date =
				//                new DateTime(iYear, buffer[curOffset + 7],
				//                        buffer[curOffset + 6], buffer[curOffset + 9],
				//                        buffer[curOffset + 8], buffer[curOffset + 11]);
				//    }
				//    else if ((status & (curStatus * 2)) != 0)		// swell
				//    {
				//        curObj.CurPointersSwell[iPhase].Pointer =
				//            Conversions.bytes_2_uint_new(ref buffer, curOffset);
				//        ushort iYear = (ushort)(buffer[curOffset + 4] + (buffer[curOffset + 5] << 8));
				//        curObj.CurPointersSwell[iPhase].Date =
				//                new DateTime(iYear, buffer[curOffset + 7],
				//                        buffer[curOffset + 6], buffer[curOffset + 9],
				//                        buffer[curOffset + 8], buffer[curOffset + 11]);
				//    }
				//    curStatus *= 4;
				//    curOffset += 16;
				//}

				//if (curObj.ConnectionScheme == ConnectScheme.Ph3W3 ||
				//    curObj.ConnectionScheme == ConnectScheme.Ph3W3_B_calc) // 3пр
				//{
				//    if ((status & 0x4000) != 0)
				//    {
				//        curObj.CurPointersDip[(int)Phase.ABC].Pointer =
				//                                Conversions.bytes_2_uint_new(ref buffer, 140);
				//        ushort iYear = (ushort)(buffer[140 + 4] + (buffer[140 + 5] << 8));
				//        curObj.CurPointersDip[(int)Phase.ABC].Date =
				//                new DateTime(iYear, buffer[140 + 7],
				//                        buffer[140 + 6], buffer[140 + 9],
				//                        buffer[140 + 8], buffer[140 + 11]);
				//    }
				//    if ((status & 0x8000) != 0)
				//    {
				//        curObj.CurPointersSwell[(int)Phase.ABC].Pointer =
				//                                Conversions.bytes_2_uint_new(ref buffer, 156);
				//        ushort iYear = (ushort)(buffer[156 + 4] + (buffer[156 + 5] << 8));
				//        curObj.CurPointersSwell[(int)Phase.ABC].Date =
				//                new DateTime(iYear, buffer[156 + 7],
				//                        buffer[156 + 6], buffer[156 + 9],
				//                        buffer[156 + 8], buffer[156 + 11]);
				//    }
				//}
				//else if (curObj.ConnectionScheme == ConnectScheme.Ph3W4 ||
				//            curObj.ConnectionScheme == ConnectScheme.Ph3W4_B_calc)  // 4 пр
				//{
				//    if ((status & 0x1000) != 0)
				//    {
				//        curObj.CurPointersDip[(int)Phase.ABCN].Pointer =
				//                                Conversions.bytes_2_uint_new(ref buffer, 108);
				//        ushort iYear = (ushort)(buffer[108 + 4] + (buffer[108 + 5] << 8));
				//        curObj.CurPointersDip[(int)Phase.ABCN].Date =
				//                new DateTime(iYear, buffer[108 + 7],
				//                        buffer[108 + 6], buffer[108 + 9],
				//                        buffer[108 + 8], buffer[108 + 11]);
				//    }
				//    if ((status & 0x2000) != 0)
				//    {
				//        curObj.CurPointersSwell[(int)Phase.ABCN].Pointer =
				//                                Conversions.bytes_2_uint_new(ref buffer, 124);
				//        ushort iYear = (ushort)(buffer[124 + 4] + (buffer[124 + 5] << 8));
				//        curObj.CurPointersSwell[(int)Phase.ABCN].Date =
				//                new DateTime(iYear, buffer[124 + 7],
				//                        buffer[124 + 6], buffer[124 + 9],
				//                        buffer[124 + 8], buffer[124 + 11]);
				//    }
				//}

				//if (!curObj.CurDnsExists())
				//{
				//    curObj.BufCurDipSwellData = null;
				//    devInfo_.GlobalIdObjectOfCurDNS = -1;
				//}

				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ParseDipSwellStatusBuffer():");
				//devInfo_.GlobalIdObjectOfCurDNS = -1;
				//if (curObj != null) curObj.ResetCurDns();
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
				//for (int iObj = 0; iObj < devInfo_.Content.Count; ++iObj)
				//{
				//    // выставляем даты по датам объекта в целом
				//    devInfo_.Content[iObj].DateStartDipSwell =
				//        devInfo_.Content[iObj].CommonBegin;
				//    devInfo_.Content[iObj].DateEndDipSwell =
				//        devInfo_.Content[iObj].CommonEnd;

				//    if (devInfo_.Content[iObj].CurDnsExists())
				//    {
				//        for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				//        {
				//            // по всем фазам читаем только 3пр или 4пр, а не обе сразу
				//            if ((devInfo_.Content[iObj].ConnectionScheme == ConnectScheme.Ph3W3 ||
				//                devInfo_.Content[iObj].ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				//                && iPhase == 6) { continue; }
				//            if ((devInfo_.Content[iObj].ConnectionScheme == ConnectScheme.Ph3W4 ||
				//                devInfo_.Content[iObj].ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
				//                && iPhase == 7) { continue; }

				//            if (devInfo_.Content[iObj].CurPointersDip[iPhase].Pointer != -1)
				//            {
				//                if (devInfo_.Content[iObj].DateStartDipSwell == DateTime.MinValue ||
				//                    devInfo_.Content[iObj].DateStartDipSwell >
				//                            devInfo_.Content[iObj].CurPointersDip[iPhase].Date)
				//                    devInfo_.Content[iObj].DateStartDipSwell =
				//                        devInfo_.Content[iObj].CurPointersDip[iPhase].Date;
				//            }
				//            if (devInfo_.Content[iObj].CurPointersSwell[iPhase].Pointer != -1)
				//            {
				//                if (devInfo_.Content[iObj].DateStartDipSwell == DateTime.MinValue ||
				//                    devInfo_.Content[iObj].DateStartDipSwell >
				//                            devInfo_.Content[iObj].CurPointersSwell[iPhase].Date)
				//                    devInfo_.Content[iObj].DateStartDipSwell =
				//                        devInfo_.Content[iObj].CurPointersSwell[iPhase].Date;
				//            }
				//        }
				//        devInfo_.Content[iObj].DateEndDipSwell = DateTime.Now;
				//    }
				//}
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
