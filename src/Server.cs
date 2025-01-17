using System.Net;
using System.Net.Sockets;
namespace codecrafters_http_server;

internal class Program
{
    private static void Main()
    {
        // You can use print statements as follows for debugging, they'll be visible when running tests.
        Console.WriteLine("Logs from your program will appear here!");

        TcpListener server = new TcpListener(IPAddress.Any, 4221);
        server.Start();
        while (true)
        {
            var socket = server.AcceptSocket(); // wait for client
            Task.Run(() => new HttpServerHandler().TakeRequest(socket));
        }
    }
}