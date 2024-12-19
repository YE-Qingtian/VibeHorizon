using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PrefabAudioVisualizer01 : MonoBehaviour {

	public GameObject Prefab;
    private GameObject currentPrefab;
	public float ScaleMultiplier;
	private GameObject[] prefabs;
	private float musicalDecibelValue;
    public GetSpectrumDataHelper getSpectrumDataHelper;
    public bool animateScaleZ = false;
    public bool animateScaleX = false;
    private float animateZAmount = 0f;
    private float animateXAmount = 0f;
    public float animateYAmount = 1.0f;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private Vector3 startingScale;
    public float[] MusicalDecibels;
    public Vector3 basePos;
    private GameObject audioCube;


    // Use this for initialization
    void Start () {
        prefabs = new GameObject[getSpectrumDataHelper.MusicalDecibels.Length]; // set the size  of our prefab array to be as large as the number of frequency decibel values (128) in the MusicalDecibels array in GetSpectrumDataHelper script
        createRowOfCubes();
        
        MusicalDecibels = getSpectrumDataHelper.MusicalDecibels;
	}

	void Update () {
        if (animateScaleX)
            animateXAmount = 1f;
        else
            animateXAmount = 0f;
        if (animateScaleZ)
            animateZAmount = 1f;
        else
            animateZAmount = 0f;
        animatePrefabs();
	}

    // This function will use a for loop to instantiate prefabs in a line and parent them to this  game object
    private void createRowOfCubes() {
        // Store starting transform values of this game object
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        startingScale = transform.localScale;

        // Zero out transform values of this game object to create a centered arc
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);
        transform.localScale = Vector3.one;

        // Define the radius of the arc (distance from center)
        float radius = 5f; // You can adjust this for a larger or smaller arc
        float angleIncrement = -180f / (prefabs.Length - 1); // Spacing to fit cubes evenly over 180 degrees

        // Create an arc of prefabs
        for (int i = 0; i < prefabs.Length; i++) {
            // Instantiate prefab and calculate its placement in the arc
            prefabs[i] = Instantiate(Prefab);
            
            // Convert the index to an angle in radians to set its position on the arc
            float angle = Mathf.Deg2Rad * (i * angleIncrement - 90); // Start at -90 degrees for symmetry
            
            // Calculate position on the arc based on radius and angle
            float x = startingPosition.x + radius * Mathf.Cos(angle);
            float z = startingPosition.z + radius * Mathf.Sin(angle);
            Vector3 positionOnArc = new Vector3(x, 0f, z);

            // Set the position and parent the prefab
            prefabs[i].transform.position = positionOnArc;
            prefabs[i].transform.parent = this.transform;

            // 确保 `pitchTextObject` 的文本朝向中心
            GameObject pitchTextObject = prefabs[i].transform.Find("Canvas/Pitch").gameObject;
            if (pitchTextObject != null) {
                // 计算到中心的方向向量并将其转换为旋转
                Vector3 directionToCenter = startingPosition - positionOnArc;
                Quaternion rotationToCenter = Quaternion.LookRotation(directionToCenter) * Quaternion.Euler(0, 180, 0);
                pitchTextObject.transform.rotation = rotationToCenter;
            }
        }

        // Reset this game object's transform
        transform.localScale = startingScale;
        transform.position = startingPosition;
        transform.rotation = startingRotation;
    }



    private void animatePrefabs() {

        // Function to generate pitch names
        string[] GeneratePitchNames() {
            string[] noteNames = { "A", "A#", "B","C", "C#", "D", "D#", "E", "F", "F#", "G", "G#"};
            string[] pitchNames = new string[128]; // MIDI notes from 0 to 127

            for (int i = 0; i < pitchNames.Length; i++) {
                int octave = (i / 12)-1; // Calculate the octave (-1 to 9)
                string note = noteNames[i % 12]; // Get the note name based on the remainder
                pitchNames[i] = note + octave.ToString(); // Combine note and octave
            }

            return pitchNames;
        }

        // Generate the pitch names dynamically
        string[] pitchNames = GeneratePitchNames();
        float textOffset = 0.1f; // 设置文本与audioCube底部之间的距离

        for (int i = 0; i < prefabs.Length; i++) {

            // Scale the audioCube
            musicalDecibelValue = getSpectrumDataHelper.MusicalDecibels[i] * ScaleMultiplier;
            audioCube = prefabs[i].transform.Find("audioCube").gameObject;
            if (audioCube == null) {
                Debug.LogWarning("audioCube not found in prefab at index " + i);
                continue; // Skip this iteration if audioCube is not found
            }

            // Scale up cubes in Y based on frequency decibel values
            audioCube.transform.localScale = Vector3.one * 0.1f + (new Vector3(animateXAmount * musicalDecibelValue, animateYAmount * musicalDecibelValue, animateZAmount * musicalDecibelValue));
            audioCube.transform.localPosition = new Vector3(
                audioCube.transform.localPosition.x,
                (audioCube.transform.localScale.y - 1) / 2,
                audioCube.transform.localPosition.z
            );

            // Change the text of the "Pitch" object inside the Canvas
            GameObject canvasObject = prefabs[i].transform.Find("Canvas").gameObject;
            if (canvasObject != null) {
                // 计算audioCube底部位置，并向下偏移一定距离
                float canvasYPosition = audioCube.transform.localPosition.y - (audioCube.transform.localScale.y / 2) - textOffset;
                canvasObject.transform.localPosition = new Vector3(
                    canvasObject.transform.localPosition.x,
                    canvasYPosition,
                    canvasObject.transform.localPosition.z
                );
            }

            // 设置文本内容
            GameObject pitchTextObject = canvasObject.transform.Find("Pitch").gameObject;
            TMPro.TextMeshProUGUI pitchText = pitchTextObject.GetComponent<TMPro.TextMeshProUGUI>();
            if (pitchText == null) {
                Debug.LogWarning("TextMeshPro component not found on Pitch object at index " + i);
                continue;
            }

            // 设置 pitch 名称
            if (i >= 0 && i < pitchNames.Length) {
                pitchText.text = pitchNames[i];
            } else {
                pitchText.text = ""; // 无效的索引设置为空
            }
        }
    }

}
