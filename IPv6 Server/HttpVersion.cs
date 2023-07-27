using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace IPv6_Server {
   internal class HttpVersion {
      static async Task Main() {
         Console.WriteLine("********************");

         using HttpListener httpListener = new HttpListener();
         httpListener.Prefixes.Add($"http://+:80/");
         httpListener.Start();
         var context = await httpListener.GetContextAsync();
         var request = context.Request;
         var response = context.Response;

         string responseString;
         string hostHeader = request.Headers["Host"];

         if (hostHeader?.ToLower() == SEYMA_DOMAIN) {
            responseString = "This is seyma's domain.";
         } else if (hostHeader?.ToLower() == ORIGINAL_DOMAIN) {
            responseString = $"This is {ORIGINAL_DOMAIN}";
         } else {
            responseString = $"The host header is {hostHeader ?? "not present in the request"}";
         }


         byte[] buffer = Encoding.UTF8.GetBytes(responseString);
         response.ContentLength64 = buffer.Length;
         var output = response.OutputStream;
         output.Write(buffer, 0, buffer.Length);
         output.Close();
         httpListener.Stop();

         return;
      }
   }
}
