using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof (AudioSource))]
public class AudioPeer : MonoBehaviour
{
    AudioSource _audioSource;
    public float[] spectrum = new float[512];
    public float[] freqband = new float[8];
    public float frequency;
    public string note;
    public double noteError;
    private static readonly string[] noteNames = {"C0","C#0","D0","D#0","E0","F0","F#0","G0","G#0","A0","A#0","B0","C1","C#1","D1","D#1","E1","F1","F#1","G1","G#1","A1","A#1","B1","C2","C#2","D2","D#2","E2","F2","F#2","G2","G2#","A2","A2#","B2","C3","C3#","D3","D3#","E3","F3","F3#","G3","G3#","A3","A3#","B3","C4","C4#","D4","D4#","E4","F4","F4#","G4","G4#","A4","A4#","B4","C5","C5#","D5","D5#","E5","F5","F5#","G5","G5#","A5","A5#","B5","C6","C6#","D6","D6#","E6","F6","F6#","G6","G6#","A6","A6#","B6","C7","C7#","D7","D7#","E7","F7","F7#","G7","G7#","A7","A7#","B7","C8","C8#","D8","D8#","E8","F8","F8#","G8","G8#","A8","A8#","B8","Beyond B8"};
    private static readonly double[] frequencies = {16.35,17.32,18.35,19.45,20.60,21.83,23.12,24.50,25.96 ,27.50 ,29.14 ,30.87 ,32.70 ,34.65 ,36.71 ,38.89 ,41.20 ,43.65 ,46.25 ,49.00 ,51.91 ,55.00 ,58.27 ,61.74 ,65.41 ,69.30 ,73.42 ,77.78 ,82.41 ,87.31 ,92.50 ,98.00 ,103.83 ,110.00 ,116.54 ,123.47 ,130.81 ,138.59 ,146.83 ,155.56 ,164.81 ,174.61 ,185.00 ,196.00 ,207.65 ,220.00 ,233.08 ,246.94 ,261.63 ,277.18 ,293.66 ,311.13 ,329.63 ,349.23 ,369.99 ,392.00 ,415.30 ,440.00 ,466.16 ,493.88 ,523.25 ,554.37 ,587.33 ,622.25 ,659.26 ,698.46 ,739.99 ,783.99 ,830.61 ,880.00 ,932.33 ,987.77 ,1046.50 ,1108.73 ,1174.66 ,1244.51 ,1318.51 ,1396.91 ,1479.98 ,1567.98 ,1661.22 ,1760.00 ,1864.66 ,1975.53 ,2093.00 ,2217.46 ,2349.32 ,2489.02 ,2637.02 ,2793.83 ,2959.96 ,3135.96 ,3322.44 ,3520.00 ,3729.31 ,3951.07 ,4186.01 ,4434.92 ,4698.64 ,4978.03 ,5274.04 ,5587.65 ,5919.91 ,6271.93 ,6644.88 ,7040.00 ,7458.62 ,7902.13,8000};


    // Start is called before the first frame update
    void Start()
    {
        _audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        GetSpectrumAudioSource();
        NoteDetection();
    }
    void GetSpectrumAudioSource() {
        _audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
    }
    void NoteDetection()
    {
        float maxVal = 0;
        int maxIndex = 0;

        // Find the peak frequency bin
        for (int i = 1; i < spectrum.Length - 1; i++)
        {
            if (spectrum[i] > maxVal)
            {
                maxVal = spectrum[i];
                maxIndex = i;
            }
        }

        // Parabolic interpolation to improve frequency accuracy
        float leftVal = spectrum[maxIndex - 1];
        float rightVal = spectrum[maxIndex + 1];

        // Offset calculation for parabolic interpolation
        float deltaIndex = 0.5f * (rightVal - leftVal) / (2 * maxVal - leftVal - rightVal);

        // Adjusted frequency calculation
        float interpolatedIndex = maxIndex + deltaIndex;
        frequency = interpolatedIndex * AudioSettings.outputSampleRate / 2 / spectrum.Length;

        // Convert frequency to note
        (note, noteError) = GetNoteAndError(frequency);

        Debug.Log($"Detected Frequency: {frequency}, Note: {note}, Error: {noteError} cents");
    }

    public static (string note, double errorCents) GetNoteAndError(double frequency)
    {
        double minError = double.MaxValue;
        int closestNoteIndex = 0;

        // Find the closest frequency
        for (int i = 0; i < frequencies.Length; i++)
        {
            double error = System.Math.Abs(frequency - frequencies[i]);
            if (error < minError)
            {
                minError = error;
                closestNoteIndex = i;
            }
        }

        // Calculate the cents error (log scale error)
        double closestFrequency = frequencies[closestNoteIndex];
        double centsError = 1200 * System.Math.Log(frequency / closestFrequency, 2);

        // Return the closest note and the error in cents
        return (noteNames[closestNoteIndex], centsError);
    }
}
