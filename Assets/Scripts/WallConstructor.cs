using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WallConstructor : MonoBehaviour
{
    [SerializeField] private float extendEdgesDistance = 50f;
    [SerializeField] private GameObject wallPrefab;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private Mesh mesh;
    private List<GameObject> walls = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;

        Triangulate(new Vector3[] {
            new Vector3(0,0,1),
            new Vector3(1,0,0),
            new Vector3(2,0,1),
            new Vector3(3,0,0),
            new Vector3(3,0,3),
            new Vector3(0,0,3),
        });
    }

    // Update is called once per frame
    void Update()
    {

    }

    //TODO: Make this a coroutine
    public void ConstructWall(List<Vector3> points, MouseWall startWall, MouseWall endWall)
    {
        if (points.Count < 2 || !startWall || !endWall)
            return;

        Vector3 firstPoint = points[0] + new Vector3(Mathf.Cos(startWall.ExitDirection), 0, Mathf.Sin(startWall.ExitDirection)) * extendEdgesDistance;
        Vector3 lastPoint = points[points.Count - 1] + new Vector3(Mathf.Cos(endWall.ExitDirection), 0, Mathf.Sin(endWall.ExitDirection)) * extendEdgesDistance;

        // Triangulate points
        var pointsArr = new Vector3[points.Count + 2];
        pointsArr[0] = firstPoint;
        pointsArr[points.Count + 1] = lastPoint;
        Array.Copy(points.ToArray(), 0, pointsArr, 1, points.Count);

        
        // Generate mesh
        // Merge with old mesh
        var oldVerts = mesh.vertices;
        var oldTris = mesh.triangles;
        int newVertCount = pointsArr.Length * 2;
        Vector3[] verts = new Vector3[oldVerts.Length + newVertCount];
        int[] tris = new int[oldTris.Length + (pointsArr.Length * 3 - 2) * 3];
        Array.Copy(oldVerts, verts, oldVerts.Length);
        Array.Copy(oldTris, tris, oldTris.Length);

        // Top cap
        var triangulation = Triangulate(pointsArr, oldVerts.Length);

        Array.Copy(pointsArr, 0, verts, oldVerts.Length, pointsArr.Length);
        Array.Copy(triangulation, 0, tris, oldTris.Length, triangulation.Length);

        // Extrusion
        var extrusion = TriangulateExtrusion(pointsArr.Length, oldVerts.Length);
        Array.Copy(pointsArr, 0, verts, oldVerts.Length + pointsArr.Length, pointsArr.Length);
        Array.Copy(OffsetVerts(pointsArr, new Vector3(0, -1, 0)), 0, verts, oldVerts.Length + pointsArr.Length * 2, pointsArr.Length);
        Array.Copy(extrusion, 0, tris, oldTris.Length + triangulation.Length, extrusion.Length);

        // Update mesh
        mesh.SetVertices(verts);
        mesh.SetIndices(tris, MeshTopology.Triangles, 0);

        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);

        // Create colliders
        /*for(int i = 1; i < points.Count-1; i++)
        {
            // Find centre
            Vector3 pos = (points[i] + points[i + 1]) / 2;
            Vector3 diff = (points[i + 1] - points[i]);
            float width = diff.magnitude;
            float yRot = Mathf.Atan2(diff.z, diff.x);
            Quaternion rot = Quaternion.AngleAxis(yRot * 180 / Mathf.PI, Vector3.up);

            // Instantiate it
            var obj = Instantiate(wallPrefab, pos, rot);
            obj.transform.localScale = new Vector3(width, 1, 1);
            walls.Add(obj);
        }*/
    }

    /// <summary>
    /// Offsets all the vertices in an array by a given offset.
    /// </summary>
    /// <param name="verts"></param>
    /// <param name="offset"></param>
    /// <returns></returns>
    /// //TODO: This is slow and allocates heap
    private Vector3[] OffsetVerts(Vector3[] verts, Vector3 offset)
    {
        var ret = new Vector3[verts.Length];
        for (int i = 0; i < verts.Length; i++)
            ret[i] = verts[i] + offset;

        return ret;
    }

    /// <summary>
    /// Given a number of points in a closed border, this generates the list of triangle indices 
    /// to form an extrusion around it.
    /// </summary>
    /// <param name="nPoints"></param>
    /// <param name="indexOffset"></param>
    /// <returns></returns>
    private int[] TriangulateExtrusion(int nPoints, int indexOffset)
    {
        var tris = new int[nPoints*6];

        for(int i = indexOffset ; i < nPoints + indexOffset; i++)
        {
            tris[i*6]   = i;
            tris[i*6+1]   = (i+1) % nPoints;
            tris[i*6+2]   = i+nPoints;
            tris[i * 6 + 3] = i + nPoints;
            tris[i*6+3] = i + (i + 1) % nPoints;
            tris[i*6+3] = i + i + ((nPoints + 1) % nPoints);
        }

        return tris;
    }

    /// <summary>
    /// Triangulates the given array of points using the ear clipping algorithm.
    /// </summary>
    /// <remarks>
    /// Adapted from: http://www.all-systems-phenomenal.com/articles/ear_clipping_triangulation/index.php
    /// </remarks>
    /// <param name="pointsArr"></param>
    /// <returns>the array of triangle indices</returns>
    private int[] Triangulate(Vector3[] pointsArr, int indexOffset = 0)
    {
        var points = new LinkedList<int>(Enumerable.Range(0, pointsArr.Length));

        var indices = new int[(points.Count - 2)*3];
        var node = points.First;
        int index = 0;
        while (points.Count > 2)
        {
            var prev = node.Previous == null ? points.Last.Value : node.Previous.Value;
            var curr = node.Value;
            var next = node.Next == null ? points.First.Value : node.Next.Value;
            var prevVal = pointsArr[prev];
            var currVal = pointsArr[curr];
            var nextVal = pointsArr[next];

            bool isEar = true;
            if (IsConvex(prevVal, currVal, nextVal))
            {
                // Source: trust me bro
                var testNode = node.Next == null ? points.First : node.Next;
                testNode = testNode.Next == null ? points.First : testNode.Next;
                while (testNode != (node.Previous==null?points.Last:node.Previous) && isEar)
                {
                    isEar = !InsideTriangle(prevVal, currVal, nextVal, pointsArr[testNode.Value]);
                    testNode = testNode.Next == null ? points.First : testNode.Next;
                }
            } else
            {
                isEar = false;
            }

            if (isEar)
            {
                indices[index] = prev + indexOffset;
                indices[index+1] = curr + indexOffset;
                indices[index+2] = next + indexOffset;
                index+=3;
                points.Remove(node);
            }
            node = node.Next == null ? points.First : node.Next;
        }

        return indices;
    }

    /// <summary>
    /// Determines if the vertex is locally convex
    /// </summary>
    /// <param name="prev"></param>
    /// <param name="curr"></param>
    /// <param name="next"></param>
    /// <returns></returns>
    private bool IsConvex(Vector3 prev, Vector3 curr, Vector3 next)
    {
        var b = prev - curr;
        var a = next - curr;
        var dot = a.x * b.x + a.z * b.z;
        var det = a.x * b.z - a.z * b.x;
        var angle = Mathf.Atan2(det, dot);
        angle = angle < 0 ? angle + 2 * Mathf.PI : angle;
        return angle <= Mathf.PI;
        //return ((prev.x * (next.z - curr.z)) + (curr.x * (prev.z - next.z)) + (next.x * (curr.z - prev.z))) < 0;
    }

    /// <summary>
    /// Performs the dot product of the x and z components of two 3d vectors.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    private static float Dot2D(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.z * b.z;
    }

    /// <summary>
    /// Determines if point p is inside the triangle abc
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="c"></param>
    /// <param name="p"></param>
    /// <returns></returns>
    private bool InsideTriangle(Vector3 a, Vector3 b, Vector3 c, Vector3 p)
    {
        // Compute differences
        Vector2 v0 = new Vector2(c.x - a.x, c.z - a.z);
        Vector2 v1 = new Vector2(b.x - a.x, b.z - a.z);
        Vector2 v2 = new Vector2(p.x - a.x, p.z - a.z);

        // Compute dot products
        float dot00 = Vector2.Dot(v0, v0);
        float dot01 = Vector2.Dot(v0, v1);
        float dot02 = Vector2.Dot(v0, v2);
        float dot11 = Vector2.Dot(v1, v1);
        float dot12 = Vector2.Dot(v1, v2);

        // Find barycentric coords
        float denom = dot00 * dot11 - dot01 * dot01;
        if (Mathf.Abs(denom) < 1e-10f)
            return true;
        float invDenom = 1/denom;
        float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
        float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

        // Check if point is in triangle
        return (u >= 0) && (v >= 0) && (u + v < 1);
    }
}
