using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class plrMovement : MonoBehaviour
{
    public float speed;
    Rigidbody2D rb;
    public TMP_Text txt;
    SpriteRenderer spriteRenderer;
    Color a;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
       rb = gameObject.GetComponent<Rigidbody2D>();
        a = GetComponent<SpriteRenderer>().color;
        a.a = 0.75f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector2 moveDirs = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        rb.AddForce(moveDirs * Time.deltaTime * speed);
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Destroy(txt);
            spriteRenderer.color = a;
        }
    }
}
