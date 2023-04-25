using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EmServiceLib;

namespace EmArchiveTree
{
	/// <summary>
	/// TreeNode that contains database information
	/// </summary>
	public class EmTreeNodeObject : EmArchNodeBase
	{
		#region Fields

		/// <summary>Folder Identifier</summary>
		public Int64 FolderId = 0;
		/// <summary>Object Identifier</summary>
		public Int64 ObjectId = 0;
		/// <summary>Object Identifier (was read from the device)</summary>
		public Int64 GlobalObjectId = 0;
		///<summary>Time of start measures</summary>
		private DateTime startDateTime_ = DateTime.MinValue;
		///<summary>Time of end measures</summary>
		private DateTime endDateTime_ = DateTime.MinValue;
		///<summary>Linear nominal voltage</summary>
		public float NominalLinearVoltage = 0;
		///<summary>Phase nominal voltage</summary>
		public float NominalPhaseVoltage = 0;
		///<summary>Phase nominal current</summary>
		public float NominalPhaseCurrent = 0;
		///<summary>Nominal Frequency</summary>
		public float NominalFrequency = 0;
		///<summary>Object name</summary>
		public string ObjectName = String.Empty;
		///<summary>Object info</summary>
		public string ObjectInfo = String.Empty;
		///<summary>Voltage limit</summary>
		public float ULimit = 0;
		///<summary>Current limit</summary>
		public float ILimit = 0;
		///<summary>Current transducer index</summary>
		public short CurrentTransducerIndex = 0;
		///<summary>Constraint type</summary>
		public short ConstraintType = 0;
		///<summary>Device serial number</summary>
		public Int64 DeviceID = 0;
		///<summary>Device name</summary>
		public string DeviceName = String.Empty;
		/// <summary>Device microcode version</summary>
		public string DeviceVersion = String.Empty;
		/// <summary>Fliker period</summary>
		public short T_Fliker = 10;
		///<summary>начало режима наибольших нагрузок</summary>
		public DateTime MlStartDateTime1;
		///<summary>окончание режима наибольших нагрузок</summary>
		public DateTime MlEndDateTime1;
		///<summary>начало режима наибольших нагрузок</summary>
		public DateTime MlStartDateTime2;
		///<summary>окончание режима наибольших нагрузок</summary>
		public DateTime MlEndDateTime2;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor without parameters for Clone() method
		/// </summary>
		public EmTreeNodeObject()
			: base(EmTreeNodeType.Object, EmDeviceType.NONE, null, null)
		{
			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
		}

		public EmTreeNodeObject(EmArchNodeBase parentDev, EmDeviceType devType)
			: base(EmTreeNodeType.Object, devType, parentDev, null)
		{
			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
		}

