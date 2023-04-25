// (c) Mars-Energo Ltd.
// 
// Description of Main records region of memory of EM 3.3
//
// Author			:	Andrew A. Golyakov 
// Version			:	2.0.0
// Last revision	:	13.06.2006 15:56
// Version history
// 1.0.1		Class name changed
// 2.0.0		Changes for new version of flash program

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using EmServiceLib;

namespace DeviceIO.Memory
{
	/// <summary>
	/// All I need from FM_MainRecord
	/// </summary>
	class EM33TPMainRecord: IMemory
	{
		#region Constructors

		/// <summary>Constructor</summary>
		public EM33TPMainRecord() { }

		#endregion

		#region Fields

		private int _index;
		private Dictionary<string, object> data = null;

		#endregion

		#region Properties

		/// <summary>Gets or sets inner index</summary>
		public int Index
		{
			get { return _index; }
			set { _index = value; }
		}

		/// <summary>Gets or sets inner data</summary>
		/// <remarks>Here is all possible keys with value datatypes:
		/// <list type="bullet">
		/// <item>
		/// <term><c>string ObjName</c></term>
		/// <description>Archive's name</description>
		/// </item>
		/// <item>
		/// <term><c>ushort ConSch</c></term>
		/// <description>Connection Scheme</description>
		/// </item>
		/// <item>
		/// <term><c>float F_Nom</c></term>
		/// <description>Nominal frequency</description>
		/// </item>
		/// <item>
		/// <term><c>float U_NomLn</c></term>
		/// <description>Nominal linear voltage</description>
		/// </item>
		/// <item>
		/// <term><c>float U_NomPh</c></term>
		/// <description>Nominal phase voltage</description>
		/// </item>
		/// <item>
		/// <term><c>DateTime StartDateTime</c></term>
		/// <description>Date and time of archive's begin</description>
		/// </item>
		/// <item>
		/// <term><c>DateTime EndDateTime</c></term>
		/// <description>Date and time of archive's end</description>
		/// </item>
		/// <item>
		/// <term><c>ushort[] AddrArchPQP</c></term>
		/// <description>FRAM address's array of 
		/// Power Quality Parameters arhives</description>
		/// </item>
		/// <item>
		/// <term>ushort[] AddrArchUnF</term>
		/// <description>FRAM address's array of 
		/// Voltages and Frequencies arhives</description>
		/// </item>
		/// <item>
		/// <term><c>int[] NumOfUnfRecords</c></term>
		/// <description>Lengths of array's of 
		/// Voltages and Frequencies arhives</description>
		/// </item>
		/// <item>
		/// <term><c>ushort AddrDNOBegin</c></term>
		/// <description>FRAM address of Events 
		/// (dips and swells) arhive begin</description>
		/// </item>
		/// <item>
		/// <term><c>ushort AddrDNOEnd</c></term>
		/// <description>FRAM address of Events 
		/// (dips and swells) arhive end</description>
		/// </item>
		/// <item>
		/// <term><c>ushort AddrAVGBegin</c></term>
		/// <description>FRAM address of Avereged values 
		/// arhive begin</description>
		/// </item>
		/// <item>
		/// <term><c>ushort AddrAVGEnd</c></term>
		/// <description>FRAM address of Avereged values 
		/// arhive end</description>
		/// </item>
		/// <item>
		/// <term><c>uint NumOfAvgRecords</c></term>
		/// <description>Number of Averaged values arhive's records
		/// (table rows)</description>
		/// </item>
		/// <item>
		/// <term><c>uint NumOfDnoRecords</c></term>
		/// <description>Number of Events (dips and swells)
		/// arhive's records (table rows)</description>
		/// </item>
		/// <item>
		/// <term><c>ushort TimeOfAveragingOut</c></term>
		/// <description>Time of averaging INDEX! 
		/// <c>0</c>: 3 sec.;
		/// <c>1</c>: 1 min.;
		/// <c>2</c>: 30 min.;
		/// </description>
		/// </item>
		/// <item>
		/// <term><c>ushort CurrentTransducerIndex</c></term>
		/// <description>Current transducer index</description>
		/// </item>
		/// <item>
		/// <term><c>float Ulimit (V)</c></term>
		/// <description>Voltage limit</description>
		/// </item>
		/// <item>
		/// <term><c>float Ilimit</c></term>
		/// <description>Current limit (A)</description>
		/// </item>
		/// <item>
		/// <term><c>uint dtTimer</c></term>
		/// <description>dT count</description>
		/// </item>
		/// </list></remarks>
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
				//addr.FRAM.Address = (ushort)(0x1480 + (ushort)this.Size * 2 * _index);
				// v.2.0.0 changes:
				addr.FRAM.Address = (ushort)(0x14e0 + (ushort)this.Size * 2 * _index);
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
				//return (ushort)0x002d /*0x005A*/;
				// v.2.0.0 changes:
				return (ushort)0x0050 /*0x00A0*/;
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
			if (Array.Length != (uint)this.Size * 2) return false;

