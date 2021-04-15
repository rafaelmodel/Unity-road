using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class RoadSegment : MonoBehaviour
{
    [SerializeField] Mesh2D shape2D;

    [Range(2,32)]
    [SerializeField] int edgeRingCount = 8;
    [SerializeField] Transform[] controlPoints = new Transform[4];

    Vector3 GetPos (int i) => controlPoints[i].position;
    Mesh mesh;

    void Awake(){
        mesh = new Mesh();
        mesh.name = "Segment";

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }

    public void setControlPoints(int i, Vector3 pos){
        controlPoints[i].position = pos;
    }

    void Update() => GenerateMesh();

    void GenerateMesh()
    {
        mesh.Clear();

        // Vertices
        List<Vector3> verts = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();

        for( int ring = 0; ring < edgeRingCount; ring++){
            float t = ring / (edgeRingCount-1f);
            OrientedPoint op = GetBezierOP(t);
            for( int i = 0; i < shape2D.VertexCount; i++){
                verts.Add(op.LocalToWorldPos(shape2D.vertices[i].point));
                normals.Add(op.LocalToWorldVec(shape2D.vertices[i].normal));
            }
        }

        // Triangles
        List<int> triIndices = new List<int>();
        for( int ring = 0; ring < edgeRingCount-1; ring++){
            int rootIndex = ring * shape2D.VertexCount;
            int rootIndexNext = (ring+1) * shape2D.VertexCount;

            for( int line = 0; line < shape2D.LineCount; line+=2){
                int lineIndexA = shape2D.lineIndices[line];
                int lineIndexB = shape2D.lineIndices[line+1];

                int currentA = rootIndex + lineIndexA;
                int currentB = rootIndex + lineIndexB;

                int nextA = rootIndexNext + lineIndexA;
                int nextB = rootIndexNext + lineIndexB;

                triIndices.Add(currentA);
                triIndices.Add(nextA);
                triIndices.Add(nextB);

                triIndices.Add(currentA);
                triIndices.Add(nextB);
                triIndices.Add(currentB);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triIndices, 0);

    }

    OrientedPoint GetBezierOP(float t){
        Vector3 p0 = GetPos(0);
        Vector3 p1 = GetPos(1);
        Vector3 p2 = GetPos(2);
        Vector3 p3 = GetPos(3);

        Vector3 a = Vector3.Lerp( p0, p1, t );
        Vector3 b = Vector3.Lerp( p1, p2, t );
        Vector3 c = Vector3.Lerp( p2, p3, t );

        Vector3 d = Vector3.Lerp( a, b, t);
        Vector3 e = Vector3.Lerp( b, c, t);

        Vector3 pos = Vector3.Lerp( d, e, t);
        Vector3 tangent = (e - d).normalized;

        return new OrientedPoint(pos, tangent);
    }
}
