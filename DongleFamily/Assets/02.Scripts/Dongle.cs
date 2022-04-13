using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public GameManager manager;

    public int level;
    public bool isDrag; // 디폴트 false
    public bool isMerge; // 합쳐지는 중인지 판단

    Rigidbody2D playerRb;
    CircleCollider2D playerCol;
    Animator playerAnim;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        playerCol = GetComponent<CircleCollider2D>();
    }

    private void OnEnable()
    {
        playerAnim.SetInteger("Level", level);
    }

    private void Update()
    {
        if (isDrag)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 마우스 포지션 받아옴=>마우스 좌표는 스크린좌표고 vector3 는 월드좌표여서 동일하게하기위해 screentoworldpoint()
            mousePos.z = 0; // 스크린좌표에서 z가 -10이라 화면상에서 안보이니까 z값 고정
            mousePos.y = 8; // 동글이 line보다 위에 오게끔 y값 고정

            // x축 경계 설정
            float minX = -4.4f + transform.localScale.x / 2f; // 벽의위치와 동글이 반지름
            float maxX = 4.4f - transform.localScale.x / 2f;

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

    private void OnCollisionStay2D(Collision2D collision)
    {
        // 물리적 충돌중일때 계속 실행되는 Stay
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            // 서로 레벨이 같으면 합치기, 7레벨이전까지만 합쳐짐
            // 1:1 만 합쳐지게 3개가 붙을경우를 방지하기 위해 isMerge사용
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // 나와 other 의 위치 가져오기
                float myX = transform.position.x;
                float myY = transform.position.y;

                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // 조건
                // 1. 내가 아래에 있을 때
                // 2. 동일한 높이일 때, 내가 오른쪽에 있을 때
                if (myY < otherY || (myY == otherY && myX > otherX))
                {
                    // other 숨기기
                    other.Hide(transform.position);
                    // my 레벨업하기
                    LevelUp();

                }
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        // 흡수를 위해 물리효과 비활성화
        playerRb.simulated = false;
        playerCol.enabled = false;

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;
        while (frameCount < 20)
        {
            frameCount++;
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            yield return null;
        }

        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;

        // 이동, 회전속도 제거하기
        playerRb.velocity = Vector2.zero;
        playerRb.angularVelocity = 0;

        StartCoroutine(LevelUpRoutine());
    }

    IEnumerator LevelUpRoutine()
    {
        yield return new WaitForSeconds(0.2f);

        playerAnim.SetInteger("Level", level + 1);

        yield return new WaitForSeconds(0.3f);
        level++;

        // 떨어지는 동글이를 maxlevel로 하기위해서
        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }
}
