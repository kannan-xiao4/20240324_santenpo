using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private static GameController s_instance;
    public static GameController Instance => s_instance;

    [Header("System Component")]
    [SerializeField] private float limitTimeSeconds;
    [SerializeField] private float countingSeconds;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private List<AudioClip> clipList;
    [SerializeField] private InputController inputController;

    [Space, Header("Title UI")]
    [SerializeField] private Canvas titleCanvas;
    [SerializeField] private Button startGameButton;

    [Space, Header("InGame UI")]
    [SerializeField] private Canvas inGameCanvas;
    [SerializeField] private TMP_Text loadingPresentTotalText;
    [SerializeField] private TMP_Text loadingPresentCountText;
    [SerializeField] private TMP_Text animationText;
    [SerializeField] private TMP_Text remaingTimeText;

    [Space, Header("Result UI")]
    [SerializeField] private Canvas resultCanvas;
    [SerializeField] private Button gotoTitleButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private TMP_Text resultText;

    private float currentTime = 0f;
    private int completeCount = 0;

    private void Awake()
    {
        if (s_instance != this && s_instance != null)
        {
            Destroy(s_instance.gameObject);
        }

        s_instance = this;

        startGameButton.onClick.AddListener(() => { GameLoop().Forget(); });
        restartGameButton.onClick.AddListener(() => { GameLoop().Forget(); });
        gotoTitleButton.onClick.AddListener(() =>
        {
            titleCanvas.gameObject.SetActive(true);
            inGameCanvas.gameObject.SetActive(false);
            resultCanvas.gameObject.SetActive(false);
        });
    }

    private async UniTaskVoid GameLoop()
    {
        titleCanvas.gameObject.SetActive(false);
        inGameCanvas.gameObject.SetActive(true);
        resultCanvas.gameObject.SetActive(false);

        currentTime = limitTimeSeconds;
        completeCount = 0;
        remaingTimeText.text = currentTime.ToString("F2");

        var cancelSource = new CancellationTokenSource();

        while (currentTime > 0f)
        {
            await UniTask.Yield();
            currentTime -= Time.deltaTime;
            remaingTimeText.text = currentTime.ToString("F2");
        }
        cancelSource.Cancel();
        cancelSource.Dispose();

        titleCanvas.gameObject.SetActive(false);
        inGameCanvas.gameObject.SetActive(false);
        resultCanvas.gameObject.SetActive(true);
        resultText.text = completeCount.ToString();
    }

    public void PlaySE()
    {
        var sound = clipList[Random.Range(0, clipList.Count)];
        if (sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }

    public void PlayTextAnimation(string text, string stateName)
    {
        var animator = animationText.GetComponent<Animator>();
        animationText.text = text;
        animator.Rebind();
        animator.Play(stateName, 0, 0);
    }
}