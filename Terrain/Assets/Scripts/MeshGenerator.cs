using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshGenerator : MonoBehaviour
{
	public bool DRAW = false;

    [SerializeField]private bool DrawEachFrame_Debug = false;

    public int seed = 0;

    public int xChunks = 1;
    public int zChunks = 1;

    public NoiseData[] noisedata;

    [SerializeField]private int xSize = 200;
	[SerializeField]private int zSize = 200;

    [SerializeField] private float edge = 20f;


    public Material material;

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
        if (DrawEachFrame_Debug)
        {
            Generate();
        }

	}


	//get index from second for loop to 2d iterate over vertices
	//int yIndex = z * xsize + z  ----------> current y
	private void Generate()
	{
        this.gameObject.transform.position += new Vector3(-xSize * xChunks / 2, 0, -zSize * zChunks / 2);
        if (!DrawEachFrame_Debug)
        {
			Debug.Log("generate start");
        }
        if (noisedata.Length <= 0)
        {
            return;
        }


        List<float[,]> maps = new List<float[,]>();
        foreach (NoiseData noiseData in noisedata)
        {
            maps.Add(NoiseMapGenerator.GeneratePerlinNM((xSize + 1) * xChunks, (zSize + 1) * zChunks, seed, noiseData));
        }

        Debug.Log("map 1:    " + maps[0].Length);

        float[,] combinedMap = CombineMaps(maps);

        //using (FileStream fs = File.Open("./scores.txt", FileMode.Create))
        //{
        //    StreamWriter sw = new StreamWriter(fs);
        //    for (int i = 0, z = 0; z <= zSize; z++)
        //    {
        //        for (int x = 0; x <= xSize; x++, i++)
        //        {
        //            sw.Write(combinedMap[x, z].ToString("0.00") + "  ");
        //        }
        //        sw.WriteLine("");
        //    }
            
        //}



        for (int zchunk = 0; zchunk < xChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                GameObject terrainChunk = new GameObject("map chunk");//Instantiate(new GameObject(), gameObject.transform);
                terrainChunk.transform.parent = gameObject.transform;
                terrainChunk.transform.localPosition = new Vector3(xSize * xchunk, 0, zSize * zchunk);
                
                Mesh mesh = terrainChunk.AddComponent<MeshFilter>().sharedMesh = new Mesh();
                MeshRenderer meshRenderer = terrainChunk.AddComponent<MeshRenderer>();
                meshRenderer.sharedMaterial = material;
                meshRenderer.material = material;
                MeshCollider meshCollider = terrainChunk.AddComponent<MeshCollider>();

                Vector3[] vertices = MakeChunk(xchunk,zchunk,combinedMap);

                mesh.vertices = vertices;

                FinalizeMesh(mesh, meshCollider);
            }
        }




        if (!DrawEachFrame_Debug)
        {
            Debug.Log("generate finish");
        }
    }

    private Vector3[] MakeChunk(int xchunk, int zchunk, float[,] combinedMap)
    {
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int i = 0, z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++, i++)
            {
                vertices[i] = new Vector3(x, combinedMap[(xSize+1)* xchunk + x, (zSize+1)*zchunk + z], z);
            }
        }

        Debug.Log("vertices: " + vertices.Length);
        return vertices;
    }

    private float[,] CombineMaps(List<float[,]> maps)
    {
        float[,] combined = new float[(xSize + 1) * xChunks, (zSize + 1) * zChunks];
        foreach (float[,] map in maps)
        {
            for (int z = 0; z <= zSize; z++)
            {
                for (int x = 0; x <= xSize; x++)
                {
                    combined[x, z] += map[x,z];
                }
            }
        }

        return combined;
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
			Debug.DrawRay(new Vector3(x,100f, z), Vector3.down * 10000, Color.magenta);
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