using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public int level;
    public bool isDrag; // ����Ʈ false
    Rigidbody2D playerRb;
    Animator playerAnim;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        playerAnim.SetInteger("Level", level);
    }

    private void Update()
    {
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // ���콺 ������ �޾ƿ�=>���콺 ��ǥ�� ��ũ����ǥ�� vector3 �� ������ǥ���� �����ϰ��ϱ����� screentoworldpoint()
            mousePos.z = 0; // ��ũ����ǥ���� z�� -10�̶� ȭ��󿡼� �Ⱥ��̴ϱ� z�� ����
            mousePos.y = 8; // ������ line���� ���� ���Բ� y�� ����

            // x�� ��� ����
            float minX = -4.2f + transform.localScale.x / 2f; // ������ġ�� ������ ������
            float maxX = 4.2f - transform.localScale.x / 2f;

            if (mousePos.x < minX)
            {
                mousePos.x = minX;
            }
            if (mousePos.x > maxX)
            {
                mousePos.x = maxX;
            }

            transform.position = Vector3.Lerp(transform.position, mousePos, 0.05f);
        }
    }

    public void Drag()
    {
        isDrag = true;
    }
    public void Drop()
    {
        isDrag = false;
        playerRb.simulated = true;
    }
}
