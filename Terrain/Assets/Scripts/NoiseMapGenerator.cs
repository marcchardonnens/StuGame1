using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NoiseMapGenerator
{
    public static float[,,,] GeneratePerlinNM(int xSize, int zSize,int seed, int xChunks, int zChunks, NoiseData noiseData)
    {
        return GeneratePerlinNM(xSize, zSize, seed, xChunks, zChunks, noiseData.scale, noiseData.persistance, noiseData.lacunarity,
            noiseData.octaves, noiseData.xOffset, noiseData.zOffset, noiseData.overallMult);
    }

    private static int count = 1;
    public static float[,,,] GeneratePerlinNM(int xSize, int zSize, int seed, int xChunks, int zChunks, float scale, float persistance, float lacunarity, int octaves, float xOffset, float zOffset, float overallMult)
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
                //xRandOffset += (xchunk * xSize);
                //zRandOffset += (zchunk * zSize);

                //Debug.Log("xchunk offset:   " + (xchunk * xSize));
                //Debug.Log("xchunk offset:   " + (zchunk * zSize));
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
                            //float sx = ((x - xSize / 2f) / scale) * frequency + xOffset + xRandOffset - ((((float)xchunk * (float)xSize) /scale));
                            //float sz = ((z - zSize / 2f) / scale) * frequency + zOffset + zRandOffset - ((((float)zchunk * (float)zSize) /scale));


                            float sx = (x / scale) * frequency + xOffset + xRandOffset + xChunkOffset;
                            float sz = (z / scale) * frequency + zOffset + zRandOffset + zChunkOffset;

                            //float sx = ((x) / scale) * frequency + xOffset + xRandOffset;
                            //float sz = ((z) / scale) * frequency + zOffset + zRandOffset;
                            float perlin = Mathf.PerlinNoise(sx, sz) * 2 - 1;
                            y += perlin * amplitude;
                            amplitude *= sPrs;
                            frequency *= sLacu;
                        }



                        noisemap[x, z, xchunk, zchunk] = y * overallMult;
                    }
                }
            }
        }

        //Debug.Log("map " + count.ToString() + "     "+ noisemap.Length);
        //Debug.Log("index 350,350    " + noisemap[350, 350].ToString());
        //using (FileStream fs = File.Open("./map"+count.ToString()+".txt", FileMode.Create))
        //{
        //    StreamWriter sw = new StreamWriter(fs);
        //    for (int i = 0, z = 0; z < zSize; z++)
        //    {
        //        for (int x = 0; x < xSize; x++, i++)
        //        {
        //            sw.Write(noisemap[x, z].ToString() + "  ");
        //        }
        //        sw.WriteLine("");
        //    }

        //}

        count++;

        return noisemap;
    }


    //TODO BUGGED
    //falloff right now is per map, need to make it over all chunks
    public static float[,,,] GenerateFalloff(int xSize, int zSize, int xChunks, int zChunks)
    {
        float[,,,] noisemap = new float[xSize, zSize, xChunks, zChunks];

        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z < zSize; z++)
                {
                    for (int x = 0; x < xSize; x++)
                    {
                        float sx = x / (float)xSize * 2 - 1;
                        float sz = z / (float)zSize * 2 - 1;

                        float y = Mathf.Max(Mathf.Abs(sx), Mathf.Abs(sz));
                        noisemap[x, z, xchunk, zchunk] = y;
                    }
                }
            }
        }


        return noisemap;
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

            //Debug.Log("map in loop     " + map.Length);
            //Debug.Log("index 350,350 in loop    " + map[350, 350].ToString());

            //Debug.Log("combined in loop     " + combined.Length);
            //Debug.Log("index 350,350 in loop    " + combined[350, 350].ToString());

        }
        //Debug.Log("combined     " + combined.Length);
        //Debug.Log("index 350,350    " + combined[350, 350].ToString());
        //using (FileStream fs = File.Open("./combined.txt", FileMode.Create))
        //{
        //    StreamWriter sw = new StreamWriter(fs);
        //    for (int i = 0, z = 0; z < zSize; z++)
        //    {
        //        for (int x = 0; x < xSize; x++, i++)
        //        {
        //            sw.Write(combined[x, z].ToString() + "  ");
        //        }
        //        sw.WriteLine("");
        //    }

        //}

        return combined;
    }


}