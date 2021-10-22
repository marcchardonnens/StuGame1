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
        System.Random rng = seed == 0 ? new System.Random() : new System.Random(seed);
        texture = new Texture2D(xSize, zSize);

        float minval = float.MaxValue;
        float maxval = float.MinValue;

        float xRandOffset = rng.Next(-100000, 100000);
        float zRandOffset = rng.Next(-100000, 100000);


        Vector3[] vertices = new Vector3[xSize * zSize];
        Color[] colorMap = new Color[xSize * zSize];


        if(scale == 0)
        {
            scale = 0.0001f;
        }

        for (int i = 0, z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++, i++)
            {
                float y = 0;
                float sPrs = persistance;
                float sLacu = lacunarity;
                float amplitude = 1;
                float frequency  = 1;
                for (int j = 0; j < octaves; j++)
                {
                    float sx = ((x - xSize/2f) / scale) * frequency + xOffset + xRandOffset;
                    float sz = ((z - zSize/2f) / scale) * frequency + zOffset + zRandOffset;
                    float perlin = Mathf.PerlinNoise(sx, sz) * 2 - 1;
                    y += perlin * amplitude;
                    amplitude *= sPrs;
                    frequency *= sLacu;
                }

                if(y > maxval)
                {
                    maxval = y;
                }
                else if(y < minval)
                {
                    minval = y;
                }

                vertices[i] = new Vector3(x, y, z);
            }
        }


        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float y = vertices[z * xSize + x].y;
                y = Mathf.InverseLerp(minval, maxval, y);
                colorMap[z * xSize + x] = Color.Lerp(Color.black, Color.white, y);
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
