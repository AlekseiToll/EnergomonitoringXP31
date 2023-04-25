using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Resources;
using System.Threading;
using System.Xml.Serialization;

using DeviceIO;
using EmDataSaver.SavingInterface;
using EmDataSaver.SavingInterface.CheckTreeView;
using EmDataSaver.XmlImage;
using EmServiceLib;
using EmServiceLib.SavingInterface;
using EmArchiveTree;

namespace EmDataSaver
{
	public class EmDataReader33 : EmDataReaderBase
	{
		#region Events

		public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
		public event SetCntArchivesHandler OnSetCntArchives;

		public delegate void StartProgressBarHandler(double reader_percent_for_one_step);
		public event StartProgressBarHandler OnStartProgressBar;

		#endregion

		#region Fields

		private DeviceCommonInfoEm33 devInfo_;
		private EmDevice device_;

		private EmXmlDeviceImage xmlImage_;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor with a parameters to save data from the DEVICE
		/// </summary>
		public EmDataReader33(
			object sender,
			Settings settings,
			BackgroundWorker bw,
			ref EmXmlDeviceImage xmlImage,
			IntPtr hMainWnd)
			: base(hMainWnd)
		{
			this.sender_ = sender;
			this.settings_ = settings;
			this.bw_ = bw;
			this.xmlImage_ = xmlImage;
		}

		#endregion

		#region Main methods

