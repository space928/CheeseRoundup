using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheeseController : MonoBehaviour
{
    public float moveSpeed;
    public bool constrainY = true;
    public float angleSnaps = 8;

    private bool  waitCheck = false;
    private float initialY;
    private float randAngle1;
    private float randAngle2;
    private GameManager manager;
    private float baseMoveSpeed = 0.01f;
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

        animator.CrossFade("walk", 0.5f);

        //Literally Just Waits The Cat Movement
        StartCoroutine(CatDoesntInstaKill());

        
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision)
    {   
        if (collision.gameObject.CompareTag("EdgeWallGeo"))
        {
            forward = Vector3.Reflect(forward, collision.contacts[0].normal);

        }
        if(collision.gameObject.CompareTag("Mouse"))
        {
            
        }
    }

    IEnumerator CatDoesntInstaKill()
    {
        yield return new WaitForSeconds(1);
        waitCheck = true;
        
    }

    void Update()
    {   
        if(!waitCheck)
        {
            return;
        }
        //Debug.Log(moveSpeed);
        transform.position += moveSpeed * Time.deltaTime * forward;
        transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
    }

}
