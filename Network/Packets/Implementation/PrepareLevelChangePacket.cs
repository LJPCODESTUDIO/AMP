﻿using AMP.Network.Data;
using AMP.Network.Data.Sync;
using AMP.Threading;
using Netamite.Client.Definition;
using Netamite.Network.Packet;
using Netamite.Network.Packet.Attributes;
using Netamite.Server.Definition;
using System;
using ThunderRoad;
using UnityEngine;

namespace AMP.Network.Packets.Implementation {
    [PacketDefinition((byte) PacketType.PREPARE_LEVEL_CHANGE)]
    public class PrepareLevelChangePacket : AMPPacket {
        [SyncedVar] public string username;
        [SyncedVar] public string level;
        [SyncedVar] public string mode;

        public PrepareLevelChangePacket() { }

        public PrepareLevelChangePacket(string username, string level, string mode) {
            this.username = username;
            this.level = level;
            this.mode = mode;
        }

        public override bool ProcessClient(NetamiteClient client) {
            Dispatcher.Enqueue(() => {
                foreach(PlayerNetworkData playerSync in ModManager.clientSync.syncData.players.Values) { // Will despawn all player creatures and respawn them after level has changed
                    if(playerSync.creature == null) continue;

                    Creature c = playerSync.creature;
                    playerSync.creature = null;
                    playerSync.isSpawning = false;
                    try {
                        c.Despawn();
                        GameObject.Destroy(c.gameObject);
                    } catch(Exception) { }
                }
            });

            ModManager.clientInstance.allowTransmission = false;
            return true;
        }

        public override bool ProcessServer(NetamiteServer server, ClientData client) {
            return base.ProcessServer(server, client);
        }
    }
}
