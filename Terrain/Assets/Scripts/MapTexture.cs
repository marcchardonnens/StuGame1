using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTexture : MonoBehaviour
{
    public bool autoupdate = false;
    public Renderer texturesRenderer;
    public int xChunks = 1;
    public int zChunks = 1;
    public int seed = 0;
    public int xSize = 100;
    public int zSize = 100;

    public NoiseData[] noisedata;



    public void Generate()
    {

        if (noisedata.Length <= 0)
        {
            Debug.LogError("no noise data");
            return;
        }

        List<float[,,,]> maps = new List<float[,,,]>();
        foreach (NoiseData noiseData in noisedata)
        {
            maps.Add(NoiseMapGenerator.GeneratePerlinNM((xSize + 1), (zSize + 1), seed, xChunks, zChunks, noiseData));
        }

        float[,,,] combinedMap;
        if (maps.Count > 1)
        {
            combinedMap = NoiseMapGenerator.CombineMaps(maps, (xSize + 1), (zSize + 1), xChunks, zChunks);
        }
        else
        {
            combinedMap = maps[0];
        }


        Texture2D texture = new Texture2D((xSize+1) * xChunks, (zSize+1) * zChunks);

        float minval = float.MaxValue;
        float maxval = float.MinValue;

        

        Color[] colorMap = new Color[(xSize+1) * (zSize+1) * xChunks * zChunks];

        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++)
                    {
                        if (combinedMap[x, z, xchunk, zchunk] > maxval)
                        {
                            maxval = combinedMap[x, z, xchunk, zchunk];
                        }
                        else if (combinedMap[x, z, xchunk, zchunk] < minval)
                        {
                            minval = combinedMap[x, z, xchunk, zchunk];
                        }

                    }
                }
            }
        }


        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++)
                    {
                        combinedMap[x, z, xchunk, zchunk] = Mathf.InverseLerp(minval, maxval, combinedMap[x, z, xchunk, zchunk]);
                    }
                }
            }
        }


        for (int zchunk = 0, i = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++,i++)
                    {
                        //colorMap[z * xSize + x] = Color.Lerp(Color.black, Color.white, noisemap[x, z, xchunk, zchunk]);
                        //colorMap[z * xSize + x + ((zchunk * zSize) * xChunks + (xchunk * xSize))] = Color.Lerp(Color.black, Color.white, combinedMap[x, z, xchunk, zchunk]);
                        colorMap[i] = Color.Lerp(Color.black, Color.white, combinedMap[x, z, xchunk, zchunk]);
                    }
                }
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();

        //texturesRenderer.sharedMaterial.mainTexture = texture;
        texturesRenderer.sharedMaterial = new Material(texturesRenderer.sharedMaterial);
        texturesRenderer.sharedMaterial.mainTexture = texture;
        texturesRenderer.transform.localScale = new Vector3((xSize+1)*xChunks, 1, (zSize+1)*zChunks);

    }

    //public void GenerateSimple()
    //{


    //    Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
    //    Color[] colorMap = new Color[(xSize + 1) * (zSize + 1)];



    //    for (int z = 0; z <= zSize; z++)
    //    {
    //        for (int x = 0; x <= xSize; x++)
    //        {
    //            float y = 0;

    //            float sx = x / scale + xOffset;
    //            float sz = z / scale + zOffset;
    //            float perlin = Mathf.PerlinNoise(sx, sz);
    //            y += perlin;

    //            vertices[z * xSize + x] = new Vector3(x, y, z);
    //            colorMap[z * xSize + x] = Color.Lerp(Color.black, Color.white, y);
    //        }
    //    }



    //    texture.SetPixels(colorMap);
    //    texture.Apply();

    //    texturesRenderer.sharedMaterial.mainTexture = texture;
    //    texturesRenderer.transform.localScale = new Vector3(xSize, 1, zSize);

    //}


    void OnValidate()
    {
        if (xSize < 1)
        {
            xSize = 1;
        }
        if (zSize < 1)
        {
            zSize = 1;
        }
        //if (noiselacunarity < 1)
        //{
        //    lacunarity = 1;
        //}
        //if (octaves < 0)
        //{
        //    octaves = 0;
        //}
    }
}
