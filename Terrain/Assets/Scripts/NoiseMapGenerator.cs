using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseMapGenerator
{

    public static float[,] GeneratePerlinNM(int xSize, int zSize, int seed, float scale, float persistance, float lacunarity, int octaves, float xOffset, float zOffset)
    {
        float[,] noisemap = new float[xSize, zSize];

        System.Random rng = seed == 0 ? new System.Random() : new System.Random(seed);



        float xRandOffset = rng.Next(-100000, 100000);
        float zRandOffset = rng.Next(-100000, 100000);

        
        Color[] colorMap = new Color[xSize * zSize];


        if (scale == 0)
        {
            scale = 0.0001f;
        }

        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float y = 0;
                float sPrs = persistance;
                float sLacu = lacunarity;
                float amplitude = 1;
                float frequency = 1;
                for (int j = 0; j < octaves; j++)
                {
                    float sx = ((x - xSize / 2f) / scale) * frequency + xOffset + xRandOffset;
                    float sz = ((z - zSize / 2f) / scale) * frequency + zOffset + zRandOffset;
                    float perlin = Mathf.PerlinNoise(sx, sz) * 2 - 1;
                    y += perlin * amplitude;
                    amplitude *= sPrs;
                    frequency *= sLacu;
                }

                noisemap[x, z] = y;
            }
        }



        return noisemap;
    }

    public static float[,] GenerateFalloff(int xSize, int zSize)
    {
        float[,] noisemap = new float[xSize, zSize];


        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                float sx = x / (float)xSize * 2 - 1;
                float sz = z / (float)zSize * 2 - 1;

                float y = Mathf.Max(Mathf.Abs(sx), Mathf.Abs(sz));
                noisemap[x, z] = y;
            }
        }



        return noisemap;
    }


}