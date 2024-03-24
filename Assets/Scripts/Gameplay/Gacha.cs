using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Gacha : MonoBehaviour
{
    [SerializeField] private List<BasePrize> gachaList;
    [SerializeField] private Image gachaPlaceImage;
    [SerializeField] private Image gachaImage;
    [SerializeField] private Image prizeImage;
    [SerializeField] private Button gachaButton;

    private void Start()
    {
        DisbaleGacha();
    }

    /// <summary>
    /// ガチャを引いて、ベースとなるものを決定する
    /// </summary>
    /// <returns></returns>
    public async UniTask<BasePrize> Processing(Action<int> consumePoint)
    {
        SetUpGacha();
        await gachaButton.OnClickAsync();
        gachaButton.gameObject.SetActive(false);
        consumePoint?.Invoke(1000);

        var resultIndex = UnityEngine.Random.Range(0, gachaList.Count);
        var basePrize = gachaList[resultIndex];
        prizeImage.gameObject.SetActive(true);
        prizeImage.sprite = basePrize.image;
        await ShowLotteryAnimation();

        DisbaleGacha();
        return basePrize;
    }

    private void SetUpGacha()
    {
        gachaPlaceImage.gameObject.SetActive(true);
        gachaImage.gameObject.SetActive(true);
        gachaButton.gameObject.SetActive(true);
    }

    private void DisbaleGacha()
    {
        gachaPlaceImage.gameObject.SetActive(false);
        gachaImage.gameObject.SetActive(false);
        prizeImage.sprite = null;
        prizeImage.gameObject.SetActive(false);
        gachaButton.gameObject.SetActive(false);
    }

    private async UniTask ShowLotteryAnimation()
    {
        GameController.Instance.PlaySE(Audio.Lottery);
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        var animator = prizeImage.GetComponent<Animator>();
        animator.Play("Insert");
        await UniTask.Yield();
        await UniTask.WaitWhile(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1);
        await UniTask.Delay(TimeSpan.FromSeconds(2));
        animator.Play("Default");
    }
}
