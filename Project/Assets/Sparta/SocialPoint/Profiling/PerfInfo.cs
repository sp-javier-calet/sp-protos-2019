using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace SocialPoint.Profiling
{
    [StructLayout(LayoutKind.Sequential)]
    public struct FrameInfo
    {
        public float FrameTime;
        public float PlayerTime;
        public float OpenglTime;
        public float PresentTime;
        public float PhysicsTime;
        public float AnimationTime;
        public float CullingTime;
        public float SkinningTime;
        public float BatchingTime;
        public float RenderTime;
        public float ScriptUpdateTime;
        public float ScriptFixedUpdateTime;
        public float ScriptCoroutinesTime;
        public uint DrawCalls;
        public uint BatchedDrawCalls;
        public uint Tris;
        public uint BatchedTris;
        public uint Verts;
        public uint BatchedVerts;
        public uint FixedUpdates;

        public float FPS
        {
            get
            {
                if(Math.Abs(FrameTime) > Single.Epsilon)
                {
                    return 1000f / FrameTime;
                }
                return 0f;
            }
        }

        override public string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("fps {0:F2}\n", FPS);
            builder.Append("times (ms) ");
            builder.AppendFormat("frame {0:F2}", FrameTime);
            builder.Append(", ");
            builder.AppendFormat("render {0:F2}", RenderTime);
            builder.Append(", ");
            builder.AppendFormat("player {0:F2}", PlayerTime);
            builder.Append(", ");
            builder.AppendFormat("opengl {0:F2}", OpenglTime);
            builder.Append(", ");
            builder.AppendFormat("present {0:F2}", PresentTime);
            builder.Append(", ");
            builder.AppendFormat("physics {0:F2}", PhysicsTime);
            builder.Append(", ");
            builder.AppendFormat("animation {0:F2}", AnimationTime);
            builder.Append("\n");
            builder.AppendFormat("culling {0:F2}", CullingTime);
            builder.Append(", ");
            builder.AppendFormat("skinning {0:F2}", SkinningTime);
            builder.Append(", ");
            builder.AppendFormat("batching {0:F2}", BatchingTime);
            builder.Append(", ");
            builder.AppendFormat("update {0:F2}", ScriptUpdateTime);
            builder.Append(", ");
            builder.AppendFormat("fixed update {0:F2}", ScriptFixedUpdateTime);
            builder.Append(", ");
            builder.AppendFormat("coroutines {0}", ScriptCoroutinesTime);
            builder.Append("\ncounts ");
            builder.AppendFormat("draws {0} ({1})", DrawCalls, BatchedDrawCalls);
            builder.Append(", ");
            builder.AppendFormat("tris {0} ({1})", Tris, BatchedTris);
            builder.Append(", ");
            builder.AppendFormat("verts {0} ({1})", Verts, BatchedVerts);
            builder.Append(", ");
            builder.AppendFormat("fixed updates {0}", FixedUpdates);
            return builder.ToString();
        }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct GarbageInfo
    {
        public uint UsedHeap;
        public uint AllocatedHeap;
        public uint MaxCollectionsNum;

        public float StopWorldTime;
        public float MarkTime;
        public float ReclaimTime;
        public float StartWorldTime;
        public float CollectionTime;
        public float TotalTime;

        
        override public string ToString()
        {
            var builder = new StringBuilder();
            builder.Append("times (ms) ");
            builder.AppendFormat("total {0:F2}", TotalTime);
            builder.Append(", ");
            builder.AppendFormat("world {0:F2} {1:F2}", StartWorldTime, StopWorldTime);
            builder.Append(", ");
            builder.AppendFormat("mark {0:F2}", MarkTime);
            builder.Append(", ");
            builder.AppendFormat("reclaim {0:F2}", ReclaimTime);
            builder.Append(", ");
            builder.AppendFormat("collection {0:F2}", CollectionTime);
            builder.Append("\ncounts ");
            builder.AppendFormat("heap {0} ({1})", UsedHeap, AllocatedHeap);
            builder.Append(", ");
            builder.AppendFormat("max collections {0}", MaxCollectionsNum);
            return builder.ToString();
        }
    }

    public class PerfInfo : IDisposable
    {
        public FrameInfo Frame;
        public GarbageInfo Garbage;

        MonoBehaviour _behaviour;
        Coroutine _updateCoroutine;
        float _updateInterval;
        float _currentInterval;

        public PerfInfo(MonoBehaviour behaviour, float updateInterval = 1.0f)
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
            Frame = SPUnityProfilerGetFrameInfo();
            Garbage = SPUnityProfilerGetGarbageInfo();
        }

        override public string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("frame");
            builder.Append(Frame.ToString());
            builder.AppendLine("\n----");
            builder.AppendLine("garbage collector");
            builder.Append(Garbage.ToString());
            return builder.ToString();
        }

        public string ToShortString()
        {
            var builder = new StringBuilder();
            builder.AppendFormat("fps: {0:F0}\n", Frame.FPS);
            builder.AppendFormat("tris: {0} ({1})\n", Frame.Tris, Frame.BatchedTris);
            builder.AppendFormat("draws: {0} ({1})\n", Frame.DrawCalls, Frame.BatchedDrawCalls);
            return builder.ToString();
        }

        const string PluginModuleName = "__Internal";

        #if UNITY_EDITOR
        public static FrameInfo SPUnityProfilerGetFrameInfo()
        {
            var stats = new FrameInfo();
            stats.FrameTime = UnityEditor.UnityStats.frameTime;
            if(Math.Abs(stats.FrameTime) < Single.Epsilon)
            {
                stats.FrameTime = Time.smoothDeltaTime * 1000;
            }
            stats.RenderTime = UnityEditor.UnityStats.renderTime;
            stats.DrawCalls = (uint)UnityEditor.UnityStats.drawCalls;
            stats.BatchedDrawCalls = (uint)(UnityEditor.UnityStats.dynamicBatchedDrawCalls + UnityEditor.UnityStats.staticBatchedDrawCalls);
            stats.Tris = (uint)UnityEditor.UnityStats.triangles;
            stats.Verts = (uint)UnityEditor.UnityStats.vertices;
            return stats;
        }
        #elif UNITY_IOS && SPARTA_PROFILER_ENABLED
        [DllImport(PluginModuleName)]
        public static extern FrameInfo SPUnityProfilerGetFrameInfo();
        #else
        public static FrameInfo SPUnityProfilerGetFrameInfo()
        {
            var stats = new FrameInfo();

            stats.FrameTime = Time.smoothDeltaTime*1000;
            for(int i = 0, maxLength = GameObject.FindObjectsOfType(typeof(MeshFilter)).Length; i < maxLength; i++)
            {
                var mf = (MeshFilter)GameObject.FindObjectsOfType(typeof(MeshFilter))[i];
                stats.Verts += (uint)mf.sharedMesh.vertexCount;
                stats.Tris += (uint)mf.sharedMesh.triangles.Length;
            }
            return stats;
        }
        #endif

        #if !UNITY_EDITOR && UNITY_IOS && SPARTA_PROFILER_ENABLED
        [DllImport(PluginModuleName)]
        public static extern GarbageInfo SPUnityProfilerGetGarbageInfo();
        
#else
        public static GarbageInfo SPUnityProfilerGetGarbageInfo()
        {
            var stats = new GarbageInfo();
            stats.AllocatedHeap = Profiler.GetMonoHeapSize();
            stats.UsedHeap = Profiler.usedHeapSize;
            return stats;
        }
        #endif
    }
}