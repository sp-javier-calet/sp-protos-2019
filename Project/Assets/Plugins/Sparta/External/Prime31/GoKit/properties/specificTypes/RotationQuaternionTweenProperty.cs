using UnityEngine;
using System.Collections;


public class RotationQuaternionTweenProperty : AbstractQuaternionTweenProperty
{
	private bool _useLocalRotation;
	public bool useLocalRotation { get { return _useLocalRotation; } }
	
	
	public RotationQuaternionTweenProperty( Quaternion endValue, bool isRelative = false, bool useLocalRotation = false ) : base( endValue, isRelative )
	{
		_useLocalRotation = useLocalRotation;
	}
	
	
	#region Object overrides
	
	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
	
	
	public override bool Equals( object obj )
	{
		// start with a base check and then compare if we are both using local values
		if( base.Equals( obj ) )
			return this._useLocalRotation == ((RotationQuaternionTweenProperty)obj)._useLocalRotation;

		return false;
	}
	
	#endregion
	
	
	public override void prepareForUse()
	{
		if (_ownerTween == null)
			return;

		if (_ownerTween.target == null)
			return;

		_target = _ownerTween.target as Transform;

		if (_target == null)
			return;
		
		_endValue = _originalEndValue;
		
		// if this is a from tween we need to swap the start and end values
		if( _ownerTween.isFrom )
		{
			_startValue = _endValue;
			
			if( _useLocalRotation )
				_endValue = _target.localRotation;
			else
				_endValue = _target.rotation;
		}
		else
		{
			if( _useLocalRotation )
				_startValue = _target.localRotation;
			else
				_startValue = _target.rotation;
		}
		
		base.prepareForUse();
	}
	
	
	public override void tick( float totalElapsedTime )
	{
		var easedTime = _easeFunction( totalElapsedTime, 0, 1, _ownerTween.duration );
		Quaternion newOrientation = Quaternion.Slerp (_startValue, _endValue, easedTime);
		
		if( _useLocalRotation )
			_target.localRotation = newOrientation;
		else
			_target.rotation = newOrientation;
	}

}
