using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace AbyssSurvivor.Tests.EditMode
{
    public sealed class DesignTokenTests
    {
        private const string TokenTypeName = "AbyssSurvivor.UI.AbyssDesignTokens, AbyssSurvivor";
        private const string ScreenMapTypeName = "AbyssSurvivor.UI.AbyssScreenMap, AbyssSurvivor";

        [Test]
        public void ReferenceSize_matches_mobile_portrait_target_when_tokens_are_loaded()
        {
            Type tokenType = RequireType(TokenTypeName);

            Assert.That(ReadInt(tokenType, "ReferenceWidth"), Is.EqualTo(1080));
            Assert.That(ReadInt(tokenType, "ReferenceHeight"), Is.EqualTo(2340));
        }

        [Test]
        public void Palette_matches_captured_figma_colors_when_tokens_are_loaded()
        {
            Type tokenType = RequireType(TokenTypeName);

            AssertColorNear(ReadColor(tokenType, "Background"), "#0d0d14");
            AssertColorNear(ReadColor(tokenType, "Panel"), "#1a1a29");
            AssertColorNear(ReadColor(tokenType, "PurpleAccent"), "#8c40ff");
            AssertColorNear(ReadColor(tokenType, "OrangeAccent"), "#e6731a");
        }

        [Test]
        public void ScreenMap_defines_all_sixteen_figma_frames_when_loaded()
        {
            Type screenMapType = RequireType(ScreenMapTypeName);

            IReadOnlyList<object> screens = ReadObjectList(screenMapType, "Screens");
            string[] ids = screens.Select(screen => ReadString(screen, "Id")).ToArray();

            Assert.That(ids, Is.EqualTo(new[]
            {
                "Title",
                "Town",
                "DungeonExplore",
                "Battle",
                "WeaponShop",
                "ArmorShop",
                "Alchemist",
                "QuestBoard",
                "RandomEvent",
                "TreasureChest",
                "LevelUpClass",
                "DeathReturn",
                "Inventory",
                "CharacterStats",
                "DungeonClear",
                "Settings",
            }));
        }

        [Test]
        public void TitleScreen_keeps_primary_figma_labels_when_loaded()
        {
            Type screenMapType = RequireType(ScreenMapTypeName);

            object titleScreen = ReadObjectList(screenMapType, "Screens").Single(screen => ReadString(screen, "Id") == "Title");
            string[] labels = ReadStringList(titleScreen, "PrimaryLabels");

            Assert.That(ReadString(titleScreen, "DisplayName"), Is.EqualTo("타이틀 화면"));
            Assert.That(ReadString(titleScreen, "Icon"), Is.EqualTo("🌑"));
            Assert.That(labels, Does.Contain("TEXT ROGUELIKE RPG"));
            Assert.That(labels, Does.Contain("ABYSS"));
            Assert.That(labels, Does.Contain("SURVIVOR"));
            Assert.That(labels, Does.Contain("심연의 끝을 마주할 준비가 되어있는가?"));
            Assert.That(labels, Does.Contain("게임 시작"));
            Assert.That(labels, Does.Contain("이어하기"));
            Assert.That(labels, Does.Contain("설정"));
        }

        private static Type RequireType(string typeName)
        {
            Type type = Type.GetType(typeName);
            Assert.That(type, Is.Not.Null, $"{typeName} should exist for the Wave 1 Figma design contract.");
            return type;
        }

        private static int ReadInt(Type type, string memberName)
        {
            object value = ReadStaticMember(type, memberName);
            Assert.That(value, Is.TypeOf<int>(), $"{type.FullName}.{memberName} should be an int.");
            return (int)value;
        }

        private static Color32 ReadColor(Type type, string memberName)
        {
            object value = ReadStaticMember(type, memberName);
            Assert.That(value, Is.TypeOf<Color32>(), $"{type.FullName}.{memberName} should be a Color32.");
            return (Color32)value;
        }

        private static IReadOnlyList<object> ReadObjectList(Type type, string memberName)
        {
            object value = ReadStaticMember(type, memberName);
            return ConvertEnumerable(value, $"{type.FullName}.{memberName}");
        }

        private static string ReadString(object instance, string propertyName)
        {
            PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(property, Is.Not.Null, $"{instance.GetType().FullName}.{propertyName} should exist.");

            object value = property.GetValue(instance);
            Assert.That(value, Is.TypeOf<string>(), $"{instance.GetType().FullName}.{propertyName} should be a string.");
            return (string)value;
        }

        private static string[] ReadStringList(object instance, string propertyName)
        {
            PropertyInfo property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
            Assert.That(property, Is.Not.Null, $"{instance.GetType().FullName}.{propertyName} should exist.");

            object value = property.GetValue(instance);
            return ConvertEnumerable(value, $"{instance.GetType().FullName}.{propertyName}")
                .Select(item =>
                {
                    Assert.That(item, Is.TypeOf<string>());
                    return (string)item;
                })
                .ToArray();
        }

        private static object ReadStaticMember(Type type, string memberName)
        {
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.Static;
            PropertyInfo property = type.GetProperty(memberName, Flags);
            if (property != null)
            {
                return property.GetValue(null);
            }

            FieldInfo field = type.GetField(memberName, Flags);
            Assert.That(field, Is.Not.Null, $"{type.FullName}.{memberName} should exist.");
            return field.GetValue(null);
        }

        private static IReadOnlyList<object> ConvertEnumerable(object value, string memberDescription)
        {
            Assert.That(value, Is.InstanceOf<IEnumerable>(), $"{memberDescription} should be enumerable.");
            return ((IEnumerable)value).Cast<object>().ToArray();
        }

        private static void AssertColorNear(Color32 actual, string expectedHex)
        {
            Color expected = ParseHtmlColor(expectedHex);
            const int ChannelTolerance = 2;

            Assert.That(actual.r, Is.InRange((byte)Mathf.RoundToInt(expected.r * 255f) - ChannelTolerance, (byte)Mathf.RoundToInt(expected.r * 255f) + ChannelTolerance));
            Assert.That(actual.g, Is.InRange((byte)Mathf.RoundToInt(expected.g * 255f) - ChannelTolerance, (byte)Mathf.RoundToInt(expected.g * 255f) + ChannelTolerance));
            Assert.That(actual.b, Is.InRange((byte)Mathf.RoundToInt(expected.b * 255f) - ChannelTolerance, (byte)Mathf.RoundToInt(expected.b * 255f) + ChannelTolerance));
            Assert.That(actual.a, Is.EqualTo(255));
        }

        private static Color ParseHtmlColor(string expectedHex)
        {
            bool parsed = ColorUtility.TryParseHtmlString(expectedHex, out Color color);
            Assert.That(parsed, Is.True, $"{expectedHex} should parse as an HTML color.");
            return color;
        }
    }
}
