using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TrailRenderer : MonoBehaviour
{
    [SerializeField] private Transform trackedTransform;
    [SerializeField] private float trailWidth = 0.2f;

    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private List<Vector3> verts = new List<Vector3>();
    private List<int> indices = new List<int>();

    private readonly float fudgeFactor = 1.5f;

    private void AddProfile(Vector3 pos, float ang)
    {
        Vector3 off = new Vector3(Mathf.Sin(ang/ fudgeFactor) * trailWidth, 0, Mathf.Cos(ang/ fudgeFactor) * trailWidth);
        verts.Add(pos + off);
        verts.Add(pos - off);
    }

    private void CreateQuad()
    {
        int i = verts.Count-1;
        indices.Add(i);
        indices.Add(i-1);
        indices.Add(i-2);
        indices.Add(i-1);
        indices.Add(i-3);
        indices.Add(i-2);
    }

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter.mesh = mesh;

        AddProfile(trackedTransform.position, trackedTransform.rotation.eulerAngles.y/180*Mathf.PI);
        AddProfile(trackedTransform.position, trackedTransform.rotation.eulerAngles.y/180*Mathf.PI);
        CreateQuad();
    }

    public void InsertTurn(float oldRot, float newRot)
    {
        Debug.Log($"Turned from {oldRot} to {newRot}");

        float ang = (oldRot + newRot) / 2 / fudgeFactor;
        float mitreWidth = trailWidth;// * (1 + Mathf.Sin(Mathf.Abs(oldRot - newRot))/2);
        Vector3 off = new Vector3(Mathf.Sin(ang) * mitreWidth, 0, Mathf.Cos(ang) * mitreWidth);
        verts[verts.Count - 1] = trackedTransform.position - off;
        verts[verts.Count - 2] = trackedTransform.position + off;

        AddProfile(trackedTransform.position, trackedTransform.rotation.eulerAngles.y / 180 * Mathf.PI);
        CreateQuad();
    }

    public void ResetTrail()
    {
        verts.Clear();
        indices.Clear();

        AddProfile(trackedTransform.position, trackedTransform.rotation.eulerAngles.y / 180 * Mathf.PI);
        AddProfile(trackedTransform.position, trackedTransform.rotation.eulerAngles.y / 180 * Mathf.PI);
        CreateQuad();
    }

    // Update is called once per frame
    void Update()
    {
        // Update trail
        // Line up with the player
        float ang = trackedTransform.rotation.eulerAngles.y / 180 * Mathf.PI / fudgeFactor;
        Vector3 off = new Vector3(Mathf.Sin(ang) * trailWidth, 0, Mathf.Cos(ang) * trailWidth);
        verts[verts.Count - 1] = trackedTransform.position - off;
        verts[verts.Count - 2] = trackedTransform.position + off;

        // Update mesh
        mesh.SetVertices(verts);
        mesh.SetTriangles(indices, 0);
        mesh.UploadMeshData(false);
    }
}
