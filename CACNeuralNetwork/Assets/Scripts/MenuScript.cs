using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScript : MonoBehaviour
{
    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ModesClick()
    {
        animator.SetTrigger("Mode");
    }

    public void ModesOut()
    {
        animator.SetTrigger("ModeOut");
    }

    public void HowToPlay()
    {
        animator.SetTrigger("h2p");
    }
    public void HowToPlayOut()
    {
        animator.SetTrigger("h2pOut");
    }

}
