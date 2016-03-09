using UnityEngine;
using System.Collections;


public class AnchoredPosition3DTweenProperty : AbstractTweenProperty
{
	protected RectTransform _target;
	
	protected Vector3 _originalEndValue;
	protected Vector3 _startValue;
	protected Vector3 _endValue;
	protected Vector3 _diffValue;
	
	public AnchoredPosition3DTweenProperty( Vector3 endValue, bool isRelative = false) : base( isRelative )
	{
		_originalEndValue = endValue;
	}
	
	public override bool validateTarget( object target )
	{
		return target is RectTransform;
	}
	
	public override void prepareForUse()
	{
		if (_ownerTween == null)
			return;

		if (_ownerTween.target == null)
			return;

		_target = _ownerTween.target as RectTransform;

		if (_target == null)
			return;
		
		
		_endValue = _originalEndValue;
		
		if( _ownerTween.isFrom )
		{
			_startValue = _isRelative ? _endValue + _target.anchoredPosition3D : _endValue;
			_endValue = _target.anchoredPosition3D;
		}
		else
		{
			_startValue = _target.anchoredPosition3D;
		}
		
		if( _isRelative && !_ownerTween.isFrom )
			_diffValue = _endValue;
		else
			_diffValue = _endValue - _startValue;
	}
	
	
	public override void tick( float totalElapsedTime )
	{
		var easedTime = _easeFunction( totalElapsedTime, 0, 1, _ownerTween.duration );
		var vec = GoTweenUtils.unclampedVector3Lerp( _startValue, _diffValue, easedTime );
		
		_target.anchoredPosition3D = vec;
	}
	
	
	public void resetWithNewEndValue( Vector3 endValue )
	{
		_originalEndValue = endValue;
		prepareForUse();
	}
	
}
