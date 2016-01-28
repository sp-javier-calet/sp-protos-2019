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

            public GameObject Camera { get { return camera; } }

            public string LayerName { get { return layerName; } }

            public CameraType Type { get { return type; } }

            public int ElementsInCamera { get; set; }

            public int Layer { get; set; }
        };

        [SerializeField]
        List<UICameraData> _cameras = new List<UICameraData>();

        Stack<UICameraData> _inactiveCameras = new Stack<UICameraData>();

        Stack<UICameraData> _activeCameras = new Stack<UICameraData>();

        IDictionary<GameObject, UICameraData> _elementsInCameras = new Dictionary<GameObject, UICameraData>();

        IDictionary<int, UICameraData> _camerasByLayer = new Dictionary<int, UICameraData>();

        void Awake()
        {
            List<UICameraData> cameras = new List<UICameraData>(_cameras);
            cameras.Reverse();

            foreach(UICameraData cameraData in cameras)
            {
                InitializeCamera(cameraData);
                _inactiveCameras.Push(cameraData);
            }

            ActivateNextUILayer(UICameraData.CameraType.GUI2D);
        }

        void InitializeCamera(UICameraData cameraData)
        {
            Assert.IsNotNull(cameraData.Camera, "Assign all the cameras in the inspector or reduce the count");
            cameraData.Camera.SetActive(false);
            int layer = LayerMask.NameToLayer(cameraData.LayerName);
            cameraData.Layer = layer;
            cameraData.Camera.GetComponent<Camera>().cullingMask = 1 << layer;

            if(_camerasByLayer.ContainsKey(layer))
            {
                throw new Exception(string.Format("There is more than one camera using the layer '{0}'.", cameraData.LayerName));
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
                throw new Exception("Trying to use more cameras than allowed");
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

        public int AddToCurrentUILayer(GameObject uiElement)
        {
            if(_activeCameras.Peek().Type != UICameraData.CameraType.GUI2D)
            {
                ActivateNextUILayer(UICameraData.CameraType.GUI2D);
            }

            UICameraData currentCamera = _activeCameras.Peek();
            int layer = LayerMask.NameToLayer(currentCamera.LayerName);

            AssignCameraToUICanvas(uiElement, currentCamera);

            return layer;
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

        public void AddToUILayer(GameObject uiElement, int layer)
        {
            UICameraData camera;

            if(_camerasByLayer.TryGetValue(layer, out camera))
            {
                camera.Camera.SetActive(true);
                AssignCameraToUICanvas(uiElement, camera);
            }
            else
            {
                string.Format("There is not any camera assigned to the layer '{0}'", layer);
            }
        }

        public void AddTo3DLayer(GameObject uiElement, int layer)
        {
            UICameraData camera;

            if(_camerasByLayer.TryGetValue(layer, out camera))
            {
                camera.Camera.SetActive(true);
                AssignCameraTo3DContainer(uiElement, camera);
            }
            else
            {
                string.Format("There is not any camera assigned to the layer '{0}'", layer);
            }
        }

        public int AddToCurrent3DLayer(GameObject uiElement)
        {
            if(_activeCameras.Peek().Type != UICameraData.CameraType.GUI3D)
            {
                ActivateNextUILayer(UICameraData.CameraType.GUI3D);
            }

            UICameraData camera = _activeCameras.Peek();
            AssignCameraTo3DContainer(uiElement, camera);

            return camera.Layer;
        }

        void AssignCameraToUICanvas(GameObject uiElement, UICameraData camera)
        {
            int layer = LayerMask.NameToLayer(camera.LayerName);

            List<Canvas> uiCanvas = new List<Canvas>();

            GetCanvasFromElement(uiElement, uiCanvas);

            if(uiCanvas.Count == 0)
            {
                throw new Exception("Not Canvas found inside the element");
            }

            foreach(Canvas canvas in uiCanvas)
            {
                canvas.worldCamera = camera.Camera.GetComponent<Camera>();
                canvas.gameObject.layer = layer;
            }

            AddElementToCameraCount(uiElement, camera);
        }

        void AssignCameraTo3DContainer(GameObject uiElement, UICameraData camera)
        {
            SetLayerRecursively(uiElement, camera.Layer);
            AddElementToCameraCount(uiElement, camera);
        }

        public void RemoveElement(GameObject uiElement)
        {
            RemoveElementFromCameraCount(uiElement);

            DisableUnusedCameras();
        }

        void SetLayerRecursively(GameObject gameObject, LayerMask layer)
        {
            var children = gameObject.GetComponentsInChildren<Transform>();
            Array.ForEach(children, child => child.gameObject.layer = layer);
        }

        void AddElementToCameraCount(GameObject gameObject, UICameraData camera)
        {
            RemoveElementFromCameraCount(gameObject);

            _elementsInCameras[gameObject] = camera;
            camera.ElementsInCamera++;
        }

        void RemoveElementFromCameraCount(GameObject gameObject)
        {
            UICameraData camera;

            if(_elementsInCameras.TryGetValue(gameObject, out camera))
            {
                _elementsInCameras.Remove(gameObject);
                camera.ElementsInCamera--;
            }
        }

        void DisableUnusedCameras()
        {
            if(_activeCameras.Count == 1)
            {
                return;
            }

            if(_activeCameras.Peek().ElementsInCamera == 0)
            {
                UICameraData camera = _activeCameras.Pop();
                _inactiveCameras.Push(camera);
                camera.Camera.SetActive(false);

                DisableUnusedCameras();
            }
        }
    }
}