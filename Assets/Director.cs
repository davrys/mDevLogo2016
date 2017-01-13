using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Director : MonoBehaviour {

    public bool showTitle = true;
    public bool autoplay = false;
    public bool loop = false;


    public GameObject logoCubePrefab; // For instantiating
    public GameObject cubesContainer;
    Vector3 cubesContainerOriginalPosition;
    Quaternion cubesContainerOriginalRotation;

    LogoCube[][] cubes;
//    List<LogoCube> middleCubes = new List<LogoCube>();
    int countCubesX = 7;
    int countCubesY = 13;
    int midCubeX = 3; //countCubesX / 2;
    int midCubeY = 6; //countCubesY / 2;

    float cubeSize = 1f;
    float cubeDistance = 0.8f;

    public float cubeSizeMultiplier = 0.9f;
    public float cubeSizeMultiplierAdjustment = 0;
    public float cubePositionMultiplier = 0.25f;
    public float cubePositionMultiplierAdjustment = 0;

    public float cubesContainerRotationSpeed = 0;
    float nextCubesRotationTime = 1;


    public AudioSource audioSource;
    AudioAnalyzer audioAnalyzer;
    bool audioStarted = false; // Started playing?
    bool audioPaused = false; // Is playback paused?
    float audioStepSmall = 10; // Seconds
    float audioStepBig = 60; // Seconds
    int audioExplosionRemainingSeconds = 25; // When the countdown should reach zero (time of first explosion)
    int audioSecondExplosionRemainingSeconds = 14; // Time of second explosion


    public GameObject titleText;
    Vector3 titleOriginalPosition;
    Vector3 titleOriginalScale;
    bool vibrateTitle = false;
    float titleVibrationMultiplier = 0.5f;
    float titleVibrationMultiplierAdjustment = 0;

    public Text timeText;
    Vector3 timeOriginalPosition;



    // Use this for initialization
    void Start()
    {
        audioAnalyzer = audioSource.GetComponent<AudioAnalyzer>();

        titleOriginalPosition = titleText.transform.position;
        titleOriginalScale = titleText.transform.localScale;
        timeOriginalPosition = timeText.gameObject.transform.localPosition;
        cubesContainerOriginalRotation = cubesContainer.transform.rotation; //rotation;
        cubesContainerOriginalPosition = cubesContainer.transform.position;

        GenerateLogoCubes();
        ShowInnerLogoCubes(false);
        ShowTitleText(showTitle);

        // Autoplay
        if (autoplay) {
            StartAudio();
        }
    }


    // Update is called once per frame
    void Update ()
    {
        float dt = Time.deltaTime;

        // End of the audio track
        if (!audioSource.isPlaying && !audioPaused && audioStarted) {
            StopAudio();
            if (loop) {
                StartAudio();
            }
        }

        // Keyboard input
        float audioStep = 0;
        // Reset / panic button
        if (Input.GetKeyDown(KeyCode.Escape)) {
            ShowTimeText(true);
            ShowCubesContainer(true);
            StopAudio();
        }
        else if (Input.GetKeyDown(KeyCode.Space)) {
            if (!audioStarted) {
                StartAudio();
            } else {
                if (audioPaused) {
                    audioSource.UnPause();
                } else {
                    audioSource.Pause();
                }
                audioPaused = !audioPaused;
            }
        }
        else if (Input.GetKeyDown(KeyCode.C)) {
            RotateLogoCubes();
        }
        else if (Input.GetKeyDown(KeyCode.I)) {
            ShowInnerLogoCubes(!innerCubesVisible);
        }
        else if (Input.GetKeyDown(KeyCode.L)) {
            loop = !loop;
        }
        else if (Input.GetKeyDown(KeyCode.R)) {
            StartCoroutine(RotateCubeContainer(5, 0));
        }
        else if (Input.GetKeyDown(KeyCode.T)) {
            bool timeShown = timeText.gameObject.activeSelf;
            ShowTimeText(!timeShown);   
        }
        else if (Input.GetKeyDown(KeyCode.M)) {
            showTitle = !showTitle;
            ShowTitleText(showTitle);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            audioStep = -audioStepBig;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow)) {
            audioStep = audioStepBig;
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow)) {
            audioStep = audioStepSmall;
//            cubeSizeMultiplierAdjustment += 0.1f;
//            cubePositionMultiplierAdjustment += 0.5f;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow)) {
            audioStep = -audioStepSmall;
//            cubeSizeMultiplierAdjustment -= 0.1f;
//            cubePositionMultiplierAdjustment -= 0.5f;
        }
        if (audioStep != 0) {
            float newTime = audioSource.time + audioStep;
            if (newTime > 0 && newTime < audioSource.clip.length) {
                audioSource.time = newTime;
            } else {
                StopAudio();
            }
        }



        // Time
        float remainingTime = audioSource.clip.length - audioSource.time;
        int remainingMinutes = (int)(remainingTime) / 60;
        int remainingSeconds = (int)(remainingTime) - remainingMinutes * 60;
