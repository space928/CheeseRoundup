using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 0.01f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private bool constrainY = true;
    [SerializeField] private float angleSnaps = 8;
    [SerializeField] private float turnTime = 0.3f;

    private float initialY;
    private float lastTurn = 0;
    private float currentAngle = 0;
    private MouseWall currentWall;

    private readonly string EDGE_WALL_TAG = "EdgeWall";

    // Start is called before the first frame update
    void Start()
    {
        initialY = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        // Find what we are pointing at
        if (lastTurn >= turnTime)
        {
            lastTurn = 0;
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

            Vector3 target = new Vector3(hit.point.x, constrainY ? initialY : hit.point.y, hit.point.z);
            Vector3 vec = target - transform.position;
            currentAngle = Mathf.Round(Mathf.Atan2(vec.z, vec.x) / 2 / Mathf.PI * angleSnaps) / angleSnaps * Mathf.PI * 2;
        }


        // Move towards it
        Vector3 forward = new Vector3(Mathf.Cos(currentAngle), 0, Mathf.Sin(currentAngle));
        transform.position += moveSpeed * Time.deltaTime * forward;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        lastTurn += Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(EDGE_WALL_TAG))
        {
            currentWall = GetComponent<MouseWall>();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(EDGE_WALL_TAG))
        {
            currentWall = null;
        }
    }
}