using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CineShake : SingletonPattern<CineShake>
{
    //script to make the camera shake when the player 
    //uses a heavy/charged attack

    private CinemachineVirtualCamera cinemachineVirtualCamera;
    private float shakeTimer = 0;

    private void Start()
    {
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
    }

    public void Shake(float intensity, float time)
    {
        //this makes the camera shake
        CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        //these two lines tell the camera to shake for how long, and how intense
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        shakeTimer = time;
    }

    private void Update()
    {
        if (shakeTimer > 0)
        {
            //count down to 0
            shakeTimer -= Time.deltaTime;
        }

        if (shakeTimer <= 0f)
        {
            //Time's up
            CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
            cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

            cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0f;
        }
    }
}
