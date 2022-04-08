using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public bool isDrag; // 디폴트 false
    public Rigidbody2D playerRb;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 마우스 포지션 받아옴=>마우스 좌표는 스크린좌표고 vector3 는 월드좌표여서 동일하게하기위해 screentoworldpoint()
            mousePos.z = 0; // 스크린좌표에서 z가 -10이라 화면상에서 안보이니까 z값 고정
            mousePos.y = 8; // 동글이 line보다 위에 오게끔 y값 고정

            // x축 경계 설정
            float minX = -4.2f + transform.localScale.x / 2f; // 벽의위치와 동글이 반지름
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
