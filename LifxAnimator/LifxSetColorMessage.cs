using System;
using System.IO;

namespace LifxAnimator
{
    public struct LifxSetColorMessage : ILifxMessage, ILifxHsbkData
    {
        public int Size => 13;

        public UInt16 Hue { get; set; }
        public UInt16 Saturation { get; set; }
        public UInt16 Brightness { get; set; }
        public UInt16 Kelvin { get; set; }
        public UInt32 Duration { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)0); // Reserved
            writer.Write(Hue);
            writer.Write(Saturation);
            writer.Write(Brightness);
            writer.Write(Kelvin);
            writer.Write(Duration);
        }
    }
}
