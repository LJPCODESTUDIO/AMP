﻿using AMP.Network.Packets.Attributes;

namespace AMP.Network.Packets.Implementation {
    [PacketDefinition((byte) PacketType.CREATURE_HEALTH_CHANGE)]
    public class CreatureHealthChangePacket : NetPacket {
        [SyncedVar]       public long  creatureId;
        [SyncedVar(true)] public float change;
    }
}
