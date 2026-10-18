using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class FenceGenerator : MonoBehaviour
{
    public Terrain terrain;
    public PlotGenerator pg;
    public float postSpacing = 10f;
    public float postHeight = 10f;
    public float postWidth = 2f;
    public float postSink = 1f;
    public float[] railHeigts = {0.4f, 0.8f};
    public GameObject basePost;
    
    [ContextMenu("Generate Fence")] public void GenerateFence()
    {
        Vector3 terrainPos = terrain.GetPosition();
        GameObject fences = new GameObject{ name = "Fences" };
        foreach (Cut c in pg.Cuts)
        {
            GameObject fence = new GameObject{ name = "fence" };
            float L = Vector2.Distance(c.a, c.b);
            float n = Mathf.Ceil(L / postSpacing);
            for (int i = 0; i <= n; i++)
            {
                GameObject post = Instantiate(basePost);
                Vector2 point = Vector2.Lerp(c.a, c.b, i / n);
                float y = terrain.SampleHeight(new Vector3(point.x, 0, point.y)) + terrainPos.y - postSink;
                Vector3 start = new Vector3(point.x, y, point.y);
                Vector3 end   = new Vector3(point.x, y + postHeight, point.y);

                post.transform.position = (start + end) / 2f;
                post.transform.localScale = new Vector3(postWidth, postHeight, postWidth);
                post.transform.SetParent(fence.transform);
            }

            List<GameObject> rails = new List<GameObject>();

            for (int i = 0; i < n; i++)
            {
                Vector3 p1 = fence.transform.GetChild(i).transform.position - new Vector3(0, postHeight / 2, 0);
                Vector3 p2 = fence.transform.GetChild(i + 1).transform.position - new Vector3(0, postHeight / 2, 0);

                foreach (float h in railHeigts)
                {
                    GameObject rail = Instantiate(basePost);
                    Vector3 start = p1 + new Vector3(0, h * postHeight, 0);
                    Vector3 end = p2 + new Vector3(0, h * postHeight, 0);
                    Vector3 dir = end - start;
                    rail.transform.position = (start + end) / 2f;
                    rail.transform.rotation = Quaternion.LookRotation(dir);
                    rail.transform.localScale = new Vector3(postWidth / 1.5f, postWidth / 2, dir.magnitude);
                    rails.Add(rail);
                }
            }

            foreach (GameObject r in rails) r.transform.SetParent(fence.transform);

            fence.transform.SetParent(fences.transform);
        }
    }
}
