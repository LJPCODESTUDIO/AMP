﻿using AMP.Extension;
using AMP.Network.Client.NetworkComponents;
using AMP.Network.Packets.Implementation;
using ThunderRoad;
using UnityEngine;

namespace AMP.Network.Data.Sync {
    public class CreatureNetworkData {
        #region Values
        internal long networkedId = 0;

        internal string creatureType;
        internal string containerID;
        internal byte factionId;

        internal Vector3 position;
        internal float rotationY;
        //public Vector3 velocity;

        internal Vector3[] ragdollParts;

        internal bool loaded = false;

        internal bool isSpawning = false;
        internal long clientsideId = 0;
        internal Creature creature;
        private NetworkCreature _networkCreature;
        internal NetworkCreature networkCreature {
            get {
                if(_networkCreature == null && creature != null) _networkCreature = creature.GetComponent<NetworkCreature>();
                return _networkCreature;
            }
        }

        internal long clientTarget = 0;

        internal float maxHealth = 100;
        internal float health = 100;

        internal float height = 2f;

        internal string[] equipment = new string[0];
        #endregion

        #region Packet Parsing
        internal void Apply(CreatureSpawnPacket p) {
            networkedId  = p.creatureId;
            clientsideId = p.clientsideId;
            creatureType = p.type;
            containerID  = p.container;
            factionId    = p.factionId;
            position     = p.position;
            rotationY    = p.rotationY;
            health       = p.health;
            maxHealth    = p.maxHealth;
            height       = p.height;

            equipment    = p.equipment;
        }

        internal void Apply(CreaturePositionPacket p) {
            if(isSpawning) return;
            position  = p.position;
            rotationY = p.rotationY;
        }

        internal void ApplyPositionToCreature() {
            if(creature == null) return;

            if(creature.isKilled) {
                networkCreature.SetRagdollInfo(ragdollParts);
            } else {
                creature.transform.eulerAngles = new Vector3(0, rotationY, 0);
                //clientsideCreature.transform.position = position;

                networkCreature.targetPos = position;
                //networkCreature.velocity = velocity;
                //clientsideCreature.locomotion.rb.velocity = velocity;
                //clientsideCreature.locomotion.velocity = velocity;
            }

            PositionChanged();
        }

        internal void Apply(CreatureRagdollPacket p, bool add_offset_back = true) {
            position = p.position;

            if(p.ragdollParts.Length == 0) {
                ragdollParts = null;
            } else {
                ragdollParts = p.ragdollParts;

                if(add_offset_back) { 
                    for(byte i = 0; i < p.ragdollParts.Length; i++) {
                        if(i % 2 == 0) ragdollParts[i] += position; // Add offset only to positions, they are at the even indexes
                    }
                }
            }
            PositionChanged();
        }

        internal void Apply(CreatureHealthSetPacket p) {
            health = p.health;
        }

        internal void Apply(CreatureHealthChangePacket p) {
            health += p.change;
        }

        internal void ApplyHealthToCreature() {
            if(creature != null) {
                creature.currentHealth = health;

                //Log.Debug($"Creature {clientsideCreature.creatureId} is now at health {health}.");

                if(health <= 0) {
                    creature.Kill();
                }
            }
        }

        internal void UpdatePositionFromCreature() {
            if(creature == null) return;

            if(creature.IsRagdolled()) {
                ragdollParts = creature.ReadRagdoll();
            } else {
                ragdollParts = null;
                position     = creature.transform.position;
                rotationY    = creature.transform.eulerAngles.y;
            }
        }

        internal void PositionChanged() {
            if(creature != null) creature.lastInteractionTime = Time.time;
        }
        #endregion

        #region Ownership stuff
        internal void SetOwnership(bool owner) {
            if(owner) {
                if(clientsideId <= 0) clientsideId = ModManager.clientSync.syncData.currentClientCreatureId++;
            } else {
                clientsideId = 0;
            }
            networkCreature?.UpdateCreature();
            //if(clientsideCreature != null) {
            //    clientsideCreature.disallowDespawn = owner;
            //}
        }
        #endregion
    }
}
