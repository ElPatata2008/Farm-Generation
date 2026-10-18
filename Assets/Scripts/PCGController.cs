using UnityEngine;

public class PCGController : MonoBehaviour
{
    public TerrainGenerator terrain;
    public TreeGenerator trees;
    public PlotGenerator plot;
    public FenceGenerator fence;

    public int seed = 1;


    public void Start() => Generate(); 

    public void GenerateSeed() => seed = Random.Range(0, 100000);
    public void SetAllSameSeed()
    {
        terrain.seed = seed;
        plot.seed = seed;
        Random.InitState(seed);
    }

    [ContextMenu("Generate")] public void Generate()
    {
        trees.GenerateTree(); // Generate L-System
        terrain.UpdateTerrain(); // Generate Perlin Noise
        plot.GeneratePlot(); // Generate BSP
        plot.AssignPlots();
        plot.PaintTerrain(); // Paint from BSP
        terrain.PlantTrees(); // Draw from L-System
        fence.GenerateFence(); 
    }
}
