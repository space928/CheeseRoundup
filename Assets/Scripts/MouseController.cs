using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MouseController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private WallConstructor wallConstructor;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private float moveSpeed = 0.01f;
    [SerializeField] private bool constrainY = true;
    [SerializeField] private float angleSnaps = 8;
    [SerializeField] private float turnTime = 0.3f;
    [SerializeField] private UnityEvent<float, float> onTurn;
    [SerializeField] private UnityEvent onHitWall;
    [SerializeField] private UnityEvent<Vector2, Vector2, bool> onSlicedWall;


    private float initialY;
    private float lastTurn = 0;
    private float currentAngle = 0;
    private MouseWall lastWall;
    private MouseWall currentWall;
    private List<Vector3> mousePoints = new List<Vector3> (32);
    private bool canMove;

    public bool CanMove { get { return canMove; } set { canMove = value; } }

    private readonly string EDGE_WALL_TAG = "EdgeWall";


    public Vector2[] EdgePoints = new Vector2[32];
    public int EdgePointsCount = 0; //Number of points that are stored in the above list
    public int CurrentEdge = 0; // Mouse always goes counter-clockwise
    public float CurrentEdgeProgress = 0; //This is the distance from the previous point that the mouse has travelled
    public bool IsCutting = false;
    public Vector2 CuttingFrom; //This is the point from which the mouse is cutting
    public Vector2 CuttingTowards; //This is the point towards which the mouse is cutting
    public int CuttingTowardsNextEdge = -1; //This is the point at the end of the wall that's being cut towards
    public int MouseDirection = 1; //1 Means counterclockwise and -1 means clockwise



    // Start is called before the first frame update
    void Start()
    {
        initialY = transform.position.y;
        EdgePoints = new Vector2[32];
        EdgePoints[0] = new Vector2(-1.5f, -1f);
        EdgePoints[1] = new Vector2(1.5f, -1f);
        EdgePoints[2] = new Vector2(1.5f, 1f);
        EdgePoints[3] = new Vector2(-1.5f, 1f);
        EdgePointsCount = 4;
    }

    float CalculateCurrentArea(){
        float Area = 0.0f;

        for(int i = 0; i < EdgePointsCount; ++i){
            Vector2 a = EdgePoints[i];
            Vector2 b = EdgePoints[(i + 1) % EdgePointsCount];
            Area += a.x * b.y - a.y * b.x;
        }

        return Area;
    }

    void CutAwayEdges(){
        IsCutting = false;



        Vector2 dir = CuttingTowards - CuttingFrom;

        List<GameObject> Cats = gameManager.CheeseList;
        int[] CatSides = new int[2];

        int CatSide = 0;
        
        foreach(GameObject Cat in Cats){
            Vector2 Position = new Vector2(Cat.transform.position.x, Cat.transform.position.z);
            Vector2 dir2 = Position - CuttingFrom;
            CatSides[Vector2.SignedAngle(dir, dir2)>0?0:1]++;
        }

        if(CatSides[0] > 0 && CatSides[1] > 0){
            Debug.Log("Lose");
            return;
        }
        if(CatSides[1] > 0) CatSide = 1;
        Debug.Log(CatSide);

        Vector2[] NewEdgePoints = new Vector2[32];
        int i = 0;
        if(true){
            NewEdgePoints[0] = CuttingFrom;
            NewEdgePoints[1] = CuttingTowards;
            Debug.Log("hi");
            onSlicedWall.Invoke(CuttingFrom, CuttingTowards, true);
            
            
            for(; i < (CurrentEdge - CuttingTowardsNextEdge + EdgePointsCount + 1) % EdgePointsCount; ++i){
                NewEdgePoints[i + 2] = EdgePoints[(CuttingTowardsNextEdge + i) % EdgePointsCount];
            }
            CurrentEdge = 1;
        } else{
            NewEdgePoints[0] = CuttingFrom;
            NewEdgePoints[1] = CuttingTowards;
            Debug.Log("hi");
            onSlicedWall.Invoke(CuttingTowards, CuttingFrom, true);
            
            Debug.Log(CuttingTowardsNextEdge + ", " + CurrentEdge);
            
            /*for(int j = (CuttingTowardsNextEdge - CurrentEdge + EdgePointsCount - 1); j >= 0; --j, ++i){
                NewEdgePoints[i + 2] = EdgePoints[(CuttingTowardsNextEdge - j + EdgePointsCount) % EdgePointsCount];
            }*/
            //NewEdgePoints[2] = EdgePoints[(CuttingTowardsNextEdge - 1) % EdgePointsCount];

            //EdgePointsCount = 3;//i + 2;

            Debug.Log("From: " + CuttingFrom  + " To: " + CuttingTowards + " ctne: " + CuttingTowardsNextEdge + " ce: " + CurrentEdge);           
            Debug.Log("Current points: " + EdgePointsCount);
            for(int k = 0; k < EdgePointsCount; k++)
                Debug.Log("   Current p: " + EdgePoints[k]); 

            int j = (CuttingTowardsNextEdge-1) % EdgePointsCount;
            while(j != CurrentEdge % EdgePointsCount)
            {
                //Debug.Log(i + ", " + j + ", " + EdgePointsCount);
                Debug.Log(EdgePoints[j] + ", " + CuttingFrom);
                NewEdgePoints[i++ + 2] = EdgePoints[j];
                j = (j-1+EdgePointsCount) % EdgePointsCount;
            }

            for(int k = 0; k < EdgePointsCount; k++)
                Debug.Log("   New p: " + NewEdgePoints[k]);

            CurrentEdge = 1;
        }


        EdgePoints = NewEdgePoints;
        EdgePointsCount = i + 2;

        

        CuttingTowardsNextEdge = -1;
    }

    Vector2 AdvancePosition(){
        CurrentEdgeProgress += Time.deltaTime / 1;

        if(!IsCutting){
            Vector2 CurrentPoint = EdgePoints[CurrentEdge % EdgePointsCount];
            Vector2 NextPoint = EdgePoints[(CurrentEdge + 1) % EdgePointsCount];
            //Vector2 NextPoint = EdgePoints[(CurrentEdge - 1 + EdgePointsCount-1) % EdgePointsCount];
            float Distance = Vector2.Distance(NextPoint, CurrentPoint);
            Vector3 Position = Vector3.Lerp(CurrentPoint, NextPoint, CurrentEdgeProgress / Distance);
            //Vector3 Position = Vector3.Lerp(CurrentPoint, NextPoint, CurrentEdgeProgress / Distance);

            if(CurrentEdgeProgress > Distance){
                CurrentEdgeProgress = 0;
                CurrentEdge++;

            }

            return Position;
        } else{
            Vector2 CurrentPoint = CuttingFrom;
            Vector2 NextPoint = CuttingTowards;
            float Distance = Vector2.Distance(NextPoint, CurrentPoint);
            Vector3 Position = Vector3.Lerp(CurrentPoint, NextPoint, CurrentEdgeProgress / Distance);

            if(CurrentEdgeProgress > Distance){
                CurrentEdgeProgress = 0;
                CutAwayEdges();
            }

            return Position;
        }
        

    }
    /*private static bool Intersects(Vector2 p, Vector2 q, Vector2 r, Vector2 s){
        float Determinant = (q.x - p.x) * (s.y - r.y) + (s.x - r.x) * (q.y - p.y);
        if(Determinant == 0) return false;
        float Lambda = ((s.y - r.y) * (s.x - p.x) + (r.x - s.x) * (s.y - p.y)) / Determinant;
        float Gamma = ((p.y - q.y) * (s.x - p.x) + (q.x - p.x) * (s.y - p.y)) / Determinant;
        return 0 < Lambda && Lambda < 1 && 0 < Gamma && Gamma < 1;
    }*/

    private static bool Intersects(Vector2 x, Vector2 y, Vector2 z, Vector2 w){
        float a = x.x;
        float b = x.y;
        float c = y.x;
        float d = y.y;
        float p = z.x;
        float q = z.y;
        float r = w.x;
        float s = w.y;
        float det = (c - a) * (s - q) - (r - p) * (d - b);
        if(det == 0) return false;
        float lambda = ((s - q) * (r - a) + (p - r) * (s - b)) / det;
        float gamma = ((b - d) * (r - a) + (c - a) * (s - b)) / det;
        return (0.02f < lambda && lambda < 0.98f) && (0.02f < gamma && gamma < 0.98f);

    }

    private static Vector2 IntersectionPoint(Vector2 _1, Vector2 _2, Vector2 _3, Vector2 _4){
        float x1 = _1.x;
        float x2 = _2.x;
        float x3 = _3.x;
        float x4 = _4.x;
        float y1 = _1.y;
        float y2 = _2.y;
        float y3 = _3.y;
        float y4 = _4.y;
        float Denominator = (y4 - y3)*(x2 - x1) - (x4 - x3)*(y2 - y1);
        //if(Denominator == 0.0f) return null;
        float ua = ((x4 - x3)*(y1 - y3) - (y4 - y3)*(x1 - x3))/Denominator;
        //float ub = ((_2.x - _1.x) * (_1.y - _3.y) - (_2.y - _1.y) * (_1.x - _3.x)) / Denominator;

        return new Vector2(x1 + ua * (x2 - x1), y1 + ua * (y2 - y1));
    }


    // Update is called once per frame
    void Update()
    {
        Vector2 CurrentPosition = AdvancePosition();
        Vector3 NextPosition = new Vector3(CurrentPosition.x, initialY, CurrentPosition.y);
        Vector3 dir = NextPosition - transform.position;
        transform.rotation.SetLookRotation(dir);
        transform.position = NextPosition;
        if(Input.GetMouseButtonDown(0) && !IsCutting && gameManager.state == GameManager.GameState.Playing){
            Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

            Vector2 ClickedPoint = new Vector2(hit.point.x, hit.point.z);

           
            

            Vector2 p = CurrentPosition;
            Vector2 q = ClickedPoint;


            for(int i = 0; i < EdgePointsCount; ++i){
                Vector2 r = EdgePoints[i];
                Vector2 s = EdgePoints[(i + 1) % EdgePointsCount];

                if(Intersects(p, q, r, s)){
                    CurrentEdgeProgress = 0;
                    CuttingFrom = CurrentPosition;
                    CuttingTowards = IntersectionPoint(p, q, r, s);
                    CuttingTowardsNextEdge = (i + 1) % EdgePointsCount;
                    IsCutting = true;

                    break;
                }
            }


        }
        /*
        // Find what we are pointing at
        Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);
        Vector3 target = new Vector3(hit.point.x, constrainY ? initialY : hit.point.y, hit.point.z);
        if (lastTurn >= turnTime)
        {
            lastTurn = 0;
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

        //transform.position =+ forward
        transform.position += moveSpeed * Mathf.Min(1,Vector3.Distance(transform.position, target))*5 * Time.deltaTime * forward;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);


        lastTurn += Time.deltaTime;
        */
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
