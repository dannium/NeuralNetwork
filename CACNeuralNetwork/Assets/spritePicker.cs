using UnityEngine;

public class spritePicker : MonoBehaviour
{
    public Sprite[] sprites;
    SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[Random.Range(0, 4)];
    }

}
