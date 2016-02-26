using UnityEngine;
using System;
using System.Collections.Generic;
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

        IDictionary<UIViewController, UICameraData> _uiCameraByController = new Dictionary<UIViewController, UICameraData>();

        IDictionary<UIViewController, UICameraData> _3dCameraByController = new Dictionary<UIViewController, UICameraData>();

        IDictionary<UIViewController, List<GameObject>> _3dObjectsByController = new Dictionary<UIViewController, List<GameObject>>();

        List<UIViewController> _controllers = new List<UIViewController>();

        IDictionary<int, UICameraData> _camerasByLayer = new Dictionary<int, UICameraData>();

        int _currentOrderInLayer = 0;

        void Awake()
        {
            Assert.AreEqual(_cameras[0].Type, UICameraData.CameraType.GUI2D, "The first camera must be a 2D camera");

            List<UICameraData> cameras = new List<UICameraData>(_cameras);
            cameras.Reverse();

            foreach(UICameraData cameraData in cameras)
            {
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
                foreach(Transform child in uiElement.transform)
                {
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

            foreach(UIViewController controller in _controllers)
            {
                if(_activeCameras.Peek().Type != UICameraData.CameraType.GUI2D)
                {
                    ActivateNextUILayer(UICameraData.CameraType.GUI2D);
                }

                UICameraData previousCameraAssigned = null;

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

                        foreach(GameObject go in _3dObjectsByController[controller])
                        {
                            AssignCameraTo3DContainer(go, _activeCameras.Peek());
                        }
                    }
                }
            }
        }

        void AssignOrderInCameraLayer(GameObject uiElement)
        {
            List<Canvas> uiCanvas = new List<Canvas>();

            GetCanvasFromElement(uiElement, uiCanvas);

            foreach(Canvas canvas in uiCanvas)
            {
                canvas.sortingOrder = _currentOrderInLayer++;
            }
        }

        void AssignCameraToUICanvas(GameObject uiElement, UICameraData camera)
        {
            int layer = LayerMask.NameToLayer(camera.LayerName);

            List<Canvas> uiCanvas = new List<Canvas>();

            GetCanvasFromElement(uiElement, uiCanvas);

            if(uiCanvas.Count == 0)
            {
                Assert.IsTrue(false, "Not Canvas found inside the element");
                return;
            }

            foreach(Canvas canvas in uiCanvas)
            {
                canvas.worldCamera = camera.Camera.GetComponent<Camera>();
                canvas.gameObject.layer = layer;
            }
        }

        void AssignCameraTo3DContainer(GameObject uiElement, UICameraData camera)
        {
            SetLayerRecursively(uiElement, camera.Layer);
        }

        void SetLayerRecursively(GameObject gameObject, LayerMask layer)
        {
            var children = gameObject.GetComponentsInChildren<Transform>();
            Array.ForEach(children, child => child.gameObject.layer = layer);
        }

        public void Add(UIViewController controller)
        {
            _controllers.Add(controller);

            RefreshCameras();
        }

        public void Remove(UIViewController controller)
        {
            _uiCameraByController.Remove(controller);
            _3dCameraByController.Remove(controller);
            _3dObjectsByController.Remove(controller);
            _controllers.Remove(controller);

            RefreshCameras();
        }

        public void Add3DContainer(UIViewController controller, GameObject go)
        {
            List<GameObject> objectsAdded = null;

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
            List<GameObject> objectsAdded = null;

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
    }
}