using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace LifxAnimator
{
    public class LifxLight
    {
        private readonly Image<Rgb24> sequence;
        private readonly int sequenceIndex;
        private ILifxHsbkData lastSetColor;

        public LifxLight(
            IPAddress ip, Image<Rgb24> sequence, int sequenceIndex)
        {
            EndPoint = new IPEndPoint(ip, 56700);
            this.sequence = sequence;
            this.sequenceIndex = sequenceIndex;
        }

        public IPEndPoint EndPoint { get; }
        public float BrightnessFactor { get; set; } = 1f;

        public Rgb24 GetColor(int frameIndex)
            => sequence[x: frameIndex, y: sequenceIndex];

        public async Task SendSetColorMessage(int frameIndex, UdpClient client,
            int transitionDuration = 0)
        {
            Rgb24 color = GetColor(frameIndex);

            LifxSetColorMessage message = new LifxSetColorMessage
            {
                Kelvin = 3500, /* Neutral */
                Duration = (uint)transitionDuration
            }.SetRgb24(color);

            message.Brightness = (ushort)Math.Min(ushort.MaxValue, message.Brightness * BrightnessFactor);

            // Prevent a white flicker when blacking a bulb with transitions enabled.
            if (message.Brightness == 0 && lastSetColor != null && transitionDuration > 0)
            {
                message.Hue = lastSetColor.Hue;
                message.Saturation = lastSetColor.Saturation;
            }

            LifxPacket packet = new LifxPacket
            {
                FrameHeaderTagged = true,
                FrameHeaderSource = 092985, // An arbitrary, unique value.
                FrameAddressAcknowledgementRequired = false,
                FrameAddressResponseRequired = false,
                ProtocolHeaderType = LifxMessageType.SetColor,
                Payload = message
            };

            using (MemoryStream data = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(data))
            {
                packet.Serialize(writer);
                await client.SendAsync(data.ToArray(), (int)data.Length, EndPoint);
            }

            lastSetColor = message;
        }
    }
}
