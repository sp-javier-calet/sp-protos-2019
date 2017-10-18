namespace SocialPoint.Attributes
{
    public sealed class ConfigStreamReader : IStreamReader
    {
        const string KGameData = "game_data";
        const string KConfig = "config";

        IStreamReader _reader;

        int _counter;
        bool _readerFinished;

        public ConfigStreamReader(byte[] data)
        {
            _reader = new JsonStreamReader(data);
        }
        
        #region IStreamReader implementation

        public bool Read()
        {
            _counter++;
            if(_counter > 4 && !_readerFinished)
            {
                _readerFinished = !_reader.Read();
                if(_readerFinished)
                {
                    _counter = 0;
                }
            }
            return _counter < 2 || !_readerFinished;
        }

        public StreamToken Token
        {
            get
            {
                if(_readerFinished && _counter == 0)
                {
                    return StreamToken.ObjectEnd;
                }
                else if(_readerFinished && _counter == 1)
                {
                    return StreamToken.ObjectEnd;
                }
                else if(_counter == 1 || _counter == 3)
                {
                    return StreamToken.ObjectStart;
                }
                else if(_counter == 2 || _counter == 4)
                {
                    return StreamToken.PropertyName;
                }
                else if(_counter > 4)
                {
                    return _reader.Token;
                }
                return _reader.Token;
            }
        }

        public object Value
        {
            get
            {
                if(_readerFinished)
                {
                    return null;
                }
                else if(_counter == 2)
                {
                    return KGameData;
                }
                else if(_counter == 4)
                {
                    return KConfig;
                }
                else if(_counter > 4)
                {
                    return _reader.Value;
                }
                return _reader.Value;
            }
        }

        #endregion
    }
}