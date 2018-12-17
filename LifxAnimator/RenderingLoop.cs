using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace LifxAnimator
{
    public delegate void RenderingFrame(int frameNumber, int repeatNumber, long elapsedMilliseconds, RenderingLoop loop);

    public class RenderingLoop : IDisposable
    {
        private readonly Image<Rgb24> sequence;
        private readonly LifxLight[] lights;
        private readonly int framesPerSecond;
        private readonly int smoothTransitionDuration;
        private readonly UdpClient client;

        public RenderingLoop(Image<Rgb24> sequence, LifxLight[] lights, int framesPerSecond)
        {
            this.sequence = sequence;
            this.lights = lights;
            this.framesPerSecond = framesPerSecond;
            smoothTransitionDuration = 1000 / framesPerSecond;
            client = new UdpClient();
        }

        public int? RepeatCount { get; set; }
        public long? RepeatMilliseconds { get; set; }
        public bool RepeatUntilCancelled => !RepeatCount.HasValue && !RepeatMilliseconds.HasValue;
        public bool SmoothTransitions { get; set; }
        public int FrameCount => sequence.Width;

        public RenderingFrame OnRenderingFrame { get; set; }

        protected bool ShouldRenderAnotherFrame(int repeatNumber, Stopwatch timer, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) return false;

            return RepeatUntilCancelled
                || repeatNumber <= RepeatCount
                || timer.ElapsedMilliseconds < RepeatMilliseconds;
        }

        protected async Task RenderFrame(int frameIndex, UdpClient client, CancellationToken cancellationToken)
        {
            var messages = new Task[lights.Length];

            for (int lightIndex = 0; lightIndex < lights.Length; lightIndex++)
            {
                LifxLight light = lights[lightIndex];
                Rgb24 color = light.GetColor(frameIndex);

                messages[lightIndex] = Task.Run(() => light.SendSetColorMessage(frameIndex, client,
                    transitionDuration: SmoothTransitions ? smoothTransitionDuration : 0),
                    cancellationToken);
            }

            await Task.WhenAll(messages);
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            int frameCount = 0;
            Stopwatch timer = Stopwatch.StartNew();

            for (int repeatNumber = 0; ShouldRenderAnotherFrame(repeatNumber, timer, cancellationToken); repeatNumber++)
            {
                for (int frameIndex = 0; frameIndex < FrameCount && ShouldRenderAnotherFrame(repeatNumber, timer, cancellationToken); frameIndex++)
                {
                    OnRenderingFrame?.Invoke(frameIndex + 1, repeatNumber, timer.ElapsedMilliseconds, this);
                    await RenderFrame(frameIndex, client, cancellationToken);

                    frameCount++;
                    int nextFrameRenderTime = frameCount * 1000 / framesPerSecond;
                    // Don't use `Task.Delay` because it's only accurate to 15.6 ms on Windows. See:
                    // https://stackoverflow.com/questions/3744032/why-are-net-timers-limited-to-15-ms-resolution
                    SpinWait.SpinUntil(() => cancellationToken.IsCancellationRequested
                        || timer.ElapsedMilliseconds >= nextFrameRenderTime);
                }
            }
        }

        public void Dispose()
        {
            client.Dispose();
        }
    }
}
