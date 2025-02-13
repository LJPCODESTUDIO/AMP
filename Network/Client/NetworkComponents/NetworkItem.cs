﻿using AMP.Data;
using AMP.Extension;
using AMP.Logging;
using AMP.Network.Client.NetworkComponents.Parts;
using AMP.Network.Data;
using AMP.Network.Data.Sync;
using AMP.Network.Packets.Implementation;
using Netamite.Helper;
using System;
using ThunderRoad;
using ThunderRoad.Skill.SpellPower;
using UnityEngine;

namespace AMP.Network.Client.NetworkComponents {
    internal class NetworkItem : NetworkPositionRotation {
        protected Item item;
        internal ItemNetworkData itemNetworkData;

        private bool isKinematicItem = false;

        internal void Init(ItemNetworkData itemNetworkData) {
            if(this.itemNetworkData != itemNetworkData) registeredEvents = false;
            this.itemNetworkData = itemNetworkData;

            targetPos = itemNetworkData.position;
            targetRot = Quaternion.Euler(itemNetworkData.rotation);

            if(ModManager.clientSync.syncData.owningItems.Contains(itemNetworkData.networkedId) != IsSending()) {
                itemNetworkData.SetOwnership(ModManager.clientSync.syncData.owningItems.Contains(itemNetworkData.networkedId));
            }

            bodyToUpdate = item?.physicBody?.rigidBody;

            RegisterEvents();
        }

        void Awake() {
            OnAwake();
        }

        protected void OnAwake() {
            item = GetComponent<Item>();
        }

        void Start() {
            if(item == null) return;

            isKinematicItem = item.physicBody.isKinematic;
            if(item.holder != null || item.isGripped || item.handlers?.Count > 0) {
                isKinematicItem = false;
            }
        }

        internal override bool IsSending() {
            return itemNetworkData != null && itemNetworkData.clientsideId > 0;
        }

        internal int lastTime = 0;
        public override void ManagedUpdate() {
            if(IsSending()) return;
            if(itemNetworkData.holdingStates == null || itemNetworkData.holdingStates.Length > 0) return;
            if(transform == null) return;

            if(itemNetworkData.lastPositionTimestamp >= NetworkData.GetDataTimestamp() - Config.NET_COMP_DISABLE_DELAY) {
                if(lastTime > 0) UpdateItem();
                lastTime = 0;

                try {
                    if(item.renderers.Count > 0 && !item.IsVisible()) {
                        transform.position = targetPos;
                        transform.rotation = targetRot;
                    } else {
                        base.ManagedUpdate();
                    }
                }catch(NullReferenceException) {
                    base.ManagedUpdate();
                }
            } else if(lastTime != (int) Time.time) {
                if(lastTime == 0) UpdateItem();
                lastTime = (int) Time.time;

                foreach(Renderer renderer in item.renderers) {
                    if(renderer == null) continue;
                    if(!renderer.isVisible) return;
                }

                if(bodyToUpdate) {
                    bodyToUpdate.rotation = targetRot;
                    bodyToUpdate.position = targetPos;
                } else {
                    transform.rotation = targetRot;
                    transform.position = targetPos;
                }
            }
        }

        #region Register Events
        public override void ManagedOnEnable() {
            if(registeredEvents) return;
            if(itemNetworkData == null) return;

            RegisterEvents();
        }

