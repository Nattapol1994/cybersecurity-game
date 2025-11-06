using System.Collections.Generic;

[System.Serializable]
public class WifiConfig
{
    public List<string> wifiNames;
    public int minFakeCount;
    public int fakePerDifficultyStep;
}