		public override bool ReadDataFromDevice()
		{
			try
			{
				if (!ConnectToDevice()) return false;

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				frmDeviceExchange wndDeviceExchange = new frmDeviceExchange(ref devInfo_);
				// дерево выбора архивов
				if (wndDeviceExchange.ShowDialog(this.sender_ as Form) != DialogResult.OK)
				{
					e_.Cancel = true;
					return false;
				}

				#region progressbar
				//////////////////////////////
				// вычисляем количество страниц в архиве (for ProgressBar)
				DeviceTreeView devTree = wndDeviceExchange.tvDeviceData;
				for (int i = 0; i < devInfo_.Content.Length; i++)
				{
					if (((ObjectTreeNode)devTree.Nodes[0].Nodes[i]).CheckState !=
								CheckState.Unchecked)
					{
						ObjectTreeNode objNodeTmp = (ObjectTreeNode)devTree.Nodes[0].Nodes[i];
						DeviceIO.ContentsLineEm33 cl = devInfo_.Content[i];
						foreach (MeasureTypeTreeNode typeNode in objNodeTmp.Nodes)
						{
							switch (typeNode.MeasureType)
							{
								case MeasureType.PQP:
									for (int ii = 0; ii < cl.PqpSet.Length; ii++)
									{
										if (typeNode.Nodes.Count > ii &&
											(typeNode.Nodes[ii] as CheckTreeNode).CheckState != 
											CheckState.Unchecked)
										{
											if (cl.PqpSet[ii].UnfPagesNAND != null)
												cnt_pages_to_read_ += (ulong)cl.PqpSet[ii].UnfPagesNAND.Length;
										}
									}
									break;
								case MeasureType.AVG:
									bool[] AvgSubsForSaving = new bool[] { false, false, false };

									for (int ii = 0; ii < typeNode.Nodes[0].Nodes.Count; ii++)
									{
										SubMeasureTreeNode curNode = (typeNode.Nodes[0].Nodes[ii] as SubMeasureTreeNode);
										if (curNode.CheckState != CheckState.Unchecked)
										{
											switch (curNode.SubMeasureType)
											{
												case SubMeasureType.AVGMain:
													AvgSubsForSaving[0] = true; break;
												case SubMeasureType.AVGHarmonics:
													AvgSubsForSaving[1] = true; break;
												case SubMeasureType.AVGAngles:
													AvgSubsForSaving[2] = true; break;
											}
										}
									}

									int pageIndex = -1;
									for (int ii = 0; ii < cl.AvgPagesNAND.Length; ii++)
									{
										// сейчас чтение по usb написано так, что читаются в 
										// любом случае все страницы, поэтому проверку делаем
										// только для RS
										if (device_.PortType == EmPortType.COM)
										{
											if (++pageIndex == 5) pageIndex = 0;

											if (pageIndex == 0 && !AvgSubsForSaving[0]) continue;
											if (pageIndex == 1 && !AvgSubsForSaving[1]) continue;
											if ((pageIndex == 2 || pageIndex == 3) &&
												!AvgSubsForSaving[2]) continue;
											if (pageIndex == 4) continue;
										}

										++cnt_pages_to_read_;
									}
									break;
								case MeasureType.DNS:
									cnt_pages_to_read_ += (ulong)cl.DnsPagesNAND.Length;
									break;
							}
						}
					}
				}
				// делаем ProgressBar с запасом, иначе на последних шагах он долго висит 
				// заполненный
				cnt_pages_to_read_ += (ulong)cnt_pages_to_read_ / 10;
				reader_percent_for_one_step_ += 100.0 * 1.0 / cnt_pages_to_read_;
				//////////////////////////////
				#endregion

				this.debugMode_ = wndDeviceExchange.DEBUG_MODE_FLAG;
				this.bCreateImageOnly_ = wndDeviceExchange.CREATE_IMAGE_ONLY_FLAG;

				if (cnt_pages_to_read_ == 0) cnt_pages_to_read_ = 1;
				if (OnStartProgressBar != null) OnStartProgressBar(100.0 * 1.0 / cnt_pages_to_read_);

				#region FOR_DEBUG

				if (DEBUG_MODE_FLAG)
				{
					System.IO.StreamWriter myWriter = null;
					System.Xml.Serialization.XmlSerializer mySerializer = null;
					try
					{
						// Create an XmlSerializer for the 
						// ApplicationSettings type.
						mySerializer = new System.Xml.Serialization.XmlSerializer(
							typeof(DeviceIO.DeviceCommonInfoEm33));
						myWriter = new System.IO.StreamWriter(
							EmService.GetXmlInfoFileName(EmDeviceType.EM33T), 
							false);
						// Serialize this instance of the ApplicationSettings 
						// class to the config file.
						mySerializer.Serialize(myWriter, devInfo_);
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "Error in ReadDataFromDevice() 1:");
					}
					finally
					{
						// If the FileStream is open, close it.
						if (myWriter != null)
						{
							myWriter.Close();
							myWriter = null;
						}
					}
				}

				#endregion

				// получаем имя для файла sql образа
				this.sqlImageFileName_ = wndDeviceExchange.ImageFileName;

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				bool res = false;
				// формируем страничный XML образ
				res = formDeviceImage(wndDeviceExchange.tvDeviceData, ref xmlImage_);
				if (!res && !e_.Cancel && !bw_.CancellationPending)
				{
					ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);
					string msg = rm.GetString("msg_device_connect_lost_text");
					string cap = rm.GetString("msg_device_connect_lost_caption");
					msg = string.Format(msg, device_.PortType, string.Empty, string.Empty);
					msg = msg.Remove(msg.IndexOf("(") - 1, msg.IndexOf(")") - msg.IndexOf("(") + 2);
					MessageBox.Show(sender_ as Form, msg, cap, MessageBoxButtons.OK, MessageBoxIcon.Error);

					e_.Cancel = true;
					return false;
				}

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				return true;
			}
			catch (EmDeviceEmptyException ex)
			{
				throw;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReadDataFromDevice():");
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				throw;
			}
			finally
			{
				if (device_ != null) device_.Close();
			}
		}

		public override void SetCancelReading()
		{
			device_.BCancelReading = true;
		}

		#endregion

		#region Connection With Device

		private bool ConnectToDevice()
		{
			try
			{
				object[] port_params = null;
				EmPortType curInterface = settings_.IOInterface;

				// if not RS-485 then we have to set the broadcasting address
				if (curInterface != EmPortType.Rs485 && curInterface != EmPortType.Modem &&
					curInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
					settings_.AutoSettings.AutoDeviceAddress = 0xFFFF;
				}

				if (curInterface == EmPortType.COM)
				{
					port_params = new object[2];
					port_params[0] = settings_.SerialPortName;
					port_params[1] = settings_.SerialPortSpeed;
				}
				if (curInterface == EmPortType.Modem ||
					curInterface == EmPortType.Ethernet ||
					curInterface == EmPortType.Rs485 ||
					curInterface == EmPortType.GPRS)
				{
					throw new EmInvalidInterfaceException();
				}

				#region Write debug info

				string debugInfo = DateTime.Now.ToString() + "   create device: "
								+ settings_.CurDeviceType.ToString() +
								"  " + curInterface.ToString() + "  ";

				debugInfo += "{not auto mode} ";
				if (curInterface == EmPortType.COM)
				{
					debugInfo += (settings_.SerialPortName + "  ");
					debugInfo += (settings_.SerialPortSpeed + "  ");
				}
				if (curInterface == EmPortType.Modem ||
					curInterface == EmPortType.Ethernet ||
					curInterface == EmPortType.Rs485 ||
					curInterface == EmPortType.GPRS)
				{
					throw new EmException("Invalid I/O interface for Em33T!");
				}
				EmService.WriteToLogGeneral(debugInfo);

				#endregion

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				long serial = -1;
				switch (settings_.CurDeviceType)
				{
					case EmDeviceType.EM33T:
					case EmDeviceType.EM33T1:
					case EmDeviceType.NONE:
						if (settings_.IOInterface == EmPortType.COM ||
									settings_.IOInterface == EmPortType.USB)
						{
							device_ = new Em33TDevice(curInterface, settings_.CurDeviceAddress,
													false, port_params, (sender_ as Form).Handle);
							serial = device_.OpenDevice();
						}
						else
						{
							throw new EmInvalidInterfaceException();
						}
						break;

					case EmDeviceType.EM31K:
						if (settings_.IOInterface == EmPortType.COM ||
									settings_.IOInterface == EmPortType.USB)
						{
							device_ = new Em31KDevice(curInterface,
												settings_.CurDeviceAddress, false, port_params,
												(sender_ as Form).Handle);
							serial = device_.OpenDevice();
						}
						else
						{
							throw new EmInvalidInterfaceException();
						}
						break;
				}
				if (serial == -1)
				{
					throw new EmDisconnectException();
				}

				device_.SerialNumber = serial;

				device_.OnStepReading +=
						new EmDevice.StepReadingHandler(saver_OnStepReading);

				if (DeviceLicenceCheck(serial) == false)
				{
					MessageBoxes.DeviceIsNotLicenced(sender_, this);
					throw new EmException("Device is not licenced");
				}

				(device_ as Em33TDevice).ErrorPages.Clear();
				ExchangeResult errCode = device_.ReadDeviceInfo();
				//if (errCode == ExchangeResult.CRC_Error || errCode == ExchangeResult.NormalByte_Error)
				//{
				if ((device_ as Em33TDevice).ErrorPages.Count > 0)
				{
					MessageBoxes.CRCerrorPages(sender_, this, (device_ as Em33TDevice).ErrorPages);
				}
				//}

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				// if reading device contents was not successfull
				if (errCode != ExchangeResult.OK)
				{
					if(!e_.Cancel && !bw_.CancellationPending)
						MessageBoxes.ReadDevInfoError(sender_, this, curInterface, port_params);
					throw new EmException("Unable to read device contents");
				}
				if (!device_.IsSomeArchiveExist())
				{
					throw new EmDeviceEmptyException();
				}

				if (settings_.CurDeviceType == EmDeviceType.EM33T ||
					settings_.CurDeviceType == EmDeviceType.EM33T1)
					devInfo_ = (device_ as Em33TDevice).DeviceInfo;
				else if (settings_.CurDeviceType == EmDeviceType.EM31K)
					devInfo_ = (device_ as Em31KDevice).DeviceInfo;

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				return true;
			}
			catch (EmDeviceEmptyException ex)
			{
				throw;
			}
			catch (ThreadAbortException)
			{
				EmService.WriteToLogFailed("ThreadAbortException in ConnectToDevice()");
				Thread.ResetAbort();
				return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ConnectToDevice():");
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				//throw;
				return false;
			}
			finally
			{
				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
				}
			}
		}

		private void saver_OnStepReading(EmDeviceType devType)
		{
			try
			{
				// set ProgressBar position
				ReaderReportProgress(Constants.stepProgress);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in saver_OnStepReading(): " + ex.Message);
			}
		}

		#endregion

		#region Private XML Image methods

		/// <summary>
		/// Top-level method of forming device image object
		/// </summary>
		/// <param name="devTree">Tree with checked/unchecked nodes</param>
		/// <param name="devInfo">Device information</param>
		/// <param name="devImage">Future XML device image</param>
		/// <param name="portType">Connection interface type</param>
		/// <param name="portSettings">Connection interface settings</param>
		/// <returns>True if all OK, false if was error while reading data</returns>
		private bool formDeviceImage(
			DeviceTreeView devTree,			
			ref EmXmlDeviceImage devImage)
		{
			try
			{
				devImage.Name = devInfo_.Name;
				devImage.SerialNumber = devInfo_.SerialNumber;
				devImage.InternalType = devInfo_.InternalType;
				devImage.DeviceType = devInfo_.DeviceType;
				devImage.Version = devInfo_.Version;

				//EmDeviceType CurrentDevice = GetDeviceType(devImage.InternalType);
				List<EmXmlArchive> list = new List<EmXmlArchive>();

				// считаем количество отмеченных архивов
				int evnt_archs = 0;
				for (int i = 0; i < devInfo_.Content.Length; i++)
				{
					if (((ObjectTreeNode)devTree.Nodes[0].Nodes[i]).CheckState !=
								CheckState.Unchecked)
					{
						evnt_archs++;
					}
				}

				int evnt_arch = 1;
				for (int i = 0; i < devInfo_.Content.Length; i++)
				{
					ObjectTreeNode objNode = (ObjectTreeNode)devTree.Nodes[0].Nodes[i];
					if (objNode.CheckState == CheckState.Unchecked)
						continue;

					// sending event OnSetCntArchives (X from Y)
					if (OnSetCntArchives != null) OnSetCntArchives(evnt_archs, evnt_arch++);

					EmXmlArchive archive = new EmXmlArchive();

					if (!formArchiveImage(objNode, devInfo_.Content[i], ref archive))
						return false;
					list.Add(archive);

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}
				devImage.ArchiveList = new EmXmlArchive[list.Count];
				list.CopyTo(devImage.ArchiveList);
				return true;
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in formDeviceImage():  " + e.Message);
				throw e;
			}
			finally
			{
				if (device_ != null) device_.Close();
			}
		}

		/// <summary>
		/// Method for filling each arhive object 
		/// in the device image with data from the Device
		/// </summary>
		/// <param name="objNode">Object node</param>
		/// <param name="cl">Content line</param>
		/// <param name="archive">Future XML arhive</param>
		/// <param name="portType">Connection interface type</param>
		/// <param name="portSettings">Connection interface settings</param>
		/// <returns>True if all OK or false</returns>
		private bool formArchiveImage(
			ObjectTreeNode objNode,
			ContentsLineEm33 cl,
			ref EmXmlArchive archive)
		{
			archive.ObjectName = cl.ObjectName;
			archive.CommonBegin = cl.CommonBegin;
			archive.CommonEnd = cl.CommonEnd;
			archive.ConnectionScheme = cl.ConnectionScheme;
			archive.CurrentTransducerIndex = cl.CurrentTransducerIndex;
			archive.F_Nominal = cl.F_Nominal;
			archive.U_NominalLinear = cl.U_NominalLinear;
			archive.U_NominalPhase = cl.U_NominalPhase;
			archive.I_Limit = cl.I_Limit;
			archive.U_Limit = cl.U_Limit;
			archive.T_fliker = cl.t_fliker;
			archive.DnsTimer = cl.DnsTimer;
			archive.AvgTime = cl.AvgTime;

			List<EmXmlArchivePart> pqpList = new List<EmXmlArchivePart>();

			try
			{
				bool success = false;

				foreach (MeasureTypeTreeNode typeNode in objNode.Nodes)
				{
					if (typeNode.CheckState == CheckState.Unchecked) continue;

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}

					switch (typeNode.MeasureType)
					{
						case MeasureType.PQP:
							{								
								for (int i = 0; i < cl.PqpSet.Length; i++)
								{
									if (typeNode.Nodes.Count > i &&
										(typeNode.Nodes[i] as CheckTreeNode).CheckState != CheckState.Unchecked)
									{
										EmXmlPQP archivePart = new EmXmlPQP();
										archivePart.T_fliker = cl.t_fliker;
										success = formArchivePartImagePQP(
											ref cl.PqpSet[i], ref archivePart);
										if (success) pqpList.Add(archivePart);

										if (!success && !e_.Cancel && !bw_.CancellationPending)
										{
											EmService.WriteToLogFailed("PQP Archive, start date " +
												cl.PqpSet[i].PqpStart.ToString() + ", was not read!");

											ResourceManager rm = new ResourceManager("EmDataSaver.emstrings",
												this.GetType().Assembly);
											string msg = string.Format(rm.GetString("pke_reading_failed"),
												cl.PqpSet[i].PqpStart.ToString());
											string cap = rm.GetString("msg_device_data_reading_error_caption");

											MessageBox.Show(sender_ as Form, msg, cap, MessageBoxButtons.OK,
												MessageBoxIcon.Error);
											//return false;
										}
									}

									if (bw_.CancellationPending)
									{
										e_.Cancel = true;
										return false;
									}
								}
							}
							break;
						case MeasureType.AVG:
							{								
								List<SubMeasureType> avgSubs_list = new List<SubMeasureType>();

								for (int i = 0; i < typeNode.Nodes[0].Nodes.Count; i++)
								{
									if ((typeNode.Nodes[0].Nodes[i] as SubMeasureTreeNode).CheckState == 
										CheckState.Checked)
									{
										avgSubs_list.Add(
											(typeNode.Nodes[0].Nodes[i] as SubMeasureTreeNode).SubMeasureType);
									}

									if (bw_.CancellationPending)
									{
										e_.Cancel = true;
										return false;
									}
								}
								
								SubMeasureType[] avgSubs = new SubMeasureType[avgSubs_list.Count];
								avgSubs_list.CopyTo(avgSubs);

								EmXmlAVG archivePart = new EmXmlAVG();
								success = formArchivePartImageAVG(cl, avgSubs, ref archivePart);
								if (success) archive.ArchiveAVG = archivePart;
							}
							break;
						case MeasureType.DNS:
							{
								EmXmlDNS archivePart = new EmXmlDNS();
								success = formArchivePartImageDNS(cl, ref archivePart);
								if (success) archive.ArchiveDNS = archivePart;

								if (bw_.CancellationPending)
								{
									e_.Cancel = true;
									return false;
								}
							}
							break;
					}
					//if (!success) break;
				}
				archive.ArchivePQP = new EmXmlPQP[pqpList.Count];
				pqpList.CopyTo(archive.ArchivePQP);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formArchiveImage():");
				throw;
			}
			return true;
		}

		/// <summary>Method for filling each PQP part of arhive</summary>
		/// <param name="pqpSet">One element of PqpSet</param>
		/// <param name="archivePart">Future XML part of archive</param>
		/// <param name="portType">Connection interface type</param>
		/// <param name="portOpenIDs">Connection opened interface identifiers</param>
		/// <returns>True if all OK or False</returns>
		private bool formArchivePartImagePQP(
			ref DeviceIO.PqpSetEm33 pqpSet,
			ref EmXmlPQP archivePart)
		{
			try
			{
				if (device_.DeviceType == EmDeviceType.EM31K) return false;

				if (pqpSet.UnfPagesNAND == null) return false;

				archivePart.Start = pqpSet.PqpStart;
				archivePart.End = pqpSet.PqpEnd;

				ExchangeResult errCode;

				DeviceIO.Memory.AddressMemory addr = new DeviceIO.Memory.AddressMemory();
				addr.NAND.Flash = 0;
				addr.NAND.Page = pqpSet.PqpPageNAND;
				addr.NAND.Exists = true;

				byte[] buffer = null;

				//errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND,
				//	addr, 8, ref buffer, false);
				//if (errCode != ExchangeResult.OK) return false;

				errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 8, ref buffer, false);
				if (errCode == ExchangeResult.NormalByte_Error || errCode == ExchangeResult.CRC_Error)
				{
					EmService.WriteToLogFailed("formArchivePartImagePQP: CRC Error in NAND, page " + addr.NAND.Page);
					MessageBoxes.CRCerrorPages(sender_, this, addr.NAND.Page);
					if ((device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 8, ref buffer, true) != ExchangeResult.OK)
					{
						EmService.WriteToLogFailed("Without CRC check - error too!");
						return false;
					}
				}
				if (errCode != ExchangeResult.OK)
				{
					EmService.WriteToLogFailed("Error in EmDataReader33::formArchivePartImageAVG: Memory = NAND, Page = " +
						addr.NAND.Page.ToString());
					return false;
				}

				byte[] cunfbuffer = null;
				byte[] unfbuffer = new byte[Em33TDevice.bytes_per_page * pqpSet.UnfPagesNAND.Length];
				for (int i = 0; i < pqpSet.UnfPagesNAND.Length; i++)
				{
					addr.NAND.Page = pqpSet.UnfPagesNAND[i];
					//errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND,
					//addr, 1, ref cunfbuffer, false);
					//if (errCode != ExchangeResult.OK) return false;
					errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 1, ref cunfbuffer, false);
					if (errCode == ExchangeResult.NormalByte_Error || errCode == ExchangeResult.CRC_Error)
					{
						EmService.WriteToLogFailed("formArchivePartImagePQP: CRC Error in NAND, page " + addr.NAND.Page);
						MessageBoxes.CRCerrorPages(sender_, this, addr.NAND.Page);
						if ((device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 1, ref cunfbuffer, true) != ExchangeResult.OK)
						{
							EmService.WriteToLogFailed("Without CRC check - error too!");
							return false;
						}
					}
					if (errCode != ExchangeResult.OK)
					{
						EmService.WriteToLogFailed(
							"Error in EmDataReader33::formArchivePartImagePQP: Memory = NAND, Page = " +
							addr.NAND.Page.ToString() + ", i = " + i.ToString());
						return false;
					}

					cunfbuffer.CopyTo(unfbuffer, i * Em33TDevice.bytes_per_page);

					if ((i % Constants.stepProgress) == 0)
					{
						// set ProgressBar position
						ReaderReportProgress(Constants.stepProgress);
					}

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}

				archivePart.DataPages = new byte[buffer.Length + unfbuffer.Length];
				buffer.CopyTo(archivePart.DataPages, 0);
				unfbuffer.CopyTo(archivePart.DataPages, 0x1000);

				archivePart.StandardSettingsType = Conversions.bytes_2_ushort(ref buffer, 384);
				if (archivePart.StandardSettingsType < 0 || archivePart.StandardSettingsType > 10)
				{
					EmService.WriteToLogFailed("invalid settings type:  " + 
						archivePart.StandardSettingsType);
					archivePart.StandardSettingsType = 1;
				}

				archivePart.PqpZone = pqpSet.PqpZone;
				archivePart.UnfPagesLength = pqpSet.UnfRecords;

				return (errCode == ExchangeResult.OK);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver::formArchivePartImagePQP(): ");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		/// <summary>Method for filling each AVG part of arhive</summary>
		/// <param name="cl">Content line</param>
		/// <param name="avgSubs">Averaged sub part types</param>
		/// <param name="archivePart">Future XML part of archive</param>
		/// <param name="portType">Connection interface type</param>
		/// <param name="portOpenIDs">Connection opened interface identifiers</param>
		/// <returns>True if all OK or False</returns>
		private bool formArchivePartImageAVG(
			DeviceIO.ContentsLineEm33 cl,
			SubMeasureType[] avgSubs,
			ref EmXmlAVG archivePart)
		{
			try
			{
				if (device_.DeviceType == EmDeviceType.EM31K) return false;

				archivePart.Start = cl.AvgBegin;
				archivePart.End = cl.AvgEnd;
				archivePart.AvgTime = cl.AvgTime;
				archivePart.AvgSub = new AvgSubTypes[avgSubs.Length];
				bool[] AvgSubsForSaving = new bool[] { false, false, false };
				for (int i = 0; i < avgSubs.Length; i++)
				{
					switch (avgSubs[i])
					{
						case SubMeasureType.AVGMain:
							archivePart.AvgSub[i] = AvgSubTypes.Main;
							AvgSubsForSaving[0] = true;
							break;
						case SubMeasureType.AVGHarmonics:
							archivePart.AvgSub[i] = AvgSubTypes.Harmonics;
							AvgSubsForSaving[1] = true;
							break;
						case SubMeasureType.AVGAngles:
							archivePart.AvgSub[i] = AvgSubTypes.HarmonicPowersAndAngles;
							AvgSubsForSaving[2] = true;
							break;
					}
				}
				archivePart.AvgNum = cl.AvgNum;
				ExchangeResult errCode = ExchangeResult.Other_Error;

				// дифференциация методов считывания в
				// зависимости от используемого протокола
				if (device_.PortType == EmPortType.COM)
				{
					#region Serial reading

					DeviceIO.Memory.AddressMemory addr = new DeviceIO.Memory.AddressMemory();
					addr.NAND.Flash = 0;
					addr.NAND.Exists = true;
					byte[] pageBuffer = null;

					byte[] data = new byte[cl.AvgPagesNAND.Length * Em33TDevice.bytes_per_page];
					int pageIndex = -1;
					int pageNumberDone = 0;
					for (int i = 0; i < cl.AvgPagesNAND.Length; i++)
					{
						if (++pageIndex == 5) pageIndex = 0;

						if (pageIndex == 0 && !AvgSubsForSaving[0]) continue;
						if (pageIndex == 1 && !AvgSubsForSaving[1]) continue;
						if ((pageIndex == 2 || pageIndex == 3) && !AvgSubsForSaving[2])
							continue;
						if (pageIndex == 4) continue;

						addr.NAND.Page = cl.AvgPagesNAND[i];
						//errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 1,
						//						ref pageBuffer, false);
						errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 1, ref pageBuffer, false);
						if (errCode == ExchangeResult.NormalByte_Error || errCode == ExchangeResult.CRC_Error)
						{
							EmService.WriteToLogFailed("formArchivePartImageAVG: CRC Error in NAND, page " + addr.NAND.Page);
							MessageBoxes.CRCerrorPages(sender_, this, addr.NAND.Page);
							if ((device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 1, ref pageBuffer, true) != ExchangeResult.OK)
							{
								EmService.WriteToLogFailed("Without CRC check - error too!");
								return false;
							}
						}
						else if (errCode != ExchangeResult.OK) return false;

						pageBuffer.CopyTo(data, pageNumberDone * Em33TDevice.bytes_per_page);
						pageNumberDone++;

						if ((pageNumberDone % Constants.stepProgress) == 0)
						{
							// set ProgressBar position
							ReaderReportProgress(Constants.stepProgress);
						}

						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}
					}
					archivePart.DataPages = new byte[pageNumberDone * Em33TDevice.bytes_per_page];
					Array.Copy(data, 0, archivePart.DataPages, 0, pageNumberDone * Em33TDevice.bytes_per_page);
					
					#endregion
				}
				else if (device_.PortType == EmPortType.USB)
				{
					#region USB reading

					#region DebugB

#if DEBUG
					// пишем исходные страницы, которые должны быть прочитаны
					System.IO.StreamWriter sw = null;
					try
					{
						sw = new System.IO.StreamWriter(@"c:\em33.avg.txt");
						sw.WriteLine("Исходные адреса архивов Усредненных Значений, которые должны быть считаны:");

						for (int i = 0; i < cl.AvgPagesNAND.Length; i++)
						{
							sw.Write(string.Format("0x{0:X}h\t", cl.AvgPagesNAND[i]));
							if (i % 8 == 0 && i != 0) sw.WriteLine(string.Empty);
						}
						sw.WriteLine(string.Empty);
						sw.WriteLine("******************************************************");
						sw.WriteLine(string.Empty);
					}
					catch (Exception ex)
					{
						sw.WriteLine(ex.Message);
					}
					finally
					{
						sw.Close();
					}
#endif
					#endregion //debug

					List<List<ushort>> listPageBlocks = new List<List<ushort>>();
					List<ushort> listCurrentPageBlock = new List<ushort>();

					listCurrentPageBlock.Add(cl.AvgPagesNAND[0]);
					int nn = cl.AvgPagesNAND[0];
					for (int i = 1; i < cl.AvgPagesNAND.Length; i++)
					{
						++nn;
						if (listCurrentPageBlock.Count < 0x80 - 0x08 &&
						   cl.AvgPagesNAND[i] == nn)
						{
							listCurrentPageBlock.Add(cl.AvgPagesNAND[i]);
						}
						else
						{
							listPageBlocks.Add(listCurrentPageBlock);
							listCurrentPageBlock = new List<ushort>();
							listCurrentPageBlock.Add(cl.AvgPagesNAND[i]);
							nn = cl.AvgPagesNAND[i];
						}

						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}
					}
					listPageBlocks.Add(listCurrentPageBlock);

					#region Debug

