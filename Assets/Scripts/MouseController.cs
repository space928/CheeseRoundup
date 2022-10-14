using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.01f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool constrainY = true;

    private float initialY;

    // Start is called before the first frame update
    void Start()
    {
        initialY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    { 
        // Find what we are pointing at
        Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

        Vector3 target = new Vector3(hit.point.x, constrainY ? initialY : hit.point.y, hit.point.z);

        // Move towards it
        transform.position = Vector3.MoveTowards(transform.position, Vector3.Lerp(transform.position, target, 0.5f), moveSpeed);
    }
}
