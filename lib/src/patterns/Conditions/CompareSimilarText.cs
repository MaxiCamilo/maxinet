namespace MaxiNet;

public record CompareSimilarText(string Text, string Match, bool DifferentiateUppercaseLetters = false)
    : ICondition, IDirectCondition
{
    public bool Execute()
    {
        return DifferentiateUppercaseLetters
            ? Text.Contains(Match)
            : Text.Contains(Match, StringComparison.InvariantCultureIgnoreCase);
    }
}