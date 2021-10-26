using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTexture : MonoBehaviour
{
    public bool autoupdate = false;
    public GameObject RenderPlane;
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

        int count = transform.childCount;
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < count; i++)
        {
            children.Add(transform.GetChild(i));
        }

        for (int i = 0; i < count; i++)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(children[i].gameObject, true);
            }
            else
            {
                Destroy(children[i]);
            }
        }
        children = null;
        count = 0;

        List<float[,,,]> maps = new List<float[,,,]>();
        foreach (NoiseData noiseData in noisedata)
        {
            maps.Add(NoiseMapGenerator.GeneratePerlinNM(xSize + 1, zSize + 1, seed, xChunks, zChunks, noiseData));
        }





        float[,,,] combinedMap;
        //if (maps.Count > 1)
        {
            combinedMap = NoiseMapGenerator.CombineMaps(maps, (xSize + 1), (zSize + 1), xChunks, zChunks);
        }
        //else
        {
            //combinedMap = maps[0];
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


        MakeChunks(combinedMap);




    }




    private void MakeChunks(float[,,,] combinedMap)
    {
        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                Texture2D texture = new Texture2D((xSize + 1), (zSize + 1));
                Color[] colorMap = new Color[(xSize + 1) * (zSize + 1)];
                for (int i = 0, z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++, i++)
                    {
                        colorMap[i] = Color.Lerp(Color.black, Color.white, combinedMap[x, z, xchunk, zchunk]);
                    }
                }

                //GameObject terrainChunk = new GameObject("map chunk " + xchunk + " " + zchunk);

                GameObject terrainChunk = Instantiate(RenderPlane, transform);
                terrainChunk.name = "map chunk " + xchunk + " " + zchunk;
                terrainChunk.transform.parent = gameObject.transform;
                terrainChunk.transform.localPosition = new Vector3((xSize+1) * 10*xchunk, 0, (zSize+1) * 10*zchunk);


                texture.SetPixels(colorMap);
                texture.Apply();


                Renderer texturesRenderer = terrainChunk.GetComponent<Renderer>();

                texturesRenderer.sharedMaterial = new Material(texturesRenderer.sharedMaterial);
                texturesRenderer.sharedMaterial.mainTexture = texture;
                texturesRenderer.transform.localScale = new Vector3((xSize + 1), 1, (zSize + 1));
            }
        }
    }



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
