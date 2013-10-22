using UnityEngine;
using System.Collections;

public class SpectrumBar : MonoBehaviour
{
    public int index;
    public bool useMeanLevel;

    AudioBridge audioBridge;

    void Awake()
    {
        audioBridge = FindObjectOfType(typeof(AudioBridge)) as AudioBridge;
    }

    void Update ()
    {
        var levels = useMeanLevel ? audioBridge.MeanLevels : audioBridge.Levels;
        if (index < levels.Length) {
            var vs = transform.localScale;
            vs.y = levels[index] * 0.2f;
            transform.localScale = vs;
        }
    }
}
