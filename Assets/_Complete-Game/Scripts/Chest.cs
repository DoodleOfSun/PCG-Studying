using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public Sprite openSprite;

    private SpriteRenderer spriteRenderer;

    public Item randomItem;

    void Awake()
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Open()
    {
        Debug.Log("상자 열기");
        
        randomItem.randomItemInit();
        
        GameObject toInstantiate = randomItem.gameObject;
        GameObject instance = Instantiate(toInstantiate, new Vector3(transform.position.x, transform.position.y, 0f), Quaternion.identity ) as GameObject;
        instance.transform.SetParent(transform.parent);
        gameObject.layer = 10;
        this.gameObject.SetActive(false);
    }
}
