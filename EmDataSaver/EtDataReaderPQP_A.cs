using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Resources;
using System.Threading;
using System.Xml.Serialization;

using Microsoft.Win32;
using NativeWifi;

using DeviceIO;
using EmDataSaver.SavingInterface;
using EmDataSaver.SavingInterface.CheckTreeView;
using EmDataSaver.XmlImage;
using EmServiceLib;
using EmServiceLib.SavingInterface;
using EmArchiveTree;

namespace EmDataSaver
{
	public class EtDataReaderPQP_A : EmDataReaderBase
	{
        #region Events

        public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
        public event SetCntArchivesHandler OnSetCntArchives;

        public delegate void StartProgressBarHandler(double reader_percent_for_one_step);
        public event StartProgressBarHandler OnStartProgressBar;

        #endregion

		#region Fields

		//public delegate void SetValueHandler(int val);
		//public event SetValueHandler OnSetValue;
		//public delegate void HideMessageWndHandler();
		//public event HideMessageWndHandler OnHideMessageWnd;
		//private CalcKuThread threadCalcProgress_ = null;
		//private Thread thread_;

		private DeviceCommonInfoEtPQP_A devInfo_;
		private EtPQP_A_Device device_;

		private EtPQP_A_XmlDeviceImage xmlImage_;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor with a parameters to save data from the DEVICE
		/// </summary>
		public EtDataReaderPQP_A(
			Form sender,
			Settings settings,
			BackgroundWorker bw,
			ref EtPQP_A_XmlDeviceImage xmlImage,
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
				for (int iReg = 0; iReg < devInfo_.Content.Count; iReg++)
				{
					if (((ObjectTreeNode)devTree.Nodes[0].Nodes[iReg]).CheckState !=
								CheckState.Unchecked)
					{
						ObjectTreeNode objNodeTmp = (ObjectTreeNode)devTree.Nodes[0].Nodes[iReg];
						DeviceIO.ContentsLineEtPQP_A cl = devInfo_.Content[iReg];
						foreach (MeasureTypeTreeNode typeNode in objNodeTmp.Nodes)
						{
							switch (typeNode.MeasureType)
							{
								case MeasureType.PQP:
									for (int iPqp = 0; iPqp < cl.PqpSet.Count; iPqp++)
									{
										if (typeNode.Nodes.Count > iPqp &&
											(typeNode.Nodes[iPqp] as CheckTreeNode).CheckState !=
															CheckState.Unchecked)
										{
											cnt_pages_to_read_ += 256;
										}
									}
									break;
								case MeasureType.AVG:
									for (int iAvg = 0; iAvg < typeNode.Nodes.Count; iAvg++)
									{
										MeasureTreeNode curMeasureNode =
											(typeNode.Nodes[iAvg] as MeasureTreeNode);
										if (curMeasureNode.CheckState != CheckState.Unchecked)
										{
											DateTime dtStart = DateTime.MinValue;
											DateTime dtEnd = DateTime.MinValue;
											int interval = 0;
											AvgTypes curAvgType = AvgTypes.ThreeSec;

											if (curMeasureNode.Text.Contains("1 min"))
												curAvgType = AvgTypes.OneMin;
											else if (curMeasureNode.Text.Contains("30 min"))
												curAvgType = AvgTypes.ThirtyMin;

											switch (curAvgType)
											{
												case AvgTypes.ThreeSec: interval = 3; break;
												case AvgTypes.OneMin: interval = 60; break;
												case AvgTypes.ThirtyMin: interval = 1800; break;
											}
											dtStart = curMeasureNode.StartDateTime;
											dtEnd = curMeasureNode.EndDateTime;

											if (curMeasureNode.ListAvgParams != null &&
												curMeasureNode.ListAvgParams.Count > 0)
											{
												ulong NE =
													(ulong)(2052 / (4 + curMeasureNode.ListAvgParams.Count));
												ulong countDates = 0;

												while (dtStart < dtEnd)
												{
													dtStart = dtStart.AddSeconds(interval);
													countDates++;
												}
												cnt_pages_to_read_ +=
													(ulong)Math.Ceiling((double)countDates / (double)NE);
											}
											else  // обычный запрос без параметров
											{
												while (dtStart < dtEnd)
												{
													dtStart = dtStart.AddSeconds(interval);
													cnt_pages_to_read_++;
												}
											}
										}
									}
									break;
								case MeasureType.DNS:
									for (int iDns = 0; iDns < typeNode.Nodes.Count; iDns++)
									{
										if ((typeNode.Nodes[iDns] as CheckTreeNode).CheckState
											!= CheckState.Unchecked)
										{
											// кол-во фаз * 2 (провалы и перенапряжения)
											cnt_pages_to_read_ += EmService.CountPhases * 2;
										}
									}
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
									typeof(DeviceIO.DeviceCommonInfoEtPQP));
						myWriter = new System.IO.StreamWriter(
							EmService.GetXmlInfoFileName(EmDeviceType.ETPQP), 
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
				res = formDeviceImage(wndDeviceExchange.tvDeviceData, wndDeviceExchange.SplitByDays);
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
				EmService.DumpException(ex, "Error in ReadDataFromDevice() 2:");
				//frmSentLogs frmLogs = new frmSentLogs();
				//frmLogs.ShowDialog();
				throw;
			}
			finally
			{
				if (device_ != null) DisconnectDevice();
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
				settings_.LoadSettings();

				object[] port_params = null;
				EmPortType curInterface;

				curInterface = settings_.IOInterface;
				// if not RS-485 then we have to set the broadcasting address
				if (curInterface != EmPortType.Rs485 && curInterface != EmPortType.Modem &&
					curInterface != EmPortType.GPRS)
				{
					settings_.CurDeviceAddress = 0xFFFF;
					settings_.AutoSettings.AutoDeviceAddress = 0xFFFF;
				}

				if (curInterface == EmPortType.USB)
				{

				}
				else if (curInterface == EmPortType.Ethernet)
				{
					port_params = new object[2];
					port_params[0] = settings_.CurrentIPAddress;
					port_params[1] = settings_.CurrentPort;
				}
				else if (curInterface == EmPortType.WI_FI)
				{
					try
					{
						if (!Wlan.IsWifiConnected(false, settings_.CurWifiProfileName))
						{
							WlanClient.WlanInterface wlanIface = Wlan.ConnectWifiEtpqpA(settings_.CurWifiProfileName,
																						settings_.WifiPassword);

							if (!Wlan.IsWifiConnected(true, wlanIface, settings_.CurWifiProfileName))
							{
								EmService.WriteToLogFailed("Wi-fi not connected!");
								return false;
							}
						}
					}
					catch (Exception ex)
					{
						EmService.DumpException(ex, "Exception in ConnectToWifi() WI-FI:");
						return false;
					}

					port_params = new object[2];
					port_params[0] = EmService.GetCurrentDhcpIpAddress();
					port_params[1] = settings_.CurrentPort;
				}
				else
				{
					throw new EmInvalidInterfaceException();
				}

				#region Write debug info

				string debugInfo = DateTime.Now.ToString() + "   create device: ETPQP-A " +
								curInterface.ToString() + "  ";
				debugInfo += "{not auto mode} ";
				if (curInterface == EmPortType.WI_FI)
				{
					debugInfo += string.Format(" Wi-fi: {0}, {1}, {2}", settings_.CurWifiProfileName,
						settings_.WifiPassword, EmService.GetCurrentDhcpIpAddress());
				}
				EmService.WriteToLogGeneral(debugInfo);
				#endregion

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				long serial = -1;
				Thread.Sleep(500);
				device_ = new EtPQP_A_Device(curInterface, settings_.CurDeviceAddress, false,
											port_params, settings_.CurWifiProfileName, settings_.WifiPassword,
											(sender_ as Form).Handle);
				serial = device_.OpenDevice();
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

				#region Time Synchro
				// проверяем соответствует ли время на компе или приборе, если разница больше минуты,
				// выдаем предупреждение
				try
				{
					byte[] bufTime = null;
					device_.ReadTime(ref bufTime);
					DateTime res = Conversions.bytes_2_DateTimeSLIP2(ref bufTime, 0);
					DoTimeSynchronizationSLIP(ref bufTime, DateTime.Now);
				}
				catch (Exception timeEx)
				{
					EmService.DumpException(timeEx, "Time synchronization error:");
				}

				#endregion

				// считываем данные об арихвах
				ExchangeResult errCode = device_.ReadDeviceInfo();

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				// if reading device contents was not successfull
				if (errCode != ExchangeResult.OK)
				{
					if (!e_.Cancel && !bw_.CancellationPending)
						MessageBoxes.ReadDevInfoError(sender_, this, curInterface, port_params);
					throw new EmException("Unable to read device contents");
				}

				// проверяем на сайте есть ли новая прошивка для прибора
				try
				{
					if (settings_.CheckFirmwareEtPQP_A)
					{
						FTPClient.FTPMarsClient ftp = new FTPClient.FTPMarsClient();
						DateTime dtFtp = ftp.FtpGetFirmwareVersion();
						EmService.WriteToLogDebug("Date FTP: " + dtFtp.ToString());
						EmService.WriteToLogDebug("Date Dev: " + device_.DeviceInfo.DevVersionDate.ToString());
						if (dtFtp > device_.DeviceInfo.DevVersionDate)
						{
							frmNewFirmware frm = new frmNewFirmware();
							frm.ShowDialog();

							//settings_.LoadSettings();
							if (settings_.CheckFirmwareEtPQP_A != frm.ShowThisMessage)
							{
								settings_.CheckFirmwareEtPQP_A = frm.ShowThisMessage;
								//settings_.SaveSettings();
							}
						}
					}
				}
				catch (Exception ftpEx)
				{
					EmService.DumpException(ftpEx, "Error in FTP code:");
				}

				if (!device_.IsSomeArchiveExist())
				{
					throw new EmDeviceEmptyException();
				}

				devInfo_ = device_.DeviceInfo;

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
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("EmException in ReadDeviceInfo()" + emx.Message);
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
				settings_.SaveSettings();

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
				}
			}
		}

		public bool DisconnectDevice()
		{
			try
			{
				EmService.WriteToLogGeneral("DisconnectDevice entry");

				if (device_ == null) return false;

				return device_.Close();
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error DisconnectDevice(): " + e.Message);
				return false;
			}
		}

		private void saver_OnStepReading(EmDeviceType devType)
		{
			try
			{
				// set ProgressBar position
				ReaderReportProgress(1.0);
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
		private bool formDeviceImage(DeviceTreeView devTree, bool splitByDays)
		{
			try
			{
				xmlImage_.SerialNumber = devInfo_.SerialNumber;
				xmlImage_.Version = devInfo_.DevVersion;

				//xmlImage_.SD_CurrentRangeName_1 = devInfo_.SD_CurrentRangeName_1;
				//xmlImage_.SD_CurrentRangeName_2 = devInfo_.SD_CurrentRangeName_2;
				//xmlImage_.SD_CurrentRangeName_3 = devInfo_.SD_CurrentRangeName_3;
				//xmlImage_.SD_CurrentRangeName_4 = devInfo_.SD_CurrentRangeName_4;
				//xmlImage_.SD_CurrentRangeName_5 = devInfo_.SD_CurrentRangeName_5;
				//xmlImage_.SD_CurrentRangeName_6 = devInfo_.SD_CurrentRangeName_6;

				List<EtPQP_A_XmlArchive> list = new List<EtPQP_A_XmlArchive>();

				// считаем количество отмеченных архивов
				int evnt_archs = 0;
				for (int i = 0; i < devInfo_.Content.Count; i++)
				{
					if (((ObjectTreeNode)devTree.Nodes[0].Nodes[i]).CheckState !=    //имя объекта
								CheckState.Unchecked)
					{
						evnt_archs++;
					}
				}

				int evnt_arch = 1;
				for (int i = 0; i < devInfo_.Content.Count; i++)
				{
					ObjectTreeNode objNode = (ObjectTreeNode)devTree.Nodes[0].Nodes[i];
					if (objNode.CheckState == CheckState.Unchecked)
						continue;

					// sending event OnSetCntArchives (X from Y)
					if (OnSetCntArchives != null) OnSetCntArchives(evnt_archs, evnt_arch++);

					EtPQP_A_XmlArchive archive = new EtPQP_A_XmlArchive();

					if (!formArchiveImage(objNode, devInfo_.Content[i], ref archive, splitByDays))
						return false;
					list.Add(archive);

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}
				xmlImage_.ArchiveList = new EtPQP_A_XmlArchive[list.Count];
				list.CopyTo(xmlImage_.ArchiveList);
				return true;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in formDeviceImage(): ");
				throw e;
			}
			finally
			{
				if (device_ != null) DisconnectDevice();
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
		private bool formArchiveImage(ObjectTreeNode objNode, ContentsLineEtPQP_A cl,
								ref EtPQP_A_XmlArchive archive, bool splitByDays)
		{
			archive.RegistrationId = cl.RegistrationId;
			archive.ObjectName = cl.ObjectName;
			archive.CommonBegin = cl.CommonBegin;
			archive.CommonEnd = cl.CommonEnd;
			archive.ConnectionScheme = cl.ConnectionScheme;
			archive.ConstraintType = cl.ConstraintType;
			archive.Constraints = cl.Constraints;
			archive.SysInfo = cl.SysInfo.Clone();
			archive.DevVersion = cl.DevVersion;
			archive.DevVersionDate = cl.DevVersionDate;

			List<EmXmlArchivePart> pqpList = new List<EmXmlArchivePart>();
			List<EmXmlArchivePart> dnsList = new List<EmXmlArchivePart>();
			List<EmXmlArchivePart> avgList = new List<EmXmlArchivePart>();

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
							for (int i = 0; i < cl.PqpSet.Count; i++)
							{
								if (typeNode.Nodes.Count > i &&
									(typeNode.Nodes[i] as CheckTreeNode).CheckState != CheckState.Unchecked)
								{
									EmXmlPQP_PQP_A archivePart = new EmXmlPQP_PQP_A();
									//archivePart.T_fliker = cl.t_fliker;
									success = formArchivePartImagePQP(cl.PqpSet[i], ref archivePart, 
												cl.RegistrationId, cl.ConstraintType);
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

									// set ProgressBar position
									ReaderReportProgress(1.0);

									if (bw_.CancellationPending)
									{
										e_.Cancel = true;
										return false;
									}
								}
							}

							break;
						case MeasureType.AVG:
							// здесь 3 - число типов усреденения: 3 сек, 10 мин, 2 hours
							// UPD: после добавления возможности разбиения на сутки тут элементов дерева
							// может быть не 3, а сколько угодно.
							for (int iAvg = 0; iAvg < typeNode.Nodes.Count /*3*/; iAvg++)
							{
							    if (typeNode.Nodes.Count > iAvg &&
							        (typeNode.Nodes[iAvg] as CheckTreeNode).CheckState !=
							                CheckState.Unchecked)
							    {
							        // sending event OnSetCntArchives (X from Y)
							        //if (OnSetCntArchives != null)
							        //	OnSetCntArchives(cnt_archives, cnt_arch_been_read++);

							        MeasureTreeNode curNode = typeNode.Nodes[iAvg] as MeasureTreeNode;

									AvgTypes_PQP_A curType = AvgTypes_PQP_A.ThreeSec;
                                    if (curNode.Text.Contains("10 min")) curType = AvgTypes_PQP_A.TenMin;
                                    else if (curNode.Text.Contains("2 hours")) curType = AvgTypes_PQP_A.TwoHours;

									// надо найти новый индекс если изменялся интервал,
									// а если не изменялся, то старый индекс в cl
							        DateTime dtStart = (typeNode.Nodes[iAvg] as MeasureTreeNode).StartDateTime;
							        DateTime dtEnd = (typeNode.Nodes[iAvg] as MeasureTreeNode).EndDateTime;
									UInt32 indexStart = cl.GetAVGStartIndexByType(curType);//675
									UInt32 indexEnd = cl.GetAVGEndIndexByType(curType);//969
									if ((dtStart > (typeNode.Nodes[iAvg] as MeasureTreeNode).OriginDateStart) || splitByDays)
									{
										if (!GetAVGIndexForData(out indexStart, ref cl, ref dtStart, curType))
										{
											//MessageBoxes.UnableToReadNewAvgIndex(sender_, this);
											//continue;
											EmService.WriteToLogFailed("Unable to read new AVG start index!");
											indexStart = cl.GetAVGStartIndexByType(curType);
										}
									}
									if ((dtEnd < (typeNode.Nodes[iAvg] as MeasureTreeNode).OriginDateEnd) || splitByDays)
									{
										if (!GetAVGIndexForData(out indexEnd, ref cl, ref dtEnd, curType))
										{
											//MessageBoxes.UnableToReadNewAvgIndex(sender_, this);
											//continue;
											EmService.WriteToLogFailed("Unable to read new AVG end index!");
											indexEnd = cl.GetAVGEndIndexByType(curType);
										}
									}

                                    EmXmlAVG_PQP_A archivePart = new EmXmlAVG_PQP_A();
                                    success = formArchivePartImageAVG(dtStart, dtEnd,
													indexStart, indexEnd,
                                                    ref archivePart, curType,
                                                    objNode.ConnectionScheme,
                                                    ref cl);
                                    if (success) avgList.Add(archivePart);

                                    if (bw_.CancellationPending)
                                    {
                                        e_.Cancel = true;
                                        return false;
                                    }
                                }
                            }

							break;
						case MeasureType.DNS:
							if (typeNode.Nodes.Count > /*iDns*/0 &&
									(typeNode.Nodes[0/*iDns*/] as CheckTreeNode).CheckState !=
											CheckState.Unchecked)
							{
								EmXmlDNS archivePart = new EmXmlDNS();
								success = formArchivePartImageDNS(
									//(typeNode.Nodes[0/*iDns*/] as MeasureTreeNode).StartDateTime,
									//(typeNode.Nodes[0/*iDns*/] as MeasureTreeNode).EndDateTime,
									cl, ref archivePart);

								if (success) dnsList.Add(archivePart);

								if (bw_.CancellationPending)
								{
									e_.Cancel = true;
									return false;
								}
							}

							break;
					}
					if (!success) break;
				}

				//bool partsExists = false;
				if (pqpList.Count > 0)
				{
					archive.ArchivePQP = new EmXmlPQP_PQP_A[pqpList.Count];
					pqpList.CopyTo(archive.ArchivePQP);
					//partsExists = true;
				}

				if (dnsList.Count > 0)
				{
					archive.ArchiveDNS = new EmXmlDNS[dnsList.Count];
					dnsList.CopyTo(archive.ArchiveDNS);
					//partsExists = true;
				}

				if (avgList.Count > 0)
				{
					archive.ArchiveAVG = new EmXmlAVG_PQP_A[avgList.Count];
					avgList.CopyTo(archive.ArchiveAVG);
					//partsExists = true;
				}
				//if (!partsExists) return false;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formArchiveImage():");
				throw;
			}
			return true;
		}

		private bool GetAVGIndexForData(out UInt32 index, ref ContentsLineEtPQP_A cl, ref DateTime dt, AvgTypes_PQP_A type)
		{
			try
			{
				index = 0;
				EmCommands curCommand = EmCommands.COMMAND_ReadAverageArchive3SecIndexByDateTime;
				if (type == AvgTypes_PQP_A.TenMin) 
					curCommand = EmCommands.COMMAND_ReadAverageArchive10MinIndexByDateTime;
				if (type == AvgTypes_PQP_A.TwoHours) 
					curCommand = EmCommands.COMMAND_ReadAverageArchive2HourIndexByDateTime;

				byte[] buffer = null;
				ExchangeResult errCode = device_.Read(curCommand, ref buffer, new object[] { 
									cl.RegistrationId, (ushort)dt.Year, 
									(ushort)dt.Month, (ushort)dt.Day, (ushort)dt.Hour, 
									(ushort)dt.Minute, (ushort)dt.Second, cl.TimeZone });

				if (errCode != ExchangeResult.OK)
				{
					EmService.WriteToLogFailed("Error GetAVGIndexForData: errCode != 0");
					return false;
				}
				if (buffer == null)
				{
					EmService.WriteToLogFailed("Error GetAVGIndexForData: buffer == null");
					return false;
				}
				if (buffer.Length < 4)
				{
					EmService.WriteToLogFailed("Invalid length GetAVGIndexForData: " + dt.ToString());
					return false;
				}

				DateTime dtNew = Conversions.bytes_2_DateTimeEtPQP_A_Local(ref buffer, 4, "GetAVGIndexForData");
				TimeSpan diff = dt - dtNew;
				if (diff > new TimeSpan(0, 0, 5))
				{
					EmService.WriteToLogFailed(string.Format("Error in GetAVGIndexForData: {0}, {1}",
						dt.ToString(), dtNew.ToString()));
					return false;
				}

				index = Conversions.bytes_2_uint_new(ref buffer, 0);
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetAVGIndexForData:");
				throw;
			}
		}

		/// <summary>Method for filling each PQP part of arhive</summary>
		/// <param name="pqpSet">One element of PqpSet</param>
		/// <param name="archivePart">Future XML part of archive</param>
		/// <param name="portType">Connection interface type</param>
		/// <param name="portOpenIDs">Connection opened interface identifiers</param>
		/// <returns>True if all OK or False</returns>
		private bool formArchivePartImagePQP(PqpSetEtPQP_A pqpSet, ref EmXmlPQP_PQP_A archivePart,
			UInt32 regId, int standardSettingsType)
		{
			try
			{
				EmService.WriteToLogGeneral("EtPQP-A formArchivePartImagePQP start");
				if(settings_.IOInterface == EmPortType.USB) 
					device_.RestartUSB();

				archivePart.Start = pqpSet.PqpStart;
				archivePart.End = pqpSet.PqpEnd;

				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] buffer = null;
				byte[] pqpBuffer = new byte[EtPQP_A_Device.PqpArchiveLength];

				int cnt_error = 0;
				try
				{
					for (ushort iSegment = 0; iSegment < EtPQP_A_Device.CntPqpSegments; ++iSegment)
					{
						if (cnt_error > 10) return false;

						errCode = device_.Read(EmCommands.COMMAND_ReadRegistrationArchiveByIndex,
											ref buffer,
											new object[] { pqpSet.PqpIndex, iSegment });

						if (errCode != ExchangeResult.OK)
						{
							EmService.WriteToLogFailed("Error formArchivePartImagePQP: errCode != 0");
							//return false;
							cnt_error++;
							iSegment--;
							continue;
						}
						if (buffer == null)
						{
							EmService.WriteToLogFailed("Error formArchivePartImagePQP: buffer == null");
							//return false;
							cnt_error++;
							iSegment--;
							continue;
						}
						if (buffer.Length != (EtPQP_A_Device.PqpSegmentLength + 6))
						{
							EmService.WriteToLogFailed("Invalid length of PQP archive: " +
								buffer.Length.ToString());
							//return false;
							cnt_error++;
							iSegment--;
							continue;
						}
						// если дошли до этой строки, значит было успешное чтение, тогда
						// обнуляем счетчик ошибок
						cnt_error = 0;

						// убираем первые 6 байт, в которых archive_id и номер сегмента
						byte[] buffer_old = buffer;
						buffer = new byte[EtPQP_A_Device.PqpSegmentLength];
						Array.Copy(buffer_old, 6, buffer, 0, EtPQP_A_Device.PqpSegmentLength);

						// копируем сегмент в общий буфер архива
						Array.Copy(buffer, 0, pqpBuffer, iSegment * EtPQP_A_Device.PqpSegmentLength,
							EtPQP_A_Device.PqpSegmentLength);

                        // set a new position for ProgressBar
                        //if (OnStepReading != null) OnStepReading(EmDeviceType.ETPQP);
                        ReaderReportProgress(1);
					}
				}
				catch (Exception exc)
				{
					EmService.DumpException(exc, "Exception in formArchivePartImagePQP():  ");
					return false;
				}

				archivePart.DataPages = new byte[pqpBuffer.Length];
				pqpBuffer.CopyTo(archivePart.DataPages, 0);

				// тип уставок
				archivePart.StandardSettingsType = standardSettingsType;

				EmService.WriteToLogGeneral("EtPQP-A formArchivePartImagePQP end");

				return (errCode == ExchangeResult.OK);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formArchivePartImagePQP:  ");
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
		private bool formArchivePartImageAVG(DateTime dtStart, DateTime dtEnd, UInt32 indexStart, UInt32 indexEnd,
            ref EmXmlAVG_PQP_A archivePart, AvgTypes_PQP_A avgType,
            ConnectScheme conScheme, ref ContentsLineEtPQP_A cl)
		{
			try
			{
				EmService.WriteToLogGeneral("EtPQP-A formArchivePartImageAVG start");

				archivePart.Start = dtStart;
				archivePart.End = dtEnd;
				archivePart.AvgType = avgType;
				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] buffer = null;

                try
                {
                    errCode = device_.ReadAvgArchive(ref buffer, dtStart, dtEnd, indexStart, indexEnd,
						avgType, ref cl);
                }
                catch (Exception exc)
                {
                    EmService.DumpException(exc, "Exception in formArchivePartImageAVG(): ");
                }
				if (errCode != ExchangeResult.OK) return false;
				if (buffer != null)
				{
					archivePart.DataPages = new byte[buffer.Length];
					buffer.CopyTo(archivePart.DataPages, 0);
				}
				else
				{
					EmService.WriteToLogFailed("Unable to read AVG archive!");
					return false;
				}

				EmService.WriteToLogGeneral("EtPQP-A formArchivePartImageAVG end");

				return errCode == ExchangeResult.OK;
			}
			catch (Exception e)
			{
				EmService.DumpException(e, "Error in formArchivePartImageAVG(): ");
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
		private bool formArchivePartImageDNS(//DateTime dtStart, DateTime dtEnd,
			DeviceIO.ContentsLineEtPQP_A cl, ref EmXmlDNS archivePart)
		{
			try
			{
				EmService.WriteToLogGeneral("EtPQP-A formArchivePartImageDNS start");

				archivePart.Start = cl.CommonBegin;
				archivePart.End = cl.CommonEnd;

				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] buffer = null;
				try
				{
					errCode = device_.ReadDipSwellArchive(ref buffer, 
											//archivePart.Start, archivePart.End, 
											cl.RegistrationId);
				}
				catch (Exception exc)
				{
					EmService.DumpException(exc, "Exception in formArchivePartImageDNS(): ");
					return false;
				}
				if (errCode != ExchangeResult.OK) return false;

				if (buffer != null && buffer.Length == 0) buffer = null;

				if (buffer != null)
				{
					EmService.WriteToLogGeneral("DNS was found: buffer != null");
					archivePart.DataPages = new byte[buffer.Length];
					buffer.CopyTo(archivePart.DataPages, 0);
				}

				if (buffer == null /*&& archivePart.CurrentDNSBuffer == null*/)
				{
					EmService.WriteToLogFailed("There was no DNS event during this period: "
						+ cl.CommonBegin.ToString() + " - " + cl.CommonEnd.ToString());
					MessageBoxes.NoDnsEvents(sender_, this);
					return false;
				}

				EmService.WriteToLogGeneral("EtPQP-A formArchivePartImageDNS end");

				return errCode == ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formArchivePartImageDNS():");
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
