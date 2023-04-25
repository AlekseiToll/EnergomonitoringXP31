using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using EmServiceLib;
using DeviceIO.Memory;

namespace DeviceIO
{
	public class Em31KDevice : EmDevice
	{
		#region Constants and emuns

		protected enum EOperation
		{
			Read,
			Write
		}

		public const int bytes_per_page = 1024;
		internal const int pages_per_block = 32;
		internal const int start_arch_page = 336;
		internal const int end_arch_page = 8159;
		internal const byte normal_byte = 0x00;
		internal const byte synchro_byte = 0x02;

		internal const int events_bytes_per_record = 16;
		internal const int events_records_per_page = bytes_per_page / events_bytes_per_record;

		#endregion

		#region Fields

		DeviceCommonInfoEm33 devInfo_;

		#endregion

		#region Properties

		public DeviceCommonInfoEm33 DeviceInfo
		{
			get { return this.devInfo_; }
		}

		#endregion

		#region Constructors

		public Em31KDevice(EmPortType portType, ushort devAddr, bool auto, object[] port_params,
			IntPtr hMainWnd) 
			: base(EmDeviceType.EM31K, portType, devAddr, auto, port_params, hMainWnd)
		{
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
				EmService.DumpException(ex, "Error in Open Em31k device: ");
				throw;
			}
		}

		public bool ReadDeviceSerialNumber(out long serialNumber)
		{
			serialNumber = -1;

			byte[] buffer = null;

			try
			{
				EM31KPSystem pageSystem = new EM31KPSystem();

				if (Read(DeviceIO.Memory.EMemory.NAND,
							pageSystem.Address,
							pageSystem.Size,
							ref buffer, false) != 0) return false;
				if (!pageSystem.Parse(ref buffer)) return false;

				// parsing device serial number from the FRAM system page
				serialNumber = (long)((ushort)pageSystem.Data["DeviceNumber"]);
				return true;
			}
			finally
			{
				serialNumber_ = serialNumber;
			}
		}

		public override bool IsSomeArchiveExist()
		{
			return (devInfo_.Content != null && devInfo_.Content.Length > 0 && devInfo_.Content[0].DnsNum > 0);
		}

