using System;
using System.Windows.Forms;
using System.Drawing;
using System.Resources;
using System.Collections.Generic;

using DbServiceLib;
using EmServiceLib;
using EmDataSaver;
using EmDataSaver.SqlImage;
using DeviceIO;
using EmArchiveTree;

namespace EmDataSaver.SavingInterface.CheckTreeView
{
	/// <summary>
	/// DeviceTreeView class (to show contents of device data)
	/// </summary>
	public class DeviceTreeView: TreeView
	{
		const int cntRecordsDnS_ = 16384;

		#region Fields

		internal bool EnableMouseChecks = true;

		private bool PrevCheckState = true;  //предыдущее состояние чекбокса "Осн. показатели" для 
											//управления галочками

		#endregion

		#region Events

		/// <summary>Delegate to describe functions of events NothingChecked and SomthingChecked</summary>
		public delegate void CheckedHandler();

		/// <summary>Event occures when all items were unchecked</summary>
		public static event CheckedHandler NothingChecked;

		/// <summary>Event occures when one of items wes checked</summary>
		public static event CheckedHandler SomethingChecked;

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		public DeviceTreeView(){ }

		#endregion

		#region Public methods

		/// <summary>
		/// Imports data from contents (for EM33T and EM31K)
		/// </summary>
		internal void ImportDataFromContents(ref DeviceIO.DeviceCommonInfoEm33 devInfo)
		{
			try
			{
				// check - if Contents is empty
				if (devInfo.Content == null || devInfo.Content.Length == 0) return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode();
				root.Text = String.Format("{0} #{1} (v.{2})",
					devInfo.Name, devInfo.SerialNumber, devInfo.Version);
				root.Tag = "Device";
				this.Nodes.Add(root);

				// creating childs
				for (int i = 0; i < devInfo.Content.Length; i++)
				{
					// creating object node
					ObjectTreeNode obj = new ObjectTreeNode(i, devInfo.Content[i].ObjectName,
															devInfo.Content[i].ConnectionScheme);

					// creating PQP measures
					if (devInfo.Content[i].PqpExists == true)
					{
						for (int iNumOfPKE = 0; iNumOfPKE < devInfo.Content[i].PqpSet.Length; iNumOfPKE++)
						{
							obj.AddMeasure(MeasureType.PQP, devInfo.DeviceType,
										devInfo.Content[i].PqpSet[iNumOfPKE].PqpStart, 
										devInfo.Content[i].PqpSet[iNumOfPKE].PqpEnd, 
										iNumOfPKE, 0);
						}
					}

					// creating AVG measures
					if (devInfo.Content[i].AvgExists == true)
					{
						obj.AddMeasure(MeasureType.AVG, devInfo.DeviceType,
										devInfo.Content[i].AvgBegin, devInfo.Content[i].AvgEnd, 
										0, 3);	// последняя 3 означает, что создаем все items
					}

					// creating DNS measures
					if (devInfo.Content[i].DnsExists == true)
					{
						obj.AddMeasure(MeasureType.DNS, devInfo.DeviceType,
										devInfo.Content[i].DnsStart, 
										devInfo.Content[i].DnsEnd, 
										0, 0);
					}
					// etc...

					// adding DateTimes to Object Name
					DateTime dtS, dtE;
					dtS = DateTime.MaxValue;
					dtE = DateTime.MinValue;

					// 
					for (int iMTN = 0; iMTN < obj.Nodes.Count; iMTN++)
					{
						for (int iMN = 0; iMN < obj.Nodes[iMTN].Nodes.Count; iMN++)
						{
							if ((obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).StartDateTime < dtS)
								dtS = (obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).StartDateTime;

							if ((obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).EndDateTime > dtE)
								dtE = (obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).EndDateTime;
						}
					}
					obj.Text = obj.Name + " " + dtS.ToString() + " - " + dtE.ToString();

					root.Nodes.Add(obj);
				}
				//root.Check();
				root.ExpandAll();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ImportDataFromContents(): " + ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Imports data from contents (for ETPQP)
		/// </summary>
		internal void ImportDataFromContents(ref DeviceIO.DeviceCommonInfoEtPQP devInfo, bool splitArchive)
		{
			try
			{
				// check - if Contents is empty
				if (devInfo.Content == null || devInfo.Content.Count == 0) return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode();
				root.Text = String.Format("ETPQP #{0} (v.{1})", devInfo.SerialNumber, devInfo.DevVersion);
				root.Tag = "Device";
				this.Nodes.Add(root);

				// creating childs
				for (int i = 0; i < devInfo.Content.Count; i++)
				{
					// creating object node
					ObjectTreeNode obj = new ObjectTreeNode(i, devInfo.Content[i].ObjectName,
															devInfo.Content[i].ConnectionScheme);

					// creating PQP measures
					if (devInfo.Content[i].PqpExists == true)
					{
						for (int iNumOfPKE = 0; iNumOfPKE < devInfo.Content[i].PqpSet.Count; iNumOfPKE++)
						{
							obj.AddMeasure(MeasureType.PQP, EmDeviceType.ETPQP,
											devInfo.Content[i].PqpSet[iNumOfPKE].PqpStart,
											devInfo.Content[i].PqpSet[iNumOfPKE].PqpEnd, iNumOfPKE, 0);
						}
					}

					// creating AVG measures
					if (devInfo.Content[i].AvgExists == true)
					{
						if (!splitArchive)
						{
							if (devInfo.Content[i].DateStartAvg3sec != DateTime.MinValue)
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP,
													devInfo.Content[i].DateStartAvg3sec,
													devInfo.Content[i].DateEndAvg3sec, 0, -1, "3 sec");
							if (devInfo.Content[i].DateStartAvg1min != DateTime.MinValue)
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP,
													devInfo.Content[i].DateStartAvg1min,
													devInfo.Content[i].DateEndAvg1min, 1, -1, "1 min");
							if (devInfo.Content[i].DateStartAvg30min != DateTime.MinValue)
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP,
													devInfo.Content[i].DateStartAvg30min,
													devInfo.Content[i].DateEndAvg30min, 2, -1, "30 min");
						}
						else
						{
							int curIndex = 0;
							if (devInfo.Content[i].DateStartAvg3sec != DateTime.MinValue)
							{
								TimeSpan diff = devInfo.Content[i].DateEndAvg3sec -
									devInfo.Content[i].DateStartAvg3sec;
								DateTime curStart = devInfo.Content[i].DateStartAvg3sec;
								do
								{
									diff = devInfo.Content[i].DateEndAvg3sec - curStart;

									DateTime curEnd = curStart.AddDays(1);
									if (curEnd > devInfo.Content[i].DateEndAvg3sec)
										curEnd = devInfo.Content[i].DateEndAvg3sec;
									obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP,
													curStart,
													curEnd, curIndex++, -1, "3 sec");
									curStart = curEnd;
								} while (diff.Days > 0);
							}
							if (devInfo.Content[i].DateStartAvg1min != DateTime.MinValue)
							{
								TimeSpan diff = devInfo.Content[i].DateEndAvg1min -
									devInfo.Content[i].DateStartAvg1min;
								DateTime curStart = devInfo.Content[i].DateStartAvg1min;
								do
								{
									diff = devInfo.Content[i].DateEndAvg1min - curStart;

									DateTime curEnd = curStart.AddDays(1);
									if (curEnd > devInfo.Content[i].DateEndAvg1min)
										curEnd = devInfo.Content[i].DateEndAvg1min;
									obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP,
													curStart,
													curEnd, curIndex++, -1, "1 min");
									curStart = curEnd;
								} while (diff.Days > 0);
							}
							if (devInfo.Content[i].DateStartAvg30min != DateTime.MinValue)
							{
								TimeSpan diff = devInfo.Content[i].DateEndAvg30min -
									devInfo.Content[i].DateStartAvg30min;
								DateTime curStart = devInfo.Content[i].DateStartAvg30min;
								do
								{
									diff = devInfo.Content[i].DateEndAvg30min - curStart;

									DateTime curEnd = curStart.AddDays(1);
									if (curEnd > devInfo.Content[i].DateEndAvg30min)
										curEnd = devInfo.Content[i].DateEndAvg30min;
									obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP,
													curStart,
													curEnd, curIndex++, -1, "30 min");
									curStart = curEnd;
								} while (diff.Days > 0);
							}
						}
					}

