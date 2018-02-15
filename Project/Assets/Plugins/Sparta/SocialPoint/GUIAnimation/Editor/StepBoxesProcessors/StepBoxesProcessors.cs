using System.Collections.Generic;
using SocialPoint.Base;
using UnityEditor;
using UnityEngine;

namespace SocialPoint.GUIAnimation
{
    // This class will control the boxes position, scale and snapping taking into account the user interaction with the boxes windows
    public sealed class StepBoxesProcessors
    {
        abstract class BaseBoxProcessor
        {
            protected StepBoxesProcessors Host;

            public BaseBoxProcessor(StepBoxesProcessors host)
            {
                Host = host;
            }

            public abstract void ResetState();

            public abstract bool UpdateState();
        }

        sealed class ResizeProcessor : BaseBoxProcessor
        {
            public ResizeProcessor(StepBoxesProcessors host) : base(host)
            {
            }

            public override void ResetState()
            {
                foreach(var pair in Host.BoxContainer.CacheWindows)
                {
                    pair.Value.WinResizer.Stop();
                }
            }

            public override bool UpdateState()
            {
                bool someIsResizing = false;

                foreach(var pair in Host.BoxContainer.CacheWindows)
                {
                    AnimationStepBox animationItemBox = pair.Value;
                    if(animationItemBox.AnimationItem is TriggerEffect)
                    {
                        continue;
                    }

                    animationItemBox.WinResizer.Resize(ref animationItemBox.Rect, Vector2.right, delta => {
                        if(Host.BoxContainer.StepsSelection.IsSelected(animationItemBox.AnimationItem))
                        {
                            Host.BoxContainer.StepsSelection.OnResized(animationItemBox.AnimationItem, delta, Host.BoxContainer.CacheWindows);
                        }
                        else
                        {
                            Host.BoxContainer.OnAnimationItemSelected(animationItemBox.AnimationItem);
                        }
                    });

                    bool isResizing = animationItemBox.WinResizer.IsResizing && animationItemBox.WinResizer.DeltaSize.magnitude > 1e-1f;
                    someIsResizing |= isResizing;
                }

                return someIsResizing;
            }
        }

        sealed class MoveProcessor : BaseBoxProcessor
        {
            public MoveProcessor(StepBoxesProcessors host) : base(host)
            {
            }

            public override void ResetState()
            {
                foreach(var pair in Host.BoxContainer.CacheWindows)
                {
                    pair.Value.WinMover.Stop();
                }
            }

            public override bool UpdateState()
            {
                bool someIsMoving = false;

                foreach(var pair in Host.BoxContainer.CacheWindows)
                {
                    AnimationStepBox animationItemBox = pair.Value;

                    // Move
                    Vector2 prevPosition = animationItemBox.Rect.position;
                    animationItemBox.WinMover.Update(ref animationItemBox.Rect);
                    bool isMoving = (animationItemBox.WinMover.IsMoving && animationItemBox.WinMover.Delta.magnitude > 1e-1f);

                    // Align
                    if(GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftShift)
                       && Host.BoxContainer.StepsSelection.IsFirstSelected(animationItemBox.AnimationItem))
                    {
                        isMoving = AlignBox(animationItemBox);
                    }

                    // Select or Expand movement to the rest of the selection
                    if(isMoving)
                    {
                        if(Host.BoxContainer.StepsSelection.IsSelected(animationItemBox.AnimationItem))
                        {
                            Vector3 delta = animationItemBox.Rect.position - prevPosition;
                            Host.BoxContainer.StepsSelection.OnMoved(animationItemBox.AnimationItem, delta, Host.BoxContainer.CacheWindows);
                        }
                        else
                        {
                            Host.BoxContainer.OnAnimationItemSelected(animationItemBox.AnimationItem);
                        }

                        Host.AddMovedBox(animationItemBox);
                    }

                    someIsMoving |= isMoving;

                    if(isMoving)
                    {
                        break;
                    }
                }