		public override ExchangeResult ReadDeviceInfo()
		{
			try
			{
				#region Reading Device info from the device

				// creating DeviceCommonInfo object
				devInfo_ = new DeviceCommonInfoEm33();

				byte[] buffer = null;

				// reading system page... if not successfully, return -1
				EM31KPSystem pageSystem = new EM31KPSystem();

				if (Read(DeviceIO.Memory.EMemory.NAND,
					pageSystem.Address,
					pageSystem.Size,
					ref buffer,
					false) != 0) return ExchangeResult.Other_Error;
				if (!pageSystem.Parse(ref buffer)) return ExchangeResult.Other_Error;

				devInfo_.SerialNumber = (ushort)pageSystem.Data["DeviceNumber"];
				devInfo_.InternalType = 1;		// 1 – Эм 3.1К, 3 – Эм 3.3Т и Эм 3.3Т1
				devInfo_.DeviceType = EmDeviceType.EM31K;
				devInfo_.Name = (string)pageSystem.Data["DeviceName"];
				devInfo_.Version = pageSystem.Data["Version"].ToString();

				#endregion

				#region Reading Main Records info from the device

				devInfo_.Content = new ContentsLineEm33[1];

				// filling in main records with
				devInfo_.Content[0] = new ContentsLineEm33();

				EM31KPMainRecord pageMainRecord = new EM31KPMainRecord();
				pageMainRecord.Index = 0;
				ExchangeResult res = Read(EMemory.NAND,
					pageMainRecord.Address,
					pageMainRecord.Size,
					ref buffer,
					false); 
				if(res != ExchangeResult.OK) return res;
				if (!pageMainRecord.Parse(ref buffer)) return ExchangeResult.Other_Error;

				devInfo_.Content[0].ObjectName = (string)pageMainRecord.Data["ObjName"];
				devInfo_.Content[0].ConnectionScheme = 
					(ConnectScheme)(ushort)pageMainRecord.Data["ConSch"];
				devInfo_.Content[0].F_Nominal = (ushort)pageMainRecord.Data["F_Nom"];
				devInfo_.Content[0].U_NominalLinear = (float)pageMainRecord.Data["U_NomLn"];
				devInfo_.Content[0].U_NominalPhase = (float)pageMainRecord.Data["U_NomPh"];
				devInfo_.Content[0].CommonBegin = (DateTime)pageMainRecord.Data["StartDateTime"];
				devInfo_.Content[0].CommonEnd = (DateTime)pageMainRecord.Data["EndDateTime"];
				devInfo_.Content[0].U_Limit = (float)pageMainRecord.Data["Ulimit"];
				devInfo_.Content[0].I_Limit = (float)pageMainRecord.Data["Ilimit"];

				devInfo_.Content[0].AvgExists = true;
				devInfo_.Content[0].AvgTime = (ushort)pageMainRecord.Data["TimeOfAveragingOut"];
				devInfo_.Content[0].AvgNum = (uint)pageMainRecord.Data["NumOfAvgRecords"];
				devInfo_.Content[0].AvgPagesNAND = new ushort[2] {
					(ushort) pageMainRecord.Data["AddrAVGBegin"],
					(ushort) pageMainRecord.Data["AddrAVGEnd"] };

				devInfo_.Content[0].DnsExists = true;
				devInfo_.Content[0].DnsNum = (uint)pageMainRecord.Data["NumOfDnoRecords"];
				devInfo_.Content[0].DnsPagesNAND = new ushort[2] {
					(ushort) pageMainRecord.Data["AddrDNOBegin"],
					(ushort) pageMainRecord.Data["AddrDNOEnd"] };

				devInfo_.Content[0].DnsTimer = (uint)pageMainRecord.Data["dtTimer"];

				#endregion

				#region Cheking main records, correcting data, reading times of start/end

				// FLAGS to continue trying to find
				bool bNeedAVG = devInfo_.Content[0].AvgTime == 3;	// avg
				bool bNeedDNO = devInfo_.Content[0].AvgTime == 4;	// events

				int iDNORecords = 0;	// number of scaned events records

				int iAvgRingMarker = 0; // last page address of the first avg record
				int iEventsRingMarker = 0; // last page address of the first events record

				// array with bool values to indicate is this content record is correct or not
				bool[] ValidContentRecords = new bool[devInfo_.Content.Length];

				for (int iRec = 0; iRec < devInfo_.Content.Length; iRec++)
				{
					// FLAGS to indicate that record are exists
					devInfo_.Content[iRec].AvgExists = false;	// avg
					devInfo_.Content[iRec].PqpExists = false;		// pqp
					devInfo_.Content[iRec].DnsExists = false;		// events

					// common verification of content record
					if (devInfo_.Content[iRec].CommonEnd == devInfo_.Content[iRec].CommonBegin ||
						devInfo_.Content[iRec].CommonEnd == DateTime.MinValue)
						continue;

					#region avg

					if (bNeedAVG && devInfo_.Content[iRec].AvgPagesNAND[1] != 0)
					{
						// number or pages per avg record
						// for avg = 5
						int PagesPerAvgRecord = 0;
						{
							if (devInfo_.Content[iRec].ConnectionScheme != ConnectScheme.Ph1W2) 
								PagesPerAvgRecord = 9;
							else PagesPerAvgRecord = 3;
						}

						ushort AvgBegin = devInfo_.Content[iRec].AvgPagesNAND[0];
						ushort AvgEnd = devInfo_.Content[iRec].AvgPagesNAND[1];//(ushort)
						//(devInfo_.Content[iRec].AvgPagesNAND[1] + (PagesPerAvgRecord - 1));

						// defining NAND memory ring marker
						if (iRec == 0) iAvgRingMarker = AvgEnd;

						// here we are place addresses of the pages with avg data
						List<ushort> listAvgPagesNAND = new List<ushort>();
						int iAvgAddress = AvgEnd;	// we are starting from the end
						long AvgLeft = devInfo_.Content[iRec].AvgNum * PagesPerAvgRecord;
						long AvgDone = 0;

						while (true)
						{
							if (AvgLeft <= 0)
								break;								// successfull operation end
							if (iAvgAddress == iAvgRingMarker && AvgDone != 0)
							{
								bNeedAVG = false;
								break;								// "marker reached" operation end
							}

							listAvgPagesNAND.Insert(0, (ushort)iAvgAddress);	//adding page to the address array
							AvgLeft--;			// decremect number of left pages to add
							iAvgAddress--;		// decremect address value
							AvgDone++;
						}

						// number of pages to delete in the begin of pages addresses array
						//int iTailToDel = 0;
						//if (bNeedAVG == false)
						//{
						//   // address alignment for the next block.
						//   // It's needed because only the next block after the page with 
						//iNandRingMarker addess 
						//   // can be assumed as correct (because writing of page in the new block erase
						//   // all pages in this block after this)
						//   iTailToDel = DP_EM31K.pages_per_block - listAvgPagesNAND[0] % 
						//DP_EM31K.pages_per_block;
						//   listAvgPagesNAND.RemoveRange(0, iTailToDel);
						//}
						//// address alignment for the number of pages in the data record
						//iTailToDel = listAvgPagesNAND.Count % PagesPerAvgRecord;
						//listAvgPagesNAND.RemoveRange(0, iTailToDel);

						// confirmation of existing avg
						// and reading start and end times of measures
						if (listAvgPagesNAND.Count > 0)
						{
							// filling page's address array
							devInfo_.Content[iRec].AvgPagesNAND = new ushort[listAvgPagesNAND.Count];
							listAvgPagesNAND.CopyTo(devInfo_.Content[iRec].AvgPagesNAND);

							// reading date of begin and end of the measurement
							AddressMemory addr = new AddressMemory();
							addr.NAND.Flash = 0;
							addr.NAND.Exists = true;
							// ...begin
							addr.NAND.Page = devInfo_.Content[iRec].AvgPagesNAND[0];
							res = Read(EMemory.NAND, addr, 1, ref buffer, false);
							if (res != ExchangeResult.OK) return res;

							devInfo_.Content[iRec].AvgBegin =
							Conversions.bytes_2_DateTime(ref buffer, 0);
							// ...end
							addr.NAND.Page = devInfo_.Content[iRec].AvgPagesNAND[
								devInfo_.Content[iRec].AvgPagesNAND.Length - PagesPerAvgRecord];
							res = Read(EMemory.NAND, addr, 1, ref buffer, false);
							if (res != ExchangeResult.OK) return res;

							devInfo_.Content[iRec].AvgEnd =
								Conversions.bytes_2_DateTime(ref buffer, 0);

							// changing number of avg records to actual
							devInfo_.Content[iRec].AvgNum = (uint)(devInfo_.Content[iRec].AvgPagesNAND.Length / PagesPerAvgRecord);

							// marking that all ok!
							devInfo_.Content[iRec].AvgExists = true;
							ValidContentRecords[iRec] = true;
						}
					}

					#endregion

					#region events

					if (bNeedDNO && devInfo_.Content[iRec].DnsNum > 0)
					{
						ushort EventsEnd = devInfo_.Content[iRec].DnsPagesNAND[1];
						if (iRec == 0) iEventsRingMarker = EventsEnd;

						int EventsLeft = (int)Math.Ceiling((double)devInfo_.Content[iRec].DnsNum / (double)events_records_per_page);

						// list of events pages addresses
						List<ushort> listEventsPages = new List<ushort>();

						// current (each in the cycle) page address
						ushort iEventAddress = EventsEnd;

						// number of records in the current (each in the cycle) page
						int iEventNumber = (int)(devInfo_.Content[iRec].DnsNum % events_records_per_page);
						//if(iEventNumber == 0)  //mycode
						//iEventNumber = DP_EM31K.events_records_per_page;  //mycode

						while (true)
						{
							if (EventsLeft <= 0)
								break;								// successfull operation end

							listEventsPages.Insert(0, (ushort)iEventAddress);	// adding page to the address array
							iDNORecords += iEventNumber;	// increment number of events records
							EventsLeft--;					// decremect number of left pages to add
							iEventAddress--;				// decremect address value

							if (iEventNumber != events_records_per_page)
								iEventNumber = events_records_per_page;
						}

						//if (bNeedDNO == false)
						//{
						//   // address alignment for the next block.
						//   // It's needed because only the next block after the page with iNandRingMarker addess 
						//   // can be assumed as correct (because writing of page in the new block erase all the
						//   // pages in this block after this)
						//   int iTailToDel = DP_EM31K.pages_per_block - listEventsPages[0] % DP_EM31K.pages_per_block;
						//   listEventsPages.RemoveRange(0, iTailToDel);
						//}

						// filling page's address array
						devInfo_.Content[iRec].DnsPagesNAND = new ushort[listEventsPages.Count];
						listEventsPages.CopyTo(devInfo_.Content[iRec].DnsPagesNAND);

						// date of begin of the measurement
						devInfo_.Content[iRec].DnsStart = devInfo_.Content[iRec].CommonBegin;
						// date of end of the measurement
						devInfo_.Content[iRec].DnsEnd = devInfo_.Content[iRec].CommonEnd;
						// other info
						devInfo_.Content[iRec].DnsNum = (uint)iDNORecords;
						devInfo_.Content[iRec].DnsExists = true;
						ValidContentRecords[iRec] = true;
					}
					#endregion
				}

				#endregion

				#region Forming real content

				List<ContentsLineEm33> list_real_content = new List<ContentsLineEm33>();

				for (int iLine = 0; iLine < ValidContentRecords.Length; iLine++)
				{
					if (ValidContentRecords[iLine])
						list_real_content.Add(devInfo_.Content[iLine]);
				}

				devInfo_.Content = new ContentsLineEm33[list_real_content.Count];
				list_real_content.CopyTo(devInfo_.Content);

				#endregion
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return ExchangeResult.Other_Error;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in Em31KDevice::ReadDeviceInfo: " + ex.Message);
				EmService.WriteToLogFailed(ex.ToString());
				throw;
			}

			return 0;
		}

