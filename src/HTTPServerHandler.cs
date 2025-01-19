using System.IO.Compression;
using System.Net.Sockets;
using System.Text;

namespace codecrafters_http_server;

public class HttpServerHandler
{
    protected static readonly List<string> SupportedEncodings = new List<string>
    {
        "gzip",
        "zyad"
    };
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
            var response = (await HandleRequest(rawRequest)).HandleBodyCompression();
            Console.WriteLine($"Sending the response:\n{response}");
            socket.Send(response.GetResponse());
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

    private async Task<HTTPRequest> HandleRequest(string rawRequest)
    {
        try
        {

            var linesOfRequest = rawRequest.Split("\r\n"); // \r\n is the CRLF
            var line0 = linesOfRequest[0].Split(" ");
            var response = new HTTPRequest()
            {
                Version = "HTTP/1.1",
                MethodOrCode = "404",
                PathOrMessage = "Not Found",
                IsResponse = true
            };
            if (line0.Length < 3)
            {
                Console.WriteLine("Invalid request line");
                response.MethodOrCode = "400";
                response.PathOrMessage = "Bad Request";
                return response;
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

            if (headers.TryGetValue("accept-encoding", out var header))
            {
                var listOfEncodings = header.Split(",",StringSplitOptions.TrimEntries);
                if (listOfEncodings.Any(x => SupportedEncodings.Contains(x)))
                {
                    response.Headers.TryAdd("Content-Encoding", header);
                }
            }

            var bodyLength = int.Parse(headers["content-length"]);
            var body = rawRequest.Split("\r\n\r\n")[1][..bodyLength]; // to skip the null characters
            var request = new HTTPRequest()
            {
                MethodOrCode = line0[0],
                PathOrMessage = line0[1],
                Version = line0[2],
                Body = Encoding.UTF8.GetBytes(body),
                Headers = headers
            };
            Console.WriteLine($"Got the request {request}");
            if (request.PathOrMessage == "/")
            {
                response.PathOrMessage = "OK";
                response.MethodOrCode = "200";
                return response;
            }

            if (request.PathOrMessage.StartsWith("/echo/"))
            {
                string msg = request.PathOrMessage[6..];
                response.PathOrMessage = "OK";
                response.MethodOrCode = "200";
                response.Headers.TryAdd("Content-Type", "text/plain");
                response.Headers.TryAdd("Content-Length", msg.Length.ToString());
                response.Body = Encoding.UTF8.GetBytes(msg);
                // var bytes = Encoding.UTF8.GetBytes(msg);
                // using var memoryStream = new MemoryStream();
                // using var gzipStream =
                //     new GZipStream(memoryStream, CompressionMode.Compress, true);
                // gzipStream.Write(bytes, 0, bytes.Length);
                // gzipStream.Flush();
                // gzipStream.Close();
                // var compressed = memoryStream.ToArray();
                // response.Body = compressed;
                // response.Headers.TryAdd("Content-Length", compressed.Length.ToString());
                // response.Headers["Content-Encoding"] = "gzip";
                return response;
            }

            if (request.PathOrMessage.StartsWith("/user-agent"))
            {
                if (headers.TryGetValue("user-agent", out var userAgentValue))
                {
                    response.PathOrMessage = "OK";
                    response.MethodOrCode = "200";
                    response.Headers.TryAdd("Content-Type", "text/plain");
                    response.Headers.TryAdd("Content-Length", userAgentValue.Length.ToString());
                    response.Body = Encoding.UTF8.GetBytes(userAgentValue);
                    return response;
                }
                response.PathOrMessage = "Bad Request";
                response.MethodOrCode = "400";
                return response;
            }
            if (request.PathOrMessage.StartsWith("/files/"))
            {
                 return await HandleFileRequestAsync(request);
            }

            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return new HTTPRequest()
            {
                IsResponse = true,
                MethodOrCode = "500",
                PathOrMessage = "Internal Server Error",
            };
        }
    }


    private async Task<HTTPRequest> HandleFileRequestAsync(HTTPRequest request)
    {
        var response = new HTTPRequest()
        {
            Version = "HTTP/1.1",
            MethodOrCode = "404",
            PathOrMessage = "Not Found",
            IsResponse = true
        };
        var dir = Environment.CurrentDirectory;
        var fileName = request.PathOrMessage.Split("/")[2];
        var pathFile = $"{dir}/{fileName}";
        if (request.MethodOrCode == "POST")
        {
            await File.WriteAllBytesAsync(pathFile, request.Body);
            response.MethodOrCode = "201";
            response.PathOrMessage = "Created";
            return response;
        }
        // the files should be next to the .exe file on the Server or the path is relative from there
        if (File.Exists(pathFile))
        {
            var contentFile = await File.ReadAllTextAsync(pathFile);
            response.MethodOrCode = "200";
            response.PathOrMessage = "OK";
            response.Headers.TryAdd("Content-Type", "application/octet-stream");
            response.Headers.TryAdd("Content-Length", contentFile.Length.ToString());
            response.Body = Encoding.UTF8.GetBytes(contentFile);
            return response;
        }
        
        return response;
    }
    
}