using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WordDictionary
{
    private HashSet<string> words = new HashSet<string>();

    public void LoadFromText(string filePath)
    {
        words.Clear();

        foreach (string line in File.ReadAllLines(filePath))
        {
            string word = line.Trim().ToUpper();
            if (!string.IsNullOrEmpty(word))
                words.Add(word);
        }
    }

    public bool IsValid(string word)
    {
        return words.Contains(word.ToUpper());
    }
}
