using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DongleManager : MonoBehaviour
{
    // 카테고리별로 변수 정리
    [Header("---------------[ Core ]")]
    public bool isGameover;
    public int score;
    public int maxLevel = 2;

    [Header("---------------[ Object Pooling ]")]
    public GameObject donglePrefab;
    public Transform dongleGroup; // 새로 생성될 동글이 위치와 동글이가 생성될 부모
    public List<Dongle> donglePool; // 동글이 오브젝트풀링을 위한 리스트
    
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool; // 이펙트 오브젝트풀링을 위한 리스트

    [Range(1, 30)] // 인스펙터창에서 간편하게 풀 사이즈를 슬라이더로 값을 조절하기 위해
    public int poolSize;
    public int poolIndex;

    public Dongle lastDongle;

    [Header("---------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer; // 효과음을 끊기지 않게하기위해 배열에 넣고 채널링
    public AudioClip[] sfxClip;
    
    public enum Sfx
    {
        LevelUp,
        Next,
        Attach,
        Button,
        GameOver
    };
    int sfxIndex;

    [Header("---------------[ UI ]")]
    public GameObject startGroup;
    public GameObject endGroup;
    public GameObject playGround;
    public Text scoreText;
    public Text maxScoreText;
    

    private void Awake()
    {
        Application.targetFrameRate = 60;

        donglePool = new List<Dongle>();
        effectPool = new List<ParticleSystem>();

        for(int i = 0; i < poolSize; i++)
        {
            MakeDongle();
        }

        // 최고 점수 저장 & 불러오기
        // PlayerPrefs : 데이터 저장을 담당하는 클래스
        if (!PlayerPrefs.HasKey("MaxScore"))
        {
            PlayerPrefs.SetInt("MaxScore", 0);
        }
       
        maxScoreText.text = PlayerPrefs.GetInt("MaxScore").ToString();
    }

    public void GameStart()
    {
        playGround.SetActive(true);
        scoreText.gameObject.SetActive(true);
        maxScoreText.gameObject.SetActive(true);
        startGroup.SetActive(false);

        bgmPlayer.Play();
        SfxPlay(Sfx.Button);
        NextDongle();
    }

    // 오브젝트 풀을 만들기 위한 함수 생성
    Dongle MakeDongle()
    {
        // 이펙트 생성
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // 동글이 생성
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;

        // 이펙트랑 동글이랑 1:1로 짝 지어줌
        instantDongle.effect = instantEffect;
        donglePool.Add(instantDongle);

        return instantDongle;
    }

    private Dongle GetDongle()
    {
        for(int i = 0; i < donglePool.Count; i++)
        {
            poolIndex = (poolIndex + 1) % donglePool.Count;
            if (!donglePool[poolIndex].gameObject.activeSelf)
            {
                return donglePool[poolIndex];
            }
        }

        // 미리 생성해둔 동글이들이 모두 활성화중(사용중)이면 makedongle 을 다시 넘겨줌
        return MakeDongle();
    }

    private void NextDongle()
    {
        if (isGameover)
        {
            return;
        }

        // 다음에 올 동글이 호출
        lastDongle = GetDongle(); ;
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);

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
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1f);

        // 최고점수갱신
        int maxScore = Mathf.Max(score, PlayerPrefs.GetInt("MaxScore"));
        PlayerPrefs.SetInt("MaxScore", maxScore);

        endGroup.SetActive(true);

        bgmPlayer.Stop();
        SfxPlay(Sfx.GameOver);
    }

    public void Reset()
    {
        SfxPlay(Sfx.Button);
        StartCoroutine(RestCoroutine());
    }

    IEnumerator RestCoroutine()
    {
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("Dongle");
    }

    public void SfxPlay(Sfx audioType)
    {
        switch(audioType) {
            case Sfx.LevelUp:
                sfxPlayer[sfxIndex].clip = sfxClip[Random.Range(0, 3)];
                break;
            case Sfx.Next:
                sfxPlayer[sfxIndex].clip = sfxClip[3];
                break;
            case Sfx.Attach:
                sfxPlayer[sfxIndex].clip = sfxClip[4];
                break;
            case Sfx.Button:
                sfxPlayer[sfxIndex].clip = sfxClip[5];
                break;
            case Sfx.GameOver:
                sfxPlayer[sfxIndex].clip = sfxClip[6];
                break;
        }

        sfxPlayer[sfxIndex].Play();
        sfxIndex = (sfxIndex + 1) % sfxPlayer.Length;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }
    private void LateUpdate()
    {
        // Update 종료 후 실행되는 생명주기 함수
        scoreText.text = "현재 점수 : " + score;
    }
}
