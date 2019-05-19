using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteBoundary : IEnumerable
{
    const float alpha_thres = 1.0f;
    Dictionary<int, Vector2Int> distance2Pos;
    //HashSet<Vector2Int> dbg_b = new HashSet<Vector2Int>();

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Section"/> class.
    /// </summary>
    /// <param name="shape">Must be pixelwise STRONGLY connected</param>
    /// <param name="realWorldSize">Real size of shape's width in meters</param>
    /// <param name="resolution">Resolution of enumarator</param>
    public SpriteBoundary(Sprite source)
    {
        Texture2D shape = source.texture;
        int width = shape.width;
        int height = shape.height;
        Texture2D shape_cpy = new Texture2D(width, height);
        RectInt bound = new RectInt(0, 0, width, height);

        Vector2Int[] eight_offsets = {
        new Vector2Int(-1,0),new Vector2Int(0,-1), new Vector2Int(0,1),
        new Vector2Int(1,0),new Vector2Int(-1,1),new Vector2Int(-1,-1), new Vector2Int(1,1),
        new Vector2Int(1,-1)};

        Vector2Int[] four_offsets = {
        new Vector2Int(-1,0),new Vector2Int(0,-1), new Vector2Int(0,1),
        new Vector2Int(1,0)};
        Queue<Vector2Int> BFSQueue = new Queue<Vector2Int>();

        /// <summary>
        /// 8-direction edge check
        /// </summary>
        bool isCountour(int i, int j)
        {
            if (shape.GetPixel(i, j).a < alpha_thres)
            {
                return false;
            }
            if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
            {
                return true;
            }
            for (int neighbor_i = i - 1; neighbor_i <= i + 1; ++neighbor_i)
            {
                for (int neighbor_j = j - 1; neighbor_j <= j + 1; ++neighbor_j)
                {
                    if (shape.GetPixel(neighbor_i, neighbor_j).a < alpha_thres)
                    {
                        return true;
                    }
                }
            }
            return false;
        }


        for(int i = 0; i != width; ++i)
        {
            for(int j = 0; j != height; ++j)
            {
                if (isCountour(i, j))
                {
                    shape_cpy.SetPixel(i, j, Color.white);
                    //dbg_b.Add(new Vector2Int(i, j));
                }
                else
                {
                    shape_cpy.SetPixel(i, j, Color.black);
                }
            }
        }

        /// <summary>
        /// Look for an origin with EXACTLY two (8-Dir) neighbors
        /// while these two should not be adjacent
        /// </summary>
        /// 
        (Vector2Int, Vector2Int) Get2NeiborPixel()
        {
            Vector2Int candidate = new Vector2Int();
            for (int i = 0; i != width; ++i)
            {
                for (int j = 0; j != height; ++j)
                {
                    candidate.x = i;
                    candidate.y = j;
                    int neighborCount = 0;

                    Vector2Int aNeighborPos = new Vector2Int(-1,-1);
                    Vector2Int bNeighborPos = new Vector2Int(-1,-1);
                    foreach (Vector2Int offset in eight_offsets)
                    {
                        Vector2Int neighborpos = candidate + offset;
                        if (bound.Contains(candidate) && shape_cpy.GetPixel(neighborpos.x, neighborpos.y) == Color.white)
                        {
                            neighborCount++;
                            if (aNeighborPos.x < 0)
                            {
                                aNeighborPos = neighborpos;
                            }
                            else
                            {
                                bNeighborPos = neighborpos;
                            }
                        }
                    }
                    if (neighborCount == 2 && (aNeighborPos - bNeighborPos).sqrMagnitude > 1.5)
                    {
                        return (candidate, aNeighborPos);
                    }
                }
            }
            throw new System.Exception("Cannot find 2-neighbor pixel");
        }

        Vector2Int origin, originNeighbor;
        (origin, originNeighbor) = Get2NeiborPixel();

        // Force one starting direction
        Dictionary<Vector2Int, int> distance = new Dictionary<Vector2Int, int>();
        distance[origin] = 0;
        distance[originNeighbor] = 1;
        shape_cpy.SetPixel(origin.x, origin.y, Color.gray);
        shape_cpy.SetPixel(originNeighbor.x, originNeighbor.y, Color.gray);
        BFSQueue.Enqueue(originNeighbor);

        // BFS single step
        void visitPos(Vector2Int pos)
        {
            int posDistance = distance[pos];
            foreach (Vector2Int offset in four_offsets)
            {
                Vector2Int neighborpos = pos + offset;
                if (bound.Contains(neighborpos) && shape_cpy.GetPixel(neighborpos.x, neighborpos.y) == Color.white)
                {
                    shape_cpy.SetPixel(neighborpos.x, neighborpos.y, Color.gray); //Discovered
                    distance[neighborpos] = posDistance + 1;
                    BFSQueue.Enqueue(neighborpos);
                }
            }
        }

        //BFS loop
        while (BFSQueue.Count != 0)
        {
            visitPos(BFSQueue.Dequeue());
        }
        
        // Inverse mapping: Distance -> Int2
        distance2Pos = new Dictionary<int, Vector2Int>();


        foreach(Vector2Int pos in distance.Keys)
        {
            int d = distance[pos];
            distance2Pos[d] = pos;
        }
    }


    public IEnumerator GetEnumerator()
    {

        for (int d = 0; d != distance2Pos.Count; ++d)
        {
            yield return distance2Pos[d];
        }

    }

}
