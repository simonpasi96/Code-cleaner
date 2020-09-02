using System.Collections.Generic;
using static RegexUtilities;

public class GroupFinder
{
    public List<Group> groups = new List<Group>();
    List<RegexMatch> openings = new List<RegexMatch>();
    List<RegexMatch> closings = new List<RegexMatch>();
    readonly string opening, closing;

    public struct Group
    {
        public RegexMatch opening, closing;

        public Group(RegexMatch opening, RegexMatch closing)
        {
            this.opening = opening;
            this.closing = closing;
        }
    }


    public GroupFinder(string opening, string closing)
    {
        this.opening = opening;
        this.closing = closing;
    }

    public void Find(string input)
    {
        groups.Clear();
        openings = RegexUtilities.GetMatches(input, opening);
        closings = RegexUtilities.GetMatches(input, closing);

        // For each closing, find the closest opening.
        for (int i = 0; i < closings.Count; i++)
        {
            bool openingFound = false;
            for (int j = 0; j < openings.Count && !openingFound; j++)
                if (openings[j].index > closings[i].index || j >= openings.Count - 1)
                {
                    openingFound = true;
                    groups.Add(new Group(openings[openings[j].index > closings[i].index ? j -1 : j], closings[i]));
                }
        }
    }

    public List<RegexMatch> GetMatches()
    {
        List<RegexMatch> result = new List<RegexMatch>(openings);
        result.AddRange(closings);
        result.Sort((bA, bB) => bA.index.CompareTo(bB.index));
        return result;
    }
}
