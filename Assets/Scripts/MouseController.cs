using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private WallConstructor wallConstructor;
    [SerializeField] private float moveSpeed = 0.01f;
    [SerializeField] private bool constrainY = true;
    [SerializeField] private float angleSnaps = 8;
    [SerializeField] private float turnTime = 0.3f;
    [SerializeField] private UnityEvent<float, float> onTurn;
    [SerializeField] private UnityEvent onHitWall;

    private float initialY;
    private float lastTurn = 0;
    private float currentAngle = 0;
    private MouseWall lastWall;
    private MouseWall currentWall;
    private List<Vector3> mousePoints = new List<Vector3> (32);
    private bool canMove;

    public bool CanMove { get { return canMove; } set { canMove = value; } }

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
            float nextAngle = Mathf.Round(Mathf.Atan2(vec.z, vec.x) / 2 / Mathf.PI * angleSnaps) / angleSnaps * Mathf.PI * 2;

            if(currentWall)
            {
                // Special movement rules on walls
                // Get the nearest angle perpendicular to the current wall
                float wallAngle = Mathf.Round((Mathf.Atan2(vec.z, vec.x) + currentWall.ExitDirection) / Mathf.PI * 2) / 2 * Mathf.PI - currentWall.ExitDirection;
                if (Mathf.Abs(currentWall.ExitDirection - wallAngle) < MathF.PI / 2 + 0.1f)
                    nextAngle = wallAngle;
                else
                    nextAngle = Mathf.Round((Mathf.Atan2(vec.z, vec.x) + (currentWall.ExitDirection)) / Mathf.PI) * Mathf.PI - currentWall.ExitDirection;
            }

            if(nextAngle != currentAngle)
            {
                //Call Collision detector
                mousePoints.Add(transform.position);
            }

            onTurn.Invoke(currentAngle, nextAngle);

            currentAngle = nextAngle;
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
            currentWall = other.GetComponent<MouseWall>();
            //Debug.Log(mousePoints);
            wallConstructor.ConstructWall(mousePoints, lastWall, currentWall);
            mousePoints.Clear();
            onHitWall.Invoke();
        }
    }



    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(EDGE_WALL_TAG) && other.GetComponent<MouseWall>() == currentWall)
        {
            lastWall = currentWall;
            currentWall = null;
            mousePoints.Add(transform.position);
        }
    }
    private static float DoThing(Vector3 p, Vector3 q, Vector3 r, Vector3 s){
        Vector3 m = q - p;
        return (m.x * s.z - m.z * s.x) / (r.x * s.z - r.z * s.x);
    }

    //Probably dead code
    bool MouseCollision(Vector3 Point1, Vector3 Point2)
    {
        //MouseCollision is called every frame.
        // For mouse collision with its own tail, call this with the current mouse position and the last item of the mousePoints array
        // For mouse collision with the cheese, call this with the cheese's position in the previous frame and this frame.

        Vector3 q = Point1;
        Vector3 s = Point2 - q;

        Vector3 p = this.mousePoints[0];
        for(int i = 1, Count = this.mousePoints.Count - 1; i < Count; ++i){
            Vector3 CurrentMousePoint = this.mousePoints[i];
            Vector3 r = CurrentMousePoint - p;

            // Maybe just add an optimisation of r cross s and if equal 0 then next


            double t = DoThing(p, q, r, s);
            if(t > 0d && t < 1d) return true;
            
            double u = DoThing(q, p, s, r);
            if(u > 0d && u < 1d) return true;

            p = CurrentMousePoint;
        }
        return false;
    }

}
