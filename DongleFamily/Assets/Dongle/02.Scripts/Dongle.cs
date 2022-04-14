using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public DongleManager manager;
    public ParticleSystem effect;

    public int level;
    public bool isDrag; // 디폴트 false
    public bool isMerge; // 합쳐지는 중인지 판단

    public Rigidbody2D playerRb;
    CircleCollider2D playerCol;
    Animator playerAnim;
    SpriteRenderer spriteRenderer;

    float deadTime;

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody2D>();
        playerAnim = GetComponent<Animator>();
        playerCol = GetComponent<CircleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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

        if (targetPos == Vector3.up * 100)
        {
            EffectPlay();
        }

        StartCoroutine(HideRoutine(targetPos));
    }

    IEnumerator HideRoutine(Vector3 targetPos)
    {
        int frameCount = 0;
        while (frameCount < 20)
        {
            frameCount++;

            if(targetPos != Vector3.up * 100)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, 0.5f);
            }
            // 게임 종료시 비정상적인 targetPos 값을 전달한 경우
            else if (targetPos == Vector3.up * 100)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
            yield return null;
        }

        // 점수 시스템
        manager.score += (int)Mathf.Pow(2, level); // Pow :  지정숫자의 거듭제곱

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
        
        EffectPlay();
        yield return new WaitForSeconds(0.3f);
        level++;

        // 떨어지는 동글이를 maxlevel로 하기위해서
        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // 일정시간(dead time)동안 line 과 닿아있으면 gameover
        if(collision.tag == "Finish")
        {
            deadTime += Time.deltaTime;

            if(deadTime >= 2)
            {
                spriteRenderer.color = Color.red;
            }
            if(deadTime >= 5)
            {
                manager.GameOver();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // 경계선에 닿았다가 벗어났을 때
        if(collision.tag == "Finish")
        {
            deadTime = 0;
            spriteRenderer.color = Color.white;
        }
    }

    void EffectPlay()
    {
        effect.transform.position = transform.position;
        effect.transform.localScale = transform.localScale;
        effect.Play();
    }
}
