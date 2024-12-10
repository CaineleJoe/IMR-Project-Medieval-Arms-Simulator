using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Random = System.Random;

public class InteractableLogic : MonoBehaviour
{
    [SerializeField] private AudioSource SoundSource;
    [SerializeField] private AudioClip[] SoundEffects;
    
    private Random rand = new Random();


    public void Start()
    {
        SoundSource = this.GetComponentInChildren<AudioSource>();
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer != 3)
            return;
        
        var clip = SoundEffects[rand.Next(0, SoundEffects.Length)];
        SoundSource.clip = clip;
        SoundSource.Play();
    }
}
