using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;

public class LoadResourcesController : SingletonMono<LoadResourcesController>
{

    public T Load<T>(string path, string fileName) where T : Object
    {
        string fullPath = Path.Combine(path, fileName);

        return Resources.Load<T>(fullPath);
    }

    public Sprite LoadSpriteFromAtlas(string atlasName, string spriteName)
    {
        try
        {
            var atlas = Load<SpriteAtlas>("Atlas/", atlasName);
            return atlas.GetSprite(spriteName);
        }
        catch
        {
            Debug.LogError($"atlas is not exist: {atlasName}");
            return null;
        }
    }

}


