
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;

namespace UnitTestProject1
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestMethod1()
		{

		}
	}
	namespace naSystem
	{
	};
	namespace MRIMClient
	{
		#region*** Идентификаторы сообщений***
			struct Msg
		{
			public const long CS_MAGIC = 0xDEADBEEF;
			public const long PROTO_VERSION = 0x10008;
			public const long MRIM_CS_HELLO = 0x1001;
			public const long MRIM_CS_HELLO_ACK = 0x1002;
			public const long MRIM_CS_LOGIN2 = 0x1038;
			public const long MRIM_CS_LOGIN_ACK = 0x1004;
			public const long MRIM_CS_LOGIN_REJ = 0x1005;
			public const long MRIM_CS_PING = 0x1006;

			public const long STATUS_OFFLINE = 0x00000000;
			public const long STATUS_ONLINE = 0x00000001;
			public const long STATUS_AWAY = 0x00000002;
			public const long STATUS_UNDETERMINATED = 0x00000003;
			public const long STATUS_FLAG_INVISIBLE = 0x80000000;
			public const long MRIM_CS_SMS = 0x1039;
		}
		#endregion

			#region*** User_Struct***
			struct User_Struct
		{
			public static string Server_IPAdress = "";
			public static int Server_Port = 0;
			public static string Login = "";
			public static string Password = "";
			public static string Nick_Name = "";
			public static string EMail_Total = "0";
			public static string EMail_UnRead = "0";
			public static int Ping_Time = 0;
			public static int Seq = 1;
			public static string User_Agent = "pymra 0.1beta"; //Идентификатор клиента
			public static string Error_String = "";
		}

		#endregion


			public class MRimClient
		{
			private const int V = 0;
			public Socket Sock;
			private static readonly byte[] vs = new byte[44];
#pragma warning disable IDE0052 // Удалить непрочитанные закрытые члены
			private readonly byte[] Buffer_Ansy = vs;
#pragma warning restore IDE0052 // Удалить непрочитанные закрытые члены

			public MRimClient(Socket sock, byte[] buffer_Ansy)
			{
				Sock = sock;
				Buffer_Ansy = buffer_Ansy ? ? throw new ArgumentNullException(nameof(buffer_Ansy));
			}



			#region*** Получает IPAdress к которому нужно подключиться***
				/// <summary>
						/// Получает IPAdress к которому нужно подключиться
						/// </summary>
				private int Get_IPAdress_Server()
			{
				try
				{
					TcpClient tc = new TcpClient("mrim.mail.ru", 2042);
					byte[] buffer = new byte[19];
					NetworkStream nss = tc.GetStream();
					nss.Read(buffer, 0, 19);
					string Buf = Encoding.ASCII.GetString(buffer).Trim();
					User_Struct.Server_IPAdress = Buf.Substring(0, Buf.IndexOf(":"));
					User_Struct.Server_Port = int.Parse(Buf.Substring(Buf.IndexOf(":") + 1, Buf.Length - Buf.IndexOf(":") - 1));
					return 0;
				}
				catch (SocketException)
				{
					return -1;
				}

			}
			#endregion

				#region*** Подключаемся к Серверу Mrim***
				/// <summary>
						/// Авторизация на сервере Mrim
						/// </summary>
				public int Login(string _Login, string _Password, long _Status)
			{
				if (Get_IPAdress_Server() != 0)
				{
					return -2;
				}
				try
				{
					TcpClient MClient = new TcpClient(User_Struct.Server_IPAdress, User_Struct.Server_Port);
					Sock = MClient.Client;
					if (Sock == null)
					{
						Console.WriteLine("Серевер недоступен!");
						return -1;
					}
					Mrim_packet_header Pack = new Mrim_packet_header(Msg.CS_MAGIC, Msg.PROTO_VERSION, User_Struct.Seq, Msg.MRIM_CS_HELLO, 0, 0, 0, 0, 0, 0, 0);
					byte[] Hello = Pack.Generat_Packet();
					Sock.Send(Hello);
					byte[] Buf = new byte[48];
					Sock.Receive(Buf);
					if (BitConverter.ToUInt32(Buf.Skip(12).Take(4).ToArray(), 0) != Msg.MRIM_CS_HELLO_ACK)
					{
						Sock.Close();
						Console.WriteLine("Серевер недоступен!");
						return -3;
					}
					System.Timers.Timer Ping_Timer = new System.Timers.Timer()


						; long j = V;
					Ping_Timer.Interval = Mrim_packet_header.Get_UL(Buf.Skip(44).ToArray(), ref j) * 100;
					Ping_Timer.Elapsed += new System.Timers.ElapsedEventHandler(Send_Ping);
					Ping_Timer.Start();
					Pack = new Mrim_packet_header(Msg.CS_MAGIC, Msg.PROTO_VERSION, User_Struct.Seq, Msg.MRIM_CS_LOGIN2, 1, 0, 0, 0, 0, 0, 0);
					Pack.Add_Date_LPS(new string[]{ _Login, _Password });
					Pack.Add_Date_UL(new long[] { _Status });
					Pack.Add_Date_LPS(new string[]{ User_Struct.User_Agent });
					byte[] vs = Pack.Generat_Packet();
					byte[] Auth = vs;
					Console.WriteLine(BitConverter.ToString(Auth));
					Sock.Send(Auth);
					Buf = new byte[48];
					Sock.Receive(Buf);
					byte[] Date_Len;
					byte[] Date;
					if (BitConverter.ToUInt32(Buf.Skip(12).Take(4).ToArray(), 0) == Msg.MRIM_CS_LOGIN_REJ)
					{
						Date_Len = new byte[4]{ Buf[16], Buf[17], Buf[18], Buf[19] };
						Date = new byte[BitConverter.ToUInt32(Date_Len, 0)];
						Sock.Receive(Date);
						return -4;
					}

					return 0;
				}
				catch (SocketException e)
				{
					Sock.Close();
					Console.WriteLine(e.Message);
					return -100;
				}
			}



			#endregion



				#region*** Отправка СМС***
				/// <summary>
						/// Отправка СМС
						/// </summary>
				public void Send_Sms(string _Phone, string _Text)
			{
				//my_header_sms = pack(formt, CS_MAGIC, PROTO_VERSION, 3, MRIM_CS_SMS, dlen) + pack('<L', 0)*6 + mydata
				Mrim_packet_header Pack = new Mrim_packet_header(Msg.CS_MAGIC, Msg.PROTO_VERSION, User_Struct.Seq, Msg.MRIM_CS_SMS, 0, 0, 0, 0, 0, 0, 0);
				Pack.Add_Date_UL(new long[] { 0 });
				Pack.Add_Date_LPS(new string[]{ _Phone, _Text });
				byte[] SMS_Send = Pack.Generat_Packet();
				Console.WriteLine(BitConverter.ToString(SMS_Send));
				Sock.Send(SMS_Send);
			}
			#endregion


				private void Send_Ping(object Sender, EventArgs args)
			{
				Console.WriteLine("Send ping");
				Mrim_packet_header Pack = new Mrim_packet_header(Msg.CS_MAGIC, Msg.PROTO_VERSION, User_Struct.Seq, Msg.MRIM_CS_PING, 0, 0, 0, 0, 0, 0, 0);
				byte[] Ping = Pack.Generat_Packet();
				lock(Sock)
				{
					Sock.Send(Ping);
					Static void.Main() {
						System.Console.WriteLine("СМС отправлено должнику");
					}
				}
			}

		}



	}
}