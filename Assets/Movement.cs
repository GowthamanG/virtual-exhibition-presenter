using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;
using UnityEngine.Networking;

public class Movement : MonoBehaviour
{
    // Start is called before the first frame update

    private Actions actions;
    private IEnumerator coroutine;
    void Start()
    {
        actions = GetComponent<Actions>();
        actions.Stay();

        coroutine = Countdown(1);
    }

    // Update is called once per frame
    void Update()
    {

        if (transform.hasChanged)
        {
            actions.Walk();
            StartCoroutine(coroutine);
        }
        
        actions.Stay();
        
    }


    IEnumerator Countdown(int seconds)
    {
        int counter = seconds;

        while (counter > 0)
        {
            yield return new WaitForSeconds(1);
            counter--;
        }
    }
}
