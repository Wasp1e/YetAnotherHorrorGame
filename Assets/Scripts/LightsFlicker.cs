using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightsFlicker : MonoBehaviour
{
    public Light lightOB;

    public AudioSource lightSound;
    public float minTime;
    public float maxTime;
    public float timer;
    public static bool isOn = false;
    void Start()
    {
        if (isOn)
        {
            timer = Random.Range(minTime, maxTime);
        }
    }

    void Update()
    {
        if (isOn)
        {
            LightsFlickering();
        }
    }

    void LightsFlickering()
    {
        if (timer > 0)
            timer -= Time.deltaTime;

        if(timer <= 0)
        {
            lightOB.enabled = !lightOB.enabled;
            timer = Random.Range(minTime, maxTime);
            lightSound.Play();
        }
    }
}