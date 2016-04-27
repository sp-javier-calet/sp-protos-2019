using SocialPoint.AssetSerializer.Helpers;
using UnityEngine;

/**
 * WARNING! / ACHTUNG! / CUIDADO!
 * 
 *   This class can not be edited. It is just used as a container for serialized information.
 *   Any modification can produce loading crashes.
 */
public class JSONSceneContainer : MonoBehaviour
{
	public TextAsset serializationJSONData;
	public GameObject[] rootGameObjects;
	public int[] rootGameObjectIDs;

    public UnityEngine.Object[] serializedAssets;
    public int[] serializedAssetIDs;

	void Awake()
	{
        BuildUnityObjectAnnotatorSingleton.ReadMapperFromSceneContainer( serializedAssets, serializedAssetIDs );
		ComponentHelper.DeserializeScene( this );
		Destroy( gameObject );
	}
}
