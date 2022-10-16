using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CheeseSlicer : MonoBehaviour
{
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private Vector2 from;
    [SerializeField] private Vector2 to;
    private int yieldControl;
    private Transform[] parts;

    // Start is called before the first frame update
    void Start()
    {
        parts = new Transform[transform.childCount];
        for (int i = 0; i < parts.Length; i++)
            parts[i] = transform.GetChild(i);
    }

    void Update()
    {
        if(false && Input.GetMouseButtonDown(0))
        {
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit rhit);
            if (from != Vector2.zero)
            {
                to = new Vector2(rhit.point.x, rhit.point.z);
                SliceCheese(from, to, false);
                from = Vector2.zero;
                to = Vector2.zero;
            } else
                from = new Vector2(rhit.point.x, rhit.point.z);
        }
    }

    public void ResetCheese()
    {
        StartCoroutine(TweenResetCheese());
        //foreach (Transform part in parts)
        //    part.gameObject.SetActive(true);
    }

    public IEnumerator TweenResetCheese()
    {
        foreach (Transform part in parts)
        {
            part.gameObject.SetActive(true);
            yieldControl++;
            if(yieldControl%2==0)
                {
                yield return new WaitForEndOfFrame();
                }
        }
    }

    public void SliceCheese(Vector2 from, Vector2 to, bool half)
    {
        Debug.Log("hiq");
        Vector2 dir = (to - from);

        particles.transform.position = new Vector3(from.x, 0, from.y);
        particles.transform.rotation = Quaternion.LookRotation(new Vector3(dir.y, 0, dir.x), Vector3.up);
        particles.Play();

        foreach (Transform part in parts)
        {
            var p = new Vector2(part.position.x, part.position.z);
            var dir2 = p - from;
            if (Vector2.SignedAngle(dir, dir2) > 0 ^ half)
                part.gameObject.SetActive(false);
        }
    }
}
