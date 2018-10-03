using System;
using System.IO;

namespace LifxImageScript
{
    public enum LifxMessageType : UInt16
    {
        SetColor = 102
    }

    public interface ILifxMessage
    {
        int Size { get; }
        void Serialize(BinaryWriter writer);
    }
}
