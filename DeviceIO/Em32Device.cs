using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using EmServiceLib;

namespace DeviceIO
{
	public class Em32Device : EmDevice
	{
		#region Constants

		// максимальное число записей в архиве DnS
		const int cntRecordsDnS_ = 16384;
		// максимальное число параметров при запросе архива усреденных
		const int maxNumAvgFieldsQuery_ = 2048;

		private const int avgArchiveLength_ = 4096;

		#endregion

		#region Fields

		DeviceCommonInfoEm32 devInfo_ = new DeviceCommonInfoEm32();

		static ushort id_avg_query_ = 1;

		#endregion

		#region Properties

		public DeviceCommonInfoEm32 DeviceInfo
		{
			get { return this.devInfo_; }
		}

		#endregion

		#region Constructors

		public Em32Device(EmPortType portType, ushort devAddr, bool auto, object[] port_params,
			IntPtr hMainWnd)
			: base(EmDeviceType.EM32, portType, devAddr, auto, port_params, hMainWnd)
		{
			byte[] temp_pswd = { 0x09, 0x02, 0x01, 0x07, 0x08, 0x08, 0x00, 0x08, 0x04, 0x06 };
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

				// для работы с Em32 сначала на всякий случай посылаем команду - сброс обработки
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
				EmService.DumpException(ex, "Error in Open Em32 device: ");
				throw;
			}
		}

		public override bool IsSomeArchiveExist()
		{
			return (devInfo_.PqpExists == true ||
					devInfo_.AvgExists == true || devInfo_.DnsExists());
		}

