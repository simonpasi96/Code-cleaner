using System.Collections.Generic;
using UnityEngine;

public class BracketsCleaner: CleanerModule
{
    List<int> openingBracketsIndexes = new List<int>();
    List<int> closingBracketsIndexes = new List<int>();
    List<int> semicolonIndexes = new List<int>();
    List<int> newlineIndexes = new List<int>();

    public struct BracketPair
    {
        public BracketType type;
        public int index;
        public int containerIndex;
        public int semicolonItem;

        public BracketPair(BracketType type, int index, int containerIndex, int semicolonItem)
        {
            this.type = type;
            this.index = index;
            this.containerIndex = containerIndex;
            this.semicolonItem = semicolonItem;
        }
    }
    public enum BracketType { Opening, Closing }
    public List<BracketPair> brackets = new List<BracketPair>();
    string input;

    public override void Find(string input)
    {
        shouldUpdatePreview = true;

        this.input = input;
        brackets.Clear();

        openingBracketsIndexes = RegexUtilities.GetMatchesIndexes(input, "{");
        closingBracketsIndexes = RegexUtilities.GetMatchesIndexes(input, "}");
        semicolonIndexes = RegexUtilities.GetMatchesIndexes(input, ";");

        // Use double space before keywords so that we exclude #if and matches in comments.
        FindBracketsFollowingIndexes(RegexUtilities.GetMatchesIndexes(input, @"  if"));
        FindBracketsFollowingIndexes(RegexUtilities.GetMatchesIndexes(input, @"  else"));
    }

    public override string Clean(string input)
    {
        newlineIndexes = RegexUtilities.GetMatchesIndexes(input, "\n");
        newlineIndexes.AddRange(RegexUtilities.GetMatchesIndexes(input, "\r"));
        for (int i = brackets.Count - 1; i >= 0; i--)
        {
            if (brackets[i].type == BracketType.Opening)
            {
                List<int> newlinesBeforeBracket = new List<int>();
                List<int> newLinesAfterBracket = new List<int>();
                for (int j = 0; j < newlineIndexes.Count; j++)
                    if (newlineIndexes[j] > brackets[i].containerIndex && newlineIndexes[j] < semicolonIndexes[brackets[i].semicolonItem])
                    {
                        if (newlineIndexes[j] > brackets[i].index)
                            newLinesAfterBracket.Add(j);
                        else
                            newlinesBeforeBracket.Add(j);
                    }
                input = input.Remove(brackets[i].index, 1);
                if (newlinesBeforeBracket.Count > 0 && newLinesAfterBracket.Count > 0)
                {
                    for (int j = newLinesAfterBracket.Count - 1; j >= 0; j--)
                        input = input.Remove(newlineIndexes[newLinesAfterBracket[j]], 1);
                }
            }
            else
            {
                input = input.Remove(brackets[i].index, 1);

                List<int> newLinesBetweenSemicolonAndBracket = new List<int>();
                for (int j = 0; j < newlineIndexes.Count; j++)
                    if (newlineIndexes[j] > semicolonIndexes[brackets[i].semicolonItem] && newlineIndexes[j] < brackets[i].index)
                        newLinesBetweenSemicolonAndBracket.Add(j);
                for (int j = newLinesBetweenSemicolonAndBracket.Count - 1; j >= 0; j--)
                    input = input.Remove(newlineIndexes[newLinesBetweenSemicolonAndBracket[j]], 1);
            }
        }
        return input;
    }

    public override string GetPreview()
    {
        if (!shouldUpdatePreview)
            return preview;
        shouldUpdatePreview = false;

        preview = input;
        for (int i = brackets.Count - 1; i >= 0; i--)
        {
            preview = preview.Insert(brackets[i].index + 1, "</color>");
            preview = preview.Insert(brackets[i].index, "<color=red>");
        }
        return preview;
    }

    public override void DrawUI()
    {
        GUILayout.Label(brackets.Count + " optional bracket(s) found.");
        if (GUILayout.Button("Clean"))
            Finalize(Clean(input));
    }

    void FindBracketsFollowingIndexes(List<int> indexes)
    {
        // Find ifs with single-line brackets.
        for (int i = 0; i < indexes.Count; i++)
        {
            // Find the next semicolon.
            int semicolonItem = -1;
            for (int j = 0; j < semicolonIndexes.Count && semicolonItem < 0; j++)
                if (semicolonIndexes[j] > indexes[i])
                    semicolonItem = j;

            // Find all opening brackets between the if and the semicolon.
            List<int> openingBracketItems = new List<int>();
            for (int j = 0; j < openingBracketsIndexes.Count; j++)
                if (openingBracketsIndexes[j] >= indexes[i] && openingBracketsIndexes[j] <= semicolonIndexes[semicolonItem])
                    openingBracketItems.Add(j);

            // Didn't find an unique opening bracket, continue.
            if (openingBracketItems.Count != 1)
                continue;

            // Find the next closing bracket.
            int closingBracketItem = -1;
            for (int j = 0; j < closingBracketsIndexes.Count && closingBracketItem < 0; j++)
                if (closingBracketsIndexes[j] > semicolonIndexes[semicolonItem])
                    closingBracketItem = j;

            // If the next semicolon isn't between the two brackets, add these brackets.
            int nextSemicolonItem = semicolonIndexes.Count - 1 > semicolonItem ? semicolonItem + 1 : -1;
            if (nextSemicolonItem < 0 || semicolonIndexes[nextSemicolonItem] > closingBracketsIndexes[closingBracketItem])
            {
                brackets.Add(new BracketPair(BracketType.Opening, openingBracketsIndexes[openingBracketItems[0]], indexes[i], semicolonItem));
                brackets.Add(new BracketPair(BracketType.Closing, closingBracketsIndexes[closingBracketItem], indexes[i], semicolonItem));
            }
        }

        // Sort result brackets.
        brackets.Sort((bA, bB) => bA.index.CompareTo(bB.index));
    }
}
