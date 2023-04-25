using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using DeviceIO.Memory;
using EmServiceLib;

namespace DeviceIO
{
	public class Em33TDevice : EmDevice
	{
		#region Constants and enums

		protected enum EOperation
		{
			Read,
			Write
		}

		protected const int iDelatAfterPortReading = 100; //ms

		public const int bytes_per_page = 512;
		internal const int pages_per_block = 32;
		internal const int pages_per_flash = 65536;
		internal const byte normal_byte = 0x00;
		internal const byte synchro_byte = 0x02;

		internal const int events_bytes_per_record = 16;
		internal const int events_records_per_page = bytes_per_page / events_bytes_per_record;
		internal const int events_common_number_of_pages = 2880;

		static bool IsExchangeFirstInvoke;

		#endregion

		#region Fields

		DeviceCommonInfoEm33 devInfo_;

		protected List<ushort> error_pages_ = new List<ushort>(); 

		#endregion

		#region Properties

		public DeviceCommonInfoEm33 DeviceInfo
		{
			get { return this.devInfo_; }
		}

		// В этом массиве хранятся номера страниц, при чтении которых произошла ошибка (обычно ошибка CRC)
		public List<ushort> ErrorPages
		{
			get { return error_pages_; }
			set { error_pages_ = value; }
		}

		#endregion

		#region Constructors

