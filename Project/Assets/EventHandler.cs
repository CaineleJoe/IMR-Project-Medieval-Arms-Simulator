using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.SceneManagement;

public class EventHandler : MonoBehaviour
{
    public AudioClip[] UISounds;
    public AudioSource audioSource;
    void Start()
    {
        audioSource = this.GetComponent<AudioSource>();
    }

    public void OnButtonHover()
    {
        audioSource.clip = UISounds[0];
        audioSource.Play();
    }

    public void PressButton()
    {
        audioSource.clip = UISounds[1];
        Debug.Log("game started");
        audioSource.Play();
        SceneManager.LoadScene(2, LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
