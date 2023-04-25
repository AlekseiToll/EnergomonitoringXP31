using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using EmServiceLib;
using BytesRoad.Net.Ftp;

namespace FTPClient
{
	public class FTPMarsClient
	{
		private const string ftp_server_ = "mars-energo.ru";
		private const string ftp_login_ = "ftp_marsen_1";
		private const string ftp_password_ = "4kjfZH3N4YG";
		private const string ftp_path_firmware_ = "FTP\\EMWorkNet\\ВПО";
		private const string ftp_path_software_ = "FTP\\EMWorkNet";

		private FTP ftp_;

		public DateTime FtpGetFirmwareVersion()
		{
			ftp_ = new FTP(10000 /*Timeout*/, ftp_server_, 21 /*Port*/, ftp_login_, ftp_password_);
			if (ftp_ == null)
			{
				EmService.WriteToLogFailed("Unable to create FTP!");
				return DateTime.MinValue;
			}
			try
			{
				string res = ftp_.connect();
				FtpItem[] items = ftp_.getFileList(FTP.toFtpString(ftp_path_firmware_));
				string firmware_name = string.Empty;
				for (int iItem = 0; iItem < items.Length; ++iItem)
				{
					if (items[iItem].Name.Contains("PqpaFirmware") && items[iItem].ItemType == FtpItemType.File)
					{
						firmware_name = items[iItem].Name;
						break;
					}
				}
				EmService.WriteToLogDebug("FTP: firmware_name = " + firmware_name);

				FileStream fs = new FileStream(firmware_name, FileMode.Open);
				fs.Position = 0x200000 + 25;
				StreamReader sr = new StreamReader(fs);
				char[] buffer = new char[64];
				sr.Read(buffer, 0, 64);
				string tmp = new string(buffer);
				EmService.WriteToLogDebug("FTP: firmware_string = " + tmp);

				int pos = tmp.IndexOf('_', 0);
				int pos2 = tmp.IndexOf('_', pos + 1);
				if (pos < 0 || pos2 < 0)
				{
					EmService.WriteToLogFailed("FTP: Invalid format firmware name!");
					return DateTime.MinValue;
				}
				tmp = tmp.Substring(pos + 1, pos2 - pos - 1);
				return DateTime.Parse(tmp);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "FTP firmware Error: ");
				return DateTime.MinValue;
			}
			finally
			{
				if (ftp_ != null) ftp_.disconnect();
			}
		}

		public short FtpGetSoftwareVersion()
		{
			ftp_ = new FTP(10000 /*Timeout*/, ftp_server_, 21 /*Port*/, ftp_login_, ftp_password_);
			if (ftp_ == null)
			{
				EmService.WriteToLogFailed("Unable to create FTP!");
				return -1;
			}
			try
			{
				string res = ftp_.connect();
				FtpItem[] items = ftp_.getFileList(FTP.toFtpString(ftp_path_software_));
				List<short> versions = new List<short>();
				for (int iItem = 0; iItem < items.Length; ++iItem)
				{
					if (items[iItem].Name.Contains("EmWorkNet") && items[iItem].ItemType == FtpItemType.Directory)
					{
						EmService.WriteToLogDebug("FTP: software_name = " + items[iItem].Name);
						short ver;
						if (Int16.TryParse(items[iItem].Name.Substring(items[iItem].Name.Length - 3), out ver))
							versions.Add(ver);
					}
				}
				if (versions.Count < 1)
				{
					EmService.WriteToLogDebug("FTP: version list is empty");
					return -1;
				}

				versions.Sort();
				return versions[versions.Count - 1];
			}
			catch (Exception ex)
			{
				// здесь не надо DumpException!
				EmService.WriteToLogFailed("FTP software Error: " + ex.Message);
				return -1;
			}
			finally
			{
				if (ftp_ != null) ftp_.disconnect();
			}
		}
	}
}
