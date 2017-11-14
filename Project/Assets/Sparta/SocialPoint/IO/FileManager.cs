
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading;

namespace SocialPoint.IO
{
    public class ReadHandler : IDisposable
    {
        Stream _stream;

        public Stream Stream
        {
            get
            {
                return _stream;
            }
        }

        IReader _reader;

        public IReader Reader
        {
            get
            {
                if(_reader == null)
                {
                    _reader = new SystemBinaryReader(_stream);
                }
                return _reader;
            }
        }

        public ReadHandler(Stream stream)
        {
            _stream = stream;   
        }

        public void Close()
        {
            _stream.Close();    
        }

        public void Dispose()
        {
            Close();
        }
    }

    public class WriteHandler : IDisposable
    {
        Stream _stream;

        public Stream Stream
        {
            get
            {
                return _stream;
            }
        }

        IWriter _writer;

        public IWriter Writer
        {
            get
            {
                if(_writer == null)
                {
                    _writer = new SystemBinaryWriter(_stream);
                }
                return _writer;
            }
        }

        public WriteHandler(Stream stream)
        {
            _stream = stream;   
        }

        public void CloseStream()
        {
            _stream.Close();    
        }

        public void Dispose()
        {
            CloseStream();
        }
    }

    public interface IFileManager
    {
        ReadHandler Read(string asset);
        WriteHandler Write(string asset);
    }

    public class StandaloneFileManager : IFileManager
    {
        public ReadHandler Read(string asset)
        {
            FileStream stream = new FileStream(asset, FileMode.Open, FileAccess.Read, FileShare.Read);
            return new ReadHandler(stream);
        }

        public WriteHandler Write(string asset)
        {
            FileUtils.CreateDirectory(Path.GetDirectoryName(asset));
            var stream = new FileStream(asset, FileMode.OpenOrCreate);
            return new WriteHandler(stream);
        }
    }

    public class FileManagerWrapper : IFileManager
    {
        IFileManager _manager;
        string _path;
        bool _format;

        public FileManagerWrapper(IFileManager manager, string path, bool format=false)
        {
            _manager = manager;
            _path = path;
            _format = format;
        }

        string GetPath(string asset)
        {
            if(_format)
            {
                return string.Format(_path, asset);
            }
            else
            {
                return Path.Combine(_path, asset);
            }
        }

        public ReadHandler Read(string asset)
        {
            return _manager.Read(GetPath(asset));

        }

        public WriteHandler Write(string asset)
        {
            return _manager.Write(GetPath(asset));
        }
    }

    public class FileManagerObserver : IFileManager
    {
        public List<string> ReadFiles{ get; private set; }
        public List<string> WriteFiles{ get; private set; }
        IFileManager _manager;

        public FileManagerObserver(IFileManager manager)
        {
            _manager = manager;
            ReadFiles = new List<string>();
            WriteFiles = new List<string>();
        }

        public ReadHandler Read(string asset)
        {
            ReadFiles.Add(asset);
            return _manager.Read(asset);
        }

        public WriteHandler Write(string asset)
        {
            WriteFiles.Add(asset);
            return _manager.Write(asset);
        }

        public void Clear()
        {
            ReadFiles.Clear();
            WriteFiles.Clear();
        }
    }
}
