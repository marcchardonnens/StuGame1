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

    public int xChunks = 1;
    public int zChunks = 1;

    public NoiseData[] noisedata;

    [SerializeField]private int xSize = 200;
	[SerializeField]private int zSize = 200;

    [SerializeField] private float edge = 20f;


    public Material material;
    public TextureData TextureData;

    public GameObject TreePrefab;
    [SerializeField] private float TreeEdge = 20f;
    


	private void Awake()
    {

        Generate();
        //StartCoroutine(GenerateObjects());


    }

	// Start is called before the first frame update
	void Start()
    {


    }

    void FixedUpdate()
    {

    }


	//get index from second for loop to 2d iterate over vertices
	//int yIndex = z * xsize + z  ----------> current y
	public void Generate()
    {
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


        this.gameObject.transform.position = new Vector3(-xSize * xChunks / 2, 0, -zSize * zChunks / 2);
        
        List<float[,,,]> maps = new List<float[,,,]>();
        foreach (NoiseData noiseData in noisedata)
        {
            maps.Add(NoiseMapGenerator.GeneratePerlinNM(xSize + 1, zSize + 1, seed, xChunks, zChunks,  noiseData));
        }
        maps.Add(NoiseMapGenerator.GenerateFalloff(xSize, zSize, xChunks, zChunks));

        float[,,,] combinedMap = NoiseMapGenerator.CombineMaps(maps,xSize+1,zSize+1,xChunks,zChunks);


        MakeChunks(combinedMap);

        TextureData.ApplyToMaterial(material);
        TextureData.UpdateMeshHeights(material,TextureData.MinHeight,TextureData.MaxHeight);

    }

    private void MakeChunks(float[,,,] combinedMap)
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
                        vertices[i] = new Vector3(x, combinedMap[x, z, xchunk, zchunk], z);
                    }
                }

                GameObject terrainChunk = new GameObject("map chunk " + xchunk + " " + zchunk);
                terrainChunk.transform.parent = gameObject.transform;
                terrainChunk.transform.localPosition = new Vector3((xSize * xchunk - xchunk*1f) /*/ transform.localScale.x*/, 0, (zSize * zchunk - zchunk*1f) /*/ transform.localScale.z*/);
                terrainChunk.transform.localScale = new Vector3(1, 1, 1);

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
		meshCollider.sharedMesh = mesh;
	}


	//shoud probably throw that out and do it properly
	private void CreateSingleMountain()
    {
		//moutain mental notes
		//select mounsize x elements
		//select mounsize z elements
		//create new array and modify y values
	}

	private void LowerEdgeVertices(float x,ref float y, float z)
    {
		if (x < edge)
		{
			y -= Mathf.Abs(0.5f * (x - edge));
		}
		else if (z < edge)
		{
			y -= Mathf.Abs(0.5f * (z - edge));
		}
		else if (x > xSize - edge)
		{
			y -= Mathf.Abs(0.5f * (x + edge - xSize));
		}
		else if (z > zSize - edge)
		{
			y -= Mathf.Abs(0.5f * (z + edge - zSize));
		}
	}


    private IEnumerator GenerateObjects()
    {
        yield return 1;

		int layermask = 1 << 8;

        for (int i = 0; i < 100; i++)
        {
            float x = Random.Range(TreeEdge, xSize - TreeEdge) + transform.position.x;
            float z = Random.Range(TreeEdge, zSize - TreeEdge) + transform.position.z;

            RaycastHit hit;
            if (Physics.Raycast(new Vector3(x,10f,z), Vector3.down, out hit, 2000f, layermask))
            {
                //Instantiate(TreePrefab, new Vector3(x,TreePrefab.transform.position.y + hit.transform.position.y,z),Quaternion.identity);
                Instantiate(TreePrefab,TreePrefab.transform.position + hit.point, Quaternion.Euler(0,Random.Range(0,360f),0));
				//Debug.Log(hit.point.y);
			}
            else
            {
			}
			//Debug.DrawRay(new Vector3(x,100f, z), Vector3.down * 10000, Color.magenta);
        }


    }


}


[Serializable]
public class NoiseData
{
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