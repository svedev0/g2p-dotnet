namespace g2p_dotnet;

public abstract class Program
{
	public static void Main(string[] args)
	{
		if (args.Length != 2)
		{
			PrintUsage();
			return;
		}
		if (args[0] != "-t")
		{
			PrintUsage();
			return;
		}
		if (string.IsNullOrEmpty(args[1]))
		{
			PrintUsage();
			return;
		}

		string text = args[1];

		Phonemizer phonemizer = new();
		string normalized = phonemizer.Normalize(text);
		string phonemized = phonemizer.Phonemize(normalized);
		Console.WriteLine(phonemized);
	}

	private static void PrintUsage()
	{
		Console.WriteLine(
			"""
			Usage: g2p_dotnet [OPTIONS]
			
			Options:
			  -t [STRING] Enter the string to convert.
			""");
	}
}
