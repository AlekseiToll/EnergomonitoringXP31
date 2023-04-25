// (c) Mars-Energo Ltd.
// 
// Description of Bad blocks region of memory of EM 3.3
//
// Author			:	Andrew A. Golyakov 
// Version			:	1.0.1
// Last revision	:	27.02.2006 14:56
// Version history
// 1.0.1		Class name changed

using System;
using System.IO;
using System.Collections.Generic;

using EmServiceLib;

namespace DeviceIO.Memory
{
	/// <summary>
	/// Bad Blocks info (array of the blocks addresses which marks as BAD)
	/// </summary>
	public class EM33TPBadBlocks: IMemory
	{
		#region Fields

		private Dictionary<string, object> data = null;

		#endregion

		#region Constructor

		public EM33TPBadBlocks() { }

		#endregion

		#region Properties

		/// <summary>Gets or sets inner data</summary>
		public Dictionary<string, object> Data
		{
			get { return data; }
		}

		/// <summary>Gets size of this memory region IN WORDS!</summary>
		/// <remarks>NOTE! IN WORDS!</remarks>
		public ushort Size
		{
			get
			{
				return (ushort)0x0010 /*0x0020*/;
			}
		}

		/// <summary>Gets address of the memory</summary>
		public AddressMemory Address
		{
			get 
			{
				AddressMemory addr = new AddressMemory();
				addr.FRAM.Address = 0x1400;
				addr.FRAM.Exists = true;
				return addr;
			}
		}
		
		#endregion

		#region Methods

		/// <summary>Parses array and fills inner object list</summary>
		/// <param name="Array">byte array to parse</param>
		/// <returns>True if all OK or False</returns>
		public bool Parse(ref byte[] Array)
		{
			try
			{
				data = new Dictionary<string, object>();

				if (Array == null) return false;
				if (Array.Length != 32) return false;
				int i;
				for (i = 0; i < 10; i++)
					if (Conversions.bytes_2_ushort(ref Array, i * 2) == 0x0000)
						break;
				ushort[] badBlocks = new ushort[i];
				for (int j = 0; j < i; j++)
					badBlocks[j] = Conversions.bytes_2_ushort(ref Array, j * 2);

				data.Add("BadBlocks", badBlocks);

				// DEBUG
				//System.IO.FileStream f_FM_SYSTEM = new System.IO.FileStream(
				//	"FM_BadBlocks.hex", System.IO.FileMode.Create);
				//f_FM_SYSTEM.Write(Array, 0, Array.Length);
				//f_FM_SYSTEM.Close();

				return true;
			}
			catch { data = null; return false; }
		}

		/// <summary>Not implemented!</summary>
		public byte[] Pack()
		{
			throw new EmException("The method or operation is not implemented.");
		}

		#endregion
	}
}
