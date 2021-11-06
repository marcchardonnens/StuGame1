using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.AI;
using Unity.AI.Navigation;

//this class is responsible for placing gameplay objects like hut, survivors, scenery, etc.. 
public class TerrainBuilder : MonoBehaviour
{
    private const String CLEANUPTAG = "Cleanup";

    public bool autoupdate = false;

    public bool IsHubScene = false;

    public bool finished = false;
    [SerializeField] private int seed = 0;
    [SerializeField] private const int XSize = 200;
    [SerializeField] private const int ZSize = 200;
    [SerializeField] private const int xChunks = 1;
    [SerializeField] private const int zChunks = 1;
    [SerializeField] private float yAdjustment = 0f;
    [SerializeField] private int TerrainScale = 1;
    [SerializeField] private float TerrainYModifier = 2;

    public float HouseClearRadius = 20f;
    public float ObjectiveClearRadius = 100f;
    public float ResourceTreesClearRadius = 10f;
    public float SideQuestClearRadius = 5f;
    public int ResourceTrees = 10;


    public NavMeshSurface Surface;


    [SerializeField]private NoiseData[] noisedata;
    public Material material;
    public GameObject[] HousePrefab;
    public GameObject SurvivorPrefab;
    public GameObject Water;
    [SerializeField] private bool spawnTrees = true;
    public int TreeDensity = 1;
    public float TreeOverallScale = 1f;
    public RandomChoice[] TreePrefabs;
    [SerializeField] private bool spawnRocks = true;
    public int RocksDensity = 1;
    public float RocksOverallScale = 1f;
    public RandomChoice[] RockPrefabs;
    public int PowerupAmount = 50;
    [SerializeField] private bool spawnPowerups = true;
    public RandomChoice[] PowerupPrefabs;
    public GameObject[] SideQuestPrefabs;


    public GameObject ResourceTreePrefab;




    public GameObject BossAreaLight;






    [SerializeField] TextureData TextureData;
    

    private float[,,,] combinedMap;
    private MeshGenerator MG;
    private Vector3 housePosition;
    private Vector3 houseGlobalPosition;
    private Vector3 objectivePosition;
    public Vector3 obejctiveGlobalPosition;
    private System.Random RNG;
    private Vector2 minmax;
    private const float groundlevel = 0;// -7.5f;
    private int xSize;
    private int zSize;

    public Transform House;

    //these are for grouping objects together
    //to hopefully prevent spamming the object viewer
    private GameObject Scenery;
    private GameObject Terrain;

    private List<Vector3> ResourceTreesPositions = new List<Vector3>();
    private List<Vector3> SideQuestPositions = new List<Vector3>();

    private List<GameObject> toCleanUp = new List<GameObject>();
    

    private void Awake()
    {
        MakeTerrain();
    }

