using System;
using System.Collections.Generic;
using System.Text;

using EmServiceLib;

namespace DeviceIO
{
	public abstract class EmDevice
	{
		#region Enums

		protected enum QueryAvgType
		{
			AAQ_TYPE_ReadStatus = 0,
			AAQ_TYPE_ResetAll = 1,
			AAQ_TYPE_Query = 2
		}

		protected enum QueryAvgCurStatus
		{
			AAQ_STATE_Idle = 0,
			AAQ_STATE_Busy = 1
		}

		#endregion

		#region Fields

		#region Const

		protected static ushort[] CRC16Table = new ushort[] {
            0X0000, 0XC0C1, 0XC181, 0X0140, 0XC301, 0X03C0, 0X0280, 0XC241,
            0XC601, 0X06C0, 0X0780, 0XC741, 0X0500, 0XC5C1, 0XC481, 0X0440,
            0XCC01, 0X0CC0, 0X0D80, 0XCD41, 0X0F00, 0XCFC1, 0XCE81, 0X0E40,
            0X0A00, 0XCAC1, 0XCB81, 0X0B40, 0XC901, 0X09C0, 0X0880, 0XC841,
            0XD801, 0X18C0, 0X1980, 0XD941, 0X1B00, 0XDBC1, 0XDA81, 0X1A40,
            0X1E00, 0XDEC1, 0XDF81, 0X1F40, 0XDD01, 0X1DC0, 0X1C80, 0XDC41,
            0X1400, 0XD4C1, 0XD581, 0X1540, 0XD701, 0X17C0, 0X1680, 0XD641,
            0XD201, 0X12C0, 0X1380, 0XD341, 0X1100, 0XD1C1, 0XD081, 0X1040,
            0XF001, 0X30C0, 0X3180, 0XF141, 0X3300, 0XF3C1, 0XF281, 0X3240,
            0X3600, 0XF6C1, 0XF781, 0X3740, 0XF501, 0X35C0, 0X3480, 0XF441,
            0X3C00, 0XFCC1, 0XFD81, 0X3D40, 0XFF01, 0X3FC0, 0X3E80, 0XFE41,
            0XFA01, 0X3AC0, 0X3B80, 0XFB41, 0X3900, 0XF9C1, 0XF881, 0X3840,
            0X2800, 0XE8C1, 0XE981, 0X2940, 0XEB01, 0X2BC0, 0X2A80, 0XEA41,
            0XEE01, 0X2EC0, 0X2F80, 0XEF41, 0X2D00, 0XEDC1, 0XEC81, 0X2C40,
            0XE401, 0X24C0, 0X2580, 0XE541, 0X2700, 0XE7C1, 0XE681, 0X2640,
            0X2200, 0XE2C1, 0XE381, 0X2340, 0XE101, 0X21C0, 0X2080, 0XE041,
            0XA001, 0X60C0, 0X6180, 0XA141, 0X6300, 0XA3C1, 0XA281, 0X6240,
            0X6600, 0XA6C1, 0XA781, 0X6740, 0XA501, 0X65C0, 0X6480, 0XA441,
            0X6C00, 0XACC1, 0XAD81, 0X6D40, 0XAF01, 0X6FC0, 0X6E80, 0XAE41,
            0XAA01, 0X6AC0, 0X6B80, 0XAB41, 0X6900, 0XA9C1, 0XA881, 0X6840,
            0X7800, 0XB8C1, 0XB981, 0X7940, 0XBB01, 0X7BC0, 0X7A80, 0XBA41,
            0XBE01, 0X7EC0, 0X7F80, 0XBF41, 0X7D00, 0XBDC1, 0XBC81, 0X7C40,
            0XB401, 0X74C0, 0X7580, 0XB541, 0X7700, 0XB7C1, 0XB681, 0X7640,
            0X7200, 0XB2C1, 0XB381, 0X7340, 0XB101, 0X71C0, 0X7080, 0XB041,
            0X5000, 0X90C1, 0X9181, 0X5140, 0X9301, 0X53C0, 0X5280, 0X9241,
            0X9601, 0X56C0, 0X5780, 0X9741, 0X5500, 0X95C1, 0X9481, 0X5440,
            0X9C01, 0X5CC0, 0X5D80, 0X9D41, 0X5F00, 0X9FC1, 0X9E81, 0X5E40,
            0X5A00, 0X9AC1, 0X9B81, 0X5B40, 0X9901, 0X59C0, 0X5880, 0X9841,
            0X8801, 0X48C0, 0X4980, 0X8941, 0X4B00, 0X8BC1, 0X8A81, 0X4A40,
            0X4E00, 0X8EC1, 0X8F81, 0X4F40, 0X8D01, 0X4DC0, 0X4C80, 0X8C41,
            0X4400, 0X84C1, 0X8581, 0X4540, 0X8701, 0X47C0, 0X4680, 0X8641,
            0X8201, 0X42C0, 0X4380, 0X8341, 0X4100, 0X81C1, 0X8081, 0X4040 };

