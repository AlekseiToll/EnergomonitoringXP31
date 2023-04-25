using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Resources;

using EmServiceLib;

namespace EmArchiveTree
{
	/// <summary>
	/// TreeNode that contains information of which measure types are present in database
	/// </summary>
	public class EmTreeNodeDBMeasureType : EmArchNodeBase
	{
		#region Fields

		/// <summary>
		/// Readonly (to set use constructor parameter). Type of the measures
		/// </summary>
		private MeasureType measureType_;

		#endregion

		#region Constructors

		public EmTreeNodeDBMeasureType(MeasureType measureType, EmDeviceType devType,
										EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(EmTreeNodeType.MeasureGroup, devType, parentDev, parentObj)
		{
			this.measureType_ = measureType;

			ResourceManager rm = new ResourceManager("EmArchiveTree.emstrings", this.GetType().Assembly);

			switch (this.measureType_)
			{
				case MeasureType.PQP:
					this.Text = rm.GetString("name_measure_type_pke_full");
					break;
				case MeasureType.AVG:
					this.Text = rm.GetString("name_measure_type_avg_full");
					break;
				case MeasureType.DNS:
					if (devType != EmDeviceType.ETPQP_A) this.Text = rm.GetString("name_measure_type_dns_full");
					else this.Text = rm.GetString("name_measure_type_events_full");
					break;
			}
		}

		/// <summary>
		/// Constructor without parameters. For Clone() method ONLY!!!
		/// </summary>
		public EmTreeNodeDBMeasureType() :
			base(EmTreeNodeType.MeasureGroup, EmDeviceType.NONE, null, null)
		{ }

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method
		/// </summary>
		/// <returns>Identical copy of this instance</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeDBMeasureType)node).measureType_ = this.measureType_;
			return node;
		}

		#endregion

		#region Methods

		/// <summary>
		/// Get text
		/// </summary>
		public static string GetText(MeasureType measureType, object sender, EmDeviceType devType)
		{
			try
			{
				ResourceManager rm = new ResourceManager("EmArchiveTree.emstrings",
					System.Reflection.Assembly.GetExecutingAssembly());
					//this.GetType().Assembly);
				string text = String.Empty;

				switch (measureType)
				{
					case MeasureType.PQP:
						text = rm.GetString("name_measure_type_pke_full");
						break;
					case MeasureType.AVG:
						text = rm.GetString("name_measure_type_avg_full");
						break;
					case MeasureType.DNS:
						if(devType != EmDeviceType.ETPQP_A) text = rm.GetString("name_measure_type_dns_full");
						else text = rm.GetString("name_measure_type_events_full");
						break;
				}
				return text;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Exception in EmTreeNodeDBMeasureType::GetText():");
				throw;
			}
		}

		#endregion

		#region Properties

		/// <summary>   
		/// Read only type of the measures
		/// </summary>
		public MeasureType MeasureType
		{
			get { return this.measureType_; }
		}

		#endregion
	}

	/// <summary>
	/// TreeNode that contains information of which measure types are present in database (NOT for EtPQP-A)
	/// </summary>
	public class EmTreeNodeAvgTypeClassic : EmArchNodeBase
	{
		#region Fields

		/// <summary>Readonly. Type of the measures</summary>
		private MeasureType measureType_ = MeasureType.AVG;
		private AvgTypes avgType_;

		#endregion

		#region Constructors

