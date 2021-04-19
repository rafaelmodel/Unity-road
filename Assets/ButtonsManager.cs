using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonsManager : MonoBehaviour
{
    public bool building = false;
    public BuildingState buildingStade;
    private RoadSegment buildRoad;
    private RoadSegment prevRoad;
    private RoadSegment hitRoad;
    public GameObject roadPrefab;

    bool placingCP;
    bool currentlyBuilding = false;
    bool wasRoadHit = false;
    public bool snapping = false;

    GameObject g;
    RaycastHit hit;
    String prevTag;
    public enum BuildingState
    {
        straitRoad, curvedRoad, idle
    };

    void Update()
    {
        if (building)

        {
            RaycastHit hitInfo;
            if (g != null)
            {
                Ray ray;
                ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hitInfo, 500.0f))
                {
                    hit = hitInfo;

                    if (hit.collider.CompareTag("Node"))
                    {

                        hitRoad = hit.collider.GetComponent<RoadSegment>();
                        g.transform.position = hit.collider.transform.position + new Vector3(0, 0.01f, 0);
                        wasRoadHit = true;
                    }
                    else if (hit.collider.CompareTag("Segment"))
                    {
                        hitRoad = hit.collider.GetComponent<RoadSegment>();


                        g.transform.position = hitRoad.GetClosestPoint(hit.point, hitRoad.paths[1]) + new Vector3(0, 0.01f, 0);

                        wasRoadHit = true;
                    }
                    else if (hit.collider.CompareTag("Terrain"))
                    {

                        g.transform.position = hit.point + new Vector3(0, 0.01f, 0);

                        wasRoadHit = false;
                    }
                }
            }
            else
                return;

            if (Input.GetMouseButtonDown(0) && buildingStade != BuildingState.idle && !currentlyBuilding)
            {
                prevTag = hit.collider.transform.tag;
                if (buildingStade == BuildingState.curvedRoad)
                    placingCP = true;


                if (wasRoadHit)
                {
                    prevRoad = hit.collider.GetComponentInParent<RoadSegment>();
                    buildRoad = CreateRoad(roadPrefab, buildingStade.ToString()).GetComponent<RoadSegment>();

                    buildRoad.points[0].position = g.transform.position;
                    currentlyBuilding = true;

                    return;
                }
                
                prevRoad = null;
                buildRoad = CreateRoad(roadPrefab, buildingStade.ToString()).GetComponent<RoadSegment>();


                buildRoad.points[0].position = g.transform.position;
                currentlyBuilding = true;

                return;
            }

            if (currentlyBuilding)
            {

                        snapping = true;

                if (buildRoad != null && buildingStade == BuildingState.straitRoad)
                {
                    Vector3 midpoint = (buildRoad.points[0].position + buildRoad.points[3].position) / 2f;
                    buildRoad.points[1].position = midpoint;
                    buildRoad.points[2].position = midpoint;

                    if (prevRoad != null)
                    {

                        float dist = Vector3.Distance(prevRoad.points[0].position, buildRoad.points[3].position);
                        print(dist);
                        if (dist < 0.3f)
                        {
                            prevRoad.points[3].localScale += new Vector3(0, 0, .1f);
                            buildRoad.points[0].localScale += new Vector3(0, 0, .1f);
                            // prevRoad.updateColider = true;
                            // buildRoad.updateColider = true;

                            prevRoad.builted = false;
                            buildRoad.builted = false;
                        }
                        else if (dist > .35f && buildRoad.points[0].localScale.z > 1)
                        {
                            prevRoad.points[3].localScale -= new Vector3(0, 0, .1f);
                            buildRoad.points[0].localScale -= new Vector3(0, 0, .1f);
                            // prevRoad.updateColider = true;
                            // buildRoad.updateColider = true;

                            prevRoad.builted = false;
                            buildRoad.builted = false;

                        }
                       
                        if (snapping)
                        {

                            Vector3 currentDir = hit.point - buildRoad.points[0].position;
                            Vector3 prevDir = prevRoad.points[3].position - prevRoad.points[0].position;
                            Vector3 pos;

                            if ((int)buildRoad.points[0].position.magnitude == (int)prevRoad.points[3].position.magnitude)
                                prevDir = (prevRoad.points[3].position - prevRoad.points[0].position);
                            else if ((int)(buildRoad.points[0].position.magnitude) == (int)prevRoad.points[0].position.magnitude)
                                prevDir = (prevRoad.points[0].position - prevRoad.points[3].position);

                            float lengh = (g.transform.position - buildRoad.points[0].position).magnitude;

                            float angle = Vector3.Angle(currentDir, prevDir);
                            float sign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(currentDir, prevDir)));
                            float signed_angle = angle * sign;

                            if (prevTag == "Node")
                            {
                                if (angle > 170)
                                    return;
                                else if (angle < 5)
                                {
                                    currentDir = prevDir;
                                    pos = lengh * currentDir.normalized + buildRoad.points[0].position;
                                    g.transform.position = pos;
                                }
                                else if (signed_angle > 85 && signed_angle < 95)
                                {
                                    currentDir = Quaternion.AngleAxis(-90, Vector3.up) * prevDir;
                                    pos = lengh * currentDir.normalized + buildRoad.points[0].position;
                                    g.transform.position = pos;

                                }
                                else if (signed_angle < -85 && signed_angle > -95)
                                {
                                    currentDir = Quaternion.AngleAxis(90, Vector3.up) * prevDir;
                                    pos = lengh * currentDir.normalized + buildRoad.points[0].position;
                                    g.transform.position = pos;
                                }
                            }
                            else if (prevTag == "Segment")
                            {

                                if (angle > 170 || angle < 10)
                                    return;
                                else if (signed_angle > 85 && signed_angle < 95)
                                {
                                    currentDir = Quaternion.AngleAxis(-90, Vector3.up) * prevDir;
                                    pos = lengh * currentDir.normalized + buildRoad.points[0].position;
                                    g.transform.position = pos;

                                }
                                else if (signed_angle < -85 && signed_angle > -95)
                                {
                                    currentDir = Quaternion.AngleAxis(90, Vector3.up) * prevDir;
                                    pos = lengh * currentDir.normalized + buildRoad.points[0].position;
                                    g.transform.position = pos;
                                }

                            }
                        }
                    }
                    
                    if (prevRoad == null && hitRoad != null)
                    {

                        if (snapping)
                        {
                            Vector3 currentDir = buildRoad.points[0].position - hit.point ;
                            Vector3 prevDir = hitRoad.points[3].position - hitRoad.points[0].position;
                            Vector3 pos;

                            if ((int)buildRoad.points[0].position.magnitude == (int)hitRoad.points[3].position.magnitude)
                                prevDir = (hitRoad.points[3].position - hitRoad.points[0].position);
                            else if ((int)(buildRoad.points[0].position.magnitude) == (int)hitRoad.points[0].position.magnitude)
                                prevDir = (hitRoad.points[0].position - hitRoad.points[3].position);

                            float lengh = (g.transform.position - buildRoad.points[0].position).magnitude;

                            float angle = Vector3.Angle(currentDir, prevDir);
                            float sign = Mathf.Sign(Vector3.Dot(Vector3.up, Vector3.Cross(currentDir, prevDir)));
                            float signed_angle = angle * sign;

                            if (hit.collider.transform.tag == "Segment")
                            {
                                print(angle);
                               
                                if (signed_angle > 85 && signed_angle < 95)
                                {
                                    currentDir = Quaternion.AngleAxis(90, Vector3.up) * prevDir;
                                    pos = lengh * currentDir.normalized + buildRoad.points[0].position;
                                    g.transform.position = pos;

                                }
                                else if (signed_angle < -85 && signed_angle > -95)
                                {
                                    currentDir = Quaternion.AngleAxis(-90, Vector3.up) * prevDir;
                                    pos = lengh * currentDir.normalized + buildRoad.points[0].position;
                                    g.transform.position = pos;
                                }
                                
                            }
                        }

                    }

                    buildRoad.points[3].position = g.transform.position;
                    //Vector3 cPPos = (buildRoad.CP[0].position + buildRoad.CP[3].position) / 2;
                    //buildRoad.CP[2].position = buildRoad.CP[1].position = cPPos;

                    if (Input.GetMouseButtonDown(0))
                    {
                        buildRoad.updateColider = true; 
                        if(prevRoad!=null)
                            prevRoad.updateColider = true;

                        buildRoad.points[0].GetComponent<SphereCollider>().enabled = true;
                        buildRoad.points[3].GetComponent<SphereCollider>().enabled = true;
                        currentlyBuilding = false;
                        buildRoad = null;
                        prevRoad = null;
                        hitRoad = null;
                    }
                }
                else if (buildRoad != null && buildingStade == BuildingState.curvedRoad)
                {
                    if (placingCP)
                    {
                        if (prevRoad != null)
                        {
                            Vector3 dir = (prevRoad.points[3].position - prevRoad.points[2].position).normalized;
                            float lengh = (g.transform.position - buildRoad.points[0].position).magnitude;
                            Vector3 pos = lengh * dir + buildRoad.points[0].position;
                            buildRoad.points[3].position = buildRoad.points[2].position = buildRoad.points[1].position = pos;

                        }
                        else
                            buildRoad.points[3].position = buildRoad.points[2].position = buildRoad.points[1].position = g.transform.position;

                        if (Input.GetMouseButtonDown(0))
                        {
                            placingCP = false;
                            return;
                        }
                    }
                    else
                        buildRoad.points[3].position = g.transform.position;

                    if (Input.GetMouseButtonDown(0))
                    {
                        buildRoad.updateColider = true;
                        buildRoad.points[0].GetComponent<SphereCollider>().enabled = true;
                        buildRoad.points[3].GetComponent<SphereCollider>().enabled = true;
                        currentlyBuilding = false;
                        buildRoad = null;
                        prevRoad = null;
                        hitRoad = null;
                    }
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                building = false;
                buildingStade = BuildingState.idle;
                currentlyBuilding = false;

                Destroy(g);

                if (buildRoad != null)
                    Destroy(buildRoad.gameObject);
            }
        }
    }


    public void BuildRoad()
    {
        if (g != null)
            Destroy(g);

        building = true;
        currentlyBuilding = false;
        buildingStade = BuildingState.idle;
        g = CreateGhost();
    }

    public void BuildStraitRoad()
    {
        buildingStade = BuildingState.straitRoad;
        building = true;
        currentlyBuilding = false;
    }

    public void BuildCurvedRoad()
    {
        buildingStade = BuildingState.curvedRoad;
        building = true;
        currentlyBuilding = false;
        placingCP = true;

    }

    GameObject CreateRoad(GameObject roadToBuild, string name)
    {
        GameObject buildRoad = Instantiate(roadToBuild, Vector3.zero, Quaternion.identity);
        buildRoad.transform.name = name;
        return buildRoad;
    }

    GameObject CreateGhost()
    {
        GameObject GO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        GO.GetComponent<MeshRenderer>().material.color = Color.green;
        GO.GetComponent<SphereCollider>().enabled = false;
        return GO;
    }
}