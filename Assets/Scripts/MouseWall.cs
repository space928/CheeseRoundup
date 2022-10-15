using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseWall : MonoBehaviour
{
    [SerializeField][Range(0f, Mathf.PI*2)]
    private float exitDirection = 0;
    [SerializeField] private bool affectedByRotation = false;

    public float ExitDirection 
    { 
        get { return affectedByRotation ? exitDirection - transform.rotation.eulerAngles.y/180*Mathf.PI : exitDirection; } 
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.DrawRay(transform.position, new Vector3(Mathf.Cos(ExitDirection), 0, Mathf.Sin(ExitDirection)));
    }
}
