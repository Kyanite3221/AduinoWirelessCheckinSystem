using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UDPServer {

    class Program {
        enum CommandID : byte {
            RFIDRev = 1,
            InfoReq =  2,
            GroupSend = 3,
            Ping = 4,
            Pong = 5
        }

        static void Main(string[] args) {
            UdpClient server = new UdpClient(new IPEndPoint(IPAddress.Any, 12345));
            IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);

            for (;;) {
                byte[] Bdata = server.Receive(ref sender);

                CommandID command = (CommandID)Bdata[0];

                switch (command) {
                    case CommandID.RFIDRev:
                        //code for SQL group and log storage
                        break;
                    case CommandID.InfoReq:
                        //code for SQL group read
                        byte data = 0b1111_1111;

                        using (UdpClient client = new UdpClient()) {
                            client.Connect(sender);
                            client.Send(new byte[2] { (byte)CommandID.GroupSend, data }, 2);
                            client.Close();
                        }
                        break;
                    case CommandID.Ping:
                        using (UdpClient client = new UdpClient()) {
                            client.Connect(sender);
                            client.Send(new byte[1] { (byte)CommandID.Pong }, 1);
                            client.Close();
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
