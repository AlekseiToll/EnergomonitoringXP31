

#if !defined RS232_H

#define	RS232_H




#include <windows.h>
#include <stdio.h>


extern int is_rs232_port_available( BYTE port );


class rs232_port
{
	public:
		rs232_port(int port);
		~rs232_port(void);
		int open(void);
		int close(void);


		void set_baudrate(DWORD new_baudrate);
		DWORD baudrate;

		BYTE bytesize;
		BYTE parity;
		BYTE stopbits2x;

		DWORD input_counter;
		DWORD output_counter;

//		BYTE *input;
		WORD pinput;
		BYTE last_rxbyte;
		DWORD last_send_counter;

		int update_modem_status(void);
		BYTE cts_state;
		BYTE dsr_state;
		BYTE dcd_state;
		BYTE ring_state;

		void purge(void);

		void set_dtr_off(void);
		void set_dtr_on(void);

		void (*dcd_gone_low_func)(void);
		void (*dcd_gone_high_func)(void);
		void (*cts_gone_low_func)(void);
		void (*cts_gone_high_func)(void);
		void (*dsr_gone_low_func)(void);
		void (*dsr_gone_high_func)(void);
		void (*ring_detected_func)(void);

		void (*input_func)(BYTE data);
		void (*port_lost_func)(void);

		BOOL port_lost;


		void send( BYTE *data, DWORD length, DWORD timeout);
		void send_overlapped( BYTE *data, DWORD length, DWORD timeout);

		void (*send_overlapped_complete_func)(void);


//	private:
		HANDLE hCOM;
		char sPort[32];
		DCB dcb;
		OVERLAPPED o;
		static void null_func(void)	{};
		static void null_input_func(BYTE data) {};
		static void null_port_lost_func(void) {};
		HANDLE MainThread;
		static DWORD threadstart0( LPVOID param )
		{
			((rs232_port*)param)->running();
			return 0;
		}
		void running(void);	

		HANDLE RxThread;
		static DWORD rxthread_start( LPVOID param )
		{
			((rs232_port*)param)->rxthread();
			return 0;
		}
		void rxthread(void);	

		HANDLE TxThread;
		static DWORD txthread_start( LPVOID param )
		{
			((rs232_port*)param)->txthread();
			return 0;
		}
		void txthread();	
		
		BYTE *txthread_data;
		DWORD txthread_length;
		DWORD txthread_timeout;
		
		void process_event( DWORD EventMask );
};




#endif
