using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using EmServiceLib;
using EmDataSaver;

namespace EnergomonitoringXP
{
	class AutoDialThread
	{
		public StatusChecker statusChecker;

		public delegate void TimerDialHandler(string phone, int attempts);
		public event TimerDialHandler OnTimerDial;
		public delegate void TimerEthernetHandler(string address, string port);
		public event TimerEthernetHandler OnTimerEthernet;
		public delegate void TimerGPRSHandler(string address, string port);
		public event TimerGPRSHandler OnTimerGPRS;
		public delegate void TimerRs485Handler(ushort devAddress);
		public event TimerRs485Handler OnTimerRs485;

		// ссылка на настройки главного окна
		Settings settings_;
		// ссылка на очередь главного окна
		AutoConnect autoConnectQueues_;
		// ссылка на главное окно
		frmMain frmMain_;

		public AutoDialThread(Settings s, AutoConnect autoConnectQueues, frmMain frm)
		{
			settings_ = s;
			autoConnectQueues_ = autoConnectQueues;
			frmMain_ = frm;
		}

		public void Run()
		{
			try
			{
				AutoResetEvent autoEvent = new AutoResetEvent(false);
				statusChecker = new StatusChecker(this, settings_, autoConnectQueues_, frmMain_);

				TimerCallback timerDelegate =
					new TimerCallback(statusChecker.CheckStatus);

				Timer stateTimer =
						new Timer(timerDelegate, autoEvent, 1000, 60000);	// 1 min

				autoEvent.WaitOne(Timeout.Infinite, false);

				// When autoEvent signals, dispose of the timer.
				stateTimer.Dispose();
				System.Diagnostics.Debug.WriteLine("timer destroyed");
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoDialThread::Run(): " + ex.Message);
			}
		}

		public void CallDialEventM(string phone, int attempts)
		{
			try
			{
				if (OnTimerDial != null) OnTimerDial(phone, attempts);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoDialThread::CallDialEventM(): " + ex.Message);
			}
		}
		public void CallDialEventE(string address, string port)
		{
			try
			{
				if (OnTimerEthernet != null) OnTimerEthernet(address, port);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoDialThread::CallDialEventE(): " + ex.Message);
			}
		}
		public void CallDialEventGPRS(string address, string port)
		{
			try
			{
				if (OnTimerGPRS != null) OnTimerGPRS(address, port);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoDialThread::CallDialEventGPRS(): " + ex.Message);
			}
		}
		public void CallDialEvent485(ushort address)
		{
			try
			{
				if (OnTimerRs485 != null) OnTimerRs485(address);
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error AutoDialThread::CallDialEvent485(): " + ex.Message);
			}
		}
	}

	class StatusChecker
	{
		public bool bStop = false;
		Settings settings_;
		AutoDialThread owner_;
		AutoConnect autoConnectQueues_;
		frmMain frmMain_;

		public StatusChecker(AutoDialThread th, Settings s, AutoConnect queues, frmMain frm)
		{
			owner_ = th;
			settings_ = s;
			autoConnectQueues_ = queues;
			frmMain_ = frm;
		}

