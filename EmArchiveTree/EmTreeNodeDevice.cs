using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EmServiceLib;

namespace EmArchiveTree
{
	public class EmTreeNodeEm32Device : EmArchNodeBase
	{
		#region Fields

		/// <summary>Device Identifier</summary>
		public Int64 DeviceId;
		/// <summary>Device Folder Identifier</summary>
		public Int64 FolderId;
		///<summary>Linear nominal voltage</summary>
		public float NominalLinearVoltage;
		///<summary>Phase nominal voltage</summary>
		public float NominalPhaseVoltage;
		///<summary>Nominal Current</summary>
		public float NominalPhaseCurrent;
		///<summary>Nominal Frequency</summary>
		public float NominalFrequency;
		///<summary>Voltage limit</summary>
		public float ULimit;
		///<summary>Current limit</summary>
		public float ILimit;
		///<summary>Current transducer index</summary>
		public ushort CurrentTransducerIndex;
		///<summary>Device serial number</summary>
		private Int64 serialNumber_;
		/// <summary>Device microcode version</summary>
		public string DeviceVersion;
		/// <summary>Device information</summary>
		public string DeviceInfo;
		///<summary>Object name</summary>
		public string ObjectName;
		///<summary>Flikker period</summary>
		public short T_fliker;
		///<summary>Constraint type</summary>
		public short ConstraintType;
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
		public EmTreeNodeEm32Device()
			: base(EmTreeNodeType.EM32Device, EmDeviceType.EM32, null, null)
		{
			this.Text = String.Empty;
			this.DeviceId = 0;
			this.connectionScheme_ = 0;
			this.NominalLinearVoltage = 0;
			this.NominalPhaseVoltage = 0;
			this.NominalPhaseCurrent = 0;
			this.NominalFrequency = 0;

			this.ULimit = 0;
			this.ILimit = 0;
			this.CurrentTransducerIndex = 0;
			this.ConstraintType = 0;

			this.SerialNumber = 0;
			this.DeviceVersion = String.Empty;
			this.DeviceInfo = String.Empty;
			this.ObjectName = String.Empty;

			this.T_fliker = 0;

			this.MlStartDateTime1 = DateTime.MinValue;
			this.MlEndDateTime1 = DateTime.MinValue;
			this.MlStartDateTime2 = DateTime.MinValue;
			this.MlEndDateTime2 = DateTime.MinValue;

			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
		}

		/// <summary>
		/// Constructor with parameters for common using
		/// </summary>
		public EmTreeNodeEm32Device(
			Int64 DevId,
			Int64 FldId,
			ConnectScheme ConnectionScheme,
			float NominalLinearVoltage,
			float NominalPhaseVoltage,
			float NominalPhaseCurrent,
			float NominalFrequency,
			float ULimit,
			float ILimit,
			ushort CurrentTransducerIndex,
			Int64 SerNumber,
			string DeviceVersion,
			string DeviceInfo,
			string ObjName,
			short T_fliker,
			short ConstrType,
			DateTime MlStartDateTime1,
			DateTime MlEndDateTime1,
			DateTime MlStartDateTime2,
			DateTime MlEndDateTime2)
			: base(EmTreeNodeType.EM32Device, EmDeviceType.EM32, null, null)
		{
			this.Text = SerNumber.ToString() + "  " + ObjName;
			this.ObjectName = ObjName;
			this.DeviceId = DevId;
			this.FolderId = FldId;
			this.connectionScheme_ = ConnectionScheme;

			this.NominalLinearVoltage = NominalLinearVoltage;
			this.NominalPhaseVoltage = NominalPhaseVoltage;
			this.NominalPhaseCurrent = NominalPhaseCurrent;
			this.NominalFrequency = NominalFrequency;
			this.ULimit = ULimit;
			this.ILimit = ILimit;
			this.CurrentTransducerIndex = CurrentTransducerIndex;
			this.ConstraintType = ConstrType;

			this.SerialNumber = SerNumber;
			this.DeviceVersion = DeviceVersion;
			this.DeviceInfo = DeviceInfo;
			this.T_fliker = T_fliker;
			this.MlStartDateTime1 = MlStartDateTime1;
			this.MlEndDateTime1 = MlEndDateTime1;
			this.MlStartDateTime2 = MlStartDateTime2;
			this.MlEndDateTime2 = MlEndDateTime2;

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
			((EmTreeNodeEm32Device)node).DeviceId = this.DeviceId;
			((EmTreeNodeEm32Device)node).FolderId = this.FolderId;
			((EmTreeNodeEm32Device)node).ConnectionScheme = this.ConnectionScheme;
			((EmTreeNodeEm32Device)node).ObjectName = this.ObjectName;
			((EmTreeNodeEm32Device)node).NominalLinearVoltage = this.NominalLinearVoltage;
			((EmTreeNodeEm32Device)node).NominalPhaseVoltage = this.NominalPhaseVoltage;
			((EmTreeNodeEm32Device)node).NominalPhaseCurrent = this.NominalPhaseCurrent;
			((EmTreeNodeEm32Device)node).NominalLinearVoltage = this.NominalLinearVoltage;
			((EmTreeNodeEm32Device)node).ULimit = this.ULimit;
			((EmTreeNodeEm32Device)node).ILimit = this.ILimit;
			((EmTreeNodeEm32Device)node).CurrentTransducerIndex = this.CurrentTransducerIndex;
			((EmTreeNodeEm32Device)node).SerialNumber = this.SerialNumber;
			((EmTreeNodeEm32Device)node).DeviceVersion = this.DeviceVersion;
			((EmTreeNodeEm32Device)node).DeviceInfo = this.DeviceInfo;
			((EmTreeNodeEm32Device)node).T_fliker = this.T_fliker;
			((EmTreeNodeEm32Device)node).ConstraintType = this.ConstraintType;
			((EmTreeNodeEm32Device)node).MlStartDateTime1 = this.MlStartDateTime1;
			((EmTreeNodeEm32Device)node).MlEndDateTime1 = this.MlEndDateTime1;
			((EmTreeNodeEm32Device)node).MlStartDateTime2 = this.MlStartDateTime2;
			((EmTreeNodeEm32Device)node).MlEndDateTime2 = this.MlEndDateTime2;
			return node;
		}

