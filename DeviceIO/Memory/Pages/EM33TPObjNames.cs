using System;
using System.Text;
using System.IO;
using System.Collections.Generic;

using EmServiceLib;

namespace DeviceIO.Memory
{
	public abstract class EMObjNamesBase
	{
		protected List<string> listNames_ = new List<string>();

		public abstract bool Parse(ref byte[] array);
		public abstract byte[] Pack();

		public bool FillStrings(ref string[] s)
		{
			try
			{
				listNames_ = new List<string>(s.Length);
				for (int iStr = 0; iStr < s.Length; ++iStr)
					listNames_.Add(s[iStr]);
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in FillStrings()");
				listNames_ = null;
				return false;
			}
		}

		public List<string> ListNames
		{
			get { return listNames_; }
		}
	}

	public class EM33TObjNames : EMObjNamesBase
	{
		#region Fields

		public const int COUNT_NAMES = 10;

		#endregion

		#region Properties

		/// <summary>Gets address of the memory</summary>
		/// <remarks>
		/// (ushort)Address[0] - FRAM Address
		/// (ushort)Address[1] - RAM Page 
		/// (ushort)Address[2] - RAM Shift
		/// </remarks>
		public AddressMemory Address
		{
			get
			{
				AddressMemory addr = new AddressMemory();
				addr.FRAM.Address = 0x0580;
				addr.FRAM.Exists = true;
				addr.RAM.Page = 0x0e;
				addr.RAM.Shift = 0x6c0;
				addr.RAM.Exists = true;
				return addr;
			}
		}

		/// <summary>Gets size of this memory region in bytes</summary>
		public ushort Size
		{
			get
			{
				return (ushort)0x0100;
			}
		}

		#endregion

		#region Methods

		/// <summary>Parses array and fills inner object list</summary>
		/// <param name="array">byte array to parse</param>
		/// <returns>True if all OK or False</returns>
		public override bool Parse(ref byte[] array)
		{
			if (array == null) return false;
			if (array.Length != this.Size) return false; // with internal CRC

			try
			{
				listNames_ = new List<string>(COUNT_NAMES);
				for (int i = 0; i < COUNT_NAMES; i++)
				{
					listNames_.Add(Conversions.bytes_2_ASCII(ref array, 16 * i, 16));
				}
				return true;
			}
			catch(Exception ex)
			{
				EmService.DumpException(ex, "Error in EM33TObjNames::ParseEm33T()");
				listNames_ = null; 
				return false; 
			}
		}

		/// <summary>Packs all inner data into array</summary>
		public override byte[] Pack()
		{
			if (listNames_ == null) return null;

			byte[] outArray = new byte[this.Size];

			for (int i = 0; i < COUNT_NAMES; i++)
			{
				System.Text.Encoding.Default.GetBytes(
					listNames_[i], 0, listNames_[i].Length, outArray, i * 16);
			}

			// reversing
			for (int i = 0; i < this.Size - 2; i += 2)
			{
				byte Char = outArray[i];
				outArray[i] = outArray[i + 1];
				outArray[i + 1] = Char;
			}
			// fixing ASCII bug
			for (int i = 0; i < this.Size / 0x10; i++)
			{
				for (int j = 0; j < 14; j++)
				{
					if (outArray[i * 0x10 + j] >= 0xE0)
						outArray[i * 0x10 + j] -= 0x40;
					else if (outArray[i * 0x10 + j] >= 0xC0)
						outArray[i * 0x10 + j] -= 0x20;

					if (outArray[i * 0x10 + j] == 0x00) outArray[i * 0x10 + j] = 0x20;
				}
			}

			// crc block
			//ushort crc = RS232Lib.CommPort._calcCRC(outArray, (ushort)(this.Size * 2 - 2), 0, false);
			//Conversions.ushort_2_bytes(crc, ref outArray, this.Size * 2 - 2);

			return outArray;
		}

		#endregion
	}

	public class EMSLIPObjNames : EMObjNamesBase
	{
		#region Fields

		private EmDeviceType devType_;
		public readonly int COUNT_NAMES;

		#endregion

		#region Constructors

		/// <summary>Constructor</summary>
		public EMSLIPObjNames(EmDeviceType devType) 
		{
			devType_ = devType;
			if (devType_ == EmDeviceType.ETPQP_A) COUNT_NAMES = 11;
			else COUNT_NAMES = 8;
		}

		#endregion

		#region Properties

		/// <summary>Gets size of this memory region in bytes</summary>
		public ushort Size
		{
			get
			{
				return (ushort)(16 * COUNT_NAMES);
			}
		}

		#endregion

		#region Methods

		public static int GetCountNames(EmDeviceType devType)
		{
			if (devType == EmDeviceType.ETPQP_A) return 11;
			else return 8;
		}

