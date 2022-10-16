using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseController : MonoBehaviour
{
    public float baseMoveSpeed = 0.01f;
    public float moveSpeed;
    public Animator animator;

    private float initialY;
    private float randAngle1;
    private float randAngle2;
    private GameManager manager;
    private MouseController mouseController;

    Vector3 forward;

    // Start is called before the first frame update
    void Start()
    {
        initialY = transform.position.y;
        randAngle1 = Mathf.Round(Mathf.Atan2(Random.value, Random.value));
        randAngle2 = Mathf.Round(Mathf.Atan2(Random.value, Random.value));
        forward = new Vector3(Mathf.Cos(randAngle1), 0, Mathf.Sin(randAngle2));
        manager = GameManager.instance;
        moveSpeed = baseMoveSpeed * (1 + ( manager.level % 5 ) * 0.3f);
        mouseController = this.manager.mouseController;

        animator.CrossFade("walk", 0.5f);
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {
        return;
        if (collision.gameObject.CompareTag("EdgeWallGeo"))
        {
            forward = Vector3.Reflect(forward, collision.contacts[0].normal);
        }

        //This should trigger on hitting mouse and trail, might fix itself idk
        if(collision.gameObject.CompareTag("Mouse"))
        {
            manager.GameOverLose();
        }
    }

    public static bool Intersects(Vector2 x, Vector2 y, Vector2 z, Vector2 w)
    {
        float a = x.x;
        float b = x.y;
        float c = y.x;
        float d = y.y;
        float p = z.x;
        float q = z.y;
        float r = w.x;
        float s = w.y;
        float det = (c - a) * (s - q) - (r - p) * (d - b);
        if (det == 0) return false;
        float lambda = ((s - q) * (r - a) + (p - r) * (s - b)) / det;
        float gamma = ((b - d) * (r - a) + (c - a) * (s - b)) / det;
        return (0.02f < lambda && lambda < 0.98f) && (0.02f < gamma && gamma < 0.98f);

    }

    public static Vector2 IntersectionPoint(Vector2 _1, Vector2 _2, Vector2 _3, Vector2 _4)
    {
        float x1 = _1.x;
        float x2 = _2.x;
        float x3 = _3.x;
        float x4 = _4.x;
        float y1 = _1.y;
        float y2 = _2.y;
        float y3 = _3.y;
        float y4 = _4.y;
        float Denominator = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1);
        //if(Denominator == 0.0f) return null;
        float ua = ((x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)) / Denominator;
        //float ub = ((_2.x - _1.x) * (_1.y - _3.y) - (_2.y - _1.y) * (_1.x - _3.x)) / Denominator;

        return new Vector2(x1 + ua * (x2 - x1), y1 + ua * (y2 - y1));
    }

    Vector2 Rotate(Vector2 v, float a)
    {
        return new Vector2(v.x * Mathf.Cos(a) - v.y * Mathf.Sin(a), v.x * Mathf.Sin(a) + v.y * Mathf.Cos(a));
    }

    void Update()
    {
        Vector2 previousPosition = new Vector2(transform.position.x, transform.position.z);


        //Debug.Log(moveSpeed);
        transform.position += moveSpeed * Time.deltaTime * forward;
        //transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
        //Makes it look at direction
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

        Vector2 currentPosition = new Vector2(transform.position.x, transform.position.z);

        for(int i = 0; i < mouseController.EdgePointsCount; ++i)
        {
            Vector2 p = mouseController.EdgePoints[i];
            Vector2 q = mouseController.EdgePoints[(i + 1) % mouseController.EdgePointsCount];
            if(Intersects(p, q, currentPosition, previousPosition))
            {
                Vector2 Direction = Vector2.Reflect(currentPosition - previousPosition, Rotate(p - q, 2.55f));
                forward = new Vector3(-Direction.x, 0, -Direction.y);
                break;
            }
        }

    }

}
