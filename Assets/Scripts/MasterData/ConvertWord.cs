using System;
using System.Collections.Generic;

[Serializable]
public class ConvertWord
{
    public string type;
    public string name;
    public float rate;

    public WordType wordType => Enum.Parse<WordType>(type);
}

public enum WordType
{
    Cheap,
    Normal,
    Expensive
}

[Serializable]
public class SelectData
{
    public List<ConvertWord> select1;
    public List<ConvertWord> select2;
}

public class ConvertResult
{
    public ConvertWord word1;
    public ConvertWord word2;
    public BasePrize basePrize;

    public string Prefix => word1?.name + word2?.name;
    public string FinalWord => word1?.name + word2?.name + basePrize.name;
}