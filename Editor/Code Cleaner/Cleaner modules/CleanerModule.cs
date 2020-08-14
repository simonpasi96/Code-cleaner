using System;

public abstract class CleanerModule
{
    protected string preview;
    protected bool shouldUpdatePreview = true;
    public Action<string> Finalize;

    public abstract void Find(string input);
    public abstract string Clean(string input);
    public abstract string GetPreview();
    public abstract void DrawUI();
}
