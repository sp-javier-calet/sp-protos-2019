#if !BEHAVIOR_DESIGNER_STANDALONE && !BEHAVIOR_DESIGNER_EDITOR_STANDALONE
using UnityEngine;
using System.Collections.Generic;

namespace BehaviorDesigner.Runtime.Tasks.Basic.SharedVariables
{
    [TaskCategory("Basic/SharedVariable")]
    [TaskDescription("Sets the SharedGameObjectList values from the GameObjects. Returns Success.")]
    public class SharedGameObjectsToGameObjectList : Action
    {
        [Tooltip("The GameObjects value")]
        public SharedGameObject[] gameObjects;
        [RequiredField]
        [Tooltip("The SharedTransformList to set")]
        public SharedGameObjectList storedGameObjectList;

        public override void OnAwake()
        {
            storedGameObjectList.Value = new List<GameObject>();
        }

        public override TaskStatus OnUpdate()
        {
            if (gameObjects == null || gameObjects.Length == 0) {
                return TaskStatus.Failure;
            }

            storedGameObjectList.Value.Clear();
            for (int i = 0; i < gameObjects.Length; ++i) {
                storedGameObjectList.Value.Add(gameObjects[i].Value);
            }

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            gameObjects = null;
            storedGameObjectList = null;
        }
    }
}
#endif