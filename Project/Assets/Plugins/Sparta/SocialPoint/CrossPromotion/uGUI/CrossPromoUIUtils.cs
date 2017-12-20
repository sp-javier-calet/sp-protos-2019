using UnityEngine;
using UnityEngine.UI;

public static class CrossPromoUIUtils
{
    static Vector2 _defaultSpritePivot = new Vector2(0.5f, 0.5f);

    public static void SetImage(Image target, Texture2D imgTexture)
    {
        if(imgTexture != null)
        {
            var spriteRect = new Rect(0, 0, imgTexture.width, imgTexture.height);
            target.sprite = Sprite.Create(imgTexture, spriteRect, _defaultSpritePivot);
        }
    }
}