		protected const ushort CRC16_SEED = 0xFFFF;

		#endregion

		protected PortManager portManager_;

		protected EmPortType portType_;
		protected EmDeviceType devType_;
		protected ushort devAddress_;
		protected object[] portParams_;
		protected long serialNumber_;

		// флаг показывает идет ли чтение в автоматич. режиме (если да, то мы читаем
		// только последний архив ѕ Ё, поэтому не надо читать сис.инфу по DNS и AVG)
		protected bool bAutoMode_;

		protected IntPtr hMainWnd_;

		// use only for Em32 and EtPQP
		protected byte[] pswd_for_writing_ = null;
		protected byte[] time_for_writing_ = new byte[20];
		protected ushort[] hash_for_writing_ = new ushort[10];

		protected bool bCancelReading_ = false;

		#endregion

		#region Events

		/// <summary>Delegate of event OnStepReading</summary>
		public delegate void StepReadingHandler(EmDeviceType devType);
		/// <summary>
		/// —обытие OnStepReading происходит, когда считано заданное число станиц (число
		/// определено константой). Ёто информаци€ дл€ ProgressBar главного окна
		/// </summary>
		public StepReadingHandler OnStepReading;

		#endregion

		#region Constructors

		public EmDevice(EmDeviceType devType, EmPortType portType, 
						ushort devAddr, bool auto, object[] port_params, IntPtr hMainWnd)
		{
			devType_ = devType;
			portType_ = portType;
			devAddress_ = devAddr;
			bAutoMode_ = auto;
			portParams_ = port_params;
			hMainWnd_ = hMainWnd;
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Open device and get serial number
		/// </summary>
		/// <returns>serial number if successful; -1 if there was some error</returns>
		public abstract int OpenDevice();

		public bool Close()
		{
			return portManager_.ClosePort(true);
		}

		public abstract ExchangeResult ReadDeviceInfo();

		public abstract bool IsSomeArchiveExist();

		public static ushort CalcCrc16(ref byte[] buffer, int start, int len)
		{
			ushort temp, crc = CRC16_SEED;
			try
			{
				// последние два байта не рассматриваем, т.к. они зарезервированы дл€ crc,
				// первый байт тоже пропускаем
				for (int i = start; i < (len + start - 2) && buffer.Length > len; ++i)
				{
					temp = (ushort)((buffer[i] ^ crc) & 0xFF);
					crc >>= 8;
					crc ^= CRC16Table[temp];
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in CalcCrc16(): " + ex.Message);
				throw;
			}
			return crc;
		}

		#endregion

		#region Protected Methods

		protected void MakeStandartAvgBuffer(object[] other_params, ref byte[] buffer)
		{
			// внутри массива other_params[]
			const int avgServiceInfoLen = 15;

			try
			{
				List<byte> listTempBuffer = new List<byte>();
				byte[] tempBuffer;

				int paramsCount = other_params.Length - avgServiceInfoLen;
				// длина одной записи
				int recordLength = 8 /*date len*/ + paramsCount * 2;

				// длина служебной инфы в сегменте
				int serviceDataLength = 8;

				// смещение внутри буфера, содержащего все сегменты
				int curAllSegmentsShift = 0;
				// смещение внутри сегмента
				int curRecordShift = serviceDataLength;

				// список в который будем складывать даты, чтобы исключить повторы
				List<DateTime> listDates = new List<DateTime>();

				while (true)
				{
					// кол-во записей в данном сегменте
					int segRecordsCount =
						Conversions.bytes_2_ushort(ref buffer, 6 + curAllSegmentsShift);

					for (int iRecord = 0; iRecord < segRecordsCount; ++iRecord)
					{
						tempBuffer = new byte[4096];

						// достаем дату из архива (это нужно только дл€ того, чтобы убедитьс€, что этой даты
						// еще не было, т.к. могут быть повторы и их надо исключить, иначе будет ошибка при
						// вставке в Ѕƒ)
						ushort iYear = 0;
						byte iMo = 0, iDay = 0;
						byte iHour = 0, iMin = 0, iSec = 0;
						try
						{
							iYear = (ushort)(buffer[curRecordShift + curAllSegmentsShift] +
													(buffer[curRecordShift + curAllSegmentsShift + 1] << 8));
							iMo = buffer[curRecordShift + curAllSegmentsShift + 3];
							iDay = buffer[curRecordShift + curAllSegmentsShift + 2];
							iHour = buffer[curRecordShift + curAllSegmentsShift + 5];
							iMin = buffer[curRecordShift + curAllSegmentsShift + 4];
							iSec = buffer[curRecordShift + curAllSegmentsShift + 6];
							DateTime res = new DateTime(iYear, iMo, iDay, iHour, iMin, iSec);
							if (listDates.Contains(res))
							{
								EmService.WriteToLogFailed(
									"MakeStandartAvgBuffer: This date already exists!  " + res.ToString());
								// смещение внутри одного сегмента
								curRecordShift += recordLength;
								continue;
							}
							else
								listDates.Add(res);
						}
						catch (ArgumentOutOfRangeException)
						{
							EmService.WriteToLogFailed("MakeStandartAvgBuffer(): Invalid date!");
							EmService.WriteToLogFailed(
								string.Format("{0}, {1}, {2}, {3}, {4}, {5}", iYear, iMo, iDay, iHour,
								iMin, iSec));
							// смещение внутри одного сегмента
							curRecordShift += recordLength;
							continue;
						}

						// заполн€ем дату
						Array.Copy(buffer, curRecordShift + curAllSegmentsShift,
									tempBuffer, 44, 8 /*date length*/);

						// заполн€ем все пол€
						int curSegmentShift = curRecordShift + 8 /*date length*/;
						for (int iParam = avgServiceInfoLen; iParam < other_params.Length; ++iParam)
						{
							Array.Copy(buffer, curSegmentShift + curAllSegmentsShift,
										tempBuffer, ((ushort)other_params[iParam] * 2), 2);

							curSegmentShift += 2;
						}
						listTempBuffer.AddRange(tempBuffer);

						// смещение внутри одного сегмента
						curRecordShift += recordLength;
					}
					// смещение внутри buffer
					curAllSegmentsShift += (serviceDataLength + segRecordsCount * recordLength);
					// сбрасываем смещение внутри сегмента
					curRecordShift = serviceDataLength;

					if ((curAllSegmentsShift + serviceDataLength) > buffer.Length)
						break;
				}

				buffer = new byte[listTempBuffer.Count];
				listTempBuffer.CopyTo(buffer);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in MakeStandartAvgBuffer(): ");
				throw;
			}
		}

		protected bool MakeIntList(ref object[] buf, out List<UInt32> list)
		{
			list = new List<UInt32>();
			if (buf == null) return true;

			for (int i = 0; i < buf.Length; ++i)
			{
				try
				{
					string s = buf[i].ToString();
					UInt32 num = UInt32.Parse(s);
					list.Add(num);
				}
				catch (InvalidCastException)
				{
					EmService.WriteToLogFailed("Error in MakeIntList: invalid cast!");
					return false;
				}
			}
			return true;
		}

		// for Em32 and EtPQP only!
		protected void CalcHashForWriting(ref byte[] time_buf, ref ushort[] hash_buf)
        {
			try
			{
				int i, j, k;
				ushort w0, w1;
				for (i = 0; i < 10; i++)
				{
					w0 = (ushort)((((ushort)pswd_for_writing_[i]) & 0x000F) ^ 0xA5A5);

					for (j = 0; j < 20; j++)
					{
						w1 = time_buf[j];
						for (k = 0; k < 8; k++)
						{
							if (((w0 ^ w1) & 0x0001) != 0x0000)
								w0 = (ushort)((w0 >> 1) ^ ((((ushort)pswd_for_writing_[i]) & 0x000F) ^ 0xA5A5));
							else
								w0 = (ushort)(w0 >> 1);
							w1 = (ushort)(w1 >> 1);
						}
					}
					hash_buf[i] = w0;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CalcHashForWriting:");
				throw;
			}
        }

		protected ushort PhaseToUshort(int phase, byte type)
		{
			ushort res = 0;
			switch (phase)
			{
				case 0: if (type == 0) res = 0x08; else res = 0x00; break;
				case 1: if (type == 0) res = 0x09; else res = 0x01; break;
				case 2: if (type == 0) res = 0x0A; else res = 0x02; break;
				case 3: if (type == 0) res = 0x0B; else res = 0x03; break;
				case 4: if (type == 0) res = 0x0C; else res = 0x04; break;
				case 5: if (type == 0) res = 0x0D; else res = 0x05; break;
				case 6: if (type == 0) res = 0x0E; else res = 0x06; break;
				case 7: if (type == 0) res = 0x0F; else res = 0x07; break;
			}
			return res;
		}

		#endregion

		#region Properties

		/// <summary>If user wants to cancel reading</summary>
		public bool BCancelReading
		{
			//get { return bCancelReading_; }
			set { bCancelReading_ = value; }
		}

		/// <summary>Device Type (EM31K, EM32, EM33T)</summary>
		public EmDeviceType DeviceType
		{
			get { return this.devType_; }
		}

		/// <summary>Port Type</summary>
		public EmPortType PortType
		{
			get { return this.portType_; }
			set { this.portType_ = value; }
		}

		/// <summary>Serial number</summary>
		public long SerialNumber
		{
			get { return this.serialNumber_; }
			set { this.serialNumber_ = value; }
		}

		#endregion
	}

	/// <summary>
	/// Short stricture of common device information
	/// </summary>
	[Serializable]
	public class BaseDeviceCommonInfo
	{
		public long SerialNumber;
		public string DevVersion;
		public string ObjectName;
		public ConnectScheme ConnectionScheme;
		public float F_Nominal;
		public float U_NominalLinear;
		public float U_NominalPhase;
		public float I_NominalPhase;
		public bool U_transformer_enable;	// for EtPQP_A only
		public short U_transformer_type;	// for EtPQP_A only
		public short I_transformer_usage;	// for EtPQP_A only
		public int I_transformer_primary;			// “рансформатор тока Ц первичный ток
		public short I_transformer_secondary;		// “рансформатор тока Ц вторичный ток

		public ushort CurrentTransducerIndex;
		public float U_Limit;
		public float I_Limit;
		public int F_Limit;

		public DateTime CommonBegin;
		public DateTime CommonEnd;
		public TimeSpan MlStartTime1;
		public TimeSpan MlEndTime1;
		public TimeSpan MlStartTime2;
		public TimeSpan MlEndTime2;
	}

	public class DnsPointerData
	{
		Int64 pointer_;
		DateTime date_;

		public DnsPointerData()
		{
			pointer_ = -1;
			date_ = DateTime.MinValue;
		}

		public DnsPointerData(Int64 p, DateTime d)
		{
			pointer_ = p;
			date_ = d;
		}

		public Int64 Pointer
		{
			get { return pointer_; }
			set { pointer_ = value; }
		}

		public DateTime Date
		{
			get { return date_; }
			set { date_ = value; }
		}
	}
}
