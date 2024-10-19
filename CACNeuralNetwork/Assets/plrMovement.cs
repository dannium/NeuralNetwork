using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class plrMovement : MonoBehaviour
{
    public float speed;
    private CharacterController charController;
    // Start is called before the first frame update
    void Start()
    {
       charController = gameObject.AddComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        charController.Move(moveDirs * Time.deltaTime * speed);
    }
}