					// creating DNS measures
					if (devInfo.Content[i].DnsExists())
					{
						if (!splitArchive)
							obj.AddMeasure(MeasureType.DNS, EmDeviceType.ETPQP,
											devInfo.Content[i].DateStartDipSwell,
											devInfo.Content[i].DateEndDipSwell, 0, 0);
						else
						{
							int curIndex = 0;
							TimeSpan diff = devInfo.Content[i].DateEndDipSwell -
									devInfo.Content[i].DateStartDipSwell;
							DateTime curStart = devInfo.Content[i].DateStartDipSwell;
							do
							{
								diff = devInfo.Content[i].DateEndDipSwell - curStart;

								DateTime curEnd = curStart.AddDays(1);
								if (curEnd > devInfo.Content[i].DateEndDipSwell)
									curEnd = devInfo.Content[i].DateEndDipSwell;
								obj.AddMeasure(MeasureType.DNS, EmDeviceType.ETPQP,
												curStart,
												curEnd, curIndex++, 0);
								curStart = curEnd;
							} while (diff.Days > 0);
						}
					}

					// adding DateTimes to Object Name
					//DateTime dtS, dtE;
					//dtS = devInfo.Content[i].CommonBegin;
					//dtE = devInfo.Content[i].CommonEnd;
					//for (int iMTN = 0; iMTN < obj.Nodes.Count; iMTN++)
					//{
					//    for (int iMN = 0; iMN < obj.Nodes[iMTN].Nodes.Count; iMN++)
					//    {
					//        if ((obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).StartDateTime < dtS)
					//            dtS = (obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).StartDateTime;

