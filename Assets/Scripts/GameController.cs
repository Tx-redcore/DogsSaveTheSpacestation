using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using UnityEngine.Audio;

public class GameController : MonoBehaviour
{
    private string saveFilePath;
    private List<float> speedrunTimes = new List<float>();

    public delegate void OnRestart();
    public static event OnRestart onRestart;
    // Start is called before the first frame update

    private AudioSource audioSourceBackground;
    private AudioSource audioSourceSFX;

    public AudioMixerGroup background;
    public AudioMixerGroup sfx;

    public AudioClip backgroundMusic;

    void Start()
        {
        saveFilePath = Path.Combine(Application.persistentDataPath, "speedrunTimes.json");
        LoadSpeedruns();

        PickUp.onPlaySound += OnPlaySound;
        OnFinish.onSave += SaveSpeedRun;

        audioSourceBackground = gameObject.AddComponent<AudioSource>();
        audioSourceSFX = gameObject.AddComponent<AudioSource>();
        audioSourceBackground.outputAudioMixerGroup = background;
        audioSourceSFX.outputAudioMixerGroup = sfx;

        audioSourceBackground.clip = backgroundMusic;
        audioSourceBackground.loop = true;
        audioSourceBackground.Play();
        }

    // Update is called once per frame
    void FixedUpdate()
        {
        //End application
        if (Input.GetKeyDown(KeyCode.Escape))
            {
            SaveSpeedruns();
            Application.Quit();
            }

        //RestartGame
        if (Input.GetKeyDown(KeyCode.R))
            {
            onRestart?.Invoke();
            }

        //Clear save 
        if (Input.GetKeyDown(KeyCode.L))
            {
            if (File.Exists(saveFilePath))
                {
                speedrunTimes = new List<float>();
                File.WriteAllText(saveFilePath, "");
                Debug.Log("Data removed successfully.");
                }
            else
                {
                Debug.Log("No data to remove.");
                }
            }
        }


    void OnPlaySound(AudioClip clip)
        {
        audioSourceSFX.clip = clip;
        audioSourceSFX.loop = false;
        audioSourceSFX.Play();
        }

    private void OnApplicationQuit()
    {
        if(speedrunTimes.Count > 0)
            {
            DataWrapper data = new DataWrapper { speedrunTimes = speedrunTimes};
            string jsonData = JsonUtility.ToJson(data);
            File.WriteAllText(saveFilePath, jsonData);
            }
    }

    public void LoadSpeedruns()
    {
        if (File.Exists(saveFilePath))
        {
            string jsonData = File.ReadAllText(saveFilePath);
            DataWrapper data = JsonUtility.FromJson<DataWrapper>(jsonData);
            if(data != null)
            {
                speedrunTimes = data.speedrunTimes;
            }
        }
    }

    public void SaveSpeedruns()
    {
        if (speedrunTimes.Count > 0)
            {
            DataWrapper data = new DataWrapper { speedrunTimes = speedrunTimes };
            string jsonData = JsonUtility.ToJson(data);
            File.WriteAllText(saveFilePath, jsonData);
            }
        }

    public void SaveSpeedRun(float time)
    {
        speedrunTimes.Add(time);
        //Debug.Log("calls on save speedRun " + time + " " +  FormatTime(time));
        SaveSpeedruns();
    }

    string FormatTime(float time)
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        string min = timeSpan.Minutes.ToString();
        string sec = timeSpan.Seconds.ToString();
        string millisec = timeSpan.Milliseconds.ToString();

        string formattedTime = (min + "m :" + sec + "s :" + millisec + "ms");
        return formattedTime;
    }

    public string GetSpeedRuns()
    {
        string speedruns = "SpeedRun Times" + "\r\n" + "Click R to retry" + "\r\n";
        for (int pos = 0; pos < speedrunTimes.Count; pos++)
            {
            string extraText = "Speedrun: ";
                if (pos == (speedrunTimes.Count-1))
                {
                    extraText = "Current speedrun: ";
                }
            string speedRunText = extraText + FormatTime(speedrunTimes[pos]) + "\r\n";
            speedruns += speedRunText;
            }
        return speedruns;
    }

    [Serializable]
    private class DataWrapper
    {
        public List<float> speedrunTimes;
    }
}
