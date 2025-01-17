using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

public class HttpServerHandler
{
    public Task TakeRequest(Socket socket)
    {
        try
        {
            Console.WriteLine($"Received request from {socket.RemoteEndPoint}");
            var requestBuffer = new byte[1024];
            socket.Receive(requestBuffer);
            string rawRequest = Encoding.UTF8.GetString(requestBuffer);
            string response = HandleRequest(rawRequest);
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

    private string HandleRequest(string rawRequest)
    {
        try
        {

            var linesOfResponse = rawRequest.Split("\r\n"); // \r\n is the CRLF
            var line0 = linesOfResponse[0].Split(" ");
            if (line0.Length < 3)
            {
                Console.WriteLine("Invalid request line");
                return "HTTP/1.1 400 Bad Request\r\n\r\n";
            }

            var request = new HTTPRequest()
            {
                Method = line0[0],
                Path = line0[1],
                Version = line0[2]
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
                var header =
                    linesOfResponse.FirstOrDefault(x =>
                        x.ToLower().Contains("user-agent: ")); // finding the header from the headers
                if (header == null)
                    return "HTTP/1.1 400 Bad Request\r\n\r\n";
                
                var userAgentValue = header.Replace(header[..12], ""); // removing the header key leaving the value
                return
                    $"{request.Version} 200 OK\r\nContent-Type: text/plain\r\nContent-Length: {userAgentValue.Length}\r\n\r\n{userAgentValue}";
            }
            if (request.Path.StartsWith("/files/"))
            {
                 return HandleFileRequest(request);
            }
            
            return $"{request.Version} 404 Not Found\r\n\r\n";
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return "HTTP/1.1 500 Internal Server Error\r\n\r\n";
        }
    }
    private string HandleFileRequest(HTTPRequest request)
    {
        string response = $"{request.Version} 404 Not Found\r\n\r\n";
        // the files should be next to the .exe file on the Server or the path is relative from there
        var dir = Environment.CurrentDirectory;
        var fileName = request.Path.Split("/")[2];
        var pathFile = $"{dir}/{fileName}";
        if (File.Exists(pathFile))
        {
            var contentFile = File.ReadAllText(pathFile);
            response =
                $"{request.Version} 200 OK\r\nContent-Type: application/octet-stream\r\nContent-Length: {contentFile.Length}\r\n\r\n{contentFile}";
            return response;
        }
        
        return response;
    }
}