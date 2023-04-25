/// (c) Mars-Energo Ltd.
/// author : Andrew A. Golyakov 
/// 
/// Interface to all classes that contain work with memory of EM 3.3

using System;

namespace EnergoMonitorIO
{
	/// <summary>
	/// Interface to pack FRAM region into array
	/// </summary>
	public interface IPackableFRAM
	{
		byte[] Pack();

		ushort Address
		{
			get;
		}

		ushort Size
		{
			get;
		}
	}
}
