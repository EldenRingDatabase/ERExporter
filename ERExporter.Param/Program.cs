using SoulsFormats;
using System.Reflection;

#pragma warning disable CA1416 // Validate platform compatibility
namespace ERExporter
{
    class Program
    {
        static private List<PARAMDEF>? paramDefs;

        static void Main(string[] args)
        {
            bool pause = false;

            // Used for quick testing
            //string[] args2 = { @"F:\Steam\steamapps\common\ELDEN RING\Game\param\gameparam\ShopLineupParam.param" };
            //args = args2;

            var assembly = Assembly.GetExecutingAssembly();
            var location = AppContext.BaseDirectory;

            if (args.Length == 0)
            {
                // Shamelessly stolen from Yabber
                Console.WriteLine(
                    $"{assembly.GetName().Name} {assembly.GetName().Version}\n\n" +
                    $"{assembly.GetName().Name} has no GUI.\n" +
                    "Drag and drop a regulation.bin or .param onto the exe to export it."
                );

                pause = true;
            }

            foreach (string path in args)
            {
                try
                {
                    var ext = Path.GetExtension(path);
                    if (ext == ".bin")
                    {
                        ExportRegulationParams(path);
                    }
                    else if (ext == ".param")
                    {
                        if (paramDefs == null)
                        {
                            paramDefs = ReadParamDefs(location);
                        }

                        var paramNames = ReadParamNames(location, Path.GetFileNameWithoutExtension(path));
                        ExportParam(path, paramDefs, paramNames);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unhandled exception: {ex}");
                    pause = true;
                }
            }

            if (pause)
            {
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
        }

        static List<PARAMDEF> ReadParamDefs(string location)
        {
            var paramDefs = new List<PARAMDEF>();
            var paramDefsLocation = Path.Combine(Path.GetDirectoryName(location), "Defs");

            foreach (var xml in Directory.GetFiles(paramDefsLocation, "*.xml"))
            {
                var paramdef = PARAMDEF.XmlDeserialize(xml);
                paramDefs.Add(paramdef);
            }

            return paramDefs;
        }

        static Dictionary<int, string> ReadParamNames(string location, string param)
        {
            var paramNames = new Dictionary<int, string>();
            var namesFile = Path.Combine(Path.GetDirectoryName(location), "Names", $"{param}.txt");

            using (var stream = File.OpenText(namesFile))
            {
                string line = String.Empty;
                while ((line = stream.ReadLine()) != null)
                {
                    var pieces = line.Split(' ', 2);
                    var index = Int32.Parse(pieces[0]);

                    if (paramNames.ContainsKey(index))
                    {
                        Console.WriteLine($"Skipping duplicate ParamNames index: {index}.");
                        continue;
                    }

                    paramNames.Add(index, pieces[1]);
                }
            }

            return paramNames;
        }

        static void ExportRegulationParams(string path)
        {
            var destination = Path.Combine(Path.GetDirectoryName(path), "param", "gameparam");
            Directory.CreateDirectory(destination);
            Console.WriteLine($"Exporting regulation params to {destination}");

            var ret = SFUtil.DecryptERRegulation(path);
            foreach (var file in ret.Files)
            {
                var fileName = Path.GetFileName(file.Name);
                Console.WriteLine($"Unpacking {fileName}...");

                var filePath = Path.Combine(destination, fileName);

                using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(file.Bytes, 0, file.Bytes.Length);
                }
            }
        }

        static void ExportParam(string path, List<PARAMDEF> paramDefs, Dictionary<int, string> paramNames)
        {
            var param = PARAM.Read(path);
            param.ApplyParamdefCarefully(paramDefs);

            var destination = Path.Combine(Path.GetDirectoryName(path), $"{Path.GetFileNameWithoutExtension(path)}.csv");
            Console.WriteLine($"Exporting param to {destination}");

            var fields = new List<string>{"Row ID", "Row Name"};
            fields.AddRange(param.AppliedParamdef.Fields.Select(field => field.InternalName));

            using (var w = new StreamWriter(destination))
            {
                w.WriteLine(String.Join(';', fields));

                foreach (var row in param.Rows)
                {
                    string? name;
                    paramNames.TryGetValue(row.ID, out name);

                    w.Write($"{row.ID};{name};");

                    foreach (var cell in row.Cells)
                    {
                        if (cell.Def.InternalType == "f32")
                        {
                            // Be compliant with Yapped, floats are rounded to 6 digits
                            float value = (float)cell.Value;
                            w.Write($"{value:0.######};");
                        }
                        else
                        {
                            if (cell.Value is byte[] value)
                            {
                                w.Write($"{Convert.ToHexString(value)};");
                            }
                            else
                            {
                                w.Write($"{cell.Value};");
                            }
                        }
                    }

                    w.WriteLine();
                }
            }
        }
    }
}
#pragma warning restore CA1416 // Validate platform compatibility
