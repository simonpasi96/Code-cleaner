using System.Collections.Generic;
using UnityEngine;

public class NewlinesCleaner : CleanerModule
{
    List<RegexUtilities.RegexMatch> matches = new List<RegexUtilities.RegexMatch>();
    string input;


    public override void Find(string input)
    {
        this.input = input;
        matches = RegexUtilities.GetMatches(input, @"(\r*\n *){3,}");
        for (int i = 0; i < matches.Count; i++)
        {
            bool matchDeleted = false;
            for (int j = 0; j < matches.Count && !matchDeleted; j++)
            {
                if (i == j)
                    continue;
                if (matches[i].index >= matches[j].index && matches[i].value.Length < matches[j].value.Length)
                {
                    matches.RemoveAt(i);
                    matchDeleted = true;
                }
            }
        }
        shouldUpdatePreview = true;
    }

    public override string Clean(string input)
    {
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            input = input.Remove(matches[i].index, matches[i].value.Length);
            input = input.Insert(matches[i].index, "\r\n\r\n");
        }
        return input;
    }

    public override void DrawUI()
    {
        GUILayout.Label(matches.Count + " newline group(s) found.");
        if (GUILayout.Button("Clean"))
            Finalize(Clean(input));
    }

    public override string GetPreview()
    {
        if (!shouldUpdatePreview)
            return preview;
        shouldUpdatePreview = false;

        preview = input;
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            preview = preview.Insert(matches[i].index + matches[i].value.Length, "←]</color>");
            preview = preview.Insert(matches[i].index, "<color=red>[→");
        }
        return preview;
    }
}
