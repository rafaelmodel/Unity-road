using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPlacer : MonoBehaviour
{
    [SerializeField] private GameObject roadSegment;
    Vector3 startPosition;
    Vector3 endPosition;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)){
            startPosition = MouseHandler.GetMousePosition();
        }

        if (Input.GetMouseButtonUp(0)){
            endPosition = MouseHandler.GetMousePosition();
            GameObject roadObject = Instantiate(roadSegment);

            RoadSegment segment = roadObject.GetComponent<RoadSegment>();

            segment.setControlPoints(0, startPosition);
            segment.setControlPoints(1, startPosition + endPosition);
            segment.setControlPoints(2, startPosition - endPosition);
            segment.setControlPoints(3, endPosition);
        }
    }
}
