using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

//this class is responsible for placing gameplay objects like hut, survivors, scenery, etc.. 
public class TerrainBuilder : MonoBehaviour
{

    

    public bool autoupdate = false;
    [SerializeField] private int seed = 0;
    [SerializeField] private const int xSize = 200;
    [SerializeField] private const int zSize = 200;
    [SerializeField] private const int xChunks = 1;
    [SerializeField] private const int zChunks = 1;
    [SerializeField] private float yAdjustment = 0f;
    [SerializeField]private NoiseData[] noisedata;
    public Material material;
    public GameObject HousePrefab;
    public GameObject SurvivorPrefab;
    public GameObject Water;
    [SerializeField] private bool spawnTrees = true;
    public RandomChoice[] TreePrefabs;
    [SerializeField] private bool spawnRocks = true;
    public RandomChoice[] RockPrefabs;
    [SerializeField] TextureData TextureData;


    private float[,,,] combinedMap;
    private MeshGenerator MG;
    private Vector3 housePosition;
    private Vector3 objectivePosition;
    private System.Random RNG;
    private Vector2 minmax;
    private const float groundlevel = -7.5f;

    private List<GameObject> toCleanUp = new List<GameObject>();


    private void Awake()
    {
        MakeTerrain();
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
        

        this.gameObject.transform.position = new Vector3(-xSize * xChunks / 2, yAdjustment, -zSize * zChunks / 2);

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
        //MG.OnComplete += SpawnTrees;

        //terrain adjustments here

        MG.Generate(false).ForEach(x =>
        {
            toCleanUp.Add(x);
            x.transform.SetParent(transform, false);
        });
        //minmax = MG.FindActualMinMax();
        minmax = MG.CalcPotentialMinMax();


        float waterHeight = Mathf.Lerp(minmax.x, minmax.y, 0.3f);// -0.33f ;
        Water.transform.localScale = new Vector3(transform.localScale.x * xSize * xChunks, /*Mathf.Abs(minmaxActual.x) - waterHeight*/ 1f, transform.localScale.z * xSize * zChunks);
        Water.transform.localPosition = new Vector3(-transform.localPosition.x, waterHeight * transform.localScale.y - (Water.transform.localScale.y / 2f), -transform.localPosition.z);





        //spawn objects

        SpawnTrees();
        SpawnRocks();

        PlaceHouse();
        PlaceObjective();

        TextureData.ApplyToMaterial(material);
        TextureData.UpdateMeshHeights(material, minmax.x * transform.localScale.y, minmax.y * transform.localScale.y);






    }

    private IEnumerator WaitForMesh()
    {
        //yield return new WaitUntil(() =>
        //{
        //    RaycastHit hit;
        //    if (Physics.Raycast(new Vector3(0, 100f, 0), Vector3.down, out hit))
        //    {
        //        if (hit.point.y != 0)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //});

        yield return new WaitForFixedUpdate();

        //SpawnTrees();
    }

    private void createTerrainObjects()
    {

    }

