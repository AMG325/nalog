using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace ConsoleApp1
{
  
    
        class Program
        {
            static void Main(string[] args)
            {
                Console.WriteLine("СМС ОТПРАВЛЕНО!");
                Console.ReadLine();
            }
        }
    }
    #region ***Идентификаторы сообщений***
    struct Msg
    {
        public const long CS_MAGIC = 0xDEADBEEF;    // Клиентский Magic
        public const long PROTO_VERSION = 0x10008;    // Версия протокола
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

    #region ***User_Struct***
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

    public class mrim_packet_header
    {

        public long magic;        // Magic
        public long proto;        // Версия протокола
        public long seq;        // Sequence
        public long msg;        // Тип пакета
        public long dlen;         // Длина данных
        public long from;        // Адрес отправителя
        public long fromport;    // Порт отправителя

        public long reserved_1;    // 
        public long reserved_2;    // 
        public long reserved_3;    // 
        public long reserved_4;    // 
        public byte[] Date;//

        public static void Loger(byte[] Buf, int N)
        {
            string Mass_string = "";
            for (int i = 1; i <= N / 4; i++)
            {
                for (int j = (i * 4) - 1; j > (i * 4) - 5; j--)
                {
                    Mass_string += BitConverter.ToString(Buf, j, 1);
                }
                Console.WriteLine("0x{0}", Mass_string);
                Mass_string = "";
            }
        }

        public static byte[] Length_Hex(long _Length)
        {
            byte[] Buf = { (byte)(_Length >> 0), (byte)(_Length >> 8), (byte)(_Length >> 16), (byte)(_Length >> 24) };
            return Buf;
        }

        public static string Get_LPS(byte[] Buf, ref long J)
        {
            long B = BitConverter.ToUInt32(Buf.Take(4).ToArray(), 0);
            byte[] Buf_Text = new byte[J];
            Buf_Text = Buf.Skip(4).Take((int)(B)).ToArray();
            J += B + 4;
            return Encoding.GetEncoding("windows-1251").GetString(Buf_Text);
        }

        public static long Get_UL(byte[] Buf, ref long J)
        {
            J += 4;
            return BitConverter.ToUInt32(Buf.Take(4).ToArray(), 0);
        }

        public void Add_Date_UL(long[] Buf)
        {
            byte[] temp;
            int j;
            if (Date == null)
            {
                temp = new byte[Buf.Length * 4];
                j = 0;
            }
            else
            {
                temp = new byte[Date.Length + (Buf.Length * 4)];
                Date.CopyTo(temp, 0);
                j = Date.Length;
            }
            for (int i = 0; i < Buf.Length; i++)
            {
                Length_Hex(Buf[i]).CopyTo(temp, j);
                j += 4;
            }
            Date = new byte[temp.Length];
            temp.CopyTo(Date, 0);
        }

        public void Add_Date_LPS(string[] Buf)
        {
            int Length_LPS = 0;
            for (int i = 0; i < Buf.Length; i++)
            {
                Length_LPS += Buf[i].Length + 4;
            }
            byte[] temp;
            int j;
            if (Date == null)
            {
                temp = new byte[Length_LPS];
                j = 0;
            }
            else
            {
                temp = new byte[Date.Length + Length_LPS];
                Date.CopyTo(temp, 0);
                j = Date.Length;
            }
            for (int i = 0; i < Buf.Length; i++)
            {
                Length_Hex(Buf[i].Length).CopyTo(temp, j);
                j += 4;
                Encoding.GetEncoding("windows-1251").GetBytes(Buf[i]).CopyTo(temp, j);
                j += Encoding.GetEncoding("windows-1251").GetBytes(Buf[i]).Length;
            }
            Date = new byte[temp.Length];
            temp.CopyTo(Date, 0);
        }

        public mrim_packet_header(long _magic, long _proto, long _seq, long _msg,
                                    long _dlen, long _from, long _fromport,
                                    long _reserved_1, long _reserved_2, long _reserved_3, long _reserved_4)
        {
            magic = _magic;
            proto = _proto;
            seq = _seq;
            msg = _msg;
            dlen = _dlen;
            from = _from;
            fromport = _fromport;
            reserved_1 = _reserved_1;
            reserved_2 = _reserved_2;
            reserved_3 = _reserved_3;
            reserved_4 = _reserved_4;
            User_Struct.Seq++;
        }

        public byte[] Generat_Packet()
        {
            if (Date != null)
                dlen = Date.Length;
            byte[] Buf = {(byte)(magic >> 0), (byte)(magic >> 8), (byte)(magic >> 16), (byte)(magic >> 24),
                          (byte)(proto >> 0), (byte)(proto >> 8), (byte)(proto >> 16), (byte)(proto >> 24),
                          (byte)(seq >> 0), (byte)(seq >> 8), (byte)(seq >> 16), (byte)(seq >> 24),
                          (byte)(msg >> 0), (byte)(msg >> 8), (byte)(msg >> 16), (byte)(msg >> 24),
                          (byte)(dlen >> 0), (byte)(dlen >> 8), (byte)(dlen >> 16), (byte)(dlen >> 24),
                          (byte)(from >> 0), (byte)(from >> 8), (byte)(from >> 16), (byte)(from >> 24),
                          (byte)(fromport >> 0), (byte)(fromport >> 8), (byte)(fromport >> 16), (byte)(fromport >> 24),
                          (byte)(reserved_1 >> 0), (byte)(reserved_1 >> 8), (byte)(reserved_1 >> 16), (byte)(reserved_1 >> 24),
                          (byte)(reserved_2 >> 0), (byte)(reserved_2 >> 8), (byte)(reserved_2 >> 16), (byte)(reserved_2 >> 24),
                          (byte)(reserved_3 >> 0), (byte)(reserved_3 >> 8), (byte)(reserved_3 >> 16), (byte)(reserved_3 >> 24),
                          (byte)(reserved_4 >> 0), (byte)(reserved_4 >> 8), (byte)(reserved_4 >> 16), (byte)(reserved_4 >> 24)};
            if (Date != null)
            {
                byte[] Buf_Date = new byte[Date.Length + Buf.Length];
                Buf.CopyTo(Buf_Date, 0);
                Date.CopyTo(Buf_Date, 44);
                return Buf_Date;
            }
            return Buf;
        }
    }


    public class MRimClient
    {
        public Socket Sock;

        private byte[] Buffer_Ansy = new byte[44];



        #region ***Получает IPAdress к которому нужно подключиться***
        /// <summary>
        /// Получает IPAdress к которому нужно подключиться
        /// </summary>
        private int Get_IPAdress_Server()
        {
            string Buf = "";
            try
            {
                TcpClient tc = new TcpClient("mrim.mail.ru", 2042);
                byte[] buffer = new byte[19];
                NetworkStream nss = tc.GetStream();
                nss.Read(buffer, 0, 19);
                Buf = Encoding.ASCII.GetString(buffer).Trim();
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

        #region ***Подключаемся к Серверу Mrim***
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
                mrim_packet_header Pack = new mrim_packet_header(Msg.CS_MAGIC, Msg.PROTO_VERSION, User_Struct.Seq, Msg.MRIM_CS_HELLO, 0, 0, 0, 0, 0, 0, 0);
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
                System.Timers.Timer Ping_Timer = new System.Timers.Timer();
                long j = 0;
                Ping_Timer.Interval = mrim_packet_header.Get_UL(Buf.Skip(44).ToArray(), ref j) * 100;
                Ping_Timer.Elapsed += new System.Timers.ElapsedEventHandler(Send_Ping);
                Ping_Timer.Start();
                Pack = new mrim_packet_header(Msg.CS_MAGIC, Msg.PROTO_VERSION, User_Struct.Seq, Msg.MRIM_CS_LOGIN2, 1, 0, 0, 0, 0, 0, 0);
                Pack.Add_Date_LPS(new string[] { _Login, _Password });
                Pack.Add_Date_UL(new long[] { _Status });
                Pack.Add_Date_LPS(new string[] { User_Struct.User_Agent });
                byte[] Auth = Pack.Generat_Packet();
                Console.WriteLine(BitConverter.ToString(Auth));
                Sock.Send(Auth);
                Buf = new byte[48];
                Sock.Receive(Buf);
                byte[] Date_Len;
                byte[] Date;
                if (BitConverter.ToUInt32(Buf.Skip(12).Take(4).ToArray(), 0) == Msg.MRIM_CS_LOGIN_REJ)
                {
                    Date_Len = new byte[4] { Buf[16], Buf[17], Buf[18], Buf[19] };
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



        #region ***Отправка СМС***
        /// <summary>
        /// Отправка СМС
        /// </summary>
        public void Send_Sms(string _Phone, string _Text)
        {
            //my_header_sms = pack(formt, CS_MAGIC, PROTO_VERSION, 3, MRIM_CS_SMS, dlen) + pack('<L', 0)*6 + mydata
            mrim_packet_header Pack = new mrim_packet_header(Msg.CS_MAGIC, Msg.PROTO_VERSION, User_Struct.Seq, Msg.MRIM_CS_SMS, 0, 0, 0, 0, 0, 0, 0);
            Pack.Add_Date_UL(new long[] { 0 });
            Pack.Add_Date_LPS(new string[] { _Phone, _Text });
            byte[] SMS_Send = Pack.Generat_Packet();
            Console.WriteLine(BitConverter.ToString(SMS_Send));
            Sock.Send(SMS_Send);
        }
        #endregion


        private void Send_Ping(object Sender, EventArgs args)
        {
            Console.WriteLine("Send ping");
            mrim_packet_header Pack = new mrim_packet_header(Msg.CS_MAGIC, Msg.PROTO_VERSION, User_Struct.Seq, Msg.MRIM_CS_PING, 0, 0, 0, 0, 0, 0, 0);
            byte[] Ping = Pack.Generat_Packet();
            lock (Sock)
            {
                Sock.Send(Ping);
            }
        }
    }