		/// <summary>Parses array and fills inner object list</summary>
		/// <param name="array">byte array to parse</param>
		/// <returns>True if all OK or False</returns>
		public override bool Parse(ref byte[] array)
		{
			if (array == null) return false;
			if (array.Length != this.Size) return false; // with internal CRC

			try
			{
				listNames_ = new List<string>(COUNT_NAMES);
				for (int i = 0; i < listNames_.Count; i++)
				{
					listNames_[i] = Conversions.bytes_2_ASCII(ref array, 16 * i, 16);
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EMSLIPObjNames::Parse()");
				listNames_ = null;
				throw;
			}
		}

		/// <summary>Parses array and fills inner object list</summary>
		/// <param name="array">byte array to parse</param>
		/// <returns>True if all OK or False</returns>
		public bool Parse(ref string[] array)
		{
			if (array.Length != COUNT_NAMES) return false;

			try
			{
				for (int i = 0; i < array.Length; i++)
				{
					listNames_.Add(array[i]);
				}
				return true;
			}
			catch (Exception ex)
			{
				EmService.DumpException(ex, "Error in EMSLIPObjNames::Parse()");
				listNames_ = null;
				throw;
			}
		}

		/// <summary>Packs all inner data into array</summary>
		public override byte[] Pack()
		{
			if (listNames_ == null) return null;

			byte[] outArray = new byte[this.Size];
			int curShift = 0;

			for (int i = 0; i < COUNT_NAMES; i++)
			{
				byte[] tempBuf = Conversions.string_2_bytes(listNames_[i]);

				tempBuf.CopyTo(outArray, curShift);
				curShift += 16;
			}

			return outArray;
		}

		#endregion
	}

	//// <summary>All I need from RAM_OBJECTS</summary>
	//public class EM33TPObjNames : IMemory
	//{
	//    #region Constructors

	//    /// <summary>Constructor</summary>
	//    public EM33TPObjNames() { }

	//    #endregion

	//    #region Fields

	//    Dictionary<string, object> data = null;
		
	//    #endregion

	//    #region Properties

	//    /// <summary>Gets or sets inner data</summary>
	//    public Dictionary<string, object> Data
	//    {
	//        get { return data; }
	//        set { data = value; }
	//    }

	//    /// <summary>Gets address of the memory</summary>
	//    /// <remarks>
	//    /// (ushort)Address[0] - FRAM Address
	//    /// (ushort)Address[1] - RAM Page 
	//    /// (ushort)Address[2] - RAM Shift
	//    /// </remarks>
	//    public AddressMemory Address
	//    {
	//        get
	//        {
	//            AddressMemory addr = new AddressMemory();
	//            addr.FRAM.Address = 0x0580;
	//            addr.FRAM.Exists = true;
	//            addr.RAM.Page = 0x0e;
	//            addr.RAM.Shift = 0x6c0;
	//            addr.RAM.Exists = true;
	//            return addr;
	//        }
	//    }

	//    /// <summary>Gets size of this memory region IN WORDS!</summary>
	//    /// <remarks>NOTE! IN WORDS!</remarks>
	//    public ushort Size
	//    {
	//        get
	//        {
	//            return (ushort)0x0080 /*0x0100*/;
	//        }
	//    }

	//    #endregion
		
	//    #region Methods

	//    /// <summary>Parses array and fills inner object list</summary>
	//    /// <param name="Array">byte array to parse</param>
	//    /// <returns>True if all OK or False</returns>
	//    public bool Parse(ref byte[] Array)
	//    {			
	//        if (Array == null) return false;
	//        if (Array.Length != (ushort)this.Size * 2) return false; // with internal CRC

	//        try
	//        {
	//            // DEBUG
	//            //System.IO.FileStream f_FM_OBJECTS = new System.IO.FileStream("FM_OBJECTS.hex", System.IO.FileMode.Create);
	//            //f_FM_OBJECTS.Write(Array, 0, Array.Length);
	//            //f_FM_OBJECTS.Close();

	//            data = new Dictionary<string, object>();

	//            string[] objectNames = new string[10];
	//            for (int i = 0; i < objectNames.Length; i++)
	//            {
	//                objectNames[i] = Conversions.bytes_2_ASCII(ref Array, 16*i, 16);
	//            }

	//            data.Add("ObjectNames", objectNames);

	//            return true;
	//        }
	//        catch {	data = null; return false; }
	//    }

	//    /// <summary>Packs all inner data into array</summary>
	//    public byte[] Pack()
	//    {
	//        if (data == null) return null;

	//        byte[] outArray = new byte[(ushort)this.Size * 2];

	//        string[] ObjectNames = data["ObjectNames"] as string[];

	//        for (int i = 0; i < 10; i++)
	//        {
	//            System.Text.Encoding.Default.GetBytes(
	//                ObjectNames[i], 0, ObjectNames[i].Length, outArray, i * 16);
	//        }

	//        // reversing
	//        for (int i = 0; i < (ushort)this.Size * 2 - 2; i += 2)
	//        {
	//            byte Char = outArray[i];
	//            outArray[i] = outArray[i + 1];
	//            outArray[i + 1] = Char;
	//        }
	//        // fixing ASCII bug
	//        for (int i = 0; i < (ushort)this.Size * 2 / 0x10; i++)
	//        {
	//            for (int j = 0; j < 14; j++)
	//            {
	//                if (outArray[i * 0x10 + j] >= 0xE0)
	//                    outArray[i * 0x10 + j] -= 0x40;
	//                else if (outArray[i * 0x10 + j] >= 0xC0)
	//                    outArray[i * 0x10 + j] -= 0x20;

	//                if (outArray[i * 0x10 + j] == 0x00) outArray[i * 0x10 + j] = 0x20;
	//            }
	//        }

	//        // crc block
	//        //ushort crc = RS232Lib.CommPort._calcCRC(outArray, (ushort)(this.Size * 2 - 2), 0, false);
	//        //Conversions.ushort_2_bytes(crc, ref outArray, this.Size * 2 - 2);

	//        return outArray;
	//    }
		
	//    #endregion		
	//}
}
