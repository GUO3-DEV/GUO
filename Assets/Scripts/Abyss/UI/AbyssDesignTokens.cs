using UnityEngine;

namespace AbyssSurvivor.UI
{
    public static class AbyssDesignTokens
    {
        public const int ReferenceWidth = 1080;
        public const int ReferenceHeight = 2340;

        public static readonly Color32 Background = Hex(0x0d, 0x0d, 0x14);
        public static readonly Color32 Panel = Hex(0x1a, 0x1a, 0x29);
        public static readonly Color32 PurpleAccent = Hex(0x8c, 0x40, 0xff);
        public static readonly Color32 OrangeAccent = Hex(0xe6, 0x73, 0x1a);
        public static readonly Color32 MutedPanel = Hex(0x12, 0x12, 0x1f);
        public static readonly Color32 TextPrimary = Hex(0xf2, 0xef, 0xff);
        public static readonly Color32 TextMuted = Hex(0xa9, 0x9d, 0xc9);

        private static Color32 Hex(byte red, byte green, byte blue)
        {
            return new Color32(red, green, blue, 255);
        }
    }
}
