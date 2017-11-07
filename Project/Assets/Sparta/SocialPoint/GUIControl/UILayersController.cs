using System;
using System.Collections.Generic;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public sealed class UILayersController : MonoBehaviour
    {
        [Serializable]
        public sealed class UICameraData
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

        public static readonly string[] LayerNames = {"UI",
            "GUILevel1",
            "GUILevel2",
            "GUILevel3",
            "GUILevel4",
            "GUILevel5",
            "GUILevel6",
            "GUILevel7",
            "GUILevel8"
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

        bool _isBeingDestroyed;

        void Awake()
        {
            DebugUtils.Assert(_cameras[0].Type == UICameraData.CameraType.GUI2D, "The first camera must be a 2D camera");

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
            DebugUtils.Assert(cameraData.Camera != null, "Assign all the cameras in the inspector or reduce the count");
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
                DebugUtils.Assert(false, string.Format("There is more than one camera using the layer '{0}'.", cameraData.LayerName));
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
                DebugUtils.Assert(false, "Trying to use more cameras than allowed");
                return;
            }

            _activeCameras.Push(_inactiveCameras.Pop());
            UICameraData cameraData = _activeCameras.Peek();

            if(cameraData != null && cameraData.Type == type)
            {
                if(cameraData.Camera != null)
                {
                    cameraData.Camera.SetActive(true);
                }
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

        public static Vector2 GetCanvasScale(Canvas canvas)
        {
            var rect = canvas.GetComponent<RectTransform>();
            if(rect == null)
            {
                return Vector2.one;
            }
            return new Vector2(rect.sizeDelta.x / Screen.width, rect.sizeDelta.y / Screen.height);
        }

        public static Vector2 GetCanvasSize(Canvas canvas)
        {
            var rect = canvas.GetComponent<RectTransform>();
            if(rect == null)
            {
                return new Vector2(Screen.width, Screen.height);
            }
            return rect.sizeDelta;
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
            if(_isBeingDestroyed)
            {
                // Avoid Refreshing cameras and layered objects when application is being closed
                return;
            }

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
                    AssignCameraToUICanvas(controller.gameObject, _activeCameras.Peek(), controller.WorldSpaceFullScreen);
                }
                AssignOrderInCameraLayer(controller.gameObject);
                // if this camera is using 3d objects, then we need to activate a 3d camera and start using the next ui camera from now on
                if(_3dObjectsByController.ContainsKey(controller))
                {
                    //if 3d object canvas is world space 
                    var list = _3dObjectsByController[controller];
                    var canvasList = GetCanvasFromElement(controller.gameObject);

                    if(canvasList.Count > 0 && canvasList[0].renderMode == RenderMode.WorldSpace)
                    {
                        _3dCameraByController[controller] = _uiCameraByController[controller];

                        for(int j = 0, maxCount = list.Count; j < maxCount; j++)
                        {
                            GameObject go = list[j];
                            AssignCameraTo3DContainer(go, _uiCameraByController[controller]);
                        }
                    }
                    else
                    {
                        //activate next 3d camera
                        ActivateNextUILayer(UICameraData.CameraType.GUI3D);
                        if(!_3dCameraByController.TryGetValue(controller, out previousCameraAssigned) || previousCameraAssigned != _activeCameras.Peek())
                        {
                            _3dCameraByController[controller] = _activeCameras.Peek();

                            for(int j = 0, maxCount = list.Count; j < maxCount; j++)
                            {
                                GameObject go = list[j];
                                AssignCameraTo3DContainer(go, _activeCameras.Peek());
                            }
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

        void PlaceElementFullScreen(GameObject uiElement, UICameraData camera)
        {
            var cam = camera.Camera.GetComponent<Camera>();

            uiElement.transform.position = new Vector3(0, 0, camera.Depth);

            float camHeight;
            if(cam.orthographic)
            {
                camHeight = cam.orthographicSize * 2;
            }
            else
            {
                camHeight = Mathf.Tan(Mathf.Deg2Rad * (cam.fieldOfView * 0.5f)) * camera.Depth;
            }

            var rect = uiElement.GetComponent<RectTransform>().rect;
            var currentHeight = uiElement.transform.localScale.y * rect.height;
            var currentWidth = uiElement.transform.localScale.x * rect.width;
            var scaleHeight = camHeight / currentHeight;
            var camWidth = (((float)Screen.width / (float)Screen.height) * camHeight);
            var scaleWidth = camWidth / currentWidth;
            var finalScale = uiElement.transform.localScale;
            finalScale.y *= scaleHeight;
            finalScale.x *= scaleWidth;
            finalScale.z *= scaleWidth;
            uiElement.transform.localScale = finalScale;
        }

        void AssignCameraToUICanvas(GameObject uiElement, UICameraData camera, bool repositionWorldSpaceController)
        {
            int layer = LayerMask.NameToLayer(camera.LayerName);

            var uiCanvas = new List<Canvas>();

            GetCanvasFromElement(uiElement, uiCanvas);

            if(uiCanvas.Count == 0)
            {
                DebugUtils.Assert(false, "Not Canvas found inside the element");
                return;
            }

            bool isInWorldSpace = false;

            for(int i = 0, uiCanvasCount = uiCanvas.Count; i < uiCanvasCount; i++)
            {
                Canvas canvas = uiCanvas[i];
                var cam = camera.Camera.GetComponent<Camera>();
                if(canvas.renderMode == RenderMode.WorldSpace)
                {
                    isInWorldSpace = true;
                    if(repositionWorldSpaceController)
                    {
                        PlaceElementFullScreen(uiElement, camera);
                    }
                }

                canvas.worldCamera = cam;
                canvas.gameObject.layer = layer;
            }

            if(isInWorldSpace)
            {
                ActivateNextUILayer(UICameraData.CameraType.GUI2D);
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
                if(_actualSceneOverlappedControllers.Contains(controller))
                {
                    _actualSceneOverlappedControllers.Remove(controller);
                }
                else
                {
                    var overlappedScenesEnumerator = _overlappedScenePrefabs.GetEnumerator();

                    while(overlappedScenesEnumerator.MoveNext())
                    {
                        var overlappedControllers = overlappedScenesEnumerator.Current.Value;

                        if(overlappedControllers.Contains(controller))
                        {
                            overlappedControllers.Remove(controller);
                            break;
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

        void OnDestroy()
        {
            _isBeingDestroyed = true;
        }
    }
}
