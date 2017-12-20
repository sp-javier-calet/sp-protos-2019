using UnityEngine;
using System;

namespace SocialPoint.Base
{
    /// <summary>
    ///     Usefull UnityObject extensions methods
    /// </summary>
    public static class UnityObjectExtensions
    {

        /// <summary>
        ///     Returns a component of type T in the game object. If the component does not exists, it 
        ///     logs an error and pauses execution
        /// </summary>
        public static T GetComponentBreakIfNull<T>(this GameObject datGameObject) where T : UnityEngine.Component
        {
            return datGameObject.GetComponent<T>().BreakIfNull("This GameObject requires a component of type " + typeof(T).Name);
        }

        /// <summary>
        ///     Instantiates a GameObject in the scene view with a given position and rotation.
        /// </summary>
        /// <param name="datGameObject">
        ///     GameObject reference
        /// </param>
        /// <typeparam name="T">
        ///     Type of the GameObject instance
        /// </typeparam>
        /// <returns>
        ///     An cloned instance of the object or null if the object could not be casted to typeparam T
        /// </returns>
        public static T Instantiate<T>(this T datGameObject, Vector3 position, Quaternion rotation) where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(datGameObject, position, rotation) as T;
        }

        /// <summary>
        ///     Instantiates a GameObject in the scene view with a given name
        /// </summary>
        /// <param name="datGameObject">
        ///     GameObject reference
        /// </param>
        /// <typeparam name="T">
        ///     Type of the GameObject instance
        /// </typeparam>
        /// <returns>
        ///     An cloned instance of the object or null if the object could not be casted to typeparam T
        /// </returns>
        /// <param name = "name">
        ///     Name for the new GameObject.
        /// </param>
        public static T InstantiateWithName<T>(this T datGameObject, string name) where T : UnityEngine.Object
        {
            var newObject = datGameObject.Instantiate();
            newObject.name = name;
            return newObject;
        }
        
        public static T InstantiateWithName<T>(this T datGameObject, string name, Vector3 position, Quaternion rotation) where T : UnityEngine.Object
        {
            var newObject = datGameObject.Instantiate(position, rotation);
            newObject.name = name;
            return newObject;
        }
        
        /// <summary>
        ///     Instantiates the specified datGameObject.
        /// </summary>
        /// <param name="datGameObject">Dat game object.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Instantiate<T>(this T datGameObject) where T : UnityEngine.Object
        {
            return UnityEngine.Object.Instantiate(datGameObject) as T;
        }
        
        /// <summary>
        ///     Makes a copy of an instantiated GameObject removing the '(clone)' string from its name
        /// </summary>
        /// <param name="datGameObject">Dat game object.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Copy<T>(this T datGameObject) where T : UnityEngine.Object
        {
            return datGameObject.InstantiateWithName(datGameObject.name);
        }
        
        /// <summary>
        ///     Sets a GameObject as a child of another GameObject. This does not modifies the transform of the 
        ///     child in any way
        /// </summary>
        /// <returns>The child GameObject reference.</returns>
        /// <param name="parentGameObject">Parent GameObject.</param>
        /// <param name="childGameObject">Child GameObject.</param>
        public static GameObject AddAsChild(this GameObject parentGameObject, GameObject childGameObject)
        {
            childGameObject.transform.parent = parentGameObject.transform;
            return childGameObject;
            
        }
             
        /// <summary>
        ///     Sets a GameObject as a child of another GameObject and translates and rotates the child object so
        ///     its world position/rotation is applied as relative to the parent.
        /// </summary>
        /// <returns>The as child relative.</returns>
        /// <param name="parentGameObject">Parent game object.</param>
        /// <param name="childGameObject">Child game object.</param>
        public static GameObject AddAsChildRelative(this GameObject parentGameObject, GameObject childGameObject)
        {
            parentGameObject.AddAsChild(childGameObject);
            childGameObject.transform.localPosition = childGameObject.transform.position;
            childGameObject.transform.localRotation = childGameObject.transform.rotation;
            return childGameObject;
        }
        /// <summary>
        ///     Search in a parent GameObject for a child GameObject with the given name. If it is not found, it creates
        ///     a new GameObject with that name as a child of the parent
        /// </summary>
        /// <returns>The or create child game object.</returns>
        /// <param name="datGameObject">Parent GameObject.</param>
        /// <param name="name">Name of the child GameObject to search for o create.</param>
        public static GameObject FindOrCreateChildGameObject(this GameObject datGameObject, string name)
        {
            var containerGo = datGameObject.GetChildRecursive(name);
            if(containerGo == null)
            {
                containerGo = new GameObject();
                datGameObject.AddAsChild(containerGo);
                containerGo.name = name;
            }
            
            return containerGo;
        }

        /// <summary>
        ///     Throws an exception if a GameObject is null. Useful to make sure that we got a valid
        ///     reference to a GameObjects when initialicing.
        /// </summary>
        /// <returns>
        ///     Throws an ArgumentNullException if datGameObject is null. Returns the same datGameObject reference otherwise.
        /// <param name="datGameObject">
        ///     GameObject reference to be checked
        /// </param>
        /// <typeparam name="T">
        ///     Type of the GameObject instance
        /// </typeparam>
        public static T BreakIfNull<T>(this T datGameObject, string optionalMessage = null, params object[] formatArgs) where T : class
        {
            if(datGameObject == null)
            {
                Log.e(
                    string.Format("[ERROR] [{0}] object [{1}] is null{2}", 
                        Time.frameCount, 
                        typeof(T),
                        optionalMessage == null ? string.Empty : " --> " + string.Format(optionalMessage, formatArgs)));
                DebugUtils.Break();
            }
            return datGameObject;
        }

        public static T LogIfNull<T>(this T datGameObject, string optionalMessage = null, params object[] formatArgs) where T : class
        {
            if(datGameObject == null)
            {
                Log.w(string.Format(
                    "[ERROR] [{0}] object [{1}] is null{2}",
                    Time.frameCount, 
                    typeof(T),
                    optionalMessage == null ? string.Empty : " --> " + string.Format(optionalMessage, formatArgs))); 
            }
            return datGameObject;
        }
        /// <summary>
        ///     Extension method to destroy a GameObject
        /// </summary>
        public static void Destroy(this UnityEngine.Object datGameObject)
        {   
            DestroyAfterThisSeconds(datGameObject, 0.0F);
        }

        /// <summary>
        ///     Extension method to destroy a GameObject after a given time lapse in seconds
        /// </summary>
        public static void DestroyAfterThisSeconds(this UnityEngine.Object datGameObject, float delaySeconds)
        {
            if(datGameObject == null)
            {
                return;
            }
            GameObject.Destroy(datGameObject, delaySeconds);
        }

        /// <summary>
        ///     It's the only way to be sure 
        ///         -- Lt. Ripley
        /// </summary>
        /// <remarks>
        ///     This uses GameOBject.DestroyInmediate, that should not be used outside editor code. 
        ///     And to enforce that practices, it throws an exception if the user attempt to use outside
        ///     editor code :)
        /// </remarks>
        public static void NukeItFromOrbit(this UnityEngine.Object datGameObject)
        {
            if(Application.isEditor)
            {
                GameObject.DestroyImmediate(datGameObject, false);
            }
            else
            {
                throw new Exception("This method calls GameObject.DestroyInmediate, which should only be used in Editor scripts.");
            }
        }
    }
}
