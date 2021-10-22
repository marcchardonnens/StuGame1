using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTexture : MonoBehaviour
{
    public bool autoupdate = false;
    public Renderer texturesRenderer;
    public int seed = 0;
    public int xSize = 100;
    public int zSize = 100;
    public float scale = 0.3f;
    public int octaves = 1;
    [Range(0,1)]
    public float persistance = 1f;
    public float lacunarity = 0f;

    public float xOffset = 0;
    public float zOffset = 0;


    private Texture2D texture;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Generate()
    {
        texture = new Texture2D(xSize, zSize);

        float minval = float.MaxValue;
        float maxval = float.MinValue;

        float[,] noisemap = NoiseMapGenerator.GeneratePerlinNM(xSize, zSize, seed, scale, persistance, lacunarity,
            octaves, xOffset, zOffset);
        float[,] noisemap2 = NoiseMapGenerator.GeneratePerlinNM(xSize, zSize, seed, 5, 0.5f, 2f,
            1, xOffset, zOffset);

        

        Color[] colorMap = new Color[xSize * zSize];
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                noisemap[x, z] += noisemap2[x, z];

                if (noisemap[x, z] > maxval)
                {
                    maxval = noisemap[x, z];
                }
                else if (noisemap[x, z] < minval)
                {
                    minval = noisemap[x, z];
                }

            }
        }

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                noisemap[x, z] = Mathf.InverseLerp(minval, maxval, noisemap[x, z]);
            }
        }



        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                colorMap[z * xSize + x] = Color.Lerp(Color.black, Color.white, noisemap[x, z]);
            }
        }

        texture.SetPixels(colorMap);
        texture.Apply();

        texturesRenderer.sharedMaterial.mainTexture = texture;
        texturesRenderer.transform.localScale = new Vector3(xSize, 1, zSize); 

    }

    public void GenerateSimple()
    {


        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        Color[] colorMap = new Color[(xSize + 1) * (zSize + 1)];



        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = 0;

                float sx = x / scale + xOffset;
                float sz = z / scale + zOffset;
                float perlin = Mathf.PerlinNoise(sx, sz);
                y += perlin;

                vertices[z * xSize + x] = new Vector3(x, y, z);
                colorMap[z * xSize + x] = Color.Lerp(Color.black, Color.white, y);
            }
        }



        texture.SetPixels(colorMap);
        texture.Apply();

        texturesRenderer.sharedMaterial.mainTexture = texture;
        texturesRenderer.transform.localScale = new Vector3(xSize, 1, zSize);

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
        if (lacunarity < 1)
        {
            lacunarity = 1;
        }
        if (octaves < 0)
        {
            octaves = 0;
        }
    }
}
