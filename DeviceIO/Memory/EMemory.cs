// (c) Mars-Energo Ltd.
//
// EM 3.3 Memory types enumeration
// 
// Author			:	Andrew A. Golyakov 
// Version			:	1.0.1
// Last revision	:	27.02.2006 14:39

using System;

namespace DeviceIO.Memory
{
	/// <summary>
	/// Types of the device momore
	/// </summary>
	public enum EMemory
	{
		RAM = 0,
		FRAM = 1,
		NAND = 2
	}
}
