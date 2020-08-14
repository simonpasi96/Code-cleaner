using System.Collections.Generic;
using UnityEngine;

public class PrivatesCleaner : CleanerModule
{
    List<int> privates = new List<int>();
    string input;


    public override void Find(string input)
    {
        shouldUpdatePreview = true;

        privates = RegexUtilities.GetMatchesIndexes(input, @"private \w+ \w+");
        this.input = input;
    }

    public override string Clean(string input)
    {
        for (int i = privates.Count - 1; i >= 0; i--)
            input = input.Remove(privates[i], "private ".Length);
        return input;
    }

    public override string GetPreview()
    {
        if (!shouldUpdatePreview)
            return preview;
        shouldUpdatePreview = false;

        preview = input;
        for (int i = privates.Count - 1; i >= 0; i--)
        {
            preview = preview.Insert(privates[i] + "private ".Length, "</color>");
            preview = preview.Insert(privates[i], "<color=red>");
        }
        return preview;
    }

    public override void DrawUI()
    {
        GUILayout.Label(privates.Count + " private(s) found.");
        if (GUILayout.Button("Clean"))
            Finalize(Clean(input));
    }
}
