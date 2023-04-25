using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DbServiceLib;
using EmServiceLib;

namespace EmArchiveTree
{
	public class EmInfoForNodePQPBase : EmInfoForNodeBase
	{
		protected Int16 constraintType_;

		public Int16 ConstraintType
		{
			get { return constraintType_; }
			set { constraintType_ = value; }
		}
	}

	public class EmInfoForNodePQPClassic : EmInfoForNodePQPBase
	{
		protected DateTime mlStartTime1_;
		protected DateTime mlEndTime1_;
		protected DateTime mlStartTime2_;
		protected DateTime mlEndTime2_;
		protected Int64 fldYearId_;
		protected Int64 fldMonthId_;

		public Int64 FldYearId
		{
			get { return fldYearId_; }
			set { fldYearId_ = value; }
		}

		public Int64 FldMonthId
		{
			get { return fldMonthId_; }
			set { fldMonthId_ = value; }
		}

		public DateTime MlStartTime1
		{
			get { return mlStartTime1_; }
			set { mlStartTime1_ = value; }
		}

		public DateTime MlEndTime1
		{
			get { return mlEndTime1_; }
			set { mlEndTime1_ = value; }
		}

		public DateTime MlStartTime2
		{
			get { return mlStartTime2_; }
			set { mlStartTime2_ = value; }
		}

		public DateTime MlEndTime2
		{
			get { return mlEndTime2_; }
			set { mlEndTime2_ = value; }
		}

		public EmInfoForNodePQPClassic(ref DbService dbService, EmDeviceType devType)
		{
			datetimeId_ = (Int64)dbService.DataReaderData("datetime_id");
			dtStart_ = (DateTime)dbService.DataReaderData("start_datetime");
			dtEnd_ = (DateTime)dbService.DataReaderData("end_time");
			constraintType_ = (Int16)dbService.DataReaderData("constraint_type");
			object mlStartTime1 = dbService.DataReaderData("ml_start_time_1");
			object mlEndTime1 = dbService.DataReaderData("ml_end_time_1");
			object mlStartTime2 = dbService.DataReaderData("ml_start_time_2");
			object mlEndTime2 = dbService.DataReaderData("ml_end_time_2");
			if (mlStartTime1 is DBNull) mlStartTime1_ = DateTime.MinValue;
			else mlStartTime1_ = (DateTime)dbService.DataReaderData("ml_start_time_1");
			if (mlEndTime1 is DBNull) mlEndTime1_ = DateTime.MinValue;
			else mlEndTime1_ = (DateTime)dbService.DataReaderData("ml_end_time_1");
			if (mlStartTime2 is DBNull) mlStartTime2_ = DateTime.MinValue;
			else mlStartTime2_ = (DateTime)dbService.DataReaderData("ml_start_time_2");
			if (mlEndTime2 is DBNull) mlEndTime2_ = DateTime.MinValue;
			else mlEndTime2_ = (DateTime)dbService.DataReaderData("ml_end_time_2");

			if (devType == EmDeviceType.EM32 || devType == EmDeviceType.ETPQP)
			{
				fldYearId_ = (Int64)dbService.DataReaderData("folder_year_id");
				fldMonthId_ = (Int64)dbService.DataReaderData("folder_month_id");
			}
		}
	}

	public class EmInfoForNodePQP_EtPQP_A : EmInfoForNodePQPBase
	{
		protected Int64 fldParentId_;

		public EmInfoForNodePQP_EtPQP_A(ref DbService dbService, EmDeviceType devType)
		{
			datetimeId_ = (Int64)dbService.DataReaderData("datetime_id");
			dtStart_ = (DateTime)dbService.DataReaderData("start_datetime");
			dtEnd_ = (DateTime)dbService.DataReaderData("end_time");
			constraintType_ = (Int16)dbService.DataReaderData("constraint_type");
			fldParentId_ = (Int64)dbService.DataReaderData("parent_folder_id");
		}
	}

	public class PQPDatesList
	{
		private List<EmInfoForNodePQPBase> list_ = new List<EmInfoForNodePQPBase>();

		public void Add(EmInfoForNodePQPBase item)
		{
			list_.Add(item);
		}

		public void SortItems()
		{
			list_.Sort(ComparePairs);
		}

		private static int ComparePairs(EmInfoForNodePQPBase x, EmInfoForNodePQPBase y)
		{
			if (x.DtStart > y.DtStart) return 1;
			if (x.DtStart == y.DtStart) return 0;
			if (x.DtStart < y.DtStart) return -1;
			return 0;
		}

		public int Count
		{
			get
			{
				return list_.Count;
			}
		}

		public EmInfoForNodePQPBase this[int index]
		{
			get
			{
				return list_[index];
			}
		}
	}
}
