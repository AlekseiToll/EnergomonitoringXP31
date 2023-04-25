/// (c) Mars-Energo Ltd.
/// author : Andrew A. Golyakov 
/// 
/// Interface to all classes that contain work with memory of EM 3.3

using System;

namespace DeviceIO.MemoryInterfaces
{
	/// <summary>
	/// Interface to parse byte array to FRAM page
	/// </summary>
	public interface IParseableFRAM: IMemory
	{
		ushort Address
		{
			get;
		}

		ushort Size
		{
			get;
		}

		bool IsParsed
		{
			get;
		}
	}
}
