using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[RequireComponent(typeof(PostProcessVolume))]
public class PostProcessController : MonoBehaviour
{
    [SerializeField] Vector2 brightnessRange;
    [SerializeField] Vector2 contrastRange;

    private PostProcessVolume postProcessVolume;

    // Start is called before the first frame update
    void Start()
    {
        postProcessVolume = GetComponent<PostProcessVolume>();
    }

    private static float Remap(float x, float min, float max)
    {
        return (x*(max-min)) + min;
    }

    public void OnUpdateBrightness(float value)
    {
        postProcessVolume.profile.GetSetting<ColorGrading>().postExposure.value = Remap(value, brightnessRange.x, brightnessRange.y);
    }

    public void OnUpdateContrast(float value)
    {
        postProcessVolume.profile.GetSetting<ColorGrading>().contrast.value = Remap(value, contrastRange.x, contrastRange.y);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
