// PQPAUsbDriver32.h

#pragma once

namespace PQPAUsbDriver32 
{
	public ref class UsbComm
	{
		// TODO: Add your methods for this class here.
		private:
			// Номер устрйоства, с которым работает драйвер.
			int ActiveDev;

		public:
			// Метод открывает соединение по интерфейсу USB.
			int OpenConnection();
			// Метод закрывает соединение по интерфейсу USB.
			int CloseConnection();
			// Метод реализует обмен по интерфейсу USB.
			int Exchange(unsigned char * pTx, unsigned long SendBytes,
                         unsigned char * pRx, unsigned long &ReadBytes,
                         unsigned int WaitBytes);
			int WriteByte(unsigned char * pTx, unsigned long SendBytes);
			int ReadByte(unsigned char * pRx, unsigned long ReadBytes, int timeout);
	};
}
