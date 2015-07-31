using System;
using System.IO;
using System.Collections.Generic;
using Ionic.Zlib;

namespace SocialPoint.Utils
{
    public enum DataCompression
    {
        None,
        Deflate,
        Gzip
    };

    public static class DataCompressionExtension
    {
        const string GzipEncoding = "gzip";
        const string DeflateEncoding = "deflate";

        public static readonly string[] Encodings = {
            GzipEncoding, DeflateEncoding
        };

        public static string ToEncoding(this DataCompression compression)
        {
            switch(compression)
            {
            case DataCompression.Gzip:
                return GzipEncoding;
            case DataCompression.Deflate:
                return DeflateEncoding;
            default:
                return string.Empty;
            }
        }

        public static DataCompression FromEncoding(string encoding)
        {
            if(encoding == GzipEncoding)
            {
                return DataCompression.Gzip;
            }
            if(encoding == DeflateEncoding)
            {
                return DataCompression.Deflate;
            }
            return DataCompression.None;
        }

    }


    public struct Data
    {
        public byte[] Bytes;

        public string String
        {
            get
            {
                return Bytes != null ? System.Text.Encoding.ASCII.GetString(Bytes) : string.Empty;
            }

            set
            {
                Bytes = System.Text.Encoding.ASCII.GetBytes(value);
            }
        }

        public int Length
        {
            get
            {
                return Bytes != null ? Bytes.Length : 0;
            }
        }

        public Data(string str)
        {
            Bytes = System.Text.Encoding.ASCII.GetBytes(str);
        }

        public Data(byte[] bytes)
        {
            Bytes = bytes;
        }

        public override string ToString()
        {
            return String;
        }

        public byte[] Range(int index, int length)
        {
            if(Bytes == null)
            {
                return new byte[0];
            }
            var blength = Bytes.Length - index;
            if(blength < length)
            {
                length = blength;
            }
            var result = new byte[length];
            Array.Copy(Bytes, index, result, 0, length);
            return result;
        }

        public string ToString(int index, int length)
        {
            return System.Text.Encoding.ASCII.GetString(Range(index, length));
        }



        public Data Compress(DataCompression comp = DataCompression.Deflate)
        {
            if(comp == DataCompression.None || Bytes == null)
            {
                return this;
            }
            byte[] bytes;
            switch(comp)
            {
            case DataCompression.Gzip:
                bytes = GZipStream.CompressBuffer(Bytes);
                break;
            default:
                bytes = DeflateStream.CompressBuffer(Bytes);
                break;
            }
            return new Data(bytes);
        }

        public Data Uncompress(DataCompression comp = DataCompression.Deflate)
        {
            if(comp == DataCompression.None || Bytes == null)
            {
                return this;
            }
            byte[] bytes;
            switch(comp)
            {
            case DataCompression.Gzip:
                bytes = GZipStream.UncompressBuffer(Bytes);
                break;
            default:
                bytes = DeflateStream.UncompressBuffer(Bytes);
                break;
            }
            return new Data(bytes);
        }
    }

}

