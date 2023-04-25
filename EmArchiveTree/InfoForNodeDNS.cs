using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DbServiceLib;
using EmServiceLib;

namespace EmArchiveTree
{
	public class EmInfoForNodeDNSBase : EmInfoForNodeBase
	{
	}

	public class EmInfoForNodeDNSClassic : EmInfoForNodeDNSBase
	{
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

		public EmInfoForNodeDNSClassic(ref DbService dbService, EmDeviceType devType)
		{
			datetimeId_ = (Int64)dbService.DataReaderData("datetime_id");
			dtStart_ = (DateTime)dbService.DataReaderData("start_datetime");
			dtEnd_ = (DateTime)dbService.DataReaderData("end_datetime");
			if (devType == EmDeviceType.EM32 || devType == EmDeviceType.ETPQP)
			{
				fldYearId_ = (Int64)dbService.DataReaderData("folder_year_id");
				fldMonthId_ = (Int64)dbService.DataReaderData("folder_month_id");
			}
		}
	}

	public class EmInfoForNodeDNS_EtPQP_A : EmInfoForNodeDNSBase
	{
		protected Int64 fldParentId_;

		public EmInfoForNodeDNS_EtPQP_A(ref DbService dbService, EmDeviceType devType)
		{
			datetimeId_ = (Int64)dbService.DataReaderData("datetime_id");
			dtStart_ = (DateTime)dbService.DataReaderData("start_datetime");
			dtEnd_ = (DateTime)dbService.DataReaderData("end_datetime");
			fldParentId_ = (Int64)dbService.DataReaderData("parent_folder_id");
		}
	}

	public class DNSDatesList
	{
		private List<EmInfoForNodeDNSBase> list_ = new List<EmInfoForNodeDNSBase>();

		public void Add(EmInfoForNodeDNSBase item)
		{
			list_.Add(item);
		}

		public void SortItems()
		{
			list_.Sort(ComparePairs);
		}

		private static int ComparePairs(EmInfoForNodeDNSBase x, EmInfoForNodeDNSBase y)
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

		public EmInfoForNodeDNSBase this[int index]
		{
			get
			{
				return list_[index];
			}
		}
	}
}
