using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.Utils
{
    [System.Serializable]
    public class LoggerContextData
    {
        [SerializeField]
        public string
            name;
        [SerializeField]
        public bool
            enabled;

        public LoggerContextData(string iName, bool iEnabled)
        {
            name = iName;
            enabled = iEnabled;
        }
    }

    public class LoggerContext : MonoBehaviour
    {
        [SerializeField]
        List<LoggerContextData>
            _contexts;
        [SerializeField]
        bool
            _filterLoggerByContext = true;
        Dictionary<string, int> _contextNameToPos = new Dictionary<string, int>();

        void Awake()
        {
            // Init Dictionary with serialized contexts
            for(int i = 0; i < _contexts.Count; ++i)
            {
                LoggerContextData contextData = _contexts[i];
                if(!_contextNameToPos.ContainsKey(contextData.name))
                {
                    _contextNameToPos.Add(contextData.name, i);
                }
            }
        }

        public bool IsContextEnabled(string context)
        {
            if(_filterLoggerByContext)
            {
                int contextPos = -1;
                bool found = _contextNameToPos.TryGetValue(context, out contextPos);
                if(!found)
                {
                    AddContext(context);

                    return false;
                }

                if(_contexts[contextPos].enabled)
                {
                    return true;
                }

                return false;
            }

            return true;
        }

        public bool AddContext(string context)
        {
            if(_contextNameToPos.ContainsKey(context))
            {
                return false;
            }

            // add new context
            _contexts.Add(new LoggerContextData(context, false));
            _contextNameToPos.Add(context, _contexts.Count - 1);

            return true;
        }
    }
}