        private float lastImbueUpdate = 0;
        private bool registeredEvents = false;
        internal void RegisterEvents() {
            if(registeredEvents) return;

            itemNetworkData.clientsideItem.OnDespawnEvent += Item_OnDespawnEvent;
            itemNetworkData.clientsideItem.OnTelekinesisGrabEvent += Item_OnTelekinesisGrabEvent;

            for(int i = 0; i < itemNetworkData.clientsideItem.imbues.Count; i++) {
                Imbue imbue = itemNetworkData.clientsideItem.imbues[i];
                int index = i;

                imbue.OnImbueEnergyFilled += (imb, spellData, amount, change, eventTime) => {
                    if(itemNetworkData.networkedId <= 0) return;
                    if(lastImbueUpdate > Time.time - 0.25) return;

                    if(spellData != null && eventTime == EventTime.OnStart) {
                        new ItemImbuePacket(itemNetworkData.networkedId, spellData.id, index, amount + change).SendToServerReliable();
                        lastImbueUpdate = Time.time;
                    }
                };
                imbue.OnImbueEnergyDrained += (imb, spellData, amount, change, eventTime) => {
                    if(itemNetworkData.networkedId <= 0) return;
                    if(lastImbueUpdate > Time.time - 0.25) return;

                    if(spellData != null && eventTime == EventTime.OnStart) {
                        new ItemImbuePacket(itemNetworkData.networkedId, spellData.id, index, amount + change).SendToServerReliable();
                        lastImbueUpdate = Time.time;
                    }
                };
                imbue.OnImbueSpellChange += (imb, spellData, amount, change, eventTime) => {
                    if(itemNetworkData.networkedId <= 0) return;
                    if(lastImbueUpdate > Time.time - 0.25) return;

                    if(spellData != null && eventTime == EventTime.OnEnd) {
                        new ItemImbuePacket(itemNetworkData.networkedId, spellData.id, index, amount + change).SendToServerReliable();
                        lastImbueUpdate = Time.time;
                    }
                };
            }

            foreach(Handle handle in itemNetworkData.clientsideItem.handles) {
                handle.SlidingStateChange += Handle_SlidingStateChange;
            }

            if(IsSending()) {
                OnHoldStateChanged();
            } else {
                itemNetworkData.UpdateHoldState();
            }
            UpdateItem();

            registeredEvents = true;
        }

        private void Handle_SlidingStateChange(RagdollHand ragdollHand, bool sliding, Handle handle, float position, EventTime eventTime) {
            if(eventTime == EventTime.OnStart) return;
            if(!IsSending()) return;

            //TODO: Fix
            //itemNetworkData.axisPosition = position;
            new ItemSlidePacket(itemNetworkData).SendToServerUnreliable();
        }
        #endregion

        #region Unregister Events
        public override void ManagedOnDisable() {
            UnregisterEvents();
        }

        internal void UnregisterEvents() {
            if(!registeredEvents) return;

            itemNetworkData.clientsideItem.OnDespawnEvent -= Item_OnDespawnEvent;
            itemNetworkData.clientsideItem.OnTelekinesisGrabEvent -= Item_OnTelekinesisGrabEvent;

            foreach(Handle handle in itemNetworkData.clientsideItem.handles) {
                handle.SlidingStateChange -= Handle_SlidingStateChange;
            }

            registeredEvents = false;
        }
        #endregion

        #region Events
        internal void OnBreak(Breakable breakable, PhysicBody[] pieces) {
            if(!IsSending()) new ItemOwnerPacket(itemNetworkData.networkedId, true).SendToServerReliable();

            Vector3[] velocities = new Vector3[breakable.subBrokenBodies.Count];
            Vector3[] angularVelocities = new Vector3[breakable.subBrokenBodies.Count];
            for(int i = 0; i < velocities.Length; i++) {
                velocities[i] = breakable.subBrokenBodies[i].velocity * 10;
                angularVelocities[i] = breakable.subBrokenBodies[i].angularVelocity;
            }
            new ItemBreakPacket(itemNetworkData.networkedId, velocities, angularVelocities).SendToServerReliable();
        }

