using System;
using System.Text;

namespace MaxiNet;

public record Oration(string Message, IReadOnlyList<string>? Parts = null)
{
    public IReadOnlyList<string> Parts { get; init; } = Parts ?? [];

    public override string ToString()
    {        
        if (Parts.Count == 0)
        {
            return Message;
        }

        var buffer = new StringBuilder();
        var partIndex = 0;

        for (int i = 0; i < Message.Length; i++)
        {
            var character = Message[i];
            if (character == '?' && (i == 0 || Message[i - 1] != '/'))
            {
                if (partIndex < Parts.Count)
                {
                    buffer.Append(Parts[partIndex]);
                    partIndex++;
                    
                    continue;
                }

            }

            buffer.Append(character);
        }

        return buffer.ToString();
    }
}