    public void MakeTerrain()
    {

        Debug.Log(Time.time);
        if(!IsHubScene)
        {
            CleanupScene();
        }
        else
        {
            TextureData.ApplyToMaterial(material);
            TextureData.UpdateMeshHeights(material, minmax.x * transform.localScale.y, minmax.y * transform.localScale.y);

            return;
        }


        xSize = XSize;
        zSize = ZSize;

        Scenery = new GameObject("Scenery");
        Scenery.tag = CLEANUPTAG;
        Scenery.transform.SetParent(transform,false);
        SceneVisibilityManager.instance.DisablePicking(Scenery, true);
        //Scenery.transform.localPosition += new Vector3(0, groundlevel, 0);
        
        

        Terrain = new GameObject("Terrain");
        Terrain.tag = CLEANUPTAG;
        Terrain.transform.SetParent(transform, false);
        //Terrain.transform.localPosition += new Vector3(0, groundlevel, 0);

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
            x.transform.SetParent(Terrain.transform, false);
            x.transform.localScale = new Vector3(transform.localScale.x *TerrainScale, transform.localScale.y * TerrainScale*TerrainYModifier, transform.localScale.z * TerrainScale);
            x.transform.localPosition += new Vector3(0, -groundlevel * (TerrainScale - 1), 0) * TerrainYModifier;
        });



        xSize = (int)(XSize * TerrainScale);
        zSize = (int)(ZSize * TerrainScale);
        


        //minmax = MG.FindActualMinMax();
        minmax = MG.CalcPotentialMinMax(TerrainScale);


        float waterHeight = Mathf.Lerp(minmax.x, minmax.y, 0.3f);// -0.33f ;
        Water.transform.localScale = new Vector3(transform.localScale.x * xSize * xChunks * TerrainScale, /*Mathf.Abs(minmaxActual.x) - waterHeight*/ 1f, transform.localScale.z * xSize * zChunks * TerrainScale);
        Water.transform.localPosition = new Vector3(-transform.localPosition.x, waterHeight * transform.localScale.y - (Water.transform.localScale.y / 2f), -transform.localPosition.z);





        //spawn objects
        if(!IsHubScene)
        {
            PlaceHouse();
            PlaceObjective(); //including boss arena
            PlaceResourceTrees();
            PlaceSideObjectives();

        SpawnPowerups();
        }

        SpawnTrees();
        SpawnRocks();


        TextureData.ApplyToMaterial(material);
        TextureData.UpdateMeshHeights(material, minmax.x * transform.localScale.y, minmax.y * transform.localScale.y);


        if (!IsHubScene)
        {
            MakeNavMesh();
        }


        finished = true;
        Debug.Log(Time.time);

    }

    private void PlaceSideObjectives()
    {
        const int edgedist = 1;
        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), XSize, ZSize);
        for (int i = 0; i < 3; i++)
        {
            float x = (float)RNG.NextDouble() * XSize;
            float z = (float)RNG.NextDouble() * ZSize;


            if (PointNotNearEdge((int)x, (int)z, edgedist))
            {
                float y = combinedMap[(int)x, (int)z, chunk.x, chunk.y];

                if (IsAtGroundLevel(y))
                {
                    x *= TerrainScale;
                    z *= TerrainScale;
                    GameObject go = Instantiate(SideQuestPrefabs[i], transform);
                    SideQuestPositions.Add(go.transform.position);
                    go.transform.localPosition = new Vector3(x, groundlevel, z);
                    go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z);
                    toCleanUp.Add(go);

                }
            }
        }
    
    }

    private void PlaceResourceTrees()
    {

        const int ResourceTreeEdgeDistance = 25;
        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), XSize, ZSize);
        float y = 0f;

        for (int i = 0; i < ResourceTrees; i++)
        {
            float x = (float)RNG.NextDouble() * XSize;
            float z = (float)RNG.NextDouble() * ZSize;


            if (PointNotNearEdge((int)x, (int)z, ResourceTreeEdgeDistance))
            {
                y = combinedMap[(int)x, (int)z, chunk.x, chunk.y];
                if (IsAtGroundLevel(y))
                {
                    x *= TerrainScale;
                    z *= TerrainScale;
                    GameObject go = Instantiate(ResourceTreePrefab, transform);
                    ResourceTreesPositions.Add(go.transform.position);
                    go.transform.localPosition = new Vector3(x, groundlevel, z);
                    go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z);
                    go.transform.localScale *= TreeOverallScale;
                    toCleanUp.Add(go);
                }
            }
        }



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


    /// <summary>
    /// !!!!!!!! includes boss arena
    /// </summary>
    private void PlaceObjective()
    {



        const int objectiveEdgeDistance = 50;
        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), XSize, ZSize);
        

        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        
        for (int i = XSize-objectiveEdgeDistance; i > objectiveEdgeDistance; i--)
        {
            for (int j = ZSize-objectiveEdgeDistance; j > objectiveEdgeDistance; j--)
            {
                y = combinedMap[i, j, chunk.x, chunk.y];
                if (IsAtGroundLevel(y))
                {
                    found = true;
                    x = i;
                    z = j;
                    objectivePosition = new Vector3(x * TerrainScale, groundlevel * TerrainYModifier, z * TerrainScale);
                    i = 0;
                    j = 0;
                    break;
                }
            }
        }

        if (!found)
        {
            Debug.Log("Bad Seed");
        }


        GameObject go = Instantiate(SurvivorPrefab, transform);
        go.name = "Survivor";
        go.transform.localPosition = objectivePosition;
        obejctiveGlobalPosition = go.transform.position;
        toCleanUp.Add(go);

        GameObject bosslight = Instantiate(BossAreaLight, transform);
        bosslight.transform.position = go.transform.position;
        bosslight.transform.position += new Vector3(0, 100f, 0);
        toCleanUp.Add(bosslight);

    }

    private void PlaceHouse()
    {
        const int houseEdgeMinDistance = 50;

        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), XSize, XSize);

        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        for (int j = houseEdgeMinDistance; j < XSize-houseEdgeMinDistance; j++)
        {
            for (int i = houseEdgeMinDistance; i < XSize-houseEdgeMinDistance; i++)
            {
                y = combinedMap[i, j, chunk.x, chunk.y];
                if(IsAtGroundLevel(y))
                {
                    found = true;
                    x = i;
                    z = j;

                    housePosition = new Vector3(x * TerrainScale, groundlevel * TerrainYModifier, z * TerrainScale);
                    i = XSize;
                    j = XSize;
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



        //RemoveObjectsInRadius(housePosition, 200f);

        GameObject go = Instantiate(HousePrefab[GameManager.ProfileData.HouseUpgradeLevel], transform);
        go.name = "House";
        go.transform.localPosition = housePosition;
        houseGlobalPosition = go.transform.position;
        House = go.transform;

        SceneVisibilityManager.instance.DisablePicking(go, true);


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

    public void SpawnTrees()
    {
        int treeDistance = (int) TreeDensity;
        const float heightvariance = 0.15f;
        const float thicknessvariance = 0.15f;
        bool resourceTreeNext = false;
        
        if (TreePrefabs.Length <= 0 || !spawnTrees)
        {
            return;
        }
        
        
        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z < XSize; z+=treeDistance)
                {
                    for (int x = 0; x < ZSize; x+=treeDistance)
                    {
                        if (IsAtGroundLevel(combinedMap[x, z, xchunk, zchunk]))
                        {
                            //if (PointIsNotNearEdge(x,z))
                            if(PointNotNearEdge(x,z, 5))
                            {
                                float cx = x + (float)RNG.NextDouble() * treeDistance;
                                float cz = z + (float)RNG.NextDouble() * treeDistance;

                                cx *= TerrainScale;
                                cz *= TerrainScale;

                                if(CheckImportantObjectBlocking(new Vector3(cx, groundlevel, cz)))
                                {
                                    continue;
                                }


                                GameObject go = RandomChoice.Choose(TreePrefabs, RNG);

                                go = Instantiate(go, Scenery.transform);
                                go.name +=  (" " + x + " " + z);
                                go.transform.localPosition = new Vector3(cx, groundlevel, cz);
                                go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z);
                                float heightScale = (float)RNG.NextDouble() * heightvariance * 2 - heightvariance;
                                float thickScale = (float)RNG.NextDouble() * thicknessvariance * 2 - thicknessvariance;
                                heightScale += 1f;
                                thickScale += 1f;
                                go.transform.localScale =
                                    new Vector3(go.transform.localScale.x * heightScale * thickScale,
                                        go.transform.localScale.y * heightScale,
                                        go.transform.localScale.z * heightScale * thickScale);
                                go.transform.localScale *= TreeOverallScale;

                                toCleanUp.Add(go);
                            }
                        }
                    }
                }
            }
        }
    }

    private bool CheckImportantObjectBlocking(Vector3 pos)
    {
        pos = new Vector3(pos.x - XSize / 2f, pos.y, pos.z - ZSize / 2f);
        float dist = Mathf.Abs(Vector3.Distance(houseGlobalPosition, pos));
        if (dist < HouseClearRadius)
        {
            return true;
        }
        dist = Mathf.Abs(Vector3.Distance(obejctiveGlobalPosition, pos));
        if (dist < ObjectiveClearRadius)
        {
            return true;
        }

        foreach (Vector3 treepos in ResourceTreesPositions)
        {
            dist = Mathf.Abs(Vector3.Distance(treepos, pos));
            if(dist < ResourceTreesClearRadius)
            {
                return true;
            }
        }

        foreach (Vector3 sidequests in SideQuestPositions)
        {
            dist = Mathf.Abs(Vector3.Distance(sidequests, pos));
            if (dist < SideQuestClearRadius)
            {
                return true;
            }
        }

        return false;
    }

    public void SpawnRocks()
    {
        const int edgeRadius = 1;
        const float sizeVariance = 0.15f; //percent
        int size = (xSize + zSize) * RocksDensity;
        if (RockPrefabs.Length <= 0 || !spawnRocks)
        {
            return;
        }

        for (int i = 0; i < size; i++)
        {
            float x = (float)RNG.NextDouble() * XSize;
            float z = (float)RNG.NextDouble() * ZSize;


            if (PointNotNearEdge((int)x, (int)z, edgeRadius))
            {
                x *= TerrainScale;
                z *= TerrainScale;
                GameObject go = RandomChoice.Choose(RockPrefabs, RNG);

                go = Instantiate(go, Scenery.transform);
                go.name += (" " + x + " " + z);
                go.transform.localPosition = new Vector3(x, groundlevel, z);
                go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x + (float)RNG.NextDouble() * 360f, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z + (float)RNG.NextDouble() * 360f);
                float scale = (float)RNG.NextDouble() * sizeVariance * 2 - sizeVariance;
                scale += 1f;
                go.transform.localScale *= scale;
                go.transform.localScale *= RocksOverallScale;
                toCleanUp.Add(go);
            }
        }
        
    }

    public void SpawnPowerups()
    {
        const int edgeRadius = 1;
        //const float sizeVariance = 0.15f; //percent
        
        if (PowerupPrefabs.Length <= 0 || !spawnPowerups)
        {
            return;
        }

        for (int i = 0; i < PowerupAmount; i++)
        {
            float x = (float)RNG.NextDouble() * XSize;
            float z = (float)RNG.NextDouble() * ZSize;


            if (PointNotNearEdge((int)x, (int)z, edgeRadius))
            {
                x *= TerrainScale;
                z *= TerrainScale;
                GameObject go = RandomChoice.Choose(PowerupPrefabs, RNG);

                go = Instantiate(go, Scenery.transform);
                go.name += (" " + x + " " + z);
                go.transform.localPosition = new Vector3(x, groundlevel, z);
                go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x + (float)RNG.NextDouble() * 360f, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z + (float)RNG.NextDouble() * 360f);
                toCleanUp.Add(go);
            }
        }
    }



    public void RemoveObjectsInRadius(Vector3 pos, float radius, int layermask = 1 << GameConstants.SCENERYLAYER)
    {

        Collider[] colliders = Physics.OverlapSphere(pos, radius, layermask);
        foreach (Collider collider in colliders)
        {
            Destroy(collider.gameObject);
        }
    }

    public void MoveObjectsInRadiusAway(Vector3 pos, float radius, int layermask = 1 << GameConstants.SCENERYLAYER)
    {

        Collider[] colliders = Physics.OverlapSphere(pos, radius, layermask);
        foreach (Collider collider in colliders)
        {
            collider.transform.position = (collider.transform.position - pos) * radius;
        }
    }


    private bool IsAtGroundLevel(float y, float margin = 1f)
    {
        return y < groundlevel + margin && y > groundlevel - margin;
    }


    private bool PointNotNearEdge(int x, int z, int dist = 10)
    {
        if (x + dist > XSize || z + dist > ZSize || x < dist || z < dist)
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
    private bool PointIsNotNearEdge(float x, float z, float dist = 10f,  float margin = 1f, int layer = 1 << 6)
    {
        float height = groundlevel;
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


    }




    private void MakeNavMesh()
    {
        if (IsHubScene)
        {
            return;
        }

        //HighMod.size = new Vector3(xSize, 75f, zSize);
        //HighMod.center = new Vector3(0, groundlevel + 1f + HighMod.size.y / 2f, 0);
        //LowMod.size = new Vector3(xSize, 75f, zSize);
        //LowMod.center = new Vector3(0, groundlevel - 1f - LowMod.size.y / 2f, 0);

        Surface.BuildNavMesh();

    }



    private void CleanupScene()
    {

        SideQuestPositions.Clear();
        ResourceTreesPositions.Clear();

        toCleanUp.ForEach(go =>
        {
            if (Application.isEditor)
            {
                DestroyImmediate(go.gameObject, true);
            }
            else
            {
                Destroy(go);
            }
            
        });

        GameObject[] cleanup = GameObject.FindGameObjectsWithTag(CLEANUPTAG);
        foreach (GameObject go in cleanup)
        {
            if (Application.isEditor)
            {
                DestroyImmediate(go.gameObject, true);
            }
            else
            {
                Destroy(go);
            }
        }
    }
    


}