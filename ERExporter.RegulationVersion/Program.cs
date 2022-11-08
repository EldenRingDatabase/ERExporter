using SoulsFormats;
using System.Reflection;

#pragma warning disable CA1416 // Validate platform compatibility
namespace ERExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            bool error = false;
            var assembly = Assembly.GetExecutingAssembly();

            if (args.Length == 0)
            {
                AwaitConfirmation(
                    $"{assembly.GetName().Name} {assembly.GetName().Version}\n\n" +
                    "Pass the path to regulation.bin as an argument to read its version.");
                return;
            }

            var path = args[0];
            if (Path.GetExtension(path) != ".bin")
            {
                AwaitConfirmation("Invalid extension of regulations.bin file.");
                return;
            }

            var ret = SFUtil.DecryptERRegulation(path);
            var version = ParseRegulationVersion(ret.Version);

            var filePath = Path.Combine(Path.GetDirectoryName(path), "regulation_version.txt");
            Console.WriteLine($"Writing regulation version to {filePath}");

            File.WriteAllText(filePath, version);
        }

        static void AwaitConfirmation(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("\nPress any key to exit.");
            Console.ReadKey();
        }

        static string ParseRegulationVersion(string version)
        {
            var major = version.Substring(0, 1);
            var minor = version.Substring(1, 2);
            var patch = version.Substring(3, 1);
            return major + "." + minor + "." + patch;
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
