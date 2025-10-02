
using UnityEngine;

public static class TextUtils
{
    // Scramble SSID while preserving original casing
    public static string ScrambleSSID(string original)
    {
        if (string.IsNullOrEmpty(original))
            return original;

        // Mapping letters to lookalike numbers
        var map = new System.Collections.Generic.Dictionary<char, char>()
        {
            {'O','0'}, {'E','3'}, {'I','1'}, {'A','4'}, {'S','5'}
        };

        char[] chars = original.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char upper = char.ToUpper(chars[i]); // normalize for lookup
            if (map.ContainsKey(upper) && Random.value > 0.5f)
            {
                chars[i] = map[upper]; // apply substitution while preserving original casing
            }
        }

        return new string(chars);
    }
}
