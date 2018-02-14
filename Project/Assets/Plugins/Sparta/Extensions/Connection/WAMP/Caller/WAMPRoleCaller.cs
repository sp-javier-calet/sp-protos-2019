using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.WAMP.Caller
{
    public delegate void HandlerCall(Error error, AttrList args, AttrDic kwargs);

    public class CallRequest : WAMPConnection.Request<HandlerCall>
    {
        public const int IdIndex = 1;

        internal AttrList Data { get; private set; }

        public CallRequest(HandlerCall handler, AttrList data) : base(handler)
        {
            Data = data;
        }
    }

    public class WAMPRoleCaller : WAMPRole
    {
        #region Data structures

        readonly Dictionary<long, CallRequest> _calls;

        #endregion

        #region Public

        public WAMPRoleCaller(WAMPConnection connection) : base(connection)
        {
            _calls = new Dictionary<long, CallRequest>();
        }

        public CallRequest CreateCall(string procedure, AttrList args, AttrDic kwargs, HandlerCall resultHandler)
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

            var data = CreateCallData(procedure, args, kwargs);
            var request = new CallRequest(resultHandler, data);
            return request;
        }

        public void SendCall(CallRequest request)
        {
            var requestId = _connection.GetAndIncrementRequestId();
            DebugUtils.Assert(!_calls.ContainsKey(requestId), "This requestId was already in use");

            request.Data.SetValue(CallRequest.IdIndex, requestId);
            _calls.Add(requestId, request);

            _connection.SendData(request.Data);
        }

        #endregion

        #region Private

        AttrList CreateCallData(string procedure, AttrList args, AttrDic kwargs)
        {
            /* [CALL, Request|id, Options|dict, Procedure|uri]
             * [48, 7814135, {}, "com.myapp.user.new", ["johnny"], {"firstname": "John", "surname": "Doe"}]
             */
            var data = new AttrList();
            data.AddValue(MsgCode.CALL);
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

            //Placeholder id
            data.InsertValue(CallRequest.IdIndex, 0L);

            return data;
        }

        internal void ProcessCallResult(AttrList msg)
        {
            // [RESULT, CALL.Request|id, Details|dict]
            // [RESULT, CALL.Request|id, Details|dict, YIELD.Arguments|list]
            // [RESULT, CALL.Request|id, Details|dict, YIELD.Arguments|list, YIELD.ArgumentsKw|dict]

            if(msg.Count < 3 || msg.Count > 5)
            {
                Log.e("Invalid RESULT message structure - length must be 3, 4 or 5");
                return;
            }

            if(!msg.Get(1).IsValue)
            {
                Log.e("Invalid RESULT message structure - CALL.Request must be an integer");
                return;
            }
            long requestId = msg.Get(1).AsValue.ToLong();

            CallRequest request; 

            if(_calls == null)
            {
                Log.e("Calls are null !!!");
                return;
            }

            if(!_calls.TryGetValue(requestId, out request))
            {
                Log.e("Bogus RESULT message for non-pending request ID");
                return;
            }

            if(request.CompletionHandler != null)
            {
                AttrList listParams = null;
                AttrDic dictParams = null;
                if(msg.Count >= 4)
                {
                    if(!msg.Get(3).IsList)
                    {
                        Log.e("Invalid RESULT message structure - YIELD.Arguments must be a list");
                        return;
                    }
                    listParams = msg.Get(3).AsList;
                }
                if(msg.Count >= 5)
                {
                    if(!msg.Get(4).IsDic)
                    {
                        Log.e("Invalid RESULT message structure - YIELD.ArgumentsKw must be a dictionary");
                        return;
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
                Log.e("Bogus ERROR message for non-pending CALL request ID");
                return;
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
            var itr = _calls.GetEnumerator();
            while(itr.MoveNext())
            {
                var request = itr.Current.Value;
                if(request.CompletionHandler != null)
                {
                    request.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"), null, null);
                }
            }
            itr.Dispose();
            _calls.Clear();
        }

        #endregion
    }
}
