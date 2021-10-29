using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class is responsible for placing gameplay objects like hut, survivors, scenery, etc.. 
public class TerrainBuilder : MonoBehaviour
{


    [SerializeField] const int houseEdgeMinDistance = 50;

    public bool autoupdate = false;
    [SerializeField]private int seed = 0;
    [SerializeField] private int xSize = 200;
    [SerializeField] private int zSize = 200;
    [SerializeField] private int xChunks = 1;
    [SerializeField] private int zChunks = 1;
    [SerializeField]private NoiseData[] noisedata;
    public Material material;
    public GameObject HousePrefab;
    public GameObject SurvivorPrefab;
    public GameObject Water;


    private float[,,,] combinedMap;
    private MeshGenerator MG;
    private Vector3 housePosition;
    private Vector3 objectivePosition;
    private System.Random RNG;
    private Vector2 minmax;
    [SerializeField] TextureData TextureData;

    private List<GameObject> toCleanUp = new List<GameObject>();


    private void Awake()
    {

    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void MakeTerrain()
    {
        
        CleanupScene();
        

        this.gameObject.transform.position = new Vector3(-xSize * xChunks / 2, 0, -zSize * zChunks / 2);

        RNG = seed == 0 ? new System.Random() : new System.Random(seed);
        

        List<float[,,,]> maps = new List<float[,,,]>();
        foreach (NoiseData noiseData in noisedata)
        {
            if (noiseData.enabled)
            {
                maps.Add(NoiseMapGenerator.GeneratePerlinNM(xSize + 1, zSize + 1, seed, xChunks, zChunks, noiseData));
            }
        };

        combinedMap = NoiseMapGenerator.CombineMaps(maps, xSize + 1, zSize + 1, xChunks, zChunks);

        MG = new MeshGenerator(combinedMap, seed, xSize, zSize, xChunks, zChunks, noisedata, material, TextureData);

        minmax = MG.FindMaxHeightMult();


        float waterHeight = Mathf.Lerp(minmax.x, minmax.y, 0.3f);// -0.33f ;
        Water.transform.localScale = new Vector3(transform.localScale.x * xSize * xChunks, /*Mathf.Abs(minmax.x) - waterHeight*/ 1f, transform.localScale.z * xSize * zChunks);
        Water.transform.localPosition = new Vector3(-transform.localPosition.x, waterHeight * transform.localScale.y - (Water.transform.localScale.y / 2f), -transform.localPosition.z);


        //adjustments here


        //spawn objects


        //PlaceHouse();
        //PlaceObjective();

        MG.Generate().ForEach(x =>
        {
            toCleanUp.Add(x);
            x.transform.SetParent(transform,false);
        });



        TextureData.ApplyToMaterial(material);
        TextureData.UpdateMeshHeights(material, minmax.x * transform.localScale.y, minmax.y * transform.localScale.y);

    }

    private void createTerrainObjects()
    {

    }

    private void PlaceObjective()
    {
        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), xSize, zSize);

        Vector2 minmax = MG.FindMaxHeightMult();


        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        for (int i = xSize-50; i > 50; i--)
        {
            for (int j = zSize-50; j > 50; j--)
            {
                var invlerpy = Mathf.InverseLerp(minmax.x, minmax.y, combinedMap[i, j, chunk.x, chunk.y]);
                if (invlerpy > 0.6f && invlerpy < 0.8f)
                {
                    found = true;
                    y = combinedMap[i, j, chunk.x, chunk.y];
                    x = i;
                    z = j;
                }
            }
        }

        CreateSmallPlatform(combinedMap, x, z, (int)SurvivorPrefab.transform.localScale.x, (int)SurvivorPrefab.transform.localScale.z, 2, chunk, false);
        
        objectivePosition = new Vector3(x, combinedMap[x, z, chunk.x, chunk.y], z);

        if (!found)
        {
            Debug.Log("Bad Seed");
        }
        


        //TODO round edges of platform to go towards rest of terrain
        //something like y-(Min(y of x+1, y of z +1)/2)


