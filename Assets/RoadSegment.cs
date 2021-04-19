using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[System.Serializable]

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class RoadSegment : MonoBehaviour
{
    [Range(.05f, 1.5f)]
    public float spacing = 1;
    public float roadWidth = 1;
    public float tiling = 20;
    public Transform[] points;
    public float resolution = 1;

    
    public List<Vector3[]> paths = new List<Vector3[]>();
    
    public bool builted = false;
    public bool updateColider = false;
    Vector3[] evenPoints;


    private void Update()
    {
        if(!builted)
            UpdateRoad();
        
    }

    public void UpdateRoad()
    {
        
       

        evenPoints = CalculateEvenlySpacedPoints(spacing);       
        Mesh mesh = CreateRoadMesh(evenPoints, false);
        GetComponent<MeshFilter>().mesh = mesh;

        
        int textureRepeat = Mathf.RoundToInt(tiling * evenPoints.Length * spacing * .05f);
        GetComponent<MeshRenderer>().sharedMaterial.mainTextureScale = new Vector3(1, textureRepeat);
        if (updateColider)
        {
           
            MeshCollider col = gameObject.GetComponent<MeshCollider>();
            col.sharedMesh = mesh;
            col.enabled = true;
            updateColider = false;
            builted = true;
        }
    }
    Vector3 closestPoint;
    public Vector3 GetClosestPoint(Vector3 from, Vector3[] path)
    {               
        for (int i = 0; i < path.Length; i++)
        {

            if(Vector3.Distance(from, path[i]) < Vector3.Distance(closestPoint, from))
            {
                closestPoint = path[i];
              
            }
           
        }
       
        return closestPoint;
    }
    
    Mesh CreateRoadMesh(Vector3[] points, bool isClosed)
    {
        Vector3[] verts = new Vector3[points.Length * 2];
                
        Vector3[] path0 = new Vector3[points.Length];
        Vector3[] path1 = new Vector3[points.Length];
        Vector3[] path2 = new Vector3[points.Length];

        Vector2[] uvs = new Vector2[verts.Length];
        int numTris = 2 * (points.Length - 1) + ((isClosed) ? 2 : 0);
        int[] tris = new int[numTris * 3];
        int vertIndex = 0;
        int triIndex = 0;

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 forward = Vector3.zero;
            if (i < points.Length - 1 || isClosed)
            {
                forward = points[(i + 1) % points.Length] - points[i];
            }
           

            forward.Normalize();
            
            Vector3 left = new Vector3(-forward.z, forward.y, forward.x);

           
                verts[vertIndex] = points[i] + left * roadWidth * .5f;
                verts[vertIndex + 1] = points[i] - left * roadWidth * .5f;
           
            //aqui
            //GameObject GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            //GO.transform.position = points[i] + left * roadWidth * .5f;

            path0[i] = points[i] - left * roadWidth * .1f;
            path1[i] = points[i];
            path2[i] = points[i] - left * roadWidth;


            float completionPercent = i / (float)(points.Length - 1);
            float v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertIndex] = new Vector3(0, v);
            uvs[vertIndex + 1] = new Vector2(1, v);

            if (i < points.Length - 1 || isClosed)
            {
                tris[triIndex] = vertIndex;
                tris[triIndex + 1] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 2] = vertIndex + 1;

                tris[triIndex + 3] = vertIndex + 1;
                tris[triIndex + 4] = (vertIndex + 2) % verts.Length;
                tris[triIndex + 5] = (vertIndex + 3) % verts.Length;
            }

            vertIndex += 2;
            triIndex += 6;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = verts;
        mesh.triangles = tris;
        mesh.uv = uvs;

        if (updateColider)
        {
            paths.Clear();
            paths.Add(path0);
            paths.Add(path1);
            paths.Add(path2);
        }
        return mesh;
    }

    public Vector3[] CalculateEvenlySpacedPoints(float spacing, float resolution = 1)
    {
        List<Vector3> evenlySpacedPoints = new List<Vector3>();
       
        evenlySpacedPoints.Add(points[0].position);
        Vector3 previousPoint = points[0].position;
        float dstSinceLastEvenPoint = 0;


        float controlNetLength = Vector3.Distance(points[0].position, points[1].position) + Vector3.Distance(points[1].position,
            points[2].position) + Vector3.Distance(points[2].position, points[3].position);
            float estimatedCurveLength = Vector3.Distance(points[0].position, points[3].position) + controlNetLength / 2f;
            int divisions = Mathf.CeilToInt(estimatedCurveLength * resolution * 10);
            float t = 0;
            while (t <= 1)
            {
                t += 1f / divisions;
                Vector3 pointOnCurve = Bezier.EvaluateCubic(points[0].position, points[1].position, points[2].position, points[3].position, t);
                dstSinceLastEvenPoint += Vector3.Distance(previousPoint, pointOnCurve);

                while (dstSinceLastEvenPoint >= spacing)
                {
                    float overshootDst = dstSinceLastEvenPoint - spacing;
                    Vector3 newEvenlySpacedPoint = pointOnCurve + (previousPoint - pointOnCurve).normalized * overshootDst;
                    evenlySpacedPoints.Add(newEvenlySpacedPoint);
                    dstSinceLastEvenPoint = overshootDst;
                    previousPoint = newEvenlySpacedPoint;
                }

                previousPoint = pointOnCurve;
            }
        

        return evenlySpacedPoints.ToArray();
    }
}
