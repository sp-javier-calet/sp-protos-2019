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

        readonly List<AllianceMemberBasicData> _members;

        readonly List<AllianceMemberBasicData> _candidates;

        public Alliance()
        {
            _members = new List<AllianceMemberBasicData>();
            _candidates = new List<AllianceMemberBasicData>();
        }

        public void AddMember(AllianceMemberBasicData member)
        {
            AddMembers(_members, new AllianceMemberBasicData[]{ member });
        }

        public void AddMembers(IEnumerable<AllianceMemberBasicData> members)
        {
            AddMembers(_members, members);
        }

        public IEnumerator<AllianceMemberBasicData> GetMembers()
        {
            return _members.GetEnumerator();
        }

        public List<AllianceMemberBasicData> GetMembersList()
        {
            return new List<AllianceMemberBasicData> (_members);
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

        public void AddCandidate(AllianceMemberBasicData candidate)
        {
            AddMembers(_candidates, new AllianceMemberBasicData[]{ candidate });
        }

        public void AddCandidates(List<AllianceMemberBasicData> candidates)
        {
            AddMembers(_candidates, candidates);
        }

        public IEnumerator<AllianceMemberBasicData> GetCandidates()
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

        static void SortMembers(List<AllianceMemberBasicData> members)
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

        static AllianceMemberBasicData GetMember(List<AllianceMemberBasicData> list, string id)
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

        static void AddMembers(List<AllianceMemberBasicData> list, IEnumerable<AllianceMemberBasicData> members)
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

        static void RemoveMember(List<AllianceMemberBasicData> list, string id)
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
