﻿using AMP.Network.Packets.Attributes;

namespace AMP.Network.Packets.Implementation {
    [PacketDefinition((byte) PacketType.ITEM_OWNER)]
    public class ItemOwnerPacket : NetPacket {
        [SyncedVar] public long itemId;
        [SyncedVar] public bool owning;
    }
}
