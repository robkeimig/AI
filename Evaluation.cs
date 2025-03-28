using System.Text;

namespace AI;

internal class Evaluation
{
    const string dataEngSpa = "Data/ENG_SPA.tsv";
    const string dataSpaEng = "Data/SPA_ENG.tsv";

    static List<(byte[], byte[])> EnglishToSpanishTranslationData;
    static List<(byte[], byte[])> SpanishToEnglishTranslationData;

    static Evaluation()
    {
        EnglishToSpanishTranslationData = new List<(byte[], byte[])>();
        SpanishToEnglishTranslationData = new List<(byte[], byte[])>();

        foreach (var line in File.ReadAllLines(dataEngSpa))
        {
            var parts = line.Split('\t');

            if (parts.Length == 4 && !string.IsNullOrWhiteSpace(parts[1]) && !string.IsNullOrWhiteSpace(parts[3]))
            {
                EnglishToSpanishTranslationData.Add(new(Encoding.UTF8.GetBytes(parts[1]), Encoding.UTF8.GetBytes(parts[3])));
            }
        }

        foreach (var line in File.ReadAllLines(dataSpaEng))
        {
            var parts = line.Split('\t');

            if (parts.Length == 4 && !string.IsNullOrWhiteSpace(parts[1]) && !string.IsNullOrWhiteSpace(parts[3]))
            {
                SpanishToEnglishTranslationData.Add(new(Encoding.UTF8.GetBytes(parts[1]), Encoding.UTF8.GetBytes(parts[3])));
            }
        }
    }

    public static int CommonPrefixLength(Span<byte> a, Span<byte> b)
    {
        int length = Math.Min(a.Length, b.Length);
        int prefixLength = 0;

        for (int i = 0; i < length; i++)
        {
            if (a[i] == b[i])
            {
                prefixLength++;
            }
            else
            {
                break;
            }
        }

        return prefixLength;
    }

    public static Objective GetObjective(Random random)
    {
        var type = random.Next(2);

        switch (type)
        {
            case 0:
                var englishToSpanish = EnglishToSpanishTranslationData[random.Next(EnglishToSpanishTranslationData.Count)];

                return new Objective
                {
                    Input = englishToSpanish.Item1,
                    Output = englishToSpanish.Item2
                };

            case 1:
                var spanishToEnglish = SpanishToEnglishTranslationData[random.Next(SpanishToEnglishTranslationData.Count)];

                return new Objective
                {
                    Input = spanishToEnglish.Item1,
                    Output = spanishToEnglish.Item2
                };
            default:
                throw new NotImplementedException();
        }
    }
}
