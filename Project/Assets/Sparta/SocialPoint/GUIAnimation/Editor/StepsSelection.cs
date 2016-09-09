using UnityEngine;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	// Class that acts as a proxy to store a selection of steps, this is used to apply one action to all the selected steps
    public sealed class StepsSelection : IStep
	{
		List<Step> _steps = new List<Step>();
		public List<Step> Steps
		{
			get
			{
				return _steps;
			}
		}

		public Step Step
		{
			get
			{
				if(_steps.Count > 0)
				{
					return _steps[_steps.Count -1];
				}
				else
				{
					return null;
				}
			}
		}

		public int Count { get { return _steps.Count; } }

	//----
	//-- IAnimationItem
	//----
		public float GetStartTime(AnimTimeMode mode)
		{
			return Step.GetStartTime(mode);
		}

		public void SetStartTime(float time, AnimTimeMode mode)
		{
			for (int i = 0; i < _steps.Count; ++i) 
			{
				_steps[i].SetStartTime(time, mode);
			}
		}
		
		public float GetEndTime(AnimTimeMode mode)
		{
			return Step.GetEndTime(mode);
		}

		public void SetEndTime(float time, AnimTimeMode mode)
		{
			for (int i = 0; i < _steps.Count; ++i) 
			{
				_steps[i].SetEndTime(time, mode);
			}
		}
		
		public float GetDuration(AnimTimeMode mode)
		{
			return Step.GetDuration(mode);
		}

		public void SetDuration(float duration, AnimTimeMode mode)
		{
			for (int i = 0; i < _steps.Count; ++i) 
			{
				_steps[i].SetDuration(duration, mode);
			}
		}

		public void Invert(bool invertTime = false)
		{
			for (int i = 0; i < _steps.Count; ++i) 
			{
				_steps[i].Invert(invertTime);
			}
		}

	//----
	//-- Selection Options
	//----
		public bool IsSelected(Step item)
		{
			return _steps.Contains(item);
		}
		
		public bool IsFirstSelected(Step item)
		{
			if(_steps.Count <= 0)
			{
				return false;
			}
			return _steps[0] == item;
		}
		
		public bool IsLastSelected(Step item)
		{
			if(_steps.Count <= 0)
			{
				return false;
			}
			return _steps[_steps.Count -1] == item;
		}
		
		public void Set(Step item)
		{
			_steps.Clear();
			
			if(item != null)
			{
				_steps.Add(item);
			}
		}
		
		public bool Remove(Step item)
		{
			return _steps.Remove(item);
		}
		
		public void Add(Step item)
		{
			if(item != null && !_steps.Contains(item))
			{
				_steps.Add(item);
			}
		}
		
		public void Clear()
		{
			_steps.Clear();
		}
		
		public void OnMoved(Step source, Vector2 delta, Dictionary<Step, AnimationStepBox> boxes)
		{
			for (int i = 0; i < _steps.Count; ++i) 
			{
				if(_steps[i] != null && _steps[i] != source)
				{
					AnimationStepBox box = GetBoxFromAnimItemItem(_steps[i], boxes);
					if(box != null)
					{
						box.Rect.position += delta;
					}
				}
			}
		}
		
		public void OnResized(Step source, Vector2 delta, Dictionary<Step, AnimationStepBox> boxes)
		{
			for (int i = 0; i < _steps.Count; ++i) 
			{
				if(_steps[i] != null && _steps[i] != source)
				{
					AnimationStepBox box = GetBoxFromAnimItemItem(_steps[i], boxes);
					if(box != null)
					{
						box.Rect.size += delta;
					}
				}
			}
		}
		
		AnimationStepBox GetBoxFromAnimItemItem(Step animItem, Dictionary<Step, AnimationStepBox> boxes)
		{
			AnimationStepBox box = null;
			boxes.TryGetValue(animItem, out box);
			
			return box;
		}
	}
}
