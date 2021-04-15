using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseHandler : MonoBehaviour
{
    public static MouseHandler Instance { get; private set; }
    [SerializeField] private LayerMask terrainColliderLayerMask;

    private Vector3 startPosition;
    // Start is called before the first frame update
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static Vector3 GetMousePosition() => Instance.GetMouseWorldPosition();

    private Vector3 GetMouseWorldPosition(){
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 500f, terrainColliderLayerMask)){
            return raycastHit.point;
        }
        else
        {
            return Vector3.zero;
        }
    }
}
