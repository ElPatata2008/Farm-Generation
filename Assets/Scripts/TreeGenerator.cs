using System.Collections.Generic;
using System.IO.Hashing;
using System.Linq.Expressions;
using System.Text;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{
    public GameObject baseTwig;
    public GameObject baseLeaf;
    public int iterations = 2;
    public float step = 5f;
    public float angle = 25f;

    string chain = "X";
    List<Line> path = new List<Line>();
    List<Vector3> leafPoints = new List<Vector3>();

    public GameObject DrawTree(Vector3 offset)
    {
        GameObject tree = new GameObject { name = "Tree" };
        tree.transform.position = offset;
        
        foreach (Line l in path) 
        {
            Vector3 newEnd = l.end + offset;
            Vector3 newStart = l.start + offset;
            GameObject twig = Instantiate(baseTwig);

            twig.transform.SetPositionAndRotation((newEnd + newStart) / 2, Quaternion.FromToRotation(Vector3.up, newEnd - newStart));
            twig.transform.localScale = new Vector3(twig.transform.localScale.x, step / 2, twig.transform.localScale.z);
            twig.name = "twig";

            twig.transform.parent = tree.transform;
        }

        foreach (Vector3 pos in leafPoints)
        {
            GameObject leaf = Instantiate(baseLeaf);
            leaf.transform.position = pos + offset;
            leaf.transform.SetParent(tree.transform);
        }

        tree.transform.localScale *= Random.Range(0.8f, 1.2f);
        tree.transform.rotation = Quaternion.Euler(tree.transform.rotation.x, Random.Range(0, 360), tree.transform.rotation.z);

        return tree;
    }

    struct TurtleState { public Vector3 pos; public Quaternion rot;  }
    struct Line { public Vector3 start, end; }
    void GenerateChain()
    {
        int i = 0;
        chain = "X";
        StringBuilder newChain = new StringBuilder();
        while (i < iterations)
        {
            foreach (char c in chain)
            {
                // if (c == 'F') newChain.Append("F[+F]F[&F]");
                // if (c == 'F') newChain.Append("F[&+F]F[^-F]F[&-F]F");
                // if (c == 'F') newChain.Append("F[-&<F][<++&F]||F[--&>F][+&F]");
                // if (c == 'F') newChain.Append("FF");
                // if (c == 'A') newChain.Append("F[&+FA][&-FA][&>FA][&<FA]");
                if (c == 'X') newChain.Append("F[&&X]>>>>>[&&X]>>>>>[&&X]");
                else newChain.Append(c);
            }
            chain = newChain.ToString();
            i++;
        }
        newChain.Replace("X", "FL");
        chain = newChain.ToString();
        Debug.Log(chain);
    }
    public void GenerateTree()
    {
        GenerateChain();
        TurtleState s = new TurtleState {pos = Vector3.zero, rot = Quaternion.identity };
        Stack<TurtleState> stack = new Stack<TurtleState>();
        path.Clear();
        leafPoints.Clear();
        
        foreach (char c in chain)
        {
            switch(c)
            {
                case 'F': {                        
                    var dir = s.rot * Vector3.up;
                    var newPos = s.pos + dir * step;
                    Line line = new Line { start = s.pos, end = newPos };
                    s.pos = newPos;
                    path.Add(line);
                } break;
                case '+': { s.rot = s.rot * Quaternion.Euler(0, 0, +angle); } break; // Yaw
                case '-': { s.rot = s.rot * Quaternion.Euler(0, 0, -angle); } break; // Yaw
                case '&': { s.rot = s.rot * Quaternion.Euler(+angle, 0, 0); } break; // Pitch
                case '^': { s.rot = s.rot * Quaternion.Euler(-angle, 0, 0); } break; // Pitch
                case '>': { s.rot = s.rot * Quaternion.Euler(0, +angle, 0); } break; // Roll
                case '<': { s.rot = s.rot * Quaternion.Euler(0, -angle, 0); } break; // Roll
                case '[': { stack.Push(s); } break;
                case ']': { 
                    // if (stack.Count > 2) leafPoints.Add(s.pos);
                    if (stack.TryPop(out var result)) s = result; 
                } break;
                case 'L': { leafPoints.Add(s.pos); } break;
                default: break;
            }
        }
    }
}