		public ExchangeResult Read(EMemory memory_type,
						AddressMemory address,
						ushort size,
						ref byte[] buffer,
						bool check_crc_mode_off)
		{
			int num = 0;
			ExchangeResult res = 0;
			while ((res = exchange_(
				EOperation.Read,
				memory_type,
				address,
				size,
				ref buffer,
				check_crc_mode_off)) != 0 &&
				++num <= 3)	// 3 попытки чтения
			{
				EmService.WriteToLogFailed(String.Format("Reading Error. Attempt {0}", num));

				if (bCancelReading_)
					break;
			}
			return res;
		}

		// устройство не поддерживает запись
		//public int Write()
		//{
		//    return -1;
		//}

		#endregion

		#region Private Methods

		protected ExchangeResult exchange_(
			EOperation oper,
			EMemory memory_type,
			AddressMemory address,
			ushort size,
			ref byte[] buffer,
			bool check_crc_mode_off)
		{
			ushort crc = 0;					// crc16

			///////////////////////////////////////////////////////////////////
			// define mode, size in bytes and address
			byte mode = 0x00;
			ushort size_in_bytes = size;

			switch (oper)
			{
				case EOperation.Read:
					switch (memory_type)
					{
						case DeviceIO.Memory.EMemory.RAM: mode = 0x00;
							break;
						case DeviceIO.Memory.EMemory.FRAM: mode = 0x00;
							break;
						case DeviceIO.Memory.EMemory.NAND:
							mode = 0x0b;
							size_in_bytes = (ushort)(size_in_bytes * bytes_per_page);
							break;
						default: return ExchangeResult.Other_Error;
					}
					break;
				case EOperation.Write:
					switch (memory_type)
					{
						case DeviceIO.Memory.EMemory.RAM: mode = 0x00;
							break;
						case DeviceIO.Memory.EMemory.FRAM: mode = 0x00;
							break;
						case DeviceIO.Memory.EMemory.NAND: mode = 0x00;
							size_in_bytes = (ushort)(size_in_bytes * bytes_per_page);
							break;
						default: return ExchangeResult.Other_Error;
					}
					break;
				default: return ExchangeResult.Other_Error;
			}

			///////////////////////////////////////////////////////////////////
			// sending query
			List<byte> query_list = new List<byte>();
			query_list.Add(synchro_byte);						// sychro-byte
			query_list.Add(mode);								// query mode
			query_list.Add(0x00);								// place for number of bytes 
			query_list.Add(0x00);								// of the next data	

			if (memory_type == DeviceIO.Memory.EMemory.NAND)
			{
				if (!address.NAND.Exists)
					throw new EmException("Not implemented needed address type: " + memory_type.ToString());

				query_list.Add((byte)(address.NAND.Flash % 0x100));	// converting to byte[]
				query_list.Add((byte)(address.NAND.Flash / 0x100));	// Flash address
				query_list.Add((byte)(address.NAND.Page % 0x100));	// converting to byte[]
				query_list.Add((byte)(address.NAND.Page / 0x100));	// Page address
			}
			else return ExchangeResult.Other_Error;

			query_list.Add((byte)(size % 0x100));				// converting to byte[]
			query_list.Add((byte)(size / 0x100));				// data size

			byte[] query_buffer = null;							// prepare byte[] buffer
			if (oper == EOperation.Write)
			{
				query_buffer = new byte[query_list.Count + 2 + buffer.Length];
				buffer.CopyTo(query_buffer, query_list.Count);
			}
			else if (oper == EOperation.Read)
			{
				query_buffer = new byte[query_list.Count + 2];
			}

			query_list.CopyTo(query_buffer);

			query_buffer[2] = (byte)((query_buffer.Length - 6) % 0x100);	// calculating number
			query_buffer[3] = (byte)((query_buffer.Length - 6) / 0x100);	// of message useful bytes

			crc = 0;
			crc = calcCRC(query_buffer, (ushort)(query_buffer.Length - 2), 0, false);	// calculating crc
			query_buffer[query_buffer.Length - 2] = (byte)(crc % 0x100);	// converting
			query_buffer[query_buffer.Length - 1] = (byte)(crc / 0x100);	// word to byte[]

			if (portManager_.WriteToPort((UInt32)query_buffer.Length, ref query_buffer) != 0)
			{
				return ExchangeResult.Write_Error;							// error exit
			}

			///////////////////////////////////////////////////////////////////
			// receiving answer
			crc = 0;

			byte[] word_buffer = new byte[2];

			// trying to read two bytes. if nothing, exit...
			if (portManager_.ReadFromPort(2, ref word_buffer) != 0)
			{
				if (portManager_.ReadFromPort(2, ref word_buffer) != 0)
				{
					EmService.WriteToLogFailed("exchange_: error reading first two bytes!");
					return ExchangeResult.Read_Error;					// error exit
				}
			}
			// calculation of crc for this bytes
			crc = calcCRC(word_buffer, 2, crc, true);

			// checking Synchro-byte
			if (/*word_buffer[1] != normal_byte ||*/ word_buffer[0] != synchro_byte)
			{
				EmService.WriteToLogFailed("exchange_: synchro_byte is invalid! " +
					word_buffer[1].ToString() + "  " + word_buffer[0].ToString());
				return ExchangeResult.SynchroByte_Error;				// error exit
			}
			byte normal_byte = word_buffer[1];

			// reading other bytes count (word value)
			if (portManager_.ReadFromPort(2, ref word_buffer) != 0)
			{
				if (portManager_.ReadFromPort(2, ref word_buffer) != 0)
				{
					EmService.WriteToLogFailed("exchange_: error reading other bytes count!");
					return ExchangeResult.Read_Error;					// error exit
				}
			}

			// calculation of crc for this bytes too
			crc = calcCRC(word_buffer, 2, crc, true);
			// converting two bytes value into a word value
			ushort bytes_of_useful_data = (ushort)(word_buffer[1] * 0x100 + word_buffer[0]);

			// reading only data(!) crc still in the port buffer
			byte[] answer_buffer = new byte[bytes_of_useful_data];
			if (bytes_of_useful_data > 0)
			{
				if (portManager_.ReadFromPort(bytes_of_useful_data, ref answer_buffer) != 0)
				{
					if (portManager_.ReadFromPort(bytes_of_useful_data, ref answer_buffer) != 0)
					{
						EmService.WriteToLogFailed("exchange_: error reading only data!");
						return ExchangeResult.Read_Error;					// error exit
					}
				}
			}
			// calculating crc for useful data and closing crc
			crc = calcCRC(answer_buffer, bytes_of_useful_data, crc, false);

			if (oper == EOperation.Read) buffer = answer_buffer;

			// reading crc from the device (word value)
			if (portManager_.ReadFromPort(2, ref word_buffer) != 0)
			{
				if (portManager_.ReadFromPort(2, ref word_buffer) != 0)
				{
					EmService.WriteToLogFailed("exchange_: error reading crc!");
					return ExchangeResult.Read_Error;					// error exit
				}
			}

			if (normal_byte != 0)
			{
				EmService.WriteToLogFailed("exchange_: normal_byte error! normal_byte = " + normal_byte);
				return ExchangeResult.NormalByte_Error;
			}

			// converting two bytes value into a word value
			ushort received_crc = (ushort)(word_buffer[1] * 0x100 + word_buffer[0]);

			// if my calculated crc and readed crc not equals, exit
			if (received_crc != crc)
			{
				EmService.WriteToLogFailed("exchange_: crc error! received_crc = " + received_crc);
				return ExchangeResult.CRC_Error;						// error exit
			}

			return ExchangeResult.OK;
		}

