using System.Collections.Generic;

[System.Serializable]
public class WordRelation
{
    public string word;
    public List<string> categories;
    public List<string> related;
    public float difficulty;
}

[System.Serializable]
public class WordRelationList
{
    public List<WordRelation> words;
}