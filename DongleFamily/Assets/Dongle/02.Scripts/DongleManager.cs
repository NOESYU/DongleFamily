using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DongleManager : MonoBehaviour
{
    public Dongle lastDongle;
    public GameObject donglePrefab;
    public Transform dongleGroup; // 새로 생성될 동글이 위치와 동글이가 생성될 부모
    
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
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();

        // 동글이 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        
        // 이펙트랑 동글이랑 1:1로 짝 지어줌
        instantDongle.effect = instantEffect;
        return instantDongle;
    }

    private void NextDongle()
    {
        if (isGameover)
        {
            return;
        }

        // 다음에 올 동글이 호출
        Dongle newDongle = GetDongle();
        lastDongle = newDongle;
        lastDongle.manager = this;
        lastDongle.level = Random.Range(0, maxLevel);
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

    public void GameOver()
    {
        if (isGameover)
        {
            return;
        }

        isGameover = true;

        // 스테이지에 쌓인 동글이들을 모두 없애면서
        // 그 동글이들을 점수로 환산

        StartCoroutine(GameoverRoutine());
    }

    IEnumerator GameoverRoutine()
    {
        Dongle[] dongles = FindObjectsOfType<Dongle>();

        // 아래께 사라지면서 물리효과 적용되어 merge 되는거 방지하기위해 물리효과 제거
        for(int i = 0; i < dongles.Length; i++)
        {
            dongles[i].playerRb.simulated = false;
        }

        for (int i = 0; i < dongles.Length; i++)
        {
            dongles[i].Hide(Vector3.up * 100); // 동글이 숨기는걸 게임 화면상 밖으로 없애버림
        }

        yield return new WaitForSeconds(0.2f);
    }
}