#if DEBUG
					// пишем страницы блоками, которыми будем читать
					try
					{
						sw = new System.IO.StreamWriter(@"c:\em33.avg.txt", true);
						sw.WriteLine(string.Format("Блоки, которыми будет производится чтение (всего {0} блоков):", listPageBlocks.Count));
						sw.WriteLine("(нумерация как и везде начинается с нуля ;)");
						for (int i = 0; i < listPageBlocks.Count; i++)
						{
							sw.WriteLine(string.Format("Блок {0}. Количество страниц {1}:", i, listPageBlocks[i].Count));

							for (int j = 0; j < listPageBlocks[i].Count; j++)
							{
								sw.Write(string.Format("0x{0:X}h\t", listPageBlocks[i][j]));
								if (j % 8 == 0 && j != 0) sw.WriteLine(string.Empty);
							}
						}

						sw.WriteLine(string.Empty);
						sw.WriteLine("**************************************************************************");
						sw.WriteLine(string.Empty);
					}
					catch (Exception ex)
					{
						sw.WriteLine(ex.Message);
					}
					finally
					{
						sw.Close();
					}
#endif
					#endregion //debug

					DeviceIO.Memory.AddressMemory addr = new DeviceIO.Memory.AddressMemory();
					addr.NAND.Flash = 0;
					addr.NAND.Exists = true;
					byte[] pageBuffer = null;
					byte[] data = new byte[cl.AvgPagesNAND.Length * Em33TDevice.bytes_per_page];
					byte[] data1 = new byte[cl.AvgPagesNAND.Length * Em33TDevice.bytes_per_page];
					int dstIndexBlockShift = 0;

					#region DebugA