    private void PlaceObjective()
    {
        const int objectiveEdgeDistance = 50;
        const float margin = 1f;
        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), xSize, zSize);

        Vector2 minmax = MG.FindActualMinMax();


        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        
        for (int i = xSize-objectiveEdgeDistance; i > objectiveEdgeDistance; i--)
        {
            for (int j = zSize-objectiveEdgeDistance; j > objectiveEdgeDistance; j--)
            {
                y = combinedMap[i, j, chunk.x, chunk.y];
                if (IsAtGroundLevel(y))
                {
                    found = true;
                    x = i;
                    z = j;
                    objectivePosition = new Vector3(x, groundlevel, z);
                    i = 0;
                    j = 0;
                    break;
                }
            }
        }

        //CreateSmallPlatform(combinedMap, x, z, (int)SurvivorPrefab.transform.localScale.x, (int)SurvivorPrefab.transform.localScale.z, 2, chunk, false);

        if (!found)
        {
            Debug.Log("Bad Seed");
        }
        


        //TODO round edges of platform to go towards rest of terrain
        //something like y-(Min(y of x+1, y of z +1)/2)


        GameObject go = Instantiate(SurvivorPrefab, transform);
        go.name = "Survivor";
        go.transform.localPosition = objectivePosition;
        //go.transform.position += transform.localPosition;
        //go.transform.position += new Vector3(go.transform.localScale.x / 2f, combinedMap[x, z, chunk.x, chunk.y] + go.transform.localScale.y / 2, go.transform.localScale.z / 2f);
        toCleanUp.Add(go);

    }

    private void PlaceHouse()
    {
        const int houseEdgeMinDistance = 50;
        const float margin = 1f;
        const int zextra = 8;
        const int xextra = 5;


        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), xSize, zSize);

        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        for (int j = houseEdgeMinDistance; j < zSize-houseEdgeMinDistance; j++)
        {
            for (int i = houseEdgeMinDistance; i < xSize-houseEdgeMinDistance; i++)
            {
                y = combinedMap[i, j, chunk.x, chunk.y];
                if(IsAtGroundLevel(y))
                {
                    found = true;
                    x = i;
                    z = j;

                    housePosition = new Vector3(x, groundlevel, z);
                    i = xSize;
                    j = zSize;
                    break;
                }
            }
        }

        if (!found)
        {
            Debug.Log("Bad Seed");
        }



        //platform for house
        //CreateSmallPlatform(combinedMap, x, z, (int)HousePrefab.transform.localScale.x, (int)HousePrefab.transform.localScale.z, 4, chunk, false);


        //TODO round edges of platform to go towards rest of terrain
        //something like y-(Min(y of x+1, y of z +1)/2)


        GameObject go = Instantiate(HousePrefab, transform);
        go.name = "House";
        go.transform.localPosition = housePosition;
        //go.transform.localPosition += transform.localPosition;
        //go.transform.localPosition += new Vector3(go.transform.localScale.x / 2f, combinedMap[x,z,chunk.x,chunk.y] + go.transform.localScale.y/2, go.transform.localScale.z / 2f);
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

    private void SpawnTrees()
    {
        const int treeDistance = 5;
        const int edgeRadius = 10;
        
        
        if (TreePrefabs.Length <= 0 || !spawnTrees)
        {
            return;
        }
        
        
        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z < xSize; z+=treeDistance)
                {
                    for (int x = 0; x < zSize; x+=treeDistance)
                    {
                        if (IsAtGroundLevel(combinedMap[x, z, xchunk, zchunk]))
                        {
                            //if (PointIsNotNearEdge(x,z))
                            if(PointNotNearEdge(x,z))
                            {
                                float cx = x + (float)RNG.NextDouble();
                                float cz = z + (float)RNG.NextDouble();
                            
                                GameObject go = RandomChoice.Choose(TreePrefabs, RNG);

                                go = Instantiate(go, transform);
                                go.name =  ("tree " + x + " " + z);
                                go.transform.localPosition = new Vector3(cx, groundlevel, cz);
                                go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z);
                                toCleanUp.Add(go);
                            }
                        }
                    }
                }
            }
        }
    }

    private void SpawnRocks()
    {
        const int edgeRadius = 1;
        const int size = (xSize + zSize) * 4;
        if (RockPrefabs.Length <= 0 || !spawnRocks)
        {
            return;
        }

        for (int i = 0; i < size; i++)
        {
            float x = (float)RNG.NextDouble() * xSize;
            float z = (float)RNG.NextDouble() * zSize;

            if (PointNotNearEdge((int)x, (int)z, edgeRadius))
            {
                GameObject go = RandomChoice.Choose(RockPrefabs, RNG);

                go = Instantiate(go, transform);
                go.name = ("Rock " + x + " " + z);
                go.transform.localPosition = new Vector3(x, groundlevel, z);
                go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z);
                toCleanUp.Add(go);
            }
        }
        
    }

    private bool IsAtGroundLevel(float y, float margin = 1f)
    {
        return y < groundlevel + margin && y > groundlevel - margin;
    }


    private bool PointNotNearEdge(int x, int z, int dist = 10)
    {
        if (x + dist > xSize || z + dist > zSize || x < dist || z < dist)
        {
            return false;
        }
        else if (!(IsAtGroundLevel(combinedMap[x + dist, z + dist, 0, 0]) &&
                 IsAtGroundLevel(combinedMap[x - dist, z + dist, 0, 0]) &&
                 IsAtGroundLevel(combinedMap[x + dist, z - dist, 0, 0]) &&
                 IsAtGroundLevel(combinedMap[x - dist, z - dist, 0, 0])))
        {
            return false;
        }

        return true;



    }


    //better but raycast doesnt seem to work
    private bool PointIsNotNearEdge(float x, float z, float dist = 10f, float height = groundlevel, float margin = 1f, int layer = 1 << 6)
    {
        for (int i = 0; i < 360; i+= 30)
        {
            float cx = x + (float)Math.Sin(i) * dist;
            float cz = z + (float)Math.Cos(i) * dist;
            RaycastHit hit;
            if (Physics.Raycast(new Vector3(cx, 100f, cz), Vector3.down, out hit, 2000f))
            {
                //Debug.DrawRay(new Vector3(cx,100f,cz), Vector3.down,Color.white, 1f, true);
                if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        return true;


        #region rectangular

        

        //RaycastHit hit;

        //if (Physics.Raycast(new Vector3(x + dist, 100f, z), Vector3.down, out hit, 200f, layer))
        //{
        //    if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return false;
        //}
        //if(Physics.Raycast(new Vector3(x - dist, 100f, z), Vector3.down, out hit, 200f, layer))
        //{
        //    if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return false;
        //}
        //if (Physics.Raycast(new Vector3(x , 100f, z - dist), Vector3.down, out hit, 200f, layer))
        //{
        //    if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return false;
        //}
        //if (Physics.Raycast(new Vector3(x , 100f, z + dist), Vector3.down, out hit, 200f, layer))
        //{
        //    if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return false;
        //}
        //if (Physics.Raycast(new Vector3(x + dist, 100f, z + dist), Vector3.down, out hit, 200f, layer))
        //{
        //    if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return false;
        //}
        //if (Physics.Raycast(new Vector3(x - dist, 100f, z - dist), Vector3.down, out hit, 200f, layer))
        //{
        //    if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return false;
        //}
        //if (Physics.Raycast(new Vector3(x + dist, 100f, z - dist), Vector3.down, out hit, 200f, layer))
        //{
        //    if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return false;
        //}
        //if (Physics.Raycast(new Vector3(x - dist, 100f, z + dist), Vector3.down, out hit, 200f, layer))
        //{
        //    if (!(Math.Abs(height) + margin < hit.point.y && hit.point.y < Math.Abs(height)))
        //    {
        //        return false;
        //    }
        //}
        //else
        //{
        //    return false;
        //}

        //return true;
        #endregion

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