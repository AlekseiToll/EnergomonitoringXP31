#ifndef CONVERSIONS_H
#define CONVERSIONS_H

//namespace DeviceIO
//{

class Conversions
{
public:
	// 1 слово беззнаковое целое
	static WORD bytes_2_ushort(BYTE* buffer, int shift)
	{
		try 
		{
			return (WORD)(buffer[shift + 1] * 0x100 + buffer[shift]);
		}
		catch(...) {
			EmService::WriteToLogFailed("Error in bytes_2_ushort()");
			throw;
		}
	}
};

//}

#endif