//        if (!audioStarted) {
//            remainingTime = 0;
//        }



        // Explosions
        // Explosions are at remaining seconds: 25 and 14.5
        if (remainingMinutes == 0 && remainingSeconds == audioExplosionRemainingSeconds) {
//            ShowCubesContainer(false);
            ShowTimeText(false);
//            // Reset title position to the center (if shown)
//            if (titleText.gameObject.activeSelf) {
//                ShowTitleText(true);
//            }
//            vibrateTitle = true;
            ShowTitleText(true);
        } else if (remainingMinutes == 0 && remainingSeconds == audioSecondExplosionRemainingSeconds) {
            ShowTitleText(true);
        }

        // TODO: doesn't work
//        if (remainingTime > audioExplosionRemainingSeconds) { 
//            ShowTimeText(true);
//       

        // Fade out vibrations
        if (remainingMinutes == 0 && remainingSeconds <= audioSecondExplosionRemainingSeconds) {
            // TOOD: Doesn't work
            float v = (audioSecondExplosionRemainingSeconds - remainingSeconds) / audioSecondExplosionRemainingSeconds;
            titleVibrationMultiplierAdjustment = Mathf.Lerp(0, -titleVibrationMultiplier, v);
        } else {
            titleVibrationMultiplierAdjustment = 0;
        }


        // Animation intensity
        float at = audioSource.time / 60; // audio time in minutes
        if (at < 10f) {
            cubeSizeMultiplier = 0.6f;
            cubePositionMultiplier = 0.2f;
        } else if (at < 15.1) {
            cubeSizeMultiplier = 0.8f;
            cubePositionMultiplier = 0.25f;
        } else if (at < 20) {
            cubeSizeMultiplier = 1.3f;
            cubePositionMultiplier = 0.3f;
        } else if (at < 23.5f) {
            cubeSizeMultiplier = 1.8f;
            cubePositionMultiplier = 0.35f;
        } else if (at < 25f) {
            cubeSizeMultiplier = 2.2f;
            cubePositionMultiplier = 0.4f;
        }  else {
            cubeSizeMultiplier = 3.0f;
            cubePositionMultiplier = 0.45f;
        }
        cubeSizeMultiplier += cubeSizeMultiplierAdjustment;
        cubePositionMultiplier += cubePositionMultiplierAdjustment;

        // Animations
        VibrateTitle();
        VibrateLogoCubes();
        if (Time.time > nextCubesRotationTime) {
            nextCubesRotationTime = Time.time + Random.Range(10,20);
            // Rotate only in first (quite) part of the music
            if (at < 15) {
                RotateLogoCubes();
            }
        }

        // UI
        // Zero time = explosion time
        float rt = remainingTime - audioExplosionRemainingSeconds;
        if (rt < 0) { rt = 0; }
        int rm = (int)(rt) / 60;
        int rs = (int)(rt) - rm * 60;
        timeText.text = rm.ToString("00") + ":" + rs.ToString("00");
