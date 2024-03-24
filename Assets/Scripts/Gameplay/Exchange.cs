using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Exchange : MonoBehaviour
{
    [SerializeField] private List<SpecialRate> specialRates;
    [SerializeField] private Image exchangePlaceImage;
    [SerializeField] private Image prizeImage;
    [SerializeField] private TMP_Text finalPrizeWord;
    [SerializeField] private Image pointImage;
    [SerializeField] private Button exchangeButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button resultButton;

    private void Start()
    {
        DisableExchange();
    }

    /// <summary>
    /// 加工後のものを渡してポイントを得る
    /// 渡すときにアニメーション
    /// </summary>
    /// <returns></returns>
    public async UniTask<bool> Processing(ConvertResult converted, Action<int> addPoint)
    {
        SetUpExchange(converted);
        await exchangeButton.OnClickAsync();
        exchangeButton.gameObject.SetActive(false);
        GameController.Instance.PlaySE(Audio.Exchange);

        finalPrizeWord.gameObject.SetActive(false);
        pointImage.gameObject.SetActive(true);
        await ShowExchangeAnimation();
        prizeImage.gameObject.SetActive(false);

        var special = specialRates.Find(s => s.combination == converted.Prefix);
        var rate= special?.rate ?? (converted.word1?.rate ?? 1 * converted.word2?.rate ?? 1);
        var resultPoint = Mathf.RoundToInt(converted.basePrize.price * rate);

        addPoint?.Invoke(resultPoint);
        restartButton.gameObject.SetActive(true);
        resultButton.gameObject.SetActive(true);

        var isRestart = await UniTask.WhenAny(restartButton.OnClickAsync(), resultButton.OnClickAsync());
        DisableExchange();
        return isRestart == 0;
    }

    private void SetUpExchange(ConvertResult result)
    {
        prizeImage.sprite = result.basePrize.image;
        prizeImage.gameObject.SetActive(true);
        finalPrizeWord.text = result.FinalWord;
        finalPrizeWord.gameObject.SetActive(true);
        exchangePlaceImage.gameObject.SetActive(true);
        exchangeButton.gameObject.SetActive(true);
    }

    private void DisableExchange()
    {
        prizeImage.sprite = null;
        prizeImage.gameObject.SetActive(false);
        pointImage.gameObject.SetActive(false);
        finalPrizeWord.text = "";
        finalPrizeWord.gameObject.SetActive(false);
        exchangePlaceImage.gameObject.SetActive(false);
        exchangeButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        resultButton.gameObject.SetActive(false);
    }

    private async UniTask ShowExchangeAnimation()
    {
        var prizeAnimator = prizeImage.GetComponent<Animator>();
        var pointAnimator = pointImage.GetComponent<Animator>();
        prizeAnimator.Play("ScaleIn");
        pointAnimator.Play("Hide");
        await UniTask.Yield();
        await UniTask.WaitWhile(() => prizeAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        // Play 換金SE
        prizeAnimator.Play("Hide");
        pointAnimator.Play("ScaleOut");
        await UniTask.Yield();
        await UniTask.WaitWhile(() => pointAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        await UniTask.Delay(TimeSpan.FromSeconds(1));
    }
}