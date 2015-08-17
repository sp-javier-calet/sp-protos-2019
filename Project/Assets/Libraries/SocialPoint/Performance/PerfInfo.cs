using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;

namespace SocialPoint.Performance
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FrameInfo
    {
        public float FrameTime;
        public float CpuPlayerTime;
        public float CpuOpenGlESDriverTime;
        public float CpuPresentTime;
        public float PlayerDetailPhysxTime;
        public float PlayerDetailAnimationTime;
        public float PlayerDetailCullingTime;
        public float PlayerDetailSkinningTime;
        public float PlayerDetailBatchingTime;
        public float PlayerDetailRenderTime;
        public float MonoScriptsUpdateTime;
        public float MonoScriptsFixedUpdateTime;
        public float MonoScriptsCoroutinesTime;
        public int DrawCalls;
        public int BatchedDrawCalls;
        public int Tris;
        public int BatchedTris;
        public int Verts;
        public int BatchedVerts;
        public int PlayerDetailFixedUpdateCountMin;
        public int PlayerDetailFixedUpdateCountMax;

        public float FPS
        {
            get
            {
                if (FrameTime != 0f)
                {
                    return 1000f / FrameTime;
                }
                else
                {
                    return 0f;
                }
            }
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GarbageInfo
    {
        public uint UsedHeap;
        public uint AllocatedHeap;
        public uint MaxNumberOfCollections;

        public float StopWorldTime;
        public float MarkTime;
        public float ReclaimTime;
        public float StartWorldTime;
        public float CollectionTime;
        public float TotalTime;
    }

    public class PerfStats : IDisposable
    {
        public FrameInfo Frame;
        public GarbageInfo Garbage;

        MonoBehaviour _behaviour;
        Coroutine _updateCoroutine;
        float _updateInterval = 0.0f;
        float _currentInterval = 0.0f;

        public PerfStats(MonoBehaviour behaviour, float updateInterval=1.0f)
        {
            _updateInterval = updateInterval;
            _behaviour = behaviour;
            _updateCoroutine = behaviour.StartCoroutine(UpdateCoroutine());
        }

        public void Dispose()
        {
            if(_updateCoroutine != null && _behaviour != null)
            {
                _behaviour.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        IEnumerator UpdateCoroutine()
        {
            while(true)
            {
                Update();
                yield return true;
            }
        }

        void Update()
        {
            _currentInterval += Time.deltaTime;
            if(_currentInterval >= _updateInterval)
            {
                _currentInterval = 0.0f;
                UpdateStats();
            }
        }

        void UpdateStats()
        {
            Frame = SPUnityPerformanceGetFrameInfo();
            Garbage = SPUnityPerformanceGetGarbageInfo();
        }

        const string PluginModuleName = "__Internal";

        #if !UNITY_EDITOR && UNITY_IOS
        [DllImport(PluginModuleName)]
        public static extern FrameInfo SPUnityPerformanceGetFrameInfo();
        #else
        public static FrameInfo SPUnityPerformanceGetFrameInfo()
        {
            var stats = new FrameInfo();

            stats.FrameTime = Time.smoothDeltaTime;
            foreach(MeshFilter mf in GameObject.FindObjectsOfType(typeof(MeshFilter)))
            {
                stats.Verts += mf.sharedMesh.vertexCount;
                stats.Tris += mf.sharedMesh.triangles.Length;
            }
            return stats;
        }
        #endif

        #if !UNITY_EDITOR && UNITY_IOS
        [DllImport(PluginModuleName)]
        public static extern GarbageInfo SPUnityPerformanceGetGarbageInfo();
        #else
        public static GarbageInfo SPUnityPerformanceGetGarbageInfo()
        {
            var stats = new GarbageInfo();
            stats.AllocatedHeap = Profiler.GetMonoHeapSize();
            stats.UsedHeap = Profiler.usedHeapSize;
            return stats;
        }
        #endif
    }
}