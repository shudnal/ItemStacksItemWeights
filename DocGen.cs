using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using static ItemStacksItemWeights.ItemStacksItemWeights;

namespace ItemStacksItemWeights
{
    public static class DocGen
    {
        private static readonly StringBuilder sb = new();
        internal const string docname = "Items stacks and weights.txt";

        public static void InitCommand()
        {
            new Terminal.ConsoleCommand("iwisdocs", $"Save documentation file {docname} to config directory", args => GenerateDocumentationFile(args.Context));
        }

        public static void GenerateDocumentationFile(Terminal terminal = null)
        {
            string file = Path.Combine(filepath, docname);
            try
            {
                Directory.CreateDirectory(filepath);
                File.WriteAllText(file, GetFileText(out int itemCount));
                string log = $"Saved {itemCount} items to \"\\BepinEx\\config\\{pluginID}\\{docname}\"";
                LogInfo(log);
                terminal?.AddString(log);
            }
            catch (Exception e)
            {
                LogWarning($"Error when writing file ({file})! Error: {e.Message}");
            }
        }

        private static string Pad(string text, int width)
        {
            if (text.Length >= width)
                return text;
            return text + new string(' ', width - text.Length);
        }

        private static string GetFileText(out int itemCount)
        {
            sb.Clear();

            sb.AppendLine("This documentation is generated automatically. It contains every available item with identifiers used to configure stacks and weights.");
            sb.AppendLine("List does not include items without icons (incorrectly configured items or hidden items used by enemies, player model customization and such.");
            sb.AppendLine();

            itemCount = 0;

            if (!(bool)ObjectDB.instance)
                return sb.ToString();

            List<(string prefab, string token, string english, string localized, string stack, string weight, string type)> rows = new();

            string originalLanguage = Localization.instance.GetSelectedLanguage();

            Localization.instance.SetLanguage("English");

            foreach (GameObject item in ObjectDB.instance.m_items)
            {
                if (item == null || item.GetComponent<ItemDrop>() is not ItemDrop itemDrop)
                    continue;

                if (itemDrop.m_itemData is not ItemDrop.ItemData itemData ||
                    itemData.m_shared is not ItemDrop.ItemData.SharedData shared ||
                    shared.m_icons.Length == 0)
                    continue;

                string prefab = item.name;
                string token = shared.m_name;
                string englishName = Localization.instance.Localize(token);
                string stack = shared.m_maxStackSize.ToString();
                string weight = shared.m_weight.ToString();
                string type = shared.m_itemType.ToString();

                rows.Add((prefab, token, englishName, "", stack, weight, type));
            }

            Localization.instance.SetLanguage(originalLanguage);

            for (int i = 0; i < rows.Count; ++i)
            {
                var r = rows[i];
                r.localized = Localization.instance.Localize(r.token);
                rows[i] = r;
            }

            int wPrefab = Math.Max("Prefab name".Length, rows.Max(r => r.prefab.Length));
            int wToken = Math.Max("Token".Length, rows.Max(r => r.token.Length));
            int wEnglish = Math.Max("English".Length, rows.Max(r => r.english.Length));
            int wLocalized = Math.Max("Localized name".Length, rows.Max(r => r.localized.Length));
            int wStack = Math.Max("Stack".Length, rows.Max(r => r.stack.Length));
            int wWeight = Math.Max("Weight".Length, rows.Max(r => r.weight.Length));
            int wType = Math.Max("Type".Length, rows.Max(r => r.type.Length));

            sb.AppendLine(
                $"{Pad("Prefab name", wPrefab)}   " +
                $"{Pad("Token", wToken)}   " +
                $"{Pad("English", wEnglish)}   " +
                $"{Pad("Localized name", wLocalized)}   " +
                $"{Pad("Stack", wStack)}   " +
                $"{Pad("Weight", wWeight)}   " +
                $"{Pad("Type", wType)}"
            );

            int totalWidth = wPrefab + wToken + wEnglish + wLocalized + wStack + wWeight + wType + 18;
            sb.AppendLine(new string('-', totalWidth));
            foreach (var r in rows)
            {
                sb.AppendLine(
                    $"{Pad(r.prefab, wPrefab)}   " +
                    $"{Pad(r.token, wToken)}   " +
                    $"{Pad(r.english, wEnglish)}   " +
                    $"{Pad(r.localized, wLocalized)}   " +
                    $"{Pad(r.stack, wStack)}   " +
                    $"{Pad(r.weight, wWeight)}   " +
                    $"{Pad(r.type, wType)}"
                );
            }

            itemCount = rows.Count;

            return sb.ToString();
        }
    }
}
