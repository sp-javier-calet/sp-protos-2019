using UnityEngine;

namespace SocialPoint.Tool.Shared.TLGUI
{
    /// <summary>
    /// A number selector with spinner functionality but targetted for selecting 'versions'.
    /// An integer number can be selected from a range and a special +1 value can also be selected if spinning on top of the maxValue.
    /// </summary>
    public sealed class TLWVersionSelectorField : TLWNumberSpinnerField
    {
        static readonly string NEW_VERSION = "+ 1";

        public bool IsNewVersion { get { return _s_number == NEW_VERSION; } }

        public TLWVersionSelectorField( TLView view, string name, GUILayoutOption[] options ) : base (view, name, options)
        {
        }

        public TLWVersionSelectorField( TLView view, string name, int minNum, int maxNum, GUILayoutOption[] options ) : base (view, name, minNum, maxNum, options)
        {
        }

        protected override void Increment()
        {
            if(_number + _step > _maxNum)
            {
                _s_number = NEW_VERSION;
            }
            else
            {
                base.Increment();
            }
        }

        protected override void Decrement()
        {
            if(_s_number == NEW_VERSION)
            {
                number = _maxNum.ToString("G");
            }
            else
            {
                base.Decrement();
            }
        }
    }
}