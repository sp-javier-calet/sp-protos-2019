using System.Collections.Generic;
using SocialPoint.Utils;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    public sealed class TimeValueGridPanel
    {
        public interface IGridChainProcesser
        {
            void ProcessChain();

            void Update();
        }

        public sealed class ProcessRemovePoint: IGridChainProcesser
        {
            readonly TimeValueGridPanel _host;
            IGridChainProcesser _next;
            bool _isProcessed;
            TimeValueBox _toRemove;

            public ProcessRemovePoint(TimeValueGridPanel host, IGridChainProcesser next)
            {
                _host = host;
                _next = next;
            }

            public void ProcessChain()
            {
                if(_isProcessed)
                {
                    _host.RemovePoint(_toRemove);
                }
                else
                {
                    if(_next != null)
                    {
                        _next.ProcessChain();
                    }
                }
            }

            public void Update()
            {
                _isProcessed = false;

                if(!_isProcessed)
                {
                    if(
                        Event.current.keyCode == KeyCode.Delete
                        && Event.current.type == EventType.keyUp
                        && _host._lastUsed != null)
                    {
                        _toRemove = _host._lastUsed;
                        _isProcessed = true;
                    }
                }
            }
        }

        public sealed class ProcessMovePoint : IGridChainProcesser
        {
            readonly TimeValueGridPanel _host;
            IGridChainProcesser _next;
            bool _isProcessed;
            TimeValueBox _lastMoved;

            public ProcessMovePoint(TimeValueGridPanel host, IGridChainProcesser next)
            {
                _host = host;
                _next = next;
            }

            public void ProcessChain()
            {
                if(!_isProcessed)
                {
                    if(_next != null)
                    {
                        _next.ProcessChain();
                    }
                }
                else
                {
                    if(_lastMoved != null)
                    {
                        _host.SetLastPointUsed(_lastMoved);
                    }
                }

                // Reset
                _isProcessed = false;
            }

            public void Update()
            {   
                _isProcessed = false;

                for(int i = 0; i < _host._timeValuesWinData.Count; ++i)
                {
                    _host._timeValuesWinData[i].WinMover.Update(ref _host._timeValuesWinData[i].Window);
                    _isProcessed = _host._timeValuesWinData[i].WinMover.IsMoving;
                    if(_isProcessed)
                    {
                        _lastMoved = _host._timeValuesWinData[i];
                        break;
                    }
                }
            }
        }

        public sealed class ProcessCreatePoint : IGridChainProcesser
        {
            readonly TimeValueGridPanel _host;
            IGridChainProcesser _next;
            bool _canCreate;

            public ProcessCreatePoint(TimeValueGridPanel host, IGridChainProcesser next)
            {
                _host = host;
                _next = next;
            }

            public void ProcessChain()
            {
                if(_canCreate)
                {
                    _host.CreatePoint(Event.current.mousePosition);
                }
                else
                {
                    if(_next != null)
                    {
                        _next.ProcessChain();
                    }
                }

                // Reset
                _canCreate = false;
            }

            public void Update()
            {
                _canCreate = false;
                if(Event.current.type == EventType.MouseDown)
                {
                    _canCreate |= _host._gridWindow.Contains(Event.current.mousePosition);
                }
            }
        }

        public sealed class ColorProperties
        {
            public Color PrevFrameColor;
            public Color FrameColor = Color.black;

            public Color PrevBackgroundColor;
            public Color BackgroundColor = Color.gray;

            public Color PointsColor = Color.black;
            public Color PrevPointsColor;

            public void Save()
            {
                PrevFrameColor = Handles.color;
                PrevBackgroundColor = GUI.backgroundColor;
            }

            public void LoadBackground()
            {
                GUI.backgroundColor = BackgroundColor;
                Handles.color = FrameColor;
            }

            public void LoadGridValues()
            {
                Handles.color = PointsColor;
            }

            public void Restore()
            {
                GUI.backgroundColor = PrevBackgroundColor;
                Handles.color = PrevFrameColor;
            }
        }

        public sealed class GridProperties
        {
            public float XAxisParts = 4f;
            public float YAxisParts = 4f;

            public float XAxisMin;
            public float XAxisMax = 1f;

            public float YAxisMin;
            public float YAxisMax = 1.0f;

            public float YAxisMinNew;
            public bool YAxisMinChanged;

            public float YAxisMaxNew = 1.0f;
            public bool YAxisMaxChanged;
        }

        sealed class TimeValueBox
        {
            public Rect Window;
            public WindowMover WinMover = new WindowMover(new Vector2(16f, 16f));
        }

        ColorProperties _colorProperties = new ColorProperties();
        GridProperties _gridProperties = new GridProperties();

        // Total Area
        Rect _window = new Rect(new Vector2(0f, 100f), new Vector2(486f, 464f));

        // Grid relative to area
        Rect _gridWindow = new Rect(new Vector2(68f, 0f), new Vector2(400f, 400f));

        List<EasePoint> _timeValues = new List<EasePoint> {
            new EasePoint(0.1f, 0.1f)
	        , new EasePoint(0.2f, 0.1f)
	        , new EasePoint(0.3f, 0.4f)
	        , new EasePoint(0.6f, 0.7f)
        };

        List<TimeValueBox> _timeValuesWinData = new List<TimeValueBox>();
        List<IGridChainProcesser> _processers = new List<IGridChainProcesser>();
        IGridChainProcesser _chainProcessing;

        TimeValueBox _lastUsed;

        bool _isInit;
        System.Action _onGridTouched;

        public List<EasePoint> RenderGUI(Rect window, Rect gridWindow, List<EasePoint> timeValues, System.Action onGridChanged = null)
        {
            _window = window;
            _gridWindow = gridWindow;
            _timeValues = timeValues;
            _onGridTouched = onGridChanged;

            if(!_isInit || !AreListEquals(timeValues))
            {
                Init();
                _isInit = true;
            }

            SaveOriginalValues();

            DoRenderGUI();

            SortWindowsData();

            return GetTimeValuesFromWinData();
        }

        bool AreListEquals(List<EasePoint> other)
        {
            if(other.Count != _timeValues.Count)
            {
                return false;
            }

            for(int i = 0; i < other.Count; ++i)
            {
                if(Mathf.Abs(other[i].x - _timeValues[i].x) > 1e-2f
                   || Mathf.Abs(other[i].y - _timeValues[i].y) > 1e-2f)
                {
                    return false;
                }
            }

            return true;
        }

        public void ResetState()
        {
            _isInit = false;

            _lastUsed = null;

            _timeValuesWinData.Clear();

            _processers.Clear();
        }

        void SetLastPointUsed(TimeValueBox lastUsed)
        {
            _lastUsed = lastUsed;
        }

        void RemovePoint(TimeValueBox point)
        {
            if(_lastUsed == point)
            {
                _lastUsed = null;
            }

            _timeValuesWinData.Remove(point);
        }

        void Init()
        {
            // Init 
            InitGridMinMaxValues();

            // Create Window Data from TimeValue data
            CreateWindowPoints();

            // Create Chain Processing
            _processers.Clear();

            IGridChainProcesser removePoint = new ProcessRemovePoint(this, null);
            _processers.Add(removePoint);

            IGridChainProcesser createPoint = new ProcessCreatePoint(this, removePoint);
            _processers.Add(createPoint);

            IGridChainProcesser movePoint = new ProcessMovePoint(this, createPoint);
            _processers.Add(movePoint);

            _chainProcessing = movePoint;
        }

        void InitGridMinMaxValues()
        {
            // Init Grid Start And End Values
            float minValue = 0f;
            float maxValue = 1f;
            for(int i = 0; i < _timeValues.Count; ++i)
            {
                minValue = Mathf.Min(minValue, _timeValues[i].y);
                maxValue = Mathf.Max(maxValue, _timeValues[i].y);
            }

            _gridProperties.YAxisMin = _gridProperties.YAxisMinNew = minValue;
            _gridProperties.YAxisMinChanged = false;
			
            _gridProperties.YAxisMax = _gridProperties.YAxisMaxNew = maxValue;
            _gridProperties.YAxisMaxChanged = false;
        }

        void CreateWindowPoints()
        {
            _timeValuesWinData.Clear();

            for(int i = 0; i < _timeValues.Count; ++i)
            {
                var window = new Rect();
                window.center = new Vector2(timeToPosition(_timeValues[i].x) - 2f, valueToPosition(_timeValues[i].y) - 2f);
                window.size = new Vector2(4f, 4f);
				
                _timeValuesWinData.Add(
                    new TimeValueBox {
                        Window = window
                    });
            }
        }

        void SaveOriginalValues()
        {
            _colorProperties.Save();
        }

        void DoRenderGUI()
        {
            GUILayout.BeginArea(_window);

            TryToTriggerTouchCallback();
            UpdateAreaEvents();
            RenderAreaBackground();
            RenderValues();

            GUILayout.EndArea();
        }

        void TryToTriggerTouchCallback()
        {
            if((Event.current.type == EventType.mouseDown || Event.current.type == EventType.mouseUp)
               && _gridWindow.Contains(Event.current.mousePosition))
            {
                TriggerModificationCallback();
            }
        }

        void TriggerModificationCallback()
        {
            if(_onGridTouched != null)
            {
                _onGridTouched();
            }
        }

        void UpdateAreaEvents()
        {
            for(int i = 0; i < _processers.Count; ++i)
            {
                _processers[i].Update();
            }

            _chainProcessing.ProcessChain();
        }

        void RenderAreaBackground()
        {
            _colorProperties.LoadBackground();
            GUI.Box(_gridWindow, "");

            _colorProperties.Restore();

            // All Axis numbers
            for(int x = 0; x <= (int)_gridProperties.XAxisParts; ++x)
            {
                Vector2 xAxisPosition = new Vector2(_gridWindow.position.x, _gridWindow.size.y + 18f) + Vector2.right * (_gridWindow.size.x / _gridProperties.XAxisParts) * ((float)x);

                float normalizedValue = ((float)x) / _gridProperties.XAxisParts;
                Vector2 labelSize = GUI.skin.GetStyle("label").CalcSize(new GUIContent(normalizedValue.ToString()));
                labelSize.y = 0f;
                GUI.Label(new Rect(xAxisPosition - labelSize * 0.5f, Vector2.one * 25f), float.Parse(normalizedValue.ToString("0.0")).ToString());
            }

            for(int y = 0; y <= (int)_gridProperties.YAxisParts; ++y)
            {
                var startPos = new Vector2(0f, _gridWindow.position.y);
                var endPos = new Vector2(0f, _gridWindow.position.y + _gridWindow.size.y);
                float normalizedPos = ((float)y) / _gridProperties.YAxisParts;
                Vector2 currentPosition = Vector2.Lerp(startPos, endPos, normalizedPos);
				
                float currentYValue = Mathf.Lerp(_gridProperties.YAxisMin, _gridProperties.YAxisMax, 1f - normalizedPos);
                Vector2 labelSize = GUI.skin.GetStyle("label").CalcSize(new GUIContent(currentYValue.ToString()));
                labelSize.y = 0f;

                if(y == 0)
                {
                    GUI.changed = false;
                    float value = float.Parse(_gridProperties.YAxisMax.ToString("0.0"));
                    if(_gridProperties.YAxisMaxChanged)
                    {
                        value = float.Parse(_gridProperties.YAxisMaxNew.ToString("0.0"));
                    }
                    _gridProperties.YAxisMaxNew = EditorGUI.FloatField(new Rect(currentPosition, new Vector2(34f, 20f)), value);
                    _gridProperties.YAxisMaxChanged |= GUI.changed;
                    if(
                        _gridProperties.YAxisMaxChanged
                        && GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftWindows))
                    {
                        _gridProperties.YAxisMax = _gridProperties.YAxisMaxNew;
                        _gridProperties.YAxisMaxChanged = false;
                        CreateWindowPoints();

                        TriggerModificationCallback();
                    }
                }
                else if(y == (int)_gridProperties.YAxisParts)
                {
                    GUI.changed = false;
                    float value = float.Parse(_gridProperties.YAxisMin.ToString("0.0"));
                    if(_gridProperties.YAxisMinChanged)
                    {
                        value = float.Parse(_gridProperties.YAxisMinNew.ToString("0.0"));
                    }
                    _gridProperties.YAxisMinNew = EditorGUI.FloatField(new Rect(currentPosition, new Vector2(34f, 20f)), value);
                    _gridProperties.YAxisMinChanged |= GUI.changed;
                    if(
                        _gridProperties.YAxisMinChanged
                        && GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftWindows))
                    {
                        _gridProperties.YAxisMin = _gridProperties.YAxisMinNew;
                        _gridProperties.YAxisMinChanged = false;
                        CreateWindowPoints();

                        TriggerModificationCallback();
                    }
                }
                else
                {
                    GUI.Label(new Rect(currentPosition, Vector2.one * 34f), currentYValue.ToString());
                }
            }

            var resetButtonPos = new Vector2(200f, _gridWindow.position.y + _gridWindow.size.y + 38f);
            if(GUI.Button(new Rect(resetButtonPos, new Vector2(100f, 20f)), "Reset Values"))
            {
                ResetValues();
            }
        }

        void ResetValues()
        {
            _timeValues.Clear();
            _timeValues.Add(new EasePoint(0f, 0f));
            _timeValues.Add(new EasePoint(1f, 1f));

            Init();
        }

        void RenderValues()
        {
            _colorProperties.LoadGridValues();

            for(int i = 0; i < _timeValuesWinData.Count; ++i)
            {
                Color prev = Handles.color;
                if(_timeValuesWinData[i] == _lastUsed)
                {
                    Handles.color = Color.white;
                }
                Handles.DrawSolidDisc(_timeValuesWinData[i].Window.center, Vector3.forward, 4f);
                Handles.color = prev;

                if(i > 0)
                {
                    Handles.DrawLine(_timeValuesWinData[i - 1].Window.center, _timeValuesWinData[i].Window.center);
                }

                // Try to show info
                var testWindow = new Rect(_timeValuesWinData[i].Window.position, new Vector2(10f, 10f));
                if(testWindow.Contains(Event.current.mousePosition))
                {
                    float xVal = XPositionToValue(_timeValuesWinData[i].Window.center.x);
                    xVal = float.Parse(xVal.ToString("0.0"));

                    float yVal = YPositionToValue(_timeValuesWinData[i].Window.center.y);
                    yVal = float.Parse(yVal.ToString("0.0"));

                    string labelContent = string.Format("({0}, {1})", xVal, yVal);
                    Vector2 labelSize = GUI.skin.label.CalcSize(new GUIContent(labelContent));
                    labelSize.y = 0f;

                    GUI.Label(new Rect(_timeValuesWinData[i].Window.position - labelSize, new Vector2(60f, 25f)), labelContent);
                }
            }

            _colorProperties.Restore();
        }

        //---- Relative to Area
        // Value to pos
        float timeToPosition(float t)
        {
            float pivotPos = _gridWindow.position.x;
            return pivotPos + t * _gridWindow.size.x;
        }

        float valueToPosition(float val)
        {
            float normalizedVal = (val - _gridProperties.YAxisMin) / (_gridProperties.YAxisMax - _gridProperties.YAxisMin);
            normalizedVal = Mathf.Clamp(normalizedVal, _gridProperties.YAxisMin, _gridProperties.YAxisMax);

            float pivotPos = _gridWindow.position.y + _gridWindow.size.y;
            return pivotPos - normalizedVal * _gridWindow.size.y;
        }

        // Pos to value
        float YPositionToValue(float yPos)
        {
            float normalizedVal = 1f - ((yPos - _gridWindow.position.y) / _gridWindow.size.y);
            return _gridProperties.YAxisMin + (_gridProperties.YAxisMax - _gridProperties.YAxisMin) * normalizedVal;
        }

        float XPositionToValue(float xPos)
        {
            float normalizedVal = ((xPos - _gridWindow.position.x) / _gridWindow.size.x);
            return _gridProperties.XAxisMin + (_gridProperties.XAxisMax - _gridProperties.XAxisMin) * normalizedVal;
        }

        List<EasePoint> GetTimeValuesFromWinData()
        {
            var newTimeValues = new List<EasePoint>();

            for(int i = 0; i < _timeValuesWinData.Count; ++i)
            {
                newTimeValues.Add(
                    new EasePoint(
                        XPositionToValue(_timeValuesWinData[i].Window.center.x),
                        YPositionToValue(_timeValuesWinData[i].Window.center.y)
                    ));
            }

            return newTimeValues;
        }

        void SortWindowsData()
        {
            _timeValuesWinData.Sort(SortFuncton);
        }

        static int SortFuncton(TimeValueBox a, TimeValueBox b)
        {
            return a.Window.center.x < b.Window.center.x ? -1 : 1;
        }

        void CreatePoint(Vector2 mousePos)
        {
            _timeValuesWinData.Add(
                new TimeValueBox {
                    Window = new Rect(mousePos, new Vector2(4f, 4f))
                }
            );

            SetLastPointUsed(_timeValuesWinData[_timeValuesWinData.Count - 1]);
        }

        static GUIStyle GetHeaderStyle()
        {
            var headerStyle = new GUIStyle();
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.fontSize = 12;
            return headerStyle;
        }
    }
}
