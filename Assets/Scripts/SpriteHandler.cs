using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    public static class SpriteHandler
    {
        static Dictionary<string, Sprite> _spriteDict = new Dictionary<string, Sprite>();
        static Dictionary<string, Texture2D> _texDict = new Dictionary<string, Texture2D>();

        public static Texture2D FetchTex(string path)
        {
            if (_texDict.ContainsKey(path))
            {
                return _texDict[path];
            }
            else
            {
                Texture2D tex = Resources.Load<Texture2D>(path);
                if (tex == null)
                {
                    Debug.LogError("Could not find texture at path " + path);
                    return null;
                }

                _texDict.Add(path, tex);
                return tex;
            }
        }

        public static Sprite FetchSprite(string path)
        {
            if (_spriteDict.ContainsKey(path))
            {
                return _spriteDict[path];
            }
            else
            {
                Sprite sprite = Resources.Load<Sprite>(path);
                if (sprite == null)
                {
                    Debug.LogError("Could not find sprite at path " + path);
                    return null;
                }

                _spriteDict.Add(path, sprite);
                return sprite;
            }
        }

        public static Sprite FetchSprite(string location, string ID)
        {
            string path = location + "/" + ID;
            return FetchSprite(path);
        }

        public static void UnloadHandledTextures()
        {
            _spriteDict.Clear();
            _texDict.Clear();
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
        }
    }
}