		public override ExchangeResult ReadDeviceInfo()
		{
			byte[] buffer = null;
			ushort connectionScheme = 1;

			// creating DeviceCommonInfo object
			devInfo_ = new DeviceCommonInfoEm32();
			devInfo_.SerialNumber = serialNumber_;
			devInfo_.DevVersion = "0.0";
			int iNumOfPkeRecords = 0;

			try
			{
				// ConnectionScheme
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)1 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read ConnectionScheme!");
				}
				else
				{
					connectionScheme = Conversions.bytes_2_ushort(ref buffer, 0);
					// нумерация схем подключения не совпадает у 33 и 32, поэтому исправляем
					if (connectionScheme == 0 || connectionScheme == 1 ||
						connectionScheme == 4 || connectionScheme == 5)	// 4пр
						devInfo_.ConnectionScheme = ConnectScheme.Ph3W4;
					else if (connectionScheme == 2 || connectionScheme == 3)	// 3пр
						devInfo_.ConnectionScheme = ConnectScheme.Ph3W3;
				}
				// end of ConnectionScheme

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
					devInfo_.DevVersion = hardwareVer.ToString() + "." + programVer.ToString() + "." +
						buildDate.ToString();
				}
				// end of device version

				// object name
				devInfo_.ObjectName = "default object";
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)33 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null)
				{
					EmService.WriteToLogFailed("Unable to read Object name!");
				}
				else
				{
					Encoding enc = Encoding.GetEncoding(0);
					char[] cars = enc.GetChars(buffer);
					devInfo_.ObjectName = new string(cars);
					devInfo_.ObjectName = devInfo_.ObjectName.Replace('\0', ' ');
					devInfo_.ObjectName = devInfo_.ObjectName.Trim(' ');
					if (devInfo_.ObjectName == string.Empty) devInfo_.ObjectName = "default object";
				}
				// end of object name

				// у Em32 период фликера всегда равен 10
				devInfo_.t_fliker = 10;

				#region pqp

				// даты ПКЭ =================================================
				if (Read(EmCommands.COMMAND_ReadQualityDates, ref buffer, null) != 0)
					return ExchangeResult.Other_Error;

				ushort iCurrentRecord = 0;
				int recLen = 28;
				List<PqpSetEm32> listPqp = new List<PqpSetEm32>();
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
						iHour = buffer[i * recLen + 4];
						iMin = buffer[i * recLen + 5];
						iSec = buffer[i * recLen + 7];

						try
						{
							dtStart = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);
						}
						catch (ArgumentOutOfRangeException ex)
						{
							EmService.WriteToLogFailed(
								"ArgumentOutOfRangeException in pqp dates:  " + ex.Message);
							EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
								iYear, iMo, iDay, iHour, iMin, iSec));
							continue;
						}

						if (dtStart > DateTime.Now)
						{
							EmService.WriteToLogFailed("Invalid PQP Date (too large)!");
							EmService.WriteToLogFailed(
							string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
							iYear, iMo, iDay, iHour, iMin, iSec));
							//continue;
						}

						iHour = buffer[i * recLen + 8];
						iMin = buffer[i * recLen + 9];
						iSec = buffer[i * recLen + 11];

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
						}
						catch (ArgumentOutOfRangeException ex)
						{
							EmService.WriteToLogFailed(
								"ArgumentOutOfRangeException in pqp dates:  " + ex.Message);
							EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
								iYear, iMo, iDay, iHour, iMin, iSec));
							continue;
						}
						PqpSetEm32 tmpPqp = new PqpSetEm32();
						tmpPqp.Year = iYear;
						tmpPqp.Month = iMo;
						tmpPqp.Day = iDay;
						tmpPqp.PqpStart = dtStart;
						tmpPqp.PqpEnd = dtEnd;
						listPqp.Add(tmpPqp);
						++iCurrentRecord;

						// имя объекта
						if (devInfo_.ObjectName == "")
						{
							//string s = "";
							//for (int j = 0; j < 16; ++j)
							//{
							//    if (buffer[i * 28 + j + 416] != 0)
							//        s += (char)buffer[i * 28 + j + 416];
							//}
							//devInfo_.ObjectName = s;

							//Encoding enc = Encoding.GetEncoding(0);
							//char[] cars = enc.GetChars(buffer);
							//devInfo_.ObjectName = new string(cars);
							//devInfo_.ObjectName = devInfo_.ObjectName.Trim('\0');
						}
					}
				}

				if (iCurrentRecord == 0)
				{
					devInfo_.PqpExists = false;
				}
				else
				{
					devInfo_.PqpSet = new PqpSetEm32[iCurrentRecord];
					listPqp.CopyTo(devInfo_.PqpSet);
					devInfo_.PqpExists = true;
				}
				// end of даты ПКЭ ==========================================

				#endregion

				#region nominals
				// f_nominal
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)4 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read f_nominal!");
					devInfo_.F_Nominal = 0;
				}
				else
				{
					devInfo_.F_Nominal = (Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true));
				}
				// end of f_nominal

				// u_nominal_phase
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)5 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read u_nominal_phase!");
					devInfo_.U_NominalPhase = 0;
				}
				else
				{
					devInfo_.U_NominalPhase = (Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true));
				}
				// end of u_nominal_phase

				// u_nominal_linear
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)6 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read u_nominal_linear!");
					devInfo_.U_NominalLinear = 0;
				}
				else
				{
					devInfo_.U_NominalLinear = (Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true));
				}
				// end of u_nominal_linear

				// i_nominal_phase
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)17 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read i_nominal_phase!");
					devInfo_.I_NominalPhase = 0;
				}
				else
				{
					devInfo_.I_NominalPhase = (Conversions.bytes_2_float2wIEEE754_old(ref buffer, 0, true));
				}
				// end of i_nominal_phase

				// constraint_type
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)11 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read constraint_type!");
					devInfo_.ConstraintType = 0;
				}
				else
				{
					devInfo_.ConstraintType = Conversions.bytes_2_short(ref buffer, 0);
					devInfo_.ConstraintType -= 1;
				}
				// end of constraint_type

				// ml_start_time1
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)7 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read ml_start_time1!");
					devInfo_.MlStartTime1 = TimeSpan.Zero;
				}
				else
				{
					devInfo_.MlStartTime1 = Conversions.bytes_2_TimeSpanHhMm(ref buffer, 0);
				}
				// end of ml_start_time1

				// ml_end_time1
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)8 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read ml_end_time1!");
					devInfo_.MlEndTime1 = TimeSpan.Zero;
				}
				else
				{
					devInfo_.MlEndTime1 = Conversions.bytes_2_TimeSpanHhMm(ref buffer, 0);
				}
				// end of ml_end_time1

				// ml_start_time2
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)9 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read ml_start_time2!");
					devInfo_.MlStartTime2 = TimeSpan.Zero;
				}
				else
				{
					devInfo_.MlStartTime2 = Conversions.bytes_2_TimeSpanHhMm(ref buffer, 0);
				}
				// end of ml_start_time2

				// ml_end_time2
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)10 }) != 0)
					return ExchangeResult.Other_Error;
				if (buffer == null || buffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read ml_end_time2!");
					devInfo_.MlEndTime2 = TimeSpan.Zero;
				}
				else
				{
					devInfo_.MlEndTime2 = Conversions.bytes_2_TimeSpanHhMm(ref buffer, 0);
				}
				// end of ml_start_time2

				if (devInfo_.ObjectName.Length < 1)
					devInfo_.ObjectName = "Unknown object";
				devInfo_.U_Limit = 0;					// dummy
				devInfo_.I_Limit = 0;					// dummy
				devInfo_.CurrentTransducerIndex = 0;	// dummy
				#endregion

				#region avg

				devInfo_.AvgExists = false;

				// читаем только если не в автоматич. режиме
				if (!bAutoMode_)
				{
					if (Read(EmCommands.COMMAND_ReadEarliestAndLatestAverageTimestamp, ref buffer, null) != 0)
						return ExchangeResult.Other_Error;

					if (buffer != null && buffer.Length == 48)
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
								iDay = buffer[2];
								iMo = buffer[3];
								iHour = buffer[5];
								iMin = buffer[4];
								iSec = buffer[7];
								dtStart3sec = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);

								iYear = (ushort)(buffer[8] + (buffer[9] << 8));
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

							devInfo_.DateStartAvg3sec = dtStart3sec;
							devInfo_.DateEndAvg3sec = dtEnd3sec;

							if (buffer[16] == 255)
							{
								dtStart1min = DateTime.MinValue;
								dtEnd1min = DateTime.MinValue;
							}
							else
							{
								iYear = (ushort)(buffer[0 + 16] + (buffer[1 + 16] << 8));
								iDay = buffer[2 + 16];
								iMo = buffer[3 + 16];
								iHour = buffer[5 + 16];
								iMin = buffer[4 + 16];
								iSec = buffer[7 + 16];
								dtStart1min = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);

								iYear = (ushort)(buffer[8 + 16] + (buffer[9 + 16] << 8));
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

							devInfo_.DateStartAvg1min = dtStart1min;
							devInfo_.DateEndAvg1min = dtEnd1min;

							if (buffer[32] == 255)
							{
								dtStart30min = DateTime.MinValue;
								dtEnd30min = DateTime.MinValue;
							}
							else
							{
								iYear = (ushort)(buffer[0 + 32] + (buffer[1 + 32] << 8));
								iDay = buffer[2 + 32];
								iMo = buffer[3 + 32];
								iHour = buffer[5 + 32];
								iMin = buffer[4 + 32];
								iSec = buffer[7 + 32];
								dtStart30min = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);

								iYear = (ushort)(buffer[8 + 32] + (buffer[9 + 32] << 8));
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

							devInfo_.DateStartAvg30min = dtStart30min;
							devInfo_.DateEndAvg30min = dtEnd30min;
						}
						catch (ArgumentOutOfRangeException aex)
						{
							EmService.WriteToLogFailed("Error in avg dates:  " + aex.Message);
							devInfo_.AvgExists = false;
							EmService.WriteToLogFailed(
								string.Format("date numbers: {0},{1},{2},{3},{4},{5}",
								iYear, iMo, iDay, iHour, iMin, iSec));
						}

						if (devInfo_.DateStartAvg3sec != DateTime.MinValue ||
								devInfo_.DateStartAvg1min != DateTime.MinValue ||
								devInfo_.DateStartAvg30min != DateTime.MinValue)
							devInfo_.AvgExists = true;
					}
				}

				#endregion

				#region dip and swell pointers

				// читаем только если не в автоматич. режиме
				if (bAutoMode_)
				{
					devInfo_.ResetAllDns();
				}
				else
				{
					// at first read current dip/swell
					if (Read(EmCommands.COMMAND_ReadDipSwellStatus, ref buffer, null) != 0)
						return ExchangeResult.Other_Error;
					// analyze result
					if (buffer != null && buffer.Length >= 172)
					{
						ParseDipSwellStatusBuffer(ref buffer);
					}

					// read dip pointers
					//ushort startParam = 69;  // номер записи в системных данных прибора
					//for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
					//{
					//    // по всем фазам читаем только 3пр или 4пр, а не обе сразу
					//    if (devInfo_.ConnectionScheme == 2 && iPhase == 6) { startParam++; continue; }
					//    if (devInfo_.ConnectionScheme == 1 && iPhase == 7) { startParam++; continue; }

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
					//    if (devInfo_.ConnectionScheme == 2 && iPhase == 6) { startParam++; continue; }
					//    if (devInfo_.ConnectionScheme == 1 && iPhase == 7) { startParam++; continue; }

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

					//if (devInfo_.ConnectionScheme == 1)	// 4 пр
					//{
					//    devInfo_.PointerDipAll = devInfo_.PointersDip[(int)Phase.ABCN];
					//    devInfo_.PointerSwellAll = devInfo_.PointersSwell[(int)Phase.ABCN];
					//}
					//else
					//{
					//    devInfo_.PointerDipAll = devInfo_.PointersDip[(int)Phase.ABC];
					//    devInfo_.PointerSwellAll = devInfo_.PointersSwell[(int)Phase.ABC];
					//}

					if (!GetDipSwellStartEndDates())
					{
						devInfo_.ResetAllDns();
					}

					if (devInfo_.DnsExists() == false && devInfo_.PqpExists == false &&
						devInfo_.AvgExists == false)
						throw new EmDeviceEmptyException();
				}

				#endregion
			}
			catch (ThreadAbortException ae)
			{
				EmService.WriteToLogFailed("Error in ReadDeviceInfo(): " + ae.Message);
				Thread.ResetAbort();
				portManager_.ClosePort(true);
				return ExchangeResult.Other_Error;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ReadDeviceInfo(): " + ex.Message);
				EmService.WriteToLogFailed(ex.ToString());
				throw;
			}

			return ExchangeResult.OK;
		}

		//public bool ReadDateOfDipSwellEvent(Int64 pointer, int phase, byte typeEvent, string typeDate,
		//                                out DateTime res)
		//{
		//    try
		//    {
		//        res = DateTime.MinValue;
		//        byte[] buffer = null;
		//        ushort uPhase = PhaseToUshort(phase, typeEvent);
		//        if (Read(COMMAND_ReadDipSwellArchive, ref buffer,
		//                        new object[] { uPhase, pointer, (ushort)1 }) != 0) return false;

		//        if (buffer != null && buffer.Length >= 64)
		//        {
		//            if (typeDate == "start")
		//            {
		//                try
		//                {
		//                    ushort iYear = (ushort)(buffer[8] + (buffer[9] << 8));
		//                    if (iYear < 2008) iYear += 2000;
		//                    byte iMo = buffer[11];
		//                    byte iDay = buffer[10];
		//                    res = new DateTime(iYear, iMo, iDay,
		//                                    buffer[13],
		//                                    buffer[12],
		//                                    buffer[14],
		//                                    Conversions.bytes_2_ushort(ref buffer, 16));
		//                }
		//                catch { return false; }
		//            }
		//            else if (typeDate == "end")
		//            {
		//                try
		//                {
		//                    ushort iYear = (ushort)(buffer[18] + (buffer[19] << 8));
		//                    if (iYear < 2008) iYear += 2000;
		//                    byte iMo = buffer[21];
		//                    byte iDay = buffer[20];
		//                    res = new DateTime(iYear, iMo, iDay,
		//                                    buffer[23],
		//                                    buffer[22],
		//                                    buffer[24],
		//                                    Conversions.bytes_2_ushort(ref buffer, 26));
		//                }
		//                catch { return false; }
		//            }
		//        }
		//        else
		//        {
		//            EmService.WriteToLogFailed("Unable to read dip/swell record!");
		//            if (typeDate == "start") res = DateTime.MaxValue;
		//            else res = DateTime.MinValue;
		//            return false;
		//        }
		//        return true;
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.WriteToLogFailed("Error in ReadDateOfDipSwellEvent(): " + ex.Message);
		//        if (typeDate == "start") res = DateTime.MaxValue;
		//        else res = DateTime.MinValue;
		//        return false;
		//    }
		//}

		public ExchangeResult ReadAvgArchive(ref byte[] buffer, DateTime dtStart, DateTime dtEnd, AvgTypes avgType)
		{
			try
			{
				DateTime dtTempStart = DateTime.MaxValue;
				DateTime dtTempEnd = DateTime.MinValue;
				int interval = 60;
				EmCommands curCommand = EmCommands.COMMAND_Read1minArchiveByTimestamp;
				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] tempBuffer = null;
				List<byte> listTempBuffer = new List<byte>();

				switch (avgType)
				{
					case AvgTypes.ThreeSec: dtTempStart = devInfo_.DateStartAvg3sec;
						dtTempEnd = devInfo_.DateEndAvg3sec;
						curCommand = EmCommands.COMMAND_Read3secArchiveByTimestamp;
						interval = 3;
						break;
					case AvgTypes.OneMin: dtTempStart = devInfo_.DateStartAvg1min;
						dtTempEnd = devInfo_.DateEndAvg1min;
						curCommand = EmCommands.COMMAND_Read1minArchiveByTimestamp;
						interval = 60;
						break;
					case AvgTypes.ThirtyMin: dtTempStart = devInfo_.DateStartAvg30min;
						dtTempEnd = devInfo_.DateEndAvg30min;
						curCommand = EmCommands.COMMAND_Read30minArchiveByTimestamp;
						interval = 1800;
						break;
				}
				while (dtTempStart < dtStart) dtTempStart = dtTempStart.AddSeconds(interval);

				while (dtTempStart <= dtEnd && dtTempStart <= dtTempEnd)
				{
					tempBuffer = null;

					errCode = Read(curCommand, ref tempBuffer, new object[] { (ushort)dtTempStart.Year, 
									(byte)dtTempStart.Month, (byte)dtTempStart.Day, (byte)dtTempStart.Hour, 
									(byte)dtTempStart.Minute, (byte)dtTempStart.Second });
					//if (errCode != 0) return errCode;

					// если был дисконнект, то выходим!
					if (errCode == ExchangeResult.Disconnect_Error) throw new EmDisconnectException();

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
					if (OnStepReading != null) OnStepReading(EmDeviceType.EM32);
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
				EmService.DumpException(ex, "Error in ReadAvgArchive(): ");
				return ExchangeResult.Other_Error;
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
					throw new EmException(
						"ReadAvgArchiveSelectively(): Invalid fields parameters count!  "
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

				for(int iField = 0; iField < fields.Count; ++iField) 
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
				if(buffer != null)
					MakeStandartAvgBuffer(query_params, ref buffer);

				return errCode;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadAvgArchiveSelectively(): ");
				return ExchangeResult.Other_Error;
			}
		}

		public ExchangeResult ReadDipSwellArchive(ref byte[] buffer, DateTime dtStart, DateTime dtEnd)
		{
			try
			{
				Int64 pointerStart, pointerEnd;
				List<byte> listTempBuffer = new List<byte>();
				byte[] tempBuffer = null;
				ExchangeResult errCode = ExchangeResult.Other_Error;

				// dips
				for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				{
					// по всем фазам читаем только 3пр или 4пр, а не обе сразу
					if ((devInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
						devInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc) 
						&& iPhase == 6) { continue; }
					if ((devInfo_.ConnectionScheme == ConnectScheme.Ph3W4 ||
						devInfo_.ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
						&& iPhase == 7) { continue; }

					if (!ReadDipSwellIndex(dtStart, iPhase, 1 /*dip*/, "start", out pointerStart))
						continue;
					if (!ReadDipSwellIndex(dtEnd, iPhase, 1 /*dip*/, "end", out pointerEnd))
						continue;

					// не читать события, которые могли успеть произойти после того как были считаны 
					// данные по COMMAND_ReadDipSwellStatus, иначе будет путаница
					if (pointerEnd >= devInfo_.CurPointersDip[iPhase].Pointer && 
						devInfo_.CurPointersDip[iPhase].Pointer != -1)
						pointerEnd = devInfo_.CurPointersDip[iPhase].Pointer - 1;

					if (pointerEnd < pointerStart)
						continue;

					long count = (int)(pointerEnd - pointerStart + 1);
					ushort curCount;
					while (count > 0)
					{
						if (count <= 256) curCount = (ushort)count;
						else curCount = 256;
						errCode = Read(EmCommands.COMMAND_ReadDipSwellArchive, ref tempBuffer,
										new object[] { PhaseToUshort(iPhase, 1 /*dip*/), pointerStart, 
													(ushort)curCount });
						if (errCode != ExchangeResult.OK) return errCode;

						if (tempBuffer != null)
							listTempBuffer.AddRange(tempBuffer);
						count -= curCount;
					}

					if (OnStepReading != null) OnStepReading(EmDeviceType.EM32);
				}

				// swells
				for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				{
					// по всем фазам читаем только 3пр или 4пр, а не обе сразу
					if ((devInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
						devInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
						&& iPhase == 6) { continue; }
					if ((devInfo_.ConnectionScheme == ConnectScheme.Ph3W4 ||
						devInfo_.ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
						&& iPhase == 7) { continue; }

					if (!ReadDipSwellIndex(dtStart, iPhase, 0 /*swell*/, "start", out pointerStart))
						continue;
					if (!ReadDipSwellIndex(dtEnd, iPhase, 0 /*swell*/, "end", out pointerEnd))
						continue;

					// не читать события, которые могли успеть произойти после того как были считаны 
					// данные по COMMAND_ReadDipSwellStatus, иначе будет путаница
					if (pointerEnd >= devInfo_.CurPointersSwell[iPhase].Pointer &&
											devInfo_.CurPointersSwell[iPhase].Pointer != -1)
						pointerEnd = devInfo_.CurPointersSwell[iPhase].Pointer - 1;

					if (pointerEnd < pointerStart)
						continue;

					long count = (int)(pointerEnd - pointerStart + 1);
					ushort curCount;
					while (count > 0)
					{
						if (count <= 256) curCount = (ushort)count;
						else curCount = 256;
						errCode = Read(EmCommands.COMMAND_ReadDipSwellArchive, ref tempBuffer,
										new object[] { PhaseToUshort(iPhase, 0 /*swell*/), pointerStart, 
													(ushort)curCount });
						if (errCode != ExchangeResult.OK) return errCode;

						if (tempBuffer != null)
							listTempBuffer.AddRange(tempBuffer);
						count -= curCount;
					}

					if (OnStepReading != null) OnStepReading(EmDeviceType.EM32);
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
				EmService.WriteToLogFailed("Error in ReadDateOfDipSwellEvent(): " + ex.Message);
				return ExchangeResult.Other_Error;
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
				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ReadDeviceSerialNumber(): " + ex.Message);
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
		public ExchangeResult ReadMomentData(ref byte[] buffer, byte type, ref BaseDeviceCommonInfo devInfo)
		{
			try
			{
				byte[] tmpBuffer = null;
				// read nominal values:

				// f_nominal
				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, 
									new object[] { (ushort)4 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (tmpBuffer == null || tmpBuffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read f_nominal!");
					devInfo.F_Nominal = 0;
				}
				else
				{
					devInfo.F_Nominal = (Conversions.bytes_2_float2wIEEE754_old(ref tmpBuffer, 0, true));
				}
				// end of f_nominal

				// u_nominal_phase
				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, 
									new object[] { (ushort)5 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (tmpBuffer == null || tmpBuffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read u_nominal_phase!");
					devInfo.U_NominalPhase = 0;
				}
				else
				{
					devInfo.U_NominalPhase = (Conversions.bytes_2_float2wIEEE754_old(ref tmpBuffer, 0, true));
				}
				// end of u_nominal_phase

				// u_nominal_linear
				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, 
									new object[] { (ushort)6 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (tmpBuffer == null || tmpBuffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read u_nominal_linear!");
					devInfo.U_NominalLinear = 0;
				}
				else
				{
					devInfo.U_NominalLinear = (Conversions.bytes_2_float2wIEEE754_old(ref tmpBuffer, 0, true));
				}
				// end of u_nominal_linear

				// i_nominal_phase
				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, 
							new object[] { (ushort)17 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (tmpBuffer == null || tmpBuffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read i_nominal_phase!");
					devInfo.I_NominalPhase = 0;
				}
				else
				{
					devInfo.I_NominalPhase = (Conversions.bytes_2_float2wIEEE754_old(ref tmpBuffer, 0, true));
				}
				// end of i_nominal_phase

				// ConnectionScheme
				ushort connectionScheme;
				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, 
								new object[] { (ushort)1 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (tmpBuffer == null || tmpBuffer.Length < 2)
				{
					EmService.WriteToLogFailed("Unable to read ConnectionScheme!");
					connectionScheme = 1;
				}
				else
				{
					connectionScheme = Conversions.bytes_2_ushort(ref tmpBuffer, 0);
					// нумерация схем подключения не совпадает у 33 и 32, поэтому исправляем
					if (connectionScheme == 0 || connectionScheme == 1 ||
						connectionScheme == 4 || connectionScheme == 5)	// 4пр
						devInfo.ConnectionScheme = ConnectScheme.Ph3W4;
					else if (connectionScheme == 2 || connectionScheme == 3)	// 3пр
						devInfo.ConnectionScheme = ConnectScheme.Ph3W3;
				}
				// end of ConnectionScheme

				// object name
				if (Read(EmCommands.COMMAND_ReadSystemData, ref tmpBuffer, 
								new object[] { (ushort)33 }) != ExchangeResult.OK)
					return ExchangeResult.Other_Error;
				if (tmpBuffer == null)
				{
					devInfo.ObjectName = "";
					EmService.WriteToLogFailed("Unable to read Object name!");
				}
				else
				{
					Encoding enc = Encoding.GetEncoding(0);
					char[] cars = enc.GetChars(tmpBuffer);
					devInfo.ObjectName = new string(cars);
					devInfo.ObjectName = devInfo.ObjectName.Trim('\0');
				}
				// end of object name

				// read AVG archive
				EmCommands command;
				switch (type)
				{
					case 1: command = EmCommands.COMMAND_Read3secValues; break;
					case 2: command = EmCommands.COMMAND_Read1minValues; break;
					case 3: command = EmCommands.COMMAND_Read30minValues; break;
					default: command = EmCommands.COMMAND_Read3secValues; break;
				}
				if (Read(command, ref buffer, null) != 0) return ExchangeResult.Other_Error;

				if (buffer == null)
					throw new EmDeviceEmptyException();
			}
			catch (System.Threading.ThreadAbortException)
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
				if (Read(EmCommands.COMMAND_ReadTime, ref buffer, null) != ExchangeResult.OK) return false;

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

		public ExchangeResult ReadNominalsAndTimes(ref object[] vals)
		{
			try
			{
				byte[] buffer = null;
				ExchangeResult errCode = ExchangeResult.Other_Error;

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
		/// функция считывания недостающих записей журнала событий
		/// (numLastRecord - последняя запись в БД)
		/// </summary>
		public bool ReadLostEventJournalRecords(ref byte[] buffer, long numLastRecord)
		{
			long recordPointer = -1;	// текущая запись в устройстве (сквозная нумерация)
			List<byte> res = new List<byte>();
			try
			{
				// номер последней записи в журнале
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)44 }) != ExchangeResult.OK)
					return false;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read Journal Record Number!");
					recordPointer = -1;
				}
				else
				{
					recordPointer = Conversions.bytes_2_uint_new(ref buffer, 0);
				}
				EmService.WriteToLogDebug("ReadLostEventJournalRecords: recordPointer = " 
							+ recordPointer.ToString());
				if (recordPointer < 0)
					return false;

				// вычисляем стартовый номер записи
				long startRecord = recordPointer;
				long count = 0;
				if (numLastRecord == -1)		// если в БД ничего нет
				{
					if (recordPointer <= 4096)	// если в журнале еще не пройден первый круг
					{
						startRecord = 0;
						count = recordPointer;
					}
					else
					{
						count = 4096;
						startRecord = recordPointer - 4096; // recordPointer - сквозной номер
					}
				}
				else    // если в БД есть записи
				{
					// с момента последнего считывания не пройден круг
					if ((recordPointer - 1) - numLastRecord <= 4096)
					{
						startRecord = numLastRecord + 1;
						count = (recordPointer - 1) - numLastRecord;
					}
					else
					{
						count = 4096;
						startRecord = recordPointer - 4096;  // recordPointer - сквозной номер
					}
				}
				EmService.WriteToLogDebug("ReadLostEventJournalRecords: startRecord = "
							+ startRecord.ToString());
				EmService.WriteToLogDebug("ReadLostEventJournalRecords: count = "
							+ count.ToString());
				if (startRecord < 0) return false;

				ushort iCurCount;
				while (count > 0)
				{
					if (count >= 128)			//if (count >= 512)
						iCurCount = 128;		//iCurCount = 512;
					else
						iCurCount = (ushort)count;

					if (Read(EmCommands.COMMAND_ReadEventLogger, ref buffer,
								new object[] { (uint)(startRecord), iCurCount }) != ExchangeResult.OK)
						return false;
					if (buffer == null)
						return false;
					res.AddRange(buffer);

					count -= iCurCount;
					startRecord += iCurCount;
				}

				buffer = new byte[res.Count];
				res.CopyTo(buffer);
			}
			catch (System.Threading.ThreadAbortException)
			{
				Thread.ResetAbort();
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadLostEventJournalRecords(): ");
				throw;
			}
			return true;
		}

		public bool ReadEventJournal(ref byte[] buffer, int count)
		{
			long recordPointer = -1;
			List<byte> res = new List<byte>();
			try
			{
				// номер последней записи в журнале
				if (Read(EmCommands.COMMAND_ReadSystemData, ref buffer, new object[] { (ushort)44 }) != 0)
					return false;
				if (buffer == null || buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Unable to read Journal Record Number!");
					recordPointer = -1;
				}
				else
				{
					recordPointer = Conversions.bytes_2_uint(ref buffer, 0);
				}
				if (recordPointer < 0)
					return false;

				// вычисляем стартовый номер записи
				long startRecord = recordPointer;
				for (int i = 0; i < count; ++i)
				{
					startRecord--;
					if (startRecord < 0)
						startRecord = 4095;
				}

				for (int i = 0; i < count; ++i)
				{
					if (Read(EmCommands.COMMAND_ReadEventLogger, ref buffer,
								new object[] { (ushort)(startRecord + i) }) != 0) 
						return false;
					if (buffer == null)
						return false;
					res.AddRange(buffer);
				}

				buffer = new byte[res.Count];
				res.CopyTo(buffer);
			}
			catch (System.Threading.ThreadAbortException)
			{
				Thread.ResetAbort();
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadEventJournal(): ");
				throw;
			}
			return true;
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
					if (res == ExchangeResult.OK)  // если успешно
						break;
					if (res == ExchangeResult.Disconnect_Error)  // если был дисконнект
					{
						//ClosePort();
						throw new EmException("Device was disconnected!");
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
				EmService.DumpException(ex, "Error in Em32Device::Read():  ");
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

				return 0;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in Em32Device::WriteNominalsAndTimes():");
				throw;
			}
		}

		public ExchangeResult WriteSets(ref byte[] buffer)
		{
			try
			{
				List<byte> tempList = new List<byte>(buffer);
				// the third parameter will be ignored
				return portManager_.WriteData(EmCommands.COMMAND_WriteSets, ref tempList);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in Em32Device::WriteSets():");
				throw;
			}
		}

		public ExchangeResult ReadDeviceName(out string name)
		{
			try
			{
				name = "";
				byte[] buffer = null;
				ExchangeResult errCode = Read(EmCommands.COMMAND_ReadSystemData, ref buffer,
						new object[] { (ushort)33 });
				if (errCode != ExchangeResult.OK)
					return errCode;
				name = Conversions.bytes_2_string(ref buffer, 0, 16);
				return errCode;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDeviceName:");
				throw;
			}
		}

		public ExchangeResult WriteDeviceName(ref byte[] buffer)
		{
			try
			{
				return WriteSystemData(33, ref buffer);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in WriteDeviceName():");
				throw;
			}
		}

		#endregion

		#region Private and Protected Methods

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

		protected void ParseDipSwellStatusBuffer(ref byte[] buffer)
		{
			try
			{
				ushort status = Conversions.bytes_2_ushort(ref buffer, 8);
				devInfo_.BufCurDipSwellData = buffer;

				ushort curStatus = 0x0001;
				ushort curOffset = 12;
				for (int iPhase = 0; iPhase < /*EmService.CountPhases*/ 6; ++iPhase)
				{
					if ((status & curStatus) != 0)		// dip
					{
						devInfo_.CurPointersDip[iPhase].Pointer = 
								Conversions.bytes_2_uint_new(ref buffer, curOffset);
						ushort iYear = (ushort)(buffer[curOffset + 4] + (buffer[curOffset + 5] << 8)); 
						devInfo_.CurPointersDip[iPhase].Date =
								new DateTime(iYear, buffer[curOffset + 7],
										buffer[curOffset + 6], buffer[curOffset + 9],
										buffer[curOffset + 8], buffer[curOffset + 11]);
					}
					else if ((status & (curStatus * 2)) != 0)		// swell
					{
						devInfo_.CurPointersSwell[iPhase].Pointer = 
							Conversions.bytes_2_uint_new(ref buffer, curOffset);
						ushort iYear = (ushort)(buffer[curOffset + 4] + (buffer[curOffset + 5] << 8));
						devInfo_.CurPointersSwell[iPhase].Date =
								new DateTime(iYear, buffer[curOffset + 7],
										buffer[curOffset + 6], buffer[curOffset + 9],
										buffer[curOffset + 8], buffer[curOffset + 11]);
					}
					curStatus *= 4;
					curOffset += 16;
				}

				if (devInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
					devInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
				{
					if ((status & 0x4000) != 0)
					{
						devInfo_.CurPointersDip[(int)Phase.ABC].Pointer =
												Conversions.bytes_2_uint_new(ref buffer, 140);
						ushort iYear = (ushort)(buffer[140 + 4] + (buffer[140 + 5] << 8));
						devInfo_.CurPointersDip[(int)Phase.ABC].Date =
								new DateTime(iYear, buffer[140 + 7],
										buffer[140 + 6], buffer[140 + 9],
										buffer[140 + 8], buffer[140 + 11]);
					}
					if ((status & 0x8000) != 0)
					{
						devInfo_.CurPointersSwell[(int)Phase.ABC].Pointer =
												Conversions.bytes_2_uint_new(ref buffer, 156);
						ushort iYear = (ushort)(buffer[156 + 4] + (buffer[156 + 5] << 8));
						devInfo_.CurPointersSwell[(int)Phase.ABC].Date =
								new DateTime(iYear, buffer[156 + 7],
										buffer[156 + 6], buffer[156 + 9],
										buffer[156 + 8], buffer[156 + 11]);
					}
				}
				else if (devInfo_.ConnectionScheme == ConnectScheme.Ph3W4 ||
						devInfo_.ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
				{
					if ((status & 0x1000) != 0)
					{
						devInfo_.CurPointersDip[(int)Phase.ABCN].Pointer =
												Conversions.bytes_2_uint_new(ref buffer, 108);
						ushort iYear = (ushort)(buffer[108 + 4] + (buffer[108 + 5] << 8));
						devInfo_.CurPointersDip[(int)Phase.ABCN].Date =
								new DateTime(iYear, buffer[108 + 7],
										buffer[108 + 6], buffer[108 + 9],
										buffer[108 + 8], buffer[108 + 11]);
					}
					if ((status & 0x2000) != 0)
					{
						devInfo_.CurPointersSwell[(int)Phase.ABCN].Pointer =
												Conversions.bytes_2_uint_new(ref buffer, 124);
						ushort iYear = (ushort)(buffer[124 + 4] + (buffer[124 + 5] << 8));
						devInfo_.CurPointersSwell[(int)Phase.ABCN].Date =
								new DateTime(iYear, buffer[124 + 7],
										buffer[124 + 6], buffer[124 + 9],
										buffer[124 + 8], buffer[124 + 11]);
					}
				}

				if (!devInfo_.CurDnsExists())
					devInfo_.BufCurDipSwellData = null;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ParseDipSwellStatusBuffer(): ");
				devInfo_.BufCurDipSwellData = null;
				devInfo_.ResetAllDns();
			}
		}


		/// <summary>
		/// Функция получает первую и последнюю дату архива DNS из прибора (учитываются все фазы)
		/// </summary>
		/// <returns></returns>
		protected bool GetDipSwellStartEndDates()
		{
			try
			{
				DateTime startDate = DateTime.MaxValue;
				DateTime endDate = DateTime.MinValue;
				devInfo_.DateStartDipSwell = DateTime.MaxValue;
				byte[] buffer = null;

				if (Read(EmCommands.COMMAND_ReadEarliestAndLatestDipSwellTimestamp, ref buffer, null) != 0)
					return false;

				if (buffer == null || buffer.Length < 20)
				{
					if (devInfo_.CurDnsExists())
					{
						for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
						{
							if (devInfo_.CurPointersDip[iPhase].Pointer != -1)
							{
								if (devInfo_.CurPointersDip[iPhase].Date < devInfo_.DateStartDipSwell)
									devInfo_.DateStartDipSwell = devInfo_.CurPointersDip[iPhase].Date;
								devInfo_.DateEndDipSwell = DateTime.Now;
							}
							if (devInfo_.CurPointersSwell[iPhase].Pointer != -1)
							{
								if (devInfo_.CurPointersSwell[iPhase].Date < devInfo_.DateStartDipSwell)
									devInfo_.DateStartDipSwell = devInfo_.CurPointersSwell[iPhase].Date;
								devInfo_.DateEndDipSwell = DateTime.Now;
							}
						}
						return true;
					}
					else
						return false;
				}

				try
				{
					ushort iYear = (ushort)(buffer[0] + (buffer[1] << 8));
					if (iYear < 2008) iYear += 2000;
					byte iMo = buffer[3];
					byte iDay = buffer[2];
					startDate = new DateTime(iYear, iMo, iDay,
									buffer[5],
									buffer[4],
									buffer[6], 0);
									//Conversions.bytes_2_ushort(ref buffer, 8));

					iYear = (ushort)(buffer[10] + (buffer[11] << 8));
					if (iYear < 2008) iYear += 2000;
					iMo = buffer[13];
					iDay = buffer[12];
					endDate = new DateTime(iYear, iMo, iDay,
									buffer[15],
									buffer[14],
									buffer[16], 0);
									//Conversions.bytes_2_ushort(ref buffer, 18));
				}
				catch (Exception e)
				{
					EmService.WriteToLogFailed("Error in GetDipSwellStartEndDates()!  " + e.Message);
					return false;
				}

				devInfo_.DateStartDipSwell = startDate;
				devInfo_.DateEndDipSwell = endDate;

				for (int iPhase = 0; iPhase < EmService.CountPhases; ++iPhase)
				{
					// по всем фазам читаем только 3пр или 4пр, а не обе сразу
					if ((devInfo_.ConnectionScheme == ConnectScheme.Ph3W3 ||
						devInfo_.ConnectionScheme == ConnectScheme.Ph3W3_B_calc)
						&& iPhase == 6) { continue; }
					if ((devInfo_.ConnectionScheme == ConnectScheme.Ph3W4 ||
						devInfo_.ConnectionScheme == ConnectScheme.Ph3W4_B_calc)
						&& iPhase == 7) { continue; }

					if (devInfo_.CurPointersDip[iPhase].Pointer != -1 ||
						devInfo_.CurPointersSwell[iPhase].Pointer != -1)
					{
						devInfo_.DateEndDipSwell = DateTime.Now;
						break;
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

		protected bool ReadDipSwellIndex(DateTime dtStart, int phase, byte typeEvent, string typeDate,
										out Int64 pointer)
		{
			pointer = -1;
			try
			{
				byte[] buffer = null;
				ushort uPhase = PhaseToUshort(phase, typeEvent);
				if (typeDate == "start")
				{
					if (Read(EmCommands.COMMAND_ReadDipSwellIndexByStartTimestamp, ref buffer,
									new object[] { uPhase, (ushort)dtStart.Year, (byte)dtStart.Month, 
									(byte)dtStart.Day, (byte)dtStart.Hour, (byte)dtStart.Minute,
									(byte)dtStart.Second }) != 0)
						return false;
				}
				else if (typeDate == "end")
				{
					if (Read(EmCommands.COMMAND_ReadDipSwellIndexByEndTimestamp, ref buffer,
									new object[] { uPhase, (ushort)dtStart.Year, (byte)dtStart.Month, 
									(byte)dtStart.Day, (byte)dtStart.Hour, (byte)dtStart.Minute,
									(byte)dtStart.Second }) != 0)
						return false;
				}
				else return false;

				if (buffer != null && buffer.Length >= 4)
				{
					pointer = Conversions.bytes_2_uint_new(ref buffer, 0);
				}
				else return false;

				return true;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ReadDipSwellIndex(): " + ex.Message);
				return false;
			}
		}

		protected static ushort GetIdAvgQuery()
		{
			++id_avg_query_;
			if (id_avg_query_ > 50) id_avg_query_ = 1;
			return id_avg_query_;
		}

		#endregion
	}
}
