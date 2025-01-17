namespace codecrafters_http_server;

public static class Extentions
{
    public static string HandleBodyCompression(this HTTPRequest response)
    {
        if (!response.Headers.ContainsKey("content-encoding"))
            return response.ToString();
        else
        {
            // to do
            return response.ToString();
        }
    }
}