                return someIsMoving;
            }

            bool AlignBox(AnimationStepBox animationItemBox)
            {
                if(DoTryToAlign(animationItemBox, 0f, 0f))
                {
                    return true;
                }
                if(DoTryToAlign(animationItemBox, 0f, 1f))
                {
                    return true;
                }
                return DoTryToAlign(animationItemBox, 1f, 0f) || DoTryToAlign(animationItemBox, 1f, 1f);

            }

            bool DoTryToAlign(AnimationStepBox animationItemBox, float sourceWidthFactor, float otherWidthFactor)
            {
                const float distanceToAlignSQ = 10f;
				
                // Find Closest EndTime to current StartTime
                Vector2 myStartPosition = animationItemBox.Rect.position + animationItemBox.Rect.size * sourceWidthFactor;
                Vector2 closestEndPosition = Vector2.zero;
                var closestEndRect = new Rect();
                float closestDistSQ = 9999f;
                foreach(var pair in Host.BoxContainer.CacheWindows)
                {
                    if(pair.Value == animationItemBox)
                    {
                        continue;
                    }

                    if(animationItemBox.AnimationItem.Slot == pair.Key.Slot
                       && Mathf.Abs(sourceWidthFactor - otherWidthFactor) < 1e-1f)
                    {
                        continue;
                    }

                    Vector2 currOtherPosition = pair.Value.Rect.position + pair.Value.Rect.size * otherWidthFactor;
                    float distSQ = Mathf.Abs(currOtherPosition.x - myStartPosition.x);
                    if(distSQ < closestDistSQ)
                    {
                        closestDistSQ = distSQ;
                        closestEndPosition = currOtherPosition;
                        closestEndRect = pair.Value.Rect;
                    }
                }
                if(closestDistSQ < distanceToAlignSQ)
                {
                    Vector2 delta = closestEndPosition - myStartPosition;
                    animationItemBox.Rect.position += new Vector2(delta.x, 0f);
					
                    RenderAlignLine(animationItemBox.Rect, closestEndRect, otherWidthFactor);
                    return true;
                }

                return false;
            }

            static void RenderAlignLine(Rect source, Rect reference, float referenceWidthFactor)
            {
                const float extraYPixels = 4f;
				
                float alignYStart = Mathf.Max(source.position.y + source.size.y, reference.position.y + reference.size.y) + extraYPixels;
                float alignYEnd = Mathf.Min(source.position.y, reference.position.y) - extraYPixels;
				
                var startPoint = new Vector3(reference.position.x + reference.size.x * referenceWidthFactor, alignYStart, 0f);
                var endPoint = new Vector3(reference.position.x + reference.size.x * referenceWidthFactor, alignYEnd, 0f);
				
                Color prevColor = Handles.color;
                Handles.color = Color.white;
                Handles.DrawLine(startPoint, endPoint);
                Handles.color = prevColor;
            }
        }

        sealed class SelectionProcessor : BaseBoxProcessor
        {
            public SelectionProcessor(StepBoxesProcessors host) : base(host)
            {
            }

            public override void ResetState()
            {
            }

