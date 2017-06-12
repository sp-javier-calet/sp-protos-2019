using System;
using System.Collections;
using System.Collections.Generic;
using SharpNav;
using SocialPoint.Utils;
using SocialPoint.IO;
#if UNITY_5_3_OR_NEWER
using UnityEngine;
using SocialPoint.Exporter;
#endif

namespace SocialPoint.Pathfinding
{
    [Serializable]
    public class NavmeshAreaSettings
    {
        #pragma warning disable 414
        #if UNITY_5_3_OR_NEWER
        [ExportTagSet]
        [SerializeField]
        #endif
        TagSet _areas;
        #pragma warning restore 414

        #pragma warning disable 414
        #if UNITY_5_3_OR_NEWER
        [SerializeField]
        #endif
        List<string> _flags = new List<string>();
        #pragma warning restore 414

        #pragma warning disable 414
        #if UNITY_5_3_OR_NEWER
        [SerializeField]
        #endif
        List<string> _mapAreas = new List<string>();
        #pragma warning restore 414

        #pragma warning disable 414
        #if UNITY_5_3_OR_NEWER
        [SerializeField]
        #endif
        List<string> _mapFlags = new List<string>();
        #pragma warning restore 414

        Dictionary<string, Area> _uniqueAreas;
        Dictionary<string, ushort> _uniqueFlags;
        Dictionary<string, ushort> _areaNamesToFlagsMap;
        Dictionary<Area, ushort> _areaObjToFlagsMap;

        //BaseArea must be equal to Area.Null (0);
        const byte BaseArea = 0;
        byte _lastAreaAdd;

        const ushort DefaultFlag = 0x01;
        const ushort BaseFlag = 0x02;
        byte _lastFlagShift;

        public void InitData()
        {
            _lastAreaAdd = 1;
            _lastFlagShift = 0;

            _uniqueAreas = new Dictionary<string, Area>();
            _uniqueFlags = new Dictionary<string, ushort>();
            _areaNamesToFlagsMap = new Dictionary<string, ushort>();
            _areaObjToFlagsMap = new Dictionary<Area, ushort>();

            for(int i = 0; i < _mapAreas.Count; i++)
            {
                string areaName = _mapAreas[i];
                if(!_uniqueAreas.ContainsKey(areaName))
                {
                    _uniqueAreas.Add(areaName, NextArea());

                    _areaNamesToFlagsMap.Add(areaName, 0);
                    _areaObjToFlagsMap.Add(_uniqueAreas[areaName], 0);
                }

                string flagName = _mapFlags[i];
                if(!_uniqueFlags.ContainsKey(flagName))
                {
                    _uniqueFlags.Add(flagName, NextFlag());
                }

                _areaNamesToFlagsMap[areaName] |= _uniqueFlags[flagName];
                _areaObjToFlagsMap[_uniqueAreas[areaName]] |= _uniqueFlags[flagName];
            }
        }

        public Area ExportArea(string name)
        {
            return _uniqueAreas[name];
        }

        public ushort ExportFlags(Area area)
        {
            if(area.Id == Area.Default)
            {
                return DefaultFlag;
            }
            return _areaObjToFlagsMap[area];
        }

        public ushort GetFlagsForArea(string name)
        {
            return _areaNamesToFlagsMap[name];
        }

        public ushort GetDefaultFlag()
        {
            return DefaultFlag;
        }

        public void Serialize(IWriter writer)
        {
            writer.WriteStringList(_mapAreas);
            writer.WriteStringList(_mapFlags);
        }

        public void Deserialize(IReader reader)
        {
            _mapAreas = reader.ReadStringList();
            _mapFlags = reader.ReadStringList();
            InitData();
        }

        Area NextArea()
        {
            byte area = (byte)(BaseArea + _lastAreaAdd);
            _lastAreaAdd++;
            if(area == Area.Default.Id)
            {
                throw new Exception("Too many different areas in NavMesh. Max amount: " + Area.Default.Id);
            }
            return new Area(area);
        }

        ushort NextFlag()
        {
            ushort flag = (ushort)(BaseFlag << _lastFlagShift);
            _lastFlagShift++;
            if(flag == 0)
            {
                throw new Exception("Too many different flags for NavMesh. Max amount: " + (sizeof(ushort) * 8));
            }
            return flag;
        }
    }
}
