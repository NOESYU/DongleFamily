using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DongleManager : MonoBehaviour
{
    // ī�װ����� ���� ����
    [Header("---------------[ Core ]")]
    public bool isGameover;
    public int score;
    public int maxLevel = 2;

    [Header("---------------[ Object Pooling ]")]
    public GameObject donglePrefab;
    public Transform dongleGroup; // ���� ������ ������ ��ġ�� �����̰� ������ �θ�
    public List<Dongle> donglePool; // ������ ������ƮǮ���� ���� ����Ʈ
    
    public GameObject effectPrefab;
    public Transform effectGroup;
    public List<ParticleSystem> effectPool; // ����Ʈ ������ƮǮ���� ���� ����Ʈ

    [Range(1, 30)] // �ν�����â���� �����ϰ� Ǯ ����� �����̴��� ���� �����ϱ� ����
    public int poolSize;
    public int poolIndex;

    public Dongle lastDongle;

    [Header("---------------[ Audio ]")]
    public AudioSource bgmPlayer;
    public AudioSource[] sfxPlayer; // ȿ������ ������ �ʰ��ϱ����� �迭�� �ְ� ä�θ�
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

        // �ְ� ���� ���� & �ҷ�����
        // PlayerPrefs : ������ ������ ����ϴ� Ŭ����
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

    // ������Ʈ Ǯ�� ����� ���� �Լ� ����
    Dongle MakeDongle()
    {
        // ����Ʈ ����
        GameObject instantEffectObj = Instantiate(effectPrefab, effectGroup);
        instantEffectObj.name = "Effect " + effectPool.Count;
        ParticleSystem instantEffect = instantEffectObj.GetComponent<ParticleSystem>();
        effectPool.Add(instantEffect);

        // ������ ����
        GameObject instantDongleObj = Instantiate(donglePrefab, dongleGroup);
        instantDongleObj.name = "Dongle " + donglePool.Count;
        Dongle instantDongle = instantDongleObj.GetComponent<Dongle>();
        instantDongle.manager = this;

        // ����Ʈ�� �����̶� 1:1�� ¦ ������
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

        // �̸� �����ص� �����̵��� ��� Ȱ��ȭ��(�����)�̸� makedongle �� �ٽ� �Ѱ���
        return MakeDongle();
    }

    private void NextDongle()
    {
        if (isGameover)
        {
            return;
        }

        // ������ �� ������ ȣ��
        lastDongle = GetDongle(); ;
        lastDongle.level = Random.Range(0, maxLevel);
        lastDongle.gameObject.SetActive(true);

        SfxPlay(Sfx.Next);

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
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(1f);

        // �ְ���������
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
        // Update ���� �� ����Ǵ� �����ֱ� �Լ�
        scoreText.text = "���� ���� : " + score;
    }
}