        GameObject go = Instantiate(SurvivorPrefab, transform);
        go.name = "Survivor";
        go.transform.position = objectivePosition;
        go.transform.position += transform.localPosition;
        go.transform.position += new Vector3(go.transform.localScale.x / 2f, combinedMap[x, z, chunk.x, chunk.y] + go.transform.localScale.y / 2, go.transform.localScale.z / 2f);
        toCleanUp.Add(go);

    }

    private void PlaceHouse()
    {

        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), xSize, zSize);

        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        for (int j = houseEdgeMinDistance; j < zSize-houseEdgeMinDistance; j++)
        {
            for (int i = houseEdgeMinDistance; i < xSize-houseEdgeMinDistance; i++)
            {
                var invlerpy = Mathf.InverseLerp(minmax.x, minmax.y, combinedMap[i, j, chunk.x, chunk.y]);
                if (invlerpy > 0.6f && invlerpy < 0.8f)
                {
                    found = true;
                    y = combinedMap[i, j, chunk.x, chunk.y];
                    x = i;
                    z = j;
                }
            }
        }
        housePosition = new Vector3(x, combinedMap[x, z, chunk.x, chunk.y], z);

        if (!found)
        {
            Debug.Log("Bad Seed");
        }



        //platform for house
        CreateSmallPlatform(combinedMap, x, z, (int)HousePrefab.transform.localScale.x, (int)HousePrefab.transform.localScale.z, 4, chunk, false);


        //TODO round edges of platform to go towards rest of terrain
        //something like y-(Min(y of x+1, y of z +1)/2)


        GameObject go = Instantiate(HousePrefab, transform);
        go.name = "House";
        go.transform.position = housePosition;
        go.transform.position += transform.localPosition;
        go.transform.position += new Vector3(go.transform.localScale.x / 2f, combinedMap[x,z,chunk.x,chunk.y] + go.transform.localScale.y/2, go.transform.localScale.z / 2f);
        toCleanUp.Add(go);
    }


    private void CreateSmallPlatform(float[,,,] combinedMap, int x, int z, int xSize, int zSize, int extraSize, Vector2Int chunk, bool smooth = true)
    {


        for (int i = -extraSize; i < xSize + extraSize; i++)
        {
            for (int j = -extraSize; j < zSize + extraSize; j++)
            {
                int cx = x + i;
                int cz = z + j;

                try
                {
                    combinedMap[cx, cz, chunk.x, chunk.y] = housePosition.y;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    Debug.Log(cx);
                    Debug.Log(cz);
                    Debug.Log(x);
                    Debug.Log(z);
                    Debug.Log(i);
                    Debug.Log(i);
                    Debug.Log(extraSize);
                    throw;
                }
            }
        }


        if(smooth)
        {

        //smoothing TODO / WIP
        //for (int i = 0; i < extraSize; i++)
        //{
        //    for (int j = 0; j < extraSize; j++)
        //    {

        //        int cx, cz;
        //        bool addSize = false;
        //        int lerpval = Math.Max(Math.Abs(i), Math.Abs(j));

        //        //if(j >= extraSize/2)
        //        //{
        //        //    cx = x + j + xSize;
        //        //    addSize = true;
        //        //}
        //        //else{
        //        //    cx = x - j;
        //        //}
        //        //if(i >= extraSize/2)
        //        //{
        //        //    cz = x + i + zSize;
        //        //    addSize = true;
        //        //}
        //        //else
        //        //{
        //        //    cz = x - i;
        //        //}

        //        cx = x - j;
        //        cz = z - j;
                
        //        combinedMap[cx,cz,chunk.x,chunk.y] = Mathf.Lerp(combinedMap[])


        //        cx = x + j + xSize;
        //        cz = z + i + zSize;

        //        if(addSize)
        //        {
        //        }
        //        else
        //        {

        //        }

        //        //float ylerp = Mathf.Lerp([x, cz, chunk.x, chunk.y], combineMap[1,2,3,4], (lerpval/4f));

        //    }
        //}
        }
        else
        {
            //simple platform;
        }


    }


    private void CleanupScene()
    {
        toCleanUp.ForEach(x =>
        {
            if (Application.isEditor)
            {
                DestroyImmediate(x.gameObject, true);
            }
            else
            {
                Destroy(x);
            }
            
        });
    }


}