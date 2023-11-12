using System.IO.Compression;
using System.Text;
using Google.Protobuf.Collections;

namespace Tunnlr.Client.Core.Helpers;

public static class Body
{
    public static string GetBody(this byte[] body, MapField<string, string> headers)
    {
        if (headers.TryGetValue("Content-Encoding", out var value))
        {
            switch (value.ToLower())
            {
                case "gzip":
                    return Encoding.UTF8.GetString(Decompress<GZipStream>(body));
                case "br":
                    return Encoding.UTF8.GetString(Decompress<BrotliStream>(body));
            }
        }
        return Encoding.UTF8.GetString(body);
    }
    
    /// <summary>
    /// Decompress bytes with given encoder
    /// </summary>
    /// <param name="bytes"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static byte[] Decompress<T>(byte[] bytes) where T : Stream
    {
        using var stream = (T)Activator.CreateInstance(typeof(T), new MemoryStream(bytes), CompressionMode.Decompress)!;
        const int size = 4096;
        var buffer = new byte[size];
        using var memory = new MemoryStream();
        
        int count;
        do
        {
            count = stream.Read(buffer, 0, size);
            if (count > 0)
            {
                memory.Write(buffer, 0, count);
            }
        }
        while (count > 0);
        return memory.ToArray();
    }
}