using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace EmDataSaver.SqlImage
{
	// класс хранит выбранные для экспорта типы архивов, которые разбиты по объектам.
	// разбиение по объектам реально используется только для EtPQP. у Em32 нет объектов, а у
	// Em33t всегда экспортируется только один объект, поэтому для этих устройств всегда создается
	// один объект (это нужно только для единства интерфейса с функциями экспорта для EtPQP)
	//public class EmSqlDataNodeTypesToExport
	//{
	//    public Int64 DeviceId;

	//    public class EmSqlObjectToExport
	//    {
	//        public Int64 ObjectId;
	//        public string ObjectName;
	//        public EmSqlDataNodeType[] Parts;
	//        public bool[] SelectedParts = new bool[] { false, false, false, false };
	//        public Int64 FolderId;

	//        public EmSqlDataNodeType[] OutputArchiveParts
	//        {
	//            get
	//            {
	//                List<EmSqlDataNodeType> temp_list = new List<EmSqlDataNodeType>();
	//                if (SelectedParts[(int)EmSqlDataNodeType.PQP])
	//                    temp_list.Add(EmSqlDataNodeType.PQP);
	//                if (SelectedParts[(int)EmSqlDataNodeType.Events])
	//                    temp_list.Add(EmSqlDataNodeType.Events);
	//                if (SelectedParts[(int)EmSqlDataNodeType.AVG])
	//                    temp_list.Add(EmSqlDataNodeType.AVG);
	//                if (temp_list.Count == 0) return null;
	//                EmSqlDataNodeType[] temp = new EmSqlDataNodeType[temp_list.Count];
	//                temp_list.CopyTo(temp);
	//                return temp;
	//            }
	//        }

	//        public bool IsSomethingSelected()
	//        {
	//            for (int i = 0; i < SelectedParts.Length; ++i)
	//            {
	//                if (SelectedParts[i]) return true;
	//            }
	//            return false;
	//        }
	//    }

	//    public List<EmSqlObjectToExport> Objects;

	//    public EmSqlObjectToExport GetObjectByName(string name)
	//    {
	//        for (int iObj = 0; iObj < Objects.Count; ++iObj)
	//        {
	//            if (Objects[iObj].ObjectName == name)
	//                return Objects[iObj];
	//        }
	//        return null;
	//    }
	//}

	public enum EmSqlDataNodeType
	{
		PQP = 0,
		Events = 1,
		AVG = 2
	}

	public class EmSqlDataNode
	{
		#region Fields

		private EmSqlDataNodeType sqlType;
		private DateTime begin;
		private DateTime end;
		private string sql;
		// use only for AVG archives
		private string avgFileName_ = "";

		#endregion

		#region Properties

		[XmlElement]
		public EmSqlDataNodeType SqlType
		{
			get { return sqlType; }
			set { sqlType = value; }
		}

		[XmlElement]
		public string AvgFileName
		{
			get { return avgFileName_; }
			set { avgFileName_ = value; }
		}

		[XmlAttribute]
		public DateTime Begin
		{
			get { return begin; }
			set { begin = value; }
		}

		[XmlAttribute]
		public DateTime End
		{
			get { return end; }
			set { end = value; }
		}

		[XmlElement]
		public string Sql
		{
			get { return sql; }
			set { sql = value; }
		}

		#endregion
	}
}