		// This method is called by the timer delegate.
		public void CheckStatus(Object stateInfo)
		{
			try
			{
				// если в это время идет опрос приборов в неавтоматич. режиме, то выходим
				if (!frmMain_.AutoMode) return;
				
				AutoResetEvent autoEvent = (AutoResetEvent)stateInfo;

				// получаем список времен из файла
				frmSettings wnd = new frmSettings(settings_, null);
				wnd.OpenDevicesInfo();
				autoConnectQueues_.ListDialData = wnd.AllAutoDialData;
				autoConnectQueues_.ListEthernetData = wnd.AllAutoEthernetData;
				autoConnectQueues_.ListGPRSData = wnd.AllAutoGPRSData;
				autoConnectQueues_.ListRs485Data = wnd.AllAuto485Data;

				if (autoConnectQueues_.ListDialData.Count < 1 &&
					autoConnectQueues_.ListEthernetData.Count < 1 &&
					autoConnectQueues_.ListRs485Data.Count < 1 &&
					autoConnectQueues_.ListGPRSData.Count < 1)
					return;

				// modem
				if (autoConnectQueues_.ListDialData.Count > 0)
				{
					autoConnectQueues_.FindFitTimes(autoConnectQueues_.ListDialData, 
						AutoConnect.AutoQueueItemType.GSM_MODEM);

					if (frmMain_.AutoMode)
					{
						// проверяем нет ли какого-нибудь устаревшего текущего элемента
						if (autoConnectQueues_.curTimeM != null)
						{
							TimeSpan tsNow = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute,
														DateTime.Now.Second);
							TimeSpan diff = tsNow - autoConnectQueues_.curTimeM.StartTime;
							// если прошло больше 8 мин, а элемент остался текущим, то видимо при чтении
							// была какая-то ошибка, поэтому возвращаем элемент в очередь
							if (diff.Duration() > new TimeSpan(0, 8, 0))
							{
								EmService.WriteToLogGeneral("diff.Duration() > new TimeSpan(0, 8, 0)");
								EmService.WriteToLogGeneral("old time: " + 
										autoConnectQueues_.curTimeM.ItemInfo);
								autoConnectQueues_.IncreaseItemStartTime(AutoConnect.AutoQueueItemType.GSM_MODEM,
									autoConnectQueues_.curTimeM, 20);
								EmService.WriteToLogGeneral("new time: " +
										autoConnectQueues_.curTimeM.ItemInfo);
								autoConnectQueues_.ReturnItemToQueue(
												AutoConnect.AutoQueueItemType.GSM_MODEM);
							}
						}

						EmDataSaver.AutoConnect.AutoQueueItemData itemToLoad =
								autoConnectQueues_.GetCurItemToLoad(AutoConnect.AutoQueueItemType.GSM_MODEM);
						if (itemToLoad != null && itemToLoad.InProcess != true)
						{
							EmService.WriteToLogGeneral("modem auto call");
							EmService.WriteToLogGeneral(DateTime.Now.ToString() + "  call item: " +
										autoConnectQueues_.curTimeM.ItemInfo);
							autoConnectQueues_.curTimeM.InProcess = true;
							owner_.CallDialEventM(itemToLoad.PhoneNumber, itemToLoad.Attempts);
						}

						if (bStop)
						{
							// Reset the counter
							autoEvent.Set();
						}
					}
				}

				// ethernet
				if (autoConnectQueues_.ListEthernetData.Count > 0)
				{
					autoConnectQueues_.FindFitTimes(autoConnectQueues_.ListEthernetData, 
						AutoConnect.AutoQueueItemType.ETHERNET);

					if (frmMain_.AutoMode)
					{
						// проверяем нет ли какого-нибудь устаревшего текущего элемента
						if (autoConnectQueues_.curTimeE != null)
						{
							TimeSpan tsNow = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute,
														DateTime.Now.Second);
							TimeSpan diff = tsNow - autoConnectQueues_.curTimeE.StartTime;
							// если прошло больше 8 мин, а элемент остался текущим, то видимо при чтении
							// была какая-то ошибка, поэтому возвращаем элемент в очередь
							if (diff.Duration() > new TimeSpan(0, 8, 0))
							{
								autoConnectQueues_.IncreaseItemStartTime(AutoConnect.AutoQueueItemType.ETHERNET,
												autoConnectQueues_.curTimeE, 20);
								autoConnectQueues_.ReturnItemToQueue(
												AutoConnect.AutoQueueItemType.ETHERNET);
							}
						}

						EmDataSaver.AutoConnect.AutoQueueItemData itemToLoad =
							autoConnectQueues_.GetCurItemToLoad(AutoConnect.AutoQueueItemType.ETHERNET);
						if (itemToLoad != null && itemToLoad.InProcess != true)
						{
							EmService.WriteToLogGeneral("ethernet auto call");
							autoConnectQueues_.curTimeE.InProcess = true;
							owner_.CallDialEventE(itemToLoad.IPAddress, itemToLoad.Port);
						}

						if (bStop)
						{
							// Reset the counter
							autoEvent.Set();
						}
					}
				}

