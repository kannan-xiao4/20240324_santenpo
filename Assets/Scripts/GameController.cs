using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private static GameController s_instance;
    public static GameController Instance => s_instance;

    [Header("System Component")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource seAudioSource;
    [SerializeField] private List<SystemSound> soundList;
    [SerializeField] private InputController inputController;
    [SerializeField] private Gacha gacha;
    [SerializeField] private Convert convert;
    [SerializeField] private Exchange exchange;

    [Space, Header("Title UI")]
    [SerializeField] private Canvas titleCanvas;
    [SerializeField] private Button startGameButton;

    [Space, Header("InGame UI")]
    [SerializeField] private Canvas inGameCanvas;
    [SerializeField] private TMP_Text ingameRemaingText;

    [Space, Header("Result UI")]
    [SerializeField] private Canvas resultCanvas;
    [SerializeField] private Button gotoTitleButton;
    [SerializeField] private Button restartGameButton;
    [SerializeField] private TMP_Text resultRemaingText;
    [SerializeField] private TMP_Text resultText;

    // 現在の所持金
    private readonly AsyncReactiveProperty<int> currentPoint = new(5000);
    // 開始時の所持金
    private int initializPoint = 0;

    private void Awake()
    {
        if (s_instance != this && s_instance != null)
        {
            Destroy(s_instance.gameObject);
        }

        s_instance = this;

        startGameButton.onClick.AddListener(() =>
        {
            initializPoint = currentPoint.Value;
            PlaySE(Audio.Enter);
            GameLoop().Forget();
        });
        restartGameButton.onClick.AddListener(() =>
        {
            initializPoint = currentPoint.Value;
            PlaySE(Audio.Enter);
            GameLoop().Forget();
        });
        gotoTitleButton.onClick.AddListener(() =>
        {
            titleCanvas.gameObject.SetActive(true);
            inGameCanvas.gameObject.SetActive(false);
            resultCanvas.gameObject.SetActive(false);
        });

        currentPoint.BindTo(monoBehaviour: this, bindAction: (parent, point) => ingameRemaingText.text = $"所持金 {point} 円");
    }

    private async UniTaskVoid GameLoop()
    {
        titleCanvas.gameObject.SetActive(false);
        inGameCanvas.gameObject.SetActive(true);
        resultCanvas.gameObject.SetActive(false);

        bgmAudioSource.volume = 0.5f;

        // ガチャを回す
        var gachaResult = await gacha.Processing(consumePoint: (usePoint) =>
        {
            PlaySE(Audio.ChangePoint);
            currentPoint.Value -= usePoint;
        });

        bgmAudioSource.volume = 0.1f;

        // 加工結果を得る
        var convertResult = await convert.Processing(gachaResult, consumePoint: (usePoint) =>
        {
            PlaySE(Audio.ChangePoint);
            currentPoint.Value -= usePoint;
        });

        bgmAudioSource.volume = 0.05f;

        // 換金結果を得る
        var isRestart = await exchange.Processing(convertResult, addPoint: (addPoint) =>
        {
            PlaySE(Audio.ChangePoint);
            currentPoint.Value += addPoint;
        });


        if (isRestart)
        {
            GameLoop().Forget();
            return;
        }

        titleCanvas.gameObject.SetActive(false);
        inGameCanvas.gameObject.SetActive(false);
        resultCanvas.gameObject.SetActive(true);
        resultRemaingText.text = $"残りの所持金 {currentPoint.Value} 円";
        var diffFromInitial = currentPoint.Value - initializPoint;
        if (diffFromInitial > 0)
        {
            resultText.text = $"+{Mathf.Abs(diffFromInitial)}円の勝ち！";
        }
        else if (diffFromInitial == 0)
        {
            resultText.text = $"差し引きゼロなので実質勝ちやな";
        }
        else
        {
            resultText.text = $"-{Mathf.Abs(diffFromInitial)}円の負け。。。";
        }
    }

    public void PlaySE(Audio type)
    {
        var sound = soundList.Find(s => s.type == type);
        if (sound != null)
        {
            seAudioSource.PlayOneShot(sound.clip);
        }
    }
}