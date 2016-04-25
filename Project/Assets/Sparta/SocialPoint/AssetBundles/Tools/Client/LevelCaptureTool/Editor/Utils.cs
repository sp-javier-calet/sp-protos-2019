using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace SocialPoint.Editor.LevelCaptureTool
{
    public static class Utils
    {
		public static void CaptureCurrenScene(Camera camera, string outputDir, ref RenderTexture rt, ref Texture2D screenShot, int width, int height)
		{
            var sceneName = Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
			
			if(sceneName.Equals(String.Empty))
			{
				return;
			}
			
			var outPath = Path.Combine(outputDir, sceneName + ".png");
			
			var count = 1;
			while(File.Exists(outPath) || count > 9)
			{
				outPath = Path.Combine(outputDir, sceneName + "_" + count.ToString() + ".png");
				++count;
			}
			
			if(count > 9)
			{
				throw new Exception(String.Format("Too many scenes with the same name ({0}, count: {1} )... is there something wrong?",
				                                  sceneName,
				                                  count.ToString()));
			}
			
			if(rt == null)
			{
				rt = new RenderTexture(width, height, 24);
				rt.hideFlags = HideFlags.HideAndDontSave;
			}
			RenderTexture.active = rt;
			camera.targetTexture = rt;

			WarmUpAllCurrentSceneSubstances();

			camera.Render();

			if(screenShot == null)
			{
				screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
				screenShot.hideFlags = HideFlags.HideAndDontSave;
			}
			screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			
			RenderTexture.active = null;
			camera.targetTexture = null;
			
			byte[] bytes = screenShot.EncodeToPNG();
			System.IO.File.WriteAllBytes(outPath, bytes);
			Debug.Log(string.Format("Took screenshot to: {0}", outPath));
		}

		/// <summary>
		/// This piece of code is critical fo the procedural textures to not appear blue(not yet generated)
		/// </summary>
		static void WarmUpAllCurrentSceneSubstances()
		{
			var builtSubstances = new HashSet<string> ();
			var renderers = UnityEngine.Object.FindObjectsOfType<Renderer>();
			foreach(var renderer in renderers)
			{
				foreach (var obj in EditorUtility.CollectDependencies(new UnityEngine.Object[] {renderer}))
				{
					ProceduralTexture txt = obj as ProceduralTexture;
					if (txt)
					{
						string substancePath = AssetDatabase.GetAssetPath(txt);
						if (!builtSubstances.Contains(substancePath))
						{
							var substanceImporter = SubstanceImporter.GetAtPath(substancePath) as SubstanceImporter;
							foreach(var sbs in substanceImporter.GetMaterials())
                            {
                                sbs.RebuildTexturesImmediately ();
                            }
                            builtSubstances.Add(substancePath);
                        }
                    }
                }
            }
		}

        public static bool IsBundleManagerLoaded()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                if(assembly.GetType("BundleManager") != null)
                {
                    return true;
                }
            }
            return false;
        }

        public static string[] GetSceneAssetPathsFomBundles()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            Type bmType = null;
            Type bdType = null;
            foreach(var assembly in assemblies)
            {
                if(bmType == null)
                {
                    bmType = assembly.GetType("BundleManager");
                }
                if(bdType == null)
                {
                    bdType = assembly.GetType("BundleData");
                }
            }

            if(bmType == null)
            {
                return null;
            }

            List<string> sceneAssetPaths = new List<string>();

            object[] bundles = (object[])(bmType.GetProperty("bundles").GetValue(null, null));
            FieldInfo includsField = bdType.GetField("includs");
            FieldInfo sceneBundleField = bdType.GetField("sceneBundle");

            foreach(var bundle in bundles)
            {
                bool isSceneBundle = (bool)sceneBundleField.GetValue(bundle);
                if(isSceneBundle)
                {
                    sceneAssetPaths.Add(((List<string>)includsField.GetValue(bundle))[0]);
                }
            }

            return sceneAssetPaths.ToArray();
        }
    }
}