#if DEBUG
					// пишем страницы которые копируем по порядку
					try
					{
						sw = new System.IO.StreamWriter(@"c:\em33.avg.txt", true);
						sw.WriteLine("Выборочное копирование прочитанных страниц из блоков.");
						sw.WriteLine(" Данная часть файла формируется до непосредственного чтения, а не в его момент.");
						sw.WriteLine(" следовательно, после формирования данной части файла может что-нибудь вылететь,");
						sw.WriteLine(" однако данная часть файла как раз должна помочь объяснить - ПОЧЕМУ вылетело!");
						sw.WriteLine(string.Empty);
						sw.WriteLine(" З.Ы. Чтение производится без проверки контрольных сумм, чтобы не падало,");
						sw.WriteLine("      если в блоке будет не закрытая контрольной суммой страница.");
						sw.WriteLine(string.Empty);
						sw.WriteLine(string.Format("Размер целевого массива в страницах: 0x{0:X}h ({0}), в байтах 0x{1:X}h ({1}).",
							cl.AvgPagesNAND.Length, data.Length));
						sw.WriteLine(string.Empty);
						for (int i = 0; i < listPageBlocks.Count; i++)
						{
							sw.WriteLine(string.Format("Блок {0}. Адреса: 0x{1:X}h..0x{2:X}h",
								i,
								(ushort)(listPageBlocks[i][listPageBlocks[i].Count - 1] - listPageBlocks[i][0]),
								listPageBlocks[i][listPageBlocks[i].Count - 1]));
							sw.WriteLine(string.Format("Размер блока в страницах: 0x{0:X}h ({0}), в байтах 0x{1:X}h ({1}).",
								listPageBlocks[i].Count,
								listPageBlocks[i].Count * Em33TDevice.bytes_per_page));
							sw.WriteLine("Страницы:");
							for (int j = 0; j < listPageBlocks[i].Count; j++)
							{
								sw.WriteLine(string.Format("\t0x{0:X}h\tCмещение в исходном массиве: 0x{1:X}h\tСмещение в целевом массиве: 0x{2:X}h",
									listPageBlocks[i][j],
									(listPageBlocks[i][j] - listPageBlocks[i][0] * Em33TDevice.bytes_per_page),
									(dstIndexBlockShift + j) * Em33TDevice.bytes_per_page));
							}
							dstIndexBlockShift += listPageBlocks[i].Count;
						}

					}
					catch (Exception ex)
					{
						sw.WriteLine(ex.Message);
					}
					finally
					{
						sw.Close();
					}
					dstIndexBlockShift = 0;
