using CommandLine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LifxImageScript
{
    class Program
    {
        class Options
        {
            [Option("path", Required = true, HelpText = "Path of image script." +
                " Image height should correspond to number of scripted lights." +
                " Image width should correspond to number of scripted frames.")]
            public string Path { get; set; }

            [Option("lights", Required = true, Min = 1, HelpText = "IP address list of lights." +
                " Order is important. The first light maps to the topmost row of image script, etc.")]
            public IReadOnlyCollection<string> Lights { get; set; }

            [Option("fps", Default = 1)]
            public int FramesPerSecond { get; set; }

            [Option("repeat-count", Default = 0, HelpText = "A negative number signifies to repeat until stopped.")]
            public int RepeatCount { get; set; }

            public bool RepeatUntilStopped => RepeatCount < 0;

            [Option("smooth-transitions", HelpText = "Prevents a strobe effect when transitioning between frames.")]
            public bool SmoothTransitions { get; set; }

            [Option("brightness-factor", Default = 1f, HelpText = "Scales down brightness so you don't need sunglasses when testing.")]
            public float BrightnessFactor { get; set; }

            public IEnumerable<string> Validate()
            {
                if (!File.Exists(Path))
                    yield return "Path does not exist.";

                var image = Image.Load(Path);

                if (image.Height < Lights.Count())
                    yield return $"Script can't handle more than {image.Height} light(s).";

                if (FramesPerSecond <= 0m || FramesPerSecond > 20m)
                    yield return "Frames per second must be greater than 0 and less than or equal to 20.";

                foreach (string light in Lights)
                {
                    if (!IPAddress.TryParse(light, out _))
                        yield return $"{light} is not a valid IP address.";
                }
            }
        }

        static void HandleValidationErrors(IEnumerable<string> errors)
        {
            Console.WriteLine("Please correct the following error(s):");

            foreach (string error in errors)
            {
                Console.WriteLine(" - " + error);
            }

            Environment.ExitCode = -1;
        }

        static async Task Run(Options options, CancellationToken cancellationToken)
        {
            var script = Image.Load<Rgb24>(options.Path);
            var lights = options.Lights.Select(IPAddress.Parse)
                .Select((ip, i) => new ScriptedLight(ip, script, i)
                {
                    BrightnessFactor = options.BrightnessFactor
                }).ToArray();
            int frameDuration = 1000 / options.FramesPerSecond;

            using (UdpClient client = new UdpClient())
            {
                for (int i = 0; i <= options.RepeatCount || options.RepeatUntilStopped; i++)
                {
                    for (int j = 0; j < script.Width; j++)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine($"Rendering frame {j + 1} / {script.Width}:");

                        foreach (ScriptedLight light in lights)
                        {
                            Rgb24 color = light.GetColor(j);
                            Console.WriteLine($" - {light.EndPoint.Address}: R={color.R:d3}, G={color.G:d3}, B={color.B:d3}");
                            Task forget = light.SendSetColorMessage(j, client,
                                transitionDuration: options.SmoothTransitions ? frameDuration : 0);
                        }

                        Console.WriteLine();
                        Console.WriteLine(options.RepeatUntilStopped ? "Repeating until stopped. Press any key to stop..."
                            : options.RepeatCount > 0 ? $"Repeating {options.RepeatCount - i} more time(s). Press any key to stop..."
                            : "Press any key to stop...");
                        await Task.Delay(frameDuration, cancellationToken);
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    var errors = o.Validate();
                    if (errors.Any())
                    {
                        HandleValidationErrors(errors);
                        return;
                    }

                    CancellationTokenSource cts = new CancellationTokenSource();
                    Task.Run(() => { Console.ReadKey(); cts.Cancel(); });
                    try { Run(o, cts.Token).Wait(); } catch { }
                });
        }
    }
}