		public Em33TDevice(EmPortType portType, ushort devAddr, bool auto, 
			object[] port_params, IntPtr hMainWnd)
			: base(EmDeviceType.EM33T, portType, devAddr, auto, port_params, hMainWnd)
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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in Open Em33T device: ");
				return -1;
			}
		}

		public override bool IsSomeArchiveExist()
		{
			return (devInfo_.Content != null && devInfo_.Content.Length > 0);
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
				EM33TPSystem pageSystem = new EM33TPSystem();

				if (Read(DeviceIO.Memory.EMemory.FRAM,
					pageSystem.Address,
					pageSystem.Size,
					ref buffer, false) != 0) return ExchangeResult.Other_Error;
				if (!pageSystem.Parse(ref buffer)) return ExchangeResult.Other_Error;

				EmService.WriteToLogGeneral("system query 1 success");

				//#if DEBUG
				//WriteDebug(@"c:\em33.PSystem.xml", pageSystem, typeof(EM33TPSystem));
				//#endif

				devInfo_.SerialNumber = (ushort)pageSystem.Data["DeviceNumber"];
				// 1 – Эм 3.1К, 3 – Эм 3.3Т и Эм 3.3Т1
				devInfo_.InternalType = (ushort)pageSystem.Data["DeviceType"]; 
				devInfo_.Name = (string)pageSystem.Data["DeviceName"];
				if (devInfo_.Name.Contains("T1"))
					devInfo_.DeviceType = EmDeviceType.EM33T1;
				else
					devInfo_.DeviceType = EmDeviceType.EM33T;
				devInfo_.Version = pageSystem.Data["Version"].ToString();

				// devInfo.Version:
				// "7.1.47"  -> "7.01.47"
				// "7.10.47" -> "7.10.47"
				int iIndexPoint1 = devInfo_.Version.IndexOf('.', 0);
				int iIndexPoint2 = devInfo_.Version.IndexOf('.', iIndexPoint1 + 1);
				if ((iIndexPoint2 - iIndexPoint1) == 2)
				{
					devInfo_.Version = devInfo_.Version.Insert(iIndexPoint1 + 1, "0");
				}

				devInfo_.Version = devInfo_.Version.Insert(
					devInfo_.Version.LastIndexOf('.') + 1,
					pageSystem.Data["Version_f0"].ToString());

				// reading archive objects page... if not successfully, return -1
				EM33TPArchObjects pageArchObjects = new EM33TPArchObjects();
				if (Read(DeviceIO.Memory.EMemory.FRAM,
					pageArchObjects.Address,
					pageArchObjects.Size,
					ref buffer, false) != 0) return ExchangeResult.Other_Error;
				if (!pageArchObjects.Parse(ref buffer)) return ExchangeResult.Other_Error;

				EmService.WriteToLogGeneral("system query 2 success");

				int mainRecordsCount = (ushort)pageArchObjects.Data["MainRecordsCount"];
				// if main records map is cleare, return -2
				if (mainRecordsCount == 0) throw new EmDeviceEmptyException();

				// reading bad blocks map... if not successfully, return -1
				EM33TPBadBlocks pageBadBlocks = new EM33TPBadBlocks();
				if (Read(DeviceIO.Memory.EMemory.FRAM,
					pageBadBlocks.Address,
					pageBadBlocks.Size,
					ref buffer, false) != 0) return ExchangeResult.Other_Error;
				if (!pageBadBlocks.Parse(ref buffer)) return ExchangeResult.Other_Error;
				//#if DEBUG
				//WriteDebug(@"c:\em33.PBadBlocks.xml", pageBadBlocks, typeof(EM33TPBadBlocks));
				//#endif
				EmService.WriteToLogGeneral("system query 3 success");

				// reading top pointers... if not successfully, return -1
				EM33TPTopPtrs pageTopPtrs = new EM33TPTopPtrs();
				if (Read(DeviceIO.Memory.EMemory.FRAM,
					pageTopPtrs.Address,
					pageTopPtrs.Size,
					ref buffer, false) != 0) return ExchangeResult.Other_Error;
				if (!pageTopPtrs.Parse(ref buffer)) return ExchangeResult.Other_Error;
				//#if DEBUG
				//WriteDebug(@"c:\em33.PTopPtrs.xml", pageTopPtrs, typeof(EM33TPTopPtrs));
				//#endif
				EmService.WriteToLogGeneral("system query 4 success");

				#endregion

				#region Reading Main Records info from the device

				List<ContentsLineEm33> listMainRecords = new List<ContentsLineEm33>();
				EmService.WriteToLogGeneral("mainRecordsCount = " + mainRecordsCount.ToString());
				// filling in main records with
				for (int i = 0; i < mainRecordsCount; i++)
				{
					try
					{
						ContentsLineEm33 tempMainRecord = new ContentsLineEm33();
						int iNextIndex = (ushort)pageArchObjects.Data["CurrentMainRecIndex"] - i;
						if (iNextIndex < 0) iNextIndex += 32;

						EM33TPMainRecord pageMainRecord = new EM33TPMainRecord();
						pageMainRecord.Index = iNextIndex;
						if (Read(EMemory.FRAM,
							pageMainRecord.Address,
							pageMainRecord.Size,
							ref buffer, false) != 0)
							continue; //return -1;
						if (!pageMainRecord.Parse(ref buffer)) continue; //return -1;

						tempMainRecord.ObjectName = (string)pageMainRecord.Data["ObjName"];
						tempMainRecord.ConnectionScheme =
							(ConnectScheme)(ushort)pageMainRecord.Data["ConSch"];
						tempMainRecord.F_Nominal = (ushort)pageMainRecord.Data["F_Nom"];
						tempMainRecord.U_NominalLinear = (float)pageMainRecord.Data["U_NomLn"];
						tempMainRecord.U_NominalPhase = (float)pageMainRecord.Data["U_NomPh"];
						tempMainRecord.CommonBegin = (DateTime)pageMainRecord.Data["StartDateTime"];
						tempMainRecord.CommonEnd = (DateTime)pageMainRecord.Data["EndDateTime"];
						tempMainRecord.U_Limit = (float)pageMainRecord.Data["Ulimit"];
						tempMainRecord.I_Limit = (float)pageMainRecord.Data["Ilimit"];
						tempMainRecord.CurrentTransducerIndex =
							(ushort)pageMainRecord.Data["CurrentTransducerIndex"];

						tempMainRecord.AvgExists = true;
						tempMainRecord.AvgTime = (ushort)pageMainRecord.Data["TimeOfAveragingOut"];
						if (tempMainRecord.AvgTime == 3) continue; // осциллограмма

						tempMainRecord.AvgNum = (uint)pageMainRecord.Data["NumOfAvgRecords"];
						tempMainRecord.AvgPagesNAND = new ushort[2] {
						(ushort) pageMainRecord.Data["AddrAVGBegin"],
						(ushort) pageMainRecord.Data["AddrAVGEnd"] };

						tempMainRecord.DnsExists = true;
						tempMainRecord.DnsNum = (uint)pageMainRecord.Data["NumOfDnoRecords"];
						tempMainRecord.DnsPagesNAND = new ushort[2] {
						(ushort) pageMainRecord.Data["AddrDNOBegin"],
						(ushort) pageMainRecord.Data["AddrDNOEnd"] };

						tempMainRecord.PqpSet =
							new PqpSetEm33[((ushort[])pageMainRecord.Data["AddrArchPQP"]).Length];
						for (int pqp_i = 0; pqp_i < tempMainRecord.PqpSet.Length; pqp_i++)
						{
							tempMainRecord.PqpSet[pqp_i].PqpPageNAND =
								((ushort[])pageMainRecord.Data["AddrArchPQP"])[pqp_i];
							tempMainRecord.PqpSet[pqp_i].UnfPagesNAND = new ushort[1] {
							((ushort[])pageMainRecord.Data["AddrArchUnF"])[pqp_i]};
							tempMainRecord.PqpSet[pqp_i].UnfRecords =
								((int[])pageMainRecord.Data["NumOfUnfRecords"])[pqp_i];
						}
						tempMainRecord.DnsTimer = (uint)pageMainRecord.Data["dtTimer"];
						// fliker
						bool flikkerExists = false;
						if (devInfo_.DeviceType == EmDeviceType.EM33T1)
							flikkerExists = true;
						else if (devInfo_.DeviceType == EmDeviceType.EM33T)
							if (Constants.isNewDeviceVersion_EM33T(devInfo_.Version))
								flikkerExists = true;
						if (flikkerExists)
							tempMainRecord.t_fliker = (short)pageMainRecord.Data["t_fliker"];

						listMainRecords.Add(tempMainRecord);
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "Bad Main Record: " + i.ToString());
					}
				}

				if (listMainRecords.Count < 1) throw new EmDeviceEmptyException();

				devInfo_.Content = new ContentsLineEm33[listMainRecords.Count];
				listMainRecords.CopyTo(devInfo_.Content);

				#endregion

				#region D E V I C E  E M U L A T I O N

				///////////////////////////////////////////////////
				///////////////////////////////////////////////////
				////                                           ////
				////      D E V I C E  E M U L A T I O N       ////
				////                                           ////
				///////////////////////////////////////////////////
				///////////////////////////////////////////////////

				//devInfo = new DeviceCommonInfo();
				//System.Xml.Serialization.XmlSerializer mySerializer = null;
				//System.IO.FileStream myFileStream = null;
				//try
				//{
				//   // Create an XmlSerializer for the ApplicationSettings type.
				//   mySerializer = new System.Xml.Serialization.XmlSerializer(typeof(DeviceIO.DeviceCommonInfo));
				//   System.IO.FileInfo fi = new System.IO.FileInfo(@"X:\PQP\test 00\em33.DeviceCommonInfo_DIRTY.xml");
				//   // If the config file exists, open it.
				//   if (fi.Exists)
				//   {
				//      myFileStream = fi.OpenRead();
				//      // Create a new instance of the ApplicationSettings by
				//      // deserializing the config file.
				//      devInfo = (DeviceIO.DeviceCommonInfo)mySerializer.Deserialize(myFileStream);
				//      // Assign the property values to this instance of 
				//      // the ApplicationSettings class.
				//   }
				//}
				//catch (Exception ex)
				//{
				//   EmService.WriteToLogFailed(ex.Message);
				//}
				//finally
				//{
				//   // If the FileStream is open, close it.
				//   if (myFileStream != null)
				//   {
				//      myFileStream.Close();
				//   }
				//}

				//pageTopPtrs = new PTopPtrs();
				//pageTopPtrs.Data = new Dictionary<string, object>();
				//pageTopPtrs.Data.Add("ArchPKE", new ushort[8] { 864, 896, 928, 960, 992, 1024, 1056, 1088 });

				/////////////////////////////////////////////////////////////
				/////////////////////////////////////////////////////////////
				////                                                     ////
				////   E N D  O F   D E V I C E  E M U L A T I O N       ////
				////                                                     ////
				/////////////////////////////////////////////////////////////
				/////////////////////////////////////////////////////////////

				#endregion

				#region Cheking main records, correcting data, reading times of start/end

				// number of 512-bytes memory pages for avg values
				//int maxOfAvgPages = DP_EM33T.pages_per_flash - (ushort)pageTopPtrs.Data["ArchAVG"];
				// maximum number of events records
				//int maxOfDnoRecords = DP_EM33T.events_common_number_of_pages * DP_EM33T.events_records_per_page;

				// FLAGS to continue trying to find
				bool bNeedAVG = true;	// avg
				bool bNeedPKE = true;	// pqp
				bool bNeedDNO = true;	// events

				int iDNORecords = 0;	// number of scaned events records
				int iPKERecords = 0;	// number of readed pqp
				int iNumOfPKE = 0;		// number of realy existing pqp records for each archive

				int iAvgRingMarker = 0; // last page address of the first avg record
				int iEventsRingMarker = 0; // last page address of the first events record

				PqpZonesEm33[] PqpValidZones = new PqpZonesEm33[8];
				for (int a = 0; a < PqpValidZones.Length; a++)
				{
					PqpValidZones[a].sPage = (pageTopPtrs.Data["ArchPQP"] as ushort[])[a];
					PqpValidZones[a].ePage = (ushort)(PqpValidZones[a].sPage + 31);
					PqpValidZones[a].Valid = true;
				}

				// array with bool values to indicate is this content record is correct or not
				bool[] ValidContentRecords = new bool[devInfo_.Content.Length];

				for (int iRec = 0; iRec < devInfo_.Content.Length; iRec++)
				{
					// FLAGS to indicate that record are exists
					devInfo_.Content[iRec].AvgExists = false;		// avg
					devInfo_.Content[iRec].PqpExists = false;		// pqp
					devInfo_.Content[iRec].DnsExists = false;		// events

					// common verification of content record
					if (devInfo_.Content[iRec].CommonEnd == devInfo_.Content[iRec].CommonBegin ||
						devInfo_.Content[iRec].CommonEnd == DateTime.MinValue)
						continue;

					#region avg

					if (bNeedAVG && devInfo_.Content[iRec].AvgPagesNAND[1] != 0)
					{
						if (devInfo_.Content[iRec].AvgTime > 2)
						{
							bNeedAVG = false;	// nothing
							bNeedPKE = false;	// can't exists
							bNeedDNO = false;	// after adc archive
						}

						// number or pages per avg record
						// for avg = 5
						int PagesPerAvgRecord = 0;
						if (devInfo_.Content[iRec].AvgTime < 3) PagesPerAvgRecord = 5;
						else
						{
							if (devInfo_.Content[iRec].ConnectionScheme != ConnectScheme.Ph1W2) 
								PagesPerAvgRecord = 18;
							else PagesPerAvgRecord = 6;
						}

						ushort AvgBegin = devInfo_.Content[iRec].AvgPagesNAND[0];

						// this value can be not correct in case of the record will be splitted
						// by one or more number of bad blocks. below we'd correct this case.
						ushort AvgEnd = (ushort)
							(devInfo_.Content[iRec].AvgPagesNAND[1] +
							(PagesPerAvgRecord - 1));
						// if the last page of the last record is in the another block that the first
						// we must be test it with bad-block test
						if ((Math.Truncate((decimal)AvgEnd / pages_per_block)) !=
							Math.Truncate((decimal)(AvgEnd + PagesPerAvgRecord - 1) / pages_per_block))
						{
							while (true)
							{
								ushort cur_block = (ushort)(AvgEnd / pages_per_block);
								ushort[] badBlocks = (ushort[])pageBadBlocks.Data["BadBlocks"];
								bool isBad = false;
								for (int iBlock = 0; iBlock < badBlocks.Length; iBlock++)
								{
									if (badBlocks[iBlock] / pages_per_block == cur_block)
									{
										isBad = true;
										break;
									}
								}
								if (isBad) AvgEnd += pages_per_block;
								else break;
							}
						}

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

							// each page that complete a block must be tested
							// with the bad-block-test
							if (iAvgAddress % pages_per_block == pages_per_block - 1)
							{
								ushort cur_block = (ushort)(iAvgAddress / pages_per_block);
								ushort[] badBlocks = (ushort[])pageBadBlocks.Data["BadBlocks"];
								bool isBad = false;
								for (int iBlock = 0; iBlock < badBlocks.Length; iBlock++)
								{
									if (badBlocks[iBlock] / pages_per_block == cur_block)
									{
										isBad = true;
										break;
									}
								}
								if (isBad)
								{
									iAvgAddress -= pages_per_block;
									continue;
								}
							}

							listAvgPagesNAND.Insert(0, (ushort)iAvgAddress);	//adding page to the address array
							AvgLeft--;			// decremect number of left pages to add
							iAvgAddress--;		// decremect address value
							if (iAvgAddress < (ushort)pageTopPtrs.Data["ArchAVG"])
								iAvgAddress = pages_per_flash - 1;	// correcting address value for ring-like type
							AvgDone++;
						}

						// number of pages to delete in the begin of pages addresses array
						int iTailToDel = 0;
						if (bNeedAVG == false)
						{
							// address alignment for the next block.
							// It's needed because only the next block after the page with iNandRingMarker addess 
							// can be assumed as correct (because writing of page in the new block erase all the
							// pages in this block after this)
							iTailToDel = pages_per_block - listAvgPagesNAND[0] % pages_per_block;
							listAvgPagesNAND.RemoveRange(0, iTailToDel);
						}
						// address alignment for the number of pages in the data record
						iTailToDel = listAvgPagesNAND.Count % PagesPerAvgRecord;
						listAvgPagesNAND.RemoveRange(0, iTailToDel);

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

							ExchangeResult res = Read(EMemory.NAND, addr, 1, ref buffer, false);
							if (res == ExchangeResult.NormalByte_Error || res == ExchangeResult.CRC_Error)
							{
								EmService.WriteToLogFailed("CRC Error in NAND, page " + addr.NAND.Page);
								error_pages_.Add(addr.NAND.Page);
								if (Read(EMemory.NAND, addr, 1, ref buffer, true) != ExchangeResult.OK)
								{
									EmService.WriteToLogFailed("Without CRC check - error too!");
									return res;
								}
							}
							else if (res != ExchangeResult.OK) return res;

							devInfo_.Content[iRec].AvgBegin =
							Conversions.bytes_2_DateTime(ref buffer, 0);
							// ...end
							addr.NAND.Page = devInfo_.Content[iRec].AvgPagesNAND[
								devInfo_.Content[iRec].AvgPagesNAND.Length - PagesPerAvgRecord];

							buffer = null;
							res = Read(EMemory.NAND, addr, 1, ref buffer, false);
							if (res == ExchangeResult.NormalByte_Error || res == ExchangeResult.CRC_Error)
							{
								EmService.WriteToLogFailed("CRC Error in NAND, page " + addr.NAND.Page);
								error_pages_.Add(addr.NAND.Page);
								if (Read(EMemory.NAND, addr, 1, ref buffer, true) != ExchangeResult.OK)
								{
									EmService.WriteToLogFailed("Without CRC check - error too!");
									return res;
								}
							}
							else if (res != ExchangeResult.OK) return res;

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

					#region pke

					if (bNeedPKE)
					{
						// calculating number of registered pqp records
						for (iNumOfPKE = 0; iNumOfPKE < devInfo_.Content[iRec].PqpSet.Length; iNumOfPKE++)
						{
							if (devInfo_.Content[iRec].PqpSet[iNumOfPKE].PqpPageNAND == 0) break;
						}
						// trying to recognize number of really existing pqp records
						// common number of pqp pages in all archives must be less or equal to eight

						if (iPKERecords + iNumOfPKE >= 8)
							bNeedPKE = false;

						// if number of pqp records more then 8 we need to use only first
						if (iPKERecords + iNumOfPKE > 8)
							iNumOfPKE = 8 - iPKERecords; //8 - iPKERecords - это оставшиеся свободные места,
						// т.е. максимально возможное число архивов
						//iPKERecords += iNumOfPKE;
						//if (iPKERecords == 8) bNeedPKE = false;

						// creating temporary array with correct addresses of pqp set
						PqpSetEm33[] pqpSet = null;
						if (iNumOfPKE > 0)
						{
							pqpSet = new PqpSetEm33[iNumOfPKE];

							for (int i = 0; i < iNumOfPKE; i++)
							{
								// defining current zone
								ushort currentZone = 255;
								ushort PqpPage = devInfo_.Content[iRec].PqpSet[i].PqpPageNAND;
								for (int pqpz = 0; pqpz < PqpValidZones.Length; pqpz++)
								{
									if (PqpPage >= PqpValidZones[pqpz].sPage &&
										PqpPage <= PqpValidZones[pqpz].ePage)
									{
										currentZone = (ushort)pqpz;
										break;
									}
								}

								if (!PqpValidZones[currentZone].Valid) continue;

								#region DEBUG

								//System.IO.StreamWriter sw = null;
								//try
								//{
								//    sw = new System.IO.StreamWriter(String.Format(@"c:\em33.pqp.mr{0}.arch{1}.txt", iRec, i));
								//    sw.WriteLine(String.Format("Главная запись {0}; Архив ПКЭ {1}", iRec, i));
								//    sw.WriteLine(String.Format("Текущий адрес: 0x{0:X}h", PqpPage));
								//    sw.WriteLine(String.Format("Данная запись принадлежит зоне {0}, ", currentZone));

								//    sw.WriteLine(String.Empty);
								//    sw.WriteLine("Адреса зон: ");

								//    for (int pqpz = 0; pqpz < PqpValidZones.Length; pqpz++)
								//    {
								//        sw.WriteLine(
								//            String.Format("Зона {0}:\t0x{1:X}h..0x{2:X}h\tValid = {3}", pqpz,
								//            PqpValidZones[pqpz].sPage, PqpValidZones[pqpz].ePage, PqpValidZones[pqpz].Valid));
								//    }
								//}
								//catch (Exception ex)
								//{
								//    sw.WriteLine(ex.Message);
								//}
								//finally
								//{
								//    sw.Close();
								//}
								#endregion

								PqpValidZones[currentZone].Valid = false;
								pqpSet[i].PqpZone = currentZone;
								++iPKERecords;   // увеличиваем счетчик обработанных архивов

								devInfo_.Content[iRec].PqpExists = true;
								ValidContentRecords[iRec] = true;

								//continue;

								buffer = null;

								AddressMemory nandAddr = new AddressMemory();
								nandAddr.NAND.Flash = 0;
								nandAddr.NAND.Page = devInfo_.Content[iRec].PqpSet[i].PqpPageNAND;
								nandAddr.NAND.Exists = true;

								ExchangeResult res = Read(EMemory.NAND, nandAddr, 8, ref buffer, false);
								if (res == ExchangeResult.NormalByte_Error || res == ExchangeResult.CRC_Error)
								{
									EmService.WriteToLogFailed("CRC Error in NAND, page " + nandAddr.NAND.Page);
									error_pages_.Add(nandAddr.NAND.Page);
									if (Read(EMemory.NAND, nandAddr, 8, ref buffer, true) != ExchangeResult.OK)
									{
										EmService.WriteToLogFailed("Without CRC check - error too!");
										return res;
									}
								}
								else if (res != ExchangeResult.OK) return res;

								pqpSet[i].PqpPageNAND = devInfo_.Content[iRec].PqpSet[i].PqpPageNAND;
								pqpSet[i].PqpStart = Conversions.bytes_2_DateTime(ref buffer, /*304*/432);
								pqpSet[i].PqpEnd = Conversions.bytes_2_DateTime(ref buffer, /*312*/440);
								pqpSet[i].UnfRecords = devInfo_.Content[iRec].PqpSet[i].UnfRecords;

								//// получаем в pqpSet[i].PqpZone номер зоны ПКЭ за i-ые сутки данного архива
								//for (int tpi = 0; tpi < 8; tpi++)
								//{
								//   if (pqpSet[i].PqpPageNAND - (pqpSet[i].PqpPageNAND % 0x20) ==
								//(pageTopPtrs.Data["ArchPQP"] as ushort[])[tpi])
								//      pqpSet[i].PqpZone = tpi;
								//}
								// получаем указатель на FRAM, где начинается область архива 
								// напряжений и частот
								// для ДАННЫХ i-ых суток данного iRec-тного архива
								ushort UnfStart = (pageTopPtrs.Data["ArchUnF"] as ushort[])[pqpSet[i].PqpZone];
								// теперь где конец
								ushort UnfEnd = 0;
								if (pqpSet[i].PqpZone < 7) UnfEnd =
									(pageTopPtrs.Data["ArchUnF"] as ushort[])[pqpSet[i].PqpZone + 1];
								else UnfEnd = (ushort)pageTopPtrs.Data["ArchEvents"];
								UnfEnd--;

								int UnfLeft = pqpSet[i].UnfRecords;

								// "метка кольца" (адрес последней страницы)
								ushort iUnfRingMarkers = devInfo_.Content[iRec].PqpSet[i].UnfPagesNAND[0];

								// DEBUG
								//iUnfRingMarkers *= 2;

								// тут мы знаем область в которой надо читать
								// (с pUnfRegionStart по pUnfRegionEnd, выкидывая на ходу битые блоки),
								// количество записей размером в 44 байта (pqpSet[i].UnfRecords)
								// количество необходимых страниц (UnfPagesToRead)
								// "метку кольца" (iUnfRingMarkers)
								//
								// остается провести аналогию со средними и считать нужные страницы,
								// сохранив количество необходимых записей...

								// current address
								int iUnfAddress = iUnfRingMarkers;
								// list of U'n'F pages addresses
								List<ushort> listUnfPages = new List<ushort>();
								bool bUnfRingMarkers = true;

								while (true)
								{
									if (UnfLeft <= 0)
										break;			// successfull operation end

									if (iUnfAddress == iUnfRingMarkers)
									{
										bUnfRingMarkers = !bUnfRingMarkers;
										if (bUnfRingMarkers) break;			// "marker reached" operation end
									}

									// each page that complete a block must be tested
									// with the bad-block-test
									if (iUnfAddress % pages_per_block == pages_per_block - 1)
									{
										ushort cur_block = (ushort)(iUnfAddress / pages_per_block);
										ushort[] badBlocks = (ushort[])pageBadBlocks.Data["BadBlocks"];
										bool isBad = false;
										for (int iBlock = 0; iBlock < badBlocks.Length; iBlock++)
										{
											if (badBlocks[iBlock] / pages_per_block == cur_block)
											{
												isBad = true;
												break;
											}
										}
										if (isBad)
										{
											iUnfAddress -= pages_per_block;
											continue;
										}
									}

									listUnfPages.Insert(0, (ushort)iUnfAddress);// adding page to the address array
									UnfLeft--;				// decremect number of left pages to add
									iUnfAddress--;			// decremect address value
									if (iUnfAddress < UnfStart)	// correcting address value for ring-like type
										iUnfAddress = UnfEnd;
								}

								// filling page's address array
								pqpSet[i].UnfPagesNAND = new ushort[listUnfPages.Count];
								listUnfPages.CopyTo(pqpSet[i].UnfPagesNAND);
							}
						}
						devInfo_.Content[iRec].PqpSet = pqpSet;
					}
					#endregion

					#region events

					if (bNeedDNO && devInfo_.Content[iRec].DnsNum > 0)
					{
						ushort EventsEnd = devInfo_.Content[iRec].DnsPagesNAND[1];
						if (iRec == 0) iEventsRingMarker = EventsEnd;

						int EventsLeft = (int)Math.Ceiling((double)devInfo_.Content[iRec].DnsNum /
															(double)events_records_per_page);

						// list of events pages addresses
						List<ushort> listEventsPages = new List<ushort>();

						// current (each in the cycle) page address
						ushort iEventAddress = EventsEnd;

						// number of records in the current (each in the cycle) page
						int iEventNumber = (int)(devInfo_.Content[iRec].DnsNum % events_records_per_page);
						//if (iEventNumber == 0)  //mycode
						//iEventNumber = DP_EM33T.events_records_per_page;  //mycode

						while (true)
						{
							if (EventsLeft <= 0)
								break;				// successfull operation end
							if (iEventAddress == iEventsRingMarker && EventsLeft == 0)
							{
								bNeedDNO = false;
								break;				// "marker reached" operation end
							}

							// each page that complete a block must be tested
							// with the bad-block-test
							if (iEventAddress % pages_per_block == pages_per_block - 1)
							{
								ushort cur_block = (ushort)(iEventAddress / pages_per_block);
								ushort[] badBlocks = (ushort[])pageBadBlocks.Data["BadBlocks"];
								bool isBad = false;
								for (int iBlock = 0; iBlock < badBlocks.Length; iBlock++)
								{
									if (badBlocks[iBlock] / pages_per_block == cur_block)
									{
										isBad = true;
										break;
									}
								}
								if (isBad)
								{
									iEventAddress -= pages_per_block;
									continue;
								}
							}

							listEventsPages.Insert(0, (ushort)iEventAddress);	// adding page to the address array
							EventsLeft--;		// decremect number of left pages to add
							iEventAddress--;	// decremect address value
							if (iEventAddress < (ushort)pageTopPtrs.Data["ArchEvents"])	// correcting address value for ring-like type
								iEventAddress = (ushort)((ushort)pageTopPtrs.Data["ArchAVG"] - 1);
							iDNORecords += iEventNumber;		// increment number of events records
							if (iEventNumber != events_records_per_page)
								iEventNumber = events_records_per_page;
						}

						if (bNeedDNO == false)
						{
							// address alignment for the next block.
							// It's needed because only the next block after the page with iNandRingMarker addess 
							// can be assumed as correct (because writing of page in the new block erase all the
							// pages in this block after this)
							int iTailToDel = pages_per_block - listEventsPages[0] % pages_per_block;
							listEventsPages.RemoveRange(0, iTailToDel);
						}

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
				EmService.DumpException(ex, "Error in ReadDeviceInfo() Em33T");
				throw;
			}

			return 0;
		}

		public ExchangeResult Read(EMemory memory_type, AddressMemory address,
						ushort size, ref byte[] buffer, bool check_crc_mode_off)
		{
			int num_attempt = 0;
			ExchangeResult res = ExchangeResult.Other_Error;
			while (++num_attempt <= 3)
			{
				res = exchange_(EOperation.Read, memory_type, address,
								size, ref buffer, check_crc_mode_off);
				if (res == ExchangeResult.OK) break;

				EmService.WriteToLogFailed(String.Format("Reading Error. Attempt {0}", num_attempt));

				if (bCancelReading_)
					break;
			}
			return res;
		}

		public ExchangeResult Write(EMemory memory_type,
						AddressMemory address,
						ushort size,
						ref byte[] buffer)
		{
			try
			{
				int cnt_attempt;// = 5;

				if (memory_type == EMemory.NAND)
				{
					cnt_attempt = 1;
				}
				else
				{
					cnt_attempt = 3;
				}
				IsExchangeFirstInvoke = true;

				int num_attempt = 0;
				ExchangeResult res;
				while ((res = exchange_(EOperation.Write, memory_type, address, size,
							ref buffer, false)) != 0 && num_attempt++ < cnt_attempt)
				{
					EmService.WriteToLogFailed(String.Format("Writing Error. Attempt {0}", num_attempt));
				}
				return res;
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return ExchangeResult.Other_Error;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in Em33TDevice::Write: ");
				throw;
			}
		}

		public bool ReadDeviceSerialNumber(out long serialNumber)
		{
			serialNumber = -1;

			byte[] buffer = null;

			try
			{
				EM33TPSystem pageSystem = new EM33TPSystem();

				if (Read(DeviceIO.Memory.EMemory.FRAM,
							pageSystem.Address,
							pageSystem.Size,
							ref buffer, false) != 0) return false;
				if (!pageSystem.Parse(ref buffer)) return false;

				// parsing device serial number from the FRAM system page
				serialNumber = (long)((ushort)pageSystem.Data["DeviceNumber"]);
				EmService.WriteToLogGeneral("SERIAL NUMBER = " + serialNumber);
				return true;
			}
			finally
			{
				serialNumber_ = serialNumber;
			}
		}

		#endregion

		#region Private Methods

		protected ExchangeResult exchange_(EOperation oper, EMemory memory_type, AddressMemory address, ushort size,
										ref byte[] buffer, bool check_crc_mode_off)
		{
			if (IsExchangeFirstInvoke)
			{
				if (memory_type == DeviceIO.Memory.EMemory.NAND)
				{
					buffer = new byte[bytes_per_page * size];

					for (ushort usPageOffset = 0; usPageOffset < size; usPageOffset++)
					{
						byte[] bufferTemp = null;

						IsExchangeFirstInvoke = false;

						int cnt_attempt = 3;
						int num = 0;
						ExchangeResult res;
						while ((res = exchange_(oper,
							memory_type,
							address,
							1, ref bufferTemp, 
							check_crc_mode_off)) != ExchangeResult.OK)
						{
							if (++num >= cnt_attempt)
								break;

							EmService.WriteToLogFailed(String.Format("Ошибка Чтения. Попытка {0} XX", num));
						}

						if (res != ExchangeResult.OK)
						{
							return res;
						}

						bufferTemp.CopyTo(buffer, usPageOffset * bytes_per_page);

						address.NAND.Page++;
					}

					return ExchangeResult.OK;
				}
			}

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
						case DeviceIO.Memory.EMemory.RAM: mode = 0x09;
							break;
						case DeviceIO.Memory.EMemory.FRAM: mode = 0x0f;
							break;
						case DeviceIO.Memory.EMemory.NAND:
							mode = check_crc_mode_off ? (byte)0x0d : (byte)0x0b;
							size_in_bytes = (ushort)(size_in_bytes * bytes_per_page);
							break;
						default: return ExchangeResult.Other_Error;
					}
					break;
				case EOperation.Write:
					switch (memory_type)
					{
						case DeviceIO.Memory.EMemory.RAM: mode = 0x08;
							break;
						case DeviceIO.Memory.EMemory.FRAM: mode = 0x0e;
							break;
						case DeviceIO.Memory.EMemory.NAND: mode = 0x0a;
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

			if (memory_type == DeviceIO.Memory.EMemory.RAM)
			{
				if (!address.RAM.Exists)
					throw new EmException("Not implemented needed address type: " + memory_type.ToString());

				query_list.Add((byte)(address.RAM.Page % 0x100));	// converting to byte[]
				query_list.Add((byte)(address.RAM.Page / 0x100));	// Page address
				query_list.Add((byte)(address.RAM.Shift % 0x100));	// converting to byte[]
				query_list.Add((byte)(address.RAM.Shift / 0x100));	// Shift address
			}
			else if (memory_type == DeviceIO.Memory.EMemory.FRAM)
			{
				if (!address.FRAM.Exists)
					throw new EmException("Not implemented needed address type: " + memory_type.ToString());

				query_list.Add((byte)(address.FRAM.Address % 0x100));	// converting to byte[]
				query_list.Add((byte)(address.FRAM.Address / 0x100));	// Page address
			}
			else if (memory_type == DeviceIO.Memory.EMemory.NAND)
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

			//-----------------------------------------------------------------
			// Clearing for modem
			//if (portType_ == EmPortType.Modem)
			//{
			//    port_.DiscardInBuffer();
			//}
			//-----------------------------------------------------------------

			if (portManager_.WriteToPort((UInt32)query_buffer.Length, ref query_buffer) != 0)
			{
				EmService.WriteToLogFailed("exchange_: error writing port!");
				return ExchangeResult.Write_Error;						// error exit
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

			/////////////////////////////////
			//DelayAfterPortReading();
			/////////////////////////////////

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

			/////////////////////////////////
			//DelayAfterPortReading();
			/////////////////////////////////

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
