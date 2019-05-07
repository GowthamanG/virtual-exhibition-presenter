using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Networking;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update

    private Actions actions;
    private Vector3 pos;
    void Start()
    {
        actions = GetComponent<Actions>();
        actions.Walk();

        pos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        actions.Stay();
        
        if (transform.hasChanged)
        {
            if (Math.Abs(pos.x - transform.position.x) > 0.005f ||
                Math.Abs(pos.z - transform.position.z) > 0.005f)
            {
                actions.Walk();
                pos = transform.position;
            }
                
        }
    }

}
