
namespace SocialPoint.GUIAnimation
{
    public sealed class AnimationEditorPlayer
    {
        enum State
        {
            Idle,
            Finish,
            Playing
        }

        State _state = State.Idle;

        public struct EditorTimeGetter : ITimeGetter
        {
            public float Get()
            {
                return (float)UnityEditor.EditorApplication.timeSinceStartup;
            }
        }

        EditorTimeGetter _editorTimeGetter = new EditorTimeGetter();
        GUIAnimationTool _animTool;

        public float GetCurrentTime()
        {
            return _animTool.AnimationModel.CurrentAnimation != null ? _animTool.AnimationModel.CurrentAnimation.CurrentTime : 0f;
        }

        public void Init(GUIAnimationTool animTool)
        {
            _animTool = animTool;
        }

        public void Play()
        {
            if(_animTool.AnimationModel.CurrentAnimation != null)
            {
                _animTool.AnimationModel.CurrentAnimation.Play();
                _state = State.Playing;
            }
        }

        public void Stop()
        {
            if(_animTool.AnimationModel.CurrentAnimation != null)
            {
                _animTool.AnimationModel.CurrentAnimation.Stop();
                _state = State.Idle;
            }
        }

        public bool IsPlaying()
        {
            if(_animTool != null && _animTool.AnimationModel != null && _animTool.AnimationModel.CurrentAnimation != null)
            {
                return _state == State.Playing || _state == State.Finish;
            }

            return false;
        }

        public void Update(GUIAnimationTool animTool)
        {
            _animTool = animTool;

            if(_state == State.Idle)
            {
                return;
            }
            if(_state == State.Playing)
            {
                if(_animTool.AnimationModel.CurrentAnimation != null)
                {
                    _animTool.AnimationModel.CurrentAnimation.SetEditorTimeGetter(_editorTimeGetter);
                    _animTool.AnimationModel.CurrentAnimation.Update();
                    _animTool.ForceRepaint();

                    if(!_animTool.AnimationModel.CurrentAnimation.IsPlaying())
                    {
                        _state = State.Finish;
                    }
                }
                else
                {
                    _state = State.Idle;
                }
                return;
            }
            if(_state == State.Finish)
            {
                _state = State.Idle;
            }
        }
    }
}
