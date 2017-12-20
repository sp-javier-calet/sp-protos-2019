// Copyright (c) 2016 Robert Rouhani <robert.rouhani@gmail.com> and other contributors (see CONTRIBUTORS file).
// Licensed under the MIT License - https://raw.github.com/Robmaister/SharpNav/master/LICENSE

using System;
using System.Collections.Generic;

using SharpNav.Geometry;

#if MONOGAME
using Vector3 = Microsoft.Xna.Framework.Vector3;

#elif OPENTK
using Vector3 = OpenTK.Vector3;

#elif SHARPDX
using Vector3 = SharpDX.Vector3;
#endif

namespace SharpNav.Pathfinding
{
    public class NavQueryFilter
    {
        private float[] areaCost;

        //[SP-Change] Added support for flags
        private ushort includeFlags;
        private ushort excludeFlags;

        public NavQueryFilter()
        {
            includeFlags = 0xffff;
            excludeFlags = 0;
            areaCost = new float[Area.MaxValues];
            for(int i = 0; i < areaCost.Length; i++)
                areaCost[i] = 1;
        }

        public virtual bool PassFilter(NavPolyId polyId, NavTile tile, NavPoly poly)
        {
            return ((poly.Flags & includeFlags) != 0 && (poly.Flags & excludeFlags) == 0);
        }

        public virtual float GetCost(Vector3 a, Vector3 b,
                                     NavPolyId prevRef, NavTile prevTile, NavPoly prevPoly,
                                     NavPolyId curRef, NavTile curTile, NavPoly curPoly,
                                     NavPolyId nextRef, NavTile nextTile, NavPoly nextPoly)
        {
            return (a - b).Length() * areaCost[(int)curPoly.Area.Id] + curPoly.Cost;
        }

        public float GetAreaCost(Area area)
        {
            return areaCost[area.Id];
        }

        public void SetAreaCost(Area area, float value)
        {
            areaCost[area.Id] = value;
        }

        public ushort GetIncludeFlags()
        {
            return includeFlags;
        }

        public void SetIncludeFlags(ushort flags)
        {
            includeFlags = flags;
        }

        public ushort GetExcludeFlags()
        {
            return excludeFlags;
        }

        public void SetExcludeFlags(ushort flags)
        {
            excludeFlags = flags;
        }
    }
}
