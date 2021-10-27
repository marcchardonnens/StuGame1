using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//this class is responsible for placing gameplay objects like hut, survivors, scenery, etc.. 
public class TerrainBuilder : MonoBehaviour
{
    public bool autoupdate = false;
    public int seed = 1;
    public GameObject House;
    public GameObject Survivor;
    
    
    
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

        housePosition = new Vector3(RNG.Next(0, MG.xSize), 0, RNG.Next(0, MG.zSize));
        int layermask = 1 << 6;


        //float x = Random.Range(TreeEdge, xSize - TreeEdge) + transform.position.x;
        //float z = Random.Range(TreeEdge, zSize - TreeEdge) + transform.position.z;

        float x = (RNG.Next(0, MG.xSize) + transform.position.x) * transform.localScale.x;
        float z = (RNG.Next(0, MG.zSize) + transform.position.z) * transform.localScale.z;



        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, 1000f, z), Vector3.down, out hit, 2000f, layermask))
        {
            //Instantiate(TreePrefab, new Vector3(x,TreePrefab.transform.position.y + hit.transform.position.y,z),Quaternion.identity);
            //Instantiate(TreePrefab, TreePrefab.transform.position + hit.point, Quaternion.Euler(0, Random.Range(0, 360f), 0));
            //Debug.Log(hit.point.y);
            float y = hit.point.y;
            Debug.DrawRay(new Vector3(x,100f, z), Vector3.down * 10000f, Color.magenta);
        }
        




        MG.Generate(combinedMap);

    }


}