            public override bool UpdateState()
            {
                bool someIsSelected = false;

                if(Event.current.type == EventType.MouseUp)
                {
                    foreach(var pair in Host.BoxContainer.CacheWindows)
                    {
                        if(pair.Value.InteractuableRect.Contains(Event.current.mousePosition))
                        {
                            if(GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftControl)
                               || GUIAnimationTool.KeyController.IsPressed(KeyCode.LeftCommand))
                            {
                                Host.BoxContainer.OnAnimationItemAppend(pair.Key);
                            }
                            else
                            {
                                if(GUIAnimationTool.MouseController.IsDoubleClick()
                                   && Host.BoxContainer.StepsSelection.IsSelected(pair.Key)
                                   && pair.Key is Group)
                                {
                                    Host.BoxContainer.SetCurrentCollection((Group)pair.Key);
                                }
                                else
                                {
                                    Host.BoxContainer.OnAnimationItemSelected(pair.Key);
                                }
                            }
                            someIsSelected = true;
                        }
                    }

                    if(!someIsSelected)
                    {
                        // Disable Selected boxes if none is selected
                        var boxesWindow = new Rect(Host.BoxContainer.BoxesOffsetPosition.x, Host.BoxContainer.BoxesOffsetPosition.y, Host.BoxContainer.GridProps.GetGridPosFromNormalizedTimeSlot(1f, 0).x, Host.BoxContainer.GridMaxHeight);
                        if(
                            Event.current.type == EventType.MouseUp
                            && boxesWindow.Contains(Event.current.mousePosition))
                        {
                            Host.BoxContainer.OnAnimationItemSelected(null);
                        }
                    }
                }