		protected static ushort calcCRC(byte[] data, ushort size, ushort start_crc, bool b_keep_open)
		{
			// мои изменения - начинаем не с нуля а со скольки нужно...
			// просто буду считать контрольную сумму "по частям"
			ushort currentCRC = start_crc;

			for (int i = 0; i < size; i++)
				currentCRC = updateCRC((ushort)data[i], currentCRC);

			// чтобы дать возможность считать по частям - нужно выйти тут с тем, что есть )
			if (b_keep_open) return currentCRC;

			currentCRC = updateCRC((ushort)0, currentCRC);
			currentCRC = updateCRC((ushort)0, currentCRC);
			currentCRC = (ushort)(((currentCRC >> 8) + (currentCRC << 8)) % 0x10000);

			return currentCRC;
		}

		/// <summary>
		/// Вспомогательная функция для подсчета контрольной суммы
		/// </summary>
		/// <param name="dataByte">Добавляемые байты к тем, у которых CRC уже посчитана</param>
		/// <param name="CRC">CRC предыдущих байт</param>
		/// <returns>Новая CRC</returns>
		protected static ushort updateCRC(ushort dataByte, ushort CRC)
		{
			for (int i = 0; i < 8; i++)
			{
				bool bPower = Convert.ToBoolean(CRC & 0x8000);
				CRC <<= 1;
				CRC += (ushort)((((dataByte <<= 1) & 0x100) != 0) ? 1 : 0);

				if (bPower) CRC ^= 0x1021;
			}
			return CRC;
		}

		#endregion
	}
}
