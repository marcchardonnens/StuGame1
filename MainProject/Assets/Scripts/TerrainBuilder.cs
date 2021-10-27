using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class is responsible for placing gameplay objects like hut, survivors, scenery, etc.. 
public class TerrainBuilder : MonoBehaviour
{
    public bool autoupdate = false;
    public int seed = 1;
    public GameObject HousePrefab;
    public GameObject SurvivorPrefab;
    
    
    
    private MeshGenerator MG;
    private Vector3 housePosition;
    private Vector3 objectivePosition;
    private System.Random RNG;

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

        RNG = seed == 0 ? new System.Random() : new System.Random(seed);
        MG = gameObject.GetComponent<MeshGenerator>();
        MG.seed = seed;

        List<float[,,,]> maps = new List<float[,,,]>();
        foreach (NoiseData noiseData in MG.noisedata)
        {
            if (noiseData.enabled)
            {
                maps.Add(NoiseMapGenerator.GeneratePerlinNM(MG.xSize + 1, MG.zSize + 1, MG.seed, MG.xChunks, MG.zChunks, noiseData));
            }
        };

        float[,,,] combinedMap = NoiseMapGenerator.CombineMaps(maps, MG.xSize + 1, MG.zSize + 1, MG.xChunks, MG.zChunks);


        //Debug.DrawRay()
        //adjustments here




        //spawn objects


        PlaceHouse(combinedMap);



        PlaceObjective(combinedMap);






        MG.Generate(combinedMap);

    }

    private void PlaceObjective(float[,,,] combinedMap)
    {
        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), MG.xSize, MG.zSize);

        Vector2 minmax = MG.FindMaxHeightMult(combinedMap);


        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        for (int i = MG.xSize-50; i > 50; i--)
        {
            for (int j = MG.zSize-50; j > 50; j--)
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
        objectivePosition = new Vector3(x, combinedMap[x, z, chunk.x, chunk.y], z);

        if (!found)
        {
            Debug.Log("Bad Seed");
        }



        //platform for house
        for (int i = -1; i < SurvivorPrefab.transform.localScale.x + 1; i++)
        {
            for (int j = -1; j < SurvivorPrefab.transform.localScale.z + 1; j++)
            {
                int cx = x + i;
                int cz = z + j;

                try
                {
                    combinedMap[cx, cz, chunk.x, chunk.y] = objectivePosition.y;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
        }

        //TODO round edges of platform to go towards rest of terrain
        //something like y-(Min(y of x+1, y of z +1)/2)


        GameObject go = Instantiate(SurvivorPrefab, transform);
        go.name = "Survivor";
        go.transform.position = objectivePosition;
        go.transform.position += transform.localPosition;
        go.transform.position += new Vector3(go.transform.localScale.x / 2f, go.transform.localPosition.y - go.transform.localScale.y / 2f, go.transform.localScale.z / 2f);

    }

    private void PlaceHouse(float[,,,] combinedMap)
    {

        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), MG.xSize, MG.zSize);

        Vector2 minmax = MG.FindMaxHeightMult(combinedMap);


        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        for (int j = 50; j < MG.zSize-50; j++)
        {
            for (int i = 50; i < MG.xSize-50; i++)
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
        for (int i = -4; i < HousePrefab.transform.localScale.x + 4; i++)
        {
            for (int j = -4; j < HousePrefab.transform.localScale.z + 4; j++)
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
                    throw;
                }
            }
        }

        //TODO round edges of platform to go towards rest of terrain
        //something like y-(Min(y of x+1, y of z +1)/2)


        GameObject go = Instantiate(HousePrefab, transform);
        go.name = "House";
        go.transform.position = housePosition;
        go.transform.position += transform.localPosition;
        go.transform.position += new Vector3(go.transform.localScale.x / 2f, go.transform.localPosition.y - go.transform.localScale.y / 2f, go.transform.localScale.z / 2f);

    }


    private void CleanupScene()
    {

        int count = transform.childCount;
        List<Transform> children = new List<Transform>();
        for (int i = 0; i < count; i++)
        {
            //TODO better way of doing this
            //right now this is so water/other objects dont get deleted.
            //deleting them and re-instantiating might be better or necessary, but this is easier for testing
            
            children.Add(transform.GetChild(i));
            
        }
        
        foreach (GameObject child in gameObject.scene.GetRootGameObjects())
        {
            if(child.name == "House" || child.name == "Survivor")
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
        }

        foreach (Transform child in children)
        {
            if (child.name == "House" || child.name == "Survivor")
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
        }


    }
}