// Samples
//        float rts = audioSource.clip.samples - audioSource.timeSamples;
//        timeText.text += ":" + rts.ToString();
    }


    // Audio


    void StartAudio()
    {
        audioSource.Play();
        audioPaused = false;
        audioStarted = true;    
    }


    void StopAudio()
    {
        audioSource.Stop();
        audioSource.time = 0;
        audioStarted = false;
        audioPaused = false;        
    }


    // Content settings


    void ShowTitleText(bool show)
    {
        // Reset state if music didn't start yet
        if (!audioStarted) {
            ShowCubesContainer(true);
            ShowTimeText(true);
        }

        titleText.SetActive(show);
        titleText.transform.position = cubesContainer.activeSelf ? titleOriginalPosition : Vector3.zero;

        cubesContainer.transform.position = show ? cubesContainerOriginalPosition : new Vector3(0, cubesContainerOriginalPosition.y, cubesContainerOriginalPosition.z);
        cubesContainer.transform.rotation = show ? cubesContainerOriginalRotation : Quaternion.identity;

        timeText.gameObject.transform.localPosition = show ? timeOriginalPosition : timeOriginalPosition + new Vector3(0, -40, 0);
        timeText.fontSize = show ? 30 : 20;
    }

    void ShowCubesContainer(bool show)
    {
        cubesContainer.SetActive(show);
    }

    void ShowTimeText(bool show)
    {
        timeText.gameObject.SetActive(show);
    }


	// Logo cubes container


    public bool isCubeContainerRotating = false;
    public IEnumerator RotateCubeContainer(float duration, float delay = 0)
    {
        if (isCubeContainerRotating) {
            yield break;
        }
        isCubeContainerRotating = true;

        // Execute code after the delay
        yield return new WaitForSeconds(delay);

        Vector3 angles = new Vector3(0, 180, 0);
        Quaternion fromAngle = cubesContainer.transform.localRotation;
        Quaternion toAngle = Quaternion.Euler(cubesContainer.transform.localEulerAngles + angles);
        for (float t = 0f ; t < 1f ; t += Time.deltaTime/duration) {
            cubesContainer.transform.localRotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null ;
        }

        isCubeContainerRotating = false;
        // This is a hack, not an universal solution...
        cubesContainer.transform.rotation = showTitle ? cubesContainerOriginalRotation : Quaternion.identity;
    }


    public IEnumerator GlobalyRotateCubeContainer(float duration, float delay = 0)
    {
        if (isCubeContainerRotating) {
            yield break;
        }
        isCubeContainerRotating = true;

        // Execute code after the delay
        yield return new WaitForSeconds(delay);

        Vector3 angles = new Vector3(0, 180, 0);
        Quaternion fromAngle = cubesContainer.transform.rotation;
        Quaternion toAngle = Quaternion.Euler(cubesContainer.transform.eulerAngles + angles);
        for (float t = 0f ; t < 1f ; t += Time.deltaTime/duration) {
            cubesContainer.transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
            yield return null ;
        }

        isCubeContainerRotating = false;
        // This is a hack, not an universal solution...
        cubesContainer.transform.rotation = showTitle ? cubesContainerOriginalRotation : Quaternion.identity;
    }


    void VibrateTitle()
    {
        if (vibrateTitle) {
            float v = Mathf.Clamp(audioAnalyzer.RmsValue, 0, 1);
            titleText.transform.localScale = titleOriginalScale + new Vector3(1,1,1) * (titleVibrationMultiplier + titleVibrationMultiplierAdjustment) * v;
        }
    }


    // Logo cubes


    void RotateLogoCubes()
    {
        RotateLogoCubes(0.5f, new Vector3(1, 0, 0) * 90, 0.8f); // up     
    }


    void RotateLogoCubes(float duration, Vector3 angles, float maxDelay)
    {
        for (int x = 0; x < countCubesX; x++) {
            for (int y = 0; y < countCubesY; y++) {
                LogoCube cube = cubes[x][y];
                float delay = (float)y / (float)countCubesY * maxDelay;
                StartCoroutine(cube.Rotate(duration, angles, delay));
            }
        }
    }


    void VibrateLogoCubes()
    {
        float v = Mathf.Clamp(audioAnalyzer.RmsValue, 0, 1);
//        v = v * v; // Smooth it

        for (int x = 0; x < countCubesX; x++) {
            for (int y = 0; y < countCubesY; y++) {
                LogoCube cube = cubes[x][y];
//                // Skip hidden objects
//                if (!cube.gameObject.activeSelf) {
//                    continue;
//                }

                cube.size = cube.originalSize + v * cubeSizeMultiplier;

                float pv = v * cubePositionMultiplier;
                float px = pv * (x - midCubeX);
                float py = pv * (y - midCubeY);

                cube.transform.localPosition = cube.originalPosition + new Vector3(px, py, v);
            }
        }
    }
        

    bool innerCubesVisible = true;
    void ShowInnerLogoCubes(bool show)
    {
        innerCubesVisible = show;
        for (int x = 0; x < countCubesX; x++) {
            for (int y = 0; y < countCubesY; y++) {
                LogoCube cube = cubes[x][y];
                if (cube.inner) {
                    cube.gameObject.SetActive(show);
                }
            }
        }        
    }


    void GenerateLogoCubes()
    {
        Quaternion originalRotation = cubesContainer.transform.rotation;
        cubesContainer.transform.rotation = Quaternion.identity;

        // Generate 2D array (7x13)
        cubes = new LogoCube[countCubesX][];
        for (int x = 0; x < countCubesX; x++) {
            cubes[x] = new LogoCube[countCubesY];
        }

        // Generate all cubes
        for (int x = 0; x < countCubesX; x++) {
            for (int y = 0; y < countCubesY; y++) {
                LogoCube cube = NewLogoCube(x,y);
                cubes[x][y] = cube;
                // Inner cube
                if ( (x>0 && x<countCubesX-1 && y>2 && y<countCubesY-2) || (x == 3 && y == 1)) {
                    cube.inner = true;
                    cube.color = new Color(0.2f, 0.2f, 0.2f);
                }
            }
        }

        cubesContainer.transform.rotation = originalRotation;
    }


    LogoCube NewLogoCube(int x, int y)
    {
        LogoCube cube = (Instantiate(logoCubePrefab, Vector3.zero, Quaternion.identity) as GameObject).GetComponent<LogoCube>();
        cube.size = cubeSize;
        cube.originalSize = cubeSize;
        cube.color = new Color(1,1,1);

        // Middle cube should be in the middle of container -> move all cubes up and left
        x -= midCubeX;
        y -= midCubeY;
        // Position of the transform relative to the parent transform.
        Vector3 position = new Vector3(x * (cubeSize + cubeDistance), cubeSize * 0.5f + y * (cubeSize + cubeDistance), 0);
        cube.transform.localPosition = position;
        cube.originalPosition = position;
        cube.originalRotation = cube.transform.localRotation;

        // Attach it to parent object transform
        cube.gameObject.transform.parent = cubesContainer.transform;

        return cube;
    }
}