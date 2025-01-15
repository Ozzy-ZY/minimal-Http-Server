using System.Net;
using System.Net.Sockets;
using System.Text;
using codecrafters_http_server;

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.WriteLine("Logs from your program will appear here!");

// Uncomment this block to pass the first stage
TcpListener server = new TcpListener(IPAddress.Any, 4221);
server.Start();
var socket = server.AcceptSocket(); // wait for client
var responsebuffer = new byte[1024];
var requestData = socket.Receive(responsebuffer);
var linesOfResponse = Encoding.ASCII.GetString(responsebuffer).Split("\r\n"); // \r\n is the CRLF
var line0 = linesOfResponse[0].Split(" ");
var request = new HTTPRequest()
{
    Method = line0[0],
    Path = line0[1],
    Version = line0[2]
};
var response = request.Path == "/"? $"{request.Version} 200 OK\r\n\r\n"
                                    :$"{request.Version} 404 Not Found\r\n\r\n";
socket.Send(Encoding.UTF8.GetBytes(response));
