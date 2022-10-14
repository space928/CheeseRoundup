using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWall : MonoBehaviour
{
    [Range(0f, Mathf.PI*2)]
    public float exitDirection = 0;

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, new Vector3(Mathf.Cos(exitDirection), 0, Mathf.Sin(exitDirection)));
    }
}
