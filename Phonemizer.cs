using System.Text.RegularExpressions;

namespace g2p_dotnet;

public class Phonemizer
{
	private readonly Dictionary<string, string> _words = [];

	public Phonemizer()
	{
		string dictPath = $@"{Environment.CurrentDirectory}\Data\cmu-dictionary.txt";
		if (!File.Exists(dictPath))
		{
			throw new FileNotFoundException("The dictionary file was not found");
		}

		// Map all dictionary words to their grapheme components
		string[] lines = File.ReadAllLines(dictPath);
		foreach (string line in lines)
		{
			string[] parts = line.Split("  ", 2);
			if (parts.Length != 2)
			{
				return;
			}

			string word = parts[0].Trim();
			string[] phonemes = [.. parts[1]
				.Trim()
				.Split(' ')
				.Select(c => PhonemeSymbols.SymbolsMap[c])];
			_words[word] = string.Join("", phonemes);
		}
	}

	public string Normalize(string input)
	{
		const string pattern = @"[^ !""'\$,.:;?A-Za-z\u00a1\u00ab\u00bb\u00bf\u00e6\u00e7\u00f0\u00f8\u0127\u014b\u0153\u01c0-\u01c3\u0250-\u0268\u026a-\u0276\u0278-\u027b\u027d-\u0284\u0288-\u0292\u0294\u0295\u0298\u0299\u029b-\u029d\u029f\u02a1\u02a2\u02a4\u02a7\u02b0-\u02b2\u02b4\u02b7\u02bc\u02c8\u02cc\u02d0\u02d1\u02de\u02e0\u02e4\u0329\u03b2\u03b8\u03c7\u1d7b\u2014\u201c\u201d\u2026\u2191-\u2193\u2197\u2198\u2c71]";
		string text = Regex.Replace(input, pattern, string.Empty);

		// Replace quotes and parentheses
		text = Regex.Replace(text, "[‘’]", "'");
		text = text.Replace("«", "“").Replace("»", "”");
		text = Regex.Replace(text, "[“”]", "\"");
		text = text.Replace("(", "«").Replace(")", "»");

		// Replace uncommon punctuation
		text = text.Replace("、", ", ")
			.Replace("。", ". ")
			.Replace("！", "! ")
			.Replace("，", ", ")
			.Replace("：", ": ")
			.Replace("；", "; ")
			.Replace("？", "? ");

		// Replace abbreviations
		text = Regex.Replace(text, @"\bD[Rr]\.(?= [A-Z])", "Doctor");
		text = Regex.Replace(text, @"\b(?:Mr\.|MR\.(?= [A-Z]))", "Mister");
		text = Regex.Replace(text, @"\b(?:Ms\.|MS\.(?= [A-Z]))", "Miss");
		text = Regex.Replace(text, @"\b(?:Mrs\.|MRS\.(?= [A-Z]))", "Mrs");
		text = Regex.Replace(text, @"\betc\.(?! [A-Z])", "etc", RegexOptions.IgnoreCase);

		// Normalize casual words
		text = Regex.Replace(text, @"\b(y)eah?\b", "$1e'a", RegexOptions.IgnoreCase);

		// Normalize whitespace
		text = Regex.Replace(text, @"[^\S\n ]+", " ");
		text = Regex.Replace(text, @" {2,}", " ");
		text = Regex.Replace(text, @"(?<=\n) +(?=\n)", "");

		return text.Trim();
	}

	public string Phonemize(string input)
	{
		const string pattern = @"(\s*[\;\:\,\.\!\?\¡\¿\—\…\«\»\“\”\(\)\{\}\[\]]+\s*)+";
		Regex regex = new(pattern, RegexOptions.Compiled);
		List<(bool, string)> sections = SplitWithDelimiters(input, regex);

		List<string> converted = [];
		foreach ((bool isMatch, string chunk) in sections)
		{
			if (isMatch)
			{
				converted.Add(chunk);
			}
			else
			{
				string[] phonemes = TextToPhonemes(chunk);
				converted.Add(string.Join(" ", phonemes));
			}
		}

		Dictionary<string, string> replacements = new()
		{
			{ "kəkˈoːɹoʊ", "kˈoʊkəɹoʊ" },
			{ "kəkˈɔːɹəʊ", "kˈəʊkəɹəʊ" },
			{ "ʲ", "j" },
			{ "x", "k" },
			{ "ɬ", "l" },
			{ @"(?<=[a-zɹː])(?=hˈʌndɹɪd)", " " },
			{ @" z(?=[;:,.!?¡¿—…" + "«»“” ]|$)", "z" },
			{ @"[^\S\n]", " " },
			{ @" {2,}", " " },
			{ @"(?<=\n) +(?=\n)", "" }
		};

		string processed = string.Join("", converted);
		foreach ((string pat, string rep) in replacements)
		{
			processed = Regex.Replace(processed, pat, rep);
		}

		return processed.Trim();
	}

	private static List<(bool, string)> SplitWithDelimiters(string text, Regex regex)
	{
		List<(bool, string)> result = [];
		int prevIndex = 0;

		foreach (Match match in regex.Matches(text))
		{
			if (match.Index > prevIndex)
			{
				result.Add((false, text[prevIndex..match.Index]));
			}

			string fullMatch = match.Value;
			if (fullMatch.Length > 0)
			{
				result.Add((true, fullMatch));
			}

			prevIndex = match.Index + fullMatch.Length;
		}

		if (prevIndex < text.Length)
		{
			result.Add((false, text[prevIndex..]));
		}

		return result;
	}

	private string[] TextToPhonemes(string text)
	{
		string[] segments = Regex.Split(text, @"([\s\p{P}])");
		return [.. segments.Select(segment =>
			_words.GetValueOrDefault(segment.ToUpper(), segment))];
	}
}
