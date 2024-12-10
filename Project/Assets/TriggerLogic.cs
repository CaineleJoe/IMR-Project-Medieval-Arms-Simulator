using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public class TriggerLogic : MonoBehaviour
{
    [SerializeField] 
    private UnityEvent<string> triggerEnter;
    
    [SerializeField]
    private UnityEvent<string> triggerExit;
    private void OnTriggerEnter(Collider collider)
    {
        triggerEnter?.Invoke(collider.tag);
    }

    private void OnTriggerExit(Collider collider)
    {
        triggerExit?.Invoke(collider.tag);
    }
}
