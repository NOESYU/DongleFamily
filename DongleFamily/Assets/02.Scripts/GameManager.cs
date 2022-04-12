using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public Transform dongleGroup; // ���� ������ ������ ��ġ�� �����̰� ������ �θ�

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
        // ������ ���� �޼ҵ�
        GameObject instant = Instantiate(donglePrefab, dongleGroup);
        Dongle instantDongle = instant.GetComponent<Dongle>();
        return instantDongle;
    }

    private void NextDongle()
    {
        // ������ �� ������ ȣ��
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.level = Random.Range(0, 8);
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
}
