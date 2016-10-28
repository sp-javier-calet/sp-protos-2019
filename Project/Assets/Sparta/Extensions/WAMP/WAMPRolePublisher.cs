using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.WAMP
{
    public class WAMPRolePublisher
    {
        #region Data structures

        public class Publication
        {
            public long Id{ get; private set; }

            public string Topic{ get; private set; }

            public Publication(long id, string topic)
            {
                Id = id;
                Topic = topic;
            }
        }

        public class PublishRequest : WAMPConnection.Request<OnPublished>
        {
            internal string Topic{ get; private set; }

            public PublishRequest(OnPublished completionHandler, string topic) : base(completionHandler)
            {
                Topic = topic;
            }
        }

        Dictionary<long, PublishRequest> _publishRequests;

        WAMPConnection _connection;

        #endregion

        #region Public 
        public WAMPRolePublisher(WAMPConnection connection)
        {
            _connection = connection;
            _publishRequests = new Dictionary<long, PublishRequest>();
        }

        public delegate void OnPublished(Error error, Publication pub);

        public void Publish(string topic, AttrList args, AttrDic kwargs, bool acknowledged, OnPublished completionHandler)
        {
            DebugUtils.Assert((acknowledged && completionHandler != null) || !acknowledged, "Asked for acknowledge but without completionHandler");

            if(!_connection.HasActiveSession())
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.NoSession), null);
                }
                return;
            }

            _connection.DebugMessage(string.Concat("Publish event ", topic, " with args", args, " and ", kwargs));

            /* [PUBLISH, Request|id, Options|dict, Topic|uri, Arguments|list, ArgumentsKw|dict]
             * [16, 239714735, {}, "com.myapp.mytopic1", [], {"color": "orange", "sizes": [23, 42, 7]}]
             */
            var requestId = _connection.GetAndIncrementRequestId();
            if(acknowledged)
            {
                DebugUtils.Assert(!_publishRequests.ContainsKey(requestId), "This requestId was already in use");
                _publishRequests.Add(requestId, new PublishRequest(completionHandler, topic));
            }

            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.PUBLISH));
            data.AddValue(requestId);
            var optionsDict = new AttrDic();
            if(acknowledged)
            {
                optionsDict.SetValue("acknowledge", true);
            }
            data.Add(optionsDict);
            data.AddValue(topic);
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
        }
        #endregion

        #region Private

        internal void ProcessPublished(AttrList msg)
        {
            // [PUBLISHED, PUBLISH.Request|id, Publication|id]
            if(msg.Count != 3)
            {
                throw new Exception("Invalid PUBLISHED message structure - length must be 3");
            }

            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid PUBLISHED message structure - PUBLISHED.Request must be an integer");
            }

            long requestId = msg.Get(1).AsValue.ToLong();

            PublishRequest request;
            if(!_publishRequests.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus PUBLISHED message for non-pending request ID");
            }

            if(!msg.Get(2).IsValue)
            {
                throw new Exception("Invalid PUBLISHED message structure - PUBLISHED.Subscription must be an integer");
            }
            long publicationId = msg.Get(2).AsValue.ToLong();

            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(null, new Publication(publicationId, request.Topic));
            }
            _publishRequests.Remove(requestId);
        }

        internal void ProcessPublishError(long requestId, string description)
        {
            PublishRequest request;
            if(!_publishRequests.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus ERROR message for non-pending PUBLISH request ID");
            }
            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(new Error(ErrorCodes.PublishError, description), new Publication(requestId, request.Topic));
            }
            _publishRequests.Remove(requestId);
        }

        #endregion
    }
}