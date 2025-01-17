using System.Net;
using System.Net.Sockets;
using System.Text;

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
            Task.Run(() => HandleRequest(socket));
        }
    }

    static Task HandleRequest(Socket socket)
    {
        try
        {
            Console.WriteLine($"Received request from {socket.RemoteEndPoint}");
            var responsebuffer = new byte[1024];
            socket.Receive(responsebuffer);
            var linesOfResponse = Encoding.ASCII.GetString(responsebuffer).Split("\r\n"); // \r\n is the CRLF
            var line0 = linesOfResponse[0].Split(" ");
            if (line0.Length < 3)
            {
                Console.WriteLine("Invalid request line");
                socket.Send("HTTP/1.1 400 Bad Request\r\n\r\n"u8.ToArray());
                return Task.CompletedTask;
            }

            var request = new HTTPRequest()
            {
                Method = line0[0],
                Path = line0[1],
                Version = line0[2]
            };
            Console.WriteLine($"Got the request {request}");
            string response  = $"{request.Version} 404 Not Found\r\n\r\n";
            if (request.Path == "/")
                response = $"{request.Version} 200 OK\r\n\r\n";
            else if (request.Path.StartsWith("/echo/"))
            {
                string msg = request.Path[6..];
                response =
                    $"{request.Version} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {msg.Length}\r\n\r\n{msg}";
            }

            else if (request.Path.StartsWith("/user-agent"))
            {
                var header =
                    linesOfResponse.First(x =>
                        x.ToLower().Contains("user-agent: ")); // finding the header from the headers
                var userAgentValue = header.Replace(header[..12], ""); // removing the header key leaving the value
                response =
                    $"{request.Version} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgentValue.Length}\r\n\r\n{userAgentValue}";
            }
            else if (request.Path.StartsWith("/files/"))
            {
                // the files should be next to the exe file on the Server or the path is relative from there
                var dir = Environment.CurrentDirectory;
                var fileName = request.Path.Split("/")[2];
                var pathFile = $"{dir}/{fileName}";
                if (File.Exists(pathFile))
                {
                    var contentFile = File.ReadAllText(pathFile);
                    response =
                        $"{request.Version} 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {contentFile.Length}\r\n\r\n{contentFile}";
                }
            }

            socket.Send(Encoding.UTF8.GetBytes(response));
            Console.WriteLine($"send the response\n{response}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {
            socket.Close();
        }
        return Task.CompletedTask;
    }
}