using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DummyLogic : MonoBehaviour
{
    public Animator animator;
    public int health = 30;
    void OnTriggerEnter(Collider collider)
    {
        Debug.Log("dummy hit");
        health = health - 10;
        if (health == 0)
        {
            animator.SetBool("died", true);
            return;
        }
        animator.SetBool("pushed", true);
    }

    private void OnTriggerExit(Collider other)
    {
        animator.SetBool("pushed", false);
    }
    
   


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
