using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace EmGraphLib.DNS
{
	/// <summary>
	///  Class describes the item to draw
	/// </summary>
	internal class ItemToDraw
	{
		#region Fields

		private float value = 0F;			// ��������

		private DateTime start;				// ������
		private DateTime end;				// �����

		private Phase phase;				// ����

		public Rectangle Rect;				// ����������

		private bool normal;				// ���� ����������, �������� �� ������ "����������"
				
		#endregion

		#region Properties

		/// <summary>Gets or sets value</summary>
		public float Value
		{
			get { return this.value; }
			set { this.value = value; }
		}

		/// <summary>Gets or sets phase</summary>
		public Phase Phase
		{
			get { return phase; }
			set { phase = value; }
		}

		/// <summary>Gets or sets start datetime</summary>
		public DateTime Start
		{
			get { return this.start; }
			set { this.start = value; }
		}

		/// <summary>Gets or sets end datetime</summary>
		public DateTime End
		{
			get { return this.end; }
			set { this.end = value; }
		}

		/// <summary>Gets or sets the normal flag</summary>
		public bool Normal
		{
			get { return normal; }
			set { normal = value; }
		}

		#endregion
	}
}
