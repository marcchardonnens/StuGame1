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
    public bool autoupdate = false;

    public bool IsHubMode = false;
    public bool DoCleanupScene = true;
    public int HubXOffset = 1000;
    public Vector3 HubHouseLocalPos = new Vector3(0, 0, 0);
    public Vector3 PlayerHubEnterLocalPos;

    public Vector3 PlayerHubWakeUpLocalPos;

    public bool finished = false;
    [SerializeField] private int Seed = 0;
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




    [SerializeField] private NoiseData[] noisedata;
    public Material material;
    public GameObject[] HousePrefab;
    public Vector3 PlayerSpawnOutsideHouseOffsetPos = new Vector3(1f, 8f, -6f);
    public Vector3 PlayerSpawnOutsideHouseRotationEuler = new Vector3(0f, 180f, 0f);
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
    public int PowerupDensity = 50;
    public float MushroomOverallScale = 2f;
    [SerializeField] private bool spawnPowerups = true;
    public RandomChoice[] PowerupPrefabs;
    public GameObject[] SideQuestPrefabs;
    public GameObject ResourceTreePrefab;
    public GameObject BossAreaLight;

    [SerializeField] TextureData TextureData;


    private float[,,,] combinedMap;
    private MeshGenerator MG;
    public Vector3 houseGlobalPosition;
    private Vector3 objectivePosition;
    public Vector3 obejctiveGlobalPosition;
    private System.Random RNG;
    private Vector2 minmax;
    private const float groundlevel = 0;// -7.5f;
    private int xSize;
    private int zSize;


    //these are for grouping objects together
    //to hopefully prevent spamming the object viewer
    private GameObject Scenery;
    private GameObject Ground;
    private GameObject Terrain;

    private List<Vector3> ResourceTreesPositions = new List<Vector3>();
    private List<Vector3> SideQuestPositions = new List<Vector3>();

    private List<GameObject> toCleanUp = new List<GameObject>();
    private List<GameObject> toCleanUpHub = new List<GameObject>();


    public void MakeHub()
    {
        if (!IsHubMode)
        {
            Debug.LogError("Making Hub not in Hub mode!");
            return;
        }

        if (DoCleanupScene)
        {
            CleanupHub();
        }

        xSize = XSize;
        zSize = ZSize;

        Scenery = new GameObject("Scenery");
        Scenery.transform.SetParent(transform, false);
        SceneVisibilityManager.instance.DisablePicking(Scenery, true);

        Ground = new GameObject("Terrain");
        Ground.transform.SetParent(transform, false);

        Terrain = new GameObject("HubTerrain");
        Terrain.transform.SetParent(transform, false);
        SceneVisibilityManager.instance.DisablePicking(Terrain, true);

        Terrain.transform.position += new Vector3(HubXOffset, 0, 0);
        Scenery.transform.SetParent(Terrain.transform, false);
        Ground.transform.SetParent(Terrain.transform, false);



        RNG = Seed == 0 ? new System.Random() : new System.Random(Seed);


        List<float[,,,]> maps = new List<float[,,,]>();
        foreach (NoiseData noiseData in noisedata)
        {
            if (noiseData.enabled)
            {
                maps.Add(NoiseMapGenerator.GeneratePerlinNM(xSize + 1, zSize + 1, Seed, xChunks, zChunks, noiseData));
            }
        };

        combinedMap = NoiseMapGenerator.CombineMaps(maps, xSize + 1, zSize + 1, xChunks, zChunks);

        MG = new MeshGenerator(combinedMap, Seed, xSize, zSize, xChunks, zChunks, noisedata, material, TextureData);

        //terrain adjustments here

        xSize = (int)(XSize * TerrainScale);
        zSize = (int)(ZSize * TerrainScale);


        Terrain.transform.position += new Vector3(-xSize / 2f, 0, -zSize / 2f);




        MG.Generate(false).ForEach(x =>
        {
            toCleanUp.Add(x);
            x.transform.SetParent(Ground.transform, false);
            x.transform.localScale = new Vector3(transform.localScale.x * TerrainScale, transform.localScale.y * TerrainScale * TerrainYModifier, transform.localScale.z * TerrainScale);
            x.transform.localPosition += new Vector3(0, -groundlevel * (TerrainScale - 1) * TerrainYModifier, 0);
        });



        minmax = MG.CalcPotentialMinMax(TerrainScale);


        ClearHubHouseArea();

        SpawnTrees(false);
        SpawnRocks(false);

        TextureData.ApplyToMaterial(material);
        TextureData.UpdateMeshHeights(material, minmax.x * transform.localScale.y, minmax.y * transform.localScale.y);

        finished = true;
    }

    private void ClearHubHouseArea()
    {
        houseGlobalPosition = new Vector3(HubXOffset, 0, 0) + HubHouseLocalPos;
    }

    public void MakeTerrain()
    {
        if (IsHubMode)
        {
            Debug.LogError("Make Terrain is in Hub Mode!");
            return;
        }

        Debug.Log(Time.time);

        if (DoCleanupScene)
        {
            CleanupGameplay();
        }
        xSize = XSize;
        zSize = ZSize;

        Scenery = new GameObject("Scenery");
        Scenery.transform.SetParent(transform, false);
        SceneVisibilityManager.instance.DisablePicking(Scenery, true);

        Ground = new GameObject("Ground");
        Ground.transform.SetParent(transform, false);

        Terrain = new GameObject("Terrain");
        Terrain.transform.SetParent(transform, false);
        // SceneVisibilityManager.instance.DisablePicking(Terrain, true);

        Scenery.transform.SetParent(Terrain.transform, false);
        Ground.transform.SetParent(Terrain.transform, false);


        RNG = Seed == 0 ? new System.Random() : new System.Random(Seed);


        List<float[,,,]> maps = new List<float[,,,]>();
        foreach (NoiseData noiseData in noisedata)
        {
            if (noiseData.enabled)
            {
                maps.Add(NoiseMapGenerator.GeneratePerlinNM(xSize + 1, zSize + 1, Seed, xChunks, zChunks, noiseData));
            }
        };

        combinedMap = NoiseMapGenerator.CombineMaps(maps, xSize + 1, zSize + 1, xChunks, zChunks);

        MG = new MeshGenerator(combinedMap, Seed, xSize, zSize, xChunks, zChunks, noisedata, material, TextureData);

        //terrain adjustments here

        xSize = (int)(XSize * TerrainScale);
        zSize = (int)(ZSize * TerrainScale);

        Terrain.transform.position += new Vector3(-xSize / 2f, 0, -zSize / 2f);


        MG.Generate(false).ForEach(x =>
        {
            toCleanUp.Add(x);
            x.transform.SetParent(Ground.transform, false);
            x.transform.localScale = new Vector3(transform.localScale.x * TerrainScale, transform.localScale.y * TerrainScale * TerrainYModifier, transform.localScale.z * TerrainScale);
            x.transform.localPosition += new Vector3(0, -groundlevel * (TerrainScale - 1) * TerrainYModifier, 0);
        });

        //minmax = MG.FindActualMinMax();
        minmax = MG.CalcPotentialMinMax(TerrainScale);

        float waterHeight = Mathf.Lerp(minmax.x, minmax.y, 0.3f);// -0.33f ;
        Water.transform.localScale = new Vector3(transform.localScale.x * xSize * xChunks * TerrainScale, /*Mathf.Abs(minmaxActual.x) - waterHeight*/ 1f, transform.localScale.z * xSize * zChunks * TerrainScale);
        Water.transform.localPosition = new Vector3(-transform.localPosition.x, waterHeight * transform.localScale.y - (Water.transform.localScale.y / 2f), -transform.localPosition.z);

        PlaceHouse();
        PlaceObjective(); //including boss arena
        PlaceResourceTrees();
        PlaceSideObjectives();

        SpawnTrees(true);
        SpawnRocks(true);
        SpawnPowerups();

        TextureData.ApplyToMaterial(material);
        TextureData.UpdateMeshHeights(material, minmax.x * transform.localScale.y, minmax.y * transform.localScale.y);

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
                    GameObject go = Instantiate(SideQuestPrefabs[i], Terrain.transform);
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
                    GameObject go = Instantiate(ResourceTreePrefab, Terrain.transform);
                    ResourceTreesPositions.Add(go.transform.position);
                    go.transform.localPosition = new Vector3(x, groundlevel, z);
                    go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z);
                    go.transform.localScale *= TreeOverallScale;
                    toCleanUp.Add(go);
                }
            }
        }
    }

    // includes boss area
    private void PlaceObjective()
    {
        const int objectiveEdgeDistance = 50;
        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), XSize, ZSize);


        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;

        for (int i = XSize - objectiveEdgeDistance; i > objectiveEdgeDistance; i--)
        {
            for (int j = ZSize - objectiveEdgeDistance; j > objectiveEdgeDistance; j--)
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
            Debug.LogError("Bad Seed for objective");
        }


        GameObject go = Instantiate(SurvivorPrefab, Terrain.transform);
        go.name = "Survivor";
        go.transform.localPosition = objectivePosition;
        obejctiveGlobalPosition = go.transform.position;
        toCleanUp.Add(go);

        GameObject bosslight = Instantiate(BossAreaLight, Terrain.transform);
        bosslight.transform.position = go.transform.position;
        bosslight.transform.position += new Vector3(0, 100f, 0);
        toCleanUp.Add(bosslight);

    }

    private void PlaceHouse()
    {
        const int houseEdgeMinDistance = 50;

        Vector3 housePosition = Vector3.zero;
        Vector2Int chunk = NoiseMapGenerator.FindChunk(new Vector2Int(1, 1), XSize, XSize);

        bool found = false;
        float y = 0f;
        int x = 0;
        int z = 0;
        for (int j = houseEdgeMinDistance; j < XSize - houseEdgeMinDistance; j++)
        {
            for (int i = houseEdgeMinDistance; i < XSize - houseEdgeMinDistance; i++)
            {
                y = combinedMap[i, j, chunk.x, chunk.y];
                if (IsAtGroundLevel(y))
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


        GameObject go = Instantiate(HousePrefab[GameManager.ProfileData.HouseUpgradeLevel], Terrain.transform);
        go.name = "House";
        go.transform.localPosition = housePosition;
        houseGlobalPosition = go.transform.position;

        SceneVisibilityManager.instance.DisablePicking(go, true);


        toCleanUp.Add(go);
    }

    public void SpawnTrees(bool cleanup)
    {
        int treeDistance = 100 / TreeDensity;
        const float heightvariance = 0.15f;
        const float thicknessvariance = 0.15f;

        if (TreePrefabs.Length <= 0 || !spawnTrees)
        {
            return;
        }


        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z < XSize; z += treeDistance)
                {
                    for (int x = 0; x < ZSize; x += treeDistance)
                    {
                        if (IsAtGroundLevel(combinedMap[x, z, xchunk, zchunk]))
                        {
                            //if (PointIsNotNearEdge(x,z))
                            if (PointNotNearEdge(x, z, 5))
                            {
                                float cx = x + (float)RNG.NextDouble() * treeDistance;
                                float cz = z + (float)RNG.NextDouble() * treeDistance;

                                cx *= TerrainScale;
                                cz *= TerrainScale;

                                if (CheckImportantObjectBlocking(new Vector3(cx, groundlevel, cz)))
                                {
                                    continue;
                                }


                                GameObject go = RandomChoice.Choose(TreePrefabs, RNG);

                                go = Instantiate(go, Scenery.transform);
                                go.name += (" " + x + " " + z);
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

                            }
                        }
                    }
                }
            }
        }
    }

    private bool CheckImportantObjectBlocking(Vector3 pos)
    {
        pos += Terrain.transform.position;
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
            if (dist < ResourceTreesClearRadius)
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

    public void SpawnRocks(bool cleanup)
    {
        const int edgeRadius = 1;
        const float sizeVariance = 0.15f; //percent
        int size = (xSize + zSize) * RocksDensity / 100;
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

                if (cleanup)
                {
                    toCleanUp.Add(go);
                }
            }
        }

    }

    public void SpawnPowerups()
    {
        const int edgeRadius = 15;
        //const float sizeVariance = 0.15f; //percent

        if (PowerupPrefabs.Length <= 0 || !spawnPowerups)
        {
            return;
        }


        int spawDistance = 100 / PowerupDensity;

        if (PowerupPrefabs.Length <= 0 || !spawnPowerups)
        {
            return;
        }

        for (int zchunk = 0; zchunk < zChunks; zchunk++)
        {
            for (int xchunk = 0; xchunk < xChunks; xchunk++)
            {
                for (int z = 0; z < XSize; z += spawDistance)
                {
                    for (int x = 0; x < ZSize; x += spawDistance)
                    {
                        if (IsAtGroundLevel(combinedMap[x, z, xchunk, zchunk]))
                        {
                            if (PointNotNearEdge(x, z, edgeRadius))
                            {
                                float rx = (float)RNG.NextDouble();
                                float rz = (float)RNG.NextDouble();

                                float cx = x + rx * spawDistance;
                                float cz = z + rz * spawDistance;



                                cx *= TerrainScale;
                                cz *= TerrainScale;

                                if (CheckImportantObjectBlocking(new Vector3(cx, groundlevel, cz)))
                                {
                                    continue;
                                }

                                float y = Mathf.Lerp(combinedMap[x, z, xchunk, zchunk], combinedMap[x + 1, z + 1, xchunk, zchunk], (rx + rz) / 2f);
                                GameObject go = RandomChoice.Choose(PowerupPrefabs, RNG);

                                go = Instantiate(go, Terrain.transform);
                                go.name += (" " + cx + " " + cz);
                                go.transform.localPosition = new Vector3(cx, groundlevel + y, cz);
                                go.transform.localEulerAngles = new Vector3(go.transform.localEulerAngles.x, (float)RNG.NextDouble() * 360f, go.transform.localEulerAngles.z);
                                go.transform.localScale *= MushroomOverallScale;
                                toCleanUp.Add(go);
                            }
                        }
                    }
                }
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
    private bool PointIsNotNearEdge(float x, float z, float dist = 10f, float margin = 1f, int layer = 1 << 6)
    {
        float height = groundlevel;
        for (int i = 0; i < 360; i += 30)
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


    private void CleanupHub()
    {
        if (Application.isEditor)
        {
            DestroyImmediate(Terrain, true);
        }
        else
        {
            Destroy(Terrain);
        }
    }

    private void CleanupGameplay()
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
        if (Application.isEditor)
        {
            DestroyImmediate(Terrain, true);
        }
        else
        {
            Destroy(Terrain);
        }
    }
}