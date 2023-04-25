using System;
using System.Windows.Forms;
using System.Collections.Generic;

using EmServiceLib;
using EmArchiveTree;

namespace EmDataSaver.SavingInterface.CheckTreeView
{
	/// <summary>
	/// MeasureTreeNode class.
	/// Contains date end time of the start and end of data measuring
	/// </summary>
	public class MeasureTreeNode : CheckTreeNode
	{
		#region Fields

		/// <summary>
		///  Type of measure
		/// </summary>
		public MeasureType MeasureType;

		/// <summary>
		/// Inner measure index
		/// </summary>
		public int MeasureIndex;

		/// <summary>
		/// Date and time of measure was starts
		/// </summary>
		public DateTime StartDateTime;

		/// <summary>
		/// Date and time of measure was ends
		/// </summary>
		public DateTime EndDateTime;

		public EmDeviceType DeviceType;

		private DateTime originStartDateTime_;
		private DateTime originEndDateTime_;

		// список полей для чтения из архива средних (для запроса с параметрами)
		private List<ushort> listAvgParams_ = null;
		// маски, указывающие заполненность архива средних
		private frmAvgParams.MaskAvgArray masks_ = new frmAvgParams.MaskAvgArray();
		// это поле указывает была ли применена маска, т.к. при значении маски -1 это нельзя
		// определить. -1 может указывать на то, что были выбраны все параметры, или что
		// юзер вообще не вызывал форму для выбора. а от этого поля зависит надо ли восстанавливать
		// расстановку галочек при последующем открытии формы выбора параметров
		private bool masksWasSet_ = false;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="StartDateTime">Date and time of measure was starts</param>
		/// <param name="EndDateTime">Date and time of measure was ends</param>
		/// <param name="MeasureIndex">Inner measure index</param>
		public MeasureTreeNode(DateTime startDateTime, DateTime endDateTime, int measureIndex,
								MeasureType curMeasureType, EmDeviceType devType)
		{
			this.StartDateTime = startDateTime;
			this.EndDateTime = endDateTime;
			this.MeasureIndex = measureIndex;
			this.Tag = "Measure";
			this.MeasureType = curMeasureType;
			this.DeviceType = devType;

			this.originStartDateTime_ = startDateTime;
			this.originEndDateTime_ = endDateTime;

			// если список параметров отсутствует, то заполняем его полностью, чтобы был считан
			// весь архив
			// вернуть это если будет нужно чтение avg одним запросом 
			//if (devType == EmDeviceType.EM32)
			//{
			//    listAvgParams_ = new List<ushort>(2048);
			//    for (ushort i = 0; i < 2048; ++i)
			//        listAvgParams_.Add(i);
			//}
			//else
			//{
				listAvgParams_ = null;
			//}
		}

		#endregion

		#region Properties

		public DateTime OriginDateStart
		{
			get { return originStartDateTime_; }
		}

		public DateTime OriginDateEnd
		{
			get { return originEndDateTime_; }
		}

		public List<ushort> ListAvgParams
		{
			get { return listAvgParams_; }
			set { listAvgParams_ = value; }
		}

		public frmAvgParams.MaskAvgArray MasksAvg
		{
			get { return masks_; }
			set { masks_ = value; }
		}

		public bool MasksAvgWasSet
		{
			get { return masksWasSet_; }
			set { masksWasSet_ = value; }
		}

		#endregion
	}
}