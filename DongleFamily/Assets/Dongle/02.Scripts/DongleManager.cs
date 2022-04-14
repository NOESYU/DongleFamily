using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DongleManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public Transform dongleGroup; // ���� ������ ������ ��ġ�� �����̰� ������ �θ�
    
    public GameObject effectPrefab;
    public Transform effectGroup;

    public bool isGameover;
    public int score;
    public int maxLevel = 2;

    private void Awake()
    {
        Application.targetFrameRate = 60;        
    }

    private void Start()
    {
        NextDongle();
    }

    private Dongle GetDongle()
    {
        // ����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        // ������ ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        
        // ����Ʈ�� �����̶� 1:1�� ¦ ������
        instantDongle.effect = instantEffect;
        return instantDongle;
    }

    private void NextDongle()
    {
        if (isGameover)
        {
            return;
        }

        // ������ �� ������ ȣ��
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.manager = this;
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        StartCoroutine(WaitNext()); // StartCoroutine() : �ڷ�ƾ ��� �����ϱ� ���� �Լ�
    }

    // �ڷ�ƾ
    IEnumerator WaitNext()
    {
        while (lastDongle != null) // �����̰� ����������� ���ѷ���
        {
            yield return null; // yield �Ⱦ��� ���ѷ��� ���ϸ� ����Ƽ �ٿ��
        }
        // yield return null; // ���������� ���� ����

        // WaitNext�� ȣ��Ǹ� 2���� ���ð��� ���� �� �� Next Dongle()�� ȣ����
        yield return new WaitForSeconds(2f); // WaitForSeconds : �� ������ ��ٸ���

        NextDongle();
    }

    public void TouchDown()
    {
        if(lastDongle==null)
        {
            return;
        }
        lastDongle.Drag();
    }

    public void TouchUp()
    {
        if (lastDongle == null)
        {
            return;
        }
        lastDongle.Drop();
        lastDongle = null;
    }

    public void GameOver()
    {
        if (isGameover)
        {
            return;
        }

        isGameover = true;

        // ���������� ���� �����̵��� ��� ���ָ鼭
        // �� �����̵��� ������ ȯ��

        StartCoroutine(GameoverRoutine());
    }

    IEnumerator GameoverRoutine()
    {
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        // �Ʒ��� ������鼭 ����ȿ�� ����Ǿ� merge �Ǵ°� �����ϱ����� ����ȿ�� ����
        for(int i = 0; i < dongles.Length; i++)
        {
            dongles[i].playerRb.simulated = false;
        }

        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100); // ������ ����°� ���� ȭ��� ������ ���ֹ���
        }

        yield return new WaitForSeconds(0.2f);
    }
}
