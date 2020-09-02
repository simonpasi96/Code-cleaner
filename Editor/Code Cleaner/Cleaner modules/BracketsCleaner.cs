using System.Collections.Generic;
using UnityEngine;
using static RegexUtilities;

public class BracketsCleaner : CleanerModule
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

        openingBracketsIndexes = GetMatchesIndexes(input, "{");
        closingBracketsIndexes = GetMatchesIndexes(input, "}");
        semicolonIndexes = GetMatchesIndexes(input, ";");

        // Use double space before keywords so that we exclude #if and matches in comments.
        FindBracketsFollowingContainters(GetMatches(input, @"(?<!else )(?<!#)if *\("));
        FindBracketsFollowingContainters(GetMatches(input, @"(?<!#)else"));
        FindBracketsFollowingContainters(GetMatches(input, @"for *\(.*\)"));
        FindBracketsFollowingContainters(GetMatches(input, @"while"));
    }

    public override string Clean(string input)
    {
        newlineIndexes = GetMatchesIndexes(input, "\n");
        newlineIndexes.AddRange(GetMatchesIndexes(input, "\r"));
        for (int i = brackets.Count - 1; i >= 0; i--)
        {
            List<int> newlinesBeforeBracket = new List<int>();
            if (brackets[i].type == BracketType.Closing)
            {
                // (closing bracket)

                // Get newlines before the bracket.
                for (int j = 0; j < newlineIndexes.Count; j++)
                    if (newlineIndexes[j] > semicolonIndexes[brackets[i].semicolonItem] && newlineIndexes[j] < brackets[i].index)
                        newlinesBeforeBracket.Add(j);
            }
            else
            {
                // (opening bracket)

                // Get newlines before the bracket.
                for (int j = 0; j < newlineIndexes.Count; j++)
                    if (newlineIndexes[j] > brackets[i].containerIndex && newlineIndexes[j] < brackets[i].index)
                            newlinesBeforeBracket.Add(j);
            }

            // Remove the bracket.
            input = input.Remove(brackets[i].index, 1);

            // Remove the newlines before the bracket.
            for (int j = 0; j < newlinesBeforeBracket.Count; j++)
                input = input.Remove(newlineIndexes[newlinesBeforeBracket[j]], 1);
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

    void FindBracketsFollowingContainters(List<RegexMatch> containerMatches)
    {
        for (int i = 0; i < containerMatches.Count; i++)
        {
            // Find the next semicolon.
            int semicolonItem = -1;
            for (int j = 0; j < semicolonIndexes.Count && semicolonItem < 0; j++)
                if (semicolonIndexes[j] > containerMatches[i].EndIndex )
                    semicolonItem = j;

            // Find opening brackets between the container and the semicolon.
            List<int> openingBracketItems = new List<int>();
            for (int j = 0; j < openingBracketsIndexes.Count; j++)
                if (openingBracketsIndexes[j] >= containerMatches[i].EndIndex && openingBracketsIndexes[j] <= semicolonIndexes[semicolonItem])
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
                brackets.Add(new BracketPair(BracketType.Opening, openingBracketsIndexes[openingBracketItems[0]], containerMatches[i].index, semicolonItem));
                brackets.Add(new BracketPair(BracketType.Closing, closingBracketsIndexes[closingBracketItem], containerMatches[i].index, semicolonItem));
            }
        }

        // Sort result brackets.
        brackets.Sort((bA, bB) => bA.index.CompareTo(bB.index));
    }
}