                return false;
            }
        }

        sealed class RealignProcessor : BaseBoxProcessor
        {
            public RealignProcessor(StepBoxesProcessors host) : base(host)
            {
            }

            public override void ResetState()
            {
            }

            public override bool UpdateState()
            {
                foreach(var pair in Host.BoxContainer.CacheWindows)
                {
                    AnimationStepBox animationItemBox = pair.Value;
                    Host.BoxContainer.AlignAnimationItemBoxPosition(ref animationItemBox, pair.Key);
                }

                for(int boxIdx = 0; boxIdx < Host.MovedBoxes.Count; ++boxIdx)
                {
                    AnimationStepBox animationItemBox = Host.MovedBoxes[boxIdx];
                    ReallocateBoxIfOverlapped(animationItemBox);
                }
                Host.ClearMovedBoxes();

                return false;
            }

            void ReallocateBoxIfOverlapped(AnimationStepBox animationItemBox)
            {
                Vector2 myStart = animationItemBox.Rect.position;
                Vector2 myEnd = animationItemBox.Rect.position + animationItemBox.Rect.size;

                foreach(var pair in Host.BoxContainer.CacheWindows)
                {
                    AnimationStepBox otherBox = pair.Value;

                    if(otherBox == animationItemBox)
                    {
                        continue;
                    }

                    // Discart boxes of other slots
                    float normSeconds = 0f;
                    int slot = animationItemBox.AnimationItem.Slot;
                    Host.BoxContainer.GridProps.GetNormalizedTimeSlotFromGridPos(ref normSeconds, ref slot, myStart);

                    normSeconds = 0f;
                    int otherSlot = otherBox.AnimationItem.Slot;
                    Host.BoxContainer.GridProps.GetNormalizedTimeSlotFromGridPos(ref normSeconds, ref slot, myStart);
                    if(otherSlot != slot)
                    {
                        continue;
                    }

                    Vector2 otherStart = otherBox.Rect.position;
                    Vector2 otherEnd = otherBox.Rect.position + otherBox.Rect.size;

                    if((myStart.x >= otherStart.x && (myStart.x) <= otherEnd.x)
                       || ((myEnd.x) >= otherStart.x && myEnd.x <= otherEnd.x))
                    {
                        ReallocateBox(animationItemBox, slot, slot + 100);
                        break;
                    }
                }
            }

            void ReallocateBox(AnimationStepBox animationItemBox, int slotMin, int slotMax)
            {
                Vector2 position = animationItemBox.Rect.position;
                if(FindFreePosition(ref position, slotMin, slotMax, animationItemBox.Rect.size.x, animationItemBox))
                {
                    animationItemBox.Rect.position = position;
                }
                else
                {
                    Log.w("Not found good position");
                }
            }

            bool FindFreePosition(ref Vector2 position, int slotMin, int slotMax, float width, AnimationStepBox discartBox = null)
            {
                for(int slotIdx = slotMin; slotIdx <= slotMax; ++slotIdx)
                {
                    if(FindFreePositionInSlot(ref position, slotIdx, width, discartBox))
                    {
                        return true;
                    }
                }

                return false;
            }

            bool FindFreePositionInSlot(ref Vector2 position, int slot, float width, AnimationStepBox discartBox = null)
            {
                var boxesInSlot = new List<AnimationStepBox>();
                foreach(var pair in Host.BoxContainer.CacheWindows)
                {
                    AnimationStepBox otherBox = pair.Value;
                    if(otherBox == discartBox)
                    {
                        continue;
                    }

                    float normSeconds = 0f;
                    int otherSlot = otherBox.AnimationItem.Slot;
                    Host.BoxContainer.GridProps.GetNormalizedTimeSlotFromGridPos(ref normSeconds, ref otherSlot, otherBox.Rect.position);
                    if(otherSlot == slot)
                    {
                        boxesInSlot.Add(otherBox);
                    }
                }

                // Get Last Position
                Vector2 lastBoxPos = Host.BoxContainer.GridProps.GetGridPosFromNormalizedTimeSlot(0f, slot);
                for(int boxIdx = 0; boxIdx < boxesInSlot.Count; ++boxIdx)
                {
                    AnimationStepBox otherBox = boxesInSlot[boxIdx];
                    Vector2 otherBoxEndPos = otherBox.Rect.position + new Vector2(otherBox.Rect.size.x, 0f);
                    if(otherBoxEndPos.x > lastBoxPos.x)
                    {
                        lastBoxPos = otherBoxEndPos;
                    }
                }

                // Check remaining X Distance is less or equal to width
                Vector2 gridLastPosition = Host.BoxContainer.GridProps.GetGridPosFromNormalizedTimeSlot(1f, slot);
                float remainingX = gridLastPosition.x - lastBoxPos.x;
                if(remainingX >= width)
                {
                    position = lastBoxPos;
                    return true;
                }

                return false;
            }
        }

        // Processors properties
        AnimationTimelinePanel BoxContainer;
        readonly List<BaseBoxProcessor> _processors = new List<BaseBoxProcessor>();
        BaseBoxProcessor EnabledProcessor;
        double _lastBlockingActionTime;
        List<AnimationStepBox> MovedBoxes = new List<AnimationStepBox>();

        void Init()
        {
            EnabledProcessor = null;
            _lastBlockingActionTime = 0;

            MovedBoxes.Clear();

            _processors.Clear();
            _processors.Add(new RealignProcessor(this));
            _processors.Add(new ResizeProcessor(this));
            _processors.Add(new MoveProcessor(this));
            _processors.Add(new SelectionProcessor(this));
        }

        public void ResetState()
        {
            Init();
        }

        public void AddMovedBox(AnimationStepBox box)
        {
            if(!MovedBoxes.Contains(box))
            {
                MovedBoxes.Add(box);
            }
        }

        public void ClearMovedBoxes()
        {
            MovedBoxes.Clear();
        }

        // Processors Methods
        public void UpdateState(AnimationTimelinePanel boxContainer)
        {
            BoxContainer = boxContainer;

            for(int i = 0; i < _processors.Count; ++i)
            {
                if(EditorApplication.timeSinceStartup - _lastBlockingActionTime < 1e-2)
                {
                    continue;
                }

                // Run all or the fixed one
                if(EnabledProcessor == null
                   || EnabledProcessor == _processors[i])
                {
                    bool isUsed = _processors[i].UpdateState();
                    if(isUsed)
                    {
                        EnabledProcessor = _processors[i];
                        break;
                    }
                }
            }

            // Reset all on mouseup
            if(Event.current.type == EventType.MouseUp)
            {
                if(EnabledProcessor != null)
                {
                    _lastBlockingActionTime = EditorApplication.timeSinceStartup;
                    EnabledProcessor = null;
                }

                for(int i = 0; i < _processors.Count; ++i)
                {
                    _processors[i].ResetState();
                }
            }
        }
    }
}