					//        if ((obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).EndDateTime > dtE)
					//            dtE = (obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).EndDateTime;
					//    }
					//}
					//devInfo.Content[i].CommonBegin = dtS;
					//devInfo.Content[i].CommonEnd = dtE;

					obj.Text = obj.Name + " " + devInfo.Content[i].CommonBegin.ToString() + " - " +
						devInfo.Content[i].CommonEnd.ToString();

					root.Nodes.Add(obj);
				}
				//root.Check();
				root.ExpandAll();
			}
			catch (Exception ex)
			{
				EmService.WriteToLogFailed("Error in ImportDataFromContents(): " + ex.Message);
				throw;
			}
		}

		/// <summary>
		/// Imports data from contents (for ETPQP-A)
		/// </summary>
		internal void ImportDataFromContents(ref DeviceIO.DeviceCommonInfoEtPQP_A devInfo, 
			bool splitArchive)
		{
			try
			{
				// check - if Contents is empty
				if (devInfo.Content == null || devInfo.Content.Count == 0) return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode();
				root.Text = String.Format("ETPQP-A #{0} (v.{1})", devInfo.SerialNumber,
					devInfo.DevVersion);
				root.Tag = "Device";
				this.Nodes.Add(root);

				// creating childs
				for (int i = 0; i < devInfo.Content.Count; i++)
				{
					// creating object node
					ObjectTreeNode obj = new ObjectTreeNode(i, devInfo.Content[i].ObjectName,
															devInfo.Content[i].ConnectionScheme);

					// creating PQP measures
					if (devInfo.Content[i].SysInfo.Pqp_cnt > 0)
					{
						for (int iNumOfPKE = 0; iNumOfPKE < devInfo.Content[i].PqpSet.Count; 
							iNumOfPKE++)
						{
							obj.AddMeasure(MeasureType.PQP, EmDeviceType.ETPQP,
											devInfo.Content[i].PqpSet[iNumOfPKE].PqpStart,
											devInfo.Content[i].PqpSet[iNumOfPKE].PqpEnd, iNumOfPKE, 0);
						}
					}

					// creating AVG measures
					if (devInfo.Content[i].AvgExists == true)
					{
						bool exists3sec = 
							devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.ThreeSec - 1].dtStart != DateTime.MinValue
							&& 
							devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.ThreeSec - 1].dtEnd != DateTime.MinValue;
						bool exists10min = 
							devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.TenMin - 1].dtStart != DateTime.MinValue
							&& 
							devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.TenMin - 1].dtEnd != DateTime.MinValue;
						bool exists2hour = 
							devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.TwoHours - 1].dtStart != DateTime.MinValue
							&& 
							devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.TwoHours - 1].dtEnd != DateTime.MinValue;

						if (!splitArchive)
						{
							if (exists3sec)
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP_A,
											devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.ThreeSec - 1].dtStart,
											devInfo.Content[i].AvgDataEnd[(int)AvgTypes_PQP_A.ThreeSec - 1].dtEnd,
													0, -1, "3 sec");
							if (exists10min)
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP_A,
											devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.TenMin - 1].dtStart,
											devInfo.Content[i].AvgDataEnd[(int)AvgTypes_PQP_A.TenMin - 1].dtEnd,
													1, -1, "10 min");
							if (exists2hour)
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP_A,
											devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.TwoHours - 1].dtStart,
											devInfo.Content[i].AvgDataEnd[(int)AvgTypes_PQP_A.TwoHours - 1].dtEnd,
													2, -1, "2 hours");
						}
						else
						{
							int curIndex = 0;
							if (exists3sec)
							{
								DateTime dtStart = 
									devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.ThreeSec - 1].dtStart;
								DateTime dtEnd = devInfo.Content[i].AvgDataEnd[(int)AvgTypes_PQP_A.ThreeSec - 1].dtEnd;
								if (dtStart != DateTime.MinValue)
								{
									TimeSpan diff = dtEnd - dtStart;
									DateTime curStart = dtStart;
									do
									{
										diff = dtEnd - curStart;

										DateTime curEnd = curStart.AddDays(1);
										if (curEnd > dtEnd) curEnd = dtEnd;
										obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP_A,
														curStart, curEnd, curIndex++, -1, "3 sec");
										curStart = curEnd;
									} while (diff.Days > 0);
								}
							}

							if (exists10min)
							{
								DateTime dtStart = 
									devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.TenMin - 1].dtStart;
								DateTime dtEnd = devInfo.Content[i].AvgDataEnd[(int)AvgTypes_PQP_A.TenMin - 1].dtEnd;
								if (dtStart != DateTime.MinValue)
								{
									TimeSpan diff = dtEnd - dtStart;
									DateTime curStart = dtStart;
									do
									{
										diff = dtEnd - curStart;

										DateTime curEnd = curStart.AddDays(1);
										if (curEnd > dtEnd) curEnd = dtEnd;
										obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP_A,
														curStart, curEnd, curIndex++, -1, "10 min");
										curStart = curEnd;
									} while (diff.Days > 0);
								}
							}

							if (exists2hour)
							{
								DateTime dtStart = 
									devInfo.Content[i].AvgDataStart[(int)AvgTypes_PQP_A.TwoHours - 1].dtStart;
								DateTime dtEnd = devInfo.Content[i].AvgDataEnd[(int)AvgTypes_PQP_A.TwoHours - 1].dtEnd;
								if (dtStart != DateTime.MinValue)
								{
									TimeSpan diff = dtEnd - dtStart;
									DateTime curStart = dtStart;
									do
									{
										diff = dtEnd - curStart;

										DateTime curEnd = curStart.AddDays(1);
										if (curEnd > dtEnd) curEnd = dtEnd;
										obj.AddMeasure(MeasureType.AVG, EmDeviceType.ETPQP_A,
														curStart, curEnd, curIndex++, -1, "2 hours");
										curStart = curEnd;
									} while (diff.Days > 0);
								}
							}
						}
					}

					// creating DNS measures
					//if (!splitArchive)
						obj.AddMeasure(MeasureType.DNS, EmDeviceType.ETPQP_A,
										devInfo.Content[i].CommonBegin,
										devInfo.Content[i].CommonEnd, 0, 0);
					//else
					//{
					//    int curIndex = 0;
					//    TimeSpan diff = devInfo.Content[i].CommonEnd -
					//            devInfo.Content[i].CommonBegin;
					//    DateTime curStart = devInfo.Content[i].CommonBegin;
					//    do
					//    {
					//        diff = devInfo.Content[i].CommonEnd - curStart;

					//        DateTime curEnd = curStart.AddDays(1);
					//        if (curEnd > devInfo.Content[i].CommonEnd)
					//            curEnd = devInfo.Content[i].CommonEnd;
					//        obj.AddMeasure(MeasureType.DNS, EmDeviceType.ETPQP_A,
					//                        curStart, curEnd, curIndex++, 0);
					//        curStart = curEnd;
					//    } while (diff.Days > 0);
					//}

					// adding DateTimes to Object Name
					//DateTime dtS, dtE;
					//dtS = devInfo.Content[i].CommonBegin;
					//dtE = devInfo.Content[i].CommonEnd;
					//for (int iMTN = 0; iMTN < obj.Nodes.Count; iMTN++)
					//{
					//    for (int iMN = 0; iMN < obj.Nodes[iMTN].Nodes.Count; iMN++)
					//    {
					//        if ((obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).StartDateTime < dtS)
					//            dtS = (obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).StartDateTime;

					//        if ((obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).EndDateTime > dtE)
					//            dtE = (obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).EndDateTime;
					//    }
					//}
					//devInfo.Content[i].CommonBegin = dtS;
					//devInfo.Content[i].CommonEnd = dtE;

					obj.Text = obj.Name + " " + devInfo.Content[i].CommonBegin.ToString() + " - " +
						devInfo.Content[i].CommonEnd.ToString();

					root.Nodes.Add(obj);
				}
				//root.Check();
				root.ExpandAll();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ImportDataFromContents():");
				throw;
			}
		}

		/// <summary>
		/// Imports data from contents (for EM32)
		/// </summary>
		internal void ImportDataFromContents(ref DeviceIO.Em32Device device,
								bool showExisting, string pgSrvConnectStr, bool splitArchive)
		{
			try
			{
				DeviceIO.DeviceCommonInfoEm32 devInfo = device.DeviceInfo;
				//listDnsPeriods = null;

				// check - if Contents is empty
				if (!device.IsSomeArchiveExist())
					return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode();
				root.Text = String.Format("{0} #{1} (v.{2})",
					"EM32", devInfo.SerialNumber, devInfo.DevVersion);
				root.Tag = "Device";
				this.Nodes.Add(root);

				// creating object node
				ObjectTreeNode obj = new ObjectTreeNode(0, devInfo.ObjectName, devInfo.ConnectionScheme);

				// creating PQP measures
				if (devInfo.PqpExists == true)
				{
					List<DateTime> listExistArchives = new List<DateTime>();
					string commandText = string.Empty;
					DbService dbService = new DbService(pgSrvConnectStr);
					if (!dbService.Open())
					{
						EmService.WriteToLogFailed("ImportDataFromContents: unable to connect DB!");
						return;
					}
					try
					{
						commandText = String.Format("SELECT start_datetime FROM day_avg_parameter_times d INNER JOIN devices dev ON dev.dev_id = d.device_id AND dev.ser_number = {0};", devInfo.SerialNumber);
						dbService.ExecuteReader(commandText);

						while (dbService.DataReaderRead())
						{
							listExistArchives.Add((DateTime)dbService.DataReaderData("start_datetime"));
						}
					}
					finally
					{
						if (dbService != null) dbService.CloseConnect();
					}

					for (int iNumOfPKE = 0; iNumOfPKE < devInfo.PqpSet.Length; iNumOfPKE++)
					{
						bool bShow = true;
						if (!showExisting)
						{
							for (int iDate = 0; iDate < listExistArchives.Count; ++iDate)
							{
								if (devInfo.PqpSet[iNumOfPKE].PqpStart == listExistArchives[iDate])
								{
									bShow = false;
									break;
								}
							}
						}
						if (bShow)
						{
							obj.AddMeasure(MeasureType.PQP, EmDeviceType.EM32,
								devInfo.PqpSet[iNumOfPKE].PqpStart,
								devInfo.PqpSet[iNumOfPKE].PqpEnd,
								iNumOfPKE, 0);
						}
					}
				}

				// creating AVG measures
				if (devInfo.AvgExists == true)
				{
					if (!splitArchive)
					{
						if (devInfo.DateStartAvg3sec != DateTime.MinValue)
							obj.AddMeasure(MeasureType.AVG, EmDeviceType.EM32,
												devInfo.DateStartAvg3sec,
												devInfo.DateEndAvg3sec, 0, -1, "3 sec");
						if (devInfo.DateStartAvg1min != DateTime.MinValue)
							obj.AddMeasure(MeasureType.AVG, EmDeviceType.EM32,
												devInfo.DateStartAvg1min,
												devInfo.DateEndAvg1min, 1, -1, "1 min");
						if (devInfo.DateStartAvg30min != DateTime.MinValue)
							obj.AddMeasure(MeasureType.AVG, EmDeviceType.EM32,
												devInfo.DateStartAvg30min,
												devInfo.DateEndAvg30min, 2, -1, "30 min");
					}
					else
					{
						int curIndex = 0;
						if (devInfo.DateStartAvg3sec != DateTime.MinValue)
						{
							TimeSpan diff = devInfo.DateEndAvg3sec - devInfo.DateStartAvg3sec;
							DateTime curStart = devInfo.DateStartAvg3sec;
							do
							{
								diff = devInfo.DateEndAvg3sec - curStart;

								DateTime curEnd = curStart.AddDays(1);
								if (curEnd > devInfo.DateEndAvg3sec)
									curEnd = devInfo.DateEndAvg3sec;
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.EM32,
												curStart,
												curEnd, curIndex++, -1, "3 sec");
								curStart = curEnd;
							} while (diff.Days > 0);
						}
						if (devInfo.DateStartAvg1min != DateTime.MinValue)
						{
							TimeSpan diff = devInfo.DateEndAvg1min - devInfo.DateStartAvg1min;
							DateTime curStart = devInfo.DateStartAvg1min;
							do
							{
								diff = devInfo.DateEndAvg1min - curStart;

								DateTime curEnd = curStart.AddDays(1);
								if (curEnd > devInfo.DateEndAvg1min)
									curEnd = devInfo.DateEndAvg1min;
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.EM32,
												curStart,
												curEnd, curIndex++, -1, "1 min");
								curStart = curEnd;
							} while (diff.Days > 0);
						}
						if (devInfo.DateStartAvg30min != DateTime.MinValue)
						{
							TimeSpan diff = devInfo.DateEndAvg30min - devInfo.DateStartAvg30min;
							DateTime curStart = devInfo.DateStartAvg30min;
							do
							{
								diff = devInfo.DateEndAvg30min - curStart;

								DateTime curEnd = curStart.AddDays(1);
								if (curEnd > devInfo.DateEndAvg30min)
									curEnd = devInfo.DateEndAvg30min;
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.EM32,
												curStart,
												curEnd, curIndex++, -1, "30 min");
								curStart = curEnd;
							} while (diff.Days > 0);
						}
					}
				}

				// creating DNS measures
				if (devInfo.DnsExists())
				{
					//if (!splitArchive)
					obj.AddMeasure(MeasureType.DNS, EmDeviceType.EM32,
										devInfo.DateStartDipSwell,
										devInfo.DateEndDipSwell, 0, 0);
					//else
					//{
					//    int curIndex = 0;
					//    TimeSpan diff = devInfo.DateEndDipSwell -
					//            devInfo.DateStartDipSwell;
					//    DateTime curStart = devInfo.DateStartDipSwell;
					//    do
					//    {
					//        diff = devInfo.DateEndDipSwell - curStart;

					//        DateTime curEnd = curStart.AddDays(1);
					//        if (curEnd > devInfo.DateEndDipSwell)
					//            curEnd = devInfo.DateEndDipSwell;
					//        obj.AddMeasure(MeasureType.DNS, EmDeviceType.EM32,
					//                        curStart,
					//                        curEnd, curIndex++, 0);
					//        curStart = curEnd;
					//    } while (diff.Days > 0);
					//}
				}

				// adding DateTimes to Object Name
				DateTime dtS, dtE;
				dtS = DateTime.MaxValue;
				dtE = DateTime.MinValue;

				for (int iMTN = 0; iMTN < obj.Nodes.Count; iMTN++)
				{
					for (int iMN = 0; iMN < obj.Nodes[iMTN].Nodes.Count; iMN++)
					{
						if ((obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).StartDateTime < dtS)
							dtS = (obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).StartDateTime;

						if ((obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).EndDateTime > dtE)
							dtE = (obj.Nodes[iMTN].Nodes[iMN] as MeasureTreeNode).EndDateTime;
					}
				}
				if (obj.Nodes.Count > 0)
				{
					obj.Text = obj.Name + " " + dtS.ToString() + " - " + dtE.ToString();
					root.Nodes.Add(obj);
				}

				//root.Check();
				root.ExpandAll();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ImportDataFromContents():");
				throw;
			}
		}

		internal void ImportDataFromSqlImage(global::EmDataSaver.SqlImage.EmSqlDeviceImage sqlImg)
		{
			try
			{
				// check - if Contents is empty
				if (sqlImg == null) return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode();
				root.Text = String.Format("{0} #{1} (v.{2})",
					sqlImg.Name, sqlImg.SerialNumber, sqlImg.Version);
				root.Tag = "Device";
				this.Nodes.Add(root);

				// creating childs
				for (int i = 0; i < sqlImg.Archives.Length; i++)
				{
					if (sqlImg.Archives[i].Data == null)
						continue;

					// creating object node
					ObjectTreeNode obj = new ObjectTreeNode(i, sqlImg.Archives[i].ObjectName,
														sqlImg.Archives[i].ConnectionScheme);
					obj.Text = string.Format("{0} {1}-{2}",
						obj.Name, sqlImg.Archives[i].CommonBegin, sqlImg.Archives[i].CommonEnd);

					for (int j = 0; j < sqlImg.Archives[i].Data.Length; ++j)
					{
						EmSqlDataNode sqlNode = sqlImg.Archives[i].Data[j];

						// проверяем наличие данных "гармоники" и "углы и мощности гармоник"
						// (это нужно только для архивов AVG)
						byte harmonics_flag = 0;	// 1-й бит - "гармоники", 
						// 2-й бит - "мощности и углы гармоник"
						if (sqlNode.SqlType == EmSqlDataNodeType.AVG)
						{
							if (sqlNode.Sql.Contains("period_avg_params_5"))
							{
								harmonics_flag |= 0x01;		// гармоники
							}
							if (sqlNode.Sql.Contains("period_avg_params_6a") ||
								sqlNode.Sql.Contains("period_avg_params_6b"))
							{
								harmonics_flag |= 0x02;		// углы и мощности гармоник
							}
						}

						switch (sqlNode.SqlType)
						{
							case EmSqlDataNodeType.AVG:
								obj.AddMeasure(MeasureType.AVG, EmDeviceType.NONE,
									sqlNode.Begin, sqlNode.End, 0, harmonics_flag);
								break;
							case EmSqlDataNodeType.PQP:
								obj.AddMeasure(MeasureType.PQP, EmDeviceType.NONE,
									sqlNode.Begin, sqlNode.End, 0, 0);
								break;
							case EmSqlDataNodeType.Events:
								obj.AddMeasure(MeasureType.DNS, EmDeviceType.NONE,
									sqlNode.Begin, sqlNode.End, 0, 0);
								break;
						}
					}

					root.Nodes.Add(obj);
				}
				root.Check();
				root.ExpandAll();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ImportDataFromSqlImage():");
				throw;
			}
		}

		internal void ImportDataFromSqlImage(global::EmDataSaver.SqlImage.EtPQPSqlDeviceImage sqlImg)
		{
			try
			{
				// check - if Contents is empty
				if (sqlImg == null) return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode();
				root.Text = String.Format("{0} #{1} (v.{2})",
					sqlImg.Name, sqlImg.SerialNumber, sqlImg.Version);
				root.Tag = "Device";
				this.Nodes.Add(root);

				// creating childs
				for (int i = 0; i < sqlImg.Objects.Length; i++)
				{
					if (sqlImg.Objects[i].DataPQP == null && sqlImg.Objects[i].DataDNS == null &&
						sqlImg.Objects[i].DataAVG == null)
						continue;

					// creating object node
					ObjectTreeNode obj = new ObjectTreeNode(i, sqlImg.Objects[i].ObjectName,
															sqlImg.Objects[i].ConnectionScheme);
					obj.Text = string.Format("{0} {1}-{2}",
						obj.Name, sqlImg.Objects[i].CommonBegin, sqlImg.Objects[i].CommonEnd);

					// AVG
					for (int j = 0; j < sqlImg.Objects[i].DataAVG.Length; ++j)
					{
						EmSqlDataNode sqlNode = sqlImg.Objects[i].DataAVG[j];

						// проверяем наличие данных "гармоники" и "углы и мощности гармоник"
						// (это нужно только для архивов AVG)
						byte harmonics_flag = 0;	// 1-й бит - "гармоники", 
						// 2-й бит - "мощности и углы гармоник"
						if (sqlNode.SqlType == EmSqlDataNodeType.AVG)
						{
							if (sqlNode.Sql.Contains("period_avg_params_5"))
							{
								harmonics_flag |= 0x01;		// гармоники
							}
							if (sqlNode.Sql.Contains("period_avg_params_6a") ||
								sqlNode.Sql.Contains("period_avg_params_6b"))
							{
								harmonics_flag |= 0x02;		// углы и мощности гармоник
							}
						}

						obj.AddMeasure(MeasureType.AVG, EmDeviceType.NONE,
							sqlNode.Begin, sqlNode.End, 0, harmonics_flag);
					}
					// PQP
					for (int j = 0; j < sqlImg.Objects[i].DataPQP.Length; ++j)
					{
						EmSqlDataNode sqlNode = sqlImg.Objects[i].DataPQP[j];
						obj.AddMeasure(MeasureType.PQP, EmDeviceType.NONE,
							sqlNode.Begin, sqlNode.End, 0, 0);
					}
					// DNS
					for (int j = 0; j < sqlImg.Objects[i].DataDNS.Length; ++j)
					{
						EmSqlDataNode sqlNode = sqlImg.Objects[i].DataDNS[j];
						obj.AddMeasure(MeasureType.DNS, EmDeviceType.NONE,
							sqlNode.Begin, sqlNode.End, 0, 0);
					}

					root.Nodes.Add(obj);
				}
				root.Check();
				root.ExpandAll();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ImportDataFromSqlImage():");
				throw;
			}
		}

		internal void ImportDataFromSqlImage(global::EmDataSaver.SqlImage.EtPQP_A_SqlDeviceImage sqlImg)
		{
			try
			{
				// check - if Contents is empty
				if (sqlImg == null) return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode();
				if (sqlImg.Name == null) sqlImg.Name = string.Empty;
				root.Text = String.Format("{0} #{1} (v.{2})",
					sqlImg.Name, sqlImg.SerialNumber, sqlImg.Version);
				root.Tag = "Device";
				this.Nodes.Add(root);

				// creating childs
				for (int i = 0; i < sqlImg.Registrations.Length; i++)
				{
					if (sqlImg.Registrations[i].DataPQP == null && sqlImg.Registrations[i].DataDNS == null &&
						sqlImg.Registrations[i].DataAVG == null)
						continue;

					// creating object node
					ObjectTreeNode obj = new ObjectTreeNode(i, sqlImg.Registrations[i].ObjectName,
															sqlImg.Registrations[i].ConnectionScheme);
					obj.Text = string.Format("{0} {1}-{2}",
						obj.Name, sqlImg.Registrations[i].CommonBegin, sqlImg.Registrations[i].CommonEnd);

					// AVG
					for (int j = 0; j < sqlImg.Registrations[i].DataAVG.Length; ++j)
					{
						EmSqlDataNode sqlNode = sqlImg.Registrations[i].DataAVG[j];
						if (sqlNode == null) continue;

						// проверяем наличие данных "гармоники" и "углы и мощности гармоник"
						// (это нужно только для архивов AVG)
						byte harmonics_flag = 0;	// 1-й бит - "гармоники", 
						// 2-й бит - "мощности и углы гармоник"
						if (sqlNode.SqlType == EmSqlDataNodeType.AVG)
						{
							if (sqlNode.Sql.Contains("period_avg_params_5"))
							{
								harmonics_flag |= 0x01;		// гармоники
							}
							if (sqlNode.Sql.Contains("period_avg_params_6a") ||
								sqlNode.Sql.Contains("period_avg_params_6b"))
							{
								harmonics_flag |= 0x02;		// углы и мощности гармоник
							}
						}

						obj.AddMeasure(MeasureType.AVG, EmDeviceType.NONE,
							sqlNode.Begin, sqlNode.End, 0, harmonics_flag);
					}
					// PQP
					for (int j = 0; j < sqlImg.Registrations[i].DataPQP.Length; ++j)
					{
						EmSqlDataNode sqlNode = sqlImg.Registrations[i].DataPQP[j];
						if(sqlNode != null)
							obj.AddMeasure(MeasureType.PQP, EmDeviceType.NONE, sqlNode.Begin, sqlNode.End, 0, 0);
					}
					// DNS
					for (int j = 0; j < sqlImg.Registrations[i].DataDNS.Length; ++j)
					{
						EmSqlDataNode sqlNode = sqlImg.Registrations[i].DataDNS[j];
						if(sqlNode != null)
							obj.AddMeasure(MeasureType.DNS, EmDeviceType.NONE, sqlNode.Begin, sqlNode.End, 0, 0);
					}

					root.Nodes.Add(obj);
				}
				root.Check();
				root.ExpandAll();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ImportDataFromSqlImage():");
				throw;
			}
		}

		internal void ImportDataFromSqlImage(global::EmDataSaver.SqlImage.EmSqlEm32Device sqlImg)
		{
			try
			{
				// check - if Contents is empty
				if (sqlImg == null) return;

				// cleaning out old stuff
				this.Nodes.Clear();

				// creating root (device) node
				CheckTreeNode root = new CheckTreeNode();
				root.Text = String.Format("{0} #{1} (v.{2})",
					"EM32", sqlImg.SerialNumber, sqlImg.DevVersion);
				root.Tag = "Device";
				this.Nodes.Add(root);

				// creating object node
				ObjectTreeNode obj = new ObjectTreeNode(0, sqlImg.ObjectName, sqlImg.ConnectionScheme);

				// creating childs
				if (sqlImg.DataPQP != null)
				{
					for (int i = 0; i < sqlImg.DataPQP.Length; i++)
					{
						if (sqlImg.DataPQP[i] != null)
							obj.AddMeasure(MeasureType.PQP, EmDeviceType.NONE,
								sqlImg.DataPQP[i].Begin, sqlImg.DataPQP[i].End, 0, 0);
					}
				}

				if (sqlImg.DataDNO != null)
				{
					for (int i = 0; i < sqlImg.DataDNO.Length; i++)
					{
						// creating object node
						//ObjectTreeNode obj = new ObjectTreeNode(i, sqlImg.ObjectName);
						//obj.Text = string.Format("{0} {1}-{2}",
						//    obj.Name, sqlImg.DataDNO[i].Begin, sqlImg.DataDNO[i].End);

						obj.AddMeasure(MeasureType.DNS, EmDeviceType.NONE,
							sqlImg.DataDNO[i].Begin, sqlImg.DataDNO[i].End, 0, 0);
						//root.Nodes.Add(obj);
					}
				}

				if (sqlImg.DataAVG != null)
				{
					for (int i = 0; i < sqlImg.DataAVG.Length; i++)
					{
						obj.AddMeasure(MeasureType.AVG, EmDeviceType.NONE,
							sqlImg.DataAVG[i].Begin, sqlImg.DataAVG[i].End, 0, 0);
					}
				}

				if (obj.Nodes.Count > 0)
				{
					obj.Text = obj.Name; //+ " " + dtS.ToString() + " - " + dtE.ToString();
					if (obj.Text.Length == 0) obj.Text = obj.NodeName;
					root.Nodes.Add(obj);
				}
				root.Check();
				root.ExpandAll();
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in ImportDataFromSqlImage():");
				throw;
			}
		}

		internal ObjectTreeNode GetParentObject(TreeNode node)
		{
			TreeNode tempNode = node;
			while (tempNode.Parent != null)
			{
				if (tempNode.Parent is ObjectTreeNode) 
						return tempNode.Parent as ObjectTreeNode;

				tempNode = tempNode.Parent;
			}
			return null;
		}

		#endregion

		#region Overriden methods

		/// <summary>Checking/unchecking</summary>
		protected override void OnMouseUp(MouseEventArgs e)
		{
			try
			{
				if (e.Button == MouseButtons.Left && EnableMouseChecks)
				{
					TreeNode node = GetNodeAt(e.X, e.Y);
					if (node != null)
					{
						if (e.X > node.Bounds.Left - 16 && e.X < node.Bounds.Left - 3)
						{
							//node.Bounds
							if ((node as CheckTreeNode).CheckState == CheckState.Unchecked)
							{
								// implement check
								(node as CheckTreeNode).Check();
								// raising SomthingChecked event
								if (SomethingChecked != null) SomethingChecked();
							}
							else // Checked or Indeterminate
							{
								// implement uncheck
								(node as CheckTreeNode).Uncheck();
								// raising event
								if ((this.Nodes[0] as CheckTreeNode).CheckState == CheckState.Unchecked)
									if (NothingChecked != null) NothingChecked();
							}
						}

						ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);
						string textMain = rm.GetString("name_submeasure_avg_main");
						string textHarmonic = rm.GetString("name_submeasure_avg_harmonics");
						string textAngles = rm.GetString("name_submeasure_avg_angles");

						TreeNode parent = node.Parent;
						if (parent != null && parent.Nodes.Count >= 3)
						{
							if (parent.Nodes[0].ToString().IndexOf(textMain) != -1 &&
								(parent.Nodes[0] as CheckTreeNode).CheckState == CheckState.Unchecked &&
								PrevCheckState)
							{
								(parent.Nodes[1] as CheckTreeNode).Uncheck();
								(parent.Nodes[2] as CheckTreeNode).Uncheck();
							}
							if (parent.Nodes[1].ToString().IndexOf(textHarmonic) != -1 &&
								(parent.Nodes[1] as CheckTreeNode).CheckState == CheckState.Checked &&
								!PrevCheckState)
							{
								(parent.Nodes[0] as CheckTreeNode).Check();
							}
							if (parent.Nodes[2].ToString().IndexOf(textAngles) != -1 &&
								(parent.Nodes[2] as CheckTreeNode).CheckState == CheckState.Checked &&
								!PrevCheckState)
							{
								(parent.Nodes[0] as CheckTreeNode).Check();
							}
							PrevCheckState = (parent.Nodes[0] as CheckTreeNode).CheckState == CheckState.Checked;
						}
					}
				}
				base.OnMouseUp(e);
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in DeviceTreeView::OnMouseUp():");
				throw;
			}
		}

		#endregion
	}
}