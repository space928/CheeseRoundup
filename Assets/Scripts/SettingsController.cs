using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    [SerializeField] private Image settingsButton;
    [SerializeField] private RawImage speakerIcon;
    [SerializeField] private MuteButton muteButton;
    [SerializeField] Texture mutedTex;
    [SerializeField] Texture unmutedTex;
    private Image settingsPanel;
    private Graphic[] settingsControls;
    private bool settingsVisible;

    public bool IsSettingsVisible { get { return settingsVisible; } }

    // Start is called before the first frame update
    void Start()
    {
        settingsPanel = GetComponent<Image>();
        settingsControls = GetComponentsInChildren<Graphic>();

        settingsPanel.CrossFadeAlpha(0, 0, true);
        settingsPanel.enabled = false;
        foreach (var s in settingsControls)
        {
            s.CrossFadeAlpha(0, 0, true);
            s.enabled = false;
        }
    }

    private IEnumerator TweenSettingsCog()
    {
        float time = 0;
        Shadow settingsShadow = settingsButton.GetComponent<Shadow>();
        float dist = settingsShadow.effectDistance.magnitude;
        while (time < 0.5f)
        {
            time += Time.deltaTime;
            float rot = Mathf.SmoothStep(0, 1, time * 2);
            settingsButton.rectTransform.localRotation = Quaternion.AngleAxis(rot * 360, Vector3.forward);
            settingsShadow.effectDistance = new Vector2(-Mathf.Sin(rot*2*Mathf.PI) * dist, -Mathf.Cos(rot * 2 * Mathf.PI) * dist);

            yield return new WaitForEndOfFrame();
        }
    }

    public void OnAudioLevelUpdate(float value)
    {
        if(value < 1e-10)
        {
            speakerIcon.texture = mutedTex;
            muteButton.IsMuted = true;
        } else
        {
            speakerIcon.texture = unmutedTex;
            muteButton.IsMuted = false;
        }
    }

    public void OnToggleSettings()
    {
        settingsVisible = !settingsVisible;
        if (settingsVisible)
        {
            Time.timeScale = 0;
            settingsPanel.enabled = true;
            StartCoroutine(TweenSettingsCog());
            settingsPanel.CrossFadeAlpha(0.5f, 0.5f, false);
            foreach (var s in settingsControls)
            {
                s.enabled = true;
                s.CrossFadeAlpha(1, 0.7f, false);
            }
        } else
        {
            Time.timeScale = 1;
            settingsPanel.enabled = false;
            StartCoroutine(TweenSettingsCog());
            settingsPanel.CrossFadeAlpha(0, 0.3f, false);
            foreach (var s in settingsControls)
            {
                s.enabled = false;
                s.CrossFadeAlpha(0, 0.3f, false);
            }
        }
    }
}
