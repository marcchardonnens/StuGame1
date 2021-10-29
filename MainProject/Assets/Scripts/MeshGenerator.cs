using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshGenerator
{
    public Vector2 minmaxActual;

    private float[,,,] combinedMap;
    private int seed;
    private int xSize;
    private int zSize;
    private int xChunks;
    private int zChunks;


    private NoiseData[] noisedata;
    private Material material;
    private TextureData TextureData;

    public MeshGenerator(float[,,,] combinedMap, int seed, int xSize, int zSize, int xChunks, int zChunks, NoiseData[] noisedata,
        Material material, TextureData textureData)
    {
        this.combinedMap = combinedMap;
        this.seed = seed;
        this.xSize = xSize;
        this.zSize = zSize;
        this.xChunks = xChunks;
        this.zChunks = zChunks;
        this.noisedata = noisedata;
        this.material = material;
        this.TextureData = textureData;
        minmaxActual = FindActualMinMax();
    }

    


    //get index from second for loop to 2d iterate over vertices
    //int yIndex = z * xsize + z  ----------> current y
    public List<GameObject> Generate(bool doLerp = false)
    {
        List<GameObject> chunks = MakeChunks(doLerp);

        return chunks;
    }



    public Vector2 FindActualMinMax()
    {
        Vector2 minmax = new Vector2(float.MaxValue,float.MinValue);
        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
                for (int i = 0, z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++, i++)
                    {
                        if(combinedMap[x,z,xchunk,zchunk] < minmax.x)
                        {
                            minmax.x = combinedMap[x, z, xchunk, zchunk];
                        }
                        else if (combinedMap[x, z, xchunk, zchunk] > minmax.y)
                        {
                            minmax.y = combinedMap[x, z, xchunk, zchunk];
                        }
                    }
                }
            }
        }

        return minmax;
    }

    public Vector2 CalcPotentialMinMax()
    {
        float multipliers = 0f;
        foreach (NoiseData noise in noisedata)
        {
            if (noise.enabled)
            {
                multipliers += noise.overallMult;
            }
            //if (noise.overallMult > multipliers)
            //{
            //    multipliers = noise.overallMult;
            //}
        }

        return new Vector2(-multipliers, multipliers);
    }


    /*
     * 
     *      very interesting terrain but not intended
     *      float y = animationCurve.Evaluate(Mathf.InverseLerp(0, 1f, combinedMap[x, z, xchunk, zchunk])) * 20f;
            vertices[i] = new Vector3(x, y, z);
     * 
     * 
     * 
     */

    private List<GameObject> MakeChunks(bool doLerp)
    {
        List<GameObject> objects = new List<GameObject>();
        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
                for (int i = 0, z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++, i++)
                    {
                        if (doLerp)
                        {
                            float ylerped = Mathf.Lerp(minmaxActual.x, minmaxActual.y, combinedMap[x, z, xchunk, zchunk]);
                            vertices[i] = new Vector3(x, ylerped, z);
                        }
                        else
                        {
                            vertices[i] = new Vector3(x, combinedMap[x, z, xchunk, zchunk], z);
                        }
                    }
                }

                GameObject terrainChunk = new GameObject("map chunk " + xchunk + " " + zchunk);
                //terrainChunk.transform.parent = gameObject.transform;
                terrainChunk.transform.localPosition = new Vector3((xSize * xchunk - xchunk), 0, (zSize * zchunk - zchunk));
                terrainChunk.transform.localScale = new Vector3(1, 1, 1);
                terrainChunk.layer = 6;

                Mesh mesh = terrainChunk.AddComponent<MeshFilter>().sharedMesh = new Mesh();
                MeshRenderer meshRenderer = terrainChunk.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = material;
                meshRenderer.material = material;
                MeshCollider meshCollider = terrainChunk.AddComponent<MeshCollider>();
                
                mesh.vertices = vertices;

                FinalizeMesh(mesh, meshCollider);

                objects.Add(terrainChunk);

            }
        }

        return objects;
    }


    private void FinalizeMesh(Mesh mesh, MeshCollider meshCollider)
    {

		int[] triangles = new int[xSize * zSize * 6];
		for (int ti = 0, vi = 0, z = 0; z < zSize; z++, vi++)
		{
			for (int x = 0; x < xSize; x++, ti += 6, vi++)
			{
				triangles[ti] = vi;
				triangles[ti + 3] = triangles[ti + 2] = vi + 1;
				triangles[ti + 4] = triangles[ti + 1] = vi + xSize + 1;
				triangles[ti + 5] = vi + xSize + 2;
			}
		}
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
        mesh.Optimize();
		meshCollider.sharedMesh = mesh;
	}


}





