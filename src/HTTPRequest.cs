namespace codecrafters_http_server;


public class HTTPRequest()
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string Version { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public string Body { get; set; }
    public override string ToString()
    {
        return $"{Method} {Version} {Path}";
    }
}