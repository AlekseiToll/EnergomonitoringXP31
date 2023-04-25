using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Globalization;

using Microsoft.Win32;

using EmDataSaver.SavingInterface;
using EmServiceLib;

namespace EmDataSaver
{
	public abstract class EmDataReaderBase
	{
		#region Fields

		protected IntPtr hMainWnd_;

		protected object sender_;
		protected Settings settings_;
		protected BackgroundWorker bw_ = null;
		protected DoWorkEventArgs e_;

		// количество страниц, которые надо читать (инфа для ProgressBar)
		protected double cnt_pages_to_read_ = 0.0;		// 100%
		protected double reader_cur_percent_ = 0;		// current percent
		protected int reader_prev_percent_ = 0;
		protected double reader_percent_for_one_step_ = 0;

		protected bool bCreateImageOnly_ = false;
		protected string sqlImageFileName_;

		protected bool debugMode_ = false;
		protected bool bAutoMode_ = false;

		#endregion

		#region Properties

		public bool DEBUG_MODE_FLAG
		{
			get { return debugMode_; }
		}

		public string SqlImageFileName
		{
			get { return sqlImageFileName_; }
		}

		public bool CreateImageOnly
		{
			get 
			{
				return bCreateImageOnly_;
			}
		}

		#endregion

		#region Constructor

		public EmDataReaderBase(IntPtr hMainWnd)
		{
			hMainWnd_ = hMainWnd;
		}

		#endregion

		#region Main methods

		/// <summary>
		/// Main saving function
		/// Start reading process
		/// </summary>
		public void Run(ref DoWorkEventArgs e)
		{
			try
			{
				//if (settings_.CurrentLanguage == "ru")
				//{
				//    Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");
				//    Thread.CurrentThread.CurrentUICulture = new CultureInfo("ru-RU");
				//}
				//else
				//{
				//    Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				//    Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
				//}

				e_ = e;
				e.Result = ReadDataFromDevice();
			}
			catch (EmException emx)
			{
				EmService.WriteToLogFailed("Error in ExDataReaderXX::Run(): " + emx.Message);
				e.Result = false;
				return;
			}
			catch (EmInvalidInterfaceException)
			{
				MessageBoxes.InvalidInterface(sender_, this, settings_.IOInterface,
								settings_.CurDeviceType.ToString());
				e.Cancel = true;
			}
			catch (EmDeviceEmptyException)
			{
				if (!e_.Cancel && !bw_.CancellationPending)
				{
					if (!bAutoMode_)
						MessageBoxes.DeviceHasNoData(sender_ as Form, this);
					e.Cancel = true;
				}
			}
			catch (EmDisconnectException)
			{
				if (!e_.Cancel && !bw_.CancellationPending)
				{
					if (!bAutoMode_)
						MessageBoxes.DeviceConnectionError(sender_ as Form, this, settings_.IOInterface,
								settings_.IOParameters);
					e.Cancel = true;
				}
				Kernel32.PostMessage(hMainWnd_, EmService.WM_USER + 3, 0, 0);
			}
			catch (EmDeviceOldVersionException)
			{
				if (!e_.Cancel && !bw_.CancellationPending)
				{
					if (!bAutoMode_)
						MessageBoxes.DeviceOldVersion(sender_ as Form, this);
					e.Cancel = true;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExDataReaderXX::Run():");
				e.Result = false;
				throw;
			}
			finally
			{
				if (bw_.CancellationPending)
				{
					e_.Cancel = true;
				}
			}
		}

		public abstract bool ReadDataFromDevice();

		public abstract void SetCancelReading();

		#endregion

		#region Protected Methods

		// проверяем соответствует ли время на компе или приборе, если разница больше минуты,
		// выдаем предупреждение
		protected void DoTimeSynchronizationSLIP(ref byte[] bufTime, DateTime curCompDateTime)
		{
			try
			{
				// сначала проверяем включена ли на компе синхронизация с инетом
				settings_.LoadSettings();

				if (!settings_.DontWarnAutoSynchroTimeDisabled)
				{
					const string keyName = @"HKEY_LOCAL_MACHINE\SYSTEM\ControlSet001\services\W32Time\Parameters";
					object obj = Registry.GetValue(keyName, "Type", null);
					if (obj.ToString().Equals("NoSync"))
					{
						frmSynchroTimeWarning frmTime = new frmSynchroTimeWarning();
						frmTime.ShowDialog();
						if (settings_.DontWarnAutoSynchroTimeDisabled != !frmTime.ShowThisMessage)
						{
							settings_.DontWarnAutoSynchroTimeDisabled = !frmTime.ShowThisMessage;
							settings_.SaveSettings();
						}
					}
				}
				// then compare times
				DateTime curDeviceDateTime = Conversions.bytes_2_DateTimeSLIP2(ref bufTime, 0);
				TimeSpan diff = curCompDateTime - curDeviceDateTime;
				if (diff > new TimeSpan(0, 5, 0) || diff < new TimeSpan(0, -5, 0))
				{
					MessageBoxes.NotCorrectTime(sender_ as Form, this);
				}
			}
			catch (Exception timeEx)
			{
				EmService.DumpException(timeEx, "ReaderBase: Time synchronization error:");
			}
		}

		protected bool DeviceLicenceCheck(long serial)
		{
			try
			{
				if (!settings_.Licences.IsLicenced(serial))
				{
					EmService.WriteToLogFailed("not licenced");
					return false;
				}
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceLicenceCheck(): ");
				return false;
			}
			return true;
		}

		protected bool ReaderReportProgress(double step)
		{
			try
			{
				reader_cur_percent_ += reader_percent_for_one_step_ * step;
				int diff = (int)reader_cur_percent_ - reader_prev_percent_;
				if (diff > 0 && bw_ != null)
					bw_.ReportProgress(diff);

				reader_prev_percent_ = (int)reader_cur_percent_;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ReaderReportProgress(): ");
				throw;
			}
			return true;
		}

		#endregion
	}
}
