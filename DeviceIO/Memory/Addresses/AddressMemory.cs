using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceIO.Memory
{
	public interface IAddressMemory
	{
		bool Exists { get; set; }
	}

	/// <summary>
	/// Address of NAND FLASH memory
	/// </summary>
	public class AddressNAND: IAddressMemory
	{
		private ushort flash = 0;
		private ushort page = 0;
		
		/// <summary>
		/// Flash number. Now always equals to zero
		/// </summary>
		public ushort Flash
		{
			get { return flash; }
			set { flash = value; }

		}

		/// <summary>
		/// Page number.
		/// </summary>
		public ushort Page
		{
			get { return page; }
			set { page = value; }
		}

		#region IAddressMemory Members

		private bool exists = false;

		/// <summary>
		/// Is this address type exists or not
		/// </summary>
		public bool Exists
		{
			get { return exists; }
			set { exists = value; }
		}

		#endregion
	}

	/// <summary>
	/// Address of FRAM
	/// </summary>
	public class AddressFRAM: IAddressMemory
	{
		private ushort address = 0;

		/// <summary>
		/// Address 
		/// </summary>
		public ushort Address
		{
			get { return address; }
			set { address = value; }
		}

		#region IAddressMemory Members

		private bool exists = false;

		/// <summary>
		/// Is this address type exists or not
		/// </summary>
		public bool Exists
		{
			get { return exists; }
			set { exists = value; }
		}

		#endregion
	}

	/// <summary>
	/// Address of RAM
	/// </summary>
	public class AddressRAM: IAddressMemory
	{
		private ushort page = 0;
		private ushort shift = 0;

		/// <summary>
		/// Page number.
		/// </summary>
		public ushort Page
		{
			get { return page; }
			set { page = value; }
		}

		/// <summary>
		/// Closed with CRC block address
		/// </summary>
		public ushort Shift
		{
			get { return shift; }
			set { shift = value; }
		}

		#region IAddressMemory Members

		private bool exists = false;

		/// <summary>
		/// Is this address type exists or not
		/// </summary>
		public bool Exists
		{
			get { return exists; }
			set { exists = value; }
		}

		#endregion
	}

	/// <summary>
	/// Set of Addresses 
	/// </summary>
	public class AddressMemory
	{
		private AddressRAM ram = new AddressRAM();
		private AddressFRAM fram = new AddressFRAM();
		private AddressNAND nand = new AddressNAND();

		/// <summary>
		/// Gets RAM address
		/// </summary>
		public AddressRAM RAM
		{
			get { return ram; }
		}

		/// <summary>
		/// Gets FRAM address
		/// </summary>
		public AddressFRAM FRAM
		{
			get { return fram; }
		}

		/// <summary>
		/// Gets NAND address
		/// </summary>
		public AddressNAND NAND
		{
			get { return nand; }
		}
	}
}
