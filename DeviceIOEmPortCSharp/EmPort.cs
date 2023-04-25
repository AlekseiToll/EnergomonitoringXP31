using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;

using EmServiceLib;
using PQPAUsbDriver32;

namespace DeviceIOEmPortCSharp
{
	public class StructReply
	{
		public UInt32 dwAddress;
		public UInt16 wCommand;
		public UInt16 wLength;
		public UInt16 wCounter;
		public byte[] bData;
		public UInt16 wCRC;

		public StructReply() 
		{
			bData = new byte[0x8000]; 
		}
	}

	public abstract class EmPort
	{
		protected UsbComm port_;
		protected EmDeviceType devType_;
		protected EmPortType portType_;
		protected IntPtr hMainWnd_;

		public EmPort(EmDeviceType devType, EmPortType portType, IntPtr hMainWnd)
		{
			devType_ = devType;
			portType_ = portType;
			hMainWnd_ = hMainWnd;
		}

		public abstract bool Open();
		public abstract bool Close();
		public abstract ExchangeResult Write(byte[] buffer);
		protected abstract ExchangeResult Read(ref byte c);
		protected abstract ExchangeResult Read(ref byte c, int timeout);
	}

	public abstract class EmPortSLIP : EmPort
	{
		#region Enums

		protected enum QueryAvgType
		{
			AAQ_TYPE_ReadStatus = 0,
			AAQ_TYPE_ResetAll = 1,
			AAQ_TYPE_Query = 2
		}

		protected enum Protocol
		{
			PROTOCOL_LISTENING,
			PROTOCOL_ADDRESS0_EXPECTED,
			PROTOCOL_ADDRESS1_EXPECTED,
			PROTOCOL_ADDRESS2_EXPECTED,
			PROTOCOL_ADDRESS3_EXPECTED,
			PROTOCOL_COMMAND0_EXPECTED,
			PROTOCOL_COMMAND1_EXPECTED,
			PROTOCOL_LENGTH0_EXPECTED,
			PROTOCOL_LENGTH1_EXPECTED,
			PROTOCOL_DATA_EXPECTED,
			PROTOCOL_CRC0_EXPECTED,
			PROTOCOL_CRC1_EXPECTED,
			PROTOCOL_PACKET_BEING_PROCESSED,
			PROTOCOL_ERROR
		}

		protected enum Stuff
		{
			STUFF_IDLE,
			STUFF_BYTE1,
			STUFF_NEWBYTE,
			STUFF_END
		}

		#endregion

		#region Constants

		protected static readonly ushort[] CRC16Table = new ushort[] {
            0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
            0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
            0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
            0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
            0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
            0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
            0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
            0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
            0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
            0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
            0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
            0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
            0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
            0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
            0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
            0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
            0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
            0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
            0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
            0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
            0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
            0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
            0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
            0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
            0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
            0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
            0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
            0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
            0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
            0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
            0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
            0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040 };

		protected const ushort CRC16_SEED = 0xFFFF;

		// Служебные байты:
		protected const byte SLIP_END = 0xC0;
		protected const byte SLIP_ESC = 0xDB;
		protected const byte SLIP_ESC_END = 0xDC;
		protected const byte SLIP_ESC_ESC = 0xDD;

		protected const int TYPE_ENERGOMONITOR32 = 0x01;
		protected const int TYPE_ENERGOTESTER = 0x02;
		protected const int SENDERTYPE_ENERGOMONITOR32 = 0x10;
		protected const int SENDERTYPE_ENERGOTESTER = 0x20;

		#endregion

		#region Events

		//protected enum ReadEvent
		//{
		//    TIMEUOT = 0,
		//    DISCONNECTED = 1,
		//    RECEIVED = 2
		//}

		//protected WaitHandle[] readEvents_ = new WaitHandle[]
		//    {
		//        new AutoResetEvent(false),
		//        new AutoResetEvent(false),
		//        new AutoResetEvent(false)
		//    };

		#endregion

		protected bool bWasReadTimeout_ = false;
		protected bool bPacketReceived_ = false;
		protected bool bDeviceDisconnected_ = false;

		//protected bool terminateRxThread_ = false;
		protected bool innerError_ = false;
		protected UInt16 devAddress_ = 0;

		protected StructReply actualReply_;
		protected static Protocol protocolState_;
		protected static Stuff stuffState_;

		//protected Thread rxThread_ = null;

		#region Timer

		//protected Timer timerRead_ = null;
		protected System.Timers.Timer timerRead_;
		protected UInt16 timeout_;

		protected void SetTimeoutCounter(UInt16 timeout)
		{
			EmService.WriteToLogGeneral("SetTimeoutCounter was called  " + DateTime.Now.ToString());
			timeout_ = timeout;
			if (timerRead_ != null)
			{
				if (timerRead_.Enabled) timerRead_.Stop();
				timerRead_.Dispose();
			}

			//timerRead_ = new System.Threading.Timer(TimerTimeoutHandler, null, timeout_ * 1000, timeout_ * 1000 /*Timeout.Infinite*/);
			timerRead_ = new System.Timers.Timer(timeout_ * 1000);
			timerRead_.Elapsed += new System.Timers.ElapsedEventHandler(TimerTimeoutHandler);
		}

		protected void ResetTimeoutCounter()
		{
			EmService.WriteToLogGeneral("ResetTimeoutCounter was called  " + DateTime.Now.ToString());
			if (timerRead_ != null)
			{
				if (timerRead_.Enabled) timerRead_.Stop();
				timerRead_.Dispose();
			}

			//timerRead_ = new System.Threading.Timer(TimerTimeoutHandler, null, timeout_ * 1000, timeout_ * 1000 /*Timeout.Infinite*/);
			timerRead_ = new System.Timers.Timer(timeout_ * 1000);
			timerRead_.Elapsed += new System.Timers.ElapsedEventHandler(TimerTimeoutHandler);
		}

		protected void DeleteTimeoutCounter()
		{
			if (timerRead_ != null)
			{
				if (timerRead_.Enabled) timerRead_.Stop();
				timerRead_.Dispose();
			}
		}

		//protected void TimerTimeoutHandler(object statusInfo)
		protected void TimerTimeoutHandler(object source, System.Timers.ElapsedEventArgs e)
		{
			//AutoResetEvent eventTimeout = (AutoResetEvent)readEvents_[(int)ReadEvent.TIMEUOT];
			//eventTimeout.Set();
			bWasReadTimeout_ = true;
			EmService.WriteToLogFailed("TimerTimeoutHandler was called  " + DateTime.Now.ToString());
		}

		#endregion

		#region Constructor

		public EmPortSLIP(EmDeviceType devType, UInt16 devAddress, EmPortType portType, IntPtr hMainWnd) :
			base(devType, portType, hMainWnd)
		{
			//terminateRxThread_ = false;

			devAddress_ = devAddress;
			//InputFunc = RxFunction; //InputFuncPointer = &RxFunction;

			actualReply_ = new StructReply();
		}

		#endregion

