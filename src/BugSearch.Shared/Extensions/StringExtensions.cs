using System.Text;
using System.Globalization;

namespace BugSearch.Shared.Extensions;

public static class StringExtensions
{
    public static string NormalizeComplex(this string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        string normalizedString = input.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC).ToLower();
    }

    public static bool ContainsNormalized(this string? input, string? value)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
            return false;

        return input.NormalizeComplex().Contains(value.NormalizeComplex());
    }

    public static bool Contains(this string? input, string? value)
    {
        if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(value))
            return false;

        return input.NormalizeComplex().Contains(value.NormalizeComplex());
    }

    public static int LevenshteinDistance(this string? word1, string word2)
    {
        if (string.IsNullOrEmpty(word1))
            return 0;

        int[,] dp = new int[word1.Length + 1, word2.Length + 1];

        for (int i = 0; i <= word1.Length; i++)
        {
            dp[i, 0] = i;
        }

        for (int j = 0; j <= word2.Length; j++)
        {
            dp[0, j] = j;
        }

        for (int i = 1; i <= word1.Length; i++)
        {
            for (int j = 1; j <= word2.Length; j++)
            {
                int cost = (word1[i - 1] == word2[j - 1]) ? 0 : 1;

                dp[i, j] = Math.Min(Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1), dp[i - 1, j - 1] + cost);
            }
        }

        return dp[word1.Length, word2.Length];
    }

    public static int LevenshteinDistance(this Dictionary<string, double>? dict, string word2)
    {
        if (dict == null)
            return 0;

        int minDistance = int.MaxValue;

        foreach (KeyValuePair<string, double> pair in dict)
        {
            int distance = LevenshteinDistance(pair.Key, word2);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }
    

    public static string FindClosestTerm(string term, List<string> dictionary)
    {
        string closestTerm = string.Empty;
        int minDistance = int.MaxValue;

        foreach (string dictTerm in dictionary)
        {
            int distance = LevenshteinDistance(term, dictTerm);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestTerm = dictTerm;
            }
        }

        return closestTerm;
    }

    public static int CalculateWordDistance(this string sentence, string word1, string word2)
    {
        string[] words = sentence.Split(' ');

        int word1Index = -1;
        int word2Index = -1;

        for (int i = 0; i < words.Length; i++)
        {
            if (words[i] == word1)
            {
                word1Index = i;
            }
            else if (words[i] == word2)
            {
                word2Index = i;
            }

            if (word1Index != -1 && word2Index != -1)
            {
                break;
            }
        }

        if (word1Index == -1 || word2Index == -1)
        {
            return -1;
        }

        int distance = Math.Abs(word1Index - word2Index);

        return distance;
    }

    public static double CalculateMeanWordDistance(this string? sentence, string[] words)
    {
        if (string.IsNullOrEmpty(sentence))
            return -1;
        
        string[] sentenceWords = sentence.Split(' ');

        int[] wordIndices = new int[words.Length];
        for (int i = 0; i < words.Length; i++)
        {
            int wordIndex = Array.IndexOf(sentenceWords, words[i]);
            if (wordIndex == -1)
            {
                return -1;
            }
            wordIndices[i] = wordIndex;
        }

        double meanDistance = wordIndices.SelectMany((x, i) => wordIndices.Skip(i + 1), (x, y) => Math.Abs(x - y)).Average();

        return meanDistance;
    }

    public static double CalculateTFIDF(this string? sentence, string[] words, Dictionary<string, double> idfDictionary)
    {
        if (string.IsNullOrEmpty(sentence))
            return -1;

        string[] sentenceWords = sentence.Split(' ');

        double[] tf = new double[words.Length];
        for (int i = 0; i < words.Length; i++)
        {
            int wordCount = sentenceWords.Count(x => x == words[i]);
            tf[i] = (double)wordCount / sentenceWords.Length;
        }

        double tfidf = 0;
        for (int i = 0; i < words.Length; i++)
        {
            tfidf += tf[i] * idfDictionary[words[i]];
        }

        return tfidf;
    }

    public static double CalculateTFIDF(this string? sentence, string[] words)
    {
        if (string.IsNullOrEmpty(sentence))
            return -1;

        string[] sentenceWords = sentence.Split(' ');

        double[] tf = new double[words.Length];
        for (int i = 0; i < words.Length; i++)
        {
            int wordCount = sentenceWords.Count(x => x == words[i]);
            tf[i] = (double)wordCount / sentenceWords.Length;
        }

        double tfidf = 0;
        for (int i = 0; i < words.Length; i++)
        {
            tfidf += tf[i];
        }

        return tfidf;
    }

    public static double CalculateTFIDFLog(this string? sentence, string[] words)
    {
        if (string.IsNullOrEmpty(sentence))
            return -1;

        string[] sentenceWords = sentence.Split(' ');

        double[] tf = new double[words.Length];
        for (int i = 0; i < words.Length; i++)
        {
            int wordCount = sentenceWords.Count(x => x == words[i]);
            tf[i] = (double)wordCount / sentenceWords.Length;
        }

        double tfidf = 0;
        for (int i = 0; i < words.Length; i++)
        {
            tfidf += Math.Log(tf[i] + 1);
        }

        return tfidf;
    }

    public static double CalculateTFIDFLog(this string? sentence, string[] words, Dictionary<string, double>? idfDictionary)
    {
        if (string.IsNullOrEmpty(sentence) || idfDictionary is null)
            return -1;

        string[] sentenceWords = sentence.Split(' ');

        double[] tf = new double[words.Length];
        for (int i = 0; i < words.Length; i++)
        {
            int wordCount = sentenceWords.Count(x => x == words[i]);
            tf[i] = (double)wordCount / sentenceWords.Length;
        }

        double tfidf = 0;
        for (int i = 0; i < words.Length; i++)
        {
            tfidf += Math.Log(tf[i] + 1) * idfDictionary[words[i]];
        }

        return tfidf;
    }


    public static Dictionary<string, double> CalculateTFIDFLogDict(this string? sentence, string[] words)
    {
        if (string.IsNullOrEmpty(sentence))
            return new Dictionary<string, double>();

        string[] sentenceWords = sentence.Split(' ');

        double[] tf = new double[words.Length];
        for (int i = 0; i < words.Length; i++)
        {
            int wordCount = sentenceWords.Count(x => x == words[i]);
            tf[i] = (double)wordCount / sentenceWords.Length;
        }

        Dictionary<string, double> tfidfDict = new Dictionary<string, double>();
        for (int i = 0; i < words.Length; i++)
        {
            tfidfDict.Add(words[i], Math.Log(tf[i] + 1));
        }

        return tfidfDict;
    }
}