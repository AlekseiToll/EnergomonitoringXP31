using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Resources;
using System.Threading;
using System.Xml.Serialization;
using System.Globalization;

using DeviceIO;
using EmDataSaver.SavingInterface;
using EmDataSaver.SavingInterface.CheckTreeView;
using EmDataSaver.XmlImage;
using EmServiceLib;
using EmServiceLib.SavingInterface;
using EmArchiveTree;

namespace EmDataSaver
{
	public class EtDataReaderPQP : EmDataReaderBase
	{
		#region Events

		public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
		public event SetCntArchivesHandler OnSetCntArchives;

		public delegate void StartProgressBarHandler(double reader_percent_for_one_step);
		public event StartProgressBarHandler OnStartProgressBar;

		#endregion

		#region Fields

		public delegate void SetValueHandler(int val);
		public event SetValueHandler OnSetValue;
		public delegate void HideMessageWndHandler();
		public event HideMessageWndHandler OnHideMessageWnd;
		private CalcKuThread threadCalcProgress_ = null;
		private Thread thread_;

		private DeviceCommonInfoEtPQP devInfo_;
		private EtPQPDevice device_;

		private EtPQPXmlDeviceImage xmlImage_;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor with a parameters to save data from the DEVICE
		/// </summary>
		public EtDataReaderPQP(
			Form sender,
			Settings settings,
			BackgroundWorker bw,
			ref EtPQPXmlDeviceImage xmlImage,
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
				for (int iObj = 0; iObj < devInfo_.Content.Count; iObj++)
				{
					if (((ObjectTreeNode)devTree.Nodes[0].Nodes[iObj]).CheckState !=
								CheckState.Unchecked)
					{
						ObjectTreeNode objNodeTmp = (ObjectTreeNode)devTree.Nodes[0].Nodes[iObj];
						DeviceIO.ContentsLineEtPQP cl = devInfo_.Content[iObj];
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
											cnt_pages_to_read_++;
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
				res = formDeviceImage(wndDeviceExchange.tvDeviceData);
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

				if (curInterface == EmPortType.COM)
				{
					port_params = new object[2];
					port_params[0] = settings_.SerialPortName;
					port_params[1] = settings_.SerialPortSpeed;
				}
				else
				{
					throw new EmInvalidInterfaceException();
				}

				#region Write debug info

				string debugInfo = DateTime.Now.ToString() + "   create device: ETPQP " +
								curInterface.ToString() + "  ";

				debugInfo += "{not auto mode} ";
				if (curInterface == EmPortType.COM)
				{
					debugInfo += (settings_.SerialPortName + "  ");
					debugInfo += (settings_.SerialPortSpeed + "  ");
				}
				else
				{
					throw new EmException("Invalid I/O interface for EtPQP!");
				}
				EmService.WriteToLogGeneral(debugInfo);

				#endregion

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				long serial = -1;
				Thread.Sleep(1000);
				device_ = new EtPQPDevice(curInterface, settings_.CurDeviceAddress, false,
											port_params, (sender_ as Form).Handle);
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
					DoTimeSynchronizationSLIP(ref bufTime, DateTime.Now);
				}
				catch (Exception timeEx)
				{
					EmService.DumpException(timeEx, "Time synchronization error:");
				}

				#endregion

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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error DisconnectDevice():");
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
				EmService.DumpException(ex, "Error in saver_OnStepReading():");
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
		private bool formDeviceImage(DeviceTreeView devTree)
		{
			try
			{
				xmlImage_.SerialNumber = devInfo_.SerialNumber;
				xmlImage_.Version = devInfo_.DevVersion;

				xmlImage_.SD_CurrentRangeName_1 = devInfo_.SD_CurrentRangeName_1;
				xmlImage_.SD_CurrentRangeName_2 = devInfo_.SD_CurrentRangeName_2;
				xmlImage_.SD_CurrentRangeName_3 = devInfo_.SD_CurrentRangeName_3;
				xmlImage_.SD_CurrentRangeName_4 = devInfo_.SD_CurrentRangeName_4;
				xmlImage_.SD_CurrentRangeName_5 = devInfo_.SD_CurrentRangeName_5;
				xmlImage_.SD_CurrentRangeName_6 = devInfo_.SD_CurrentRangeName_6;

				List<EtPQPXmlArchive> list = new List<EtPQPXmlArchive>();

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

					EtPQPXmlArchive archive = new EtPQPXmlArchive();

					if (!formArchiveImage(objNode, devInfo_.Content[i], ref archive))
						return false;
					list.Add(archive);

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}
				xmlImage_.ArchiveList = new EtPQPXmlArchive[list.Count];
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
		private bool formArchiveImage(
			ObjectTreeNode objNode,
			DeviceIO.ContentsLineEtPQP cl,
			ref EtPQPXmlArchive archive)
		{
			archive.GlobalObjectId = cl.GlobalObjectId;
			archive.ObjectName = cl.ObjectName;
			archive.CommonBegin = cl.CommonBegin;
			archive.CommonEnd = cl.CommonEnd;
			archive.ConnectionScheme = cl.ConnectionScheme;
			archive.CurrentTransducerIndex = cl.CurrentTransducerIndex;
			archive.ConstraintType = cl.ConstraintType;
			archive.F_Nominal = cl.F_Nominal;
			archive.U_NominalLinear = cl.U_NominalLinear;
			archive.U_NominalPhase = cl.U_NominalPhase;
			archive.I_NominalPhase = cl.I_NominalPhase;
			archive.I_Limit = cl.I_Limit;
			archive.U_Limit = cl.U_Limit;
			archive.T_fliker = cl.t_fliker;
			archive.MlStartTime1 = cl.MlStartTime1;
			archive.MlEndTime1 = cl.MlEndTime1;
			archive.MlStartTime2 = cl.MlStartTime2;
			archive.MlEndTime2 = cl.MlEndTime2;

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
									EmXmlPQP archivePart = new EmXmlPQP();
									archivePart.T_fliker = cl.t_fliker;
									success = formArchivePartImagePQP(cl.PqpSet[i], ref archivePart, 
												cl.GlobalObjectId);
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
							// здесь 3 - число типов усреденения: 3 сек, 1 мин, 30 мин
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

									DateTime dtStart = (typeNode.Nodes[iAvg] as MeasureTreeNode).StartDateTime;
									DateTime dtEnd = (typeNode.Nodes[iAvg] as MeasureTreeNode).EndDateTime;
									AvgTypes curType = AvgTypes.ThreeSec;

									if(curNode.Text.Contains("1 min")) curType = AvgTypes.OneMin;
									else if(curNode.Text.Contains("30 min")) curType = AvgTypes.ThirtyMin;

									// проверяем были ли выбраны параметры. если нет, то берем из формы
									// выбора параметров набор по умолчанию
									if (!curNode.MasksAvgWasSet)
									{
										using (frmAvgParams frmParams =
											new frmAvgParams(objNode.ConnectionScheme,
															EmDeviceType.ETPQP, true))
										{
											curNode.MasksAvg = frmParams.GetMask();
										}
									}

									EmXmlAVG archivePart = new EmXmlAVG();
									success = formArchivePartImageAVG(dtStart, dtEnd,
													ref archivePart, curType,
													curNode.MasksAvg, objNode.ConnectionScheme,
													cl.GlobalObjectId);
									if (success) avgList.Add(archivePart);	

                                    if (!success && !e_.Cancel && !bw_.CancellationPending)
                                    {
                                        EmService.WriteToLogFailed("Unable to read AVG " +
                                            curType.ToString() + "  " + dtStart.ToString());

                                        ResourceManager rm = new ResourceManager("EmDataSaver.emstrings",
                                            this.GetType().Assembly);
                                        string msg = string.Format(rm.GetString("avg_reading_failed"),
                                            dtStart.ToString());
                                        string cap = rm.GetString("msg_device_data_reading_error_caption");

                                        MessageBox.Show(sender_ as Form, msg, cap, MessageBoxButtons.OK,
                                            MessageBoxIcon.Error);
                                        //return false;
                                    }

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
                                DateTime dtStart = (typeNode.Nodes[0/*iDns*/] as MeasureTreeNode).StartDateTime;
                                DateTime dtEnd = (typeNode.Nodes[0/*iDns*/] as MeasureTreeNode).EndDateTime;
								EmXmlDNS archivePart = new EmXmlDNS();
								success = formArchivePartImageDNS(dtStart, dtEnd,
									cl, ref archivePart);

								if (success) dnsList.Add(archivePart);
								
                                if (!success && !e_.Cancel && !bw_.CancellationPending)
                                {
                                    EmService.WriteToLogFailed("Unable to read DNS " + dtStart.ToString());

                                    //ResourceManager rm = new ResourceManager("EmDataSaver.emstrings",
                                    //    this.GetType().Assembly);
                                    //string msg = string.Format(rm.GetString("dns_reading_failed"),
                                    //    dtStart.ToString());
                                    //string cap = rm.GetString("msg_device_data_reading_error_caption");
                                    //MessageBox.Show(sender_ as Form, msg, cap, MessageBoxButtons.OK,
                                    //    MessageBoxIcon.Error);
                                    //return false;
                                }

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
					archive.ArchivePQP = new EmXmlPQP[pqpList.Count];
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
					archive.ArchiveAVG = new EmXmlAVG[avgList.Count];
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

		/// <summary>Method for filling each PQP part of arhive</summary>
		/// <param name="pqpSet">One element of PqpSet</param>
		/// <param name="archivePart">Future XML part of archive</param>
		/// <param name="portType">Connection interface type</param>
		/// <param name="portOpenIDs">Connection opened interface identifiers</param>
		/// <returns>True if all OK or False</returns>
		private bool formArchivePartImagePQP(PqpSetEtPQP pqpSet, ref EmXmlPQP archivePart, 
			int objectId)
		{
			try
			{
				archivePart.Start = pqpSet.PqpStart;
				archivePart.End = pqpSet.PqpEnd;

				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] buffer = null;

				// читаем сам архив
				bool bContinueReading = true;
				while (bContinueReading)
				{
					try
					{
						errCode = device_.Read(EmCommands.COMMAND_ReadQualityEntryByTimestampByObject,
											ref buffer,
											new object[] { (ushort)pqpSet.PqpStart.Year, 
														(byte)pqpSet.PqpStart.Month, 
														(byte)pqpSet.PqpStart.Day,
														(byte)pqpSet.PqpStart.Hour, 
														(byte)pqpSet.PqpStart.Minute, 
														(byte)pqpSet.PqpStart.Second,
														objectId });
					}
					catch (Exception exc)
					{
						EmService.DumpException(exc, "Exception in formArchivePartImagePQP():  ");
						return false;
					}

					if (errCode != ExchangeResult.OK)
					{
						EmService.WriteToLogFailed("Error formArchivePartImagePQP: errCode != 0");
						return false;
					}
					if (buffer == null)
					{
						EmService.WriteToLogFailed("Error formArchivePartImagePQP: buffer == null");
						return false;
					}

					if (buffer.Length != 8192)
					{
						EmService.WriteToLogFailed("Invalid length of PQP archive: " +
							buffer.Length.ToString());
						return false;
					}

					// признак готовности Ku
					short readySign = Conversions.bytes_2_short(ref buffer, 14);

					// uncomment for Ku
					// проверяем успел ли прибор рассчитать Ku
					if (readySign < 242 && (threadCalcProgress_ == null || !threadCalcProgress_.bNotWait))
					{
						EmService.WriteToLogDebug("formArchivePartImagePQP readySign = " + 
							readySign.ToString());

						bContinueReading = true;
						if (threadCalcProgress_ == null)
						{
							threadCalcProgress_ = new CalcKuThread(this, settings_);
							thread_ = new Thread(new ThreadStart(threadCalcProgress_.ThreadEntry));
							thread_.Start();
							Thread.Sleep(100);
						}

						OnSetValue(readySign);

						for (int iSleep = 0; iSleep < 100; ++iSleep)
						{
							if (threadCalcProgress_ != null && threadCalcProgress_.bNotWait)
							{
								bContinueReading = false;
								if (OnHideMessageWnd != null)
									OnHideMessageWnd();
								threadCalcProgress_ = null;
								break;
							}
							Thread.Sleep(50);
							Application.DoEvents();
						}
					}
					else
					{
						bContinueReading = false;
						if (threadCalcProgress_ != null)
						{
							if (OnHideMessageWnd != null)
								OnHideMessageWnd();
							threadCalcProgress_ = null;
						}
						break;
					}	//uncomment for Ku
				}

				archivePart.DataPages = new byte[buffer.Length];
				buffer.CopyTo(archivePart.DataPages, 0);

				// тип уставок
				archivePart.StandardSettingsType = 
					Conversions.bytes_2_ushort(ref buffer, 384);
				if (archivePart.StandardSettingsType < 0 || archivePart.StandardSettingsType > 10)
				{
					EmService.WriteToLogFailed("invalid settings type:  " + 
													archivePart.StandardSettingsType);
					archivePart.StandardSettingsType = 1;
				}

				// читаем массив частот и напряжений
				try
				{
					errCode = device_.Read(EmCommands.COMMAND_ReadQualityVFArraysByTimestampByObject,
										ref buffer,
										new object[] { (ushort)pqpSet.PqpStart.Year, 
														(byte)pqpSet.PqpStart.Month, 
														(byte)pqpSet.PqpStart.Day,
														(byte)pqpSet.PqpStart.Hour, 
														(byte)pqpSet.PqpStart.Minute, 
														(byte)pqpSet.PqpStart.Second,
														objectId });
					archivePart.DataPagesExtra = new byte[buffer.Length];
					buffer.CopyTo(archivePart.DataPagesExtra, 0);
				}
				catch (Exception exc)
				{
					EmService.DumpException(exc, "Exception in formArchivePartImagePQP() 2:  ");
				}

				if (errCode != ExchangeResult.OK)
				{
					EmService.WriteToLogFailed("Error formArchivePartImagePQP 2: errCode != 0");
					return false;
				}
				if (buffer == null)
				{
					EmService.WriteToLogFailed("Error formArchivePartImagePQP 2: buffer == null");
					return false;
				}

				if (buffer.Length != 29544)
				{
					EmService.WriteToLogFailed("Invalid length of PQP archive 2: " +
						buffer.Length.ToString());
					return false;
				}

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
		private bool formArchivePartImageAVG(DateTime dtStart, DateTime dtEnd,
			ref EmXmlAVG archivePart, AvgTypes avgType,
			frmAvgParams.MaskAvgArray masksAvg, ConnectScheme conScheme, int globalObjId)
		{
			try
			{
				archivePart.Start = dtStart;
				archivePart.End = dtEnd;
				archivePart.AvgType = avgType;
				archivePart.MasksAvg = masksAvg;
				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] buffer = null;

				List<ushort> listAvgParams = null;
				frmAvgParams.GetParametersList(out listAvgParams, masksAvg,
												conScheme, EmDeviceType.ETPQP);

				try
				{
					if (listAvgParams == null || listAvgParams.Count < 1)
						errCode = device_.ReadAvgArchive(ref buffer, dtStart, dtEnd, avgType, globalObjId);
					else
						errCode = device_.ReadAvgArchiveSelectively(ref buffer, dtStart, dtEnd, avgType,
																	ref listAvgParams);
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
		private bool formArchivePartImageDNS(
			DateTime dtStart, DateTime dtEnd,
			DeviceIO.ContentsLineEtPQP cl,
			ref EmXmlDNS archivePart)
		{
			try
			{
				archivePart.Start = dtStart;
				archivePart.End = dtEnd;

				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] buffer = null;
				try
				{
					errCode = device_.ReadDipSwellArchive(ref buffer, 
											archivePart.Start, archivePart.End, ref cl);
				}
				catch (Exception exc)
				{
					EmService.DumpException(exc, "Exception in formArchivePartImageDNS(): ");
					return false;
				}
				if (errCode != ExchangeResult.OK) return false;
				if (buffer != null)
				{
					archivePart.DataPages = new byte[buffer.Length];
					buffer.CopyTo(archivePart.DataPages, 0);
				}
				if (cl.CurDnsExists())
					archivePart.CurrentDNSBuffer = cl.BufCurDipSwellData;
				else
					archivePart.CurrentDNSBuffer = null;

				if (buffer == null && archivePart.CurrentDNSBuffer == null)
				{
					EmService.WriteToLogFailed("There was no DNS event during this period: "
						+ dtStart.ToString() + " - " + dtEnd.ToString());
					MessageBoxes.NoDnsEvents(sender_, this);
					return false;
				}

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

	// поток, который показывает предупреждающее окно если расчет Ku не закончен
	class CalcKuThread
	{
		private static object lockWnd_ = new object();
		private frmIfKuCalcCompleted wndInfo_;
		private Settings settings_;

		public bool bNotWait = false;

		public CalcKuThread(EtDataReaderPQP owner, Settings settings)
		{
			owner.OnSetValue += new EtDataReaderPQP.SetValueHandler(wndInfo_OnShowProgress);
			owner.OnHideMessageWnd += new EtDataReaderPQP.HideMessageWndHandler(wndInfo_OnHideMessageWnd);
			settings_ = settings;
		}

		public void ThreadEntry()
		{
			if (settings_.CurrentLanguage == "ru")
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
			}
			else
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			}

			lock (lockWnd_)
			{
				wndInfo_ = new frmIfKuCalcCompleted();
				wndInfo_.ShowDialog();
			}
		}

		void wndInfo_OnShowProgress(int value)
		{
			try
			{
				if (wndInfo_.InvokeRequired == false) // thread checking
				{
					EmService.WriteToLogDebug("!!frmIfKuCalcCompleted value: " + value);

					bNotWait = wndInfo_.bNotwait;
					//lock (lockWnd_)
					//{
						wndInfo_.SetProgressValue(value);
					//}
				}
				else
				{
					EtDataReaderPQP.SetValueHandler showMess =
						new EtDataReaderPQP.SetValueHandler(wndInfo_OnShowProgress);
					wndInfo_.Invoke(showMess, new object[] { value });
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in wndInfo_OnShowMessage(): ");
			}
		}

		void wndInfo_OnHideMessageWnd()
		{
			try
			{
				if (wndInfo_.InvokeRequired == false) // thread checking
				{
					lock (lockWnd_)
					{
						if (wndInfo_ != null) wndInfo_.Hide();
						Thread.Sleep(100);
						wndInfo_ = null;
					}
				}
				else
				{
					EtDataReaderPQP.HideMessageWndHandler hideWnd =
						new EtDataReaderPQP.HideMessageWndHandler(wndInfo_OnHideMessageWnd);
					wndInfo_.Invoke(hideWnd);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in wndInfo_OnHideMessageWnd(): ");
			}
		}
	}
}
