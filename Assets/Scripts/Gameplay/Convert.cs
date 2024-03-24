using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Convert : MonoBehaviour
{
    [Header("Convert word data")]
    [SerializeField] private TextAsset dataJson;
    [SerializeField] private List<ConvertWord> cheapNames1;
    [SerializeField] private List<ConvertWord> normalNames1;
    [SerializeField] private List<ConvertWord> expensiveNames1;
    [SerializeField] private List<ConvertWord> cheapNames2;
    [SerializeField] private List<ConvertWord> normalNames2;
    [SerializeField] private List<ConvertWord> expensiveNames2;

    [Header("Convert Scene UI")]
    [SerializeField] private Image convertPlaceImage;
    [SerializeField] private TMP_Text selectConvertText1;
    [SerializeField] private TMP_Text selectConvertText2;
    [SerializeField] private TMP_Text basePrizeText;
    [SerializeField] private GameObject convertedTextParent;
    [SerializeField] private Image basePrizeImage;
    [SerializeField] private TMP_Text cheapConvertPriceText;
    [SerializeField] private TMP_Text normalConvertPriceText;
    [SerializeField] private TMP_Text expensiveConvertPriceText;
    [SerializeField] private GameObject convertPriceTextParent;
    [SerializeField] private ToggleGroup select1ToggleGroup;
    [SerializeField] private ToggleGroup select2ToggleGroup;
    [SerializeField] private List<Toggle> select1CheapToggles;
    [SerializeField] private List<Toggle> select1NormalToggles;
    [SerializeField] private List<Toggle> select1ExpensiveToggles;
    [SerializeField] private List<Toggle> select2CheapToggles;
    [SerializeField] private List<Toggle> select2NormalToggles;
    [SerializeField] private List<Toggle> select2ExpensiveToggles;
    [SerializeField] private Button decideButton;

    private AsyncReactiveProperty<ConvertWord> _select1 = new(null);
    private AsyncReactiveProperty<ConvertWord> _select2 = new(null);

    const string DefaultConvertText = "_ _ _ _ _";

    private void Start()
    {
        string jsonString = dataJson.text;
        SelectData data = JsonUtility.FromJson<SelectData>(jsonString);
        cheapNames1 = data.select1.Where(word => word.wordType == WordType.Cheap).ToList();
        normalNames1 = data.select1.Where(word => word.wordType == WordType.Normal).ToList();
        expensiveNames1 = data.select1.Where(word => word.wordType == WordType.Expensive).ToList();
        cheapNames2 = data.select2.Where(word => word.wordType == WordType.Cheap).ToList();
        normalNames2 = data.select2.Where(word => word.wordType == WordType.Normal).ToList();
        expensiveNames2 = data.select2.Where(word => word.wordType == WordType.Expensive).ToList();

        _select1.BindTo(monoBehaviour: this, bindAction: (parent, value) => { selectConvertText1.text = value?.name ?? DefaultConvertText; });
        _select2.BindTo(monoBehaviour: this, bindAction: (parent, value) => { selectConvertText2.text = value?.name ?? DefaultConvertText; });

        DisableConvert();
    }

    public async UniTask<ConvertResult> Processing(BasePrize basePrize, Action<int> consumePoint)
    {
        SeupConvert(basePrize);

        await decideButton.OnClickAsync();
        decideButton.gameObject.SetActive(false);
        GameController.Instance.PlaySE(Audio.Convert);

        var result = new ConvertResult()
        {
            word1 = _select1,
            word2 = _select2,
            basePrize = basePrize
        };
        consumePoint?.Invoke(CalculateConsumePoint(_select1) + CalculateConsumePoint(_select2));

        select1ToggleGroup.gameObject.SetActive(false);
        select2ToggleGroup.gameObject.SetActive(false);
        convertPriceTextParent.SetActive(false);

        await UniTask.Delay(TimeSpan.FromSeconds(1));

        DisableConvert();
        return result;
    }

    private void SeupConvert(BasePrize basePrize)
    {
        var selectTargetWords1 = new List<ConvertWord>();
        var selectTargetWords2 = new List<ConvertWord>();

        var copyCheap1 = new List<ConvertWord>(cheapNames1);
        var copyNormal1 = new List<ConvertWord>(normalNames1);
        var copyExpensive1 = new List<ConvertWord>(expensiveNames1);
        var copyCheap2 = new List<ConvertWord>(cheapNames2);
        var copyNormal2 = new List<ConvertWord>(normalNames2);
        var copyExpensive2 = new List<ConvertWord>(expensiveNames2);

        for (int i = 0; i < copyCheap1.Count; i++)
        {
            var index = UnityEngine.Random.Range(0, copyCheap1.Count);
            selectTargetWords1.Add(copyCheap1[index]);
            copyCheap1.RemoveAt(index);

            index = UnityEngine.Random.Range(0, copyNormal1.Count);
            selectTargetWords1.Add(copyNormal1[index]);
            copyNormal1.RemoveAt(index);

            index = UnityEngine.Random.Range(0, copyExpensive1.Count);
            selectTargetWords1.Add(copyExpensive1[index]);
            copyExpensive1.RemoveAt(index);

            index = UnityEngine.Random.Range(0, copyCheap2.Count);
            selectTargetWords2.Add(copyCheap2[index]);
            copyCheap2.RemoveAt(index);

            index = UnityEngine.Random.Range(0, copyNormal2.Count);
            selectTargetWords2.Add(copyNormal2[index]);
            copyNormal2.RemoveAt(index);

            index = UnityEngine.Random.Range(0, copyExpensive2.Count);
            selectTargetWords2.Add(copyExpensive2[index]);
            copyExpensive2.RemoveAt(index);
        }

        selectTargetWords1.Sort((a, b) => a.type.CompareTo(b.type));
        selectTargetWords2.Sort((a, b) => a.type.CompareTo(b.type));

        foreach (var (value, index) in selectTargetWords1.Select((value, index) => (value, index)))
        {
            var toggle = value.wordType switch
            {
                WordType.Cheap => select1CheapToggles[index % select1CheapToggles.Count],
                WordType.Normal => select1NormalToggles[index % select1NormalToggles.Count],
                WordType.Expensive => select1ExpensiveToggles[index % select1ExpensiveToggles.Count],
                _ => null
            };

            toggle.GetComponentInChildren<TMP_Text>().text = value.name;
            toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    _select1.Value = value;
                }

                if (!isOn)
                {
                    var anyToggleIsOn = select1CheapToggles.Concat(select1NormalToggles).Concat(select1ExpensiveToggles).Any(t => t.isOn);
                    if (!anyToggleIsOn)
                    {
                        _select1.Value = null;
                    }
                }
            });
        }

        foreach (var (value, index) in selectTargetWords2.Select((value, index) => (value, index)))
        {
            var toggle = value.wordType switch
            {
                WordType.Cheap => select2CheapToggles[index % select2CheapToggles.Count],
                WordType.Normal => select2NormalToggles[index % select1NormalToggles.Count],
                WordType.Expensive => select2ExpensiveToggles[index % select2ExpensiveToggles.Count],
                _ => null
            };

            toggle.GetComponentInChildren<TMP_Text>().text = value.name;
            toggle.onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    _select2.Value = value;
                }

                if (!isOn)
                {
                    var anyToggleIsOn = select2CheapToggles.Concat(select2NormalToggles).Concat(select2ExpensiveToggles).Any(t => t.isOn);
                    if (!anyToggleIsOn)
                    {
                        _select2.Value = null;
                    }
                }
            });
        }

        basePrizeText.text = basePrize.name;
        convertedTextParent.SetActive(true);

        basePrizeImage.sprite = basePrize.image;
        basePrizeImage.gameObject.SetActive(true);

        convertPriceTextParent.SetActive(true);

        select1ToggleGroup.gameObject.SetActive(true);
        select2ToggleGroup.gameObject.SetActive(true);
        select1ToggleGroup.SetAllTogglesOff();
        select2ToggleGroup.SetAllTogglesOff();

        decideButton.gameObject.SetActive(true);
        convertPlaceImage.gameObject.SetActive(true);
    }

    private void DisableConvert()
    {
        _select1.Value = null;
        _select2.Value = null;

        foreach (var toggle in select1CheapToggles
            .Concat(select1NormalToggles)
            .Concat(select1ExpensiveToggles)
            .Concat(select2CheapToggles)
            .Concat(select2NormalToggles)
            .Concat(select2ExpensiveToggles))
        {
            toggle.GetComponentInChildren<TMP_Text>().text = "";
            toggle.onValueChanged.RemoveAllListeners();
        }

        basePrizeText.text = "";
        convertedTextParent.SetActive(false);

        basePrizeImage.sprite = null;
        basePrizeImage.gameObject.SetActive(false);

        convertPriceTextParent.SetActive(false);

        select1ToggleGroup.SetAllTogglesOff();
        select2ToggleGroup.SetAllTogglesOff();
        select1ToggleGroup.gameObject.SetActive(false);
        select2ToggleGroup.gameObject.SetActive(false);

        decideButton.gameObject.SetActive(false);

        convertPlaceImage.gameObject.SetActive(false);
    }

    private int CalculateConsumePoint(ConvertWord word)
    {
        if (word == null)
        {
            return 0;
        }

        if (cheapNames1.Concat(cheapNames2).Contains(word))
        {
            return 10;
        }
        else if (normalNames1.Concat(normalNames2).Contains(word))
        {
            return 100;
        }
        else if (expensiveNames1.Concat(expensiveNames2).Contains(word))
        {
            return 1000;
        }
        return 0;
    }
}