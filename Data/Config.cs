﻿using System;
using System.Collections.Specialized;
using ThunderRoad;
using static ThunderRoad.TextData;
using static ThunderRoad.UIList.Value;

namespace AMP.Data {
    public static class Config {

        public const long DISCORD_APP_ID = 1005583334392471583;



        public static readonly ItemData.Type[] ignoredTypes = {
            ItemData.Type.Body,
            ItemData.Type.Spell,
            //ItemData.Type.Prop,
            ItemData.Type.Wardrobe
        };

        public static readonly object[,] itemCategoryReplacement = {
            { ItemData.Type.Shield, "ShieldRound" },
            { ItemData.Type.Food,   "FoodApple" },

            { ItemData.Type.Misc,   "SwordShortCommon" } // Others
        };

        public const int TICK_RATE = 15;

        public const int MAX_ITEMS_FOR_CLIENT = 150; // TODO: Maybe implement a item limit per client

        // Assume the item is the same if they are the same if they are not that much apart
        public const float SMALL_ITEM_CLONE_MAX_DISTANCE = 0.01f * 0.01f; //~10cm
        public const float MEDIUM_ITEM_CLONE_MAX_DISTANCE = 0.1f * 0.1f; //~10cm
        public const float BIG_ITEM_CLONE_MAX_DISTANCE = 1f * 1f; //~1m

        // Min distance a item needs to move before its position is updated
        public const float REQUIRED_MOVE_DISTANCE = 0.0001f; // ~1cm

        // Min distance a item needs to move before its position is updated
        public const float REQUIRED_PLAYER_MOVE_DISTANCE = 0.0001f; // ~1cm

        // Min distance a item needs to move before its position is updated
        public const float REQUIRED_ROTATION_DISTANCE = 2f * 2f; // ~2°

        // Distance needed for the ragdoll to be teleported to the player (Happens when it's glitching out)
        public const float RAGDOLL_TELEPORT_DISTANCE = 2f * 2f; // ~2m
    }
}
