using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmArchiveTree
{
	public class EmInfoForNodeBase
	{
		protected DateTime dtStart_;
		protected DateTime dtEnd_;
		protected Int64 datetimeId_;

		public Int64 DatetimeId
		{
			get { return datetimeId_; }
			set { datetimeId_ = value; }
		}

		public DateTime DtStart
		{
			get { return dtStart_; }
			set { dtStart_ = value; }
		}

		public DateTime DtEnd
		{
			get { return dtEnd_; }
			set { dtEnd_ = value; }
		}
	}
}
