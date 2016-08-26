using UnityEngine;
using System;
using System.Collections.Generic;

namespace SocialPoint.Editor.SPAMGui
{
    public sealed class SPAMTaskResult : IEquatable<SPAMTaskResult>
    {
        public enum TaskState {
            PENDING,
            SUCCESSFUL,
            FAILED
        };

        public string                       Id      { get; private set; }
        public string                       Scene   { get; private set; }
        public string                       Version { get; private set; }
        public string                       Bundle  { get; private set; }
        public DateTime                     Date    { get; private set; }
        public TaskState                    State   { get; set; }
        public long                         Size    { get; set; }

        public SPAMTaskResult()
        {
        }

        public SPAMTaskResult(string taskId, string scene, string version, string bundle)
        {
            Id = taskId;
            Scene = scene;
            Version = version;
            Bundle = bundle;
            Date = DateTime.Now;
            State = TaskState.PENDING;
            Size = -1;
        }

        public SPAMTaskResult(string taskId, string scene, string version, string bundle, TaskState state)
        {
            Id = taskId;
            Scene = scene;
            Version = version;
            Bundle = bundle;
            Date = DateTime.Now;
            State = state;
            Size = -1;
        }

        public SPAMTaskResult(string taskId, string scene, string version, string bundle, TaskState state, long size)
        {
            Id = taskId;
            Scene = scene;
            Version = version;
            Bundle = bundle;
            Date = DateTime.Now;
            State = state;
            Size = size;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            SPAMTaskResult objAsPart = obj as SPAMTaskResult;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }

        public bool Equals(SPAMTaskResult other)
        {
            if (other == null) return false;
            return (this.Id.Equals(other.Id));
        }

        public override int GetHashCode ()
        {
            return Id.GetHashCode();
        }

        public static bool operator ==(SPAMTaskResult a, SPAMTaskResult b)
        {
            if (System.Object.ReferenceEquals(a, b)) return true;
            if (((object)a == null) || ((object)b == null)) return false;

            return a.Equals(b);
        }

        public static bool operator !=(SPAMTaskResult a, SPAMTaskResult b)
        {
            return !(a == b);
        }

        public static int SortByDate(SPAMTaskResult a, SPAMTaskResult b)
        {
            //Inverse sort, newer go first
            return a.Date.CompareTo(b.Date) * -1;
        }
    }
}