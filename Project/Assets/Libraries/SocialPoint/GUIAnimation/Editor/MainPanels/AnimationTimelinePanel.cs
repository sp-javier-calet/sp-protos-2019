using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	public class AnimationStepBox
	{
		public Step AnimationItem;
		public WindowResizer WinResizer = new WindowResizer(10, Vector2.one * 16f);
		public WindowMover WinMover = new WindowMover(Vector2.zero);
		public Rect Rect;
		public Rect InteractuableRect;
	}

	// This class show the grid with the options that can be done in the current Collection
	public class AnimationTimelinePanel 
	{
		public class GridProperties
		{
			public float PixelsPerSecond = 316f;
			public float PixelsPerSlot = 36f;

			public float MaxBoxRectPixels = 250f;

			float _gridGlobalStartTime = 0f;
			public float GridGlobalStartTime { get { return _gridGlobalStartTime; } }

			float _gridGlobalEndTime = 1f;
			public float GridGlobalEndTime { get { return _gridGlobalEndTime; } }

			Vector2 _pivotGridPosition = Vector2.zero;
			public Vector2 PivotGridPosition { get { return _pivotGridPosition; } }

			public float SecondsToPixels(float time)
			{
				return time * PixelsPerSecond;
			}

			public float PixelsToSeconds(float pixels)
			{
				return pixels / PixelsPerSecond;
			}

			public float SlotsToPixels(int timeline)
			{
				return ((float)timeline) * PixelsPerSlot;
			}

			public int PixelsToSlots(float pixels)
			{
				return Mathf.RoundToInt(pixels / PixelsPerSlot);
			}

			public float ConvertGlobalToNormalizedTime(float globalTime)
			{
				return globalTime/(_gridGlobalEndTime-_gridGlobalStartTime);
			}

			public float ConvertNormalizedTimeToGlobal(float localTime)
			{
				return localTime * (_gridGlobalEndTime-_gridGlobalStartTime);
			}

			public void UpdateState(float gridGlobalStartTime, float gridGlobalEndTime, Vector2 gridPivotPosition)
			{
				_gridGlobalStartTime = gridGlobalStartTime;
				_gridGlobalEndTime = gridGlobalEndTime;

				_pivotGridPosition = gridPivotPosition;
			}

			public void GetGlobalTimeSlotFromGridPos(ref float globalSeconds, ref int slot, Vector2 gridPosition)
			{
				gridPosition -= _pivotGridPosition;

				float localDistance = gridPosition.x; //gridPosition.x - SecondsToPixels(_gridGlobalStartTime);
				float distanceNorm = localDistance / (SecondsToPixels(_gridGlobalEndTime) - SecondsToPixels(_gridGlobalStartTime));

				globalSeconds = _gridGlobalStartTime +  (_gridGlobalEndTime - _gridGlobalStartTime) * distanceNorm;
				slot = PixelsToSlots(gridPosition.y);
			}

			public void GetNormalizedTimeSlotFromGridPos(ref float normalizedSeconds, ref int slot, Vector2 gridPosition)
			{
				gridPosition -= _pivotGridPosition;
				
				float distance = SecondsToPixels(_gridGlobalEndTime) - SecondsToPixels(_gridGlobalStartTime);
				float normalizedValue = gridPosition.x / distance;
				normalizedSeconds = normalizedValue;

				slot = PixelsToSlots(gridPosition.y);
			}

			public Vector2 GetGridPosFromTimeSlot(float seconds, int slot)
			{
				float distanceX = SecondsToPixels(_gridGlobalEndTime) - SecondsToPixels(_gridGlobalStartTime);
				float positionY = SlotsToPixels(slot);

				float normalizedTime = (seconds - _gridGlobalStartTime) / (_gridGlobalEndTime - _gridGlobalStartTime);
				Vector2 localPosition = new Vector2(distanceX * normalizedTime, positionY);

				return localPosition + _pivotGridPosition;
			}

			public Vector2 GetGridPosFromNormalizedTimeSlot(float normalizedTime, int slot)
			{
				Vector2 gridStartPosition = GetGridPosFromTimeSlot(_gridGlobalStartTime, slot);
				Vector2 gridEndPosition = GetGridPosFromTimeSlot(_gridGlobalEndTime, slot);

				Vector2 gridPosition = (gridEndPosition-gridStartPosition) * normalizedTime;
				gridPosition.y = SlotsToPixels(slot);

				gridPosition += _pivotGridPosition;

				return gridPosition;
			}
		}

		public enum StateMode
		{
			Preview,
			Edit
		}

		public delegate void OnAnimationItemStateChange(Step animationItem, Group collection);
		OnAnimationItemStateChange _onAnimationItemSelectedDelegate;

		TimelineInputController _inputController = new TimelineInputController();

		GUIAnimationTool _animationTool;
		public GUIAnimationTool AnimationToolEditor { get { return _animationTool; } }

		float _gridMaxHeight = 2400f;
		public float GridMaxHeight { get { return _gridMaxHeight; } }
		Vector2 _gridStartPosition = new Vector2(0f, 100f);
		public Vector2 GridStartPosition { get { return _gridStartPosition; } }

		Vector2 _gridScrollPosition = Vector2.zero;
		public Vector2 GridScrollPosition { get { return _gridScrollPosition; } }

		Group _currentCollection;
		public Group CurrentCollection { get { return _currentCollection; } }

		StepsSelection _stepsSelection = new StepsSelection();
		public StepsSelection StepsSelection { get { return _stepsSelection; } }
		public Step SelectedStep { get { return _stepsSelection.Step; } }

		float _previewInteractuableHeight = 28f;
		Vector2 _boxesOffsetPosition = new Vector2(8f, 34f);
		public Vector2 BoxesOffsetPosition { get { return _boxesOffsetPosition; } }

		GridProperties _gridProperties = new GridProperties();
		public GridProperties GridProps { get { return _gridProperties; } }
		float _defaultDuration = 1f;

		float _gridVisibleHeight = 500f;
		public float PanelHeight { get { return _gridStartPosition.y + _gridVisibleHeight; } }

		Dictionary<Step, AnimationStepBox> _cacheWindows = new Dictionary<Step, AnimationStepBox>();
		public Dictionary<Step, AnimationStepBox> CacheWindows { get { return _cacheWindows; } }

		Vector2 _timelinePosition = Vector2.zero;

		float _currentGlobalTime = 0f;
		public float CurrentTime { get { return _currentGlobalTime; } set { _currentGlobalTime = value; } }

		bool _isInit = false;

		StepBoxesProcessors _animationBoxProcessors = new StepBoxesProcessors();

		private const float kEpsilon = 1e-4f;
		private const float kGridExtraWidth = 4f;

		public void Render(GUIAnimationTool animationTool)
		{
			_animationTool = animationTool;

			if(!_isInit)
			{
				Init();
				_isInit = true;
			}

			if(_currentCollection == null)
			{
				if(_animationTool.AnimationModel.CurrentAnimation != null)
				{
					_currentCollection = (Group) _animationTool.AnimationModel.CurrentAnimation.Root;
				}
			}

			if(_currentCollection == null)
			{
				return;
			}

			DoUpdate();
			DoRender();
		}

		void DoUpdate()
		{
			if(GUIAnimationTool.AnimationEditorPlayer.IsPlaying())
			{
				if(SelectedStep != null)
				{
					OnAnimationItemSelected(null);
				}

				_currentGlobalTime = GUIAnimationTool.AnimationEditorPlayer.GetCurrentTime();
				_timelinePosition = _gridProperties.GetGridPosFromTimeSlot(_currentGlobalTime, 0);
			}

			_gridProperties.UpdateState(_currentCollection.GetStartTime(AnimTimeMode.Global), _currentCollection.GetEndTime(AnimTimeMode.Global), _boxesOffsetPosition);
			_inputController.Update();
		}

		void DoRender()
		{
			RenderHeaderOptions();
			RenderGrid();
		}

		void Init()
		{
			_cacheWindows.Clear();
			_inputController.Init(this);
			_stepsSelection.Clear();
			_cacheWindows.Clear();
			_animationBoxProcessors.ResetState();
		}

		public void ResetState()
		{
			_isInit = false;
			_currentCollection = null;
			TriggerOnAnimationItemStateChange();
		}

		public void RemoveOnAnimationItemSelectedDelegate(OnAnimationItemStateChange callback)
		{
			_onAnimationItemSelectedDelegate -= callback;
		}

		public void AddOnAnimationItemSelectedDelegate(OnAnimationItemStateChange callback)
		{
			_onAnimationItemSelectedDelegate += callback;
		}

		void TriggerOnAnimationItemStateChange()
		{
			if(_onAnimationItemSelectedDelegate != null)
			{
				_onAnimationItemSelectedDelegate(SelectedStep, CurrentCollection);
			}

//			if(_animationTool)
//			{
//				_animationTool.SaveState();
//			}
		}

		void RenderHeaderOptions()
		{
			GUILayout.BeginVertical();

			RenderCollectionActionPanel();

			GUILayout.Space(8f);
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Vector3 lineStart = new Vector3(lastRect.position.x, lastRect.position.y + 6f, 0f);
			Vector3 lineEnd = lineStart; lineEnd.x += 2048f;
			DrawSimpleLine(lineStart, lineEnd, new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.25f));

			RenderHierarchyPanel();  RenderGridConfigPanel();

			GUILayout.EndVertical();
		}

		void RenderHierarchyPanel()
		{
			GUILayout.BeginArea(new Rect(0f, 75f, 2048f, 40f));
			List<Step> animItemHierarchy = new List<Step>();
			Step current = _currentCollection;
			while(current != null)
			{
				animItemHierarchy.Add(current);
				current = current.Parent;
			}

			// Render Hierarchy
			GUILayout.BeginHorizontal();
			for (int i = animItemHierarchy.Count-1; i >= 0; --i) 
			{
				Step animItem = animItemHierarchy[i];
				if(GUILayout.Button("/ " + animItem.StepName, GUILayout.ExpandWidth(false), GUILayout.Height(23f)))
				{
					OnAdvancedAnimationItemAdvancedEdition(animItem);

				}
			}
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		void RenderCollectionActionPanel()
		{
			GUILayout.BeginArea(new Rect(0f, 50f, 2048f, 40f));

			GUILayout.BeginHorizontal();

			// Group Exclusive Options
			bool collectionGUIEnabled = CurrentCollection.GetType() == typeof(Group);
			UnityEngine.GUI.enabled = collectionGUIEnabled;
			if(GUILayout.Button("+ Transition", GUILayout.ExpandWidth(false)))
			{
				CreateActionCollection();
			}

			if(GUILayout.Button("+ Trigger", GUILayout.ExpandWidth(false)))
			{
				CreateInstantAction();
			}

			UnityEngine.GUI.enabled = collectionGUIEnabled && _stepsSelection.Steps.Count > 1;
			if(GUILayout.Button("+ Group Selection", GUILayout.ExpandWidth(false)))
			{
				GroupSelection();
			}
			UnityEngine.GUI.enabled = collectionGUIEnabled;

			UnityEngine.GUI.enabled = collectionGUIEnabled && (SelectedStep != null && SelectedStep.GetType() == typeof(Group));
			if(GUILayout.Button("+ Ungroup", GUILayout.ExpandWidth(false)))
			{
				UngroupSelection();
			}
			UnityEngine.GUI.enabled = true;

			// Play Mode
			GUILayout.Space(220f);
			GUILayout.Label("Play Mode: ", GUILayout.Width(60f));
			_animationTool.AnimationModel.CurrentAnimation.Mode = (Animation.PlayMode) EditorGUILayout.EnumPopup(_animationTool.AnimationModel.CurrentAnimation.Mode, GUILayout.Width(100f));

			// Play On Start
			GUILayout.Space(22f);
			GUILayout.Label("Play On Start: ", GUILayout.Width(75f));
			_animationTool.AnimationModel.CurrentAnimation.PlayOnStart = EditorGUILayout.Toggle(_animationTool.AnimationModel.CurrentAnimation.PlayOnStart, GUILayout.Width(25f));

			GUILayout.EndHorizontal();

			GUILayout.EndArea();
		}

		void RenderGridConfigPanel()
		{
			GUILayout.BeginArea(new Rect(0f, 75f, 2048f, 40f));
			float rightOptionsSpaceX = 410f;
			float zoomButtonsSpaceX = 46f;
			Rect rightAnimItemOptions = new Rect(rightOptionsSpaceX, 100f, 600f, 25f);

			GUILayout.BeginHorizontal();
			GUILayout.Space(562f);

			// Exclude Group Option
			UnityEngine.GUI.enabled = CurrentCollection.GetType() == typeof(Group);
			{
				// Duplicate selected
				if(GUILayout.Button("Duplicate selected", GUILayout.ExpandWidth(false), GUILayout.ExpandWidth(false)))
				{
					DuplicateSelectedAnimationItems();
				}
			}
			UnityEngine.GUI.enabled = true;

			// Scale Grid
			Rect lastWindow = GUILayoutUtility.GetLastRect();
			Rect textureWindow = rightAnimItemOptions; textureWindow.position = new Vector2(lastWindow.position.x + lastWindow.width + zoomButtonsSpaceX -15f, lastWindow.position.y); textureWindow.size = new Vector2(16f, 16f);
			UnityEngine.GUI.DrawTexture(textureWindow, Resources.Load<Texture>("ZoomIcon"));
			GUILayout.Space(zoomButtonsSpaceX);
			GUILayout.Label("Grid", GUILayout.Width(28f), GUILayout.Height(18f));
			if(GUILayout.Button("-", GUILayout.Width(28f), GUILayout.Height(18f)))
			{
				ScaleGrid(0.80f);
			}
			if(GUILayout.Button("+", GUILayout.Width(20f), GUILayout.Width(28f), GUILayout.Height(18f)))
			{
				ScaleGrid(1.2f);
			}
			
			// Scale only Boxes
			lastWindow = GUILayoutUtility.GetLastRect();
			textureWindow = rightAnimItemOptions; textureWindow.position = new Vector2(lastWindow.position.x + lastWindow.width + zoomButtonsSpaceX -15f, lastWindow.position.y); textureWindow.size = new Vector2(16f, 16f);
			UnityEngine.GUI.DrawTexture(textureWindow, Resources.Load<Texture>("ZoomIcon"));
			GUILayout.Space(zoomButtonsSpaceX);
			GUILayout.Label("Boxes", GUILayout.Width(36f), GUILayout.Height(18f));
			if(GUILayout.Button("-", GUILayout.Width(28f), GUILayout.Height(18f)))
			{
				ScaleBoxes(0.90f);
			}
			if(GUILayout.Button("+", GUILayout.Width(20f), GUILayout.Width(28f), GUILayout.Height(18f)))
			{
				ScaleBoxes(1.1f);
			}

			GUILayout.EndHorizontal();

			GUILayout.EndArea();
		}

		void RenderGrid()
		{
			float gridMaxWidth = _gridProperties.GetGridPosFromNormalizedTimeSlot(1f, 0).x;

			float gridVisbleWidth = _animationTool.position.width + kGridExtraWidth;

			Rect scrollRectPos = new Rect (_gridStartPosition.x, _gridStartPosition.y, gridVisbleWidth, _gridVisibleHeight);		// External Scroll Rect
			Rect scrollableArea = new Rect (0f, 0f, gridMaxWidth, _gridMaxHeight);												// Internal Size of the Scroll

			bool isMouseInGridOutOfBox = scrollRectPos.Contains(Event.current.mousePosition);

			_gridScrollPosition = UnityEngine.GUI.BeginScrollView (
				scrollRectPos,
				_gridScrollPosition,
				scrollableArea
				);

			RenderTimelineController(gridMaxWidth, _gridMaxHeight);
			bool isMouseInBox = DoRenderGrid();
			isMouseInGridOutOfBox = isMouseInGridOutOfBox && !isMouseInBox;
			if(   isMouseInGridOutOfBox
			   && (Event.current.type == EventType.mouseDown || Event.current.type == EventType.mouseDrag))
			{
				// Disable Selected Item
				if(SelectedStep != null)
				{
					OnAnimationItemSelected(null);
				}
				
				// Update Timeline
				_timelinePosition = Event.current.mousePosition;
				if(GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftShift))
				{
					Snapper.ResultData resultData = new Snapper.ResultData();
					if( Snapper.Snap(ref resultData, ConvertDictionaryToList(CacheWindows), _timelinePosition, 26f) )
					{
						_timelinePosition = resultData.Pos;
					}
				}
				_timelinePosition.y = _gridScrollPosition.y;
				
				PlayAtCurrentTimeline();
			}

			UnityEngine.GUI.EndScrollView ();
		}

		List<Rect> ConvertDictionaryToList(Dictionary<Step, AnimationStepBox> cacheWindows)
		{
			List<Rect> rects = new List<Rect>();
			foreach (var pair in cacheWindows) 
			{
				rects.Add(pair.Value.Rect);
			}
			return rects;
		}

		public void PlayAtCurrentTimeline()
		{
			_currentGlobalTime = 0f;
			int lineIdx = 0;
			_gridProperties.GetGlobalTimeSlotFromGridPos(ref _currentGlobalTime, ref lineIdx, new Vector2(_timelinePosition.x, 0f));

			_animationTool.AnimationModel.CurrentAnimation.RefreshAndInit();
			_animationTool.AnimationModel.CurrentAnimation.PlayAt(_currentGlobalTime);
		}

		void RenderTimelineController(float gridMaxWidth, float gridMaxHeight)
		{
			Rect previewInteractuableWindow = new Rect(_boxesOffsetPosition.x, _gridScrollPosition.y, _gridProperties.SecondsToPixels(_currentCollection.GetDuration(AnimTimeMode.Global)), _previewInteractuableHeight);

			// Render and Control Preview Timeline
			Color prevBgColor = UnityEngine.GUI.backgroundColor;
			UnityEngine.GUI.backgroundColor = Color.gray;
			UnityEngine.GUI.Box(previewInteractuableWindow, "");
			UnityEngine.GUI.backgroundColor = prevBgColor;

			// Red Line
			DrawCurrentTimeline(_timelinePosition, _timelinePosition + new Vector2(0f, gridMaxHeight));

			int deltaSeconds = Mathf.CeilToInt(_gridProperties.GridGlobalEndTime - _gridProperties.GridGlobalStartTime);

			float minDistanceToDraw = 50f;
			Vector2 prevPosDrawn = Vector2.zero;
			for (int second = 0; second <= deltaSeconds; ++second) 
			{
				float deltaSeconsFloat = (float) second;
				if(deltaSeconsFloat > (_gridProperties.GridGlobalEndTime-_gridProperties.GridGlobalStartTime))
				{
					deltaSeconsFloat = _gridProperties.GridGlobalEndTime - _gridProperties.GridGlobalStartTime;
				}

				Vector2 posTop = new Vector2(_boxesOffsetPosition.x + _gridStartPosition.x, _gridScrollPosition.y) + new Vector2(_gridProperties.SecondsToPixels(deltaSeconsFloat), 0f);
				Vector2 posDown = posTop + new Vector2(0f, 5f);

				// Try to skip number if too close
				if(  
				   	  second > 0 && second < deltaSeconds
				   && Mathf.Abs(prevPosDrawn.x - posTop.x) < minDistanceToDraw)
				{
					continue;
				}
				else
				{
					prevPosDrawn = posTop;
				}

				DrawGrayLine(posTop, posDown);

				float currentTime = _gridProperties.GridGlobalStartTime + deltaSeconsFloat;
				string currentTimeString = currentTime.ToString("0.0");
				Vector2 numberSize = UnityEngine.GUI.skin.GetStyle("Label").CalcSize(new GUIContent(currentTimeString));

				Vector2 posLabel = posTop + new Vector2(0f, 15f) - numberSize * 0.5f;
				UnityEngine.GUI.Label(new Rect(posLabel, new Vector2(100f, 100f)), currentTimeString);
			}
		}

		void DrawGrayLine(Vector2 startPos, Vector2 endPos)
		{
			Color prevColor = Handles.color;
			Handles.color = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.25f);
			Handles.DrawLine(endPos + new Vector2(0f, 25f), endPos + new Vector2(0f, 1600f));

			Handles.color = prevColor;

			Handles.DrawLine(startPos, endPos);
		}

		void DrawCurrentTimeline(Vector2 startPos, Vector2 endPos)
		{
			Color prevColor = Handles.color;
			Handles.color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.5f);
			Handles.DrawLine(startPos, endPos);
			Handles.color = prevColor;
		}

		void DrawSimpleLine(Vector2 startPos, Vector2 endPos, Color color)
		{
			Color prevColor = Handles.color;
			Handles.color = color;
			Handles.DrawLine(startPos, endPos);
			Handles.color = prevColor;
	    }

		bool DoRenderGrid()
		{
			// Update box positions
			_animationBoxProcessors.UpdateState(this);

			// Update Time by its position and size
			for (int i = 0; i < _currentCollection.AnimItems.Count; ++i) 
			{
				Step animationItem = _currentCollection.AnimItems[i];

				AnimationStepBox animationItemBox;
				if(!_cacheWindows.TryGetValue(animationItem, out animationItemBox))
				{
					// Create the box by the animation item start and end time info
					Vector2 animItemGridPosStart = _gridProperties.GetGridPosFromNormalizedTimeSlot(animationItem.GetStartTime(AnimTimeMode.Local), animationItem.Slot);
					Vector2 animItemGridPosEnd = _gridProperties.GetGridPosFromNormalizedTimeSlot(animationItem.GetEndTime(AnimTimeMode.Local), animationItem.Slot) + new Vector2(0, _gridProperties.PixelsPerSlot * 0.90f);

					animationItemBox = new AnimationStepBox(){ Rect = new Rect(animItemGridPosStart, (animItemGridPosEnd-animItemGridPosStart)), AnimationItem = animationItem };

					_cacheWindows.Add(animationItem, animationItemBox);
				}
				else
				{
					// Save current time by its position
					float normalizedStartTime = 0f;
					int slot = 0;
					_gridProperties.GetNormalizedTimeSlotFromGridPos(ref normalizedStartTime, ref slot, animationItemBox.Rect.position);
					animationItem.SetStartTime(normalizedStartTime, AnimTimeMode.Local);

					float normalizedEndTime = 0f;
					_gridProperties.GetNormalizedTimeSlotFromGridPos(ref normalizedEndTime, ref slot, animationItemBox.Rect.position + new Vector2(animationItemBox.Rect.width, 0f));
					animationItem.SetEndTime(normalizedEndTime, AnimTimeMode.Local);

					if(animationItem is TriggerEffect)
					{
						Vector2 size = animationItemBox.Rect.size;

						Vector2 animItemGridPosStart = _gridProperties.GetGridPosFromTimeSlot(0f, 0);
						Vector2 animItemGridPosEnd = _gridProperties.GetGridPosFromTimeSlot(((TriggerEffect)animationItem).GetFixedDuration() , 0);
						size.x = (animItemGridPosEnd-animItemGridPosStart).x;

						animationItemBox.Rect.size = size;
					}

					animationItem.SetSlot(slot);
				}
			}

			// Render Boxes
			bool isMouseOnBox = false;
			for (int id = 0; id < _currentCollection.AnimItems.Count; ++id) 
			{
				Step animationItem = _currentCollection.AnimItems[id];
				AnimationStepBox animationItemBox = _cacheWindows[animationItem];

				isMouseOnBox |= DoDrawGridBox(animationItemBox, animationItem);
			}

			return isMouseOnBox;
		}

		bool DoDrawGridBox(AnimationStepBox animationItemBox, Step animationItem)
		{
			GUIStyle style = AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft);
			Vector2 headerSize = style.CalcSize(new GUIContent(animationItem.StepName));

			Rect boxWindowContainer = new Rect(animationItemBox.Rect.position, new Vector2(Mathf.Max(animationItemBox.Rect.size.x, _gridProperties.MaxBoxRectPixels), animationItemBox.Rect.size.y));
			Rect visibleBoxWindow = new Rect(new Vector2(0f, 0f), animationItemBox.Rect.size);
			animationItemBox.InteractuableRect = animationItemBox.Rect;
			if(animationItem is TriggerEffect)
			{
//				visibleBoxWindow = new Rect(new Vector2(0f, 0f), new Vector2(_gridProperties.PixelsPerSlot, animationItemBox.Rect.size.y));
//
//				// Fix animItemBox
//				animationItemBox.InteractuableRect = new Rect(animationItemBox.Rect.position, new Vector2(_gridProperties.PixelsPerSlot, animationItemBox.Rect.size.y));
//				animationItemBox.WinMover.GrabSize = new Vector2(_gridProperties.PixelsPerSlot, 0f);
			}

			float isMouseOver = animationItemBox.InteractuableRect.Contains(Event.current.mousePosition) ? 1f : 0f;

			GUILayout.BeginArea(boxWindowContainer);

			// Save Prev Colors
			Color prevBgColor = UnityEngine.GUI.backgroundColor;
			Color prevColor = UnityEngine.GUI.color;

			// Main Box
			Color boxImgColor = animationItemBox.AnimationItem.EditorColor; boxImgColor = new Color(boxImgColor.r, boxImgColor.g, boxImgColor.b, 0.5f);
			UnityEngine.GUI.color = boxImgColor;
			UnityEngine.GUI.Box(visibleBoxWindow, "");

			// Figure
			Vector3 imgPivot = new Vector3(visibleBoxWindow.size.x * 0.001f, visibleBoxWindow.y * 0.001f, 0f);
			Vector3 imgSize = new Vector3(visibleBoxWindow.size.x * 0.999f, visibleBoxWindow.size.y *0.999f, 0f);
			imgSize.x = Mathf.Min(imgSize.x, imgSize.y); imgSize.y = imgSize.x;
			DrawGridBoxFigure(imgPivot, imgSize, animationItem);

			// Title
			UnityEngine.GUI.color = isMouseOver>0f ? Color.white : prevColor;

			GUIStyle titleStyle = AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft);
			Vector2 titleSize = titleStyle.CalcSize( new GUIContent( animationItem.StepName ));
			float titlePadding = Mathf.Max(Mathf.Min(imgSize.x + 2f, animationItemBox.InteractuableRect.size.x -titleSize.x), 0f);
			Rect titleWindow = new Rect(new Vector2(titlePadding, 0f), new Vector2(Mathf.Max(animationItemBox.InteractuableRect.size.x - titlePadding, titleSize.x * isMouseOver), headerSize.y));

			UnityEngine.GUI.Label(titleWindow, animationItem.StepName, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft));

			if(StepsSelection.IsSelected(animationItem))
			{
				DrawFrame(visibleBoxWindow, Color.white, SelectedStep == animationItem ? 2f : 0f);
			}
			else
			{
				DrawFrame(visibleBoxWindow, new Color(0f, 0f, 0f, 0.5f), 0f);
			}

			// Load Prev Colors
			UnityEngine.GUI.backgroundColor = prevBgColor;
			UnityEngine.GUI.color = prevColor;
			
			GUILayout.EndArea();

			return isMouseOver > 0.5f;
		}

		void DrawFrame(Rect window, Color color, float sinFactor)
		{
			DoDrawFrame(window, color, 1f);
			DoDrawFrame(window, color, 2f);
		}

		void DoDrawFrame(Rect window, Color color, float minOffset)
		{
			float offset = minOffset;
			Vector2 pivot = window.position + new Vector2(offset, offset);
			Vector2 size = window.size + new Vector2(-2f*offset +0.5f, -2f*offset -0.5f);

			Color prevColor = Handles.color;
			Handles.color = color;

			Vector3[] points = new Vector3[]
			{
				pivot,
				pivot + new Vector2(size.x, 0f),
				pivot + new Vector2(size.x, size.y),
				pivot + new Vector2(0f, size.y),
				pivot,
			};

			Handles.DrawAAPolyLine(points);

			Handles.color = prevColor;
		}

		void DrawGridBoxFigure(Vector3 pivot, Vector3 size, Step animationItem)
		{
			if(animationItem is TriggerEffect)
			{
				Texture texture = Resources.Load<Texture>("InstantAction");
				UnityEngine.GUI.DrawTexture(new Rect(pivot, size), texture);
			}
			else if(animationItem is BlendEffect)
			{
				Texture texture = Resources.Load<Texture>("BlendAction");
				UnityEngine.GUI.DrawTexture(new Rect(pivot, size), texture);
			}
			else if(animationItem is EffectsGroup)
			{
				Texture texture = Resources.Load<Texture>("ActionsCollection");
				UnityEngine.GUI.DrawTexture(new Rect(pivot, size), texture);
			}
			else if(animationItem is Group)
			{
				Texture texture = Resources.Load<Texture>("Collection");
				UnityEngine.GUI.DrawTexture(new Rect(pivot, size), texture);
			}
		}

		public void AlignAnimationItemBoxPosition(ref AnimationStepBox box, Step item)
		{
			Vector2 startPos = box.Rect.position;

			int slot = 0; float seconds = 0f;
			_gridProperties.GetNormalizedTimeSlotFromGridPos(ref seconds, ref slot, startPos);
			slot = Mathf.Max(slot, 0);
			seconds = Mathf.Max(seconds, 0);

			startPos = _gridProperties.GetGridPosFromNormalizedTimeSlot(seconds, slot);
			float boxWidth = _gridProperties.SecondsToPixels(item.GetEndTime(AnimTimeMode.Global) - item.GetStartTime(AnimTimeMode.Global));

			Vector2 gridMinPos = _gridProperties.GetGridPosFromNormalizedTimeSlot(0f, 0);
			Vector2 gridMaxPos = _gridProperties.GetGridPosFromNormalizedTimeSlot(1f, 0) - new Vector2(boxWidth, 0f);
			startPos.x =  Mathf.Max(Mathf.Min(startPos.x, gridMaxPos.x), gridMinPos.x);

			box.Rect.position = startPos;
		}

		void OnAdvancedAnimationItemAdvancedEdition(Step item)
		{
			if(item is Group)
			{
				ResetState();

				_currentCollection = (Group)item;
				TriggerOnAnimationItemStateChange();
			}
		}

		public void OnAnimationItemSelected(Step animItem)
		{
			_stepsSelection.Set(animItem);
			TriggerOnAnimationItemStateChange();
		}

		public void OnAnimationItemAppend(Step animItem)
		{
			_stepsSelection.Add(animItem);
			TriggerOnAnimationItemStateChange();
		}

		void CreateActionCollection()
		{
			Vector2 position = CalculateSpawnPosition();

			Step newAnimItem = _currentCollection.AddAnimationItem(typeof(EffectsGroup), StepsManager.GetStepName(typeof(EffectsGroup)));
			SetupAnimationItemFromPosition(newAnimItem, position);

			// Auto Add the current selected targets
			for (int i = 0; Selection.gameObjects != null && i < Selection.gameObjects.Length; ++i) 
			{
				GameObject go = Selection.gameObjects[i];
				((EffectsGroup)newAnimItem).AddTarget(go.transform);
			}

			// Color for the editor
			InitializeColor(newAnimItem);

			OnAnimationItemSelected(newAnimItem);
		}

		void CreateInstantAction()
		{
			Vector2 position = CalculateSpawnPosition();
			
			CreateEffectPopup createNameWindow = (CreateEffectPopup) EditorWindow.GetWindow(typeof(CreateEffectPopup));
			createNameWindow.SetTitle("Select The Trigger");
			createNameWindow.SetAnimItemType(StepType.TriggerEffect);
			createNameWindow.SetCallbacks(()=> {
				Step newAnimItem = _currentCollection.AddAnimationItem(createNameWindow.Value, StepsManager.GetStepName(createNameWindow.Value));
				
				SetupAnimationItemFromPosition(newAnimItem, position);
				
				// Duration to zero as it is a Trigger Action
				newAnimItem.SetDuration(0.2f, AnimTimeMode.Global);
				
				// Set Target if possible
				if(Selection.gameObjects != null && Selection.gameObjects.Length > 0)
				{
					((Effect) newAnimItem).Target = Selection.gameObjects[0].transform;
				}
				
				// Create Default Action values
				((Effect) newAnimItem).SetOrCreateDefaultValues();

				// Color for the editor
				InitializeColor(newAnimItem);
				
				OnAnimationItemSelected(newAnimItem);
			});
		}

		void InitializeColor(Step animItem)
		{
			Vector3 random = Random.onUnitSphere;
			animItem.EditorColor = new Color(random.x, random.y, random.z, 1f);
		}

		public bool GroupSelection()
		{
			if(_stepsSelection.Steps.Count > 0)
			{
				Step collection = _currentCollection.AddAnimationItem(typeof(Group), StepsManager.GetStepName(typeof(Group)));
				((Group)collection).AddAndCopyAnimationItems(_stepsSelection.Steps, true);

				// Remove items
				for (int i = 0; i < _stepsSelection.Steps.Count; ++i) 
				{
					RemoveAnimationItem(_stepsSelection.Steps[i]);
				}

				// Select collection
				OnAnimationItemSelected(collection);

				return true;
			}

			return false;
		}

		public bool UngroupSelection()
		{
			List<Step> animItems = new List<Step>(((Group)SelectedStep).AnimItems);
			if(
				   SelectedStep.GetType() == typeof(Group)
				&& animItems.Count > 0)
			{
				((Group)CurrentCollection).AddAndCopyAnimationItems(animItems, false);
				RemoveAnimationItem(SelectedStep);
				
				return true;
			}

			return false;
		}

		Vector2 CalculateSpawnPosition()
		{
			int slot = _currentCollection.GetFirstFreeSlot(0, 999);

			float time = 0f;
			for (int i = 0; i < _currentCollection.AnimItems.Count; ++i) 
			{
				time = Mathf.Max(time, _currentCollection.AnimItems[i].GetEndTime(AnimTimeMode.Global));
			}

			// Add some epsilon time to avoid overlapping
			if(_currentCollection.AnimItems.Count > 0)
			{
				time += kEpsilon;
			}

			float collectionEndTime = _currentCollection.GetEndTime(AnimTimeMode.Global);
			float collectionStart = _currentCollection.GetStartTime(AnimTimeMode.Global);
			time = Mathf.Max(Mathf.Min(time, collectionEndTime - _defaultDuration), collectionStart);

			Vector2 position = _gridProperties.GetGridPosFromTimeSlot(time, slot);
			return position;
		}

		void SetupAnimationItemFromPosition(Step animItem, Vector2 position)
		{
			float localStartTime = 1f;	int localTimeIdx = 0;
			_gridProperties.GetNormalizedTimeSlotFromGridPos(ref localStartTime, ref localTimeIdx, position);

			animItem.SetStartTime(localStartTime, AnimTimeMode.Local);
			animItem.SetEndTime(localStartTime + _gridProperties.ConvertGlobalToNormalizedTime(_defaultDuration), AnimTimeMode.Local);
			animItem.SetSlot(localTimeIdx);
		}

		public void SetTimeAt(float iGlobalTime)
		{
			_currentGlobalTime = iGlobalTime;
			_timelinePosition = _gridProperties.GetGridPosFromTimeSlot(iGlobalTime, 0);
			_animationTool.AnimationModel.CurrentAnimation.PlayAt(iGlobalTime);
			_animationTool.AnimationModel.RefreshScreen();
		}

		public void SaveValuesAt(float localTimeNormalized)
		{
			for (int i = 0; i < StepsSelection.Count; ++i) 
			{
				Step step = StepsSelection.Steps[i];
				step.SaveValuesAt(localTimeNormalized);
			}

			_timelinePosition = _gridProperties.GetGridPosFromTimeSlot(_currentGlobalTime, 0);
			_animationTool.AnimationModel.CurrentAnimation.PlayAt(_currentGlobalTime);
			_animationTool.AnimationModel.RefreshScreen();
		}

		public void SaveValues()
		{
			for (int i = 0; i < StepsSelection.Count; ++i) 
			{
				Step step = StepsSelection.Steps[i];
				step.SaveValues();
			}
			
			_timelinePosition = _gridProperties.GetGridPosFromTimeSlot(_currentGlobalTime, 0);
//			_animationToolWindow.AnimationModel.CurrentAnimation.PlayAt(_currentGlobalTime);
			_animationTool.AnimationModel.RefreshScreen();
		}

		public void ScaleGrid(float scale)
		{
			float scaleY = Mathf.Lerp(1f, scale, 0.1f);

			_gridProperties.PixelsPerSecond *= scale;
			_gridProperties.PixelsPerSlot *= scaleY;

			foreach (var item in _cacheWindows) 
			{
				item.Value.Rect.position = new Vector2((item.Value.Rect.position.x-_gridProperties.PivotGridPosition.x) * scale + _gridProperties.PivotGridPosition.x, item.Value.Rect.position.y * scaleY);
				item.Value.Rect.size = new Vector2(item.Value.Rect.size.x * scale, item.Value.Rect.size.y * scaleY);
			}
		}

		public void ScaleBoxes(float scale)
		{
			foreach (var item in _cacheWindows) 
			{
				item.Value.Rect.position = new Vector2((item.Value.Rect.position.x-_gridProperties.PivotGridPosition.x)*scale+_gridProperties.PivotGridPosition.x, item.Value.Rect.position.y);
				item.Value.Rect.size = new Vector2(item.Value.Rect.size.x*scale, item.Value.Rect.size.y);
			}
		}

		public void RemoveSelectedAnimationItems()
		{
			if(SelectedStep == null)
			{
				return;
			}

			string title = string.Format("Really want to Remove {0} ?", SelectedStep.StepName);
			if(StepsSelection.Count > 1)
			{
				title = string.Format("Really want to Remove {0} transitions ?", StepsSelection.Count);
			}

			ConfirmationPopup confirmationPopup = (ConfirmationPopup) EditorWindow.GetWindow(typeof(ConfirmationPopup));
			confirmationPopup.SetTitle(title);
			confirmationPopup.SetCallbacks(()=> {
				DoRemoveSelecterdAnimationItems();
			});
		}

		void DoRemoveSelecterdAnimationItems()
		{
			for (int i = 0; i < StepsSelection.Steps.Count; ++i) 
			{
				RemoveAnimationItem(_stepsSelection.Steps[i]);
			}
		}

		public void RemoveAnimationItem(Step animItem)
		{
			if(animItem == null)
			{
				return;
			}

			if(animItem == SelectedStep)
			{
				_stepsSelection.Remove(animItem);
			}

			CurrentCollection.RemoveAnimItem(animItem);

			Group currCollection = CurrentCollection;
			ResetState();
			_currentCollection = currCollection;

			TriggerOnAnimationItemStateChange();
		}

		public void DuplicateSelectedAnimationItems()
		{
			for (int i = 0; i < _stepsSelection.Steps.Count; ++i) 
			{
				DuplicateAnimaitionItem(_stepsSelection.Steps[i]);
			}
		}
		
		void DuplicateAnimaitionItem(Step animItem)
		{
			float animDuration = animItem.GetDuration(AnimTimeMode.Global);
			int freeSlot = CurrentCollection.GetFirstFreeSlot(0, 999);
			
			float time = 0f;
			for (int i = 0; i < CurrentCollection.AnimItems.Count; ++i) 
			{
				time = Mathf.Max(time, CurrentCollection.AnimItems[i].GetEndTime(AnimTimeMode.Global));
			}
			if(CurrentCollection.AnimItems.Count > 0)
			{
				time += kEpsilon;
			}
			time = Mathf.Min(time, CurrentCollection.GetEndTime(AnimTimeMode.Global) - animDuration);
			
			Step copy = CurrentCollection.AddAndCopyAnimationItem(animItem);
			copy.SetStartTime(time, AnimTimeMode.Global);
			copy.SetDuration(animDuration, AnimTimeMode.Global);
			copy.SetSlot(freeSlot);
		}

		public void RefreshSelectedAnimItemsBoxSize()
		{
			for (int i = 0; i < StepsSelection.Steps.Count; ++i) 
			{
				DoRefreshAnimItemBoxSize(StepsSelection.Steps[i]);
			}
		}

		void DoRefreshAnimItemBoxSize(Step animItem)
		{
			AnimationStepBox box;
			if(_cacheWindows.TryGetValue(animItem, out box))
			{
				box.Rect.position = new Vector2(_gridProperties.GetGridPosFromNormalizedTimeSlot(animItem.GetStartTime(AnimTimeMode.Local), 0).x, box.Rect.position.y);
				box.Rect.size = new Vector2(_gridProperties.SecondsToPixels( animItem.GetDuration(AnimTimeMode.Global)), box.Rect.size.y);
			}
		}

		public void SetCurrentCollection(Group collection)
		{
			Init();

			_currentCollection = collection;
			OnAnimationItemSelected(null);
		}
	}
}