		/// <summary>
		/// send query and receive answer into the buffer
		/// </summary>
		public ExchangeResult ReadData(UInt16 command, ref byte[] buffer, List<UInt32> list_params)
		{
			try
			{
				//terminateRxThread_ = false;
				int commandTimeout = 100;

				// sending query
				List<byte> query_list = new List<byte>();
				byte[] query_buffer = null;

				// идентификатор запроса AVG. нужен чтобы распознавать нужные пакеты от прибора
				UInt16 id_avg_query = 0;
				// тип запроса для команды COMMAND_AverageArchiveQuery
				UInt16 queryType = 0;

				UInt16 currentDevType = TYPE_ENERGOMONITOR32;
				if (devType_ == EmDeviceType.ETPQP) currentDevType = TYPE_ENERGOTESTER;

				#region Create Packet

				// формируем пакет
				byte btemp0;
				query_list.Add(SLIP_END);
				//uint dwtemp0 = (0xFFFF) | ((TYPE_ENERGOMONITOR32 & 0x000F) << (16 + 8));
				UInt32 dwtemp0 = (UInt32)(devAddress_) | ((UInt32)(currentDevType & 0x000F) << (16 + 8));
				btemp0 = (byte)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.Add(btemp0);
				btemp0 = (byte)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.Add(btemp0);
				btemp0 = (byte)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.Add(btemp0);
				btemp0 = (byte)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.Add(btemp0);

				query_list.Add((byte)(command & 0xFF));        // команда
				query_list.Add((byte)((command >> 8) & 0xFF));

				// заполняем длину данных и сами данные
				int datalen;
				switch (command)
				{
					case (ushort)EmCommands.COMMAND_ReadQualityDatesByObject:
						if (list_params.Count >= 1)
						{
							// длина данных
							datalen = 2;
							query_list.Add((byte)((datalen >> 0) & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// object id
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadRegistrationByIndex:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive3SecByIndex:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive10MinByIndex:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive2HourByIndex:
					case (ushort)EmCommands.COMMAND_ReadDSIArchivesByRegistration:
						if (list_params.Count >= 1)
						{
							// длина данных
							datalen = 4;
							query_list.Add((byte)((datalen >> 0) & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// record id or registration id
							query_list.Add((byte)((list_params[0]) & 0xFF));
							query_list.Add((byte)((list_params[0] / 0x100) & 0xFF));
							query_list.Add((byte)((list_params[0] / 0x10000) & 0xFF));
							query_list.Add((byte)((list_params[0] / 0x1000000) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadAverageArchive3SecMinMaxIndices:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive10MinMinMaxIndices:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive2HourMinMaxIndices:
						if (list_params.Count >= 1)
						{
							// длина данных
							datalen = 4;
							query_list.Add((byte)((datalen >> 0) & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// record id or registration id
							query_list.Add((byte)((list_params[0]) & 0xFF));
							query_list.Add((byte)((list_params[0] / 0x100) & 0xFF));
							query_list.Add((byte)((list_params[0] / 0x10000) & 0xFF));
							query_list.Add((byte)((list_params[0] / 0x1000000) & 0xFF));
						}
						commandTimeout = 500;
						break;

					case (ushort)EmCommands.COMMAND_ReadRegistrationArchiveByIndex:
						if (list_params.Count >= 2)
						{
							// the length of data
							datalen = 6;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// data:
							// absolute index of the pqp archive
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 16) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 24) & 0xFF));
							// the number of the segment to read
							query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadQualityEntry:
						if (list_params.Count >= 3)
						{
							// длина данных
							datalen = 4;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							query_list.Add((byte)(list_params[2] & 0xFF));
							query_list.Add((byte)(list_params[1] & 0xFF));
						}
						else
						{
							// длина данных
							query_list.Add(0);
							query_list.Add(0);
						}
						break;

					//case EmCommands.COMMAND_ReadQualityEntryObjectDemand:
					//	if (list_params.Count >= 4)
					//	{
					//		// длина данных
					//		datalen = 6;
					//		query_list.Add((byte)(datalen & 0xFF));
					//		query_list.Add((byte)((datalen >> 8) & 0xFF));
					//		// данные
					//		query_list.Add((byte)((other_params[0] >> 0) & 0xFF));
					//		query_list.Add((byte)((other_params[0] >> 8) & 0xFF));
					//		query_list.Add((byte)(other_params[2] & 0xFF));
					//		query_list.Add((byte)(other_params[1] & 0xFF));
					//		// номер объекта
					//		query_list.Add((byte)((other_params[3] >> 0) & 0xFF));
					//		query_list.Add((byte)((other_params[3] >> 8) & 0xFF));
					//	}
					//	else
					//	{
					//		// длина данных
					//		query_list.Add(0);
					//		query_list.Add(0);
					//	}
					//	break;

					case (ushort)EmCommands.COMMAND_ReadQualityEntryByTimestampByObject:
						if (list_params.Count >= 7)
						{
							// длина данных
							datalen = 10;	// 5 слов
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							query_list.Add((byte)(list_params[2] & 0xFF));
							query_list.Add((byte)(list_params[1] & 0xFF));
							query_list.Add((byte)(list_params[4] & 0xFF));
							query_list.Add((byte)(list_params[3] & 0xFF));
							query_list.Add((byte)(list_params[5] & 0xFF));
							query_list.Add(0);
							// номер объекта
							query_list.Add((byte)((list_params[6] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[6] >> 8) & 0xFF));
						}
						else
						{
							// длина данных
							query_list.Add(0);
							query_list.Add(0);
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadSystemData:
						if (list_params.Count > 0)
						{
							// длина данных
							datalen = 2 + 64;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							for (int i = 0; i < 64; i++)
								query_list.Add((byte)0xAA);
						}
						else
						{
							// длина данных
							query_list.Add(0);
							query_list.Add(0);
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadEventLogger:
						if (list_params.Count >= 2)
						{
							// длина данных
							datalen = 6;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// номер записи
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 16) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 24) & 0xFF));
							// кол-во записей
							query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadDipSwellArchive:
						if (list_params.Count >= 3)
						{
							// длина данных
							datalen = 8;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// фаза
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							// номер записи
							query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 16) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 24) & 0xFF));
							// кол-во записей
							query_list.Add((byte)((list_params[2] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[2] >> 8) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadDipSwellArchiveByObject:
						if (list_params.Count >= 4)
						{
							// длина данных
							datalen = 10;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// фаза
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							// номер записи
							query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 16) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 24) & 0xFF));
							// кол-во записей
							query_list.Add((byte)((list_params[2] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[2] >> 8) & 0xFF));
							// номер объекта
							query_list.Add((byte)((list_params[3] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[3] >> 8) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadDipSwellIndexByStartTimestamp:
					case (ushort)EmCommands.COMMAND_ReadDipSwellIndexByEndTimestamp:
						if (list_params.Count >= 7)
						{
							// длина данных
							datalen = 10;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// фаза
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							// год
							query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
							// день
							query_list.Add((byte)list_params[3]);
							// месяц
							query_list.Add((byte)list_params[2]);
							// минуты
							query_list.Add((byte)list_params[5]);
							// часы
							query_list.Add((byte)list_params[4]);
							// секунды
							query_list.Add((byte)list_params[6]);
							query_list.Add(0);
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject:
						if (list_params.Count >= 13)
						{
							// длина данных
							datalen = 18;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные:
							// дата начала:
							// год
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							// день
							query_list.Add((byte)list_params[2]);
							// месяц
							query_list.Add((byte)list_params[1]);
							// минуты
							query_list.Add((byte)list_params[4]);
							// часы
							query_list.Add((byte)list_params[3]);
							// секунды
							query_list.Add((byte)list_params[5]);
							query_list.Add(0);
							// дата окончания:
							// год
							query_list.Add((byte)((list_params[6] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[6] >> 8) & 0xFF));
							// день
							query_list.Add((byte)list_params[8]);
							// месяц
							query_list.Add((byte)list_params[7]);
							// минуты
							query_list.Add((byte)list_params[10]);
							// часы
							query_list.Add((byte)list_params[9]);
							// секунды
							query_list.Add((byte)list_params[11]);
							query_list.Add(0);
							// object id
							query_list.Add((byte)((list_params[12] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[12] >> 8) & 0xFF));
						}
						break;

					//case EmCommands.COMMAND_ReadDipSwellIndexByStartTimestampByObject:
					//case EmCommands.COMMAND_ReadDipSwellIndexByEndTimestampByObject:
					//	if (list_params.Count >= 8)
					//	{
					//		// длина данных
					//		datalen = 12;
					//		query_list.Add((byte)(datalen & 0xFF));
					//		query_list.Add((byte)((datalen >> 8) & 0xFF));
					//		// данные
					//		// фаза
					//		query_list.Add((byte)((other_params[0] >> 0) & 0xFF));
					//		query_list.Add((byte)((other_params[0] >> 8) & 0xFF));
					//		// год
					//		query_list.Add((byte)((other_params[1] >> 0) & 0xFF));
					//		query_list.Add((byte)((other_params[1] >> 8) & 0xFF));
					//		// день
					//		query_list.Add((byte)other_params[3]);
					//		// месяц
					//		query_list.Add((byte)other_params[2]);
					//		// минуты
					//		query_list.Add((byte)other_params[5]);
					//		// часы
					//		query_list.Add((byte)other_params[4]);
					//		// секунды
					//		query_list.Add((byte)other_params[6]);
					//		query_list.Add(0);
					//		// object id
					//		query_list.Add((byte)((other_params[7] >> 0) & 0xFF));
					//		query_list.Add((byte)((other_params[7] >> 8) & 0xFF));
					//	}
					//	break;

					case (ushort)EmCommands.COMMAND_ReadAverageArchive3SecIndices:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive10MinIndices:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive2HourIndices:
						if (list_params.Count >= 1)
						{
							// длина данных
							datalen = 4;
							query_list.Add((byte)((datalen >> 0) & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// registration id
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 16) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 24) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadAverageArchive3SecIndexByDateTime:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive10MinIndexByDateTime:
					case (ushort)EmCommands.COMMAND_ReadAverageArchive2HourIndexByDateTime:
						if (list_params.Count >= 8)
						{
							// длина данных
							datalen = 32;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// registration id
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 16) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 24) & 0xFF));
							// the device uses only local time so we can put zero to uts fields
							// day
							query_list.Add((byte)0);
							query_list.Add((byte)0);
							// month
							query_list.Add((byte)0);
							query_list.Add((byte)0);
							// year
							query_list.Add((byte)0);
							query_list.Add((byte)0);
							// hour
							query_list.Add((byte)0);
							query_list.Add((byte)0);
							// min
							query_list.Add((byte)0);
							query_list.Add((byte)0);
							// sec
							query_list.Add((byte)0);
							query_list.Add((byte)0);
							// day
							query_list.Add((byte)((list_params[3] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[3] >> 8) & 0xFF));
							// month
							query_list.Add((byte)((list_params[2] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[2] >> 8) & 0xFF));
							// year
							query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
							// hour
							query_list.Add((byte)((list_params[4] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[4] >> 8) & 0xFF));
							// min
							query_list.Add((byte)((list_params[5] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[5] >> 8) & 0xFF));
							// sec
							query_list.Add((byte)((list_params[6] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[6] >> 8) & 0xFF));
							// it won't be used
							query_list.Add((byte)0);
							query_list.Add((byte)0);
							// TimeZone
							query_list.Add((byte)((list_params[7] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[7] >> 8) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_Read3secArchiveByTimestamp:
					case (ushort)EmCommands.COMMAND_Read1minArchiveByTimestamp:
					case (ushort)EmCommands.COMMAND_Read30minArchiveByTimestamp:
						if (list_params.Count >= 6)
						{
							// длина данных
							datalen = 8;
							query_list.Add((byte)(datalen & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// год
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							// день
							query_list.Add((byte)list_params[2]);
							// месяц
							query_list.Add((byte)list_params[1]);
							// минуты
							query_list.Add((byte)list_params[4]);
							// часы
							query_list.Add((byte)list_params[3]);
							// секунды
							query_list.Add((byte)list_params[5]);
							query_list.Add(0);
						}
						break;

					case (ushort)EmCommands.COMMAND_Read3secArchiveByTimestampObjectDemand:
					case (ushort)EmCommands.COMMAND_Read1minArchiveByTimestampObjectDemand:
					case (ushort)EmCommands.COMMAND_Read30minArchiveByTimestampObjectDemand:
						if (list_params.Count >= 7)
						{
							// длина данных
							datalen = 10;
							query_list.Add((byte)((datalen >> 0) & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// год
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
							// день
							query_list.Add((byte)list_params[2]);
							// месяц
							query_list.Add((byte)list_params[1]);
							// минуты
							query_list.Add((byte)list_params[4]);
							// часы
							query_list.Add((byte)list_params[3]);
							// секунды
							query_list.Add((byte)list_params[5]);
							query_list.Add(0);
							// object id
							query_list.Add((byte)((list_params[6] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[6] >> 8) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand:
						if (list_params.Count >= 1)
						{
							// длина данных
							datalen = 2;
							query_list.Add((byte)((datalen >> 0) & 0xFF));
							query_list.Add((byte)((datalen >> 8) & 0xFF));
							// данные
							// object id
							query_list.Add((byte)((list_params[0] >> 0) & 0xFF));
							query_list.Add((byte)((list_params[0] >> 8) & 0xFF));
						}
						break;

					case (ushort)EmCommands.COMMAND_AverageArchiveQuery:
						if (list_params.Count >= 2)
						{
							queryType = (UInt16)list_params[0];
							switch (queryType)
							{
								case (ushort)QueryAvgType.AAQ_TYPE_ResetAll:
									// длина данных
									datalen = 4;
									query_list.Add((byte)(datalen & 0xFF));
									query_list.Add((byte)((datalen >> 8) & 0xFF));

									// Тип запроса
									query_list.Add((byte)((queryType >> 0) & 0xFF));
									query_list.Add((byte)((queryType >> 8) & 0xFF));

									// Идентификатор запроса
									query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
									query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
									id_avg_query = (UInt16)list_params[1];

									break;
								case (ushort)QueryAvgType.AAQ_TYPE_Query:
									// длина данных
									datalen = 22 + (list_params.Count - 15) * 2;
									// здесь 15 - число элементов массива ДО списка параметров
									// 22 - длина в байтах этих 15-ти элементов массива

									query_list.Add((byte)(datalen & 0xFF));
									query_list.Add((byte)((datalen >> 8) & 0xFF));

									// данные
									// Тип запроса
									query_list.Add((byte)((queryType >> 0) & 0xFF));
									query_list.Add((byte)((queryType >> 8) & 0xFF));

									// Идентификатор запроса
									query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
									query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
									id_avg_query = (UInt16)list_params[1];

									// тип записи (3 сек, 1 мин, 30 мин)
									query_list.Add((byte)(((UInt16)list_params[2] >> 0) & 0xFF));
									query_list.Add((byte)(((UInt16)list_params[2] >> 8) & 0xFF));

									// дата начала периода
									query_list.Add((byte)(((UInt16)list_params[3] >> 0) & 0xFF));
									query_list.Add((byte)(((UInt16)list_params[3] >> 8) & 0xFF));
									// день
									query_list.Add((byte)list_params[5]);
									// месяц
									query_list.Add((byte)list_params[4]);
									// минуты
									query_list.Add((byte)list_params[7]);
									// часы
									query_list.Add((byte)list_params[6]);
									// секунды
									query_list.Add((byte)list_params[8]);
									query_list.Add(0);

									// дата окончания периода
									query_list.Add((byte)((list_params[9] >> 0) & 0xFF));
									query_list.Add((byte)((list_params[9] >> 8) & 0xFF));
									// день
									query_list.Add((byte)list_params[11]);
									// месяц
									query_list.Add((byte)list_params[10]);
									// минуты
									query_list.Add((byte)list_params[13]);
									// часы
									query_list.Add((byte)list_params[12]);
									// секунды
									query_list.Add((byte)list_params[14]);
									query_list.Add(0);

									for (int iParam = 15; iParam < list_params.Count; ++iParam)
									{
										query_list.Add((byte)((list_params[iParam] >> 0) & 0xFF));
										query_list.Add((byte)((list_params[iParam] >> 8) & 0xFF));
									}
									break;
								case (ushort)QueryAvgType.AAQ_TYPE_ReadStatus:
								default:
									// длина данных
									datalen = 4;
									query_list.Add((byte)(datalen & 0xFF));
									query_list.Add((byte)((datalen >> 8) & 0xFF));

									// Тип запроса
									query_list.Add((byte)((queryType >> 0) & 0xFF));
									query_list.Add((byte)((queryType >> 8) & 0xFF));

									// Идентификатор запроса
									query_list.Add((byte)((list_params[1] >> 0) & 0xFF));
									query_list.Add((byte)((list_params[1] >> 8) & 0xFF));
									id_avg_query = (UInt16)list_params[1];

									break;
							}
						}
						break;

					/*case COMMAND_Read3secValues:
						{
						}
					case COMMAND_Read1minValues:
						{
						}
					case COMMAND_Read30minValues:
						{
						}*/
					default:
						// длина данных
						query_list.Add(0);
						query_list.Add(0);
						break;
				}

				#endregion

				// резервируем два байта под crc
				query_list.Add(0);
				query_list.Add(0);

				UInt16 crc = CalcCrc16(ref query_list);	// calculating crc
				query_list[query_list.Count - 2] = (byte)(crc & 0xFF);
				query_list[query_list.Count - 1] = (byte)((crc >> 8) & 0xFF);

				// создаем буфер
				if (!CreateBufferToSendSLIP(ref query_list, ref query_buffer))
					return ExchangeResult.Other_Error;

				UInt16 timeOut;
				try
				{
					// отправляем запрос
					Thread.Sleep(100);
					string debugStr = "command " + EmService.GetEm32CommandText(command) + ": was sent ";
					debugStr += DateTime.Now.ToString();
					//if (portType_ == Modem)
					//{
					//    WriteModemInfo(debugStr.c_str());
					//}
					//else 
					EmService.WriteToLogGeneral(debugStr);

					if (portType_ == EmPortType.Modem || portType_ == EmPortType.GPRS)
					{
						timeOut = 60; //таймер срабатывает раз в секунду => таймаут = 1 min
						if (command == (ushort)EmCommands.COMMAND_AverageArchiveQuery && queryType == (ushort)QueryAvgType.AAQ_TYPE_Query)
						{
							timeOut = 180;  // 3 min
						}
					}
					else if (portType_ == EmPortType.Ethernet)
					{
						timeOut = 30;  // 30 sec
					}
					else
					{
						if (command == (ushort)EmCommands.COMMAND_AverageArchiveQuery && queryType == (ushort)QueryAvgType.AAQ_TYPE_Query)
						{
							timeOut = 300;  // 5 min
						}
						else
						{
							timeOut = 20; //таймаут = 20 сек

							if (devType_ == EmDeviceType.ETPQP)
								timeOut = 40; // 40 sec
						}
					}
					if (devType_ == EmDeviceType.ETPQP_A)
					{
						timeOut = 40; // 40 sec
						if (command == (ushort)EmCommands.COMMAND_ReadAverageArchive3SecMinMaxIndices ||
							command == (ushort)EmCommands.COMMAND_ReadAverageArchive10MinMinMaxIndices)
							timeOut = 150;
					}

					stuffState_ = Stuff.STUFF_IDLE;
					protocolState_ = Protocol.PROTOCOL_LISTENING;

					// for debug
					/*EmService.WriteToLogGeneral("Buffer to write:\n");
					for(int iDebug = 0; iDebug < bufLen; ++iDebug)
					{
						EmService.WriteToLogGeneral2(EmService.NumberToString(query_buffer[iDebug]));
						if((iDebug % 20) == 0) EmService.WriteToLogGeneral2("\n");
					}
					EmService.WriteToLogGeneral("End of buffer to write:\n");*/
					// end of for debug

					SetTimeoutCounter(timeOut);
					//ResetTimeoutCounter();
					//EmService.WriteToLogDebug("before write");
					ExchangeResult resW = Write(query_buffer);
					//EmService.WriteToLogDebug("after write");
					if (resW != ExchangeResult.OK)
					{
						EmService.WriteToLogFailed("Error EmPortSLIP.Write()");
						return ExchangeResult.Write_Error;					// error exit
					}
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Exeption in ReadData() before receiving");
					throw;
				}

				//#if DEBUG
				// если большой пакет - ждем чтобы прибор успел его получить
				//if (bufLen > 3000)
				//{
				//	if (portType_ == Modem ||
				//			((portType_ == COM || portType_ == Rs485) && portSpeed_ < 115200))
				//	{
				//		EmService.WriteToLogGeneral("query_buffer.Length > 3000: wait 15 sec");
				//		Sleep(15000);
				//	}
				//	else
				//	{
				//		EmService.WriteToLogGeneral("query_buffer.Length > 3000: wait 5 sec");
				//		Sleep(5000);
				//	}
				//}
				//#endif

				UInt16 currentSenderType = SENDERTYPE_ENERGOMONITOR32;
				if (devType_ == EmDeviceType.ETPQP) currentSenderType = SENDERTYPE_ENERGOTESTER;

				// получаем ответ
				bool bAllPacketsReceived = false, bTypeSenderIsCorrect = false;
				bool bCurPacketReceived = false;
				//byte* tempBuffer;

				/////////////////////////////////////////////////////////
				List<byte> listTempBuffer = new List<byte>();
				
				while (!bAllPacketsReceived)
				{
					protocolState_ = Protocol.PROTOCOL_LISTENING;
					stuffState_ = Stuff.STUFF_IDLE;
					while (!bCurPacketReceived)
					{
						byte c = 0x00;
						if (Read(ref c, commandTimeout) == 0)
							throw new TimeoutException();
						//EmService.WriteToLogGeneral("NEW BYTE: " + c.ToString());
						RxFunction(c);

						if(bWasReadTimeout_)
						{
							Thread.Sleep(1000);
							bWasReadTimeout_ = false;
							EmService.WriteToLogGeneral("bWasReadTimeout_ 1");
							//сбрасываем таймаут
							ResetTimeoutCounter();
							EmService.WriteToLogFailed("ReadData(): hEventCommunicationTimeout_ " + DateTime.Now.ToString());
							return ExchangeResult.Timeout_Error;
						}

						if (bPacketReceived_)
						{
							bPacketReceived_ = false;
							EmService.WriteToLogGeneral("bPacketReceived_ 1");
							bCurPacketReceived = true;
							//сбрасываем таймаут
							ResetTimeoutCounter();

							// проверяем прибором ли был отправлен пакет (только для em32)
							if (devType_ == EmDeviceType.EM32)
							{
								byte typeSender = (byte)(actualReply_.dwAddress >> 24);
								if ((typeSender & (byte)currentSenderType) == 0)
								{
									EmService.WriteToLogFailed("typeSender wrong!");
								}
								else
								{
									bTypeSenderIsCorrect = true;
								}
							}
							else
							{
								bTypeSenderIsCorrect = true;
							}
						}

						if (bDeviceDisconnected_)
						{
							bDeviceDisconnected_ = false;
							EmService.WriteToLogGeneral("bDeviceDisconnected_ 1");
							//сбрасываем таймаут
							DeleteTimeoutCounter();
							// закрываем порт
							Close();

							EmService.WriteToLogFailed("ReadData(): hEventDisconnected_");
							return ExchangeResult.Disconnect_Error;
						}
					}
					bCurPacketReceived = false;//????????????????????????

					EmService.WriteToLogGeneral(EmService.GetEm32CommandText(command));
					EmService.WriteToLogGeneral(EmService.GetEm32CommandText(actualReply_.wCommand));
					EmService.WriteToLogGeneral(queryType.ToString());
					EmService.WriteToLogGeneral(devType_.ToString());

					////////////////////////////////
					// анализируем полученный пакет:
					////////////////////////////////
					// если получили ответ "Не найдены данные, удовлетворяющие условию", то выходим без
					// копирования буфера
					if (/*command != COMMAND_AverageArchiveQuery*/
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_OK)
					{
						EmService.WriteToLogGeneral("actualReply_.wCommand = COMMAND_OK");
						//break;
					}
					else if (actualReply_.wCommand == (ushort)EmCommands.COMMAND_NO_DATA)
					{
						EmService.WriteToLogGeneral("actualReply_.wCommand = COMMAND_NO_DATA");
						break;
					}
					else if (actualReply_.wCommand == (ushort)EmCommands.COMMAND_UNKNOWN_COMMAND ||
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_CRC_ERROR ||
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_BAD_DATA ||
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_BAD_PASSWORD ||
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_ACCESS_ERROR ||
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_CHECK_FAILED)
					{
						string answer = "actualReply_.wCommand = ";
						answer += EmService.GetEm32CommandText(actualReply_.wCommand);
						EmService.WriteToLogGeneral(answer);
						break;
					}
					else if (actualReply_.wCommand != command)	//??????????????
					{
						if (actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadDSIArchives &&
							command == (ushort)EmCommands.COMMAND_ReadDSIArchivesByRegistration)
						{
							// ok. do nothing
							int x = 0;
						}
						else
						{
							//SetTimeoutCounter(timeOut);
							//continue;
							return ExchangeResult.Other_Error;
						}
					}

					#region PQP
					//если пришел ответ не на ту команду, которая была запрошена, то игнорируем пакет
					if (devType_ == EmDeviceType.ETPQP)
					{
						if ((command != actualReply_.wCommand) &&
							(actualReply_.wCommand != (ushort)EmCommands.COMMAND_OK) &&
							!(command == (ushort)EmCommands.COMMAND_ReadQualityDatesByObject &&
								actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadQualityDates) &&
							!(command == (ushort)EmCommands.COMMAND_ReadQualityEntryByTimestampByObject &&
								actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadQualityEntry) &&
							!(command == (ushort)EmCommands.COMMAND_ReadDipSwellArchiveByObject &&
								actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadDipSwellArchive) &&
							//!(command == COMMAND_ReadDipSwellIndexByStartTimestampByObject &&
							//	actualReply_.wCommand == COMMAND_ReadDipSwellIndexByStartTimestamp) &&
							!(command == (ushort)EmCommands.COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject &&
								actualReply_.wCommand ==
										(ushort)EmCommands.COMMAND_ReadDipSwellIndexesByStartAndEndTimestampsByObject) &&
							//!(command == COMMAND_ReadDipSwellIndexByEndTimestampByObject &&
							//actualReply_.wCommand == COMMAND_ReadDipSwellIndexByEndTimestamp) &&
							!(command == (ushort)EmCommands.COMMAND_ReadEarliestAndLatestAverageTimestampObjectDemand &&
								actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadEarliestAndLatestAverageTimestamp) &&
							!(command == (ushort)EmCommands.COMMAND_Read3secArchiveByTimestampObjectDemand &&
								actualReply_.wCommand == (ushort)EmCommands.COMMAND_Read3secArchiveByTimestamp) &&
							!(command == (ushort)EmCommands.COMMAND_Read1minArchiveByTimestampObjectDemand &&
								actualReply_.wCommand == (ushort)EmCommands.COMMAND_Read1minArchiveByTimestamp) &&
							!(command == (ushort)EmCommands.COMMAND_Read30minArchiveByTimestampObjectDemand &&
								actualReply_.wCommand == (ushort)EmCommands.COMMAND_Read30minArchiveByTimestamp))
						{
							EmService.WriteToLogGeneral("ReadData(): unexpected command answer " +
								EmService.GetEm32CommandText(actualReply_.wCommand));
							SetTimeoutCounter(timeOut);
							continue;
						}
					}

					#endregion

					// усредненные, запрашиваемые через COMMAND_AverageArchiveQuery, могут приходить
					// несколькими пакетами, поэтому надо остаться в цикле для получения оставшихся пакетов.
					// то же самое относится к командам COMMAND_ReadAverageArchiveXXXIndices, COMMAND_ReadDSIArchivesByRegistration.
					// а в этот if мы попадаем во всех остальных случаях, чтобы выйти из цикла
					if (command != (ushort)EmCommands.COMMAND_AverageArchiveQuery &&
						actualReply_.wCommand != (ushort)EmCommands.COMMAND_AverageArchiveQuery &&
						command != (ushort)EmCommands.COMMAND_ReadAverageArchive3SecIndices &&
						actualReply_.wCommand != (ushort)EmCommands.COMMAND_ReadAverageArchive3SecIndices &&
						command != (ushort)EmCommands.COMMAND_ReadAverageArchive10MinIndices &&
						actualReply_.wCommand != (ushort)EmCommands.COMMAND_ReadAverageArchive10MinIndices &&
						command != (ushort)EmCommands.COMMAND_ReadAverageArchive2HourIndices &&
						actualReply_.wCommand != (ushort)EmCommands.COMMAND_ReadAverageArchive2HourIndices &&
						command != (ushort)EmCommands.COMMAND_ReadDSIArchivesByRegistration &&
						actualReply_.wCommand != (ushort)EmCommands.COMMAND_ReadDSIArchivesByRegistration)
					{
						if (actualReply_.wLength > 0x8000)
							EmService.WriteToLogFailed("!!!!!!!!!!ReadData(): actualReply_.wLength > 0x8000!");
						for (int iByte = 0; iByte < actualReply_.wLength; ++iByte)
							listTempBuffer.Add(actualReply_.bData[iByte]);
						break;
					}

					// если запрашивались не усредненные, а получили усредненные, то игнорируем пакет
					if (command != (ushort)EmCommands.COMMAND_AverageArchiveQuery &&
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_AverageArchiveQuery)
					{
						EmService.WriteToLogFailed("ReadData(): unexpected AVG packet!");
						SetTimeoutCounter(timeOut);
						continue;
					}

					// если запрашивались не индексы, а получили индексы, то игнорируем пакет
					if ((command != (ushort)EmCommands.COMMAND_ReadAverageArchive3SecIndices &&
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadAverageArchive3SecIndices) ||
						(command != (ushort)EmCommands.COMMAND_ReadAverageArchive10MinIndices &&
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadAverageArchive10MinIndices) ||
						(command != (ushort)EmCommands.COMMAND_ReadAverageArchive2HourIndices &&
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadAverageArchive2HourIndices))
					{
						EmService.WriteToLogFailed("ReadData(): unexpected AVG indexes!");
						SetTimeoutCounter(timeOut);
						continue;
					}

					// если запрашивались не многопакетные провалы, а получили их, то игнорируем пакет
					if (command != (ushort)EmCommands.COMMAND_ReadDSIArchivesByRegistration &&
						actualReply_.wCommand == (ushort)EmCommands.COMMAND_ReadDSIArchivesByRegistration)
					{
						EmService.WriteToLogFailed("ReadData(): unexpected DNS packet!");
						SetTimeoutCounter(timeOut);
						continue;
					}

					// если запрашивались усредненные, а получили что-то другое - выходим
					if (command == (ushort)EmCommands.COMMAND_AverageArchiveQuery &&
						actualReply_.wCommand != (ushort)EmCommands.COMMAND_AverageArchiveQuery)
					{
						EmService.WriteToLogFailed("ReadData(): needed AVG packet!");
						break;
					}

					#region NOT EtPQP-A

					if (devType_ != EmDeviceType.ETPQP_A)
					{
						// если запрашивались усредненные, но id запроса не совпадает с id ответа, 
						// то игнорируем пакет
						UInt16 curId = Conversions.bytes_2_ushort(ref actualReply_.bData, 2);
						if (id_avg_query != curId)
						{
							EmService.WriteToLogFailed(string.Format("ReadData(): invalid id AVG query! curId = {0}  id_avg_query = {1}",
								curId, id_avg_query));
							SetTimeoutCounter(timeOut);
							continue;
						}

						// если тип запроса и тип ответа не совпадают, то игнорируем пакет
						UInt16 curType = Conversions.bytes_2_ushort(ref actualReply_.bData, 0);
						if (curType != queryType)
						{
							EmService.WriteToLogFailed(string.Format("ReadData(): invalid AVG query type!  curType = {0}, queryType = {1}",
								curType, queryType));
							SetTimeoutCounter(timeOut);
							continue;
						}

						// если это была команда Reset для средних, то ждем немного, 
						// чтобы прибор успел среагировать
						if (queryType == 1 && curType == 1 /*ResetAll*/)
						{
							EmService.WriteToLogGeneral("queryType == 1 && curType == 1");
							Thread.Sleep(1500);
						}

						// если запрашивались усредненные, то проверяем последний ли это сегмент
						if (queryType == (ushort)QueryAvgType.AAQ_TYPE_Query)
						{
							UInt16 numSegment = Conversions.bytes_2_ushort(ref actualReply_.bData, 4);
							EmService.WriteToLogGeneral("AVG number of segment: " + numSegment);

							if (numSegment != 0xFFFF)
								bAllPacketsReceived = false;
							else
								bAllPacketsReceived = true;

							Thread.Sleep(0);

							// получен очередной сегмент, поэтому меняем прогрессбар
							Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 1, 1, 0);
						}
						else
							bAllPacketsReceived = true;

						// сохраняем пакет усредненных:
						if (actualReply_.wLength > 0x8000)
							EmService.WriteToLogFailed(
							"!!!!!!!!!!ReadData(): actualReply_.wLength > 0x8000!");
						for (int iByte = 0; iByte < actualReply_.wLength; ++iByte)
							listTempBuffer.Add(actualReply_.bData[iByte]);
					}

					#endregion

					if (devType_ == EmDeviceType.ETPQP_A)
					{
						// если запрашивались индексы усредненных или провалы, то проверяем последний ли это пакет
						if (command == (ushort)EmCommands.COMMAND_ReadAverageArchive3SecIndices ||
							command == (ushort)EmCommands.COMMAND_ReadAverageArchive10MinIndices ||
							command == (ushort)EmCommands.COMMAND_ReadAverageArchive2HourIndices ||
							command == (ushort)EmCommands.COMMAND_ReadDSIArchivesByRegistration)
						{
							EmService.WriteToLogGeneral("AVG indexes or DNS packet length: " + actualReply_.wLength);

							if (actualReply_.wLength > 0)
								bAllPacketsReceived = false;
							else
								bAllPacketsReceived = true;

							Thread.Sleep(0);
						}
						else
							bAllPacketsReceived = true;

						// сохраняем пакет индексов усредненных или провалов:
						if (actualReply_.wLength > 0)
						{
							if (command == (ushort)EmCommands.COMMAND_ReadAverageArchive3SecIndices ||
								command == (ushort)EmCommands.COMMAND_ReadAverageArchive10MinIndices ||
								command == (ushort)EmCommands.COMMAND_ReadAverageArchive2HourIndices ||
								command == (ushort)EmCommands.COMMAND_ReadDSIArchivesByRegistration)
							{
								for (int iByte = 0; iByte < actualReply_.wLength; ++iByte)
									listTempBuffer.Add(actualReply_.bData[iByte]);
							}
						}
					}

					//если будем продолжать прием пакетов, ставим таймаут заново
					if (!bAllPacketsReceived)
					{
						SetTimeoutCounter(timeOut);
					}
				}

				// если удалось что-то считать, сохраняем данные
				if (listTempBuffer.Count > 0)
				{
					buffer = new byte[listTempBuffer.Count];
					listTempBuffer.CopyTo(buffer);
				}
				else
					buffer = null;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmPortSLIP.ReadData()");
				return ExchangeResult.Other_Error;
			}

			return ExchangeResult.OK;
		}

		/// <summary>
		/// send query and receive answer into the buffer
		/// </summary>
		public ExchangeResult WriteData(UInt16 command, ref byte[] buffer)
		{
			try
			{
				int bufLength = 0;
				if (buffer != null) bufLength = buffer.Length;

				//terminateRxThread_ = false;

				// sending query
				List<byte> query_list = new List<byte>();
				byte[] query_buffer = null;

				// идентификатор запроса AVG. нужен чтобы распознавать нужные пакеты от прибора
				UInt16 id_avg_query = 0;
				// тип запроса для команды COMMAND_AverageArchiveQuery
				UInt16 queryType = 0;

				UInt16 currentDevType = TYPE_ENERGOMONITOR32;
				if (devType_ == EmDeviceType.ETPQP) currentDevType = TYPE_ENERGOTESTER;

				#region Create Packet

				// формируем пакет
				byte btemp0;
				query_list.Add(SLIP_END);
				//query_list.Add(SLIP_END);
				//query_list.Add(SLIP_END);
				//uint dwtemp0 = (0xFFFF) | ((TYPE_ENERGOMONITOR32 & 0x000F) << (16 + 8));
				UInt32 dwtemp0 = (UInt32)(devAddress_) | ((UInt32)(currentDevType & 0x000F) << (16 + 8));
				btemp0 = (byte)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.Add(btemp0);
				btemp0 = (byte)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.Add(btemp0);
				btemp0 = (byte)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.Add(btemp0);
				btemp0 = (byte)(dwtemp0 & 0xFF); dwtemp0 >>= 8; query_list.Add(btemp0);

				query_list.Add((byte)(command & 0xFF));        // команда
				query_list.Add((byte)((command >> 8) & 0xFF));

				// заполняем длину данных и сами данные
				switch (command)
				{
					case (ushort)EmCommands.COMMAND_WriteSets:
						if (bufLength > 0)
						{
							// длина данных
							query_list.Add((byte)((bufLength >> 0) & 0xFF));
							query_list.Add((byte)((bufLength >> 8) & 0xFF));
							// данные
							for (int iByte = 0; iByte < bufLength; ++iByte)
							{
								query_list.Add(buffer[iByte]);
							}
						}
						break;

					case (ushort)EmCommands.COMMAND_WriteSystemData:
						if (bufLength > 0)
						{
							// длина данных
							query_list.Add((byte)((bufLength >> 0) & 0xFF));
							query_list.Add((byte)((bufLength >> 8) & 0xFF));
							// данные
							for (int iByte = 0; iByte < bufLength; ++iByte)
							{
								query_list.Add(buffer[iByte]);
							}
						}
						else
						{
							// длина данных
							query_list.Add(0);
							query_list.Add(0);
						}
						break;

					case (ushort)EmCommands.COMMAND_RestartInterface:
						// длина данных
						query_list.Add(0);
						query_list.Add(0);
						break;

					default:
						// длина данных
						query_list.Add(0);
						query_list.Add(0);
						break;
				}

				#endregion

				// резервируем два байта под crc
				query_list.Add(0);
				query_list.Add(0);

				UInt16 crc = CalcCrc16(ref query_list);	// calculating crc
				query_list[query_list.Count - 2] = (byte)(crc & 0xFF);
				query_list[query_list.Count - 1] = (byte)((crc >> 8) & 0xFF);

				// создаем буфер
				if (!CreateBufferToSendSLIP(ref query_list, ref query_buffer))
					return ExchangeResult.Other_Error;

				UInt16 timeOut;
				try
				{
					// отправляем запрос
					Thread.Sleep(100);
					string debugStr = "command " + EmService.GetEm32CommandText(command) + ": was sent ";
					debugStr += DateTime.Now.ToString();
					//if (portType_ == Modem)
					//{
					//    WriteModemInfo(debugStr.c_str());
					//}
					//else 
					EmService.WriteToLogGeneral(debugStr);

					if (portType_ == EmPortType.Modem || portType_ == EmPortType.GPRS)
					{
						timeOut = 60; //таймер срабатывает раз в секунду => таймаут = 1 min
						if (command == (ushort)EmCommands.COMMAND_AverageArchiveQuery && queryType == (ushort)QueryAvgType.AAQ_TYPE_Query)
						{
							timeOut = 180;  // 3 min
						}
					}
					else if (portType_ == EmPortType.Ethernet)
					{
						timeOut = 30;  // 30 sec
					}
					else
					{
						if (command == (ushort)EmCommands.COMMAND_AverageArchiveQuery && queryType == (ushort)QueryAvgType.AAQ_TYPE_Query)
						{
							timeOut = 300;  // 5 min
						}
						else
						{
							timeOut = 20; //таймаут = 20 сек

							if (devType_ == EmDeviceType.ETPQP)
								timeOut = 40; // 40 sec
						}
					}
					if (devType_ == EmDeviceType.ETPQP_A)
					{
						timeOut = 40; // 40 sec
						if (command == (ushort)EmCommands.COMMAND_ReadAverageArchive3SecMinMaxIndices ||
							command == (ushort)EmCommands.COMMAND_ReadAverageArchive10MinMinMaxIndices)
							timeOut = 150;
					}
					SetTimeoutCounter(timeOut);

					stuffState_ = Stuff.STUFF_IDLE;
					protocolState_ = Protocol.PROTOCOL_LISTENING;

					ExchangeResult resW = Write(query_buffer);
					if (resW != 0)
					{
						EmService.WriteToLogFailed("Error EmPortSLIP.Write()");
						return ExchangeResult.Write_Error;					// error exit
					}

					// если это была команда перезапуска USB, то выходим, т.к. на эту команду нет ответа
					if (command == (ushort)EmCommands.COMMAND_RestartInterface)
					{
						DeleteTimeoutCounter();
						return resW;
					}
				}
				catch (Exception ex)
				{
					EmService.WriteToLogFailed("Exeption in ReadData() before receiving");
					throw;
				}

				//UInt16 currentSenderType = SENDERTYPE_ENERGOMONITOR32;
				//if (devType_ == EmDeviceType.ETPQP) currentSenderType = SENDERTYPE_ENERGOTESTER;

				// получаем ответ
				bool bCurPacketReceived = false;
				//byte* tempBuffer;

				/////////////////////////////////////////////////////////
				List<byte> listTempBuffer = new List<byte>();

				protocolState_ = Protocol.PROTOCOL_LISTENING;
				stuffState_ = Stuff.STUFF_IDLE;
				while (!bCurPacketReceived)
				{
					byte c = 0x00;
					if (Read(ref c) == 0)
						throw new TimeoutException();
					//EmService.WriteToLogGeneral("NEW BYTE: " + c.ToString());
					RxFunction(c);

					if (bWasReadTimeout_)
					{
						bWasReadTimeout_ = false;
						EmService.WriteToLogGeneral("bWasReadTimeout_ 2");
						//сбрасываем таймаут
						ResetTimeoutCounter();
						EmService.WriteToLogFailed("ReadData(): hEventCommunicationTimeout_ " + DateTime.Now.ToString());
						return ExchangeResult.Timeout_Error;
					}

					if (bPacketReceived_)
					{
						bPacketReceived_ = false;
						EmService.WriteToLogGeneral("bPacketReceived_ 2");
						bCurPacketReceived = true;
						//сбрасываем таймаут
						ResetTimeoutCounter();

						// проверяем прибором ли был отправлен пакет (только для em32)
						//if (devType_ == EmDeviceType.EM32)
						//{
						//    byte typeSender = (byte)(actualReply_.dwAddress >> 24);
						//    if ((typeSender & (byte)currentSenderType) == 0)
						//    {
						//        EmService.WriteToLogFailed("typeSender wrong!");
						//    }
						//    else
						//    {
						//        bTypeSenderIsCorrect = true;
						//    }
						//}
						//else
						//{
						//    bTypeSenderIsCorrect = true;
						//}
					}

					if (bDeviceDisconnected_)
					{
						bDeviceDisconnected_ = false;
						EmService.WriteToLogGeneral("bDeviceDisconnected_ 2");
						//сбрасываем таймаут
						ResetTimeoutCounter();
						// закрываем порт
						Close();

						EmService.WriteToLogFailed("ReadData(): hEventDisconnected_");
						return ExchangeResult.Disconnect_Error;
					}
				}

				EmService.WriteToLogGeneral(EmService.GetEm32CommandText(command));
				EmService.WriteToLogGeneral(EmService.GetEm32CommandText(actualReply_.wCommand));
				EmService.WriteToLogGeneral(queryType.ToString());
				EmService.WriteToLogGeneral(devType_.ToString());

				////////////////////////////////
				// анализируем полученный пакет:
				////////////////////////////////
				if (actualReply_.wCommand == (ushort)EmCommands.COMMAND_OK)
				{
					EmService.WriteToLogGeneral("actualReply_.wCommand = COMMAND_OK");
				}
				else
				{
					EmService.WriteToLogGeneral("actualReply_.wCommand = " +
						EmService.GetEm32CommandText(actualReply_.wCommand));
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmPortSLIP.ReadData()");
				return ExchangeResult.Other_Error;
			}

			return ExchangeResult.OK;
		}

		protected void RxFunction(byte new_byte)
		{
			try
			{
				if (protocolState_ == Protocol.PROTOCOL_PACKET_BEING_PROCESSED)
				{
					while (true)
					{
						if (innerError_) break;
						if (protocolState_ != Protocol.PROTOCOL_PACKET_BEING_PROCESSED) break;
						Thread.Sleep(0);
					}
				}
				//__________________________________________________________________
				if (new_byte == SLIP_END)
				{
					stuffState_ = Stuff.STUFF_END;
				}
				//__________________________________________________________________
				switch (stuffState_)
				{
					//-------------------------------------------------------------	
					case Stuff.STUFF_IDLE:

						switch (new_byte)
						{
							//-------------------------------------------------------------	
							case SLIP_ESC:
								stuffState_ = Stuff.STUFF_BYTE1;
								break;
							//-------------------------------------------------------------	
							default:
								stuffState_ = Stuff.STUFF_NEWBYTE;
								break;
						}
						break;
					//-------------------------------------------------------------	
					case Stuff.STUFF_BYTE1:
						stuffState_ = Stuff.STUFF_IDLE;
						switch (new_byte)
						{
							//-------------------------------------------------------------	
							case SLIP_ESC_END:
								new_byte = SLIP_END;
								stuffState_ = Stuff.STUFF_NEWBYTE;
								break;
							//-------------------------------------------------------------	
							case SLIP_ESC_ESC:
								new_byte = SLIP_ESC;
								stuffState_ = Stuff.STUFF_NEWBYTE;
								break;
							//-------------------------------------------------------------	
							default:
								protocolState_ = Protocol.PROTOCOL_LISTENING;
								break;
						}
						break;
					//-------------------------------------------------------------	
					case Stuff.STUFF_END:
						stuffState_ = Stuff.STUFF_NEWBYTE;
						protocolState_ = Protocol.PROTOCOL_LISTENING;
						break;
					//-------------------------------------------------------------	
					default:
						stuffState_ = Stuff.STUFF_IDLE;
						protocolState_ = Protocol.PROTOCOL_LISTENING;
						break;
					//-------------------------------------------------------------	
				}
				//__________________________________________________________________
				if (stuffState_ == Stuff.STUFF_NEWBYTE)
				{
					stuffState_ = Stuff.STUFF_IDLE;
					switch (protocolState_)
					{
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_LISTENING:
							if (new_byte == SLIP_END)
								protocolState_ = Protocol.PROTOCOL_ADDRESS0_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_ADDRESS0_EXPECTED:
							/*if (new_byte == SLIP_END)
							{
								protocolState_ = PROTOCOL_ADDRESS0_EXPECTED;
								break;
							}*/
							actualReply_.dwAddress = 0x000000FF & (UInt32)new_byte;
							protocolState_ = Protocol.PROTOCOL_ADDRESS1_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_ADDRESS1_EXPECTED:
							actualReply_.dwAddress |= (UInt32)(new_byte << 8);
							protocolState_ = Protocol.PROTOCOL_ADDRESS2_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_ADDRESS2_EXPECTED:
							actualReply_.dwAddress |= (UInt32)(new_byte << 16);
							protocolState_ = Protocol.PROTOCOL_ADDRESS3_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_ADDRESS3_EXPECTED:
							actualReply_.dwAddress |= (UInt32)(new_byte << 24);
							protocolState_ = Protocol.PROTOCOL_COMMAND0_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_COMMAND0_EXPECTED:
							actualReply_.wCommand = (UInt16)new_byte;
							protocolState_ = Protocol.PROTOCOL_COMMAND1_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_COMMAND1_EXPECTED:
							actualReply_.wCommand |= ((UInt16)(new_byte << 8));
							protocolState_ = Protocol.PROTOCOL_LENGTH0_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_LENGTH0_EXPECTED:
							actualReply_.wLength = (UInt16)(0x00FF & (UInt16)new_byte);
							protocolState_ = Protocol.PROTOCOL_LENGTH1_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_LENGTH1_EXPECTED:
							actualReply_.wLength |= ((UInt16)(new_byte << 8));
							if (actualReply_.wLength == 0)
							{
								protocolState_ = Protocol.PROTOCOL_CRC0_EXPECTED;
								EmService.WriteToLogGeneral("actualReply_.wLength = 0");
							}
							else
							{
								protocolState_ = Protocol.PROTOCOL_DATA_EXPECTED;
								actualReply_.wCounter = 0;
							}
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_DATA_EXPECTED:
							if (actualReply_.wCounter > 0x8000) // 0x8000 - это размер буфера
								EmService.WriteToLogFailed("!!!!!!!!!!RxFunction(): actualReply_.wCounter > 0x8000!");
							actualReply_.bData[actualReply_.wCounter] = new_byte;
							actualReply_.wCounter++;
							if (actualReply_.wCounter == actualReply_.wLength)
							{
								protocolState_ = Protocol.PROTOCOL_CRC0_EXPECTED;
							}
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_CRC0_EXPECTED:
							actualReply_.wCRC = (UInt16)(0x00FF & (UInt16)new_byte);
							protocolState_ = Protocol.PROTOCOL_CRC1_EXPECTED;
							break;
						//-------------------------------------------------------------	
						case Protocol.PROTOCOL_CRC1_EXPECTED:
							actualReply_.wCRC |= (UInt16)(new_byte << 8);
							UInt16 wTemp;
							UInt16 i;
							wTemp = CRC16_SEED;
							crc16((byte)(((actualReply_.dwAddress) >> (8 * 0)) & 0xFF), ref wTemp);
							crc16((byte)(((actualReply_.dwAddress) >> (8 * 1)) & 0xFF), ref wTemp);
							crc16((byte)(((actualReply_.dwAddress) >> (8 * 2)) & 0xFF), ref wTemp);
							crc16((byte)(((actualReply_.dwAddress) >> (8 * 3)) & 0xFF), ref wTemp);
							crc16((byte)(((actualReply_.wCommand) >> (8 * 0)) & 0xFF), ref wTemp);
							crc16((byte)(((actualReply_.wCommand) >> (8 * 1)) & 0xFF), ref wTemp);
							crc16((byte)(((actualReply_.wLength) >> (8 * 0)) & 0xFF), ref wTemp);
							crc16((byte)(((actualReply_.wLength) >> (8 * 1)) & 0xFF), ref wTemp);
							if (actualReply_.wLength > 0x8000)
								EmService.WriteToLogFailed("!!!!!!!!!!RxFunction(): actualReply_.wLength > 0x8000!");
							for (i = 0; i < (actualReply_.wLength); i++)
								crc16(actualReply_.bData[i], ref wTemp);
							crc16((byte)(((actualReply_.wCRC) >> (8 * 0)) & 0xFF), ref wTemp);
							crc16((byte)(((actualReply_.wCRC) >> (8 * 1)) & 0xFF), ref wTemp);
							if (wTemp == 0)
							{
								EmService.WriteToLogGeneral(string.Format("command: {0}    crc ok!  {1}",
									EmService.GetEm32CommandText(actualReply_.wCommand), DateTime.Now.ToString()));

								protocolState_ = Protocol.PROTOCOL_PACKET_BEING_PROCESSED;
								//AutoResetEvent eventReceived = (AutoResetEvent)readEvents_[(int)ReadEvent.RECEIVED];
								//eventReceived.Set();
								bPacketReceived_ = true;
							}
							else
							{
								EmService.WriteToLogGeneral(string.Format("command: {0}    crc failed!  {1}",
									EmService.GetEm32CommandText(actualReply_.wCommand), DateTime.Now.ToString()));

								protocolState_ = Protocol.PROTOCOL_ERROR;
								//AutoResetEvent eventTimeout = (AutoResetEvent)readEvents_[(int)ReadEvent.TIMEUOT];
								//eventTimeout.Set();
								bWasReadTimeout_ = true;//??????????????????? почему здесь таймаут, надо другой тип ошибки
							}
							break;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in RxFunction()");
				throw;
			}
		}

		#region CRC Functions

		protected UInt16 CalcCrc16(ref List<byte> data)
		{
			UInt16 temp, crc = CRC16_SEED;
			try
			{
				// последние два байта не рассматриваем, т.к. они зарезервированы для crc,
				// первый байт тоже пропускаем
				for (int i = 1; i < data.Count - 2; ++i)
				{
					temp = (UInt16)((data[i] ^ crc) & 0xFF);
					//crc >>= 8;
					//crc ^= CRC16Table[temp];
					crc = (ushort)(crc >> 8);
					crc = (ushort)(crc ^ CRC16Table[temp]);
				}

				// from Misha
				//ushort temp = (ushort)(((b & 0x00FF) ^ crc) & 0x00FF);
				//crc = (ushort)(crc >> 8);
				//crc = (ushort)(crc ^ Em32Com.CRC16Table[temp]);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in CalcCrc16()");
				throw;
			}
			return crc;
		}

		UInt16 CalcCrc16(ref List<byte> buffer, int start, int len)
		{
			UInt16 temp, crc = CRC16_SEED;
			try
			{
				// последние два байта не рассматриваем, т.к. они зарезервированы для crc,
				// первый байт тоже пропускаем
				for (int i = start; i < (len + start - 2) && buffer.Count > len; ++i)
				{
					temp = (UInt16)((buffer[i] ^ crc) & 0xFF);
					crc >>= 8;
					crc ^= CRC16Table[temp];
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in CalcCrc16()");
				throw;
			}
			return crc;
		}

		static void crc16(byte Byte, ref UInt16 crc)
		{
			try
			{
				UInt16 temp = (UInt16)(((Byte & 0x00FF) ^ crc) & 0xFF);
				crc >>= 8;
				crc ^= CRC16Table[temp];
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in crc16()");
			}
		}

		#endregion

		protected bool CreateBufferToSendSLIP(ref List<byte> buf_temp, ref byte[] buf)
		{
			try
			{
				// начинаем с 1-го индекса, т.к. с 0-ым идет SLIP_END, который не надо менять
				for (int iByte = 1; iByte != buf_temp.Count; ++iByte)
				{
					switch (buf_temp[iByte])
					{
						case SLIP_END:
							buf_temp[iByte] = SLIP_ESC;
							buf_temp.Insert(++iByte, SLIP_ESC_END);
							break;
						case SLIP_ESC:
							buf_temp[iByte] = SLIP_ESC;
							buf_temp.Insert(++iByte, SLIP_ESC_ESC);
							break;
						default:
							break;
					}
				}
				buf = new byte[buf_temp.Count];
				buf_temp.CopyTo(buf);
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CreateBufferToSendSLIP():");
				return false;
			}
		}
	}

	public class EtPqpAUSB : EmPortSLIP
	{
		public EtPqpAUSB(EmDeviceType devType, UInt16 devAddress, IntPtr hMainWnd)
			: base(devType, devAddress, EmPortType.USB, hMainWnd)
		{
		}

		//void RxThread()
		//{
		//    try
		//    {
		//        byte c = 0;
		//        while (true)
		//        {
		//            if (terminateRxThread_) break;

		//            if (ReadByte(ref c) == 0)
		//                //throw new Exception("Em timeout");//TimeoutException();
		//                continue;
		//            RxFunction(c);
		//        }
		//    }
		//    catch (Exception ex)
		//    {
		//        EmService.DumpException(ex, "Error in EtPqpAUSB.RxThread()!");
		//        innerError_ = true;
		//        AutoResetEvent eventDisconnect = (AutoResetEvent)readEvents_[(int)ReadEvent.DISCONNECTED];
		//        eventDisconnect.Set();
		//    }
		//}

		//protected static void RxThreadStart(object param)
		//{
		//    ((EtPqpAUSB)param).RxThread();
		//}

		public override bool Open()
		{
			try
			{
				port_ = new UsbComm();
				if (port_.OpenConnection() != 0)
					return false;

				//rxThread_ = new Thread(new ParameterizedThreadStart(RxThreadStart));
				//rxThread_.Start(this);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPqpAUSB.Open():");
				return false;
			}
			return true;
		}

		public override bool Close()
		{
			try
			{
				//if (rxThread_ != null) rxThread_.Abort();
				//rxThread_ = null;
				if(timerRead_ != null)
					timerRead_.Dispose();

				if (port_.CloseConnection() != 0)
					return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPqpAUSB.Close():");
				return false;
			}
			return true;
		}

		protected override ExchangeResult Read(ref byte c)
		{
			try
			{
				int res = ReadByte(ref c, 100);
				if (res != 0) return ExchangeResult.Other_Error;
				else return ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPqpAUSB.ReadByte():");
				return ExchangeResult.Other_Error;
			}
		}

		protected override ExchangeResult Read(ref byte c, int timeout)
		{
			try
			{
				int res = ReadByte(ref c, timeout);
				if (res != 0) return ExchangeResult.Other_Error;
				else return ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPqpAUSB.ReadByte():");
				return ExchangeResult.Other_Error;
			}
		}

		protected unsafe int ReadByte(ref byte c, int timeout)
		{
			byte rx_byte = 0x00;
			try
			{
				c = 0x00;
				if (port_.ReadByte(&rx_byte, 1, timeout) != 0)
					return 0;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPqpAUSB.ReadByte():");
				return 0;
			}
			c = rx_byte;
			return 1;
		}

		public override ExchangeResult Write(byte[] buffer)
		{
			try
			{
				for (int iByte = 0; iByte < buffer.Length; ++iByte)
				{
					//if (WriteByte(buffer[iByte]) != 0)
					//	EmService.WriteToLogFailed("Error write bytes! (EtPqpAUSB)");
					WriteByte(buffer[iByte]);// ????????????????????? значение ошибки теряется, но иначе слишком много
						// записей в логе - часто бывает ошибка
				}
				return ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "EtPqpAUSB Write failed!");
				return ExchangeResult.Other_Error;
			}
		}

		protected unsafe int WriteByte(byte c)
		{
			try
			{
				if (port_.WriteByte(&c, 1) != 0)
					return 0;

				#region Misha code
				//byte tx_byte = 0x00;
				//if (start)
				//{
				//    tx_byte = c;
				//    if (port_.WriteByte(&tx_byte, 1) != 0)
				//        return 0;
				//}
				//else
				//{
				//    switch (c)
				//    {
				//        case SLIP_FEND:
				//            tx_byte = SLIP_FESC;
				//            if (port_.WriteByte(&tx_byte, 1) != 0)
				//                return 0;
				//            tx_byte = SLIP_TFEND;
				//            if (port_.WriteByte(&tx_byte, 1) != 0)
				//                return 0;
				//            break;
				//        case SLIP_FESC:
				//            tx_byte = SLIP_FESC;
				//            if (port_.WriteByte(&tx_byte, 1) != 0)
				//                return 0;
				//            tx_byte = SLIP_TFESC;
				//            if (port_.WriteByte(&tx_byte, 1) != 0)
				//                return 0;
				//            break;
				//        default:
				//            tx_byte = c;
				//            if (port_.WriteByte(&tx_byte, 1) != 0)
				//                return 0;
				//            break;
				//    }
				//}
				#endregion
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EtPqpAUSB.WriteByte():");
				return 0;
			}
			return 1;
		}
	}
}