				// GPRS
				if (autoConnectQueues_.ListGPRSData.Count > 0)
				{
					autoConnectQueues_.FindFitTimes(autoConnectQueues_.ListGPRSData,
						AutoConnect.AutoQueueItemType.GPRS);

					if (frmMain_.AutoMode)
					{
						// проверяем нет ли какого-нибудь устаревшего текущего элемента
						if (autoConnectQueues_.curTimeGPRS != null)
						{
							TimeSpan tsNow = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute,
														DateTime.Now.Second);
							TimeSpan diff = tsNow - autoConnectQueues_.curTimeGPRS.StartTime;
							// если прошло больше 8 мин, а элемент остался текущим, то видимо при чтении
							// была какая-то ошибка, поэтому возвращаем элемент в очередь
							if (diff.Duration() > new TimeSpan(0, 8, 0))
							{
								autoConnectQueues_.IncreaseItemStartTime(AutoConnect.AutoQueueItemType.GPRS,
												autoConnectQueues_.curTimeGPRS, 20);
								autoConnectQueues_.ReturnItemToQueue(
												AutoConnect.AutoQueueItemType.GPRS);
							}
						}

						EmDataSaver.AutoConnect.AutoQueueItemData itemToLoad =
							autoConnectQueues_.GetCurItemToLoad(AutoConnect.AutoQueueItemType.GPRS);
						if (itemToLoad != null && itemToLoad.InProcess != true)
						{
							EmService.WriteToLogGeneral("GPRS auto call");
							autoConnectQueues_.curTimeGPRS.InProcess = true;
							owner_.CallDialEventGPRS(itemToLoad.IPAddress, itemToLoad.Port);
						}

						if (bStop)
						{
							// Reset the counter
							autoEvent.Set();
						}
					}
				}

				// RS 485
				if (autoConnectQueues_.ListRs485Data.Count > 0)
				{
					autoConnectQueues_.FindFitTimes(autoConnectQueues_.ListRs485Data, 
						AutoConnect.AutoQueueItemType.RS485);

					if (frmMain_.AutoMode)
					{
						// проверяем нет ли какого-нибудь устаревшего текущего элемента
						if (autoConnectQueues_.curTime485 != null)
						{
							TimeSpan tsNow = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute,
														DateTime.Now.Second);
							TimeSpan diff = tsNow - autoConnectQueues_.curTime485.StartTime;
							// если прошло больше 8 мин, а элемент остался текущим, то видимо при чтении
							// была какая-то ошибка, поэтому возвращаем элемент в очередь
							if (diff.Duration() > new TimeSpan(0, 8, 0))
							{
								autoConnectQueues_.IncreaseItemStartTime(AutoConnect.AutoQueueItemType.RS485,
											autoConnectQueues_.curTime485, 20);
								autoConnectQueues_.ReturnItemToQueue(
												AutoConnect.AutoQueueItemType.RS485);
							}
						}

						EmDataSaver.AutoConnect.AutoQueueItemData itemToLoad =
							autoConnectQueues_.GetCurItemToLoad(AutoConnect.AutoQueueItemType.RS485);
						if (itemToLoad != null && itemToLoad.InProcess != true)
						{
							EmService.WriteToLogGeneral("485 auto call");
							autoConnectQueues_.curTime485.InProcess = true;
							owner_.CallDialEvent485(itemToLoad.DevAddress);
						}

						if (bStop)
						{
							// Reset the counter
							autoEvent.Set();
						}
					}
				}
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error StatusChecker::CheckStatus(): " + ex.Message);
			}
		}
	}
}
