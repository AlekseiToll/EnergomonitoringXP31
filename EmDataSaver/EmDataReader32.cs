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

using DbServiceLib;
using DeviceIO;
using EmDataSaver.SavingInterface;
using EmDataSaver.SavingInterface.CheckTreeView;
using EmDataSaver.XmlImage;
using EmServiceLib;
using EmServiceLib.SavingInterface;
using EmArchiveTree;

namespace EmDataSaver
{
	public class EmDataReader32 : EmDataReaderBase
	{
		#region Events

		public delegate void SetCntArchivesHandler(int totalArchives, int curArchive);
		public event SetCntArchivesHandler OnSetCntArchives;

		public delegate void SetSerialNumberHandler(long serial, string info, EmPortType type);
		public event SetSerialNumberHandler OnSetSerialNumber;

		public delegate void StartProgressBarHandler(double reader_percent_for_one_step);
		public event StartProgressBarHandler OnStartProgressBar;

		#endregion

		#region Fields

        //ссылка на очереди автоматич.опроса
        private AutoConnect autoConnectQueues_;

		private string pgConnectStr_;

		private Em32Device device_;

		private Em32XmlDeviceImage xmlImage_;

		#endregion

		#region Constructors

		public EmDataReader32(
			Form sender,
			Settings settings,
			string pgConnectStr,
			BackgroundWorker bw,
			ref Em32XmlDeviceImage xmlImage,
			AutoConnect autoConnectQueues,
			bool auto,
			IntPtr hMainWnd)
			: base(hMainWnd)
		{
			this.sender_ = sender;
			this.settings_ = settings;
            this.bAutoMode_ = auto;
			this.autoConnectQueues_ = autoConnectQueues;
			this.bw_ = bw;
			this.pgConnectStr_ = pgConnectStr;
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

				frmDeviceExchange wndDeviceExchange = new frmDeviceExchange(ref device_, pgConnectStr_);
				if (!bAutoMode_)
				{
					// дерево выбора архивов
					if (wndDeviceExchange.ShowDialog(this.sender_ as Form) != DialogResult.OK)
					{
						e_.Cancel = true;
						return false;
					}
				}

				#region progressbar
				//////////////////////////////
				// вычисляем количество страниц в архиве (for ProgressBar)
				DeviceTreeView devTree = wndDeviceExchange.tvDeviceData;

				if (devTree.Nodes.Count > 0 && devTree.Nodes[0].Nodes.Count > 0)
				{
					if (((ObjectTreeNode)devTree.Nodes[0].Nodes[0]).CheckState !=
								CheckState.Unchecked)
					{
						ObjectTreeNode objNodeTmp = (ObjectTreeNode)devTree.Nodes[0].Nodes[0];
						foreach (MeasureTypeTreeNode typeNode in objNodeTmp.Nodes)
						{
							switch (typeNode.MeasureType)
							{
								case MeasureType.PQP:
									for (int iPqp = 0; iPqp < typeNode.Nodes.Count; iPqp++)
									{
										if ((typeNode.Nodes[iPqp] as CheckTreeNode).CheckState
											!= CheckState.Unchecked)
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
				cnt_pages_to_read_ += 1;
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
							typeof(DeviceIO.DeviceCommonInfoEm32));
						myWriter = new System.IO.StreamWriter(
							EmService.GetXmlInfoFileName(EmDeviceType.EM32), 
							false);
						// Serialize this instance of the ApplicationSettings 
						// class to the config file.
						mySerializer.Serialize(myWriter, device_.DeviceInfo);
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
							if (myWriter != null)
							{
								try { if (myWriter != null) myWriter.Close(); }
								catch { }
							}
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

					if (!bAutoMode_)
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

				if (!bAutoMode_)
				{
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
					if (curInterface == EmPortType.Modem)
					{
						port_params = new object[5];
						port_params[0] = settings_.SerialPortNameModem;
						port_params[1] = settings_.SerialSpeedModem;
						port_params[2] = settings_.CurPhoneNumber;
						port_params[3] = settings_.AttemptNumber;
						port_params[4] = settings_.CurDeviceAddress;
					}
					if (curInterface == EmPortType.Ethernet)
					{
						port_params = new object[2];
						port_params[0] = settings_.CurrentIPAddress;
						port_params[1] = settings_.CurrentPort;
					}
					if (curInterface == EmPortType.GPRS)
					{
						port_params = new object[3];
						port_params[0] = settings_.CurrentIPAddress;
						port_params[1] = settings_.CurrentPort;
						port_params[2] = settings_.CurDeviceAddress;
					}
					if (curInterface == EmPortType.Rs485)
					{
						port_params = new object[3];
						port_params[0] = settings_.SerialPortName485;
						port_params[1] = settings_.SerialSpeed485;
						port_params[2] = settings_.CurDeviceAddress;
					}
				}
				else    // auto mode
				{
					curInterface = settings_.AutoSettings.AutoIOInterface;
					// if not RS-485 then we have to set the broadcasting address
					if (curInterface != EmPortType.Rs485 && curInterface != EmPortType.Modem &&
						curInterface != EmPortType.GPRS)
					{
						settings_.CurDeviceAddress = 0xFFFF;
						settings_.AutoSettings.AutoDeviceAddress = 0xFFFF;
					}

					if (curInterface == EmPortType.Modem)
					{
						port_params = new object[5];
						port_params[0] = settings_.SerialPortNameModem;
						port_params[1] = settings_.SerialSpeedModem;
						port_params[2] = settings_.AutoSettings.AutoPhoneNumber;
						port_params[3] = settings_.AttemptNumber;
						port_params[4] = settings_.CurDeviceAddress;
					}
					else if (curInterface == EmPortType.Ethernet)
					{
						port_params = new object[2];
						port_params[0] = settings_.AutoSettings.AutoIPAddress;
						port_params[1] = settings_.AutoSettings.AutoPort;
					}
					else if (curInterface == EmPortType.GPRS)
					{
						port_params = new object[3];
						port_params[0] = settings_.AutoSettings.AutoIPAddress;
						port_params[1] = settings_.AutoSettings.AutoPort;
						port_params[2] = settings_.CurDeviceAddress;
					}
					else if (curInterface == EmPortType.Rs485)
					{
						port_params = new object[3];
						port_params[0] = settings_.SerialPortName485;
						port_params[1] = settings_.SerialSpeed485;
						port_params[2] = settings_.AutoSettings.AutoDeviceAddress;
					}
					//settings_.CurDeviceType = EmDeviceType.EM32;
				}

				#region Write debug info

				string debugInfo = DateTime.Now.ToString() + "   create device: EM32 "
								 + curInterface.ToString() + "  ";
				if (!bAutoMode_)
				{
					debugInfo += "{not auto mode} ";
					if (curInterface == EmPortType.COM)
					{
						debugInfo += (settings_.SerialPortName + "  ");
						debugInfo += (settings_.SerialPortSpeed + "  ");
					}
					if (curInterface == EmPortType.Modem)
					{
						debugInfo += (settings_.SerialPortNameModem + "  ");
						debugInfo += (settings_.SerialSpeedModem + "  ");
						debugInfo += (settings_.CurPhoneNumber + "  ");
						debugInfo += (settings_.AttemptNumber + "  ");
					}
					if (curInterface == EmPortType.Ethernet)
					{
						debugInfo += (settings_.CurrentIPAddress + "  ");
						debugInfo += (settings_.CurrentPort + "  ");
					}
					if (curInterface == EmPortType.GPRS)
					{
						debugInfo += (settings_.CurrentIPAddress + "  ");
						debugInfo += (settings_.CurrentPort + "  ");
					}
					if (curInterface == EmPortType.Rs485)
					{
						debugInfo += (settings_.SerialPortName485 + "  ");
						debugInfo += (settings_.SerialSpeed485 + "  ");
						debugInfo += (settings_.CurDeviceAddress + "  ");
					}
				}
				else    // auto mode
				{
					debugInfo += "{auto mode} ";
					if (curInterface == EmPortType.Modem)
					{
						debugInfo += (settings_.SerialPortNameModem + "  ");
						debugInfo += (settings_.SerialSpeedModem + "  ");
						debugInfo += (settings_.AutoSettings.AutoPhoneNumber + "  ");
						debugInfo += (settings_.AttemptNumber + "  ");
					}
					if (curInterface == EmPortType.Ethernet)
					{
						debugInfo += (settings_.AutoSettings.AutoIPAddress + "  ");
						debugInfo += (settings_.AutoSettings.AutoPort + "  ");
					}
					if (curInterface == EmPortType.GPRS)
					{
						debugInfo += (settings_.AutoSettings.AutoIPAddress + "  ");
						debugInfo += (settings_.AutoSettings.AutoPort + "  ");
					}
					if (curInterface == EmPortType.Rs485)
					{
						debugInfo += (settings_.SerialPortName485 + "  ");
						debugInfo += (settings_.SerialSpeed485 + "  ");
						debugInfo += (settings_.AutoSettings.AutoDeviceAddress + "  ");
					}
				}
				EmService.WriteToLogGeneral(debugInfo);

				#endregion

				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
					return false;
				}

				long serial = -1;
				if (bAutoMode_)
				{
					device_ = new Em32Device(curInterface, settings_.AutoSettings.AutoDeviceAddress,
								bAutoMode_, port_params, (sender_ as Form).Handle);
					serial = device_.OpenDevice();
					if (serial == -1)
					{
						throw new EmDisconnectException();
					}
				}
				else
				{
					if (settings_.IOInterface == EmPortType.COM ||
						settings_.IOInterface == EmPortType.Ethernet ||
						settings_.IOInterface == EmPortType.GPRS ||
						settings_.IOInterface == EmPortType.Modem ||
						settings_.IOInterface == EmPortType.Rs485)
					{
						device_ = new Em32Device(curInterface, settings_.CurDeviceAddress,
												bAutoMode_, port_params, (sender_ as Form).Handle);
						serial = device_.OpenDevice();
					}
					else
					{
						throw new EmInvalidInterfaceException();
					}
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
					if (!bAutoMode_)
					{
						MessageBoxes.DeviceIsNotLicenced(sender_, this);
					}
					throw new EmException("Device is not licenced");
				}

				if (!bAutoMode_)
				{
					if (curInterface == EmPortType.Modem)
					{
						if (OnSetSerialNumber != null) OnSetSerialNumber(serial, settings_.CurPhoneNumber,
																 EmPortType.Modem);
					}
					if (curInterface == EmPortType.Ethernet)
					{
						if (OnSetSerialNumber != null) OnSetSerialNumber(serial, settings_.CurrentIPAddress,
																 EmPortType.Ethernet);
					}
					if (curInterface == EmPortType.GPRS)
					{
						if (OnSetSerialNumber != null) OnSetSerialNumber(serial, settings_.CurrentIPAddress,
																 EmPortType.GPRS);
					}
					if (curInterface == EmPortType.Rs485)
					{
						if (OnSetSerialNumber != null)
							OnSetSerialNumber(serial, settings_.CurDeviceAddress.ToString(),
																 EmPortType.Rs485);
					}
				}

				#region Time Synchro

				if (!bAutoMode_)
				{
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
					if (!bAutoMode_ && !e_.Cancel && !bw_.CancellationPending)
					{
						MessageBoxes.ReadDevInfoError(sender_, this, curInterface, port_params);
					}
					throw new EmException("Unable to read device contents");
				}
				if (!device_.IsSomeArchiveExist())
				{
					throw new EmDeviceEmptyException();
				}

				#region Update device data (only for Em32)

				// обновляем сведения об устройстве в базе (если они изменились)
				DeviceCommonInfoEm32 devInfo32 = (device_ as Em32Device).DeviceInfo;

				DbService dbService = new DbService(settings_.PgServers[settings_.CurServerIndex].PgConnectionStringEm32);
				if (!dbService.Open())
				{
					if (!bAutoMode_ && !e_.Cancel && !bw_.CancellationPending)
					{
						MessageBoxes.DbConnectError(sender_, this, dbService.Host,
							dbService.Port, dbService.Database);
					}
					EmService.WriteToLogFailed("ConnectToDevice Em32: db connect error");
					return false;
				}
				try
				{
					// получаем dev_id устройства (если оно есть в базе)
					string commandText = String.Format("SELECT dev_id FROM devices WHERE ser_number = {0};", devInfo32.SerialNumber);
					object oDevId = dbService.ExecuteScalar(commandText);
					long iDevId = -1;
					if (!(oDevId == null) && !(oDevId is DBNull))
					{
						iDevId = (long)oDevId;

						// считываем номер последней записи журнала
						long iNumEvent = -1;
						commandText = String.Format("SELECT max(event_number) FROM event_journal e WHERE device_id = {0};", iDevId);
						object oNumEvent = dbService.ExecuteScalar(commandText);
						if (!(oNumEvent == null) && !(oNumEvent is DBNull))
						{
							iNumEvent = (long)oNumEvent;
						}
						byte[] bufJournal = null;
						// считываем из устройства недостающие записи
						(device_ as Em32Device).ReadLostEventJournalRecords(ref bufJournal, iNumEvent);
						// вставляем эти записи в БД
						InsertJornalRecordsToDB(ref bufJournal, ref dbService, iDevId);

						#region Update other device data (only for Em32)

						// обновляем в БД информацию о приборе
						commandText = String.Format("SELECT * FROM devices WHERE ser_number = {0};", devInfo32.SerialNumber);
						dbService.ExecuteReader(commandText);
						while (dbService.DataReaderRead())
						{
							// схема подключения
							object oRes = dbService.DataReaderData("con_scheme");
							if (!(oRes == null) && !(oRes is DBNull))
							{
								ConnectScheme cRes = (ConnectScheme)(short)oRes;
								if (cRes != devInfo32.ConnectionScheme)
								{
									commandText = String.Format("UPDATE devices SET con_scheme = {0} WHERE ser_number = {1};", 
										devInfo32.ConnectionScheme, devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);
								}
							}

							// версия прошивки
							oRes = dbService.DataReaderData("dev_version");
							if (!(oRes == null) && !(oRes is DBNull))
							{
								string sRes = (string)oRes;
								if (sRes != devInfo32.DevVersion)
								{
									commandText = String.Format("UPDATE devices SET dev_version = '{0}' WHERE ser_number = {1};",
										devInfo32.DevVersion, devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);
								}
							}

							// f_nominal
							oRes = dbService.DataReaderData("f_nom");
							if (!(oRes == null) && !(oRes is DBNull))
							{
								float fRes = (float)oRes;
								if (fRes != devInfo32.F_Nominal)
								{
									commandText = String.Format("UPDATE devices SET f_nom = {0} WHERE ser_number = {1};", 
										devInfo32.F_Nominal.ToString(new CultureInfo("en-US")), devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);
								}
							}

							// u_nominal_phase
							oRes = dbService.DataReaderData("u_nom_ph");
							if (!(oRes == null) && !(oRes is DBNull))
							{
								float fRes = (float)oRes;
								if (fRes != devInfo32.U_NominalPhase)
								{
									commandText = String.Format("UPDATE devices SET u_nom_ph = {0} WHERE ser_number = {1};",
										devInfo32.U_NominalPhase.ToString(new CultureInfo("en-US")), devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);
								}
							}

							// u_nominal_lin
							oRes = dbService.DataReaderData("u_nom_lin");
							if (!(oRes == null) && !(oRes is DBNull))
							{
								float fRes = (float)oRes;
								if (fRes != devInfo32.U_NominalLinear)
								{
									commandText = String.Format("UPDATE devices SET u_nom_lin = {0} WHERE ser_number = {1};",
										devInfo32.U_NominalLinear.ToString(new CultureInfo("en-US")), devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);
								}
							}

							// i_nominal
							oRes = dbService.DataReaderData("i_nom_ph");
							if (!(oRes == null) && !(oRes is DBNull))
							{
								float fRes = (float)oRes;
								if (fRes != devInfo32.I_NominalPhase)
								{
									commandText = String.Format("UPDATE devices SET i_nom_ph = {0} WHERE ser_number = {1};",
										devInfo32.I_NominalPhase.ToString(new CultureInfo("en-US")), devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);
								}
							}

							// тип уставок
							oRes = dbService.DataReaderData("constraint_type");
							if (!(oRes == null) && !(oRes is DBNull))
							{
								short iRes = (short)oRes;
								if (iRes != devInfo32.ConstraintType)
								{
									commandText = String.Format("UPDATE devices SET constraint_type = {0} WHERE ser_number = {1};",
										devInfo32.ConstraintType, devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);
								}
							}

							// имя объекта
							oRes = dbService.DataReaderData("object_name");
							if (!(oRes == null) && !(oRes is DBNull))
							{
								string sRes = (string)oRes;
								if (sRes != devInfo32.ObjectName)
								{
									commandText = String.Format("UPDATE devices SET object_name = '{0}' WHERE ser_number = {1};",
										devInfo32.ObjectName, devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);

									string folder_name = string.Format("{0}  #{1}",
														devInfo32.SerialNumber,
														sRes);
									commandText = String.Format("UPDATE folders SET name = '{0}' WHERE device_id = (SELECT dev_id FROM devices WHERE ser_number = {1}) AND folder_type = 3;", folder_name, devInfo32.SerialNumber);
									dbService.ExecuteNonQuery(commandText, true);
								}
							}

							// режим наибольших нагрузок
							DateTime dtMlStartDateTime1;	// начало режима наибольших нагрузок
							DateTime dtMlEndDateTime1;		// окончание режима наибольших нагрузок
							DateTime dtMlStartDateTime2;	// начало режима наибольших нагрузок 2
							DateTime dtMlEndDateTime2;		// окончание режима наибольших нагрузок 2
							try
							{
								dtMlStartDateTime1 = (DateTime)dbService.DataReaderData("ml_start_time_1");
							}
							catch (InvalidCastException)
							{
								dtMlStartDateTime1 = DateTime.MinValue;
							}
							try
							{
								dtMlEndDateTime1 = (DateTime)dbService.DataReaderData("ml_end_time_1");
							}
							catch (InvalidCastException)
							{
								dtMlEndDateTime1 = DateTime.MinValue;
							}
							try
							{
								dtMlStartDateTime2 = (DateTime)dbService.DataReaderData("ml_start_time_2");
							}
							catch (InvalidCastException)
							{
								dtMlStartDateTime2 = DateTime.MinValue;
							}
							try
							{
								dtMlEndDateTime2 = (DateTime)dbService.DataReaderData("ml_end_time_2");
							}
							catch (InvalidCastException)
							{
								dtMlEndDateTime2 = DateTime.MinValue;
							}

							TimeSpan mlStart1 = new TimeSpan(dtMlStartDateTime1.Hour,
								dtMlStartDateTime1.Minute, dtMlStartDateTime1.Second);
							TimeSpan mlEnd1 = new TimeSpan(dtMlEndDateTime1.Hour,
								dtMlEndDateTime1.Minute, dtMlEndDateTime1.Second);
							TimeSpan mlStart2 = new TimeSpan(dtMlStartDateTime2.Hour,
								dtMlStartDateTime2.Minute, dtMlStartDateTime2.Second);
							TimeSpan mlEnd2 = new TimeSpan(dtMlEndDateTime2.Hour,
								dtMlEndDateTime2.Minute, dtMlEndDateTime2.Second);


							if ((mlStart1 != devInfo32.MlStartTime1) || (mlEnd1 != devInfo32.MlEndTime1) || (mlStart2 != devInfo32.MlStartTime2) || (mlEnd2 != devInfo32.MlEndTime2))
							{
								DateTime mlStart1ToBD = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
									DateTime.Now.Day, devInfo32.MlStartTime1.Hours,
									devInfo32.MlStartTime1.Minutes, devInfo32.MlStartTime1.Seconds);
								DateTime mlEnd1ToBD = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
									DateTime.Now.Day, devInfo32.MlEndTime1.Hours,
									devInfo32.MlEndTime1.Minutes, devInfo32.MlEndTime1.Seconds);
								DateTime mlStart2ToBD = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
									DateTime.Now.Day, devInfo32.MlStartTime2.Hours,
									devInfo32.MlStartTime2.Minutes, devInfo32.MlStartTime2.Seconds);
								DateTime mlEnd2ToBD = new DateTime(DateTime.Now.Year, DateTime.Now.Month,
									DateTime.Now.Day, devInfo32.MlEndTime2.Hours,
									devInfo32.MlEndTime2.Minutes, devInfo32.MlEndTime2.Seconds);

								commandText = String.Format("UPDATE devices SET ml_start_time_1 = '{0}', ml_end_time_1 = '{1}', ml_start_time_2 = '{2}', ml_end_time_2 = '{3}' WHERE ser_number = {4};",
									mlStart1ToBD.ToString("MM.dd.yyyy HH:mm:ss"),
									mlEnd1ToBD.ToString("MM.dd.yyyy HH:mm:ss"),
									mlStart2ToBD.ToString("MM.dd.yyyy HH:mm:ss"),
									mlEnd2ToBD.ToString("MM.dd.yyyy HH:mm:ss"),
									devInfo32.SerialNumber);
								dbService.ExecuteNonQuery(commandText, true);
							}
						}

						#endregion
					}
				}
				catch (Exception ex)
				{
					EmService.DumpException(ex, "Unable to update Device Info: ");
					throw;
				}
				finally
				{
					if (dbService != null) dbService.CloseConnect();
				}

				#endregion

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
				EmService.DumpException(ex, "Error DisconnectDevice(): ");
				return false;
			}
		}

