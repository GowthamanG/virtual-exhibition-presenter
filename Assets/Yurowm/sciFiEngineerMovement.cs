using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sciFiEngineerMovement : MonoBehaviour
{
    private Actions actions;
    // Start is called before the first frame update
    void Start()
    {
        actions = GetComponent<Actions>();
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.hasChanged)
            actions.Walk();
        else
            actions.Stay();
        
        
    }
}
