using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace SocialPoint.GUIControl
{
    public class UILayersController : MonoBehaviour
    {
        [Serializable]
        public class UICameraData
        {
            [Serializable]
            public enum CameraType
            {
                GUI2D,
                GUI3D
            }

            [SerializeField]
            GameObject camera;

            [SerializeField]
            string layerName;

            [SerializeField]
            CameraType type;

            [SerializeField]
            int depth;

            public GameObject Camera { get { return camera; } }

            public string LayerName { get { return layerName; } }

            public CameraType Type { get { return type; } }

            public int Depth { get { return depth; } }

            public int ElementsInCamera { get; set; }

            public int Layer { get; set; }
        };

        [SerializeField]
        List<UICameraData> _cameras = new List<UICameraData>();

        Stack<UICameraData> _inactiveCameras = new Stack<UICameraData>();

        Stack<UICameraData> _activeCameras = new Stack<UICameraData>();

        Dictionary<UIViewController, UICameraData> _uiCameraByController = new Dictionary<UIViewController, UICameraData>();

        readonly Dictionary<UIViewController, UICameraData> _3dCameraByController = new Dictionary<UIViewController, UICameraData>();

        readonly Dictionary<UIViewController, List<GameObject>> _3dObjectsByController = new Dictionary<UIViewController, List<GameObject>>();

        List<UIViewController> _controllers = new List<UIViewController>();

        List<UIViewController> _overlappedControllers = new List<UIViewController>();

        Dictionary<GameObject, List<UIViewController>> _overlappedScenePrefabs = new Dictionary<GameObject, List<UIViewController>>();

        List<UIViewController> _actualSceneOverlappedControllers = new List<UIViewController>();

        GameObject _lastDisplayedScene;

        Dictionary<int, UICameraData> _camerasByLayer = new Dictionary<int, UICameraData>();

        int _currentOrderInLayer;

        void Awake()
        {
            Assert.AreEqual(_cameras[0].Type, UICameraData.CameraType.GUI2D, "The first camera must be a 2D camera");

            var cameras = new List<UICameraData>(_cameras);
            cameras.Reverse();

            for(int i = 0, camerasCount = cameras.Count; i < camerasCount; i++)
            {
                UICameraData cameraData = cameras[i];
                InitializeCamera(cameraData);
                _inactiveCameras.Push(cameraData);
            }

            RefreshCameras();
        }

        void InitializeCamera(UICameraData cameraData)
        {
            Assert.IsNotNull(cameraData.Camera, "Assign all the cameras in the inspector or reduce the count");
            cameraData.Camera.SetActive(false);
            int layer = LayerMask.NameToLayer(cameraData.LayerName);
            cameraData.Layer = layer;

            var cameraComponent = cameraData.Camera.GetComponent<Camera>();
            cameraComponent.cullingMask = 1 << layer;
            cameraComponent.depth = cameraData.Depth;
            cameraComponent.clearFlags = CameraClearFlags.Depth;

            if(cameraData.Type == UICameraData.CameraType.GUI3D)
            {
                cameraData.Camera.AddComponent<UI3DCamera>();
            }

            if(_camerasByLayer.ContainsKey(layer))
            {
                Assert.IsTrue(false, string.Format("There is more than one camera using the layer '{0}'.", cameraData.LayerName));
            }
            else
            {
                _camerasByLayer[layer] = cameraData;
            }
        }

        void ActivateNextUILayer(UICameraData.CameraType type)
        {
            if(_inactiveCameras.Count == 0)
            {
                Assert.IsTrue(false, "Trying to use more cameras than allowed");
                return;
            }

            _activeCameras.Push(_inactiveCameras.Pop());
            UICameraData cameraData = _activeCameras.Peek();

            if(cameraData.Type == type)
            {
                cameraData.Camera.SetActive(true);
            }
            else
            {
                ActivateNextUILayer(type);
            }
        }

        public static List<Canvas> GetCanvasFromElement(GameObject uiElement, List<Canvas> uiCanvas = null)
        {
            if(uiCanvas == null)
            {
                uiCanvas = new List<Canvas>();
            }
            var canvas = uiElement.GetComponent<Canvas>();

            if(canvas == null)
            {
                var itr = uiElement.transform.GetEnumerator();
                while(itr.MoveNext())
                {
                    var child = (Transform)itr.Current;
                    GetCanvasFromElement(child.gameObject, uiCanvas);
                }
            }
            else
            {
                uiCanvas.Add(canvas);
            }
            return uiCanvas;
        }

        void ResetCameras()
        {
            if(_activeCameras.Count > 0)
            {
                _activeCameras.Peek().Camera.SetActive(false);
                _inactiveCameras.Push(_activeCameras.Pop());

                ResetCameras();
            }
            else
            {
                _currentOrderInLayer = 0;
                ActivateNextUILayer(UICameraData.CameraType.GUI2D);
            }
        }

        void RefreshCameras()
        {
            ResetCameras();

            for(int i = 0, _controllersCount = _controllers.Count; i < _controllersCount; i++)
            {
                var controller = _controllers[i];

                if(_overlappedControllers.Contains(controller))
                {
                    continue;
                }
                if(_activeCameras.Peek().Type != UICameraData.CameraType.GUI2D)
                {
                    ActivateNextUILayer(UICameraData.CameraType.GUI2D);
                }
                UICameraData previousCameraAssigned;
                // check if we are changing the camera assigned to this controller
                if(!_uiCameraByController.TryGetValue(controller, out previousCameraAssigned) || previousCameraAssigned != _activeCameras.Peek())
                {
                    _uiCameraByController[controller] = _activeCameras.Peek();
                    AssignCameraToUICanvas(controller.gameObject, _activeCameras.Peek());
                }
                AssignOrderInCameraLayer(controller.gameObject);
                // if this camera is using 3d objects, then we need to activate a 3d camera and start using the next ui camera from now on
                if(_3dObjectsByController.ContainsKey(controller))
                {
                    //activate next 3d camera
                    ActivateNextUILayer(UICameraData.CameraType.GUI3D);
                    if(!_3dCameraByController.TryGetValue(controller, out previousCameraAssigned) || previousCameraAssigned != _activeCameras.Peek())
                    {
                        _3dCameraByController[controller] = _activeCameras.Peek();
                        var list = _3dObjectsByController[controller];
                        for(int j = 0, maxCount = list.Count; j < maxCount; j++)
                        {
                            GameObject go = list[j];
                            AssignCameraTo3DContainer(go, _activeCameras.Peek());
                        }
                    }
                }
            }
        }

        void AssignOrderInCameraLayer(GameObject uiElement)
        {
            var uiCanvas = new List<Canvas>();

            GetCanvasFromElement(uiElement, uiCanvas);

            for(int i = 0, uiCanvasCount = uiCanvas.Count; i < uiCanvasCount; i++)
            {
                Canvas canvas = uiCanvas[i];
                canvas.sortingOrder = _currentOrderInLayer++;
            }
        }

        static void AssignCameraToUICanvas(GameObject uiElement, UICameraData camera)
        {
            int layer = LayerMask.NameToLayer(camera.LayerName);

            var uiCanvas = new List<Canvas>();

            GetCanvasFromElement(uiElement, uiCanvas);

            if(uiCanvas.Count == 0)
            {
                Assert.IsTrue(false, "Not Canvas found inside the element");
                return;
            }

            for(int i = 0, uiCanvasCount = uiCanvas.Count; i < uiCanvasCount; i++)
            {
                Canvas canvas = uiCanvas[i];
                canvas.worldCamera = camera.Camera.GetComponent<Camera>();
                canvas.gameObject.layer = layer;
            }
        }

        static void AssignCameraTo3DContainer(GameObject uiElement, UICameraData camera)
        {
            SetLayerRecursively(uiElement, camera.Layer);
        }

        static void SetLayerRecursively(GameObject gameObject, LayerMask layer)
        {
            var children = gameObject.GetComponentsInChildren<Transform>();
            for(int i = 0; i < children.Length; i++)
            {
                var child = children[i];
                child.gameObject.layer = layer;
            }
        }

        public void Add(UIViewController controller)
        {
            _controllers.Add(controller);

            RefreshCameras();
        }

        public void DisplayScene(GameObject scene = null)
        {
            if(_lastDisplayedScene == scene)
            {
                return;
            }

            OverlapPreviousControllers();

            if(scene == null)
            {
                RemoveOverlap(_actualSceneOverlappedControllers);

                _actualSceneOverlappedControllers.Clear();
            }
            else
            {
                if(_overlappedScenePrefabs.ContainsKey(scene))
                {
                    RemoveOverlappedScene(scene);
                }
            }

            _lastDisplayedScene = scene;

            RefreshCameras();
        }

        public void RemoveScene(GameObject scene)
        {
            RemoveOverlappedScene(scene);
        }

        void RemoveOverlappedScene(GameObject scene)
        {
            List<UIViewController> controllers;

            if(_overlappedScenePrefabs.TryGetValue(scene, out controllers))
            {
                RemoveOverlap(controllers);

                _overlappedScenePrefabs.Remove(scene);
            }
        }

        void RemoveOverlap(List<UIViewController> controllers)
        {
            for(int index = 0; index < controllers.Count; ++index)
            {
                var controller = controllers[index];

                SetCanvasEnabled(controller, true);

                _overlappedControllers.Remove(controller);
            }
        }

        void OverlapPreviousControllers()
        {
            for(int index = 0; index < _controllers.Count; ++index)
            {
                var controller = _controllers[index];

                if(_overlappedControllers.Contains(controller))
                {
                    continue;
                }

                SetCanvasEnabled(controller, false);

                _overlappedControllers.Add(controller);

                if(_lastDisplayedScene == null)
                {
                    _actualSceneOverlappedControllers.Add(controller);
                }
                else
                {
                    List<UIViewController> lastSceneControllers;

                    if(!_overlappedScenePrefabs.TryGetValue(_lastDisplayedScene, out lastSceneControllers))
                    {
                        lastSceneControllers = new List<UIViewController>();
                        _overlappedScenePrefabs[_lastDisplayedScene] = lastSceneControllers;
                    }

                    lastSceneControllers.Add(controller);
                }
            }
        }

        void SetCanvasEnabled(UIViewController controller, bool enabled)
        {
            var canvasList = GetCanvasFromElement(controller.gameObject);

            for(int canvasIndex = 0; canvasIndex < canvasList.Count; ++canvasIndex)
            {
                canvasList[canvasIndex].enabled = enabled;
            }
        }

        public void Remove(UIViewController controller)
        {
            _uiCameraByController.Remove(controller);
            _3dCameraByController.Remove(controller);
            _3dObjectsByController.Remove(controller);
            _controllers.Remove(controller);

            if(_overlappedControllers.Contains(controller))
            {
                bool found = _actualSceneOverlappedControllers.Contains(controller);

                if(found)
                {
                    _actualSceneOverlappedControllers.Remove(controller);
                }
                else
                {
                    var overlappedScenesEnumerator = _overlappedScenePrefabs.GetEnumerator();

                    while(overlappedScenesEnumerator.MoveNext() && !found)
                    {
                        var overlappedControllers = overlappedScenesEnumerator.Current.Value;

                        for(int index = 0; !found && index < overlappedControllers.Count; ++index)
                        {
                            var overlappedController = overlappedControllers[index];

                            if(overlappedController == controller)
                            {
                                overlappedControllers.Remove(overlappedController);
                                found = true;
                            }
                        }
                    }

                    overlappedScenesEnumerator.Dispose();
                }

                _overlappedControllers.Remove(controller);
            }

            RefreshCameras();
        }

        public void Add3DContainer(UIViewController controller, GameObject go)
        {
            List<GameObject> objectsAdded;

            if(_3dObjectsByController.TryGetValue(controller, out objectsAdded))
            {
                objectsAdded.Add(go);
                AssignCameraTo3DContainer(go, _3dCameraByController[controller]);
            }
            else
            {
                objectsAdded = new List<GameObject>();
                objectsAdded.Add(go);

                _3dObjectsByController[controller] = objectsAdded;

                RefreshCameras();
            }
        }

        public void Remove3DContainer(UIViewController controller, GameObject go)
        {
            List<GameObject> objectsAdded;

            if(_3dObjectsByController.TryGetValue(controller, out objectsAdded))
            {
                objectsAdded.Remove(go);

                if(objectsAdded.Count == 0)
                {
                    _3dObjectsByController.Remove(controller);
                    _3dCameraByController.Remove(controller);

                    RefreshCameras();
                }
            }
        }

        public UICameraData GetCameraDataByLayer(int layer)
        {
            for(int index = 0; index < _cameras.Count; ++index)
            {
                var cameraData = _cameras[index];

                if(cameraData.Layer == layer)
                {
                    return cameraData;
                }
            }
            return null;
        }
    }
}
