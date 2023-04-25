// (c) Mars-Energo Ltd.
//
// Memory object interface
// 
// Author			:	Andrew A. Golyakov 
// Version			:	1.0.0
// Last revision	:	27.02.2006 12:35

using System;
using System.Collections.Generic;
using System.Text;

namespace DeviceIO.Memory
{
	/// <summary>
	/// Memory object interface
	/// </summary>
	interface IMemory
	{
		Dictionary<String, Object> Data { get; }
		
		ushort Size { get; }
		AddressMemory Address { get; }
		
		bool Parse(ref byte[] Array);
		byte[] Pack();
	}
}
