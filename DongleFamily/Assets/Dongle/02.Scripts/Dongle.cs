using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dongle : MonoBehaviour
{
    public DongleManager manager;
    public ParticleSystem effect;

    public int level;
    public bool isDrag; // ����Ʈ false
    public bool isMerge; // �������� ������ �Ǵ�

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
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition); // ���콺 ������ �޾ƿ�=>���콺 ��ǥ�� ��ũ����ǥ�� vector3 �� ������ǥ���� �����ϰ��ϱ����� screentoworldpoint()
            mousePos.z = 0; // ��ũ����ǥ���� z�� -10�̶� ȭ��󿡼� �Ⱥ��̴ϱ� z�� ����
            mousePos.y = 8; // ������ line���� ���� ���Բ� y�� ����

            // x�� ��� ����
            float minX = -4.4f + transform.localScale.x / 2f; // ������ġ�� ������ ������
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
        // ������ �浹���϶� ��� ����Ǵ� Stay
        if (collision.gameObject.tag == "Dongle")
        {
            Dongle other = collision.gameObject.GetComponent<Dongle>();

            // ���� ������ ������ ��ġ��, 7�������������� ������
            // 1:1 �� �������� 3���� ������츦 �����ϱ� ���� isMerge���
            if (level == other.level && !isMerge && !other.isMerge && level < 7)
            {
                // ���� other �� ��ġ ��������
                float myX = transform.position.x;
                float myY = transform.position.y;

                float otherX = other.transform.position.x;
                float otherY = other.transform.position.y;

                // ����
                // 1. ���� �Ʒ��� ���� ��
                // 2. ������ ������ ��, ���� �����ʿ� ���� ��
                if (myY < otherY || (myY == otherY && myX > otherX))
                {
                    // other �����
                    other.Hide(transform.position);
                    // my �������ϱ�
                    LevelUp();

                }
            }
        }
    }

    public void Hide(Vector3 targetPos)
    {
        isMerge = true;

        // ����� ���� ����ȿ�� ��Ȱ��ȭ
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
            // ���� ����� ���������� targetPos ���� ������ ���
            else if (targetPos == Vector3.up * 100)
            {
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, 0.2f);
            }
            yield return null;
        }

        // ���� �ý���
        manager.score += (int)Mathf.Pow(2, level); // Pow :  ���������� �ŵ�����

        isMerge = false;
        gameObject.SetActive(false);
    }

    void LevelUp()
    {
        isMerge = true;

        // �̵�, ȸ���ӵ� �����ϱ�
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

        // �������� �����̸� maxlevel�� �ϱ����ؼ�
        manager.maxLevel = Mathf.Max(level, manager.maxLevel);

        isMerge = false;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // �����ð�(dead time)���� line �� ��������� gameover
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
        // ��輱�� ��Ҵٰ� ����� ��
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
