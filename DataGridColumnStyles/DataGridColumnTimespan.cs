using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;

namespace DataGridColumnStyles
{
	public class DataGridColumnTimespan: DataGridColumnGroupCaption
	{
		public DataGridColumnTimespan() { }
		public DataGridColumnTimespan(DataGridGroupCaption Caption, int Index)
		{
			this.GroupCaption = Caption;
			this.GroupIndex = Index;
		}

		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			string retval = string.Empty;
			try
			{
				TimeSpan tsVal = TimeSpan.FromTicks((System.Int64)((DataRowView)source.List[rowNum]).Row[this.MappingName]);
				//retval = String.Format("{0:00}:{1:00}:{2:00}", tsVal.Hours, tsVal.Minutes, tsVal.Seconds);
				retval = String.Format("{0:00}:{1:00}:{2:00}", tsVal.Hours + (tsVal.Days * 24), tsVal.Minutes, tsVal.Seconds);
			}
			catch
			{
				retval = string.Empty;
			}
			return retval;
		}
	}
}
