using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.IO;
using System.Resources;
using System.Globalization;

using EmServiceLib;
using DeviceIO;
using EmDataSaver.SqlImage;

namespace EmDataSaver
{
	public abstract class EmDataExportBase
	{
		#region Fields

		protected object sender_;
		protected Settings settings_;
		protected string pgSrvConnectStr_;
		protected string pgHost_;
		protected EmDeviceType devType_;

		protected EmSqlDataNodeType[] parts_;
		protected Int64 device_id_to_export_;
        // object or registration
		protected Int64 object_reg_id_to_export_;
		protected long archive_id_;
		protected long device_id_;

		protected string sqlImageFileName_;

		#endregion

		#region Properties

		public string SqlImageFileName
		{
			get { return sqlImageFileName_; }
		}

		public EmDeviceType DeviceType
		{
			get { return devType_; }
		}

		#endregion

		protected string GetNumericFieldAsString(object value)
		{
			try
			{
				string res = "null";
				if (!(value is DBNull))
				{
					try
					{
						int i = int.Parse(value.ToString());
						res = i.ToString();
					}
					catch
					{
						try
						{
							float f = (float)value;
							res = f.ToString(new CultureInfo("en-US"));
						}
						catch
						{
							EmService.WriteToLogFailed("GetNumericFieldAsString: both casts failed!");
							EmService.WriteToLogFailed(value.ToString());
						}
					}
				}
				return res;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in GetNumericFieldAsString():");
				return "null";
			}
		}
	}
}
