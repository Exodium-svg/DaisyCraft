using System.Net;

internal class Program
{
    public static void Main(string[] args) => AsyncMain().Wait();

    public static async Task AsyncMain()
    {
        HttpListener listener = new();
        listener.Prefixes.Add("https://localhost:443/");

        listener.Start();

        while (listener.IsListening)
        {
            HttpListenerContext ctx = await listener.GetContextAsync();

            Console.WriteLine(ctx.Request.Url);
        }
    }
}