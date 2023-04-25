using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Collections;

namespace XML.Excel
{
	/// <summary>
	/// Inner list class
	/// </summary>
	class DataGridStruct
	{
		#region Fields

		/// <summary>
		/// DataGrid object
		/// </summary>
		public DataGrid dataGrid;

		/// <summary>
		/// Caption
		/// </summary>
		public String caption;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructour
		/// </summary>
		/// <param name="DataGrid">DataGrid object</param>
		/// <param name="Caption">Caption</param>
		public DataGridStruct(DataGrid DataGrid, String Caption)
		{
			this.dataGrid = DataGrid;
			this.caption = Caption;
		}

		#endregion
	}

	public class DataGridList
	{
		#region Fields

		/// <summary>
		/// Inner list of grids
		/// </summary>
		private List<DataGridStruct> dataGridList;

		#endregion

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public DataGridList() { dataGridList = new List<DataGridStruct>(); } 

		#endregion

		#region Public collection methods

		public void Add(DataGrid dataGrid, String caption)
		{
			dataGridList.Add(new DataGridStruct(dataGrid, caption));
		}

		public void Clear()
		{
			dataGridList.Clear();
		}

		public void Remove(int index)
		{
			dataGridList.RemoveAt(index);
		}

		public DataGrid this[int index]
		{
			get
			{
				return dataGridList[index].dataGrid;
			}
			set
			{
				dataGridList[index].dataGrid = value;
			}
		}

		public String this[int index, bool IsCaption]
		{
			get
			{
				return dataGridList[index].caption;
			}
			set
			{
				dataGridList[index].caption = value;
			}
		}

		public int Count
		{
			get { return dataGridList.Count ; }
		}

		#endregion
	}
}
