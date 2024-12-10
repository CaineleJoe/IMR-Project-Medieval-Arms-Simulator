using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableLogic : MonoBehaviour
{
    public Transform ControllerBase;
    public XRGrabInteractable xrGrab;
    
    void Start()
    {
        xrGrab = this.GetComponent<XRGrabInteractable>();
       // StartCoroutine(WaitForInit());
    }

    IEnumerator WaitForInit()
    {
        yield return new WaitForSeconds(1);
        ControllerBase = GameObject.FindWithTag("RC_Base").transform;
        xrGrab.attachTransform = ControllerBase.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