		public EmTreeNodeAvgTypeClassic(AvgTypes avgType, EmDeviceType devType,
									EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(EmTreeNodeType.AvgGroup, devType, parentDev, parentObj)
		{
			avgType_ = avgType;

			ResourceManager rm = new ResourceManager("EmArchiveTree.emstrings", this.GetType().Assembly);
			this.Text = rm.GetString(EmService.GetAvgTypeStringForRM(avgType_));
		}

		public EmTreeNodeAvgTypeClassic(short iAvgType, EmDeviceType devType,
								EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(EmTreeNodeType.AvgGroup, devType, parentDev, parentObj)
		{
			ResourceManager rm = new ResourceManager("EmArchiveTree.emstrings", this.GetType().Assembly);

			switch (iAvgType)
			{
				case ((short)AvgTypes.ThreeSec): avgType_ = AvgTypes.ThreeSec; break;
				case ((short)AvgTypes.OneMin): avgType_ = AvgTypes.OneMin; break;
				case ((short)AvgTypes.ThirtyMin): avgType_ = AvgTypes.ThirtyMin; break;
				default:
					EmService.WriteToLogFailed("EmTreeNodeAvgType(): Invalid Avg Type!  "
						+ iAvgType.ToString());
					avgType_ = AvgTypes.Bad;
					this.Text = "Bad data";
					return;
			}
			this.Text = rm.GetString(EmService.GetAvgTypeStringForRM(avgType_));
		}

		/// <summary>
		/// Constructor without parameters. For Clone() method ONLY!!!
		/// </summary>
		public EmTreeNodeAvgTypeClassic() :
			base(EmTreeNodeType.AvgGroup, EmDeviceType.NONE, null, null)
		{ }

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method
		/// </summary>
		/// <returns>Identical copy of this instance</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeAvgTypeClassic)node).measureType_ = this.measureType_;
			((EmTreeNodeAvgTypeClassic)node).avgType_ = this.avgType_;
			return node;
		}

		#endregion

		#region Methods

		/// <summary>Get text</summary>
		public static string GetText(AvgTypes avgType, object sender)
		{
			ResourceManager rm = new ResourceManager("EmArchiveTree.emstrings", sender.GetType().Assembly);
			return rm.GetString(EmService.GetAvgTypeStringForRM(avgType));
		}

		#endregion

		#region Properties

		/// <summary>Read only type of the measures</summary>
		public MeasureType MeasureType
		{
			get { return this.measureType_; }
		}

		public AvgTypes AvgType
		{
			get { return this.avgType_; }
		}

		#endregion
	}

	/// <summary>
	/// TreeNode that contains information of which measure types are present in database
	/// </summary>
	public class EmTreeNodeAvgTypeEtPQP_A : EmArchNodeBase
	{
		#region Fields

		/// <summary>
		/// Readonly. Type of the measures
		/// </summary>
		private MeasureType measureType_ = MeasureType.AVG;
		private AvgTypes_PQP_A avgType_;

		#endregion

		#region Constructors

		public EmTreeNodeAvgTypeEtPQP_A(AvgTypes_PQP_A avgType, EmDeviceType devType,
									EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(EmTreeNodeType.AvgGroup, devType, parentDev, parentObj)
		{
			avgType_ = avgType;

			ResourceManager rm = new ResourceManager("EmArchiveTree.emstrings", this.GetType().Assembly);
			this.Text = rm.GetString(EmService.GetAvgTypeStringForRM(avgType_));
		}

		public EmTreeNodeAvgTypeEtPQP_A(short iAvgType, EmDeviceType devType,
								EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(EmTreeNodeType.AvgGroup, devType, parentDev, parentObj)
		{
			ResourceManager rm = new ResourceManager("EmArchiveTree.emstrings", this.GetType().Assembly);
			switch (iAvgType)
			{
				case ((short)AvgTypes_PQP_A.ThreeSec):
					avgType_ = AvgTypes_PQP_A.ThreeSec; break;
				case ((short)AvgTypes_PQP_A.TenMin):
					avgType_ = AvgTypes_PQP_A.TenMin; break;
				case ((short)AvgTypes_PQP_A.TwoHours):
					avgType_ = AvgTypes_PQP_A.TwoHours; break;
				default:
					EmService.WriteToLogFailed("EmTreeNodeAvgType(): Invalid Avg Type!  "
						+ iAvgType.ToString());
					avgType_ = AvgTypes_PQP_A.Bad;
					this.Text = "Bad data";
					return;
			}
			this.Text = rm.GetString(EmService.GetAvgTypeStringForRM(avgType_));
		}

		/// <summary>
		/// Constructor without parameters. For Clone() method ONLY!!!
		/// </summary>
		public EmTreeNodeAvgTypeEtPQP_A() :
			base(EmTreeNodeType.AvgGroup, EmDeviceType.NONE, null, null)
		{ }

		#endregion

		#region Overriden methods

		/// <summary>
		/// Overriden method
		/// </summary>
		/// <returns>Identical copy of this instance</returns>
		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeAvgTypeEtPQP_A)node).measureType_ = this.measureType_;
			((EmTreeNodeAvgTypeEtPQP_A)node).avgType_ = this.avgType_;
			return node;
		}

