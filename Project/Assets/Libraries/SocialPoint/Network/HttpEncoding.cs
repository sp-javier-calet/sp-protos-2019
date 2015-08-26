using System;
using Ionic.Zlib;

public static class HttpEncoding {

    public const string Deflate = "deflate";
    public const string Gzip = "gzip";

    public static readonly string[] CompressedEncodings = {
        Deflate, Gzip
    };

    public static string DefaultBodyCompression = Gzip;

    public static bool IsCompressed(string enc)
    {
        return enc == Deflate || enc == Gzip;
    }

    public static byte[] Encode(byte[] data, string enc)
    {
        if(string.IsNullOrEmpty(enc))
        {
            return data;
        }
        else if(enc == Deflate)
        {
            return DeflateStream.CompressBuffer(data);
        }
        else if(enc == Gzip)
        {
            return GZipStream.CompressBuffer(data);
        }
        else
        {
            throw new InvalidOperationException("Unknown encoding.");
        }
    }

    public static byte[] Decode(byte[] data, string enc)
    {
        if(string.IsNullOrEmpty(enc))
        {
            return data;
        }
        else if(enc == Deflate)
        {
            return DeflateStream.UncompressBuffer(data);
        }
        else if(enc == Gzip)
        {
            return GZipStream.UncompressBuffer(data);
        }
        else
        {
            throw new InvalidOperationException("Unknown encoding.");
        }
    }
}
