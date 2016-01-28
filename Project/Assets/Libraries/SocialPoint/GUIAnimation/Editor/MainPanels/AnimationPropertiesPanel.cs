using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.GUIAnimation
{
	public class AnimationPropertiesPanel 
	{
		public class MonitorChangedEventData
		{
			public System.Type EffectType;
			public float ChangedTime;
		}

		GUIAnimationTool _animationTool;

		AnimationTimelinePanel _timelinePanel;

		Vector2 _mainScrollPosition = Vector2.zero;
		Vector2 _targetsScrollPosition = Vector2.zero;
		Vector2 _actionsNameScrollPosition = Vector2.zero;

		TimeValueGridPanel _timeValueGridEditor = new TimeValueGridPanel();
		Rect _timeValueWindow = new Rect(new Vector2(-30f, 50f), new Vector2(472f, 454f));
		Rect _timeValueGridWindow = new Rect(new Vector2(38f, 0f), new Vector2(246f, 120f));

		int _viewModeId = 0;
		List<string> _viewMode = new List<string>(){ "START", "END" };

		int _commonPropertyId = 0;

		bool _isInit = false;

		List<BaseGUIActionRenderer> _actionRenderers = new List<BaseGUIActionRenderer>();

		Vector2 _startPosition = Vector2.zero;

		List<StepMonitorData> _firedMonitors = new List<StepMonitorData>();
		List<MonitorChangedEventData> _monitorChangedEventDatas = new List<MonitorChangedEventData>();
		float _changedEventDatasEffectTime = 1.5f;
		TargetMonitorController _monitorController = new TargetMonitorController();

		public void ResetState()
		{
			_timeValueGridEditor.ResetState();

			_viewModeId = 0;
			_isInit = false;

			_monitorController.Backup();
		}

		public void Render(GUIAnimationTool animationTool, Vector2 position)
		{
			_animationTool = animationTool;
			_startPosition = position;

			if(!_isInit)
			{
				Init();
				_isInit = true;
			}

			DoUpdate();
			DoRender();
		}

		void DoUpdate()
		{
			RegisterMonitorChangesTime();
			TryToSaveStateIfMonitorChangesAreApplied();
		}

		void RegisterMonitorChangesTime()
		{
			if(_monitorController.Monitor(_firedMonitors))
			{
				_timelinePanel.SaveValuesAt((_viewModeId == 0 ? 0f : 1f));
				
				for (int i = 0; i < _firedMonitors.Count; ++i) 
				{
					MonitorChangedEventData monitorChangedEventData = _monitorChangedEventDatas.Find( (MonitorChangedEventData adata) => { return adata.EffectType == _firedMonitors[i].StepType; } );
					if(monitorChangedEventData == null)
					{
						monitorChangedEventData = new MonitorChangedEventData();
						monitorChangedEventData.EffectType = _firedMonitors[i].StepType;
						
						_monitorChangedEventDatas.Add(monitorChangedEventData);
					}
					
					monitorChangedEventData.ChangedTime = (float)EditorApplication.timeSinceStartup;
				}
			}
		}

		void TryToSaveStateIfMonitorChangesAreApplied()
		{
			bool haveToClean = _monitorChangedEventDatas.Count > 0;
			for (int i = 0; i < _monitorChangedEventDatas.Count; ++i) 
			{
				if( ((float)EditorApplication.timeSinceStartup - _monitorChangedEventDatas[i].ChangedTime) <= _changedEventDatasEffectTime)
				{
					haveToClean = false;
					break;
				}
			}

			if(haveToClean)
			{
				_monitorChangedEventDatas.Clear();
				
				if(_animationTool)
				{
					_animationTool.SaveState();
				} 
			}
		}

		void DoRender()
		{
			float maxScrollWidth = 1200f;
			float maxScrollHeight = 400f;
			
			Rect scrollRectPos = new Rect(_startPosition, new Vector2(_animationTool.position.width, 400f));	// External Size of the Scroll
			Rect scrollableArea = new Rect (0f, 0f, maxScrollWidth, maxScrollHeight);							// Internal Size of the Scroll
			
			_mainScrollPosition = UnityEngine.GUI.BeginScrollView(
				scrollRectPos,
				_mainScrollPosition,
				scrollableArea
				);

			if(_timelinePanel.SelectedStep is BlendEffect)
			{
				DrawBlendEffectPanel();
			}
			else if(_timelinePanel.SelectedStep is TriggerEffect)
			{
				DrawTriggerEffectPanel();
			}
			else if(_timelinePanel.SelectedStep is EffectsGroup)
			{
				DrawEffectsGroupPanel();
			}
			else if(_timelinePanel.SelectedStep is Group)
			{
				DrawGroupPanel();
			}
			
			UnityEngine.GUI.EndScrollView();
		}

		void Init()
		{
			InitTargetMonitors();
			InitActionRenderers();
		}

		void InitActionRenderers()
		{
			_actionRenderers.Clear();

			_actionRenderers.Add(new GUIUniformRenderer());
			_actionRenderers.Add(new GUITransformRenderer(_timelinePanel.SelectedStep is Effect));
			_actionRenderers.Add(new GUIAnchorRenderer(_timelinePanel.SelectedStep is Effect));
			_actionRenderers.Add(new GUIAnimatorRenderer());

			_actionRenderers.Add(new GUIDefaultActionRenderer());
		}

		void InitTargetMonitors()
		{
			_monitorController.Init(_timelinePanel.SelectedStep);
		}

		void DrawBlendEffectPanel()
		{
			if(RenderTransitionPanel())
			{
				if(RenderCollectionTargetsPanel())
				{
					RenderEffectsPanel();
				}
			}
		}

		void DrawTriggerEffectPanel()
		{
			if(DoRenderInstantMainProps())
			{
				if(RenderCollectionTargetsPanel())
				{
					RenderEffectsPanel();
				}
			}
		}

		void DrawEffectsGroupPanel()
		{
			if(RenderTransitionPanel())
			{
				if(RenderCollectionTargetsPanel())
				{
					RenderEffectsPanel();
				}
			}
		}

		void DrawGroupPanel()
		{
			RenderTransitionPanel();
		}

		void DoRenderInstantActionProps()
		{
			float xOffset = 450f; float yOffset = 0f;
			
			GUILayout.BeginArea(new Rect(new Vector2(xOffset, yOffset), new Vector2(1024f, 512f)));
			
			GUILayout.BeginVertical();
			DoRenderEffectsProperties((Effect)_timelinePanel.SelectedStep);
			GUILayout.EndVertical();
			
			GUILayout.EndArea();
		}

		void DisplayTransformActionUnableToShowMessage()
		{
			GUILayout.TextArea("Transform must be edited updating\neach target directly", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textArea, TextAnchor.MiddleLeft), GUILayout.ExpandWidth(false), GUILayout.MaxWidth(200f));
		}

		bool DoRenderInstantMainProps()
		{
			float xOffset = 6f;
			GUILayout.BeginArea(new Rect(new Vector2(xOffset, 0f), new Vector2(800f, 512f)));
			
			GUILayout.BeginVertical();

			// Title
			EditorGUILayout.LabelField("Transition:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.ExpandWidth(false));
			
			// Name
			GUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Name:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(48f));
			_timelinePanel.SelectedStep.StepName = EditorGUILayout.TextField(_timelinePanel.SelectedStep.StepName, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle3, UnityEngine.GUI.skin.textField, TextAnchor.MiddleLeft), GUILayout.Width(250f));

			EditorGUILayout.LabelField("Enabled:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(54f));
			_timelinePanel.SelectedStep.IsEnabled = EditorGUILayout.Toggle(_timelinePanel.SelectedStep.IsEnabled, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			
			GUILayout.Label("Start at", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label), GUILayout.ExpandWidth(false));
			UnityEngine.GUI.changed = false;
			float globalTime = EditorGUILayout.FloatField(_timelinePanel.StepsSelection.GetStartTime(AnimTimeMode.Global), AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(65f));
			if(UnityEngine.GUI.changed)
			{
				globalTime = Mathf.Clamp(globalTime, _timelinePanel.CurrentCollection.GetStartTime(AnimTimeMode.Global), _timelinePanel.CurrentCollection.GetEndTime(AnimTimeMode.Global));
				_timelinePanel.StepsSelection.SetStartTime(globalTime, AnimTimeMode.Global);
				_timelinePanel.RefreshSelectedAnimItemsBoxSize();
			}

			GUILayout.EndHorizontal();
			
			GUILayout.EndVertical();
			
			GUILayout.EndArea();

			return true;
		}

		public void SetBoxEditor(AnimationTimelinePanel boxEditor)
		{
			_timelinePanel = boxEditor;
			boxEditor.AddOnAnimationItemSelectedDelegate(OnBoxEditorStateChanged);
		}

		void OnBoxEditorStateChanged(Step selectedAnimItem, Group collection)
		{
			_timeValueGridEditor.ResetState();
			ResetState();

			if(selectedAnimItem == null)
			{
				return;
			}

			float halfTime = (selectedAnimItem.GetStartTime(AnimTimeMode.Global) + selectedAnimItem.GetEndTime(AnimTimeMode.Global)) * 0.5f;
			if(_timelinePanel.CurrentTime < halfTime)
			{
				_viewModeId = 0;
				_timelinePanel.SetTimeAt(selectedAnimItem.GetStartTime(AnimTimeMode.Global));
			}
			else
			{
				_viewModeId = 1;
				_timelinePanel.SetTimeAt(selectedAnimItem.GetEndTime(AnimTimeMode.Global));
			}
		}

		bool RenderEffectsPanel()
		{
			float xOffset = 590f;
			GUILayout.BeginArea(new Rect(new Vector2(xOffset, 0f), new Vector2(1024f, 512f)));

			DoRenderEffectsList();

			DoRenderCommonProperties();

			GUILayout.EndArea();


			return true;
		}

		void DrawSeparatorLine(Vector3 start, Vector3 end)
		{
			Color prevColor = Handles.color;
			Handles.color = new Color(Color.gray.r, Color.gray.g, Color.gray.b, 0.2f);

			Handles.DrawLine(start, end);

			Handles.color = prevColor;
		}

		public class GUIMonitor
		{
			bool _isEnabled = false;

			public bool HasChanged()
			{
				bool hasChanged = false;

				if(Event.current.type == EventType.mouseDown)
				{
					_isEnabled = true;
				}
				if(_isEnabled)
				{
					hasChanged = UnityEngine.GUI.changed;
				}

				if(Event.current.type == EventType.mouseUp)
				{
					_isEnabled = false;
				}

				return hasChanged;
			}
		}

		bool DoRenderEasing(IBlendeableEffect blendeableItem, float gridOffsetX, float gridOffsetY)
		{
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal();
			GUILayout.Label("Enable custom easing", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label), GUILayout.Width(128f));

			UnityEngine.GUI.changed = false;
			blendeableItem.UseEaseCustom = EditorGUILayout.Toggle(blendeableItem.UseEaseCustom, GUILayout.Width(25f));
			if(UnityEngine.GUI.changed) 
			{
				TryCopyTemplateEasingValues();
			}

			GUILayout.EndHorizontal();
			if(blendeableItem.UseEaseCustom)
			{
				UnityEngine.GUI.changed = false;

				Rect easingWindow = new Rect(gridOffsetX, gridOffsetY, _timeValueWindow.size.x, _timeValueWindow.size.y);
				blendeableItem.EaseCustom = _timeValueGridEditor.RenderGUI(easingWindow, _timeValueGridWindow, blendeableItem.EaseCustom, ()=>
	            {
					TryCopyTemplateEasingValues();
					GUIAnimationTool.Blackboard.Set(AnimationBlackboardKey.FocusPanelKey, AnimationBlackboardValue.EasingGridPanel);

				});

				if(UnityEngine.GUI.changed) 
				{
					TryCopyTemplateEasingValues();
				}
			}
			else
			{
				UnityEngine.GUI.changed = false;
				blendeableItem.EaseType = (EaseType) EditorGUILayout.EnumPopup(blendeableItem.EaseType, GUILayout.ExpandWidth(false));
				if(UnityEngine.GUI.changed) 
				{
					TryCopyTemplateEasingValues();
				}
			}
			GUILayout.EndVertical();

			return true;
		}

		void TryCopyTemplateEasingValues()
		{
			if(_timelinePanel.SelectedStep is EffectsGroup)
			{
				((EffectsGroup)_timelinePanel.SelectedStep).OverrideEasing();
			}
		}

		bool RenderCollectionTargetsPanel()
		{

			float xOffset = 390f;
			float xPadding = 10f;
			DrawSeparatorLine(new Vector3(xOffset, 0f, 0f), new Vector3(xOffset, 1024f, 0f));

			GUILayout.BeginArea(new Rect(new Vector2(xOffset+xPadding, 0f), new Vector2(300f, 500f)));

			GUILayout.BeginVertical();

			// Title
			GUILayout.Label("Target objects:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.ExpandWidth(false), GUILayout.MaxWidth(100f));

			if(_timelinePanel.SelectedStep is Effect)
			{
				((Effect)_timelinePanel.SelectedStep).Target = (Transform) EditorGUILayout.ObjectField(((Effect)_timelinePanel.SelectedStep).Target, typeof(Transform), true, GUILayout.MaxWidth(150f));
			}
			else
			{
				// Add Objects
				List<GameObject> selection = new List<GameObject>(Selection.gameObjects);
				EffectsGroup actionsCollection = (EffectsGroup) _timelinePanel.SelectedStep;
				if(GUILayout.Button("Add selected objects", GUILayout.Width(150f)))
				{
					for (int i = 0; i < selection.Count; ++i) 
					{
						GameObject selectedTarget = selection[i];
						if(selectedTarget != null)
						{
							actionsCollection.AddTarget(selectedTarget.transform);

							InitTargetMonitors();
						}
					}
				}
				
				// Show Current Objects
				GUILayout.Space(10f);
				float scrollHeight = 512f;
				Rect scrollRectPos = new Rect(Vector2.zero, new Vector2(210f, scrollHeight));	// External Size of the Scroll
				Rect scrollableArea = new Rect (0f, 0f, 0f, 512f);								// Internal Size of the Scroll
				
				_targetsScrollPosition = UnityEngine.GUI.BeginScrollView(
					scrollRectPos,
					_targetsScrollPosition,
					scrollableArea
					);
				
				List<Transform> targetObjects = new List<Transform>();
				targetObjects.AddRange( actionsCollection.Targets);
				
				for (int i = 0; i < targetObjects.Count; ++i) 
				{
					Transform targetObject = targetObjects[i];
					bool isSelected = selection.Contains(targetObject.gameObject);
					
					GUILayout.BeginHorizontal();
					
					Color prevColor = UnityEngine.GUI.color;
					if(isSelected)
					{
						UnityEngine.GUI.backgroundColor = Color.green;
						EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true, GUILayout.MaxWidth(150f));
						UnityEngine.GUI.backgroundColor = prevColor;
					}
					else
					{
						UnityEngine.GUI.backgroundColor = Color.gray;
						EditorGUILayout.ObjectField(targetObject, typeof(GameObject), true, GUILayout.MaxWidth(150f));
						UnityEngine.GUI.backgroundColor = prevColor;
					}
					
					if(GUILayout.Button("X", GUILayout.Width(25f)))
					{
						actionsCollection.RemoveTarget(targetObject);
					}
					
					GUILayout.EndHorizontal();
				}
				
				UnityEngine.GUI.EndScrollView();

			}

			GUILayout.EndVertical();

			GUILayout.EndArea();

			return true;
		}

		void DoRenderEffectsList()
		{
			float xOffset = 0f;
			float yOffset = 0f;
			float xPadding = 10f;

			DrawSeparatorLine(new Vector3(xOffset, 0f, 0f), new Vector3(xOffset, 1024f, 0f));

			GUILayout.BeginArea(new Rect(new Vector2(xOffset+xPadding, yOffset), new Vector2(300f, 512f)));

			GUILayout.BeginVertical();

			GUILayout.Label("Effects:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.ExpandWidth(false), GUILayout.MaxWidth(120f));

			if(_timelinePanel.SelectedStep is Effect)
			{
				Color prevBackgroundColor = UnityEngine.GUI.backgroundColor;

				GUIStyle buttonStyle = AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle3, UnityEngine.GUI.skin.label);
				string animItemName = StepsManager.GetStepName(_timelinePanel.SelectedStep.GetType());

				UnityEngine.GUI.backgroundColor = prevBackgroundColor;
				
				if(GUILayout.Button(animItemName, buttonStyle , GUILayout.Width(100f)))
				{
					// Nothing
				}

				UnityEngine.GUI.Box(new Rect(GUILayoutUtility.GetLastRect().position, new Vector2(150f, buttonStyle.CalcSize( new GUIContent( animItemName )).y)), "");
			}
			else
			{
				Rect scrollRectPos = new Rect(Vector2.zero, new Vector2(150f, 512f));	// External Size of the Scroll
				Rect scrollableArea = new Rect (0f, 0f, 100f, 512f);					// Internal Size of the Scroll
				
				_actionsNameScrollPosition = UnityEngine.GUI.BeginScrollView(
					scrollRectPos,
					_actionsNameScrollPosition,
					scrollableArea
					);

				List<Component> blendActions = ((EffectsGroup) _timelinePanel.SelectedStep ).GetActionTemplates();
				EffectsGroup actionsCollection = (EffectsGroup) _timelinePanel.SelectedStep;

				if(GUILayout.Button("Add effects", GUILayout.Width(150f)))
				{
					CreateBlendingAction();
				}

				for (int i = 0; i < blendActions.Count; ++i) 
				{
					GUILayout.BeginHorizontal();

					System.Type currentEffectType = blendActions[i].GetType();
					GUIStyle buttonStyle = AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle3, UnityEngine.GUI.skin.label);
					string animItemName = StepsManager.GetStepName(currentEffectType);

					Color prevBackgroundColor = GUI.backgroundColor;
					Color prevColor = GUI.color;

					MonitorChangedEventData monitor = _monitorChangedEventDatas.Find( (MonitorChangedEventData mdata) => { return mdata.EffectType == currentEffectType; } );
					if( monitor != null )
					{
						if( EditorApplication.timeSinceStartup - monitor.ChangedTime > _changedEventDatasEffectTime )
						{
							_monitorChangedEventDatas.Remove(monitor);
						}
						else
						{
							Color startColor = Color.white;
							Color endColor = Color.red;
							
							float t = ((float)(EditorApplication.timeSinceStartup - monitor.ChangedTime)) / _changedEventDatasEffectTime;
							Color col = Color.Lerp( startColor, Color.red, Mathf.Pow( 0.5f+0.5f*Mathf.Sin(t * 2f*Mathf.PI + Mathf.PI*3f/2f), 0.1f));
							GUI.backgroundColor = col;
							GUI.color = col;
						}
					}

					GUILayout.Label(animItemName, buttonStyle , GUILayout.Width(100f));
					GUI.backgroundColor = prevBackgroundColor;
					GUI.color = prevColor;
					if(GUILayout.Button("X", GUILayout.Width(25f)))
					{
						actionsCollection.RemoveActionType<Effect>(currentEffectType);
					}
					
					GUILayout.EndHorizontal();
				}

				UnityEngine.GUI.EndScrollView();
			}

			GUILayout.EndVertical();

			GUILayout.EndArea();
		}

		void CreateBlendingAction()
		{
			CreateEffectPopup createNameWindow = (CreateEffectPopup) EditorWindow.GetWindow(typeof(CreateEffectPopup));
			createNameWindow.SetTitle("Select The Effect");
			createNameWindow.SetAnimItemType(StepType.BlendEffect);
			createNameWindow.SetCallbacks(()=> {
				EffectsGroup actionsCollection = (EffectsGroup) _timelinePanel.SelectedStep;
				actionsCollection.AddActionType<Effect>(createNameWindow.Value);
			});
		}

		void DoRenderCommonProperties()
		{
			float xOffset = 200f; float yOffset = 0f; float xPadding = 10f;

			DrawSeparatorLine(new Vector3(xOffset, 0f, 0f), new Vector3(xOffset, 1024f, 0f));
			GUILayout.BeginArea(new Rect(new Vector2(xOffset+xPadding, yOffset), new Vector2(800f, 512f)));
			
			GUILayout.BeginVertical();

			if(_timelinePanel.SelectedStep is Effect)
			{
				GUILayout.Label(StepsManager.GetStepName(_timelinePanel.SelectedStep.GetType()) + " properties:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.ExpandWidth(false), GUILayout.MaxWidth(200f));
				
				DoRenderEffectsProperties((Effect) _timelinePanel.SelectedStep);
			}
			else if(_timelinePanel.SelectedStep is EffectsGroup)
			{
				GUILayout.Label("Shared properties:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.ExpandWidth(false), GUILayout.MaxWidth(200f));
				// Upper selection of the current elements that are enabled and have something in common
				EffectsGroup actionsCollection = (EffectsGroup) _timelinePanel.SelectedStep;
				List<Component> effectsTemplates = actionsCollection.GetActionTemplates();

				List<string> effectNames = new List<string>();
				List<System.Type> effectsTypes = new List<System.Type>();

				for (int i = 0; i < effectsTemplates.Count; ++i) 
				{
					System.Type stepType = effectsTemplates[i].GetType();
					StepData stepData = StepsManager.GetBlendStepData(stepType);
					if(!stepData.ConfigurableInEditor)
					{
						continue;
					}

					effectNames.Add(StepsManager.GetStepName(stepType));
					effectsTypes.Add(stepType);
				}

				// ToolBar with all the shared properties
				effectNames.Insert(0, "Easing");
				float size = 80f * ((float) effectNames.Count);
				_commonPropertyId = GUILayout.Toolbar(_commonPropertyId, effectNames.ToArray(), GUILayout.Width(size), GUILayout.Height(20f));
				_commonPropertyId = System.Math.Min(_commonPropertyId, effectNames.Count -1);

				// Render the shared property
				if(_commonPropertyId >= 0)
				{
					if(effectNames[_commonPropertyId] == "Easing")
					{
						xOffset = 0f;
						GUILayout.BeginArea(new Rect(new Vector2(xOffset, 50f), new Vector2(1024f, 512f)));
						
						bool preEnabled = UnityEngine.GUI.enabled;
						UnityEngine.GUI.enabled = preEnabled && (_timelinePanel.StepsSelection.Count == 1);
						DoRenderEasing(((IBlendeableEffect)_timelinePanel.SelectedStep), 0f, 50f);
						UnityEngine.GUI.enabled = preEnabled;
						
						GUILayout.EndArea();

					}
					else
					{
						Effect selectedStepTemplate = ((EffectsGroup)_timelinePanel.SelectedStep).GetActionTemplate<Effect>(effectsTypes[_commonPropertyId-1]);
						DoRenderEffectsProperties(selectedStepTemplate);
					}
				}
			}
				
			GUILayout.EndVertical();
			GUILayout.EndArea();
		}

		StepData GetBlendEffectByType(System.Type type)
		{
			return StepsManager.BlendStepsData.Find( (StepData adata) => { return adata.StepType == type; } );
		}

		void DoRenderEffectsProperties(Effect effect)
		{
			for (int i = 0; i < _actionRenderers.Count; ++i) 
			{
				if(_actionRenderers[i].CanRender(effect))
				{
					_actionRenderers[i].Render(effect, _timelinePanel.StepsSelection, (Effect iEffect)=>
                   	{
						if(_timelinePanel.SelectedStep is EffectsGroup)
						{
							((EffectsGroup)_timelinePanel.SelectedStep).OverrideAnimItemsByTemplate(effect.GetType());
						}

						_timelinePanel.PlayAtCurrentTimeline();

//						if(_animationTool != null)
//						{
//							_animationTool.SaveState();
//						}
					});

					break;
				}
			}
		}

		void RenderActionDetails()
		{
			SerializedObject serializedStep = new SerializedObject(_timelinePanel.SelectedStep);
			SerializedProperty prop = serializedStep.GetIterator();
			bool moreAvailable = prop.NextVisible(true);
			while (moreAvailable)
			{
				EditorGUILayout.PropertyField(prop, true);
				moreAvailable = prop.NextVisible(false);
			}
			
			if (UnityEngine.GUI.changed)
			{
				serializedStep.ApplyModifiedProperties();
			}
			
			GUILayout.Space(20f);
		}

		bool RenderTransitionPanel()
		{
			float xOffset = 6f; float verticalThemeSpace = 4f;

			GUILayout.BeginArea(new Rect(new Vector2(xOffset, 0f), new Vector2(800f, 512f)));
			
			GUILayout.BeginVertical();

			// Title
			EditorGUILayout.LabelField("Transition:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Title, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.ExpandWidth(false));

			// Name
			GUILayout.BeginHorizontal();

			_timelinePanel.SelectedStep.EditorColor = EditorGUILayout.ColorField(_timelinePanel.SelectedStep.EditorColor, GUILayout.MaxWidth(42f));

			EditorGUILayout.LabelField("Name:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(48f));
			_timelinePanel.SelectedStep.StepName = EditorGUILayout.TextField(_timelinePanel.SelectedStep.StepName, AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle3, UnityEngine.GUI.skin.textField, TextAnchor.MiddleLeft), GUILayout.Width(200f));

			EditorGUILayout.LabelField("Enabled:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label, TextAnchor.MiddleLeft), GUILayout.Width(54f));
			_timelinePanel.SelectedStep.IsEnabled = EditorGUILayout.Toggle(_timelinePanel.SelectedStep.IsEnabled, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(15f));
			GUILayout.EndHorizontal();

			// Start/End/SetKeyframe
			GUILayout.Space(verticalThemeSpace);
			GUILayout.BeginHorizontal();

			int newViewModeId = GUILayout.Toolbar(_viewModeId, _viewMode.ToArray(), GUILayout.Width(136f), GUILayout.Height(20f));
			if(newViewModeId != _viewModeId)
			{
				_viewModeId = newViewModeId;

				if(_viewModeId == 0)
				{
					float globalStartTime = _timelinePanel.SelectedStep.GetStartTime(AnimTimeMode.Global);
					_timelinePanel.SetTimeAt(globalStartTime);
					_monitorController.Backup();
				}
				else
				{
					float globalEndTime = _timelinePanel.SelectedStep.GetEndTime(AnimTimeMode.Global);
					_timelinePanel.SetTimeAt(globalEndTime);
					_monitorController.Backup();
				}
			}

			GUILayout.Space(42f);

			string pasteLabel = string.Format("Copy To {0}", _viewModeId == 0 ? "End" : "Start");
			if(GUILayout.Button(pasteLabel, GUILayout.Width(90f)))
			{
				_timelinePanel.SaveValues();
			}

			// Invert
			GUILayout.Space(6f);
			if(GUILayout.Button("Invert Keys", GUILayout.Width(90f), GUILayout.Height(20f)))
			{
				_timelinePanel.StepsSelection.Invert(false);
				_timelinePanel.PlayAtCurrentTimeline();

				_timeValueGridEditor.ResetState();
			}
			GUILayout.EndHorizontal();

			// Start Time
			GUILayout.Space(verticalThemeSpace);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Start at:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label), GUILayout.Width(46f));
			UnityEngine.GUI.changed = false;
			float globalTime = EditorGUILayout.FloatField(_timelinePanel.StepsSelection.GetStartTime(AnimTimeMode.Global), AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(80f));
			if(UnityEngine.GUI.changed)
			{
				globalTime = Mathf.Clamp(globalTime, _timelinePanel.CurrentCollection.GetStartTime(AnimTimeMode.Global), _timelinePanel.CurrentCollection.GetEndTime(AnimTimeMode.Global));
				_timelinePanel.StepsSelection.SetStartTime(globalTime, AnimTimeMode.Global);
				_timelinePanel.RefreshSelectedAnimItemsBoxSize();
			}
			GUILayout.EndHorizontal();

			// Duration
			GUILayout.Space(verticalThemeSpace);
			GUILayout.BeginHorizontal();
			GUILayout.Label("Duration:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label), GUILayout.Width(46f));
			UnityEngine.GUI.changed = false;
			globalTime = EditorGUILayout.FloatField(_timelinePanel.StepsSelection.GetDuration(AnimTimeMode.Global), AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(80f));
			if(UnityEngine.GUI.changed)
			{
				globalTime = Mathf.Clamp(globalTime, 0f, _timelinePanel.CurrentCollection.GetEndTime(AnimTimeMode.Global) - _timelinePanel.CurrentCollection.GetStartTime(AnimTimeMode.Global));
				_timelinePanel.StepsSelection.SetDuration(globalTime, AnimTimeMode.Global);
				_timelinePanel.RefreshSelectedAnimItemsBoxSize();
			}
			GUILayout.EndHorizontal();

			// End Time
			GUILayout.BeginHorizontal();
			GUILayout.Label("End at:", AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Subtitle2, UnityEngine.GUI.skin.label), GUILayout.Width(46f));
			UnityEngine.GUI.changed = false;
			globalTime = EditorGUILayout.FloatField(_timelinePanel.StepsSelection.GetEndTime(AnimTimeMode.Global), AnimationToolUtility.GetStyle(AnimationToolUtility.TextStyle.Text, UnityEngine.GUI.skin.textField), GUILayout.MaxWidth(80f));
			if(UnityEngine.GUI.changed)
			{
				globalTime = Mathf.Clamp(globalTime, _timelinePanel.CurrentCollection.GetStartTime(AnimTimeMode.Global), _timelinePanel.CurrentCollection.GetEndTime(AnimTimeMode.Global));
				_timelinePanel.StepsSelection.SetEndTime(globalTime, AnimTimeMode.Global);
				_timelinePanel.RefreshSelectedAnimItemsBoxSize();
			}
			GUILayout.EndHorizontal();

			// Easing
			GUILayout.Space(verticalThemeSpace*2f);

			if(_timelinePanel.SelectedStep is IBlendeableEffect && !(_timelinePanel.SelectedStep is EffectsGroup))
			{
				bool preEnabled = UnityEngine.GUI.enabled;
				UnityEngine.GUI.enabled = preEnabled && (_timelinePanel.StepsSelection.Count == 1);
				DoRenderEasing(((IBlendeableEffect)_timelinePanel.SelectedStep), 0f, 160f);
				UnityEngine.GUI.enabled = preEnabled;
			}

			GUILayout.EndVertical();
			
			GUILayout.EndArea();

			bool isOk = true;

			return isOk;
		}
	}
}
