using fastJSON;
using System.Collections.Generic;

namespace SocialPoint.Attributes
{
    public class FastJsonStreamReader : IStreamReader
    {
        JsonParser _parser;
        bool _expectedPropertyNameToken = false;
        Stack<bool> _containers = new  Stack<bool>();

        StreamToken _currentToken;

        public StreamToken Token
        {
            get
            {
                return _currentToken;
            }
        }

        object _currentValue;

        public object Value
        {
            get
            {
                return _currentValue;
            }
        }

        public FastJsonStreamReader(byte[] data) : this(System.Text.Encoding.UTF8.GetString(data))
        {
        }

        public FastJsonStreamReader(string data)
        {
            _parser = new JsonParser(data);
        }

        void UpdateCurrentTokenAndValue()
        {
            _currentValue = null;
            switch(_parser.CurrentToken)
            {
            case JsonParser.Token.Curly_Open:
                _containers.Push(true);
                _expectedPropertyNameToken = true;
                _currentToken = StreamToken.ObjectStart;
                break;

            case JsonParser.Token.Curly_Close:
                _containers.Pop();
                _expectedPropertyNameToken = false;
                _currentToken = StreamToken.ObjectEnd;
                break;

            case JsonParser.Token.Squared_Open:
                _containers.Push(false);
                _currentToken = StreamToken.ArrayStart;
                break;

            case JsonParser.Token.Squared_Close:
                _containers.Pop();
                _currentToken = StreamToken.ArrayEnd;
                break;

            case JsonParser.Token.True:
            case JsonParser.Token.False:
                _currentToken = StreamToken.Boolean;
                _currentValue = _parser.CurrentValue;
                break;

            case JsonParser.Token.Null:
                _currentToken = StreamToken.Null;
                break;

            case JsonParser.Token.String:
                if(_expectedPropertyNameToken && _containers.Peek())
                {
                    _currentToken = StreamToken.PropertyName;
                }
                else
                {
                    _currentToken = StreamToken.String;
                }
                _currentValue = _parser.CurrentValue;
                _expectedPropertyNameToken = false;
                break;

            case JsonParser.Token.Number:
                if(_parser.CurrentValue is double)
                {
                    _currentToken = StreamToken.Double;
                    _currentValue = _parser.CurrentValue;
                }
                else
                {
                    long value = (long)_parser.CurrentValue;
                    if(value >= int.MinValue && value <= int.MaxValue)
                    {
                        _currentValue = (int)value;
                        _currentToken = StreamToken.Int;
                    }
                    else
                    {
                        _currentValue = value;
                        _currentToken = StreamToken.Long;
                    }
                }
                break;

            default:
                _currentToken = StreamToken.None;
                _currentValue = null;
                break;
            }

        }

        public bool Read()
        {
            bool result = _parser.Read();
            if(result)
            {
                if(_parser.CurrentToken == JsonParser.Token.Comma)
                {
                    _expectedPropertyNameToken = true;
                    return Read();
                }
                if(_parser.CurrentToken == JsonParser.Token.Colon)
                {
                    return Read();
                }
                else
                {
                    _currentValue = _parser.CurrentValue;
                    UpdateCurrentTokenAndValue();
                }
            }
            else
            {
                _currentToken = StreamToken.None;
                _currentValue = null;
            }
            return result;
        }
    }
}