using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	public class MultiTargetValueMonitor : StepMonitor
	{
		List<StepMonitor> _targetMonitors = new List<StepMonitor>();

		System.Type _monitorType;
		public System.Type MonitorType { get { return _monitorType; } }

		public void Init(List<Transform> targets, System.Type type)
		{
			_targetMonitors.Clear();

			for (int i = 0; i < targets.Count; ++i) 
			{
				AddMonitors(targets[i], type);
			}
		}

		public void Init(Transform target, StepMonitor valueMonitor)
		{
			_targetMonitors.Clear();
			AddMonitor(target, valueMonitor);
		}

		void AddMonitors(Transform target, System.Type type)
		{
			if( GetTargetIndex(target) >= 0 )
			{
				return;
			}

			_monitorType = type;
			StepMonitor targetMonitor = (StepMonitor) System.Activator.CreateInstance(_monitorType);
			targetMonitor.Target = target;
			_targetMonitors.Add(targetMonitor);
		}

		public void AddMonitor(Transform target, StepMonitor targetMonitor)
		{
			_targetMonitors.Clear();

			if(targetMonitor == null)
			{
				return;
			}

			if( GetTargetIndex(targetMonitor.Target) >= 0 )
			{
				return;
			}
			_monitorType = targetMonitor.GetType();
			targetMonitor.Target = target;
			_targetMonitors.Add(targetMonitor);
		}

		public bool RemoveTarget(Transform target)
		{
			int idx = GetTargetIndex(target);
			if(idx >= 0)
			{
				_targetMonitors.RemoveAt(idx);
			
				return true;
			}
			else
			{
				return false;
			}
		}

		int GetTargetIndex(Transform target)
		{
			return _targetMonitors.FindIndex((StepMonitor aMonitor)=>{ return aMonitor.Target == target; });
		}

		public override void Backup()
		{
			for (int i = 0; i < _targetMonitors.Count; ++i) 
			{
				_targetMonitors[i].Backup();
			}
		}

		public override bool HasChanged()
		{
			for (int i = 0; i < _targetMonitors.Count; ++i) 
			{
				if(_targetMonitors[i].HasChanged())
				{
					return true;
				}
			}

			return false;
		}
	}
}
