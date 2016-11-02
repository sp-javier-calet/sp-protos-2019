﻿using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.WAMP.Publisher
{
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

    public delegate void OnPublished(Error error, Publication pub);

    public class PublishRequest : WAMPConnection.Request<OnPublished>
    {
        internal string Topic{ get; private set; }

        public PublishRequest(OnPublished completionHandler, string topic) : base(completionHandler)
        {
            Topic = topic;
        }
    }

    public class WAMPRolePublisher : WAMPRole
    {
        #region Data structures

        Dictionary<long, PublishRequest> _publishRequests;

        #endregion

        #region Public

        public WAMPRolePublisher(WAMPConnection connection) : base(connection)
        {
            _publishRequests = new Dictionary<long, PublishRequest>();
        }

        public PublishRequest Publish(string topic, AttrList args, AttrDic kwargs, bool acknowledged, OnPublished completionHandler)
        {
            DebugUtils.Assert((acknowledged && completionHandler != null) || !acknowledged, "Asked for acknowledge but without completionHandler");

            if(!_connection.HasActiveSession())
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.NoSession), null);
                }
                return null;
            }

            _connection.DebugMessage(string.Concat("Publish event ", topic, " with args", args, " and ", kwargs));

            /* [PUBLISH, Request|id, Options|dict, Topic|uri, Arguments|list, ArgumentsKw|dict]
             * [16, 239714735, {}, "com.myapp.mytopic1", [], {"color": "orange", "sizes": [23, 42, 7]}]
             */
            var requestId = _connection.GetAndIncrementRequestId();
            PublishRequest request = null;
            if(acknowledged)
            {
                DebugUtils.Assert(!_publishRequests.ContainsKey(requestId), "This requestId was already in use");
                request = new PublishRequest(completionHandler, topic);
                _publishRequests.Add(requestId, request);
            }

            var data = new AttrList();
            data.AddValue(MsgCode.PUBLISH);
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

            return request;
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

        internal override void AddRoleDetails(AttrDic detailsDic)
        {
            detailsDic.Set("publisher", new AttrDic());
        }

        internal override void ResetToInitialState()
        {
            for(var i = 0; i < _publishRequests.Count; i++)
            {
                var request = _publishRequests[i];
                if(request.CompletionHandler != null)
                {
                    request.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"), null);
                }
            }

            _publishRequests.Clear();
        }

        #endregion
    }
}