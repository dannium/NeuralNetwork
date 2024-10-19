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
       charController = gameObject.GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirs = new Vector2(Mathf.Clamp(Input.GetAxis("Horizontal"), -1, 1), Mathf.Clamp(Input.GetAxis("Vertical"), -1, 1));
        print(moveDirs.x + ", " + moveDirs.y);
        charController.Move(moveDirs * Time.deltaTime * speed);
    }
}
