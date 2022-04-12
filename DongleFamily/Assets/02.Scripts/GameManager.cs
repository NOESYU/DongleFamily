using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public Transform dongleGroup; // 새로 생성될 동글이 위치와 동글이가 생성될 부모

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
        // 동글이 생성 메소드
        GameObject instant = Instantiate(donglePrefab, dongleGroup);
        Dongle instantDongle = instant.GetComponent<Dongle>();
        return instantDongle;
    }

    private void NextDongle()
    {
        // 다음에 올 동글이 호출
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.level = Random.Range(0, 8);
        lastDongle.gameObject.SetActive(true);

        StartCoroutine(WaitNext()); // StartCoroutine() : 코루틴 제어를 시작하기 위한 함수
    }

    // 코루틴
    IEnumerator WaitNext()
    {
        while (lastDongle != null) // 동글이가 비워질때까지 무한루프
        {
            yield return null; // yield 안쓰고 무한루프 안하면 유니티 다운됨
        }
        // yield return null; // 한프레임을 쉬는 리턴

        // WaitNext가 호출되면 2초의 대기시간을 갖고 그 뒤 Next Dongle()을 호출함
        yield return new WaitForSeconds(2f); // WaitForSeconds : 초 단위로 기다리기

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
