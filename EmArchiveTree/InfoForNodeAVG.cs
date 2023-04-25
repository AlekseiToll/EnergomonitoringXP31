using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DbServiceLib;
using EmServiceLib;

namespace EmArchiveTree
{
	public class EmInfoForNodeAVGBase : EmInfoForNodeBase
	{
		
	}

	public class EmInfoForNodeAVGClassic : EmInfoForNodeAVGBase
	{
		protected Int64 fldYearId_;
		protected Int64 fldMonthId_;
		protected AvgTypes avgType_;

		public AvgTypes AvgType
		{
			get { return avgType_; }
			set { avgType_ = value; }
		}

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

		public EmInfoForNodeAVGClassic(ref DbService dbService, EmDeviceType devType)
		{
			datetimeId_ = (Int64)dbService.DataReaderData("datetime_id");
			dtStart_ = (DateTime)dbService.DataReaderData("start_datetime");
			dtEnd_ = (DateTime)dbService.DataReaderData("end_datetime");
			if (devType == EmDeviceType.EM32 || devType == EmDeviceType.ETPQP)
			{
				fldYearId_ = (Int64)dbService.DataReaderData("folder_year_id");
				fldMonthId_ = (Int64)dbService.DataReaderData("folder_month_id");
			}
			short period_id = (short)dbService.DataReaderData("period_id");

			//??????????????????????? разобраться с нумерацией avgType!!!  комменты в БД!
			// нумерация типов усреднения не совпадает у Эм32 и Эм33Т,
			// поэтому исправляем
			if (devType == EmDeviceType.EM31K || devType == EmDeviceType.EM33T ||
				devType == EmDeviceType.EM33T1)
				period_id++;

			switch (period_id)
			{
				case 1: avgType_ = AvgTypes.ThreeSec; break;
				case 2: avgType_ = AvgTypes.OneMin; break;
				case 3: avgType_ = AvgTypes.ThirtyMin; break;
				default: avgType_ = AvgTypes.Bad; break;
			}
		}
	}

	public class EmInfoForNodeAVG_EtPQP_A : EmInfoForNodeAVGBase
	{
		protected Int64 fldParentId_;
		protected AvgTypes_PQP_A avgType_;

		public AvgTypes_PQP_A AvgType
		{
			get { return avgType_; }
			set { avgType_ = value; }
		}

		public EmInfoForNodeAVG_EtPQP_A(ref DbService dbService, EmDeviceType devType)
		{
			datetimeId_ = (Int64)dbService.DataReaderData("datetime_id");
			dtStart_ = (DateTime)dbService.DataReaderData("start_datetime");
			dtEnd_ = (DateTime)dbService.DataReaderData("end_datetime");
			fldParentId_ = (Int64)dbService.DataReaderData("parent_folder_id");
			short period_id = (short)dbService.DataReaderData("period_id");

			//??????????????????????? разобраться с нумерацией avgType!!! см таблицу periods
			switch (period_id)
			{
				case 1: avgType_ = AvgTypes_PQP_A.ThreeSec; break;
				case 2: avgType_ = AvgTypes_PQP_A.TenMin; break;
				case 3: avgType_ = AvgTypes_PQP_A.TwoHours; break;
				default: avgType_ = AvgTypes_PQP_A.Bad; break;
			}
		}
	}

	public class AVGDatesList
	{
		private List<EmInfoForNodeAVGBase> list_ = new List<EmInfoForNodeAVGBase>();

		public void Add(EmInfoForNodeAVGBase item)
		{
			list_.Add(item);
		}

		public void SortItems()
		{
			list_.Sort(ComparePairs);
		}

		private static int ComparePairs(EmInfoForNodeAVGBase x, EmInfoForNodeAVGBase y)
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

		public EmInfoForNodeAVGBase this[int index]
		{
			get
			{
				return list_[index];
			}
		}
	}
}
