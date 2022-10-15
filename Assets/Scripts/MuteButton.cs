using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MuteButton : MonoBehaviour
{
    [SerializeField] Texture mutedTex;
    [SerializeField] Texture unmutedTex;
    private bool muted;

    public bool IsMuted { get { return muted; } }

    public void OnButtonClick()
    {
        muted = !muted;
        if (muted)
            GetComponent<RawImage>().texture = mutedTex;
        else                     
            GetComponent<RawImage>().texture = unmutedTex;
    }
}
