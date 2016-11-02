using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.WAMP.Caller
{
    public delegate void HandlerCall(Error error, AttrList args, AttrDic kwargs);

    public class CallRequest : WAMPConnection.Request<HandlerCall>
    {
        internal CallRequest(HandlerCall handler) : base(handler)
        {
        }
    }

    public class WAMPRoleCaller : WAMPRole
    {
        #region Data structures

        Dictionary<long, CallRequest> _calls;

        #endregion

        #region Public

        public WAMPRoleCaller(WAMPConnection connection) : base(connection)
        {
            _calls = new Dictionary<long, CallRequest>();
        }

        public CallRequest Call(string procedure, AttrList args, AttrDic kwargs, HandlerCall resultHandler)
        {
            if(!_connection.HasActiveSession())
            {
                if(resultHandler != null)
                {
                    resultHandler(new Error(ErrorCodes.NoSession), null, null);
                }
                return null;
            }

            _connection.DebugMessage(string.Concat("Request call ", procedure));

            var requestId = _connection.GetAndIncrementRequestId();
            DebugUtils.Assert(!_calls.ContainsKey(requestId), "This requestId was already in use");
            var request = new CallRequest(resultHandler);
            _calls.Add(requestId, request);

            /* [CALL, Request|id, Options|dict, Procedure|uri]
             * [48, 7814135, {}, "com.myapp.user.new", ["johnny"], {"firstname": "John", "surname": "Doe"}]
             */
            var data = new AttrList();
            data.AddValue(MsgCode.CALL);
            data.AddValue(requestId);
            data.Add(new AttrDic());
            data.AddValue(procedure);
            if(args != null)
            {
                data.Add(args);
            }
            else
            {
                data.Add(new AttrList());
            }

            if(kwargs != null)
            {
                data.Add(kwargs);
            }

            _connection.SendData(data);

            return request;
        }

        #endregion

        #region Private

        void ProcessCallResult(AttrList msg)
        {
            // [RESULT, CALL.Request|id, Details|dict]
            // [RESULT, CALL.Request|id, Details|dict, YIELD.Arguments|list]
            // [RESULT, CALL.Request|id, Details|dict, YIELD.Arguments|list, YIELD.ArgumentsKw|dict]

            if(msg.Count < 3 || msg.Count > 5)
            {
                throw new Exception("Invalid RESULT message structure - length must be 3, 4 or 5");
            }

            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid RESULT message structure - CALL.Request must be an integer");
            }
            long requestId = msg.Get(1).AsValue.ToLong();

            CallRequest request; 
            if(!_calls.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus RESULT message for non-pending request ID");
            }

            if(request.CompletionHandler != null)
            {
                AttrList listParams = null;
                AttrDic dictParams = null;
                if(msg.Count >= 4)
                {
                    if(!msg.Get(3).IsList)
                    {
                        throw new Exception("Invalid RESULT message structure - YIELD.Arguments must be a list");
                    }
                    listParams = msg.Get(3).AsList;
                }
                if(msg.Count >= 5)
                {
                    if(!msg.Get(4).IsDic)
                    {
                        throw new Exception("Invalid RESULT message structure - YIELD.ArgumentsKw must be a dictionary");
                    }
                    dictParams = msg.Get(4).AsDic;
                }
                request.CompletionHandler(null, listParams, dictParams);
            }
            _calls.Remove(requestId);
        }

        internal void ProcessCallError(long requestId, int code, string description, AttrList listArgs, AttrDic dictArgs)
        {
            CallRequest request;
            if(!_calls.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus ERROR message for non-pending CALL request ID");
            }
            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(new Error(code, description), listArgs, dictArgs);
            }
            _calls.Remove(requestId);
        }

        internal override void AddRoleDetails(AttrDic detailsDic)
        {
            detailsDic.Set("caller", new AttrDic());
        }

        internal override void ResetToInitialState()
        {
            for(var i = 0; i < _calls.Count; i++)
            {
                var request = _calls[i];
                if(request.CompletionHandler != null)
                {
                    request.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"), null, null);
                }
            }

            _calls.Clear();
        }

        #endregion
    }
}