			try
			{
				data = new Dictionary<string, object>();

				string objectName = Conversions.bytes_2_ASCII(ref Array, 0, 16);
				if (objectName.Equals(String.Empty)) objectName = "default object";
				data.Add("ObjName", objectName);
				
				ushort connectionScheme = Conversions.bytes_2_ushort(ref Array, 16);
				if (connectionScheme == 4) connectionScheme = 3;
				data.Add("ConSch", connectionScheme);
				data.Add("F_Nom", Conversions.bytes_2_ushort(ref Array, 18));
				data.Add("U_NomLn", Conversions.bytes_2_float2w65536(ref Array, 20));
				data.Add("U_NomPh", Conversions.bytes_2_float2w65536(ref Array, 24));
				data.Add("StartDateTime", Conversions.bytes_2_DateTime(ref Array, 28));
				data.Add("EndDateTime", Conversions.bytes_2_DateTime(ref Array, 36));

				int iNumOfPkeRecords;
				for (iNumOfPkeRecords = 0; iNumOfPkeRecords < 8; iNumOfPkeRecords++)
				{
					if (Conversions.bytes_2_ushort(ref Array, 44 + iNumOfPkeRecords * 2) == 0)
						break;
				}
				ushort[] addrArchPKE = new ushort[iNumOfPkeRecords];
				for (int i = 0; i < iNumOfPkeRecords; i++)
				{
					addrArchPKE[i] = Conversions.bytes_2_ushort(ref Array, 44 + i * 2);
				}
				data.Add("AddrArchPQP", addrArchPKE);

				ushort[] addrArchUnF = new ushort[iNumOfPkeRecords];
				int[] numOfUnfRecords = new int[iNumOfPkeRecords];
				for (int i = 0; i < iNumOfPkeRecords; i++)
				{
					addrArchUnF[i] = Conversions.bytes_2_ushort(ref Array, 60 + i * 2);
					numOfUnfRecords[i] = Conversions.bytes_2_ushort(ref Array, 76 + i * 2);
				}
				data.Add("AddrArchUnF", addrArchUnF);
				data.Add("NumOfUnfRecords", numOfUnfRecords);
				
				data.Add("AddrDNOBegin", Conversions.bytes_2_ushort(ref Array, 92));
				data.Add("AddrDNOEnd", Conversions.bytes_2_ushort(ref Array, 94));
				data.Add("AddrAVGBegin", Conversions.bytes_2_ushort(ref Array, 96));
				data.Add("AddrAVGEnd", Conversions.bytes_2_ushort(ref Array, 98));

				data.Add("NumOfAvgRecords", 
					(uint)Conversions.bytes_2_ushort(ref Array, 100) * 0x10000 + 
					Conversions.bytes_2_ushort(ref Array, 102));
				data.Add("NumOfDnoRecords",
					(uint)Conversions.bytes_2_ushort(ref Array, 104) * 0x10000 + 
					Conversions.bytes_2_ushort(ref Array, 106));

				ushort avgType = Conversions.bytes_2_ushort(ref Array, 108);
				data.Add("TimeOfAveragingOut", avgType);
				data.Add("CurrentTransducerIndex", Conversions.bytes_2_ushort(ref Array, 110));
				data.Add("Ilimit", Conversions.bytes_2_float2w65536(ref Array, 112));
				data.Add("Ulimit", Conversions.bytes_2_float2w65536(ref Array, 116));
				data.Add("dtTimer", Conversions.bytes_2_uint(ref Array, 120));
				// fliker
				data.Add("t_fliker", Conversions.bytes_2_short(ref Array, 124));

				//uint temp = Conversions.bytes_2_uint(ref Array, 120);

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
