using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseController : MonoBehaviour
{
    public float moveSpeed = 0.01f;
    public bool constrainY = true;
    public float angleSnaps = 8;

    private float initialY;
    private float randAngle;
    Vector3 forward;

    // Start is called before the first frame update
    void Start()
    {
        initialY = transform.position.y;
        randAngle = Mathf.Round(Mathf.Atan2(Random.value, Random.value) / 2 / Mathf.PI * angleSnaps) / angleSnaps * Mathf.PI * 2;
        forward = new Vector3(Mathf.Cos(randAngle), 0, Mathf.Sin(randAngle));

    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {   
        Debug.Log(collision.gameObject.name);
        if (collision.gameObject.CompareTag("EdgeWallGeo"))
        {
            forward = Vector3.Reflect(forward, collision.contacts[0].normal);

        }
    }

    void Update()
    {   
        transform.position += moveSpeed * Time.deltaTime * forward;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

}
