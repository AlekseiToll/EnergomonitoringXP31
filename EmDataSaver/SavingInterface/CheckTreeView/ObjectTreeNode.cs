using System;
using System.Windows.Forms;
using System.Resources;

using EmServiceLib;
using EmArchiveTree;

namespace EmDataSaver.SavingInterface.CheckTreeView
{
	/// <summary>
	/// ObjectTreeNode class. 
	/// Contains object names 
	/// (one of several main records of the device) and times of measuring
	/// </summary>
	public class ObjectTreeNode : CheckTreeNode
	{
		#region Fields

		/// <summary>
		/// Number of this object in EnergomonitorIO.ContentLines array
		/// </summary>
		private int number_;
		private ConnectScheme connectionScheme_ = ConnectScheme.Ph3W4;

		#endregion

		#region Properties

		/// <summary>
		/// Gets inner index number
		/// </summary>
		public int Number
		{
			get { return number_; }
		}

		/// <summary>
		/// Gets name
		/// </summary>
		public string NodeName
		{
			get { return this.Name; }
		}

		/// <summary>
		/// Gets or sets connection scheme
		/// </summary>
		public ConnectScheme ConnectionScheme
		{
			get { return connectionScheme_; }
			set { connectionScheme_ = value; }
		}

		#endregion

		#region Constructors

		/// <summary>Constructor</summary>
		/// <param name="Number">Inner index number</param>
		/// <param name="NodeName">Name</param>
		public ObjectTreeNode(int Number, string NodeName, ConnectScheme conSheme)
		{
			this.Tag = "Object";
			this.number_ = Number;
			this.Name = NodeName;
			this.connectionScheme_ = conSheme;
		}

		#endregion

		#region Public methods

		/// <summary>
		/// Add measure
		/// </summary>
		/// <param name="MeasureType">Type of measure</param>
		/// <param name="StartDateTime">Date and time of measure was starts</param>
		/// <param name="EndDateTime">Date and time of measure was ends</param>
		/// <param name="MeasureIndex">Inner measure index</param>
		/// <param name="AVGitems">Which AVG items exist (used only for AVG)</param>
		public void AddMeasure(MeasureType measureType, EmDeviceType devType, 
			DateTime startDateTime, DateTime endDateTime, int measureIndex, 
			int harmonics_flag)
		{
			AddMeasure(measureType, devType, startDateTime, endDateTime, measureIndex, harmonics_flag, string.Empty);
		}

		/// <summary>
		/// Add measure with additional text
		/// </summary>
		/// <param name="MeasureType">Type of measure</param>
		/// <param name="StartDateTime">Date and time of measure was starts</param>
		/// <param name="EndDateTime">Date and time of measure was ends</param>
		/// <param name="MeasureIndex">Inner measure index</param>
		/// <param name="AVGitems">Which AVG items exist (used only for AVG)</param>
		public void AddMeasure(MeasureType measureType, EmDeviceType devType, 
			DateTime startDateTime, DateTime endDateTime, int measureIndex, 
			int harmonics_flag, string addText)
		{
			try
			{
				int iNeededMeasureTypeIndex = -1;

				// searching node with needes MeasureType
				for (int i = 0; i < this.Nodes.Count; i++)
				{
					if ((this.Nodes[i] as MeasureTypeTreeNode).MeasureType == measureType)
					{
						iNeededMeasureTypeIndex = i;
					}
				}

				// if searching node no find we had to create it
				if (iNeededMeasureTypeIndex == -1)
				{
					MeasureTypeTreeNode mt_node = new MeasureTypeTreeNode(measureType);
					ResourceManager rm = new ResourceManager("EmDataSaver.emstrings", this.GetType().Assembly);

					switch (measureType)
					{
						case MeasureType.PQP:
							{
								mt_node.Text = rm.GetString("name_measure_type_pke_full");
								break;
							}
						case MeasureType.AVG:
							{
								mt_node.Text = rm.GetString("name_measure_type_avg_full");
								break;
							}
						case MeasureType.DNS:
							{
								mt_node.Text = rm.GetString("name_measure_type_dns_full");
								break;
							}
					}
					this.Nodes.Add(mt_node);
					iNeededMeasureTypeIndex = this.Nodes.Count - 1;
				}

				// adding Measure 
				MeasureTreeNode m_node = new MeasureTreeNode(startDateTime, endDateTime, measureIndex,
											measureType, devType);
				m_node.Text = startDateTime.ToString() + " - " + endDateTime.ToString();
				if (addText != string.Empty)
					m_node.Text += ("  " + addText);
				this.Nodes[iNeededMeasureTypeIndex].Nodes.Add(m_node);

				// adding SubMeasures
				if (measureType == MeasureType.AVG && harmonics_flag != -1)
				{
					m_node.Nodes.Add(new SubMeasureTreeNode(SubMeasureType.AVGMain));

					// harmonics_flag показывает существуют ли данные "гармоники" (младший бит) 
					// и "углы и мощности гармоник" (второй бит)
					if ((harmonics_flag & 0x01) != 0)
						m_node.Nodes.Add(new SubMeasureTreeNode(SubMeasureType.AVGHarmonics));
					if ((harmonics_flag & 0x02) != 0)
						m_node.Nodes.Add(new SubMeasureTreeNode(SubMeasureType.AVGAngles));
				}
			}
			catch(Exception ex)
			{
				EmService.DumpException(ex, "Exception in AddMeasure():  ");
				throw;
			}
		}

		#endregion
	}
}