using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class PopupLogic : MonoBehaviour
{
    [SerializeField] private Camera PlayerCamera;
    [SerializeField] private Canvas PopupCanvas;
    [SerializeField] private BoxCollider TriggerZone;
    [SerializeField] private AudioSource TriggerSound;
    [SerializeField] private Animator PopupAnimator;

    private void Start()
    {
        PlayerCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        PopupCanvas = this.GetComponentInChildren<Canvas>();
        TriggerZone = this.GetComponentInChildren<BoxCollider>();
        TriggerSound = this.GetComponent<AudioSource>();
        PopupAnimator = this.GetComponentInChildren<Animator>();
    }
    
    private void Update()
    {
        PopupCanvas.transform.LookAt(PlayerCamera.transform.position);
    }

    public void TriggerZoneOnEnter(string r_tag)
    {
        if (r_tag == "MainCamera")
        {
            
            PopupCanvas.gameObject.SetActive(true);
            PopupAnimator.SetBool("Enabled", true);
            TriggerSound.Play();
        }
    }

    public void TriggerZoneOnExit(string r_tag)
    {
        if (r_tag == "MainCamera")
        {
            PopupAnimator.SetBool("Enabled", false);
            StartCoroutine(DelayedDisable());
        }
    }

    IEnumerator DelayedDisable()
    {
        yield return new WaitForSeconds(0.7f);
        PopupCanvas.gameObject.SetActive(false);
    }
    
    

    
}
