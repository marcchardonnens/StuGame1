using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NoiseMapGenerator
{
    public static float[,,,] GeneratePerlinNM(int xSize, int zSize,int seed, int xChunks, int zChunks, NoiseData noiseData)
    {
        return GeneratePerlinNM(xSize, zSize, seed, xChunks, zChunks, noiseData.scale, noiseData.persistance, noiseData.lacunarity,
            noiseData.octaves, noiseData.xOffset, noiseData.zOffset, noiseData.overallMult, noiseData.animationCurve, noiseData.positiveOnly);
    }

    private static int count = 1;
    public static float[,,,] GeneratePerlinNM(int xSize, int zSize, int seed, int xChunks, int zChunks, float scale, float persistance, 
        float lacunarity, int octaves, float xOffset, float zOffset, float overallMult, AnimationCurve animationCurve, bool positiveOnly)
    {
        float[,,,] noisemap = new float[xSize, zSize,xChunks,zChunks];

        System.Random rng = seed == 0 ? new System.Random() : new System.Random(seed);

        float xRandOffset = rng.Next(-100000, 100000);
        float zRandOffset = rng.Next(-100000, 100000);

        if (scale == 0)
        {
            scale = 0.0001f;
        }

        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                float xChunkOffset = 0f;
                float zChunkOffset = 0f;
                if (scale > 1)
                {
                    xChunkOffset = (((float)xchunk * (float)xSize) / scale);
                    zChunkOffset = (((float)zchunk * (float)zSize) / scale);
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
                            float sx = ((x - xSize / 2f) / scale) * frequency + xOffset + xRandOffset + xChunkOffset;
                            float sz = ((z - zSize / 2f) / scale) * frequency + zOffset + zRandOffset + zChunkOffset;

                            float perlin = Mathf.PerlinNoise(sx, sz);
                            perlin = animationCurve.Evaluate(perlin);
                            if (!positiveOnly)
                            {
                                perlin = perlin * 2 - 1;
                            }
                            y += perlin * amplitude;
                            amplitude *= sPrs;
                            frequency *= sLacu;
                        }

                        noisemap[x, z, xchunk, zchunk] = y * overallMult;
                    }
                }
            }
        }
        
        count++;

        return noisemap;
    }


    //TODO BUGGED
    //falloff right now is per map, need to make it over all chunks
    public static float[,,,] GenerateFalloff(int xSize, int zSize, int xChunks, int zChunks)
    {
        float[,,,] noisemap = new float[xSize + 1, zSize + 1, xChunks, zChunks];

        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++)
                    {
                        float sx = (x / (float)(xSize*xChunks)) * 2 - 1;
                        float sz = (z / (float)(zSize*zChunks)) * 2 - 1;

                        float y = Mathf.Max(Mathf.Abs(sx), Mathf.Abs(sz));
                        y = Evaluate(y);
                        y = Mathf.Lerp(-30f, 1, y);
                        noisemap[x, z, xchunk, zchunk] = -y;
                    }
                }
            }
        }


        return noisemap;

    //    float[,] map = new float[size, size];

    //    for (int i = 0; i < size; i++)
    //    {
    //        for (int j = 0; j < size; j++)
    //        {
    //            float x = i / (float)size * 2 - 1;
    //            float y = j / (float)size * 2 - 1;

    //            float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
    //            map[i, j] = Evaluate(value);
    //        }
    //    }

    //    return map;
    //}


    }
    private static float Evaluate(float value)
    {
        float a = 3;
        float b = 2.2f;

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }

    public static float[,,,] CombineMaps(List<float[,,,]> maps, int xSize, int zSize, int xChunks, int zChunks)
    {
        float[,,,] combined = new float[xSize, zSize, xChunks, zChunks];
        foreach (float[,,,] map in maps)
        {
            for (int zchunk = 0; zchunk < zChunks; zchunk++)
            {
                for (int xchunk = 0; xchunk < xChunks; xchunk++)
                {
                    for (int z = 0; z < zSize; z++)
                    {
                        for (int x = 0; x < xSize; x++)
                        {
                            combined[x, z, xchunk, zchunk] += map[x, z, xchunk, zchunk];
                        }
                    }
                }
            }
        }

        return combined;
    }

    public static Vector2Int FindChunk(Vector2 pos, int xSize, int zSize)
    {
        Vector2Int chunk = new Vector2Int();

        float x = pos.x;
        float z = pos.y;

        chunk.x = (int)x / xSize;
        chunk.y = (int)z / zSize;


        return chunk;
    }




}