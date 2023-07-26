using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Threading.Tasks;

namespace IPv6_Server {
   internal class Program {
      /**
       * This program is buggy.
       * Sometimes it loops twice for one request.
       * It nearly always loops twice.
       * Why is that? Any help is appreciated.
       **/
      async static Task Main(string[] args) {
         _ = StartClearConsole();
         const int PORT = 80;

         if (!Socket.OSSupportsIPv6) {
            Console.Error.WriteLine("Your system does not support IPv6\r\n" +
                "Check you have IPv6 enabled and have changed machine.config");
            return;
         }

         Console.WriteLine("Ctrl + c to stop the server.");
         
         for (; ; ) {
            Console.WriteLine("********************");
            using Socket listener = new Socket(
               AddressFamily.InterNetworkV6,
               SocketType.Stream,
               ProtocolType.Tcp);

            listener.Bind(new IPEndPoint(IPAddress.IPv6Any, PORT));

            listener.Listen();
            Console.WriteLine("Listening. . .");

            using Socket socket = listener.Accept();
            listener.Close();
            string remoteInfo = "";
            string remoteIP = ((IPEndPoint)socket.RemoteEndPoint)?.Address.ToString();
            
            if(!string.IsNullOrEmpty(remoteIP))
               remoteInfo += $"Your IP address is: {remoteIP}</br>";
            
            string remoteHostName = "";
            try {
               remoteHostName = Dns.GetHostEntry(IPAddress.Parse(remoteIP)).HostName;
               remoteInfo += $"Host name: {remoteHostName}</br>";
            } catch(SocketException e) {
               // I think it means that the host name is not found.
               remoteInfo += $"No associated host name found for this IP.</br>";
            }

            Console.WriteLine($"Connected: {remoteIP}  {remoteHostName}");

            byte[] buffer = new byte[2048]; // Enough for HTTP GET request but HTTP request doesn't necessarily need to fit in this buffer since i reuse the buffer while receiving the data.
            string request = "";

            for (; ; ) {

               if (socket.Available == 0) break; // No data in socket receive buffer.
               int bytesReceived = socket.Receive(buffer);

               request += Encoding.ASCII.GetString(buffer, 0, bytesReceived);


               // // Console.WriteLine(bytesReceived);
               // if (bytesReceived < buffer.Length) break;
               // if (bytesReceived == 0) break; // This doesn't work because socket.Receive() blocks the thread and never returns 0.
               // if (request.EndsWith("\r\n\r\n")) break;
            }
            Console.WriteLine("Recevied all the data.");

            //string response = $"HTTP/1.1 200 OK\r\n" +
            //    $"Content-Type: text/html; charset=UTF-8\r\n" +
            //    $"\r\n" +
            //    $"<html><body>{request}</body></html>\r\n" +
            //    $"\r\n";
            
            // string html = File.ReadAllText("./index.html");
            
            string htmlHardcoded =
               $"<!DOCTYPE html>\r\n" +
               $"<html lang=\"en\">\r\n" +
               $"<head>\r\n" +
               $"   <meta charset=\"UTF-8\">\r\n" +
               $"   <meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">\r\n" +
               $"   <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">\r\n" +
               $"   <title>apila</title>\r\n" +
               $"</head>\r\n" +
               $"\r\n" +
               $"<body style=\"background-color: #101010;\">\r\n" +
               $"   <main>\r\n" +
               $"      <h1 style=\"text-align: center; color: aliceblue;\">if you read this i von zulul</h1>\r\n" +
               $"      <img src=\"https://cdn.7tv.app/emote/6151517243b2d9da0d32a4a4/4x.webp\" alt=\"zulul\" style=\"display: block; margin-left: auto; margin-right: auto; color: aliceblue;\">\r\n" +
               $"   </main>\r\n" +
               $"   <footer>\r\n" +
               $"       <h5 style=\"color: aliceblue\">\r\n" +
               $"          {remoteInfo}\r\n" +
               $"       </h5>\r\n" +
               $"    </footer>\r\n" +
               $"</body>\r\n" +
               $"</html>\r\n";
            string response =
               "HTTP/1.1 200 OK\r\n" +
               "Connection: close\r\n" +
               "\r\n" + // end of headers
               $"{htmlHardcoded}\r\n" +
               "\r\n"; // end of message

            for (int bytesSent = 0; bytesSent < response.Length; 
               bytesSent += socket.Send(Encoding.ASCII.GetBytes(response))) 
               ;
            
            Console.WriteLine("Sent back response");

            socket.Close();
            

            Console.WriteLine("--------------------");
         }
      }

      async static Task StartClearConsole() {
         await Console.Out.WriteLineAsync("Ctrl + l to clear the screen");
         await Task.Run(() => {
            for (; ; ) {
               if (Console.KeyAvailable) {
                  var cki = Console.ReadKey(true);
                  if (cki.Modifiers.HasFlag(ConsoleModifiers.Control) && cki.Key == ConsoleKey.L) {
                     Console.Clear();
                  }
               }
            }
         });
      }
   }
}