using System.Text;

namespace CodebookGenerator;

public class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || args[0] == "" || args[0] == "/?")
        {
            string filename = Path.GetFileName(Environment.GetCommandLineArgs()[0]);
            Console.WriteLine($@"
{filename} <mensagem> [texto] [/?]

Nomes de arquivo podem ser especificados
Uma cadeia de caracteres aleatória será gerada se o argumento de texto não for especificado
".Trim());

            return 0;
        }

        string hidden = args[0];

        if (File.Exists(args[0]))
        {
            FileInfo file = new(args[0]);

            Console.Write("Lendo arquivo: ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(file.FullName);
            Console.ResetColor();

            StreamReader reader = file.OpenText();
            hidden = reader.ReadToEnd();
            reader.Close();
        }

        string input = "";

        if (args.Length == 1)
        {
            Console.WriteLine("\nUm parâmetro de texto não foi especificado");

            List<char> chars = new();

            int[] decimalCodes = hidden.ToCharArray().Select(c => Convert.ToInt32(c))
                .Concat(new int[26].Select((_, i) => 97 + i)).Distinct().ToArray();

            Random random = new(DateTime.Now.Millisecond);
            for (int i = 0; i < hidden.Length * 2; i++)
            {
                int number = decimalCodes[random.Next(decimalCodes.Length)];
                chars.Add(number % 2 > 0 && !hidden.Contains(Convert.ToChar(number)) ? Convert.ToChar(number) : char.Parse(Convert.ToChar(number).ToString().ToUpper()));
            }

            foreach (char c in hidden.Where(ch => !chars.Contains(ch)).Distinct()) chars.Insert(random.Next(chars.Count), c);

            input = new(chars.ToArray());
        }
        else
        {
            input = args[1];

            if (File.Exists(args[1]))
            {
                FileInfo file = new(args[1]);

                Console.Write("Lendo arquivo: ");
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine(file.FullName);
                Console.ResetColor();

                StreamReader reader = file.OpenText();
                input = reader.ReadToEnd();
                reader.Close();
            }
        }

        char[] _chars = hidden.ToCharArray().Distinct().ToArray();
        if (!_chars.All(c => input.Contains(c)))
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("A mensagem possui caracteres que não estão no texto:");

            char[] invalidChars = _chars.Where(c => !input.Contains(c)).ToArray();
            for (int i = 0; i < invalidChars.Length; i++)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write($"\"{invalidChars[i]}\"");
                Console.ResetColor();

                if (invalidChars.Length - 1 > i) Console.Write(", ");
            }

            Console.WriteLine();

            return 1;
        }

        if (File.Exists(args[0]) || (args.Length > 1 && File.Exists(args[1]))) Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine(input);
        Console.ResetColor();
        Console.WriteLine();

        List<int[]> codebook = new();
        Random random_ = new(DateTime.Now.Millisecond);
        for (int i = 0; i < hidden.Length; i++)
        {
            int[] positions = input.Select((c, i_) => c == hidden[i] ? i_ : -1).Where(i_ => i_ != -1).ToArray();
            int position = positions[random_.Next(positions.Length)];

            StreamReader stream = new(new MemoryStream(Encoding.UTF8.GetBytes(input)));
            int line = input.Take(position).Count(c => c == '\n') + 1, lineChars = 0;
            for (int i_ = 0; i_ < line - 1; i_++) lineChars += stream.ReadLine() is string str ? str.Length + 1 : 0;
            stream.Close();

            codebook.Add(new int[] { line, position - lineChars + 1 });
        }

        foreach (int[] code in codebook) Console.WriteLine($"{code[0]}:{code[1]}");

        return 0;
    }
}