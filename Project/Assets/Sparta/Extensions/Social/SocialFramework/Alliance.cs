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

        public AllianceAccessType Type;

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
            DebugUtils.Assert(HasMember(member.Uid), string.Format("Trying to add player {0} to alliance {1} while he is already a member", member.Uid, Id));
            _members.Add(member);
            SortMembers(_members);
        }

        public void AddMembers(List<AllianceMember> members)
        {
            DebugUtils.Assert(() => {
                for(var i = 0; i < members.Count; ++i)
                {
                    var member = members[i];
                    if(HasMember(member.Uid))
                    {
                        return string.Format("Trying to add player {0} to alliance {1} while he is already a member", member.Uid, Id);
                    }
                }
                return null;
            });

            _members.AddRange(members);
            SortMembers(_members);
        }

        void SortMembers(List<AllianceMember> members)
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

        AllianceMember GetMember(List<AllianceMember> list, string id)
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

        public IEnumerator<AllianceMember> GetMembers()
        {
            return _members.GetEnumerator();
        }

        public bool HasMember(string id)
        {
            return GetMember(_members, id) != null;
        }

        public void SetMemberType(string id, AllianceMemberType type)
        {
            DebugUtils.Assert(HasMember(id), string.Format("Promoting unexistent alliance {0} member {1}", Id, id));
            var member = GetMember(_members, id);
            if(member != null)
            {
                member.Type = type;
                SortMembers(_members);
            }
        }

        public void RemoveMember(string id)
        {
            DebugUtils.Assert(HasMember(id), string.Format("Removing unexistent alliance {0} member {1}", Id, id));
            _members.Remove(GetMember(_members, id));
        }

        public void AddCandidate(AllianceMember candidate)
        {
            DebugUtils.Assert(HasMember(candidate.Uid), string.Format("Trying to add player {0} to alliance {1} while he is already a member", candidate.Uid, Id));
            _candidates.Add(candidate);
            SortMembers(_candidates);
        }

        public void AddCandidates(List<AllianceMember> candidates)
        {
            DebugUtils.Assert(() => {
                for(var i = 0; i < candidates.Count; ++i)
                {
                    var candidate = candidates[i];
                    if(HasCandidate(candidate.Uid))
                    {
                        return string.Format("Trying to add player {0} to alliance {1} while he is already a member", candidate.Uid, Id);
                    }
                }
                return null;
            });

            _candidates.AddRange(candidates);
            SortMembers(_candidates);
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
            if(candidate != null)
            {
                AddMember(candidate);
                _candidates.Remove(candidate);
            }
        }

        public void RemoveCandidate(string id)
        {
            DebugUtils.Assert(HasMember(id), string.Format("Removing unexistent alliance {0} candidate {1}", Id, id));
            _members.Remove(GetMember(_candidates, id));
        }

        public void RemoveAllCandidates()
        {
            _candidates.Clear();
        }
    }
}
