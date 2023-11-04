﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AMP.Compression {
    public static class EmojiJoinCode {
        
        private static string[] emojis = {
            "😆",
            "😇",
            "🙃",
            "😋",
            "😘",
            "😎",
            "☹️",
            "😭",
            "🤯",
            "🥵",
            "🥶",
            "🎸",
            "🤥",
            "🍪",
            "😐",
            "🤮",
            "🤒",
            "🤡",
            "💩",
            "👻",
            "☠️",
            "👽",
            "🎃",
            "👋",
            "💪",
            "👣",
            "👂",
            "👃",
            "🧠",
            "🦷",
            "👁",
            "☂️",
            "👓",
            "👖",
            "🧥",
            "🧦",
            "👞",
            "👑",
            "🎩",
            "💍",
            "💼",
            "🐶",
            "🐱",
            "🐭",
            "🐰",
            "🐻",
            "🐷",
            "🐸",
            "🐵",
            "🐔",
            "🐧",
            "🦆",
            "🦅",
            "🦇",
            "🦉",
            "🦄",
            "🐝",
            "🕷",
            "🦂",
            "🐢",
            "🦋",
            "🐌",
            "🐍",
            "🦕",
            "🐙",
            "🦀",
            "🐬",
            "🐳",
            "🐊",
            "🐅",
            "🐘",
            "🐫",
            "🦏",
            "🦘",
            "🐎",
            "🐑",
            "🦌",
            "🦢",
            "🦨",
            "🌲",
            "🍄",
            "🐚",
            "🌷",
            "🌙",
            "🌍",
            "⭐️ ",
            "⚡️",
            "☄️",
            "🌪",
            "🔥",
            "🌈",
            "☀️",
            "☁️",
            "🌧",
            "❄️",
            "⛄️",
            "💧",
            "🌊",
            "🍏",
            "🍌",
            "🍉",
            "🍓",
            "🍒",
            "🍍",
            "🍅",
            "🍆",
            "🥑",
            "🥒",
            "🌽",
            "🥕",
            "🥐",
            "🍞",
            "🥨",
            "🧀",
            "🥚",
            "🍳",
            "🍗",
            "🌭",
            "🍔",
            "🍟",
            "🍕",
            "🌮",
            "🥫",
            "🥗",
            "🥠",
            "🍦",
            "🎂",
            "🍭",
            "🍫",
            "🍩",
            "🥜",
            "☕️",
            "🍺",
            "🧂",
            "⚽️",
            "🏈",
            "⚾️",
            "🎱",
            "🏓",
            "🏒",
            "🪁",
            "🏹",
            "🤿",
            "🥊",
            "🛹",
            "⛸",
            "⛷",
            "🪂",
            "🏋️",
            "🏊",
            "🏄",
            "🏆",
            "🎟",
            "🎬",
            "🎤",
            "🎧",
            "🎹",
            "🥁",
            "🎲",
            "♟",
            "🎳",
            "🎮",
            "🚗",
            "🚌",
            "🚚",
            "🦽",
            "🚲",
            "🏍",
            "🚝",
            "✈️",
            "🚀",
            "🚁",
            "⛵️",
            "⚓️",
            "🚧",
            "🚦",
            "🌋",
            "🏠",
            "🌌",
            "⌚️",
            "📱",
            "💻",
            "🖨",
            "🖱",
            "💾",
            "📼",
            "📷",
            "📺",
            "📻",
            "🧭",
            "🔋",
            "💡",
            "🕯",
            "💰",
            "💳",
            "💎",
            "🔧",
            "🔨",
            "⛏",
            "⚙️",
            "⛓",
            "🧲",
            "💣",
            "🪓",
            "🔪",
            "🛡",
            "🚬",
            "🔬",
            "🩹",
            "💊",
            "🧬",
            "🦠",
            "🧻",
            "🚽",
            "🚿",
            "🧼",
            "🧽",
            "🔑",
            "🚪",
            "🪑",
            "🛏",
            "🛒",
            "🎁",
            "🎈",
            "✉️",
            "🗓",
            "🗑",
            "📖",
            "📎",
            "📏",
            "📌",
            "✂️",
            "✏️",
            "🔎",
            "🔒",
            "❤️",
            "☢️",
            "❌",
            "⭕️ ",
            "⚠️",
            "💤",
            "➡️",
            "⬅️ ",
            "⬆️ ",
            "⬇️ ",
            "❗️",
            "❓",
            "➕",
            "➖",
            "➗",
            "✔️",
            "📢",
            "💬",
            "🏳️",
            "🏁",
            "⚔️"
        };
        
        
        public static string GenerateCode(string ip, ushort port) {
            List<byte> bytes = new List<byte>();

            // Address
            string[] splits = ip.Split('.');
            if(splits.Length == 4 && ip.Length <= 15) {
                if(  byte.TryParse(splits[0], out byte b1)
                  && byte.TryParse(splits[1], out byte b2)
                  && byte.TryParse(splits[2], out byte b3)
                  && byte.TryParse(splits[3], out byte b4)) {
                    bytes.AddRange(new byte[] { b1, b2, b3, b4 });
                }
            } else {
                bytes.AddRange(Encoding.ASCII.GetBytes(ip));
            }

            // Port
            bytes.AddRange(BitConverter.GetBytes(port));

            return Convert(bytes.ToArray()).TrimEnd('=');
        }

        public static KeyValuePair<string, ushort> DecodeCode(string code) {
            while(code.Length % 4 != 0) {
                code += "=";
            }
            byte[] bytes = Decode(code);

            ushort port = BitConverter.ToUInt16(bytes, bytes.Length - 2);

            string address;
            if(bytes.Length == 6) {
                address = bytes[0] + "." + bytes[1] + "." + bytes[2] + "." + bytes[3];
            } else {
                address = Encoding.ASCII.GetString(bytes, 0, bytes.Length - 2);
            }

            return new KeyValuePair<string, ushort>(address, port);
        }

        private static string Convert(byte[] bytes) {
            string str = "";
            foreach(byte b in bytes) {
                str += emojis[b];
            }
            return str;
        }

        private static byte[] Decode(string str) {
            List<string> list = new List<string>(emojis);
            List<byte> bytes = new List<byte>();
            for(int i = 0; i < str.Length; i++) {
                bytes.Add((byte)list.FindIndex(a => a == str.Substring(i, 1)));
            }
            return bytes.ToArray();
        }
    }
}