#endif
					#endregion  //debug

					int cnt_readed = 0;
					for (int i = 0; i < listPageBlocks.Count; i++)
					{
						// reading
						addr.NAND.Page = listPageBlocks[i][0];
						//errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr,
						//	(ushort)listPageBlocks[i].Count, ref pageBuffer, true);
						errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, (ushort)listPageBlocks[i].Count, 
								ref pageBuffer, false);
						if (errCode == ExchangeResult.NormalByte_Error || errCode == ExchangeResult.CRC_Error)
						{
							EmService.WriteToLogFailed("formArchivePartImageAVG: CRC Error in NAND, page " + addr.NAND.Page);
							MessageBoxes.CRCerrorPages(sender_, this, addr.NAND.Page);
							if ((device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, (ushort)listPageBlocks[i].Count, 
								ref pageBuffer, true) != ExchangeResult.OK)
							{
								EmService.WriteToLogFailed("Without CRC check - error too!");
								return false;
							}
						}
						if (errCode != ExchangeResult.OK)
						{
							EmService.WriteToLogFailed(
								"Error in EmDataReader33::formArchivePartImageAVG: Memory = NAND, Page = " +
								addr.NAND.Page.ToString() + ", Count = " + listPageBlocks[i].Count.ToString()
								+ ", i = " + i.ToString());
						}

						int lenToCopy = listPageBlocks[i].Count * Em33TDevice.bytes_per_page;
						if (lenToCopy > pageBuffer.Length)
						{
							EmService.WriteToLogFailed(string.Format("formArchivePartImageAVG: lenToCopy > pageBuffer.Length, {0}, {1}",
														lenToCopy, pageBuffer.Length));
							lenToCopy = pageBuffer.Length;
						}
						Array.Copy(pageBuffer,		// source array
								   0,               // source index
								   data,		    // destination array
								   dstIndexBlockShift * Em33TDevice.bytes_per_page,	// destination index
								   lenToCopy);  // lenght 
						dstIndexBlockShift += listPageBlocks[i].Count;

						// для ProgressBar
						for (int j = 0; j < listPageBlocks[i].Count; ++j)
						{
							++cnt_readed;
							if ((cnt_readed % Constants.stepProgress) == 0)
							{
								// set ProgressBar position
								ReaderReportProgress(Constants.stepProgress);
							}
						}

						if (bw_.CancellationPending)
						{
							e_.Cancel = true;
							return false;
						}
					}
			
					// analyzing and copying to dst array
					int pageIndex = -1;
					int pageNumberDone = 0;
					for (int j = 0; j < cl.AvgPagesNAND.Length; j++)
					{
						if (++pageIndex == 5) pageIndex = 0;
						if (pageIndex == 0 && !AvgSubsForSaving[0]) continue;
						if (pageIndex == 1 && !AvgSubsForSaving[1]) continue;
						if ((pageIndex == 2 || pageIndex == 3) && !AvgSubsForSaving[2]) continue;
						if (pageIndex == 4) continue;

						Array.Copy(data,		    // source array
							j * Em33TDevice.bytes_per_page,  // source index
							data1,		    // destination array
							pageNumberDone * Em33TDevice.bytes_per_page,  // destination index
							Em33TDevice.bytes_per_page);    	// lenght
						pageNumberDone++;
					}
					long len = pageNumberDone * Em33TDevice.bytes_per_page;
					archivePart.DataPages = new byte[len];
					if (data1.Length < len)
						EmService.WriteToLogFailed(
							"data1.Length < pageNumberDone * Em33TDevice.bytes_per_page!!!");
					Array.Copy(data1, 0, archivePart.DataPages, 0,
						len < data1.Length ? len : data1.Length);
					#endregion   //end of USB reading
				}
				return errCode == ExchangeResult.OK;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in EmDataSaver::formArchivePartImageAVG():");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				return false;
			}
		}

		/// <summary>Method for filling each DNS part of arhive</summary>
		/// <param name="cl">Content line</param>
		/// <param name="archivePart">Future XML part of archive</param>
		/// <returns>True if all OK or False</returns>
		private bool formArchivePartImageDNS(
			DeviceIO.ContentsLineEm33 cl,
			ref EmXmlDNS archivePart)
		{
			try
			{
				archivePart.Start = cl.DnsStart;
				archivePart.End = cl.DnsEnd;
				archivePart.DnsNum = cl.DnsNum;
				archivePart.DnsTimer = cl.DnsTimer;

				int PageSize = 0;
				if (device_.DeviceType == EmDeviceType.EM33T ||
					device_.DeviceType == EmDeviceType.EM33T1) 
					PageSize = Em33TDevice.bytes_per_page;
				if (device_.DeviceType == EmDeviceType.EM31K) PageSize = Em31KDevice.bytes_per_page;

				DeviceIO.Memory.AddressMemory addr = new DeviceIO.Memory.AddressMemory();
				addr.NAND.Flash = 0;
				addr.NAND.Exists = true;
				byte[] pageBuffer = null;

				archivePart.DataPages = new byte[cl.DnsPagesNAND.Length * PageSize];
				ExchangeResult errCode = ExchangeResult.Other_Error;
				for (int i = 0; i < cl.DnsPagesNAND.Length; i++)
				{
					addr.NAND.Page = cl.DnsPagesNAND[i];

					if (device_.DeviceType == EmDeviceType.EM33T ||
						device_.DeviceType == EmDeviceType.EM33T1)
					{
						//errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND,
						//										addr, 1, ref pageBuffer, false);

						errCode = (device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 1, ref pageBuffer, false);
						if (errCode == ExchangeResult.NormalByte_Error || errCode == ExchangeResult.CRC_Error)
						{
							EmService.WriteToLogFailed("formArchivePartImageDNS: CRC Error in NAND, page " + addr.NAND.Page);
							MessageBoxes.CRCerrorPages(sender_, this, addr.NAND.Page);
							if ((device_ as Em33TDevice).Read(DeviceIO.Memory.EMemory.NAND, addr, 1, ref pageBuffer, true) != ExchangeResult.OK)
							{
								EmService.WriteToLogFailed("Without CRC check - error too!");
								return false;
							}
						}
						//else if (errCode != ExchangeResult.OK) return false;
						if (errCode != ExchangeResult.OK)
						{
							EmService.WriteToLogFailed(
								"Error in EmDataReader33::formArchivePartImageDNS: Memory = NAND, Page = " +
								addr.NAND.Page.ToString() + ", i = " + i.ToString());
						}
					}
					if (device_.DeviceType == EmDeviceType.EM31K)
					{
						errCode = (device_ as Em31KDevice).Read(DeviceIO.Memory.EMemory.NAND,
																addr, 1, ref pageBuffer, false);
					}

					pageBuffer.CopyTo(archivePart.DataPages, i * PageSize);

					if ((i % Constants.stepProgress) == 0)
					{
						// set ProgressBar position
						ReaderReportProgress(Constants.stepProgress);
					}

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}

				return errCode == ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EmDataSaver::formArchivePartImageDNS(): ");
				//if (EmService.ShowWndFeedback)
				//{
				//    frmSentLogs frmLogs = new frmSentLogs();
				//    frmLogs.ShowDialog();
				//    EmService.ShowWndFeedback = false;
				//}
				return false;
			}
		}

		#endregion

		#region Properties

		public bool IsDevInfoFull
		{
			get { return this.devInfo_ != null; }
		}

		#endregion
	}
}
