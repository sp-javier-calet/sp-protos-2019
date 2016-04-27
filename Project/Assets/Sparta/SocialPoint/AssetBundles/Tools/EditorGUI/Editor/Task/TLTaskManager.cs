using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.Tool.Shared.TLGUI
{
	public class TLTaskManager
	{
		private List<TLTask> _tasks;
		private int _currentTaskIdx;

		public int taskCount { get { return _tasks.Count; } }
		public int currentTaskIdx { get { return _currentTaskIdx; } }
		public List<TLTask> tasks { get { return _tasks; } }

		public TLTaskManager()
		{
			_tasks = new List<TLTask>();
			_currentTaskIdx = 0;
		}

		public void Add( TLTask task )
		{
			_tasks.Add( task );
		}

		public void AddRange( List<TLTask> tasks )
		{
			_tasks.AddRange( tasks );
		}

		public void Clear()
		{
			_tasks.Clear();
			_currentTaskIdx = 0;
		}
		
		public void TaskLoopStart()
		{
			_currentTaskIdx = 0;
		}
		
		public void PerformCurrentTask()
		{
			if ( !HasNext() )
				throw new UnityException( "Current task is null" );
			
			_tasks[_currentTaskIdx].Perform();
		}

		public void CleanUpCurrentTask()
		{
			if ( !HasNext() )
				throw new UnityException( "Current task is null" );

			_tasks[_currentTaskIdx].CleanUp();
		}
		
		public void NextTask()
		{
			_currentTaskIdx++;
		}
		
		public bool HasNext()
		{
			return _currentTaskIdx < _tasks.Count;
		}

		public TLTask GetCurrentTask()
		{
			if ( !HasNext() )
				throw new UnityException( "Current task is null" );
			
			return _tasks[_currentTaskIdx];
		}
		
		public float GetProgress()
		{
			if ( taskCount == 0 ) return 0.0f;
			
			return (float) currentTaskIdx / (float) taskCount;
		}
	}
}
