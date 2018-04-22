using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using S22.Sasl;
using System.Threading.Tasks;

namespace xmpp_consoletest
{
    class Program
    {
        static void Main(string[] args)
        {
            Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("rooms.chinwag.im", 5222);
            //String req = "<?xml version='1.0'?><stream:stream xmlns=\"jabber:client\" version=\"1.0\" xmlns:stream=\"http://etherx.jabber.org/streams\" from=\"pauliunas@404.city\" to=\"chinwag.im\" xml:lang=\"en\">";
            
            string msg = "<?xml version='1.0'?><stream:stream xmlns=\"jabber: client\" version=\"1.0\" xmlns:stream=\"http://etherx.jabber.org/streams\" from=\"pauliunas@404.city\" to=\"chinwag.im\" xml:lang=\"en\">";
            byte[] msgb = Encoding.ASCII.GetBytes(msg);
            socket.Send(msgb);
            byte[] recv = new byte[2000];
            int len = socket.Receive(recv);
            Console.WriteLine(Encoding.ASCII.GetString(recv, 0, len));

            string msg1 = "<starttls xmlns='urn:ietf:params:xml:ns:xmpp-tls' />";
            byte[] msgb1 = Encoding.ASCII.GetBytes(msg1);
            socket.Send(msgb1);
            len = socket.Receive(recv);
            Console.WriteLine(Encoding.ASCII.GetString(recv, 0, len));

            using (NetworkStream netStream = new NetworkStream(socket))
            using (SslStream stream = new SslStream(netStream))
            {
                stream.AuthenticateAsClient("rooms.chinwag.im");
                stream.Write(msgb);

                len = stream.Read(recv, 0, recv.Length);
                string resp = Encoding.ASCII.GetString(recv, 0, len);
                Console.WriteLine("SSL Response: " + resp);

                if (resp.IndexOf("<mechanism>SCRAM-SHA-1</mechanism>") == -1)
                    return;

                SaslMechanism sasl = SaslFactory.Create("Scram-Sha-1");
                sasl.Properties.Add("Username", "testuser123456");
                sasl.Properties.Add("Password", "testpassword");

                string initial = sasl.GetResponse("");

                msgb1 = Encoding.ASCII.GetBytes("<auth xmlns=\"urn:ietf:params:xml:ns:xmpp-sasl\" mechanism=\"SCRAM-SHA-1\">" + initial + "</auth>");
                stream.Write(msgb1, 0, msgb1.Length);
                len = stream.Read(recv, 0, recv.Length);
                resp = Encoding.ASCII.GetString(recv, 0, len);


                string req;
                byte[] bytes;
                int length;
                while (socket.Connected)
                {
                    req = Console.ReadLine();
                    bytes = Encoding.ASCII.GetBytes(req);
                    stream.Write(bytes, 0, bytes.Length);
                    bytes = new byte[2000];
                    length = stream.Read(bytes, 0, bytes.Length);
                    Console.WriteLine("Response: " + Encoding.ASCII.GetString(bytes, 0, length));
                }
            }

            

        }
    }
}
