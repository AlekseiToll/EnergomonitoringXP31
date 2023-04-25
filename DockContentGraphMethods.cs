using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Data;

using WeifenLuo.WinFormsUI;
using EmServiceLib;

namespace EnergomonitoringXP
{
	public class DockContentGraphMethods : DockContent
	{
		protected void SortByColumn(DataGrid dg, int columnIndex, bool bDesc)
		{
			try
			{
				if (dg.TableStyles.Count == 0)
				{
					EmService.WriteToLogFailed("Error in SortByColumn(): Count = 0, " + dg.Name);
					return;
				}

				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dg.DataSource, dg.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.Sort =
					dg.TableStyles[0].GridColumnStyles[columnIndex].MappingName + (bDesc ? " DESC" : " ASC");
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in SortByColumn():  " + columnIndex.ToString());
				//throw;
			}
		}

		protected String GetCurrentSortRule(DataGrid dg)
		{
			try
			{
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dg.DataSource, dg.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				return dataView.Sort;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in GetCurrentSortRule(): " + ex.Message);
				throw;
			}
		}

		protected void SetSortRule(DataGrid dg, string rule)
		{
			try
			{
				CurrencyManager currencyManager = (CurrencyManager)BindingContext[dg.DataSource, dg.DataMember];
				DataView dataView = (DataView)currencyManager.List;
				dataView.Sort = rule;
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in SetSortRule(): " + ex.Message);
				throw;
			}
		}
	}
}
