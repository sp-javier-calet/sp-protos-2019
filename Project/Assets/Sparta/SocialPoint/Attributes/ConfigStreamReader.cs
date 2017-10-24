namespace SocialPoint.Attributes
{
    public sealed class ConfigStreamReader : IStreamReader
    {
        const string KGameData = "game_data";
        const string KConfig = "config";

        const int KFirstWord = 1;
        const int KSecondWord = 2;
        const int KThirdWord = 3;
        const int KFourthWord = 4;

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
            if(_counter > KFourthWord && !_readerFinished)
            {
                _readerFinished = !_reader.Read();
                if(_readerFinished)
                {
                    _counter = 0;
                }
            }
            return _counter < KSecondWord || !_readerFinished;
        }

        public StreamToken Token
        {
            get
            {
                if(_readerFinished && _counter == 0)
                {
                    return StreamToken.ObjectEnd;
                }
                else if(_readerFinished && _counter == KFirstWord)
                {
                    return StreamToken.ObjectEnd;
                }
                else if(_counter == KFirstWord || _counter == KThirdWord)
                {
                    return StreamToken.ObjectStart;
                }
                else if(_counter == KSecondWord || _counter == KFourthWord)
                {
                    return StreamToken.PropertyName;
                }
                else if(_counter > KFourthWord)
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
                else if(_counter == KSecondWord)
                {
                    return KGameData;
                }
                else if(_counter == KFourthWord)
                {
                    return KConfig;
                }
                else if(_counter > KFourthWord)
                {
                    return _reader.Value;
                }
                return _reader.Value;
            }
        }

        #endregion
    }
}