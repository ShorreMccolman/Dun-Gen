using System.Collections.Generic;
using UnityEngine;

namespace DunGen
{
    ///
    /// This sprite handler class is ripped from my game where its more relevant, not sure if I want to use this for release or not
    /// but I already have it set up so might as well just use it for now
    /// 
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