		#endregion

		#region Methods

		/// <summary>Get text</summary>
		public static string GetText(AvgTypes_PQP_A avgType, object sender)
		{
			ResourceManager rm = new ResourceManager("EmArchiveTree.emstrings", sender.GetType().Assembly);
			return rm.GetString(EmService.GetAvgTypeStringForRM(avgType));
		}

		#endregion

		#region Properties

		/// <summary>Read only type of the measures</summary>
		public MeasureType MeasureType
		{
			get { return this.measureType_; }
		}

		public AvgTypes_PQP_A AvgType
		{
			get { return this.avgType_; }
		}

		#endregion
	}

	/// <summary>Measure of any type</summary>
	public abstract class EmTreeNodeDBMeasureBase : EmArchNodeBase
	{
		#region Fields

		/// <summary>Date and time of start measure</summary>
		protected DateTime start_;
		/// <summary>Date and time of end measure</summary>
		protected DateTime end_;
		/// <summary>Identifier of database record with this measure</summary>
		protected Int64 identifier_;
		/// <summary>Версия прошивки прибора</summary>
		protected string dev_version_;
		/// <summary>Период фликера</summary>
		protected short t_fliker_;
		/// <summary>Тип уставок</summary>
		protected short constraint_type_;

		#endregion

		/// <summary> Constructor </summary>
		public EmTreeNodeDBMeasureBase(EmDeviceType devType,
				EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(EmTreeNodeType.Measure, devType, parentDev, parentObj)
		{
		}

		#region Properties

		/// <summary>
		/// Gets identifier of database record with this measure
		/// </summary>
		public Int64 Id
		{
			get { return identifier_; }
		}

		/// <summary>
		/// Gets date and time of start measure. Read only (to setting up use constructor)
		/// </summary>
		public DateTime StartDateTime
		{
			get { return start_; }
		}

		/// <summary>
		/// Gets date and time of start measure. Read only (to setting up use constructor)
		/// </summary>
		public DateTime EndDateTime
		{
			get { return end_; }
		}

		/// <summary>
		/// Версия прошивки устройства
		/// </summary>
		public string DevVersion
		{
			get { return dev_version_; }
		}

		/// <summary>
		/// Период фликера
		/// </summary>
		public short T_fliker
		{
			get { return t_fliker_; }
		}

		/// <summary>
		/// Тип уставок
		/// </summary>
		public short ConstraintType
		{
			get { return constraint_type_; }
		}

		#endregion
	}

	/// <summary>Measure of any type</summary>
	public class EmTreeNodeDBMeasureClassic : EmTreeNodeDBMeasureBase
	{
		#region Fields

		/// <summary>avg type (for AVG archive)</summary>
		private AvgTypes avg_type_;
		///<summary>начало режима наибольших нагрузок</summary>
		private DateTime mlStartDateTime1_;
		///<summary>окончание режима наибольших нагрузок</summary>
		private DateTime mlEndDateTime1_;
		///<summary>начало режима наибольших нагрузок</summary>
		private DateTime mlStartDateTime2_;
		///<summary>окончание режима наибольших нагрузок</summary>
		private DateTime mlEndDateTime2_;

