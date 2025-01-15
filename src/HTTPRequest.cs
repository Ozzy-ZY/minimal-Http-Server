namespace codecrafters_http_server;


public class HTTPRequest()
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string Version { get; set; }
    public override string ToString()
    {
        return $"{Method} {Version} {Path}";
    }
}