		#endregion

		#region Properties

		public Int64 SerialNumber
		{
			get { return serialNumber_; }
			set { serialNumber_ = value; }
		}

		#endregion
	}

	public class EmTreeNodeEtPQPDevice : EmArchNodeBase
	{
		#region Fields

		/// <summary>Device Identifier</summary>
		public Int64 DeviceId;
		/// <summary>Device Folder Identifier</summary>
		public Int64 FolderId;
		/// <summary>Serial Number</summary>
		private Int64 serialNumber_;
		/// <summary>Device microcode version</summary>
		public string DeviceVersion;
		/// <summary>Device information</summary>
		public string DeviceInfo;
		///<summary>Flikker period</summary>
		public short T_fliker;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor without parameters for Clone() method
		/// </summary>
		public EmTreeNodeEtPQPDevice()
			: base(EmTreeNodeType.ETPQPDevice, EmDeviceType.ETPQP, null, null)
		{
			this.Text = String.Empty;
			this.DeviceId = 0;
			this.SerialNumber = 0;
			this.DeviceVersion = String.Empty;
			this.DeviceInfo = String.Empty;
			this.T_fliker = 0;

			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
		}

		/// <summary>
		/// Constructor with parameters for common using
		/// </summary>
		public EmTreeNodeEtPQPDevice(
			Int64 DevId,
			Int64 FldId,
			Int64 SerNumber,
			string DevVersion,
			string DevInfo,
			short T_fliker)
			: base(EmTreeNodeType.ETPQPDevice, EmDeviceType.ETPQP, null, null)
		{
			this.Text = "ETPQP # " + SerNumber.ToString();
			this.DeviceId = DevId;
			this.FolderId = FldId;
			this.SerialNumber = SerNumber;
			this.DeviceVersion = DevVersion;
			this.DeviceInfo = DevInfo;
			this.T_fliker = T_fliker;

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
			((EmTreeNodeEtPQPDevice)node).DeviceId = this.DeviceId;
			((EmTreeNodeEtPQPDevice)node).FolderId = this.FolderId;
			((EmTreeNodeEtPQPDevice)node).SerialNumber = this.SerialNumber;
			((EmTreeNodeEtPQPDevice)node).DeviceVersion = this.DeviceVersion;
			((EmTreeNodeEtPQPDevice)node).DeviceInfo = this.DeviceInfo;
			((EmTreeNodeEtPQPDevice)node).T_fliker = this.T_fliker;
			return node;
		}

		#endregion

		#region Properties

		public Int64 SerialNumber
		{
			get { return serialNumber_; }
			set { serialNumber_ = value; }
		}

		#endregion
	}

	public class EmTreeNodeEtPQP_A_Device : EmArchNodeBase
	{
		#region Fields

		/// <summary>Device Identifier</summary>
		public Int64 DeviceId;
		/// <summary>Device Folder Identifier</summary>
		public Int64 FolderId;
		/// <summary>Serial Number</summary>
		private Int64 serialNumber_;
		/// <summary>Device microcode version</summary>
		public string DeviceVersion;
		/// <summary>Device information</summary>
		public string DeviceInfo;
		///<summary>Flikker period</summary>
		public short T_fliker;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor without parameters for Clone() method
		/// </summary>
		public EmTreeNodeEtPQP_A_Device()
			: base(EmTreeNodeType.ETPQP_A_Device, EmDeviceType.ETPQP_A, null, null)
		{
			this.Text = String.Empty;
			this.DeviceId = 0;
			this.SerialNumber = 0;
			this.DeviceVersion = String.Empty;
			this.DeviceInfo = String.Empty;
			this.T_fliker = 0;

			this.ImageIndex = 2;
			this.SelectedImageIndex = 2;
		}

		/// <summary>
		/// Constructor with parameters for common using
		/// </summary>
		public EmTreeNodeEtPQP_A_Device(
			Int64 DevId,
			Int64 FldId,
			Int64 SerNumber,
			string DevVersion,
			string DevInfo,
			short T_fliker)
			: base(EmTreeNodeType.ETPQP_A_Device, EmDeviceType.ETPQP_A, null, null)
		{
			this.Text = "ETPQP-A # " + SerNumber.ToString();
			this.DeviceId = DevId;
			this.FolderId = FldId;
			this.SerialNumber = SerNumber;
			this.DeviceVersion = DevVersion;
			this.DeviceInfo = DevInfo;
			this.T_fliker = T_fliker;

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
			((EmTreeNodeEtPQP_A_Device)node).DeviceId = this.DeviceId;
			((EmTreeNodeEtPQP_A_Device)node).FolderId = this.FolderId;
			((EmTreeNodeEtPQP_A_Device)node).SerialNumber = this.SerialNumber;
			((EmTreeNodeEtPQP_A_Device)node).DeviceVersion = this.DeviceVersion;
			((EmTreeNodeEtPQP_A_Device)node).DeviceInfo = this.DeviceInfo;
			((EmTreeNodeEtPQP_A_Device)node).T_fliker = this.T_fliker;
			return node;
		}

		#endregion

		#region Properties

		public Int64 SerialNumber
		{
			get { return serialNumber_; }
			set { serialNumber_ = value; }
		}

		#endregion
	}
}
