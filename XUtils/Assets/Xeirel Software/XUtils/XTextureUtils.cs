using UnityEngine;

namespace XUtils.TextureUtils
{
    public static class XTextureUtils
    {
        public static Sprite ToSprite(this Texture2D texture, float pixelsPerUnit = 100f)
        {
            if (texture == null) return null;

            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f), pixelsPerUnit);
        }
    }
}
