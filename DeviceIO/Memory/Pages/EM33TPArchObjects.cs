// (c) Mars-Energo Ltd.
// 
// Description of Archive objects region of memory of EM 3.3
// Contains
//	real number of Main Records
//	index of current Main Record in buffer of 32 main records
//	index of current PQP Record in buffer of 8 main records
//
// Author			:	Andrew A. Golyakov 
// Version			:	1.0.2
// Last revision	:	09.03.2006 14:59
// Version history
// 1.0.1		Class name changed
// 1.0.2		Methods was changed

using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using EmServiceLib;

namespace DeviceIO.Memory
{
	/// <summary>All I need from </summary>
	public class EM33TPArchObjects: IMemory
	{
		#region Constructors

		/// <summary>Constructor</summary>
		public EM33TPArchObjects() { }

		#endregion

		#region Fields

		Dictionary<string, object> data = null;
		
		#endregion

		#region Properties

		/// <summary>Gets or sets inner data</summary>
		public Dictionary<string, object> Data
		{
			get { return data; }
			set { data = value; }
		}

		/// <summary>Gets address of the memory</summary>
		/// <remarks>FRAM address only</remarks>
		public AddressMemory Address
		{
			get
			{
				AddressMemory addr = new AddressMemory();
				addr.FRAM.Address = 0x0200;
				addr.FRAM.Exists = true;
				return addr;
			}
		}

		/// <summary>Gets size of this memory region IN WORDS!</summary>
		/// <remarks>NOTE! IN WORDS!</remarks>
		public ushort Size
		{
			get
			{
				return 0x0090 /*0x0120*/;
			}
		}

		#endregion
		
		#region Methods

		/// <summary>Parses array and fills inner object list</summary>
		/// <param name="Array">byte array to parse</param>
		/// <returns>True if all OK or False</returns>
		public bool Parse(ref byte[] Array)
		{			
			if (Array == null) return false;
			if (Array.Length != (ushort)this.Size * 2) return false; // with internal CRC

			try
			{
				data = new Dictionary<string, object>();
				data.Add("MainRecordsCount", Conversions.bytes_2_ushort(ref Array, 262));
				data.Add("CurrentMainRecIndex", Conversions.bytes_2_ushort(ref Array, 258));
				data.Add("CurrentPQPIndex", Conversions.bytes_2_ushort(ref Array, 260));

				return true;
			}
			catch {	data = null; return false; }
		}

		/// <summary>Not implemented!</summary>
		public byte[] Pack()
		{
			throw new EmException("The method or operation is not implemented.");
		}
		
		#endregion
	}  
}