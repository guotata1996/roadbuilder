using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteVerticeSampler : IEnumerable

{
    SpriteBoundary spriteBoundary;
    List<Vector2> sampledVertice;

    public SpriteVerticeSampler(Sprite source, float spriteRealWidth, float sampleRealResolution)
    {
        spriteBoundary = new SpriteBoundary(source);
        sampledVertice = new List<Vector2>();

        Vector2Int last = new Vector2Int(int.MinValue, int.MinValue);
        float cumulativeDist = 0f;
        foreach (Vector2Int i in spriteBoundary)
        {
            if (last.x != int.MinValue)
            {
                cumulativeDist += (last - i).magnitude / (float)source.texture.width * spriteRealWidth;
                if (cumulativeDist >= sampleRealResolution)
                {
                    cumulativeDist %= sampleRealResolution;

                    Vector2Int centerOffset = i - new Vector2Int(source.texture.width / 2, source.texture.height / 2);
                    centerOffset.y *= -1; // The y-axis of sprite points down.
                    sampledVertice.Add((Vector2)centerOffset / (float)source.texture.width * spriteRealWidth);
                }
            }
            last = i;
        }
    }

    public IEnumerator GetEnumerator()
    {
        return sampledVertice.GetEnumerator();
    }

}
