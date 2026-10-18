using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Numerics;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2;



public class TerrainGenerator : MonoBehaviour
{
    public TreeGenerator tg;
    public PlotGenerator pg;
    public Terrain terrain;
    public int seed = 1;
    public int octaves = 5, lacunarity = 2;
    public float scale = 1, persistence = 0.5f;

    public int treeAmount = 20;
    public float maxSteepness = 30f;
    public float maxCloseness = 20f;

    public void GenerateSeed() => seed = Random.Range(0, 100000);

    [ContextMenu("Generate Terrain")] public void UpdateTerrain()
    {
        Random.InitState(seed);
        TerrainData data = terrain.terrainData;
        int res = data.heightmapResolution;
        float [,] heights = data.GetHeights(0, 0, res, res);

        int offsetX = Random.Range(-10000, 10000);
        int offsetZ = Random.Range(-10000, 10000);
        for (int y = 0; y < res; y++)
        {
            for (int x = 0; x < res; x++)
            {
                float height = 0;
                float frequency = 1;
                float amplitude = 1;
                float totalAmplitude = 0;
                for (int i = 0; i < octaves; i++) {
                    float nx = (float)x / (res - 1) * scale * frequency + offsetX;
                    float nz = (float)y / (res - 1) * scale * frequency + offsetZ;
                    height += Mathf.PerlinNoise(nx, nz) * amplitude;
                    frequency *= 2;
                    totalAmplitude += amplitude;
                    amplitude *= persistence;
                }
                heights[y, x] = Mathf.Clamp01(height / totalAmplitude);
            }
        }
        terrain.terrainData.SetHeights(0, 0, heights);
    }

    [ContextMenu("Plant Trees")] public void PlantTrees()
    {
        Random.InitState(seed);
        // if (FindAnyObjectByType<GameObject>())
        GameObject group = new GameObject{ name = "Group" };
        List<Vector3> prevs = new List<Vector3>();
        Vector3 origin = Vector3.zero;
        Vector3 terrainPos = terrain.GetPosition();
        float tsx = terrain.terrainData.size.x;
        float tsz = terrain.terrainData.size.z;

        for (int i = 0; i < treeAmount; i++)
        {
            bool spotFound = true;
            int counter = 0;
            while (true)
            {
                if (counter++ > 10)
                {
                    spotFound = false;
                    break;
                }
                bool validSpot = true;
                float x = terrainPos.x + Random.Range(0, tsx);
                float z = terrainPos.z + Random.Range(0, tsz);
                float y = terrain.SampleHeight(new Vector3(x, 0, z)) + terrainPos.y;

                var u = (x - terrainPos.x) / tsx;
                var v = (z - terrainPos.z) / tsz;
                if (terrain.terrainData.GetSteepness(u, v) >= maxSteepness) {
                    // Debug.Log("Steepness too great!");
                    validSpot = false;
                    // continue;
                }

                origin = new Vector3(x, y, z);
                if (prevs.Count > 0)
                {
                    Vector2 a = new Vector2(x, z);
                    foreach (Vector3 t in prevs)
                    {
                        Vector2 b = new Vector2(t.x, t.z);
                        if (Vector2.Distance(a, b) < maxCloseness) { 
                            // Debug.Log("Too Close!");
                            validSpot = false;
                            // continue;
                        }
                    }
                }

                Plot p = pg.GetPlotAt(x, z);
                if (p == null || p.type != PlotType.Grass) {
                    // Debug.Log("Invalid Spot");
                    validSpot = false;
                }
                else
                {
                    float m = 2;
                    Rect boundary = new Rect(p.r.xMin + m, p.r.yMin + m, p.r.width - 2 * m, p.r.height - 2 * m);

                    if (!boundary.Contains(new Vector2(x, z))) {
                        // Debug.Log("Invalid Spot");
                        validSpot = false;
                    }
                }

                if (validSpot) break;
            }

            if (!spotFound) continue;

            prevs.Add(origin);
            GameObject tree = tg.DrawTree(origin);
            tree.transform.parent = group.transform;
        }

        Debug.Log($"{group.transform.childCount} Trees Generated");
    }
}
