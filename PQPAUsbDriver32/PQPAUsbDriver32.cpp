// This is the main DLL file.

#include "stdafx.h"

#include "PQPAUsbDriver32.h"
#include "UsbDevice.h"

namespace PQPAUsbDriver32
{
	// Метод выполняет открытие соединения с подключенным прибором.
	int UsbComm::OpenConnection()
	{ 
		if (UsbConnect())
			return 0;
		else
			return -1;
	}

	// Метод выполняет закрытие соединенияпо USB.
	int UsbComm::CloseConnection()
	{   // Вызываем метод для закрытия соединения.
		UsbDisconnect();
		// Возвращаем код успешного закрытия соединения по USB.
		return 0;
	}

	// Метод реализует обмен по USB: передача заданного кол-ва байтов
	// из указанного массива и прием в указанный буфер указанного кол-ва байтов.
	int UsbComm::Exchange(unsigned char * pTx, unsigned long SendBytes,
		                  unsigned char * pRx, unsigned long &ReadBytes,
                          unsigned int WaitBytes)
	{   
		// Возвращаем код успешного обмена с прибором.
		return 0;
	}
	// Метод выполняет передачу байта в порт.
	int UsbComm::WriteByte(unsigned char * pTx, unsigned long SendBytes)
	{   // Вызываем метод записи байта в порт.
		if (SendData(pTx, SendBytes, 500))
			return 0;
		else
			return -2;
	}
	// Метод выполняет чтение байта из порта.
	int UsbComm::ReadByte(unsigned char * pRx, unsigned long ReadBytes, int timeout)
	{   // Вызываем метод для чтения байта.
		return ReadData(pRx, timeout);
	}

}