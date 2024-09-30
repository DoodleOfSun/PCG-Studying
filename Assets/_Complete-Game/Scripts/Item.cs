using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public enum itemType
{
    glove, boot
}


public class Item : MonoBehaviour
{
    public Sprite glove;
    public Sprite boot;

    public itemType equipmentType;
    public Color color;
    public int attackMod, defenseMod;

    private SpriteRenderer spriteRenderer;


    public void randomItemInit()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        selectItem();
    }

    private void selectItem()
    {
        Debug.Log("SELECTITEM");
        // ·£´ý µ¹¸²
        var equipmentCount = Enum.GetValues(typeof(itemType)).Length;
        equipmentType = (itemType)Random.Range(0, equipmentCount);

        // switch¹®À¸·Î È¹µæÇÒ ¾ÆÀÌÅÛ °áÁ¤
        switch (equipmentType)
        {
            case itemType.glove:
                Debug.Log("glove È¹µæ!");
                attackMod = Random.Range(1, 4);
                defenseMod = 0;
                spriteRenderer.sprite = glove;
                break;

            case itemType.boot:
                Debug.Log("boot È¹µæ!");
                attackMod = 0;
                defenseMod = Random.Range(1, 4);
                spriteRenderer.sprite = boot;
                break;
        }


        // ¾ÆÀÌÅÛÀÇ ·¹¾îµµ ±¸Çö. ÆÄ¶û - > ÃÊ·Ï - > ³ë¶û - > »¡°­ ¼ø¼­·Î °­·ÂÇÏ´Ù.

        int randomLevel = Random.Range(0, 100);

        if (randomLevel >= 0 && randomLevel < 50)
        {
            Debug.Log("ÆÄ¶û»ö");
            spriteRenderer.color = color = Color.blue;
            attackMod += Random.Range(1, 4);
            defenseMod += Random.Range(1, 4);
        }

        else if (randomLevel >= 50 && randomLevel < 75)
        {
            Debug.Log("ÃÊ·Ï»ö");
            spriteRenderer.color = color = Color.green;
            attackMod += Random.Range(4, 10);
            defenseMod += Random.Range(4, 10);
        }

        else if (randomLevel >= 75 && randomLevel < 90)
        {
            Debug.Log("³ë¶û»ö");
            spriteRenderer.color = color =  Color.yellow;
            attackMod += Random.Range(15, 25);
            defenseMod += Random.Range(15, 25);
        }

        else
        {
            Debug.Log("»¡°­»ö");
            spriteRenderer.color = color = Color.red;
            attackMod += Random.Range(40, 55);
            defenseMod += Random.Range(40, 55);
        }



    }
}
