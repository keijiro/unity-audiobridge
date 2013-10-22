// Unity Audio Bridge Plug-in / C# interface
// By Keijiro Takahashi, 2013
// https://github.com/keijiro/unity-audiobridge
using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class AudioBridge : MonoBehaviour
{
    #region Octave band type definition
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
    
    static float[][] middleFrequenciesForBands = {
        new float[]{ 125.0f, 500, 1000, 2000 },
        new float[]{ 250.0f, 400, 600, 800 },
        new float[]{ 63.0f, 125, 500, 1000, 2000, 4000, 6000, 8000 },
        new float[]{ 31.5f, 63, 125, 250, 500, 1000, 2000, 4000, 8000, 16000 },
        new float[]{ 25.0f, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000 },
        new float[]{ 20.0f, 25, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000, 12500, 16000, 20000 },
    };
    
    static float[] bandwidthForBands = {
        1.414f, // 2^(1/2)
        1.260f, // 2^(1/3)
        1.414f, // 2^(1/2)
        1.414f, // 2^(1/2)
        1.122f, // 2^(1/6)
        1.122f  // 2^(1/6)
    };
    #endregion

    #region Public variables
    public BandType bandType = BandType.TenBand;
    public float sensibility = 8.0f;
    public bool internalMode = false;
    #endregion
    
    #region Private variables
    float[] levels;
    float[] meanLevels;
    #endregion
    
    #region Public properties
    public float[] Levels {
        get { return levels; }
    }

    public float[] MeanLevels {
        get { return meanLevels; }
    }
    #endregion

    #region Interface for the native plug-in
    [StructLayout (LayoutKind.Sequential)]
    struct SharedObject
    {
        public int fftPointNumber;
        public int bandType;
        [MarshalAs (UnmanagedType.ByValArray, SizeConst=32)]
        public float[]
            bandLevels;
    }

#if UNITY_STANDALONE_OSX
    [DllImport ("UnityAudioBridgePlugin")]
    static extern int UnityAudioBridge_Update (ref SharedObject shared);
#else
    static int UnityAudioBridge_Update (ref SharedObject shared)
    {
        // Make dummy spectrum.
        shared.bandLevels = new float[32];
        var bandCount = middleFrequenciesForBands [shared.bandType].Length;
        for (var i = 0; i < 32; i++) {
            shared.bandLevels [i] = (i < bandCount) ? -100.0f : 1.0f;
        }
        return 0;
    }
#endif

    void UpdateWithNativePlugin ()
    {
        SharedObject shared = new SharedObject ();

        // Set up the current configuration.
        shared.fftPointNumber = fftPointNumberForBands [(int)bandType];
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

    #region Internal audio source mode
    float[] altSpectrum;

    int FrequencyToSpectrumIndex (float f)
    {
        var i = Mathf.FloorToInt (f / AudioSettings.outputSampleRate * 2.0f * altSpectrum.Length);
        return Mathf.Clamp (i, 0, altSpectrum.Length - 1);
    }

    void UpdateWithInternalAudioSources ()
    {
        var bandCount = middleFrequenciesForBands [(int)bandType].Length;

        // Reallocate the buffers if it needs.
        if (levels == null || levels.Length != bandCount) {
            levels = new float[bandCount];
            meanLevels = new float[bandCount];
            altSpectrum = new float[fftPointNumberForBands [(int)bandType]];
        }

        // Do FFT.
        AudioListener.GetSpectrumData (altSpectrum, 0, FFTWindow.Blackman);

        // Convert the spectrum into octave bands.
        float[] middlefrequencies = middleFrequenciesForBands [(int)bandType];
        var bandwidth = bandwidthForBands [(int)bandType];
        var filter = Mathf.Exp (-sensibility * Time.deltaTime);
        
        for (var bi = 0; bi < levels.Length; bi++) {
            // Specify the spectrum range of the band.
            int imin = FrequencyToSpectrumIndex (middlefrequencies [bi] / bandwidth);
            int imax = FrequencyToSpectrumIndex (middlefrequencies [bi] * bandwidth);

            // Specify the max level of the band.
            var bandMax = altSpectrum [imin];
            for (var fi = imin + 1; fi < imax; fi++) {
                bandMax = Mathf.Max (bandMax, altSpectrum [fi]);
            }

            // Convert amplitude to decibel.
            bandMax = 20.0f * Mathf.Log10 (bandMax / 2.5f + 1.5849e-13f);

            // Store the result.
            levels [bi] = bandMax;
            meanLevels [bi] = bandMax - (bandMax - meanLevels [bi]) * filter;
        }
    }
    #endregion

    #region MonoBehaviour functions
    void Start ()
    {
        if (internalMode) {
            UpdateWithInternalAudioSources ();
        } else {
            UpdateWithNativePlugin ();
        }
    }

    void Update ()
    {
        if (internalMode) {
            UpdateWithInternalAudioSources ();
        } else {
            UpdateWithNativePlugin ();
        }
    }
    #endregion
}
