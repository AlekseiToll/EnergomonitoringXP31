// (c) Mars-Energo Ltd.
// 
// Description of memory map of EM 3.3
// Contains pointers to top addresses to
//	PQP archives array (8 elements) 
//	AVG archive
//	Events (Dips and Swells) archive
//
// Author			:	Andrew A. Golyakov 
// Version			:	2.0.0
// Last revision	:	13.06.2006 11:21
// Version history
// 1.0.1		Class name changed
// 1.0.2		Data keys were renamed
// 2.0.0		Memory map changes

using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using EmServiceLib;

namespace DeviceIO.Memory
{
	/// <summary>All I need from </summary>
	public class EM33TPTopPtrs : IMemory
	{
		#region Constructors

		/// <summary>Constructor</summary>
		public EM33TPTopPtrs() { }

		#endregion

		#region Fields

		Dictionary<string, object> data = null;

		#endregion

		#region Properties

		/// <summary>Gets or sets inner data</summary>
		/// <remarks>Here is all possible keys with value datatypes:
		/// <list type="bullet">
		/// <item>
		/// <term><c>ushort[8] ArchPQP</c></term>
		/// <description>Power Quality Parameters
		/// archive's addresses array (8 elements)</description>
		/// </item>
		/// <item>
		/// <term><c>ushort[8] ArchUnF</c></term>
		/// <description>Voltage and Frequency
		/// archive's addresses array (8 elements) </description>
		/// </item>
		/// <item>
		/// <term><c>ushort ArchEvents</c></term>
		/// <description>Events arhive's address</description>
		/// </item>
		/// <item>
		/// <term><c>ushort ArchAVG</c></term>
		/// <description>Averaged values archive's address</description>
		/// </item>		
		/// </list>
		/// </remarks>
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
				addr.FRAM.Address = 0x1420;
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
				return (ushort)0x0020 /*0x0040*/;
				//v.2.0.0 changes
				//return (ushort)0x0010 /*0x0020*/;
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
			if (Array.Length != (ushort)this.Size * 2) return false;

			try
			{
				data = new Dictionary<string, object>();
								
				ushort[] archPKE =  new ushort[8];
				for (int i = 0; i < 8; i++)
				{
					archPKE[i] = Conversions.bytes_2_ushort(ref Array, i * 2);
				}
				data.Add("ArchPQP", archPKE);
				//data.Add("ArchPQP", archPKE);
				//data.Add("ArchEvents", Conversions.bytes_2_ushort(ref Array, 16));
				//data.Add("ArchAVG", Conversions.bytes_2_ushort(ref Array, 18));
				//v.2.0.0 changes:
				ushort[] archUnF =  new ushort[8];
				for (int i = 0; i < 8; i++)
				{
					archUnF[i] = Conversions.bytes_2_ushort(ref Array, 16 + i * 2);
				}
				data.Add("ArchUnF", archUnF);
				data.Add("ArchEvents", Conversions.bytes_2_ushort(ref Array, 32));
				data.Add("ArchAVG", Conversions.bytes_2_ushort(ref Array, 34));
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