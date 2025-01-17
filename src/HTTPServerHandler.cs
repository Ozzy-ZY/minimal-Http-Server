using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

public class HttpServerHandler
{
    private Dictionary<string, string> ParseHeaders(string headers)
    {
        var headersDictionary = new Dictionary<string, string>();
        var lines = headers.Split("\r\n");
        for (int i = 0; i < lines.Length; i++)
        {
            for (int j = 0; j < lines[i].Length; j++)
            {// find the first : and then extract the key and value then end the iteration
                if (lines[i][j] == ':')
                {
                    var key = lines[i][..j].ToLower();
                    var value = lines[i][(j + 1)..];
                    value = value.Trim('\r', '\n', ' ');
                    headersDictionary.TryAdd(key, value);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Found header: {key}={value}");
                    Console.ResetColor();
                    break;
                }
            }
        }
        return headersDictionary;
    }
    public async Task<Task> TakeRequest(Socket socket)
    {
        try
        {
            Console.WriteLine($"Received request from {socket.RemoteEndPoint}");
            var requestBuffer = new byte[1024];
            socket.Receive(requestBuffer);
            string rawRequest = Encoding.UTF8.GetString(requestBuffer);
            string response = await HandleRequest(rawRequest);
            Console.WriteLine($"Sending the response: {response}");
            socket.Send(Encoding.UTF8.GetBytes(response));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            socket.Close();
        }
        return Task.CompletedTask;
    }

    private async Task<string> HandleRequest(string rawRequest)
    {
        try
        {

            var linesOfRequest = rawRequest.Split("\r\n"); // \r\n is the CRLF
            var line0 = linesOfRequest[0].Split(" ");
            if (line0.Length < 3)
            {
                Console.WriteLine("Invalid request line");
                return "HTTP/1.1 400 Bad Request\r\n\r\n";
            }
            StringBuilder headersAsString = new StringBuilder();
            foreach (var line in linesOfRequest.Skip(1))
            {
                if (line == "")
                {
                    break;
                }
                headersAsString.AppendLine(line);
            }
            var headers = ParseHeaders(headersAsString.ToString());
            var bodyLength = int.Parse(headers["content-length"]);
            var body = rawRequest.Split("\r\n\r\n")[1][..bodyLength]; // to skip the null characters
            var request = new HTTPRequest()
            {
                Method = line0[0],
                Path = line0[1],
                Version = line0[2],
                Body = body,
                Headers = headers
            };
            Console.WriteLine($"Got the request {request}");
            if (request.Path == "/")
                return $"{request.Version} 200 OK\r\n\r\n";
            
            if (request.Path.StartsWith("/echo/"))
            {
                string msg = request.Path[6..];
                return 
                    $"{request.Version} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {msg.Length}\r\n\r\n{msg}";
            }

            if (request.Path.StartsWith("/user-agent"))
            {
                if (headers.TryGetValue("user-agent", out var userAgentValue))
                {
                    return
                        $"{request.Version} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgentValue.Length}\r\n\r\n{userAgentValue}";
                }
                return "HTTP/1.1 400 Bad Request\r\n\r\n";
            }
            if (request.Path.StartsWith("/files/"))
            {
                 return await HandleFileRequestAsync(request);
            }
            
            return $"{request.Version} 404 Not Found\r\n\r\n";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "HTTP/1.1 500 Internal Server Error\r\n\r\n";
        }
    }
    private async Task<string> HandleFileRequestAsync(HTTPRequest request)
    {
        string response = $"{request.Version} 404 Not Found\r\n\r\n";

        var dir = Environment.CurrentDirectory;
        var fileName = request.Path.Split("/")[2];
        var pathFile = $"{dir}/{fileName}";
        if (request.Method == "POST")
        {
            await File.WriteAllTextAsync(pathFile, request.Body);
            response = "HTTP/1.1 201 Created\r\n\r\n";
            return response;
        }
        // the files should be next to the .exe file on the Server or the path is relative from there
        if (File.Exists(pathFile))
        {
            var contentFile = await File.ReadAllTextAsync(pathFile);
            response =
                $"{request.Version} 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {contentFile.Length}\r\n\r\n{contentFile}";
            return response;
        }
        
        return response;
    }
}