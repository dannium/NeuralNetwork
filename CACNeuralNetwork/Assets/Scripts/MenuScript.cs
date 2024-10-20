using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public void Play()
    {
        SceneManager.LoadSceneAsync(0);
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
