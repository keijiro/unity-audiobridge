using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class AudioBridge : MonoBehaviour
{
    #region Band type definition
    public enum BandType
    {
        FourBand,
        FourBandVisual,
        EightBand,
        TenBand,
        TwentySixBand,
        ThirtyOneBand
    };

    static int[] fftPointNumberForBands = {
        1024,
        1024,
        2048,
        2048,
        4096,
        8192
    };
    #endregion
    
    #region Public variables
    public BandType bandType = BandType.TenBand;
    public float sensibility = 8.0f;
    #endregion
    
    #region Private variables
    float[] levels;
    float[] meanLevels;
    #endregion
    
    #region Public property
    public float[] Levels {
        get { return levels; }
    }
    
    public float[] MeanLevels {
        get { return meanLevels; }
    }
    #endregion

    #region Interface for native plug-in
    [StructLayout (LayoutKind.Sequential)]
    struct SharedObject
    {
        public int fftPointNumber;
        public int bandType;
        [MarshalAs (UnmanagedType.ByValArray, SizeConst=32)]
        public float[] bandLevels;
    }

    [DllImport ("UnityAudioBridgePlugin")]
    static extern int UnityAudioBridge_Update (ref SharedObject shared);

    void UpdateWithNativePlugin ()
    {
        SharedObject shared = new SharedObject ();

        // Set up the current configuration.
        shared.fftPointNumber = fftPointNumberForBands[(int)bandType];
        shared.bandType = (int)bandType;

        // Run the native plug-in.
        UnityAudioBridge_Update (ref shared);

        // Count the number of bands.
        var bandCount = 0;
        while (shared.bandLevels[bandCount] <= 0.0f) {
            bandCount++;
        }

        // Reallocate the arrays if it needs.
        if (levels == null || levels.Length != bandCount) {
            levels = new float[bandCount];
            meanLevels = new float[bandCount];
        }

        // Retrieve the result.
        var filter = Mathf.Exp (-sensibility * Time.deltaTime);
        for (var i = 0; i < bandCount; i++) {
            levels [i] = shared.bandLevels [i];
            meanLevels [i] = levels [i] - (levels [i] - meanLevels [i]) * filter;
        }
    }
    #endregion

    #region MonoBehaviour functions
    void Start ()
    {
        UpdateWithNativePlugin ();
    }

    void Update ()
    {
        UpdateWithNativePlugin ();
    }
    #endregion
}
