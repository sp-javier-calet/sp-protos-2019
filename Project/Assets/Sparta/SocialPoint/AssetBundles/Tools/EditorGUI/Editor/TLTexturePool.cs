using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using SocialPoint.Tool.Shared.TLGUI.Utils;

namespace SocialPoint.Tool.Shared.TLGUI
{
	public enum TLTextureType
	{
		Single,
		SquareHorizontalAtlas,
		SquareVerticalAtlas
	}
	
    /// <summary>
    /// Utility static class for storing Texture2D in memory.
    /// </summary>
    /// TLTexturePool can import single and atlas texture and store them as an array of Texture2D in the convenient
    /// format that TLImage and TLAnimatedImage needs.
	public static class TLTexturePool
	{
        class TLTexture
        {
            public Texture2D[]     tex;
            public string          imagePath;
            public TLTextureType   type;
            public int[]           paramList;

            public TLTexture(string _imagePath, TLTextureType _type, int[] _paramList, Texture2D[] _tex)
            {
                imagePath = _imagePath;
                type = _type;
                paramList = _paramList;
                tex = _tex;
            }
        }

        static readonly Dictionary<string, TLTexture> _pool;
		
		static TLTexturePool()
		{
            _pool = new Dictionary<string, TLTexture> ();
		}
		
        /// <summary>
        /// Loads an image path into the texture pool and returns the corresponding Texture2D array.
        /// </summary>
        /// <returns>The Texture2D array for the splitted image. The array has a single element for a single image format.</returns>
        /// <param name="imagesId">Images identifier in the pool dictionary.</param>
        /// <param name="imagePath">The path relative to the project folder(for use with AssetDatabase) where the image is located.</param>
        /// <param name="type">The TLTextureType. Only Single and SquareHorizontalAtlas are currently supported.</param>
        /// <param name="addParamList">Additional parameter array. If two params are passed, the textures will be scaled([0]width, [1]height).</param>
		static public Texture2D[] LoadImage( string imagesId, string imagePath, TLTextureType type, int[] addParamList )
		{
			// Load Images
			if (_pool.ContainsKey (imagesId)) {
				Debug.LogWarning(string.Format("TLImagePool already contains an entry for the given key. ( key: {0} )", imagesId));
				return null;
			}

            Texture2D[] texArr = null;
			
			switch (type) {
				case TLTextureType.Single:
                    texArr = LoadSingle( imagePath, addParamList, ref texArr );
					break;
				case TLTextureType.SquareHorizontalAtlas:
                    texArr = LoadSquareHorizontalAtlas( imagePath, addParamList, ref texArr );
					break;
				default:
					Debug.LogWarning(string.Format("The loading method for the specified image type is not yet implemented. ( type: {0} )", type));
					return null;
			}
			
			_pool.Add( imagesId, new TLTexture(imagePath, type, addParamList, texArr) );
			return _pool [imagesId].tex;
		}
		
        /// <summary>
        /// Get an already loaded array of textures in the texture pool by its idetifier.
        /// </summary>
        /// <returns>The Texture2D array for the splitted image. The array has a single element for a single image format.</returns>
        /// <param name="imagesId">Images identifier in the pool dictionary.</param>
        static public Texture2D[] GetImages( string imagesId )
		{
			// Load Images
			if (!_pool.ContainsKey (imagesId)) {
				Debug.LogError(string.Format("The requested tetxure array could not be found in the pool. ( key: {0} )", imagesId));
				return null;
			}
			
			return _pool [imagesId].tex;
		}
		
		/// <summary>
		/// Load single image.
		/// Takes no additional parameters.
		/// </summary>
        static private Texture2D[] LoadSingle( string imagePath, int[] addParamList, ref Texture2D[] texArr )
		{
            if (texArr == null)
                texArr = new Texture2D[1];

			texArr [0] = (Texture2D)(AssetDatabase.LoadAssetAtPath (imagePath, typeof(Texture2D)));

            EvalTextureParams(addParamList, ref texArr);

            return texArr;
		}
		
		/// <summary>
		/// Loads the square horizontal atlas.
		/// Takes no additional parameters as as the texture height will be used to divide the frames by width.
		/// </summary>
        static private Texture2D[] LoadSquareHorizontalAtlas( string imagePath, int[] addParamList, ref Texture2D[] texArr )
		{
			TextureImporter ti = TextureImporter.GetAtPath (imagePath) as TextureImporter;
			ti.isReadable = true;
			ti.npotScale = TextureImporterNPOTScale.None;
			AssetDatabase.ImportAsset (imagePath);
			Texture2D atlasTexture = (Texture2D)(AssetDatabase.LoadAssetAtPath (imagePath, typeof(Texture2D)));
			
			int numTextures = atlasTexture.width / atlasTexture.height;
			int textureSize = atlasTexture.height;

            if (texArr == null)
                texArr = new Texture2D[numTextures];
			
			for (int i=0; i < numTextures; ++i) {
				Color[] framePixels = atlasTexture.GetPixels(i*textureSize, 0, textureSize, textureSize);
				
                texArr[i] = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
                texArr[i].hideFlags = HideFlags.HideAndDontSave;
                texArr[i].SetPixels(framePixels);
                texArr[i].Apply(false,false);
			}

            EvalTextureParams(addParamList, ref texArr);

            return texArr;
		}

        static private void EvalTextureParams(int[] addParamList, ref Texture2D[] texArr)
        {
            if (addParamList.Length > 0) {
                for (int i=0; i < texArr.Length; ++i) {
                    int width = addParamList.Length >= 1 ? addParamList[0] : texArr[i].width;
                    int height = addParamList.Length >= 2 ? addParamList[1] : texArr[i].height;
                    TLUtils.TextureScale.Point (texArr[i], width, height);
                }
            }
        }

        /// <summary>
        /// Reimport all the stored Texture2D in the texture pool.
        /// </summary>
        static public void Reimport()
        {
            foreach (TLTexture entry in _pool.Values) {
                switch (entry.type) {
                case TLTextureType.Single:
                    LoadSingle( entry.imagePath, entry.paramList, ref entry.tex );
                    break;
                case TLTextureType.SquareHorizontalAtlas:
                    LoadSquareHorizontalAtlas(entry.imagePath, entry.paramList, ref entry.tex);
                    break;
                default:
                    Debug.LogWarning(string.Format("The loading method for the specified image type is not yet implemented. ( type: {0} )", entry.type));
                    break;
                }
            }
        }
	}
}
