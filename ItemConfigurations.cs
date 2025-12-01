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
        public Dictionary<string, float> weightMultiplier = new(StringComparer.OrdinalIgnoreCase)
        {
            { global, 1f }
        };

        [YamlMember(Alias = "WeightAmount")]
        public Dictionary<string, float> weightAmount = new(StringComparer.OrdinalIgnoreCase)
        {
             { "Wood", 2f }
        };

        [YamlMember(Alias = "StackMultiplier")]
        public Dictionary<string, float> stackMultiplier = new(StringComparer.OrdinalIgnoreCase)
        {
            { global, 1f }
        };

        [YamlMember(Alias = "StackSize")]
        public Dictionary<string, int> stackSize = new(StringComparer.OrdinalIgnoreCase)
        {
             { "Wood", 50 }
        };

        public void Serialize(ref ZPackage pkg)
        {
            WriteDict(pkg, weightMultiplier);
            WriteDict(pkg, weightAmount);
            WriteDict(pkg, stackMultiplier);
            WriteDict(pkg, stackSize);
        }

        public void Deserialize(ref ZPackage pkg)
        {
            ReadDict(pkg, weightMultiplier);
            ReadDict(pkg, weightAmount);
            ReadDict(pkg, stackMultiplier);
            ReadDict(pkg, stackSize);
        }

        public void Load(ItemConfigurations itemConfigurations)
        {
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
