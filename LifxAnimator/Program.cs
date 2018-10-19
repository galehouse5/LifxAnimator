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

namespace LifxAnimator
{
    class Program
    {
        class Options
        {
            [Option("path", Required = true, HelpText = "Path of sequence image."
                + " Pixel rows correspond to lights and pixel columns correspond to frames.")]
            public string Path { get; set; }

            [Option("lights", Required = true, Min = 1, HelpText = "Space-separated list of IP addresses. Order is important." +
                " The first light maps to the topmost pixel row of the sequence image.")]
            public IReadOnlyCollection<string> Lights { get; set; }

            [Option("fps", Default = 1)]
            public int FramesPerSecond { get; set; }

            [Option("repeat-count", Default = 0, HelpText = "A negative number repeats until stopped.")]
            public int RepeatCount { get; set; }

            public bool RepeatUntilStopped => RepeatCount < 0;

            [Option("smooth-transitions", HelpText = "Smoothly adjust color and brightness when transitioning frames.")]
            public bool SmoothTransitions { get; set; }

            [Option("brightness-factor", Default = 1f, HelpText = "Scales brightness so you don't need sunglasses while testing.")]
            public float BrightnessFactor { get; set; }

            public IEnumerable<string> Validate()
            {
                if (!File.Exists(Path))
                    yield return "Path does not exist.";

                var image = Image.Load(Path);

                if (image.Height < Lights.Count())
                    yield return $"Sequence can't handle more than {image.Height} light(s).";

                if (FramesPerSecond <= 0m || FramesPerSecond > 20m)
                    yield return "FPS must be greater than 0 and less than or equal to 20.";

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
            var sequence = Image.Load<Rgb24>(options.Path);
            var lights = options.Lights.Select(IPAddress.Parse)
                .Select((ip, i) => new LifxLight(ip, sequence, i)
                {
                    BrightnessFactor = options.BrightnessFactor
                }).ToArray();
            int frameDuration = 1000 / options.FramesPerSecond;

            using (UdpClient client = new UdpClient())
            {
                for (int i = 0; i <= options.RepeatCount || options.RepeatUntilStopped; i++)
                {
                    for (int j = 0; j < sequence.Width; j++)
                    {
                        if (cancellationToken.IsCancellationRequested) break;

                        Console.Clear();
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine($"Rendering frame {j + 1} / {sequence.Width}:");

                        foreach (LifxLight light in lights)
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
