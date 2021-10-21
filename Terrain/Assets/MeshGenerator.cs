using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField]private bool DrawEachFrame_Debug = false;

    [SerializeField]private int xSize = 20;
	[SerializeField]private int zSize = 20;

    [SerializeField] private float edge = 20f;
	 
	[SerializeField]private float XNoiseMult = 0.3f;
	[SerializeField]private float ZNoiseMult = 0.3f;
	[SerializeField]private float OverallMult = 2f;

    public GameObject TreePrefab;
    [SerializeField] private float TreeEdge = 20f;

    private Mesh mesh;

	

	private void Awake()
    {
		mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		//GetComponent<MeshCollider>().sharedMesh = mesh;
		this.gameObject.transform.position += new Vector3(-xSize/2,0,-zSize/2);
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

    // Update is called once per frame
    void Update()
    {

        Debug.DrawRay(Vector3.zero, Vector3.up * 1000f, Color.magenta);
	}




	private void Generate()
	{
		GetComponent<MeshFilter>().mesh = mesh = new Mesh();


        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
		for (int i = 0, z = 0; z <= zSize; z++)
		{
			for (int x = 0; x <= xSize; x++, i++)
			{
				float y = Mathf.PerlinNoise(x*XNoiseMult, z*ZNoiseMult) * OverallMult;
				//y += Mathf.PerlinNoise(x*2f, z*2f) * 5f; //experiment second level of noise
				LowerEdgeVertices(x,ref y, z);

				//moutain mental notes
				//select mounsize x elements
				//select mounsize z elements
				//create new array and modify y values

				vertices[i] = new Vector3(x, y, z);
			}
		}
		mesh.vertices = vertices;

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
		GetComponent<MeshCollider>().sharedMesh = mesh;
	}

	private void CreateSingleMountain()
    {

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