        private void Item_OnDespawnEvent(EventTime eventTime) {
            //if(!IsSending()) return;
            if(!ModManager.clientInstance.allowTransmission) return;

            if(  (itemNetworkData.holdingStates == null || itemNetworkData.holdingStates.Length == 0) // Check if the item isn't held by anything
              || (itemNetworkData.clientsideId > 0 && itemNetworkData.networkedId > 0)) { // Check if the item is already networked and is in ownership of the client
                new ItemDespawnPacket(itemNetworkData).SendToServerReliable();
                Log.Debug(Defines.CLIENT, $"Event: Item {itemNetworkData.dataId} ({itemNetworkData.networkedId}) is despawned.");

                ModManager.clientSync.syncData.items.TryRemove(itemNetworkData.networkedId, out _);

                itemNetworkData.networkedId = 0;

                Destroy(this);
            }
        }

        // If the player grabs an item with telekenesis, we give him control over the position data
        private void Item_OnTelekinesisGrabEvent(Handle handle, SpellTelekinesis teleGrabber) {
            if(IsSending()) return;

            new ItemOwnerPacket(itemNetworkData.networkedId, true).SendToServerReliable();
            itemNetworkData.SetOwnership(true);
        }
        #endregion

        internal bool hasSendedFirstTime = false;
        internal void OnHoldStateChanged() {
            if(itemNetworkData == null) return;

            if(itemNetworkData.holdingStates == null) itemNetworkData.holdingStates = new ItemHoldingState[0];

            ItemHoldingState[] holdingStates = itemNetworkData.holdingStates;
            //float axisPosition        = itemNetworkData.axisPosition;

            itemNetworkData.UpdateFromHolder();

            //if(itemNetworkData.axisPosition != axisPosition) {
            //    new ItemSlidePacket(itemNetworkData).SendToServerReliable();
            //}

            if(  hasSendedFirstTime
              && ItemHoldingState.Equals(itemNetworkData.holdingStates, holdingStates)) return; // Nothing changed so no need to send it again / Also check if it has even be sent, otherwise send it anyways. Side and Draw Slot have valid default values

            if(!IsSending()) {
                new ItemOwnerPacket(itemNetworkData.networkedId, true).SendToServerReliable();
                itemNetworkData.SetOwnership(true);
            }

            hasSendedFirstTime = true;
            if(itemNetworkData.holdingStates.Length > 0) {  // currently held by a creature
                new ItemSnapPacket(itemNetworkData).SendToServerReliable();
            } else if (itemNetworkData.holdingStates.Length > 0 || holdingStates.Length > 0) { // was held by a creature, but now is not anymore
                new ItemUnsnapPacket(itemNetworkData).SendToServerReliable();
            }
        }

        internal void UpdateItem() {
            bool owner = IsSending();

            if(item != null) {
                bool active = itemNetworkData.lastPositionTimestamp >= NetworkData.GetDataTimestamp() - Config.NET_COMP_DISABLE_DELAY;

                item.DisallowDespawn = !owner || item.data.type == ItemData.Type.Prop;
                item.physicBody.useGravity = owner || (!owner && !active);
                //item.physicBody.isKinematic = (owner ? isKinematicItem : true); // TODO: Fix, causing snapped items to malfunction

                // Check if the item is active and set the tick rate accordingly
                if(active && !owner) {
                    // Item is active and receiving data, we want it to update every frame
                    NetworkComponentManager.SetTickRate(this, 1, ManagedLoops.Update);
                } else {
                    // Item is inactive and not receiving any new data, just update it from time to time
                    NetworkComponentManager.SetTickRate(this, UnityEngine.Random.Range(150, 250), ManagedLoops.Update);
                    if(lastTime == 0) lastTime = (int) Time.time;
                }
            } else {
                // Item is sending data, just update it from time to time, probably not nessesary at all, but for good measure. The data sending step is done in a seperated thread
                NetworkComponentManager.SetTickRate(this, UnityEngine.Random.Range(150, 250), ManagedLoops.Update);
            }
        }

        internal void UpdateIfNeeded() {
            if(NetworkComponentManager.GetTickRate(this, ManagedLoops.Update) != 1) {
                UpdateItem();
            }
        }
    }
}
