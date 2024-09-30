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
        // ���� ����
        var equipmentCount = Enum.GetValues(typeof(itemType)).Length;
        equipmentType = (itemType)Random.Range(0, equipmentCount);

        // switch������ ȹ���� ������ ����
        switch (equipmentType)
        {
            case itemType.glove:
                Debug.Log("glove ȹ��!");
                attackMod = Random.Range(1, 4);
                defenseMod = 0;
                spriteRenderer.sprite = glove;
                break;

            case itemType.boot:
                Debug.Log("boot ȹ��!");
                attackMod = 0;
                defenseMod = Random.Range(1, 4);
                spriteRenderer.sprite = boot;
                break;
        }


        // �������� ��� ����. �Ķ� - > �ʷ� - > ��� - > ���� ������ �����ϴ�.

        int randomLevel = Random.Range(0, 100);

        if (randomLevel >= 0 && randomLevel < 50)
        {
            Debug.Log("�Ķ���");
            spriteRenderer.color = color = Color.blue;
            attackMod += Random.Range(1, 4);
            defenseMod += Random.Range(1, 4);
        }

        else if (randomLevel >= 50 && randomLevel < 75)
        {
            Debug.Log("�ʷϻ�");
            spriteRenderer.color = color = Color.green;
            attackMod += Random.Range(4, 10);
            defenseMod += Random.Range(4, 10);
        }

        else if (randomLevel >= 75 && randomLevel < 90)
        {
            Debug.Log("�����");
            spriteRenderer.color = color =  Color.yellow;
            attackMod += Random.Range(15, 25);
            defenseMod += Random.Range(15, 25);
        }

        else
        {
            Debug.Log("������");
            spriteRenderer.color = color = Color.red;
            attackMod += Random.Range(40, 55);
            defenseMod += Random.Range(40, 55);
        }



    }
}
