using System.Collections;
using SocialPoint.Base;
using Spine.Unity;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SocialPoint.IntroAnimation
{
    public sealed class IntroScreenController : MonoBehaviour
    {
        // This is used just for convenience, just drag&drop the scene object to this field and the name will
        // be serialized. No more typos!

        /// <summary>
        ///     Here's is where we are actually going to serialize the name of the next scene to load
        /// </summary>
        [ReadOnly]
        [SerializeField]
        [Tooltip("We only save the name of the scene to load, beware if you change the scene's name")]
        string NextSceneToLoadName;

        /// <summary>
        /// Delay (secs) before starting to load the next scene after this Component starts. 
        /// </summary>
        [SerializeField]
        float DelaySecsBeforeLoading = 0.3f;

        /// <summary>
        /// Delay (secs) before cleaning up this scene's object after the next scene has been loaded.
        /// The sound's duration is larger than the animation so we need to create a small delay after
        /// the animation has finished and before deleting everything or the sound will be cut.
        /// </summary>
        [SerializeField]
        float DelaySecsDeleteAfterLoading = 1f;

        // Spine animation. Should play when this scene starts
        [SerializeField] SkeletonAnimation IntroAnimation;

        // Animation sound. Should play when this scene starts
        [SerializeField] AudioSource IntroSound;

        #if UNITY_EDITOR
        void OnValidate()
        {
            var scene = IntroScreenData.Instance.NextScene as SceneAsset;
            NextSceneToLoadName = scene == null ? string.Empty : scene.name;
        }
        #endif

        void Start()
        {
            if(IntroAnimation == null || IntroSound == null)
            {
                LoadNextScene();

                return;
            }

            IntroSound.Play();

            StartCoroutine(LoadNextSceneCoroutine(DelaySecsBeforeLoading));
        }

        void LoadNextScene()
        {
            var asyncOp = SceneManager.LoadSceneAsync(NextSceneToLoadName, LoadSceneMode.Additive);

            StartCoroutine(WaitUntilSceneLoadedCorutine(asyncOp));

        }

        /// <summary>
        ///     Clean up all resources for the intro animation
        /// </summary>
        void DeleteSceneElements()
        {
            if(this != null)
            {
                gameObject.Destroy();
            }

            // Animation's atlas and sound are not used anymore in the game, so unload them to save memory
            Resources.UnloadUnusedAssets();
        }


        IEnumerator WaitUntilSceneLoadedCorutine(AsyncOperation asyncOp)
        {
            while(!asyncOp.isDone)
                yield return null;

            StartCoroutine(DeleteSceneElementsCoroutine(DelaySecsDeleteAfterLoading));
        }

        IEnumerator LoadNextSceneCoroutine(float waitMsecs)
        {
            if(waitMsecs > 0)
            {
                yield return new WaitForSeconds(waitMsecs);
            }
            LoadNextScene();
        }

        IEnumerator DeleteSceneElementsCoroutine(float waitMsecs)
        {
            if(waitMsecs > 0)
            {
                yield return new WaitForSeconds(waitMsecs);
            }
            DeleteSceneElements();
        }
    }
}