		/// <summary>
		/// Constructor with parameters for common using
		/// </summary>
		public EmTreeNodeObject(
			DateTime StartDateTime,
			DateTime EndDateTime,
			ConnectScheme ConnectionScheme,
			float NominalLinearVoltage,
			float NominalPhaseVoltage,
			float NominalFrequency,
			string ObjName,
			string ObjInfo,
			float ULimit,
			float ILimit,
			float CurrentTransducerIndex,
			short t_fliker,
			Int64 DeviceID,
			string DeviceName,
			string DeviceVersion,
			Int64 ObjID,
			EmDeviceType devType,
			EmArchNodeBase parentDev)
			: base(EmTreeNodeType.Object, devType, parentDev, null)
		{
			this.Text = StartDateTime.ToString() + " - " + EndDateTime.ToString();
			this.ObjectId = ObjID;
			this.StartDateTime = StartDateTime;
			this.EndDateTime = EndDateTime;
			this.connectionScheme_ = ConnectionScheme;

			this.NominalLinearVoltage = NominalLinearVoltage;
			this.NominalPhaseVoltage = NominalPhaseVoltage;
			this.NominalFrequency = NominalFrequency;
			this.T_Fliker = t_fliker;

			this.ObjectName = ObjName;
			this.ObjectInfo = ObjInfo;

			this.DeviceID = DeviceID;
			this.DeviceName = DeviceName;
			this.DeviceVersion = DeviceVersion;

			// setting images
			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method Clone()
		/// </summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeObject)node).ObjectId = this.ObjectId;
			((EmTreeNodeObject)node).GlobalObjectId = this.GlobalObjectId;
			((EmTreeNodeObject)node).ConnectionScheme = this.ConnectionScheme;
			((EmTreeNodeObject)node).ConstraintType = this.ConstraintType;
			((EmTreeNodeObject)node).CurrentTransducerIndex = this.CurrentTransducerIndex;
			((EmTreeNodeObject)node).StartDateTime = this.StartDateTime;
			((EmTreeNodeObject)node).EndDateTime = this.EndDateTime;
			((EmTreeNodeObject)node).MlStartDateTime1 = this.MlStartDateTime1;
			((EmTreeNodeObject)node).MlStartDateTime2 = this.MlStartDateTime2;
			((EmTreeNodeObject)node).MlEndDateTime1 = this.MlEndDateTime1;
			((EmTreeNodeObject)node).MlEndDateTime2 = this.MlEndDateTime2;
			((EmTreeNodeObject)node).NominalLinearVoltage = this.NominalLinearVoltage;
			((EmTreeNodeObject)node).NominalPhaseVoltage = this.NominalPhaseVoltage;
			((EmTreeNodeObject)node).NominalFrequency = this.NominalFrequency;
			((EmTreeNodeObject)node).NominalPhaseCurrent = this.NominalPhaseCurrent;
			((EmTreeNodeObject)node).ILimit = this.ILimit;
			((EmTreeNodeObject)node).ULimit = this.ULimit;
			((EmTreeNodeObject)node).T_Fliker = this.T_Fliker;
			((EmTreeNodeObject)node).ObjectName = this.ObjectName;
			((EmTreeNodeObject)node).ObjectInfo = this.ObjectInfo;
			((EmTreeNodeObject)node).DeviceID = this.DeviceID;
			((EmTreeNodeObject)node).DeviceType = this.DeviceType;
			((EmTreeNodeObject)node).DeviceName = this.DeviceName;
			((EmTreeNodeObject)node).DeviceVersion = this.DeviceVersion;
			((EmTreeNodeObject)node).FolderId = this.FolderId;
			return node;
		}

		#endregion

		#region Properties

		public DateTime StartDateTime
		{
			get { return startDateTime_; }
			set { startDateTime_ = value; }
		}

		public DateTime EndDateTime
		{
			get { return endDateTime_; }
			set { endDateTime_ = value; }
		}

		#endregion
	}

	/// <summary>
	/// TreeNode that contains database information
	/// </summary>
	public class EmTreeNodeRegistration : EmArchNodeBase
	{
		#region Fields

		/// <summary>Folder Identifier</summary>
		public Int64 FolderId = 0;
		/// <summary>Object Identifier</summary>
		public Int64 RegistrationId = 0;
		///<summary>Time of start measures</summary>
		private DateTime startDateTime_ = DateTime.MinValue;
		///<summary>Time of end measures</summary>
		private DateTime endDateTime_ = DateTime.MinValue;
		///<summary>Linear nominal voltage</summary>
		public float NominalLinearVoltage = 0;
		///<summary>Phase nominal voltage</summary>
		public float NominalPhaseVoltage = 0;
		///<summary>Phase nominal current</summary>
		public float NominalPhaseCurrent = 0;
		///<summary>Nominal Frequency</summary>
		public float NominalFrequency = 0;
		///<summary>Object name</summary>
		public string ObjectName = String.Empty;
		///<summary>Object info</summary>
		public string ObjectInfo = String.Empty;
		///<summary>Voltage limit</summary>
		public float ULimit = 0;
		///<summary>Current limit</summary>
		public float ILimit = 0;
		///<summary>Constraint type</summary>
		public short ConstraintType = 0;
		///<summary>Device id</summary>
		public Int64 DeviceID = 0;
		///<summary>Device serial number</summary>
		public Int64 DeviceSerNumber = 0;
		///<summary>Device name</summary>
		public string DeviceName = String.Empty;
		/// <summary>Device microcode version</summary>
		public string DeviceVersion = String.Empty;
		/// <summary>Fliker period for EtPQP-A (always = 10)</summary>
		public short T_Fliker = 10;
		public bool U_transformer_enable = false;
		public short U_transformer_type = 0;
		public short I_transformer_usage = 0;
		public int I_transformer_primary = 1;
		public short I_transformer_secondary = 1;

		public double GPS_Latitude = 0;
		public double GPS_Longitude = 0;
		public bool Autocorrect_time_gps_enable;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor without parameters for Clone() method
		/// </summary>
		public EmTreeNodeRegistration()
			: base(EmTreeNodeType.Registration, EmDeviceType.ETPQP_A, null, null)
		{
			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
		}

