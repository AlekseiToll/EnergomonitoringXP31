using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BytesRoad.Net.Ftp;

namespace FTPClient
{
	public class FTP
	{
		//Сам клиент ФТП
		private FtpClient client_ = new FtpClient();

		private int timeoutFTP_;
		private string ftpServer_;     		//ФТП сервер
		private int ftpPort_ = 21;			//порт для передачи данных
		private string ftpLogin_;			//логин на ФТП
		private string ftpPassword_;		//пароль на ФТП

		public bool IsServerConnect { get; set; }

		public FTP(int timeoutFTP, string ftpServer, int ftpPort, string ftpLogin, string ftpPassword)
		{
			client_.PassiveMode = true;      //Включаем пассивный режим ФТП

			timeoutFTP_ = timeoutFTP;
			ftpServer_ = ftpServer;
			ftpPort_ = ftpPort;
			ftpLogin_ = ftpLogin;
			ftpPassword_ = ftpPassword;
		}

		public FTP(int timeoutFTP, string ftpServer, int ftpPort, string ftpLogin, string ftpPassword, string serverProxy, int portProxy, int proxyMode)
		{
			client_.PassiveMode = true;      //Включаем пассивный режим ФТП

			timeoutFTP_ = timeoutFTP;
			ftpServer_ = ftpServer;
			ftpPort_ = ftpPort;
			ftpLogin_ = ftpLogin;
			ftpPassword_ = ftpPassword;

			FtpProxyInfo pinfo = new FtpProxyInfo();
			pinfo.Server = serverProxy;
			pinfo.Port = portProxy;

			switch (proxyMode)
			{
				case 0:
					pinfo.Type = FtpProxyType.HttpConnect;
					break;
				case 1:
					pinfo.Type = FtpProxyType.Socks4;
					break;
				case 2:
					pinfo.Type = FtpProxyType.Socks4a;
					break;
				case 3:
					pinfo.Type = FtpProxyType.Socks5;
					break;
				default: pinfo.Type = FtpProxyType.HttpConnect;
					break;
			}

			pinfo.PreAuthenticate = false;

			//Присваиваем параметры прокси клиенту.
			client_.ProxyInfo = pinfo;
		}

		public FTP(int timeoutFTP, string ftpServer, int ftpPort, string ftpLogin, string ftpPassword, string serverProxy, int portProxy, int proxyMode, string proxyLogin, string proxyPassword)
		{
			client_.PassiveMode = true;      //Включаем пассивный режим ФТП

			timeoutFTP_ = timeoutFTP;
			ftpServer_ = ftpServer;
			ftpPort_ = ftpPort;
			ftpLogin_ = ftpLogin;
			ftpPassword_ = ftpPassword;

			FtpProxyInfo pinfo = new FtpProxyInfo();
			pinfo.Server = serverProxy;
			pinfo.Port = portProxy;

			switch (proxyMode)
			{
				case 0:
					pinfo.Type = FtpProxyType.HttpConnect;
					break;
				case 1:
					pinfo.Type = FtpProxyType.Socks4;
					break;
				case 2:
					pinfo.Type = FtpProxyType.Socks4a;
					break;
				case 3:
					pinfo.Type = FtpProxyType.Socks5;
					break;
				default: pinfo.Type = FtpProxyType.HttpConnect;
					break;
			}

			pinfo.PreAuthenticate = true;
			pinfo.User = proxyLogin;
			pinfo.Password = proxyPassword;

			//Присваиваем параметры прокси клиенту.
			client_.ProxyInfo = pinfo;
		}

		public string connect()
		{
			try
			{
				client_.Connect(timeoutFTP_, ftpServer_, ftpPort_);
				client_.Login(timeoutFTP_, ftpLogin_, ftpPassword_);
				IsServerConnect = true;
				return "Successfully connected to " + ftpServer_;
			}
			catch (Exception ex)
			{
				IsServerConnect = false;
				return ex.Message;
			}
		}

		public string disconnect()
		{
			try
			{
				client_.Disconnect(timeoutFTP_);
				IsServerConnect = false;
				return "Successfylly disconnected";
			}
			catch (Exception ex)
			{
				IsServerConnect = true;
				return ex.Message;
			}
		}

		public FtpItem[] getFileList()
		{
			try
			{
				return client_.GetDirectoryList(timeoutFTP_);
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public FtpItem[] getFileList(string folder)
		{
			try
			{
				return client_.GetDirectoryList(timeoutFTP_, folder);
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public string downloadFile(string srcPath, string savePath)
		{
			try
			{
				client_.GetFile(timeoutFTP_, savePath, srcPath);
				return "File successfully downloaded at location:\n" + savePath;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		//public string uploadFile(string srcPath, string savePath)
		//{
		//    try
		//    {
		//        client_.PutFile(timeoutFTP_, savePath, srcPath);
		//        return "File successfully uploaded at location:\n" + savePath;
		//    }
		//    catch (Exception ex)
		//    {
		//        return ex.Message;
		//    }
		//}

		public static string toFtpString(string str)
		{
			return str.Replace("FTP", "").Replace("\\\\", "\\").Replace("\\", "//");
		}

		//public void DeleteFile(string path)
		//{
		//    client_.DeleteFile(timeoutFTP_, path);
		//}

		public string getFileInfo(string path)
		{
			FtpItem item = client_.GetDirectoryList(timeoutFTP_, path)[0];
			return item.Name + "~" + item.Size + "~" + item.Date.ToShortDateString();
		}
	}
}