		#endregion

		#region Constructors

		/// <summary> Constructor for PQP </summary>
		public EmTreeNodeDBMeasureClassic(Int64 Identifier, DateTime Start, DateTime End,
				string DevVersion, short t_fliker,
				EmDeviceType devType,
				DateTime MlStartDate1, DateTime MlEndDate1,
				DateTime MlStartDate2, DateTime MlEndDate2,
				short ConstrType, EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(devType, parentDev, parentObj)
		{
			this.identifier_ = Identifier;
			this.start_ = Start;
			this.end_ = End;
			this.Text = String.Format("{0} - {1}", start_, end_);
			this.Active = false;
			this.dev_version_ = DevVersion;
			this.t_fliker_ = t_fliker;
			this.mlStartDateTime1_ = MlStartDate1;
			this.mlEndDateTime1_ = MlEndDate1;
			this.mlStartDateTime2_ = MlStartDate2;
			this.mlEndDateTime2_ = MlEndDate2;
			this.constraint_type_ = ConstrType;
		}

		/// <summary> Constructor for AVG </summary>
		public EmTreeNodeDBMeasureClassic(Int64 Identifier, DateTime Start, DateTime End,
				string DevVersion, short t_fliker,
				EmDeviceType devType,
				AvgTypes avgType, short ConstrType, EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(devType, parentDev, parentObj)
		{
			this.identifier_ = Identifier;
			this.start_ = Start;
			this.end_ = End;
			this.Text = String.Format("{0} - {1}", start_, end_);
			this.Active = false;
			this.dev_version_ = DevVersion;
			this.t_fliker_ = t_fliker;
			this.avg_type_ = avgType;
			this.constraint_type_ = ConstrType;
		}

		/// <summary>
		/// Constructor without parameters. Do not use! For Clone() method ONLY!!!
		/// </summary>
		public EmTreeNodeDBMeasureClassic() :
			base(EmDeviceType.NONE, null, null)
		{ }

		#endregion

		#region Overriden methods

		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeDBMeasureClassic)node).active_ = this.active_;
			((EmTreeNodeDBMeasureClassic)node).ActiveColor = this.ActiveColor;
			((EmTreeNodeDBMeasureClassic)node).constraint_type_ = this.constraint_type_;
			((EmTreeNodeDBMeasureClassic)node).DefaultColor = this.DefaultColor;
			((EmTreeNodeDBMeasureClassic)node).dev_version_ = this.dev_version_;
			((EmTreeNodeDBMeasureClassic)node).identifier_ = this.identifier_;
			((EmTreeNodeDBMeasureClassic)node).start_ = this.start_;
			((EmTreeNodeDBMeasureClassic)node).end_ = this.end_;
			((EmTreeNodeDBMeasureClassic)node).mlStartDateTime1_ = this.mlStartDateTime1_;
			((EmTreeNodeDBMeasureClassic)node).mlEndDateTime1_ = this.mlEndDateTime1_;
			((EmTreeNodeDBMeasureClassic)node).mlStartDateTime2_ = this.mlStartDateTime2_;
			((EmTreeNodeDBMeasureClassic)node).mlEndDateTime2_ = this.mlEndDateTime2_;
			((EmTreeNodeDBMeasureClassic)node).constraint_type_ = this.constraint_type_;
			((EmTreeNodeDBMeasureClassic)node).avg_type_ = this.avg_type_;
			((EmTreeNodeDBMeasureClassic)node).t_fliker_ = this.t_fliker_;
			return node;

		}

		#endregion

		#region Properties

		/// <summary>
		/// Period id (for AVG archive)
		/// </summary>
		public AvgTypes AvgType
		{
			get { return avg_type_; }
		}

		public DateTime MlStartDateTime1
		{
			get { return mlStartDateTime1_; }
		}

