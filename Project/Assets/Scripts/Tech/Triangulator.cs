﻿
using UnityEngine;
using System.Collections.Generic;

public class Triangulator
{
    List<Vector2> m_points = new List<Vector2>();
    List<int> m_indices = new List<int>();

    int _currentPoints = -1;

    public Triangulator()
    {
        _currentPoints = -1;
    }

    public void Init (Vector2[] points, int numPoints = -1)
    {
        m_points.Clear();

        if(numPoints == -1)
        {
            m_points.AddRange(points);

            _currentPoints = points.Length;
        }
        else
        {
            for(var i = 0; i < numPoints; ++i)
            {
                m_points.Add(points[i]);
            }

            _currentPoints = numPoints;
        }
    }

    public int[] Triangulate()
    {
        m_indices.Clear();

        int n = _currentPoints;
        if (n < 3)
            return m_indices.ToArray();

        int[] V = new int[n];
        if (Area() > 0)
        {
            for (int v = 0; v < n; v++)
                V[v] = v;
        }
        else
        {
            for (int v = 0; v < n; v++)
                V[v] = (n - 1) - v;
        }

        int nv = n;
        int count = 2 * nv;
        for (int v = nv - 1; nv > 2; )
        {
            if ((count--) <= 0)
                return m_indices.ToArray();

            int u = v;
            if (nv <= u)
                u = 0;
            v = u + 1;
            if (nv <= v)
                v = 0;
            int w = v + 1;
            if (nv <= w)
                w = 0;

            if (Snip(u, v, w, nv, V))
            {
                int a, b, c, s, t;
                a = V[u];
                b = V[v];
                c = V[w];
                m_indices.Add(a);
                m_indices.Add(b);
                m_indices.Add(c);
                for (s = v, t = v + 1; t < nv; s++, t++)
                    V[s] = V[t];
                nv--;
                count = 2 * nv;
            }
        }

        m_indices.Reverse();
        return m_indices.ToArray();
    }

    float Area ()
    {
        int n = _currentPoints;
        float A = 0.0f;

        for (int p = n - 1, q = 0; q < n; p = q++)
        {
            Vector2 pval = m_points[p];
            Vector2 qval = m_points[q];
            A += pval.x * qval.y - qval.x * pval.y;
        }

        return (A * 0.5f);
    }

    public bool Snip (int u, int v, int w, int n, int[] V)
    {
        int p;
        Vector2 A = m_points[V[u]];
        Vector2 B = m_points[V[v]];
        Vector2 C = m_points[V[w]];

        if (Mathf.Epsilon > (((B.x - A.x) * (C.y - A.y)) - ((B.y - A.y) * (C.x - A.x))))
            return false;

        for (p = 0; p < n; p++)
        {
            if ((p == u) || (p == v) || (p == w))
                continue;

            Vector2 P = m_points[V[p]];
            if (InsideTriangle(A, B, C, P))
                return false;
        }

        return true;
    }

    public bool InsideTriangle (Vector2 A, Vector2 B, Vector2 C, Vector2 P)
    {
        float ax, ay, bx, by, cx, cy, apx, apy, bpx, bpy, cpx, cpy;
        float cCROSSap, bCROSScp, aCROSSbp;

        ax = C.x - B.x; ay = C.y - B.y;
        bx = A.x - C.x; by = A.y - C.y;
        cx = B.x - A.x; cy = B.y - A.y;
        apx = P.x - A.x; apy = P.y - A.y;
        bpx = P.x - B.x; bpy = P.y - B.y;
        cpx = P.x - C.x; cpy = P.y - C.y;

        aCROSSbp = ax * bpy - ay * bpx;
        cCROSSap = cx * apy - cy * apx;
        bCROSScp = bx * cpy - by * cpx;

        return ((aCROSSbp >= 0.0f) && (bCROSScp >= 0.0f) && (cCROSSap >= 0.0f));
    }
}
