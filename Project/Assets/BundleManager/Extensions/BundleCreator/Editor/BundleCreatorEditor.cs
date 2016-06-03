using UnityEngine;
using UnityEditor;
using System.Collections;

public class BundleCreatorEditor : EditorWindow
{
    private static BundleCreator creator;

    enum BundleType
    {
        Simple,
        SimpleCombined,
        Scene
    }

    [MenuItem("Assets/Bundle Creator/Create Simple Bundle")]
    static void CreateSimpleBundle()
    {
        CreateBundles(BundleType.Simple, false);
    }

    [MenuItem("Assets/Bundle Creator/Create Simple Combined Bundle")]
    static void CreateSimpleCombinedBundle()
    {
        CreateBundles(BundleType.SimpleCombined, false);
    }

    [MenuItem("Assets/Bundle Creator/Create Scene Bundle")]
    static void CreateSceneBundle()
    {
        CreateBundles(BundleType.Scene, false);
    }

    [MenuItem("Assets/Bundle Creator/Create and Build Simple Bundle")]
    static void CreateBuildSimpleBundle()
    {
        CreateBundles(BundleType.Simple, true);
    }

    [MenuItem("Assets/Bundle Creator/Create and Build Simple Combined Bundle")]
    static void CreateBuildSimpleCombinedBundle()
    {
        CreateBundles(BundleType.SimpleCombined, true);
    }

    static private void CreateBundles(BundleType type, bool build)
    {
        creator = new BundleCreator();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();

        Object[] fixedSelection = FixSelection(Selection.objects);

        string errorLog = "";
        switch(type)
        {
        case BundleType.Scene:
            {
                errorLog = creator.CreateSimpleBundle(fixedSelection, true, build);
                break;
            }
        case BundleType.Simple:
            {
                errorLog = creator.CreateSimpleBundle(fixedSelection, false, build);
                break;
            }
        case BundleType.SimpleCombined:
            {
                errorLog = creator.CreateSimpleCombinedBundle(fixedSelection, build);
                break;
            }
        }

        if(errorLog.Length > 0)
        {
            EditorUtility.DisplayDialog("Bundle Creator", errorLog, "Close");
        }
        else
        {
            EditorUtility.DisplayDialog("Bundle Creator", "SUCCESS! Finished Creating Bundles", "Ok");
        }

        EditorUtility.ClearProgressBar();

        AssetDatabase.Refresh();
        AssetDatabase.SaveAssets();
    }

    static private Object[] FixSelection(Object[] selected)
    {
        Object[] fixedSelection = new Object[selected.Length];
        if(selected[0].GetType() == typeof(GameObject))
        {
            for(int i = 0; i < selected.Length; i++)
            {
                GameObject selectedGO = (GameObject)selected[i];

                while(selectedGO.transform.parent != null)
                {
                    selectedGO = selectedGO.transform.parent.gameObject;
                }
                fixedSelection[i] = (Object)selectedGO;
            }
        }
        fixedSelection = selected;
		
        return fixedSelection;
    }
}
