/// (c) Mars-Energo Ltd.
/// author : Andrew A. Golyakov 
/// 
/// Interface to all classes that contain work with memory of EM 3.3

using System;

namespace EnergoMonitorIO
{
	/// <summary>
	/// Interface to pack RAM region into array
	/// </summary>
	public interface IPackableRAM
	{
		byte[] Pack();

		ushort Page
		{

			get;
		}

		ushort Shift
		{
			get;
		}

		ushort Size
		{
			get;
		}
	}
}
