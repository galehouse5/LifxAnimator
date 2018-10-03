using System;
using System.IO;

namespace LifxImageScript
{
    // Reference: https://lan.developer.lifx.com/docs/header-description
    public struct LifxPacket
    {
        // Frame header
        public bool FrameHeaderTagged { get; set; } // Determines usage of the Frame Address target field
        public UInt32 FrameHeaderSource { get; set; } // Source identifier: unique value set by the client, used by responses

        // Frame address
        public UInt64 FrameAddressTarget { get; set; } // 6 byte device address (MAC address) or zero (0) means all devices
        public bool FrameAddressAcknowledgementRequired { get; set; } // Acknowledgement message required
        public bool FrameAddressResponseRequired { get; set; } // Response message required
        public byte FrameAddressSequence { get; set; } // Wrap around message sequence number

        // Protocol header
        public LifxMessageType ProtocolHeaderType { get; set; } // Message type determines the payload being used

        public ILifxMessage Payload { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            UInt16 frameHeaderSize = (UInt16)(36 + (Payload?.Size ?? 0)); // Size of entire message in bytes including this field

            // Frame header
            writer.Write(frameHeaderSize);
            writer.Write((UInt16)
                ((0 << 14) // Message origin indicator: must be zero (0)
                + ((FrameHeaderTagged ? 1 : 0) << 13) // Determines usage of the Frame Address target field
                + (1 << 12) // Message includes a target address: must be one (1)
                + 1024) // Protocol number: must be 1024 (decimal)
            );
            writer.Write(FrameHeaderSource);

            // Frame address
            writer.Write(FrameAddressTarget);
            writer.Write(new byte[6]);
            writer.Write((byte)(
                (0 << 2) // Reserved
                + ((FrameAddressAcknowledgementRequired ? 1 : 0) << 1) // Acknowledgement message required
                + (FrameAddressResponseRequired ? 1 : 0)) // Response message required
            );
            writer.Write(FrameAddressSequence);

            // Protocol header
            writer.Write((UInt64)0); // Reserved
            writer.Write((UInt16)ProtocolHeaderType);
            writer.Write((UInt16)0); // Reserved

            Payload?.Serialize(writer);
        }
    }
}