		private void InsertJornalRecordsToDB(ref byte[] buffer, ref DbService dbService, long devId)
		{
			try
			{
				if (buffer == null || buffer.Length == 0) return;

				short recLen = 32;		// длина одной записи
				int cntRecords = buffer.Length / recLen;
				List<JournalEntry> listEvents = new List<JournalEntry>();
				DateTime dt;
				short dateShift = 6;

				for (int i = 0; i < cntRecords; ++i)
				{
					try
					{
						ushort year = Conversions.bytes_2_ushort(ref buffer, 8 + i * recLen + dateShift);
						byte mo = buffer[11 + i * recLen + dateShift];
						byte day = buffer[10 + i * recLen + dateShift];
						byte hour = buffer[13 + i * recLen + dateShift];
						byte min = buffer[12 + i * recLen + dateShift];
						byte sec = buffer[14 + i * recLen + dateShift];

						dt = new DateTime(year, mo, day, hour, min, sec);
					}
					catch
					{
						EmService.WriteToLogFailed("JournalEntry invalid data");
						continue;
					}

					// номер записи
					uint num = Conversions.bytes_2_uint_new(ref buffer, i * recLen + 0);

					short type = Conversions.bytes_2_short(ref buffer, i * recLen + 4);
					//Int64 extra = Conversions.bytes_2_uint(ref buffer, i * recLen + 22);

					ushort crc = Conversions.bytes_2_ushort(ref buffer, i * recLen + 30);
					ushort crc2 = DeviceIO.EmDevice.CalcCrc16(ref buffer, i * recLen, 32);

					if (crc == crc2)
						listEvents.Add(new JournalEntry(num, dt, type));
					else
					{
						EmService.WriteToLogFailed("JournalEntry crc error");
					}
				}

				if (listEvents.Count > 0)
				{
					for (int i = 0; i < listEvents.Count; ++i)
					{
						try
						{
							string commandText = String.Format("INSERT INTO event_journal(device_id, event_number, event_date, event_type, extra_data) VALUES({0}, {1}, '{2}', {3}, {4})",
								devId,
								listEvents[i].EntryNumber,
								listEvents[i].Date.ToString("MM.dd.yyyy HH:mm:ss.fffffff"),
								listEvents[i].EventType,
								0);	//listEvents[i].ExtraData
							dbService.ExecuteNonQuery(commandText, true);
						}
						catch (Exception ex)
						{
							EmService.DumpException(ex, "InsertJornalRecordsToDB() failed: ");
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in InsertJornalRecordsToDB():");
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
				EmService.DumpException(ex, "Error in saver_OnStepReading(): ");
			}
		}

		#endregion

		#region Private XML Image methods

		/// <summary>
		/// Top-level method of forming device image object
		/// </summary>
		/// <param name="devTree">Tree with checked/unchecked nodes</param>
		/// <param name="devImage">Future XML device image</param>
		/// <param name="ListAvgParams">List of AVG params</param>
		/// <param name="MasksAvg">Mask for AVG params</param>
		/// <returns>True if all OK, false if was error while reading data</returns>
		private bool formDeviceImage(
			DeviceTreeView devTree,
			ref Em32XmlDeviceImage devImage)
		{
			try
			{
				DeviceIO.DeviceCommonInfoEm32 devInfo = device_.DeviceInfo;

				devImage.SerialNumber = devInfo.SerialNumber;
				devImage.DevVersion = devInfo.DevVersion;
				devImage.ObjectName = devInfo.ObjectName;
				devImage.ConnectionScheme = devInfo.ConnectionScheme;
				devImage.CurrentTransducerIndex = devInfo.CurrentTransducerIndex;
				devImage.F_Nominal = devInfo.F_Nominal;
				devImage.U_NominalLinear = devInfo.U_NominalLinear;
				devImage.U_NominalPhase = devInfo.U_NominalPhase;
				devImage.I_NominalPhase = devInfo.I_NominalPhase;
				devImage.I_Limit = devInfo.I_Limit;
				devImage.U_Limit = devInfo.U_Limit;
				devImage.T_fliker = devInfo.t_fliker;
				devImage.ConstraintType = devInfo.ConstraintType;
				devImage.MlStartTime1 = devInfo.MlStartTime1;
				devImage.MlEndTime1 = devInfo.MlEndTime1;
				devImage.MlStartTime2 = devInfo.MlStartTime2;
				devImage.MlEndTime2 = devInfo.MlEndTime2;

				if (devTree.Nodes.Count == 0 || devTree.Nodes[0].Nodes.Count == 0)
					return true;
				ObjectTreeNode objNode = (ObjectTreeNode)devTree.Nodes[0].Nodes[0];

                if (!bAutoMode_)
				{
					if (objNode.CheckState == CheckState.Unchecked)
						return true;
				}

				#region StatusBar

				// число архивов для statusbar
				int cnt_archives = 0;
				// считаем число архивов
				foreach (MeasureTypeTreeNode typeNode in objNode.Nodes)
				{
					if (typeNode.CheckState == CheckState.Unchecked) continue;

					switch (typeNode.MeasureType)
					{
						case MeasureType.PQP:
							for (int iPqp = 0; iPqp < typeNode.Nodes.Count; ++iPqp)
							{
								if (/*typeNode.Nodes.Count > i &&*/
									(typeNode.Nodes[iPqp] as CheckTreeNode).CheckState != 
														CheckState.Unchecked)
								{
									cnt_archives++;
								}
							}
							break;
						case MeasureType.DNS:
							for (int iDns = 0; iDns < typeNode.Nodes.Count; ++iDns)
							{
								if (/*typeNode.Nodes.Count > i &&*/
									(typeNode.Nodes[iDns] as CheckTreeNode).CheckState != 
														CheckState.Unchecked)
								{
									cnt_archives++;
								}
							}
							break;
						case MeasureType.AVG:
							for (int iAvg = 0; iAvg < typeNode.Nodes.Count; ++iAvg)
							{
								if (/*typeNode.Nodes.Count > i &&*/
									(typeNode.Nodes[iAvg] as CheckTreeNode).CheckState !=
														CheckState.Unchecked)
								{
									cnt_archives++;
								}
							}
							break;
					}
				}

				#endregion

				if (devTree.Nodes.Count > 0 && devTree.Nodes[0].Nodes.Count > 0)
				{
					EmXmlArchiveEm32 archive = new EmXmlArchiveEm32();

					if (!formArchiveImage(objNode, ref archive, cnt_archives))
						return false;

					if (archive.ArchiveAVG != null ||
						(archive.ArchiveDNS != null && archive.ArchiveDNS.Length > 0) ||
						(archive.ArchivePQP != null && archive.ArchivePQP.Length > 0))
					{
						devImage.ArchiveAVG = archive.ArchiveAVG;
						devImage.ArchiveDNS = archive.ArchiveDNS;
						devImage.ArchivePQP = archive.ArchivePQP;
					}

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}
				}
				return true;
			}
			catch (Exception e)
			{
				EmService.WriteToLogFailed("Error in formDeviceImage():  " + e.Message);
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
			ref EmXmlArchiveEm32 archive,
			int cnt_archives)
		{
			List<EmXmlArchivePart> pqpList = new List<EmXmlArchivePart>();
			List<EmXmlArchivePart> dnsList = new List<EmXmlArchivePart>();
			List<EmXmlArchivePart> avgList = new List<EmXmlArchivePart>();
			// count of archives have been read for statusbar
			int cnt_arch_been_read = 1;
			int iArch = 0;
			int iPqp = 0;
			int curPqpIndex = -1;

			try
			{
				bool success = false;

				foreach (MeasureTypeTreeNode typeNode in objNode.Nodes)
				{
                    if (!bAutoMode_)
					{
						if (typeNode.CheckState == CheckState.Unchecked) continue;
					}

					if (bw_.CancellationPending)
					{
						e_.Cancel = true;
						return false;
					}

					switch (typeNode.MeasureType)
					{
						case MeasureType.PQP:
                            if (!bAutoMode_)
							{
								for (iArch = 0; iArch < device_.DeviceInfo.PqpSet.Length; iArch++)
								{
									if (typeNode.Nodes.Count > iArch &&
										(typeNode.Nodes[iArch] as CheckTreeNode).CheckState 
																		!= CheckState.Unchecked)
									{
										// sending event OnSetCntArchives (X from Y)
										if (OnSetCntArchives != null)
											OnSetCntArchives(cnt_archives, cnt_arch_been_read++);

										EmXmlPQP archivePart = new EmXmlPQP();
										
										curPqpIndex = -1;
										for (iPqp = 0; iPqp < device_.DeviceInfo.PqpSet.Length; ++iPqp)
										{
											if (device_.DeviceInfo.PqpSet[iPqp].PqpStart ==
												(typeNode.Nodes[iArch] as MeasureTreeNode).StartDateTime)
											{
												curPqpIndex = iPqp;
												break;
											}
										}
										if (curPqpIndex == -1) continue;

										archivePart.T_fliker = device_.DeviceInfo.t_fliker;
										success = formArchivePartImagePQP(
											ref device_.DeviceInfo.PqpSet[curPqpIndex], 
											ref archivePart);
										if (success) pqpList.Add(archivePart);

										if (!success && !e_.Cancel && !bw_.CancellationPending && !bAutoMode_)
										{
											EmService.WriteToLogFailed("PQP Archive, start date " +
												device_.DeviceInfo.PqpSet[curPqpIndex].PqpStart.ToString() + 
												", was not read!");

											ResourceManager rm = new ResourceManager("EmDataSaver.emstrings",
												this.GetType().Assembly);
											string msg = string.Format(rm.GetString("pke_reading_failed"),
												device_.DeviceInfo.PqpSet[curPqpIndex].PqpStart.ToString());
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
							}
							else   // автоматический режим
							{
								#region check database
								//List<DateTime> archInDB = new List<DateTime>();
								//NpgsqlConnection conEmDb = new NpgsqlConnection(pgSrvConnectStr);
								//NpgsqlDataReader dataReader;
								//try
								//{
								//	conEmDb.Open();
								//	NpgsqlCommand sqlCommand = new NpgsqlCommand();
								//	sqlCommand.Connection = conEmDb;

								// проверяем какие за какие даты есть архивы, считанные 
								// с ЭТОГО прибора
								//commandText = String.Format("SELECT start_datetime FROM databases WHERE device_id = {0};", serialNumber);
								//commandText = String.Format("SELECT t.start_datetime FROM databases d INNER JOIN day_avg_parameter_times t ON d.device_id = {0} AND d.db_id = t.database_id;", dev.SerialNumber);
								//dbService.ExecuteReader(commandText);

								//while (dbService.DataReaderRead())
								//{
								//	DateTime start_datetime = (DateTime)dbService.DataReaderData(0];
								//	archInDB.Add(start_datetime);
								//}
								//}
								//catch (Exception ex)
								//{
								//    EmService.WriteToLogFailed("Exception in formArchiveImage():\n" + ex.Message);
								//    return false;
								//}
								//finally
								//{
								//    if (dbService != null) dbService.CloseConnect();
								//}

								//for (int index = 0; index < dev.PqpSet.Length; index++)
								//{
								//bool bAlreadyExists = false;
								//foreach (DateTime d in archInDB)
								//{
								//TimeSpan tCur = new TimeSpan(cl.PqpSet[index].PqpStart.Ticks);
								//TimeSpan tdtNow = new TimeSpan(DateTime.Now.Ticks);
								//if (d == dev.PqpSet[index].PqpStart)
								//{
								//bAlreadyExists = true;
								//break;
								//}
								//else
								// проверяем давность архива. Если не будет этой проверки, то в
								// приборе может быть слишком много архивов, которых нет в базе и
								// маловероятно, что большое кол-во архивов удастся скачать без 
								// ошибки...
								//if(tdtNow - tCur > new TimeSpan(7, 0, 0, 0))
								//{
								//	bAlreadyExists = true;
								//	break;
								//}
								//}
								// если архив уже есть в базе, выходим
								//if (bAlreadyExists)
								//	continue;

								// если архива в базе еще нет, загружаем
								#endregion

								// загружаем последний архив
								EmXmlPQP archivePart2 = new EmXmlPQP();
								archivePart2.T_fliker = device_.DeviceInfo.t_fliker;
								success = formArchivePartImagePQP(
									ref device_.DeviceInfo.PqpSet[device_.DeviceInfo.PqpSet.Length - 1],
									ref archivePart2);

								if (success)
								{
									pqpList.Add(archivePart2);
								}
								if (!success)
								{
									// перекладываем несчитанный прибор в конец очереди
									if (device_.PortType == EmPortType.Modem)
									{
										if (autoConnectQueues_ != null)
										{
											if (autoConnectQueues_.curTimeM != null)
												EmService.WriteToLogGeneral("call ReturnItemToQueue " +
													autoConnectQueues_.curTimeM.ItemInfo);
											autoConnectQueues_.ReturnItemToQueue(
													AutoConnect.AutoQueueItemType.GSM_MODEM);
										}
									}
									if (device_.PortType == EmPortType.Ethernet)
									{
										if (autoConnectQueues_ != null)
											autoConnectQueues_.ReturnItemToQueue(
												AutoConnect.AutoQueueItemType.ETHERNET);
									}
									if (device_.PortType == EmPortType.GPRS)
									{
										if (autoConnectQueues_ != null)
											autoConnectQueues_.ReturnItemToQueue(
												AutoConnect.AutoQueueItemType.GPRS);
									}
									if (device_.PortType == EmPortType.Rs485)
									{
										if (autoConnectQueues_ != null)
											autoConnectQueues_.ReturnItemToQueue(
												AutoConnect.AutoQueueItemType.RS485);
									}

									EmService.WriteToLogFailed("Automatic download PQP failed!  \n PQP date:  " + device_.DeviceInfo.PqpSet[device_.DeviceInfo.PqpSet.Length - 1].PqpStart + " - " + device_.DeviceInfo.PqpSet[device_.DeviceInfo.PqpSet.Length - 1].PqpEnd);
									// если нет успешно считанных архивов, возвращаем ошибку
									if (pqpList.Count == 0)
										return false;
									else     // если считанные архивы есть, выходим но без ошибки
									{
										archive.ArchivePQP = new EmXmlPQP[pqpList.Count];
										pqpList.CopyTo(archive.ArchivePQP);
										return true;
									}
								}

								// удаляем считанный архив из класса автозагрузки
								if (device_.PortType == EmPortType.Modem)
								{
									EmService.WriteToLogGeneral("call DeleteItem " +
												autoConnectQueues_.curTimeM.ItemInfo);
									autoConnectQueues_.DeleteItem(AutoConnect.AutoQueueItemType.GSM_MODEM);
								}
								if (device_.PortType == EmPortType.Ethernet)
								{
									EmService.WriteToLogGeneral("call DeleteItem " +
												autoConnectQueues_.curTimeE.ItemInfo);
									autoConnectQueues_.DeleteItem(AutoConnect.AutoQueueItemType.ETHERNET);
								}
								if (device_.PortType == EmPortType.GPRS)
								{
									EmService.WriteToLogGeneral("call DeleteItem " +
												autoConnectQueues_.curTimeGPRS.ItemInfo);
									autoConnectQueues_.DeleteItem(AutoConnect.AutoQueueItemType.GPRS);
								}
								if (device_.PortType == EmPortType.Rs485)
								{
									EmService.WriteToLogGeneral("call DeleteItem " +
												autoConnectQueues_.curTime485.ItemInfo);
									autoConnectQueues_.DeleteItem(AutoConnect.AutoQueueItemType.RS485);
								}
							}
							break;

						case MeasureType.AVG:
                            if (!bAutoMode_)
							{
								// здесь 3 - число типов усреденения: 3 сек, 1 мин, 30 мин
								for (int iAvg = 0; iAvg < typeNode.Nodes.Count; iAvg++) 
								{
									if (typeNode.Nodes.Count > iAvg &&
										(typeNode.Nodes[iAvg] as CheckTreeNode).CheckState !=
												CheckState.Unchecked)
									{
										MeasureTreeNode curNode = typeNode.Nodes[iAvg] as MeasureTreeNode;
										// sending event OnSetCntArchives (X from Y)
										if (OnSetCntArchives != null)
											OnSetCntArchives(cnt_archives, cnt_arch_been_read++);

										DateTime dtStart = DateTime.MinValue, dtEnd = DateTime.MinValue;

										AvgTypes curType = AvgTypes.ThreeSec;
										if (curNode.Text.Contains("1 min")) curType = AvgTypes.OneMin;
										else if (curNode.Text.Contains("30 min")) curType = AvgTypes.ThirtyMin;

										switch (curType)
										{
											case AvgTypes.ThreeSec:
												device_.DeviceInfo.DateStartAvg3sec = curNode.StartDateTime;
												device_.DeviceInfo.DateEndAvg3sec = curNode.EndDateTime;
												dtStart = device_.DeviceInfo.DateStartAvg3sec;
												dtEnd = device_.DeviceInfo.DateEndAvg3sec;
												break;
											case AvgTypes.OneMin:
												device_.DeviceInfo.DateStartAvg1min = curNode.StartDateTime;
												device_.DeviceInfo.DateEndAvg1min = curNode.EndDateTime;
												dtStart = device_.DeviceInfo.DateStartAvg1min;
												dtEnd = device_.DeviceInfo.DateEndAvg1min;
												break;
											case AvgTypes.ThirtyMin:
												device_.DeviceInfo.DateStartAvg30min = curNode.StartDateTime;
												device_.DeviceInfo.DateEndAvg30min = curNode.EndDateTime;
												dtStart = device_.DeviceInfo.DateStartAvg30min;
												dtEnd = device_.DeviceInfo.DateEndAvg30min;
												break;
										}

										// проверяем были ли выбраны параметры. если нет, то берем из формы
										// выбора параметров набор по умолчанию
										if (!curNode.MasksAvgWasSet)
										{
											using (frmAvgParams frmParams = 
												new frmAvgParams(objNode.ConnectionScheme, 
																EmDeviceType.EM32, true))
											{
												curNode.MasksAvg = frmParams.GetMask();
											}
										}

										EmXmlAVG archivePart = new EmXmlAVG();
										success = formArchivePartImageAVG(dtStart, dtEnd,
														ref archivePart, curType,
														curNode.MasksAvg, objNode.ConnectionScheme);
										if (success) avgList.Add(archivePart);
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

                            if (!bAutoMode_)
							{
								if (typeNode.Nodes.Count > /*iDns*/0 &&
									(typeNode.Nodes[0/*iDns*/] as CheckTreeNode).CheckState != 
											CheckState.Unchecked)
								{
									// sending event OnSetCntArchives (X from Y)
									if (OnSetCntArchives != null)
										OnSetCntArchives(cnt_archives, cnt_arch_been_read++);

									EmXmlDNS archivePart = new EmXmlDNS();

									MeasureTreeNode curNode = typeNode.Nodes[0/*iDns*/] as MeasureTreeNode;
									device_.DeviceInfo.DateStartDipSwell = curNode.StartDateTime;
									device_.DeviceInfo.DateEndDipSwell = curNode.EndDateTime;

									success = formArchivePartImageDNS(
												device_.DeviceInfo.DateStartDipSwell,
												device_.DeviceInfo.DateEndDipSwell,
												ref archivePart);

									if (success) dnsList.Add(archivePart);

									if (bw_.CancellationPending)
									{
										e_.Cancel = true;
										return false;
									}
								}
							}
							break;
					}
				}

				if (pqpList.Count > 0)
				{
					archive.ArchivePQP = new EmXmlPQP[pqpList.Count];
					pqpList.CopyTo(archive.ArchivePQP);
				}

				if (dnsList.Count > 0)
				{
					archive.ArchiveDNS = new EmXmlDNS[dnsList.Count];
					dnsList.CopyTo(archive.ArchiveDNS);
				}

				if (avgList.Count > 0)
				{
					archive.ArchiveAVG = new EmXmlAVG[avgList.Count];
					avgList.CopyTo(archive.ArchiveAVG);
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formArchiveImage():");
				EmService.WriteToLogFailed("iArch =  " + iArch.ToString() +
										", curPqpIndex =   " + curPqpIndex.ToString() +
										", iPqp =  " + iPqp.ToString());
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
			ref DeviceIO.PqpSetEm32 pqpSet,
			ref EmXmlPQP archivePart)
		{
			try
			{
				archivePart.Start = pqpSet.PqpStart;
				archivePart.End = pqpSet.PqpEnd;

				ExchangeResult errCode = ExchangeResult.Other_Error;
				byte[] buffer = null;

				try
				{
					errCode = device_.Read(EmCommands.COMMAND_ReadQualityEntry, ref buffer,
										new object[] { pqpSet.Year, pqpSet.Month, pqpSet.Day });
				}
				catch (Exception exc)
				{
					EmService.WriteToLogFailed("Exception in formArchivePartImagePQP():  " 
						+ exc.Message);
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

				archivePart.DataPages = new byte[buffer.Length];
				buffer.CopyTo(archivePart.DataPages, 0);

				// тип уставок
				archivePart.StandardSettingsType = Conversions.bytes_2_ushort(ref buffer, 384);
				if (archivePart.StandardSettingsType < 0 || archivePart.StandardSettingsType > 10)
				{
					EmService.WriteToLogFailed("invalid settings type:  " + 
											archivePart.StandardSettingsType);
					archivePart.StandardSettingsType = 1;
				}

				return (errCode == ExchangeResult.OK);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formArchivePartImagePQP:");
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
			frmAvgParams.MaskAvgArray masksAvg, ConnectScheme conScheme)
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
												conScheme, EmDeviceType.EM32);
				
				try
				{
					if (listAvgParams == null || listAvgParams.Count < 1)
						errCode = device_.ReadAvgArchive(ref buffer, dtStart, dtEnd, avgType);
					else
						errCode = device_.ReadAvgArchiveSelectively(ref buffer, dtStart, dtEnd, avgType,
																	ref listAvgParams);
				}
				catch (Exception exc)
				{
					EmService.WriteToLogFailed("Exception in formArchivePartImageAVG():  " + exc.Message);
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
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formArchivePartImageAVG():");
				if (EmService.ShowWndFeedback)
				{
					frmSentLogs frmLogs = new frmSentLogs();
					frmLogs.ShowDialog();
					EmService.ShowWndFeedback = false;
				}
				throw;
			}
		}

		/// <summary>Method for filling each DNS part of arhive</summary>
		/// <param name="archivePart">Future XML part of archive</param>
		/// <returns>True if all OK or False</returns>
		private bool formArchivePartImageDNS(DateTime dtStart, DateTime dtEnd,
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
					errCode = device_.ReadDipSwellArchive(ref buffer, dtStart, dtEnd);
				}
				catch (Exception exc)
				{
					EmService.WriteToLogFailed(
						"Exception in formArchivePartImageDNS():  " + exc.Message);
				}
				if (errCode != ExchangeResult.OK) return false;
				if (buffer != null)
				{
					archivePart.DataPages = new byte[buffer.Length];
					buffer.CopyTo(archivePart.DataPages, 0);
				}
				if (device_.DeviceInfo.CurDnsExists())
					archivePart.CurrentDNSBuffer = device_.DeviceInfo.BufCurDipSwellData;
				else
					archivePart.CurrentDNSBuffer = null;

				if (buffer == null && archivePart.CurrentDNSBuffer == null)
				{
					EmService.WriteToLogFailed("There was no DNS event during this period: "
						+ dtStart.ToString() + " - " + dtEnd.ToString());
					return false;
				}

				return errCode == ExchangeResult.OK;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in formArchivePartImageDNS():");
				throw;
			}
		}

		#endregion

		#region Properties

		public bool IsDevInfoFull
		{
			get 
			{ 
				return this.device_.DeviceInfo != null && device_.DeviceInfo.ObjectName != null; 
			}
		}

		#endregion
	}

	/// <summary>
	/// Класс, содержащий очереди объектов для автоматического считывания
	/// </summary>
	public class AutoConnect
	{
		#region Fields

		// queue phone
		private Queue<AutoQueueItemData> qTimesToLoadM_ = new Queue<AutoQueueItemData>();
		// текущий элемент (элемент, который является текущим НЕ присутствует в очереди)
		public AutoQueueItemData curTimeM = null;

		// queue ethernet
		private Queue<AutoQueueItemData> qTimesToLoadE_ = new Queue<AutoQueueItemData>();
		public AutoQueueItemData curTimeE = null;

		// queue GPRS
		private Queue<AutoQueueItemData> qTimesToLoadGPRS_ = new Queue<AutoQueueItemData>();
		public AutoQueueItemData curTimeGPRS = null;

		// queue rs-485
		private Queue<AutoQueueItemData> qTimesToLoad485_ = new Queue<AutoQueueItemData>();
		public AutoQueueItemData curTime485 = null;

		object lockModemQueue_ = new object();
		object lockEthernetQueue_ = new object();
		object lockGPRSQueue_ = new object();
		object lockRs485Queue_ = new object();

		public List<EmDataSaver.AutoConnect.AutoQueueItemData> ListDialData = null;
		public List<EmDataSaver.AutoConnect.AutoQueueItemData> ListEthernetData = null;
		public List<EmDataSaver.AutoConnect.AutoQueueItemData> ListGPRSData = null;
		public List<EmDataSaver.AutoConnect.AutoQueueItemData> ListRs485Data = null;

		#endregion

		#region Public Methods

		/// <summary>
		/// Метод выбирает из списка элемент, время которого приблизительно равно текущему времени
		/// и добавляет его в очередь на загрузку
		/// </summary>
		/// <param name="listTimes">Список элементов</param>
		/// <param name="itemType">Тип элемента</param>
		public void FindFitTimes(List<AutoQueueItemData> listTimes, AutoQueueItemType itemType)
		{
			try
			{
				Queue<AutoQueueItemData> queue = GetCurrentQueue(itemType);
				object lockQueue = GetCurrentLockObject(itemType);

				lock (lockQueue)
				{
					TimeSpan tsNow = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
					for (int i = 0; i < listTimes.Count; ++i)
					{
						TimeSpan diff = tsNow - listTimes[i].StartTime;
						if (diff.Duration() < new TimeSpan(0, 1, 0) && tsNow > listTimes[i].StartTime)
						{
							queue.Enqueue(listTimes[i]);
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoConnect::FindFitTimes(): " + ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Метод извлекает элемент из очереди на загрузку (элемент удаляется из очереди)
		/// </summary>
		/// <param name="itemType">Тип элемента</param>
		/// <returns></returns>
		public AutoQueueItemData GetCurItemToLoad(AutoQueueItemType itemType)
		{
			try
			{
				switch (itemType)
				{
					case AutoQueueItemType.GSM_MODEM: return GetCurItemToLoadM();
					case AutoQueueItemType.ETHERNET: return GetCurItemToLoadE();
					case AutoQueueItemType.GPRS: return GetCurItemToLoadGPRS();
					case AutoQueueItemType.RS485: return GetCurItemToLoad485();
					default: throw new EmException("GetCurItemToLoad(): Unknown item type!");
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoConnect::GetCurItemToLoad(): " + ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Метод возвращает элемент в очередь на загрузку
		/// </summary>
		/// <param name="itemType">Тип элемента</param>
		public void ReturnItemToQueue(AutoQueueItemType itemType)
		{
			try
			{
				switch (itemType)
				{
					case AutoQueueItemType.GSM_MODEM: ReturnItemToQueueM(); break;
					case AutoQueueItemType.ETHERNET: ReturnItemToQueueE(); break;
					case AutoQueueItemType.GPRS: ReturnItemToQueueGPRS(); break;
					case AutoQueueItemType.RS485: ReturnItemToQueue485(); break;
					default: throw new EmException("ReturnItemToQueue(): Unknown item type!");
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoConnect::ReturnItemToQueue(): " + ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Метод увеличивает время старта загрузки на заданное число минут и при этом проверяет
		/// чтобы не было совпадения времени с уже существующими элементами
		/// </summary>
		public void IncreaseItemStartTime(AutoQueueItemType itemType, AutoQueueItemData item, int min)
		{
			try
			{
				Queue<AutoQueueItemData> queue = GetCurrentQueue(itemType);
				object lockQueue = GetCurrentLockObject(itemType);
				List<EmDataSaver.AutoConnect.AutoQueueItemData> curList = GetCurrentListTimes(itemType);

				item.StartTime = item.StartTime.Add(new TimeSpan(0, min, 0));

				bool duplicate = false;
				// проверить нет ли в очереди или в списке элемента, у которого время равно 
				// новому времени айтема и если есть то увеличить еще
				do
				{
					duplicate = false;
					for (int iL = 0; iL < curList.Count; ++iL)
					{
						if (curList[iL].StartTime == item.StartTime)
						{
							item.StartTime = item.StartTime.Add(new TimeSpan(0, 20, 0));
							duplicate = true;
						}
					}
				} while (duplicate);

				lock (lockQueue)
				{
					do
					{
						duplicate = false;
						for (int iQ = 0; iQ < queue.Count; ++iQ)
						{
							AutoQueueItemData curItem = queue.Dequeue();
							if ((curItem != item) && (curItem.StartTime == item.StartTime))
							{
								item.StartTime = item.StartTime.Add(new TimeSpan(0, 20, 0));
								duplicate = true;
							}
							queue.Enqueue(curItem);
						}
					} while (duplicate);
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error IncreaseItemStartTime(): " + ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Метод удаляет элемент, извлеченный из очереди на загрузку
		/// </summary>
		/// <param name="itemType"></param>
		public void DeleteItem(AutoQueueItemType itemType)
		{
			try
			{
				switch (itemType)
				{
					case AutoQueueItemType.GSM_MODEM:
						lock (lockModemQueue_) { curTimeM = null; }
						break;
					case AutoQueueItemType.ETHERNET:
						lock (lockEthernetQueue_) { curTimeE = null; }
						break;
					case AutoQueueItemType.GPRS:
						lock (lockGPRSQueue_) { curTimeGPRS = null; }
						break;
					case AutoQueueItemType.RS485:
						lock (lockRs485Queue_) { curTime485 = null; }
						break;
					default: throw new EmException("DeleteItem(): Unknown item type!");
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoConnect::DeleteItem(): " + ex.Message);
				throw;
			}
		}

		#endregion

		#region Private Methods

		private Queue<AutoQueueItemData> GetCurrentQueue(AutoQueueItemType itemType)
		{
			try
			{
				switch (itemType)
				{
					case AutoQueueItemType.GSM_MODEM: return qTimesToLoadM_;
					case AutoQueueItemType.ETHERNET: return qTimesToLoadE_;
					case AutoQueueItemType.GPRS: return qTimesToLoadGPRS_;
					case AutoQueueItemType.RS485: return qTimesToLoad485_;
					default: throw new EmException("GetCurrentQueue(): Unknown item type!");
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoConnect::GetCurrentQueue(): " + ex.Message);
				throw;
			}
		}

		private object GetCurrentLockObject(AutoQueueItemType itemType)
		{
			try
			{
				switch (itemType)
				{
					case AutoQueueItemType.GSM_MODEM: return lockModemQueue_;
					case AutoQueueItemType.ETHERNET: return lockEthernetQueue_;
					case AutoQueueItemType.GPRS: return lockGPRSQueue_;
					case AutoQueueItemType.RS485: return lockRs485Queue_;
					default: throw new EmException("GetCurrentLockObject(): Unknown item type!");
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoConnect::GetCurrentLockObject(): " + ex.Message);
				throw;
			}
		}

		private List<EmDataSaver.AutoConnect.AutoQueueItemData> GetCurrentListTimes(AutoQueueItemType itemType)
		{
			try
			{
				switch (itemType)
				{
					case AutoQueueItemType.GSM_MODEM: return ListDialData;
					case AutoQueueItemType.ETHERNET: return ListEthernetData;
					case AutoQueueItemType.GPRS: return ListGPRSData;
					case AutoQueueItemType.RS485: return ListRs485Data;
					default: throw new EmException("GetCurrentListTimes(): Unknown item type!");
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoConnect::GetCurrentListTimes(): " + ex.Message);
				throw;
			}
		}

		private AutoQueueItemData GetCurItemToLoadM()
		{
			try
			{
				// если item уже считывается, возвращаем null
				if (curTimeM != null && curTimeM.InProcess == true)
					return null;

				lock (lockModemQueue_)
				{
					if (curTimeM == null && qTimesToLoadM_.Count > 0)
						curTimeM = qTimesToLoadM_.Dequeue();
					if (curTimeM == null) return null;
					// если время загрузки еще не пришло, возвращаем в очередь
					if (curTimeM.StartTime > new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0))
					{
						qTimesToLoadM_.Enqueue(curTimeM);
						curTimeM = null;
					}
					return curTimeM;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error GetCurItemToLoadM(): " + ex.Message);
				throw;
			}
		}

		private AutoQueueItemData GetCurItemToLoadE()
		{
			try
			{
				if (curTimeE != null && curTimeE.InProcess == true)
					return null;

				lock (lockEthernetQueue_)
				{
					if (curTimeE == null && qTimesToLoadE_.Count > 0)
						curTimeE = qTimesToLoadE_.Dequeue();
					if (curTimeE == null) return null;
					// если время загрузки еще не пришло, возвращаем в очередь
					if (curTimeE.StartTime > new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0))
					{
						qTimesToLoadE_.Enqueue(curTimeE);
						curTimeE = null;
					}
					return curTimeE;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error GetCurItemToLoadE(): " + ex.Message);
				throw;
			}
		}

		private AutoQueueItemData GetCurItemToLoadGPRS()
		{
			try
			{
				if (curTimeGPRS != null && curTimeGPRS.InProcess == true)
					return null;

				lock (lockGPRSQueue_)
				{
					if (curTimeGPRS == null && qTimesToLoadGPRS_.Count > 0)
						curTimeGPRS = qTimesToLoadGPRS_.Dequeue();
					if (curTimeGPRS == null) return null;
					// если время загрузки еще не пришло, возвращаем в очередь
					if (curTimeGPRS.StartTime > new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0))
					{
						qTimesToLoadE_.Enqueue(curTimeGPRS);
						curTimeGPRS = null;
					}
					return curTimeGPRS;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error GetCurItemToLoadGPRS(): " + ex.Message);
				throw;
			}
		}

		private AutoQueueItemData GetCurItemToLoad485()
		{
			try
			{
				if (curTime485 != null && curTime485.InProcess == true)
					return null;

				lock (lockRs485Queue_)
				{
					if (curTime485 == null && qTimesToLoad485_.Count > 0)
						curTime485 = qTimesToLoad485_.Dequeue();
					if (curTime485 == null) return null;
					// если время загрузки еще не пришло, возвращаем в очередь
					if (curTime485.StartTime > new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0))
					{
						qTimesToLoad485_.Enqueue(curTime485);
						curTime485 = null;
					}
					return curTime485;
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error GetCurItemToLoad485(): " + ex.Message);
				throw;
			}
		}

		private void ReturnItemToQueueM()
		{
			try
			{
				lock (lockModemQueue_)
				{
					if (curTimeM != null)
					{
						curTimeM.InProcess = false;
						qTimesToLoadM_.Enqueue(curTimeM);
						curTimeM = null;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error ReturnItemToQueueM(): " + ex.Message);
				throw;
			}
		}

		private void ReturnItemToQueueE()
		{
			try
			{
				lock (lockEthernetQueue_)
				{
					if (curTimeE != null)
					{
						curTimeE.InProcess = false;
						qTimesToLoadE_.Enqueue(curTimeE);
						curTimeE = null;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error ReturnItemToQueueE(): " + ex.Message);
				throw;
			}
		}

		private void ReturnItemToQueueGPRS()
		{
			try
			{
				lock (lockGPRSQueue_)
				{
					if (curTimeGPRS != null)
					{
						curTimeGPRS.InProcess = false;
						qTimesToLoadGPRS_.Enqueue(curTimeGPRS);
						curTimeGPRS = null;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error ReturnItemToQueueGPRS(): " + ex.Message);
				throw;
			}
		}

		private void ReturnItemToQueue485()
		{
			try
			{
				lock (lockRs485Queue_)
				{
					if (curTime485 != null)
					{
						curTime485.InProcess = false;
						qTimesToLoad485_.Enqueue(curTime485);
						curTime485 = null;
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error ReturnItemToQueue485(): " + ex.Message);
				throw;
			}
		}

		#endregion

		public class AutoQueueItemData
		{
			public AutoQueueItemType ItemType;
			public TimeSpan StartTime;
			public DateTime dtStartTime = DateTime.MinValue;
			public bool AutoDial = false;

			// for modem
			public string PhoneNumber = "";
			public int Attempts = 3;

			// for ethernet or GPRS
			public string IPAddress = "0.0.0.0";
			public string Port = "";

			//совпадает с серийным номером
			public ushort DevAddress = 0;

			// флаг показывает, что item уже считывается, чтобы повторно не запускалось его считывание
			// при последующих срабатываниях таймера
			public bool InProcess = false;

			public string Comment = "";
			public string DevType = "";
			//public string SerialNumber = "";
			public bool Active = false;

			public string ItemInfo
			{
				get
				{
					return StartTime.ToString() + " | " + PhoneNumber + " | " + InProcess.ToString();
				}
			}

			/// <summary>
			/// Constructor for modem items
			/// </summary>
			/// <param name="p">Phone number</param>
			/// <param name="t">Time</param>
			/// <param name="a">AutoDial</param>
			/// <param name="att">Attempts number</param>
			/// <param name="itemType">Item type (modem)</param>
			public AutoQueueItemData(string p, ushort address, TimeSpan t, bool a, int att, AutoQueueItemType itemType)
			{
				PhoneNumber = p;
				StartTime = t;
				AutoDial = a;
				Attempts = att;
				ItemType = itemType;
				InProcess = false;
				DevAddress = address;
			}

			/// <summary>
			/// Constructor for RS485 items
			/// </summary>
			/// <param name="addr">Device address</param>
			/// <param name="t">Time</param>
			/// <param name="a">AutoDial</param>
			/// <param name="itemType">Item type (RS485)</param>
			public AutoQueueItemData(ushort addr, TimeSpan t, bool a, AutoQueueItemType itemType)
			{
				DevAddress = addr;
				StartTime = t;
				AutoDial = a;
				ItemType = itemType;
				InProcess = false;
			}

			/// <summary>
			/// Constructor for Ethernet items
			/// </summary>
			/// <param name="addr">IP Address</param>
			/// <param name="p">Port number</param>
			/// <param name="t">Time</param>
			/// <param name="a">AutoDial</param>
			/// <param name="itemType">Item type (Ethernet or GPRS)</param>
			public AutoQueueItemData(string addr, string p, TimeSpan t, bool a, AutoQueueItemType itemType)
			{
				IPAddress = addr;
				Port = p;
				StartTime = t;
				AutoDial = a;
				ItemType = itemType;
				InProcess = false;
			}

			/// <summary>
			/// Constructor for GPRS items
			/// </summary>
			/// <param name="addr">IP Address</param>
			/// <param name="p">Port number</param>
			/// <param name="t">Time</param>
			/// <param name="a">AutoDial</param>
			/// <param name="itemType">Item type (Ethernet or GPRS)</param>
			public AutoQueueItemData(string addr, ushort address, string p, TimeSpan t, bool a, AutoQueueItemType itemType)
			{
				IPAddress = addr;
				Port = p;
				StartTime = t;
				AutoDial = a;
				ItemType = itemType;
				InProcess = false;
				DevAddress = address;
			}

			public AutoQueueItemData()
			{ }
		}

		public enum AutoQueueItemType
		{
			GSM_MODEM = 0,
			ETHERNET = 1,
			RS485 = 2,
			GPRS = 3
		}
	}
}
