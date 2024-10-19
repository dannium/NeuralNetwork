using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bgRotate : MonoBehaviour
{

    public float speed;

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.Rotate(0, 0, Time.deltaTime*speed);
    }
}
//i changed the script from bgmovement to this one cuz there were some errors - D
//wrote this to test my github perms - Creative