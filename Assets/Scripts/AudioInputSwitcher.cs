using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static GetSpectrumDataHelper;

public class AudioInputSwitcher : MonoBehaviour
{
    public GetSpectrumDataHelper gsdh; // Reference to GetSpectrumDataHelper
    public AudioClip musicClip;        // Music clip to be played by the AudioSource
    public Button toggleButton;       // Button to toggle the input mode
    private AudioSource audioSource;  // AudioSource component for playing the music
    public bool isAudioSourceMode = true; // Tracks whether AudioSource or AudioListener is active
    public AudioMixerGroup silentAudioMixerGroup;

    void Start()
    {
        // Initialize the AudioSource component
        audioSource = gsdh.audioSource;
        if (audioSource == null)
        {
            Debug.LogError("AudioSource not assigned in GetSpectrumDataHelper!");
            return;
        }

        // Set the initial mode to AudioSource and configure it
        ToggleInputMode();
    }

    // Switch to using AudioSource for spectrum data
    private void ActivateAudioSource()
    {
        if (musicClip == null)
        {
            Debug.LogError("MusicClip not assigned!");
            return;
        }

        // Stop the microphone if it's currently active
        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        // Configure the AudioSource
        audioSource.outputAudioMixerGroup = null;
        audioSource.clip = musicClip;
        audioSource.loop = true;
        audioSource.Play();

        // Update the GetSpectrumDataHelper mode
        gsdh.AudioListenerOrAudioSource = GetSpectrumDataHelper.MethodType.AudioSource;
        isAudioSourceMode = true;

        Debug.Log("Switched to AudioSource mode.");
    }

    // Switch to using AudioListener for spectrum data
    private void ActivateAudioListener()
    {
        // Stop the AudioSource if it's playing
        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        // Start listening to the microphone
        string microphoneDevice = Microphone.devices.Length > 0 ? Microphone.devices[0] : null;
        if (microphoneDevice == null)
        {
            Debug.LogError("No microphone found!");
            return;
        }

        audioSource.clip = Microphone.Start(microphoneDevice, true, 10, AudioSettings.outputSampleRate);
        audioSource.loop = true;
        audioSource.outputAudioMixerGroup = silentAudioMixerGroup;
        StartCoroutine(WaitForMicrophoneToBeReady());

        // Ensure the GetSpectrumDataHelper is using AudioListener for spectrum data
        gsdh.AudioListenerOrAudioSource = MethodType.AudioSource;
        isAudioSourceMode = false;

        Debug.Log("Switched to AudioListener (Microphone) mode.");
    }



    // Coroutine to wait for the microphone to start recording
    private System.Collections.IEnumerator WaitForMicrophoneToBeReady()
    {
        while (!(Microphone.GetPosition(null) > 0))
        {
            yield return null; // Wait for the next frame
        }

        audioSource.Play();
    }

    // Toggle between AudioSource and AudioListener modes
    public void ToggleInputMode()
    {
        if (isAudioSourceMode)
        {
            ActivateAudioListener();
        }
        else
        {
            ActivateAudioSource();
        }
    }
}
