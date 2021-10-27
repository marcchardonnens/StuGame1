using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshGenerator : MonoBehaviour
{
	public bool autoupdate = false;
    
    public int seed = 0;
    public int xSize = 200;
    public int zSize = 200;
    public int xChunks = 1;
    public int zChunks = 1;

    public AnimationCurve animationCurve = new AnimationCurve();

    public NoiseData[] noisedata;




    public Material material;
    public GameObject Water;
    public TextureData TextureData;



    public void GenerateInternal()
    {
        List<float[,,,]> maps = new List<float[,,,]>();
        foreach (NoiseData noiseData in noisedata)
        {
            if (noiseData.enabled)
            {
                maps.Add(NoiseMapGenerator.GeneratePerlinNM(xSize + 1, zSize + 1, seed, xChunks, zChunks, noiseData));
            }
        };

        float[,,,] combinedMap = NoiseMapGenerator.CombineMaps(maps, xSize + 1, zSize + 1, xChunks, zChunks);

        Generate(combinedMap);
    }



    //get index from second for loop to 2d iterate over vertices
    //int yIndex = z * xsize + z  ----------> current y
    public void Generate(float[,,,] combinedMap)
    {
        int count = transform.childCount;
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < count; i++)
        {   
            //TODO better way of doing this
            //right now this is so water/other objects dont get deleted.
            //deleting them and re-instantiating might be better or necessary, but this is easier for testing
            if(transform.GetChild(i).name.StartsWith("map chunk"))
            {
                children.Add(transform.GetChild(i));
            }
        }

        foreach(Transform child in children)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(child.gameObject, true);
            }
            else
            {
                Destroy(child);
            }
        }
        children = null;
        count = 0;


        this.gameObject.transform.position = new Vector3(-xSize * xChunks / 2, 0, -zSize * zChunks / 2);
        



        Vector2 minmax = FindMaxHeightMult(combinedMap);

        float waterHeight = Mathf.Lerp(minmax.x, minmax.y, 0.3f);// -0.33f ;
        Water.transform.localPosition = new  Vector3(transform.localScale.x * 100f, waterHeight * transform.localScale.y, transform.localScale.z * 100f);
        Water.transform.localScale = new Vector3(transform.localScale.x * 200f, 1, transform.localScale.z * 200f);

        MakeChunks(combinedMap, minmax);


        TextureData.ApplyToMaterial(material);
        TextureData.UpdateMeshHeights(material,minmax.x * transform.localScale.y,minmax.y * transform.localScale.y);

    }

    private Vector2 FindMaxHeightMult(float[,,,] combined)
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
                        if(combined[x,z,xchunk,zchunk] < minmax.x)
                        {
                            minmax.x = combined[x, z, xchunk, zchunk];
                        }
                        else if (combined[x, z, xchunk, zchunk] > minmax.y)
                        {
                            minmax.y = combined[x, z, xchunk, zchunk];
                        }
                    }
                }
            }
        }

        return minmax;
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



    private void MakeChunks(float[,,,] combinedMap, Vector2 minmax)
    {
        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
                for (int i = 0, z = 0; z <= zSize; z++)
                {
                    for (int x = 0; x <= xSize; x++, i++)
                    {
                        //float y = animationCurve.Evaluate(Mathf.InverseLerp(minmax.y, 1f, combinedMap[x, z, xchunk, zchunk])) * 20f;
                        float cury = combinedMap[x, z, xchunk, zchunk];
                        float yinv =  Mathf.InverseLerp(minmax.x, minmax.y, combinedMap[x, z, xchunk, zchunk]);
                        float y = animationCurve.Evaluate(yinv);
                        Vector3 v =    new Vector3(x, combinedMap[x,z,xchunk,zchunk]*y, z);
                        vertices[i] = v;
                    }
                }

                GameObject terrainChunk = new GameObject("map chunk " + xchunk + " " + zchunk);
                terrainChunk.transform.parent = gameObject.transform;
                terrainChunk.transform.localPosition = new Vector3((xSize * xchunk - xchunk*1f) /*/ transform.localScale.x*/, 0, (zSize * zchunk - zchunk*1f) /*/ transform.localScale.z*/);
                terrainChunk.transform.localScale = new Vector3(1, 1, 1);
                terrainChunk.layer = 6;

                Mesh mesh = terrainChunk.AddComponent<MeshFilter>().sharedMesh = new Mesh();
                MeshRenderer meshRenderer = terrainChunk.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = material;
                meshRenderer.material = material;
                MeshCollider meshCollider = terrainChunk.AddComponent<MeshCollider>();
                
                mesh.vertices = vertices;

                FinalizeMesh(mesh, meshCollider);

            }
        }
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


	//shoud probably throw that out and do it properly
	//private void CreateSingleMountain()
 //   {
	//	//moutain mental notes
	//	//select mounsize x elements
	//	//select mounsize z elements
	//	//create new array and modify y values
	//}

	//private void LowerEdgeVertices(float x,ref float y, float z)
 //   {
	//	if (x < edge)
	//	{
	//		y -= Mathf.Abs(0.5f * (x - edge));
	//	}
	//	else if (z < edge)
	//	{
	//		y -= Mathf.Abs(0.5f * (z - edge));
	//	}
	//	else if (x > xSize - edge)
	//	{
	//		y -= Mathf.Abs(0.5f * (x + edge - xSize));
	//	}
	//	else if (z > zSize - edge)
	//	{
	//		y -= Mathf.Abs(0.5f * (z + edge - zSize));
	//	}
	//}


  //  private IEnumerator GenerateObjects()
  //  {
  //      yield return 1;

		//int layermask = 1 << 8;

  //      for (int i = 0; i < 100; i++)
  //      {
  //          float x = Random.Range(TreeEdge, xSize - TreeEdge) + transform.position.x;
  //          float z = Random.Range(TreeEdge, zSize - TreeEdge) + transform.position.z;

  //          RaycastHit hit;
  //          if (Physics.Raycast(new Vector3(x,10f,z), Vector3.down, out hit, 2000f, layermask))
  //          {
  //              //Instantiate(TreePrefab, new Vector3(x,TreePrefab.transform.position.y + hit.transform.position.y,z),Quaternion.identity);
  //              Instantiate(TreePrefab,TreePrefab.transform.position + hit.point, Quaternion.Euler(0,Random.Range(0,360f),0));
		//		//Debug.Log(hit.point.y);
		//	}
  //          else
  //          {
		//	}
		//	//Debug.DrawRay(new Vector3(x,100f, z), Vector3.down * 10000, Color.magenta);
  //      }


  //  }


}


[Serializable]
public class NoiseData
{
    public bool enabled = true;
    public float scale = 50f;
    public int octaves = 4;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2f;
    public float overallMult = 1f;

    public float xOffset = 0;
    public float zOffset = 0;
}


[Serializable]
public class TextureData
{

    public float MinHeight = 0f;
    public float MaxHeight = 10f;

    public Color[] baseColours;
    [Range(0, 1)]
    public float[] baseStartHeights;

    float savedMinHeight;
    float savedMaxHeight;

    public void ApplyToMaterial(Material material)
    {

        material.SetInt("baseColourCount", baseColours.Length);
        material.SetColorArray("baseColours", baseColours);
        material.SetFloatArray("baseStartHeights", baseStartHeights);

        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

}