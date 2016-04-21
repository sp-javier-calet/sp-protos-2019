using UnityEngine;
using System;
using System.Collections.Generic;

namespace SocialPoint.Editor.SPAMGui
{
    public class BRCompilationResult : IEquatable<BRCompilationResult>
    {
        public enum CompilationState {
            PENDING,
            SUCCESSFUL,
            FAILED,
            WARNING
        };
        
        public int                          Id      { get; private set; }
        public List<string>                 Bundles { get; private set; }
        public DateTime                     Date    { get; private set; }
        public CompilationState             State   { get; set; }
        public string                       author  { get; set; }
        
        public BRCompilationResult()
        {
        }
        
        public BRCompilationResult(int compilationId)
        {
            Id = compilationId;
            Date = DateTime.Now;
            State = BRCompilationResult.CompilationState.PENDING;
            Bundles = new List<string> ();
        }

        public BRCompilationResult(int compilationId, CompilationState state)
        {
            Id = compilationId;
            Date = DateTime.Now;
            State = state;
            Bundles = new List<string>();
        }

        public BRCompilationResult(int compilationId, CompilationState state, DateTime date) : this(compilationId,state)
        {
            Date = date;
        }


        public void SetBundles(IEnumerable<string> bundles)
        {
            foreach(var bundle in bundles)
            {
                if(!Bundles.Contains(bundle))
                {
                    Bundles.Add(bundle);
                }
            }
        }
        
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            BRCompilationResult objAsPart = obj as BRCompilationResult;
            if (objAsPart == null) return false;
            else return Equals(objAsPart);
        }
        
        public bool Equals(BRCompilationResult other)
        {
            if (other == null) return false;
            return (this.Id.Equals(other.Id));
        }
        
        public override int GetHashCode ()
        {
            return Id.GetHashCode();
        }
        
        public static bool operator ==(BRCompilationResult a, BRCompilationResult b)
        {
            if (System.Object.ReferenceEquals(a, b)) return true;
            if (((object)a == null) || ((object)b == null)) return false;
            
            return a.Equals(b);
        }
        
        public static bool operator !=(BRCompilationResult a, BRCompilationResult b)
        {
            return !(a == b);
        }
        
        public static int SortByDate(BRCompilationResult a, BRCompilationResult b)
        {
            //Inverse sort, newer go first
            return a.Date.CompareTo(b.Date) * -1;
        }
    }
}