		public DateTime MlEndDateTime1
		{
			get { return mlEndDateTime1_; }
		}

		public DateTime MlStartDateTime2
		{
			get { return mlStartDateTime2_; }
		}

		public DateTime MlEndDateTime2
		{
			get { return mlEndDateTime2_; }
		}

		#endregion
	}

	/// <summary>Measure of any type</summary>
	public class EmTreeNodeDBMeasureEtPQP_A : EmTreeNodeDBMeasureBase
	{
		#region Fields

		/// <summary>avg type (for AVG archive)</summary>
		private AvgTypes_PQP_A avg_type_PQP_A_;

		#endregion

		#region Constructors

		/// <summary> Constructor for PQP</summary>
		public EmTreeNodeDBMeasureEtPQP_A(Int64 Identifier, DateTime Start, DateTime End,
				string DevVersion, short t_fliker,
				EmDeviceType devType,
				short ConstrType, EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(devType, parentDev, parentObj)
		{
			this.identifier_ = Identifier;
			this.start_ = Start;
			this.end_ = End;
			this.Text = String.Format("{0} - {1}", start_, end_);
			this.Active = false;
			this.dev_version_ = DevVersion;
			this.t_fliker_ = t_fliker;
			this.constraint_type_ = ConstrType;
		}

		/// <summary> Constructor for AVG </summary>
		public EmTreeNodeDBMeasureEtPQP_A(Int64 Identifier, DateTime Start, DateTime End,
				string DevVersion, short t_fliker,
				EmDeviceType devType,
				AvgTypes_PQP_A avgType, short ConstrType, EmArchNodeBase parentDev, EmArchNodeBase parentObj)
			: base(devType, parentDev, parentObj)
		{
			this.identifier_ = Identifier;
			this.start_ = Start;
			this.end_ = End;
			this.Text = String.Format("{0} - {1}", start_, end_);
			this.Active = false;
			this.dev_version_ = DevVersion;
			this.t_fliker_ = t_fliker;
			this.avg_type_PQP_A_ = avgType;
			this.constraint_type_ = ConstrType;
		}

		/// <summary>
		/// Constructor without parameters. Do not use! For Clone() method ONLY!!!
		/// </summary>
		public EmTreeNodeDBMeasureEtPQP_A() :
			base(EmDeviceType.NONE, null, null)
		{ }

		#endregion

		#region Overriden methods

		public override object Clone()
		{
			object node = base.Clone();
			((EmTreeNodeDBMeasureEtPQP_A)node).active_ = this.active_;
			((EmTreeNodeDBMeasureEtPQP_A)node).ActiveColor = this.ActiveColor;
			((EmTreeNodeDBMeasureEtPQP_A)node).constraint_type_ = this.constraint_type_;
			((EmTreeNodeDBMeasureEtPQP_A)node).DefaultColor = this.DefaultColor;
			((EmTreeNodeDBMeasureEtPQP_A)node).dev_version_ = this.dev_version_;
			((EmTreeNodeDBMeasureEtPQP_A)node).identifier_ = this.identifier_;
			((EmTreeNodeDBMeasureEtPQP_A)node).start_ = this.start_;
			((EmTreeNodeDBMeasureEtPQP_A)node).end_ = this.end_;
			((EmTreeNodeDBMeasureEtPQP_A)node).constraint_type_ = this.constraint_type_;
			((EmTreeNodeDBMeasureEtPQP_A)node).avg_type_PQP_A_ = this.avg_type_PQP_A_;
			((EmTreeNodeDBMeasureEtPQP_A)node).t_fliker_ = this.t_fliker_;
			return node;

		}

		#endregion

		#region Properties

		/// <summary>
		/// AVG type (for AVG archive)
		/// </summary>
		public AvgTypes_PQP_A AvgType_PQP_A
		{
			get { return avg_type_PQP_A_; }
		}

		#endregion
	}
}