		public EmTreeNodeRegistration(EmArchNodeBase parentDev, EmDeviceType devType)
			: base(EmTreeNodeType.Registration, devType, parentDev, null)
		{
			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
		}

		/// <summary>
		/// Constructor with parameters for common using
		/// </summary>
		public EmTreeNodeRegistration(
			DateTime StartDateTime,
			DateTime EndDateTime,
			ConnectScheme ConnectionScheme,
			float NominalLinearVoltage,
			float NominalPhaseVoltage,
			float NominalFrequency,
			string ObjName,
			string ObjInfo,
			float ULimit,
			float ILimit,
			double GPSLatitude, double GPSLongitude,
			//short t_fliker,
			Int64 DeviceID,
			string DeviceName,
			string DeviceVersion,
			Int64 RegID,
			EmDeviceType devType,
			EmArchNodeBase parentDev)
			: base(EmTreeNodeType.Registration, devType, parentDev, null)
		{
			this.Text = StartDateTime.ToString() + " - " + EndDateTime.ToString();
			this.RegistrationId = RegID;
			this.StartDateTime = StartDateTime;
			this.EndDateTime = EndDateTime;
			this.connectionScheme_ = ConnectionScheme;

			this.NominalLinearVoltage = NominalLinearVoltage;
			this.NominalPhaseVoltage = NominalPhaseVoltage;
			this.NominalFrequency = NominalFrequency;
			//this.T_Fliker = t_fliker;

			this.ObjectName = ObjName;
			this.ObjectInfo = ObjInfo;

			this.DeviceID = DeviceID;
			this.DeviceName = DeviceName;
			this.DeviceVersion = DeviceVersion;

			// setting images
			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;

			this.GPS_Latitude = GPSLatitude;
			this.GPS_Longitude = GPSLongitude;
		}

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method Clone()
		/// </summary>
		/// <returns>Same as the base method Clone()</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeRegistration)node).RegistrationId = this.RegistrationId;
			//((EmTreeNodeRegistration)node).GlobalObjectId = this.GlobalObjectId;
			((EmTreeNodeRegistration)node).ConnectionScheme = this.ConnectionScheme;
			((EmTreeNodeRegistration)node).ConstraintType = this.ConstraintType;
			//((EmTreeNodeRegistration)node).CurrentTransducerIndex = this.CurrentTransducerIndex;
			((EmTreeNodeRegistration)node).StartDateTime = this.StartDateTime;
			((EmTreeNodeRegistration)node).EndDateTime = this.EndDateTime;
			((EmTreeNodeRegistration)node).NominalLinearVoltage = this.NominalLinearVoltage;
			((EmTreeNodeRegistration)node).NominalPhaseVoltage = this.NominalPhaseVoltage;
			((EmTreeNodeRegistration)node).NominalFrequency = this.NominalFrequency;
			((EmTreeNodeRegistration)node).NominalPhaseCurrent = this.NominalPhaseCurrent;
			((EmTreeNodeRegistration)node).ILimit = this.ILimit;
			((EmTreeNodeRegistration)node).ULimit = this.ULimit;
			//((EmTreeNodeRegistration)node).T_Fliker = this.T_Fliker;
			((EmTreeNodeRegistration)node).ObjectName = this.ObjectName;
			((EmTreeNodeRegistration)node).ObjectInfo = this.ObjectInfo;
			((EmTreeNodeRegistration)node).DeviceID = this.DeviceID;
			((EmTreeNodeRegistration)node).DeviceType = this.DeviceType;
			((EmTreeNodeRegistration)node).DeviceName = this.DeviceName;
			((EmTreeNodeRegistration)node).DeviceVersion = this.DeviceVersion;
			((EmTreeNodeRegistration)node).FolderId = this.FolderId;
			return node;
		}

		#endregion

		#region Properties

		public DateTime StartDateTime
		{
			get { return startDateTime_; }
			set { startDateTime_ = value; }
		}

		public DateTime EndDateTime
		{
			get { return endDateTime_; }
			set { endDateTime_ = value; }
		}

		public DateTime DevVersionDateTime
		{
			get
			{
				if (DeviceVersion.Length < 6) //??????????????????????? new dip mode
					return DateTime.MinValue;

				return new DateTime(
						Int32.Parse(DeviceVersion.Substring(DeviceVersion.Length - 6, 2)) + 2000,
						Int32.Parse(DeviceVersion.Substring(DeviceVersion.Length - 4, 2)),
						Int32.Parse(DeviceVersion.Substring(DeviceVersion.Length - 2, 2)));
			}
		}

		#endregion
	}
}
