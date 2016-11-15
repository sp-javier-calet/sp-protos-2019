using System.Collections.Generic;
using SocialPoint.Base;

namespace SocialPoint.Social
{
    public class Alliance
    {
        public string Id;

        public string Name;

        public string Description;

        public int Requirement;

        public int AccessType;

        public int Avatar;

        public int ActivityIndicator;

        public bool IsNewAlliance;

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

        readonly List<AllianceMember> _members;

        readonly List<AllianceMember> _candidates;

        public Alliance()
        {
            _members = new List<AllianceMember>();
            _candidates = new List<AllianceMember>();
        }

        public void AddMember(AllianceMember member)
        {
            AddMembers(_members, new AllianceMember[]{ member });
        }

        public void AddMembers(IEnumerable<AllianceMember> members)
        {
            AddMembers(_members, members);
        }

        public IEnumerator<AllianceMember> GetMembers()
        {
            return _members.GetEnumerator();
        }

        public bool HasMember(string id)
        {
            return GetMember(_members, id) != null;
        }

        public void SetMemberRank(string id, int rank)
        {
            var member = GetMember(_members, id);
            DebugUtils.Assert(member != null, string.Format("Promoting unexistent alliance {0} member {1}", Id, id));
            if(member != null && member.Rank != rank)
            {
                member.Rank = rank;
            }
        }

        public void RemoveMember(string id)
        {
            RemoveMember(_members, id);
        }

        public void AddCandidate(AllianceMember candidate)
        {
            AddMembers(_candidates, new AllianceMember[]{ candidate });
        }

        public void AddCandidates(List<AllianceMember> candidates)
        {
            AddMembers(_candidates, candidates);
        }

        public IEnumerator<AllianceMember> GetCandidates()
        {
            return _candidates.GetEnumerator();
        }

        public bool HasCandidate(string id)
        {
            return GetMember(_candidates, id) != null;
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

        static void SortMembers(List<AllianceMember> members)
        {
            members.Sort((a, b) => {
                if(a.Score != b.Score)
                {
                    return a.Score - b.Score;
                }
                if(a.Name != b.Name)
                {
                    return string.Compare(a.Name, b.Name);
                }
                return string.Compare(a.Uid, b.Uid);
            });
        }

        static AllianceMember GetMember(List<AllianceMember> list, string id)
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

        static void AddMembers(List<AllianceMember> list, IEnumerable<AllianceMember> members)
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

        static void RemoveMember(List<AllianceMember> list, string id)
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
