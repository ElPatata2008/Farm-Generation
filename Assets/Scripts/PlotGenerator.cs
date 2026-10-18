using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using Random = UnityEngine.Random;



public enum PlotType { Grass, Plowed }

public class Plot { public Rect r; public PlotType type; }

public struct Cut { public Vector2 a, b; }

public class PlotGenerator : MonoBehaviour
{
    public Terrain t;
    public int seed = 1;
    public float minPlotSize = 60f;
    [Range(0f, 1f)] public float stopChance = 0.2f;
    [Range(0f, 1f)] public float plowedChance = 0.4f;
    public int maxDepth = 8;
    public float tolerance = 1.25f;
    public float plowMargin = 10f;

    List<Plot> plots = new List<Plot>();
    List<Cut> cuts = new List<Cut>();
    public IReadOnlyList<Cut> Cuts => cuts;

    void Start()
    {
        minPlotSize = minPlotSize <= 0 ? 60f : minPlotSize;
        maxDepth = maxDepth < 0 ? 8 : maxDepth; 
    }
    
    public void GenerateSeed() => seed = Random.Range(0, 100000);

    [ContextMenu("Generate")] public void Generate()
    {
        GeneratePlot();
        AssignPlots();
        PaintTerrain();
    }
    [ContextMenu("Generate Plots")] public void GeneratePlot()
    {
        plots.Clear();
        cuts.Clear();
        Random.InitState(seed);

        Vector3 origin = t.transform.position;
        Vector3 size = t.terrainData.size;
        Rect r = new Rect(origin.x, origin.z, size.x, size.z);

        Split(r, 0);

        float total = 0;
        foreach (Plot p in plots) total += p.r.width * p.r.height;
        Debug.Log($"Plots: {plots.Count}, Cuts: {cuts.Count}, Area: {total} (Expected {size.x * size.z})");

        AssignPlots();
    }

    private void Split(Rect r, int depth)
    {
        bool limitReached = depth >= maxDepth;
        bool stop = depth > 0 && Random.value < stopChance;

        bool canV = r.width >= 2 * minPlotSize;
        bool canH = r.height >= 2 * minPlotSize;

        if (!(canV && canH) || limitReached || stop)
        {
            plots.Add(new Plot {r = r, type = PlotType.Grass});
            return;
        }
        
        int action = 2;
        if (canV && !canH) action = 0;
        else if (!canV && canH) action = 1;
        else if (canV && canH)
        {
            float aspect = Mathf.Max(r.width, r.height) / Mathf.Min(r.width, r.height);
            if (aspect < tolerance) action = Random.Range(0, 2);
            else action = (r.width > r.height) ? 0 : 1;
        }
        

        float ratio = Mathf.Lerp(0.35f, 0.65f, Random.value);

        switch(action)
        {
            case 0:
                {
                    float pos = Mathf.Clamp(r.xMin + ratio * r.width, r.xMin + minPlotSize, r.xMax - minPlotSize);
                    Rect left  = new Rect(r.xMin, r.yMin, pos - r.xMin, r.height);
                    Rect right = new Rect(pos,    r.yMin, r.xMax - pos, r.height);
                    cuts.Add(new Cut{ a = new Vector2(pos, r.yMin), b = new Vector2(pos, r.yMax) });
                    Split(left, depth + 1);
                    Split(right, depth + 1);
                } break;
            case 1:
                {
                    float pos = Mathf.Clamp(r.yMin + ratio * r.height, r.yMin + minPlotSize, r.yMax - minPlotSize);
                    Rect down  = new Rect(r.xMin, r.yMin, r.width, pos - r.yMin);
                    Rect up    = new Rect(r.xMin, pos,    r.width, r.yMax - pos);   
                    cuts.Add(new Cut{ a = new Vector2(r.xMin, pos), b = new Vector2(r.xMax, pos) });
                    Split(down, depth + 1);
                    Split(up, depth + 1);
                } break;
            default: {} break;
        }
    }

    [ContextMenu("Assing Plots")] public void AssignPlots()
    {
        foreach (Plot p in plots) p.type = Random.value < plowedChance ? PlotType.Plowed : PlotType.Grass; 
    }

    [ContextMenu("Paint Terrain")] public void PaintTerrain()
    {
        Random.InitState(seed);
        TerrainData data = t.terrainData;
        int res = data.alphamapResolution;
        int layers = data.alphamapLayers;
        float[,,] map = data.GetAlphamaps(0, 0, res, res);
        Vector3 origin = t.transform.position;
        Vector3 size = data.size;

        foreach (Plot p in plots)
        {
            FillRect(map, p.r, (int)PlotType.Grass, origin, size, res, layers);
            if (p.type.Equals(PlotType.Plowed))
            {
                Rect inner = new Rect(p.r.xMin + plowMargin, p.r.yMin + plowMargin, p.r.width - 2 * plowMargin, p.r.height - 2 * plowMargin);
                if (inner.width > 0 && inner.height > 0) FillRect(map, inner, (int)PlotType.Plowed, origin, size, res, layers);
            }
        }

        data.SetAlphamaps(0, 0, map);
    }

    void FillRect(float[,,] map, Rect r, int layer, Vector3 origin, Vector3 size, int res, int layers)
    {
        int ix0 = ToCell(r.xMin, origin.x, size.x, res);
        int ix1 = ToCell(r.xMax, origin.x, size.x, res);
        int iz0 = ToCell(r.yMin, origin.z, size.z, res);
        int iz1 = ToCell(r.yMax, origin.z, size.z, res);
        for (int iz = iz0; iz < iz1; iz++)
            {
                for (int ix = ix0; ix < ix1; ix++)
                {
                    for (int l = 0; l < layers; l++)
                    {

                        map[iz, ix, l] = (l == layer) ? 1f : 0f;
                    }
                }
            }
    }

    int ToCell(float world, float origin, float size, int res)
    {
        float u = (world - origin) / size;
        return Mathf.Clamp(Mathf.RoundToInt(u * res), 0, res);
    }

    public Plot GetPlotAt(float x, float z)
    {
        foreach(Plot p in plots)
        {
            if (p.r.Contains(new Vector2(x, z))) return p;
        }
        return null;
    }
}
