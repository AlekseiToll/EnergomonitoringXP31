// (c) Mars-Energo Ltd.
// 
// Description of system page of EM 3.3
// Contains such information as
//	device type
//	device number
//	device software version
//	date of the last device software update
//	nominal frequency
//  nominal line voltage
//  nominal phase voltage
//  start peak load time 
//  end peak load time 
//
// Author			:	Andrew A. Golyakov 
// Version			:	2.0.0
// Last revision	:	13.06.2006 12:56
// Version history
// 1.0.1		Class name changed
// 2.0.0		Changes for new version of flash program

using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using EmServiceLib;

namespace DeviceIO.Memory
{
	/// <summary>Device's system page</summary>
	public class EM33TPSystem: IMemory
	{
		#region Constructors

		/// <summary>Constructor</summary>
		public EM33TPSystem() { }

		#endregion

		#region Fields

		Dictionary<string, object> data = null;
		
		#endregion

		#region Properties

		/// <summary>Gets or sets inner data</summary>
		/// <remarks>Here is all possible keys with value datatypes:
		/// <list type="bullet">
		/// <item>
		/// <term><c>string DeviceName</c></term>
		/// <description>String name of the Device</description>
		/// </item>
		/// <item>
		/// <term><c>ushort DeviceNumber</c></term>
		/// <description>Serial number of the Device</description>
		/// </item>
		/// <item>
		/// <term><c>Version Version</c></term>
		/// <description>A Version of the microcode of the Device</description>
		/// </item>
		/// <item>
		/// <term><c>DateTime Date</c></term>
		/// <description>Date when microcode of the Device was chaneged</description>
		/// </item>
		/// <item>
		/// <term><c>ushort DeviceType</c></term>
		/// <description>Device type identifier. 
		/// For EM-3.3T this value always equals to 3</description>
		/// </item>
		/// <item>
		/// <term><c>ushort FNominal</c></term>
		/// <description>Nominal frequency. Can be equals only to 50 of 60 Herts</description>
		/// </item>
		/// <item>
		/// <term><c>float ULineNominal</c></term>
		/// <description>Nominal nilear voltage.</description>
		/// </item>
		/// <item>
		/// <term><c>float UPhaseNominal</c></term>
		/// <description>Nominal phase voltage</description>
		/// </item>
		/// <item>
		/// <term><c>DateTime StartPeakLoadInterval1</c></term>
		/// <description>Start of the first peek load interval</description>
		/// </item>
		/// <item>
		/// <term><c>DateTime EndPeakLoadInterval1</c></term>
		/// <description>Start of the first peek load interval</description>
		/// </item>
		/// <item>
		/// <term><c>DateTime StartPeakLoadInterval2</c></term>
		/// <description>Start of the second peek load interval</description>
		/// </item>
		/// <item>
		/// <term><c>DateTime EndPeakLoadInterval2</c></term>
		/// <description>Start of the second peek load interval</description>
		/// </item>
		/// </list>
		/// </remarks>
		public Dictionary<string, object> Data
		{
			get { return data; }
			set { data = value; }
		}

		/// <summary>Gets address of the memory</summary>
		/// <remarks>FRAM and RAM addresses only</remarks>
		public AddressMemory Address
		{
			get
			{
				AddressMemory addr = new AddressMemory();
				addr.FRAM.Address = 0x0000;
				addr.FRAM.Exists = true;
				addr.RAM.Page = 0x0e;
				addr.RAM.Shift = 0x400;
				addr.RAM.Exists = true;
				return addr;
			}
		}

		/// <summary>Gets size of this memory region IN WORDS!</summary>
		/// <remarks>NOTE! IN WORDS!</remarks>
		public ushort Size
		{
			get
			{
				return (ushort)0x0100 /*0x0200*/;
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
				data.Add("DeviceName", Conversions.bytes_2_ASCII(ref Array, 0, 16));
				data.Add("DeviceNumber", (ushort)(Array[17] * 0x100 + Array[16]));
				data.Add("Version", new Version((int)Array[19],
					Conversions.byte_2_DAA(Array[18])
					// 10
					,
					Conversions.byte_2_DAA(Array[20])));
				data.Add("Date", new DateTime(
					Conversions.bytes_2_DAA(ref Array, 22),
					Conversions.byte_2_DAA(Array[25]),
					Conversions.byte_2_DAA(Array[24])));

				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0:x}", Array[21]);
				data.Add("Version_f0", sb);

				data.Add("DeviceType", Conversions.bytes_2_ushort(ref Array, 26));
				data.Add("FNominal", Conversions.bytes_2_ushort(ref Array, 262));
				data.Add("ULineNominal", Conversions.bytes_2_float2w65536(ref Array, 264));
				data.Add("UPhaseNominal", Conversions.bytes_2_float2w65536(ref Array, 268));

				byte hh = 0; byte mm = 0;
				DateTime sdt, edt;
				hh = Conversions.byte_2_DAA(Array[249]);
				mm = Conversions.byte_2_DAA(Array[248]);
				sdt = new DateTime();
				if (hh >= 0 && hh < 24 && mm >= 0 && mm < 60)
					sdt = new DateTime(1, 1, 1, (int)hh, (int)mm, 0);
				data.Add("StartPeakLoadInterval1", sdt);

				hh = Conversions.byte_2_DAA(Array[251]);
				mm = Conversions.byte_2_DAA(Array[250]);
				edt = new DateTime();
				if (hh >= 0 && hh < 24 && mm >= 0 && mm < 60)
					edt = new DateTime(1, 1, 1, (int)hh, (int)mm, 0);
				data.Add("EndPeakLoadInterval1", edt);

				hh = Conversions.byte_2_DAA(Array[253]);
				mm = Conversions.byte_2_DAA(Array[252]);
				sdt = new DateTime();
				if (hh >= 0 && hh < 24 && mm >= 0 && mm < 60)
					sdt = new DateTime(1, 1, 1, (int)hh, (int)mm, 0);
				data.Add("StartPeakLoadInterval2", sdt);

				hh = Conversions.byte_2_DAA(Array[255]);
				mm = Conversions.byte_2_DAA(Array[254]);
				edt = new DateTime();
				if (hh >= 0 && hh < 24 && mm >= 0 && mm < 60)
					edt = new DateTime(1, 1, 1, (int)hh, (int)mm, 0);
				data.Add("EndPeakLoadInterval2", edt);

				// DEBUG
				//System.IO.FileStream f_FM_SYSTEM = new System.IO.FileStream("FM_SYSTEM.hex", System.IO.FileMode.Create);
				//f_FM_SYSTEM.Write(Array, 0, Array.Length);
				//f_FM_SYSTEM.Close();
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
