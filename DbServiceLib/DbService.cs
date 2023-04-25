using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Npgsql;
using EmServiceLib;

namespace DbServiceLib
{
	public class DbService
	{
		private string connectString_;
		private NpgsqlConnection conEmDb_;
		private NpgsqlCommand sqlCommand_ = new NpgsqlCommand();
		private NpgsqlDataReader dataReader_;

		// переменная показывает нужно ли закрывать соединение. 
		// не нужно - если использовался коннект от другого экземпляра
		// (тогда он будет закрыт в том другом экземпляре)
		private bool needCloseConnect_ = true;

		public string Host
		{
			get
			{
				if (conEmDb_ != null) return conEmDb_.Host;
				return string.Empty;
			}
		}

		public int Port
		{
			get
			{
				if (conEmDb_ != null) return conEmDb_.Port;
				return -1;
			}
		}

		public string Database
		{
			get
			{
				if (conEmDb_ != null) return conEmDb_.Database;
				return string.Empty;
			}
		}

		public System.Data.ConnectionState ConnectionState
		{
			get { return conEmDb_.State; }
		}

		public bool DataReaderHasRows
		{
			get { return dataReader_.HasRows; }
		}

		public DbService(string connectString)
		{
			connectString_ = connectString;
		}

		public bool Open()
		{
			connectString_ = connectString_.Replace("localhost", "127.0.0.1");
			conEmDb_ = new NpgsqlConnection(connectString_);
			try { conEmDb_.Open(); }
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error connection to DB");
				return false;
			}
			sqlCommand_.Connection = conEmDb_;
			return true;
		}

		public int ExecuteNonQuery(string commandText, bool dumpException)
		{
			sqlCommand_.CommandText = commandText;
			try
			{
				return sqlCommand_.ExecuteNonQuery();
			}
			catch (Exception ex)
			{
				if (dumpException)
				{
					EmService.WriteToLogFailed(commandText);
					EmService.DumpException(ex, "Error in ExecuteNonQuery():");
				}
				else EmService.WriteToLogFailed("Error in ExecuteNonQuery: " + ex.Message);
				return -1;
			}
		}

		public bool CreateAndFillDataAdapter(string querryText, string tableName, ref DataSet ds)
		{
			try
			{
				NpgsqlDataAdapter da = new NpgsqlDataAdapter(querryText, conEmDb_);
				da.TableMappings.Add("Table", tableName);
				da.Fill(ds);
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in CreateAndFillDataAdapter():");
				return false;
			}
		}

		public Int64 ExecuteScalarInt64(string commandText)
		{
			sqlCommand_.CommandText = commandText;
			try
			{
				return (Int64)sqlCommand_.ExecuteScalar();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(commandText);
				EmService.DumpException(ex, "Error in ExecuteScalarInt64():");
				return -1;
			}
		}

		public short ExecuteScalarInt16(string commandText)
		{
			sqlCommand_.CommandText = commandText;
			try
			{
				return (Int16)sqlCommand_.ExecuteScalar();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(commandText);
				EmService.DumpException(ex, "Error in ExecuteScalarInt16():");
				return -1;
			}
		}

		public string ExecuteScalarString(string commandText)
		{
			sqlCommand_.CommandText = commandText;
			try
			{
				return sqlCommand_.ExecuteScalar().ToString();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(commandText);
				EmService.DumpException(ex, "Error in ExecuteScalarInt64():");
				return string.Empty;
			}
		}

		public object ExecuteScalar(string commandText)
		{
			sqlCommand_.CommandText = commandText;
			try
			{
				return sqlCommand_.ExecuteScalar();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed(commandText);
				EmService.DumpException(ex, "Error in ExecuteScalarInt64():");
				return null;
			}
		}

		public bool DataReaderRead()
		{
			return dataReader_.Read();
		}

		public bool ExecuteReader(string commandText)
		{
			sqlCommand_.CommandText = commandText;
			try
			{
				dataReader_ = sqlCommand_.ExecuteReader();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ExecuteReader(): " + commandText);
				return false;
			}
			return true;
		}

		public object DataReaderData(string columnName, bool returnNullAsString)
		{
			//return dataReader_[columnName];
			object res = dataReader_[columnName];
			if ((res is DBNull) && returnNullAsString) return "null";
			return res;
		}

		public object DataReaderData(int columnNumber, bool returnNullAsString)
		{
			object res = dataReader_[columnNumber];
			if ((res is DBNull) && returnNullAsString) return "null";
			return res;
		}

		public object DataReaderData(string columnName)
		{
			//return dataReader_[columnName];
			object res = dataReader_[columnName];
			if (res is DBNull) return "null";
			return res;
		}

		public object DataReaderData(int columnNumber)
		{
			object res = dataReader_[columnNumber];
			if (res is DBNull) return "null";
			return res;
		}

		public void CloseReader()
		{
			if (dataReader_ != null && !dataReader_.IsClosed) dataReader_.Close();
		}

		public void CloseConnect()
		{
			if (dataReader_ != null && !dataReader_.IsClosed) dataReader_.Close();
			if (needCloseConnect_)
			{
				try
				{
					if ((conEmDb_ != null) && (conEmDb_.State == ConnectionState.Open))
						conEmDb_.Close();
				}
				catch 
				{
					EmService.WriteToLogFailed("CloseConnect() exception");
				}
			}
		}

		public bool CopyConnection(ref DbService other)
		{
			if (other == null) return false;
			if (other.conEmDb_ == null) return false;

			conEmDb_ = other.conEmDb_;
			sqlCommand_.Connection = conEmDb_;

			// используем чужой коннект, следовательно не закрываем
			needCloseConnect_ = false;
			return true;
		}
	}
}
