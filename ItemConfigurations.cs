using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using YamlDotNet.Serialization;

namespace ItemStacksItemWeights
{
    public class ItemConfigurations : ISerializableParameter
    {
        public const string global = "Global";

        [YamlMember(Alias = "WeightMultiplier")]
        public Dictionary<string, float> weightMultiplier = new(StringComparer.OrdinalIgnoreCase);

        [YamlMember(Alias = "WeightAmount")]
        public Dictionary<string, float> weightAmount = new(StringComparer.OrdinalIgnoreCase);

        [YamlMember(Alias = "StackMultiplier")]
        public Dictionary<string, float> stackMultiplier = new(StringComparer.OrdinalIgnoreCase);

        [YamlMember(Alias = "StackSize")]
        public Dictionary<string, int> stackSize = new(StringComparer.OrdinalIgnoreCase);

        public static ItemConfigurations CreateDefaultTemplate()
        {
            return new ItemConfigurations
            {
                weightMultiplier = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { global, 1f }
                },
                weightAmount = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Wood", 2f }
                },
                stackMultiplier = new Dictionary<string, float>(StringComparer.OrdinalIgnoreCase)
                {
                    { global, 1f }
                },
                stackSize = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Wood", 50 }
                }
            };
        }

        public void EnsureInitialized()
        {
            weightMultiplier = NormalizeDictionary(weightMultiplier);
            weightAmount = NormalizeDictionary(weightAmount);
            stackMultiplier = NormalizeDictionary(stackMultiplier);
            stackSize = NormalizeDictionary(stackSize);
        }

        private static Dictionary<string, T> NormalizeDictionary<T>(Dictionary<string, T> source)
        {
            if (source != null && source.Comparer.Equals(StringComparer.OrdinalIgnoreCase))
                return source;

            Dictionary<string, T> result = new(StringComparer.OrdinalIgnoreCase);
            if (source != null)
                foreach (KeyValuePair<string, T> pair in source)
                    result[pair.Key] = pair.Value;

            return result;
        }

        public void Validate()
        {
            EnsureInitialized();

            foreach (KeyValuePair<string, int> entry in stackSize)
                if (entry.Value < 1)
                    throw new FormatException($"StackSize for '{entry.Key}' must be positive.");

            ValidateNumbers(stackMultiplier, "StackMultiplier", allowZero: false);
            ValidateNumbers(weightMultiplier, "WeightMultiplier", allowZero: true);
            ValidateNumbers(weightAmount, "WeightAmount", allowZero: true);
        }

        private static void ValidateNumbers(Dictionary<string, float> values, string section, bool allowZero)
        {
            foreach (KeyValuePair<string, float> entry in values)
                if (float.IsNaN(entry.Value) || float.IsInfinity(entry.Value) || entry.Value < 0f || (!allowZero && entry.Value == 0f))
                    throw new FormatException($"{section} for '{entry.Key}' must be finite and {(allowZero ? "non-negative" : "positive")}.");
        }

        public void Serialize(ref ZPackage pkg)
        {
            EnsureInitialized();
            WriteDict(pkg, weightMultiplier);
            WriteDict(pkg, weightAmount);
            WriteDict(pkg, stackMultiplier);
            WriteDict(pkg, stackSize);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            EnsureInitialized();
            ReadDict(pkg, weightMultiplier);
            ReadDict(pkg, weightAmount);
            ReadDict(pkg, stackMultiplier);
            ReadDict(pkg, stackSize);
        }

        public void Load(ItemConfigurations itemConfigurations)
        {
            EnsureInitialized();
            if (ReferenceEquals(this, itemConfigurations))
                return;

            itemConfigurations ??= new ItemConfigurations();
            itemConfigurations.EnsureInitialized();

            weightMultiplier.Clear();
            weightAmount.Clear();
            stackMultiplier.Clear();
            stackSize.Clear();

            weightMultiplier.Copy(itemConfigurations.weightMultiplier);
            weightAmount.Copy(itemConfigurations.weightAmount);
            stackMultiplier.Copy(itemConfigurations.stackMultiplier);
            stackSize.Copy(itemConfigurations.stackSize);
        }

        public void FillItemNames()
        {
            weightMultiplier.Keys.ToList().Do(key => weightMultiplier[key.GetItemName()] = weightMultiplier[key]);
            weightAmount.Keys.ToList().Do(key => weightAmount[key.GetItemName()] = weightAmount[key]);
            stackMultiplier.Keys.ToList().Do(key => stackMultiplier[key.GetItemName()] = stackMultiplier[key]);
            stackSize.Keys.ToList().Do(key => stackSize[key.GetItemName()] = stackSize[key]);
        }

        private void ReadDict(ZPackage pkg, Dictionary<string, float> dictionary)
        {
            dictionary.Clear();

            int num = pkg.ReadInt();
            for (int index = 0; index < num; ++index)
                dictionary[pkg.ReadString()] = pkg.ReadSingle();
        }

        private void ReadDict(ZPackage pkg, Dictionary<string, int> dictionary)
        {
            dictionary.Clear();

            int num = pkg.ReadInt();
            for (int index = 0; index < num; ++index)
                dictionary[pkg.ReadString()] = pkg.ReadInt();
        }

        private void WriteDict(ZPackage pkg, Dictionary<string, float> dictionary)
        {
            pkg.Write(dictionary.Count);
            foreach (var item in dictionary)
            {
                pkg.Write(item.Key);
                pkg.Write(item.Value);
            }
        }

        private void WriteDict(ZPackage pkg, Dictionary<string, int> dictionary)
        {
            pkg.Write(dictionary.Count);
            foreach (var item in dictionary)
            {
                pkg.Write(item.Key);
                pkg.Write(item.Value);
            }
        }
    }
}
