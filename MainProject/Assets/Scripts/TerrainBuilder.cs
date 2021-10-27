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


        int x = (new System.Random().Next(20, (MG.xSize-20))) - (int)transform.localScale.x;
        int z = (new System.Random().Next(20, (MG.zSize-20))) - (int)transform.localScale.z;

        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(x, z), MG.xSize, MG.zSize);

        housePosition = new Vector3(x, combinedMap[x, z,chunk.x, chunk.y], z);


        //platform for house
        for (int i = -4; i < HousePrefab.transform.localScale.x +4; i++)
        {
            for (int j = -4; j < HousePrefab.transform.localScale.z + 4; j++)
            {
                int cx = x + i;
                int cz = z + j;

                combinedMap[cx, cz, chunk.x, chunk.y] = housePosition.y;
            }
        }

        //TODO round edges of platform to go towards rest of terrain
        //something like y-(Min(y of x+1, y of z +1)/2)


        GameObject go = Instantiate(HousePrefab, transform);
        go.name = "House";
        go.transform.position = housePosition;
        go.transform.position += transform.localPosition;
        go.transform.position += new Vector3(go.transform.localScale.x / 2f, go.transform.localPosition.y - go.transform.localScale.y/2f, go.transform.localScale.z / 2f);

        housePosition = Vector3.zero;

        








        MG.Generate(combinedMap);

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
            if(child.name == "House")
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
            if (child.name == "House")
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
