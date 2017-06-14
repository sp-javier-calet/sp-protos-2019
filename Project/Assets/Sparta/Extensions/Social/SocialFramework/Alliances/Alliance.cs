using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    /// <summary>
    /// Common Alliance data.
    /// This class is only intended to be inherited by library classes,
    /// containing the common data for all of them.
    /// </summary>
    public abstract class AllianceData
    {
        public string Id;

        public string Name;

        public int Avatar;

        public int Requirement;

        public int AccessType;

        public int ActivityIndicator;

        public bool IsNewAlliance;

    }

    /// <summary>
    /// Alliance Basic Data.
    /// This class represents the minimal Alliance Data, used in list like
    /// Alliances search or rankings.
    /// </summary>
    public class AllianceBasicData : AllianceData
    {
        public int Score;

        public int Members;

        public int Candidates;
    }

    /// <summary>
    /// Alliance
    /// Complete Alliance Data
    /// </summary>
    public class Alliance : AllianceData
    {
        public string Description;
        public string Message;

        public int Score
        {
            get
            {
                var score = 0;
                for(var i = 0; i < _members.Count; ++i)
                {
                    score += _members[i].Score;
                }
                return score;
            }
        }

        public int Members
        {
            get
            {
                return _members.Count;
            }
        }

        public int Candidates
        {
            get
            {
                return _candidates.Count;
            }
        }

        readonly List<SocialPlayer> _members;

        readonly List<SocialPlayer> _candidates;

        public Alliance()
        {
            _members = new List<SocialPlayer>();
            _candidates = new List<SocialPlayer>();
        }

        public void AddMember(SocialPlayer member)
        {
            AddMembers(_members, new []{ member });
        }

        public void AddMembers(IEnumerable<SocialPlayer> members)
        {
            AddMembers(_members, members);
        }

        public IEnumerator<SocialPlayer> GetMembers()
        {
            return _members.GetEnumerator();
        }

        public List<SocialPlayer> GetMembersList()
        {
            return new List<SocialPlayer>(_members);
        }

        public bool HasMember(string id)
        {
            return GetMember(_members, id) != null;
        }

        public void SetMemberRank(string id, int rank)
        {
            var member = GetMember(_members, id);
            DebugUtils.Assert(member != null, string.Format("Promoting unexistent alliance {0} member {1}", Id, id));
            if(member != null)
            {
                var component = member.GetComponent<AlliancePlayerBasic>();
                if(component != null && component.Rank != rank)
                {
                    component.Rank = rank;
                }
            }
        }

        public void RemoveMember(string id)
        {
            RemoveMember(_members, id);
        }

        public void AddCandidate(SocialPlayer candidate)
        {
            AddMembers(_candidates, new []{ candidate });
        }

        public void AddCandidates(List<SocialPlayer> candidates)
        {
            AddMembers(_candidates, candidates);
        }

        public IEnumerator<SocialPlayer> GetCandidates()
        {
            return _candidates.GetEnumerator();
        }

        public List<SocialPlayer> GetCandidatesList()
        {
            return new List<SocialPlayer> (_candidates);
        }

        public bool HasCandidate(string id)
        {
            return GetMember(_candidates, id) != null;
        }

        public SocialPlayer GetCandidate(string id)
        {
            return GetMember(_candidates, id);
        }

        public void AcceptCandidate(string id)
        {
            var candidate = GetMember(_candidates, id);
            DebugUtils.Assert(candidate != null, string.Format("Accepting unexistent alliance candidate {0}", id));
            if(candidate != null)
            {
                AddMember(candidate);
                _candidates.Remove(candidate);
            }
        }

        public void RemoveCandidate(string id)
        {
            RemoveMember(_candidates, id);
        }

        public void RemoveAllCandidates()
        {
            _candidates.Clear();
        }

        #region Static methods to manager members lists

        static void SortMembers(List<SocialPlayer> members)
        {
            members.Sort((a, b) => {
                if(a.Score != b.Score)
                {
                    return a.Score - b.Score;
                }
                return a.Name != b.Name ? string.Compare(a.Name, b.Name) : string.Compare(a.Uid, b.Uid);
            });
        }

        static SocialPlayer GetMember(List<SocialPlayer> list, string id)
        {
            for(var i = 0; i < list.Count; ++i)
            {
                var member = list[i];
                if(member.Uid == id)
                {
                    return member;
                }
            }
            return null;
        }

        static void AddMembers(List<SocialPlayer> list, IEnumerable<SocialPlayer> members)
        {
            var itr = members.GetEnumerator();
            while(itr.MoveNext())
            {
                var member = itr.Current;
                DebugUtils.Assert(GetMember(list, member.Uid) == null, string.Format("Trying to add player {0}  while he is already a member", member.Uid));
                list.Add(member);
            }
            itr.Dispose();
            SortMembers(list);
        }

        static void RemoveMember(List<SocialPlayer> list, string id)
        {
            var member = GetMember(list, id);
            DebugUtils.Assert(member != null, string.Format("Removing unexistent alliance member {0}", id));
            if(member != null)
            {
                list.Remove(member);
            }
        }

        #endregion
    }
}
