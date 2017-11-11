using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPServer {

    class Program {
        private static SQLHelper database = new SQLHelper();

        enum CommandID : byte {
            RFIDRev = 1,
            InfoReq =  2,
            GroupSend = 3,
            Ping = 4,
            Pong = 5
        }

        static void Main(string[] args) {
            for (; ; ) {
                try {
                    using (UdpClient server = new UdpClient(new IPEndPoint(IPAddress.Any, 12345))) {

                        IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

                        //database.LogRFID("123");

                        for (; ; ) {
                            byte[] Bdata = server.Receive(ref sender);

                            CommandID command = (CommandID)Bdata[0];

                            switch (command) {
                                case CommandID.RFIDRev:
                                    Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {sender.Address.ToString()}: RFID RECIEVED");

                                    //code for SQL group and log storage
                                    break;
                                case CommandID.InfoReq:
                                    Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {sender.Address.ToString()}: INFOREQ RECIEVED");

                                    //code for SQL group read
                                    //byte data = 0b1111_1111;

                                    using (UdpClient client = new UdpClient()) {
                                        client.Connect(sender);
                                        client.Send(new byte[2] { (byte)CommandID.GroupSend, database.ReadGroups() }, 2);
                                        client.Close();
                                    }
                                    break;
                                case CommandID.Ping:
                                    Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {sender.Address.ToString()}: PING RECIEVED");

                                    using (UdpClient client = new UdpClient()) {
                                        client.Connect(sender);
                                        client.Send(new byte[1] { (byte)CommandID.Ping }, 1);
                                        client.Close();
                                    }
                                    break;
                                case CommandID.Pong:
                                    Console.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {sender.Address.ToString()}: PONG RECIEVED");
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                }catch(Exception ex) {
                    Console.Error.WriteLine("Intern restart after: ");
                    Console.Error.WriteLine($"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToLongTimeString()}: {ex.GetType().FullName} {ex.Message}:\n{ex.StackTrace}");
                }
            }
        }
    }
}
