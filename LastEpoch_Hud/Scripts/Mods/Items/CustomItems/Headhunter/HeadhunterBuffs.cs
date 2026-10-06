using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

public static class HeadhunterBuffs
{
    private const int MaxStack = 10;
    private const string BuffNamePrefix = "HHBuff";

    private static List<HeadhunterBuffEntry> _addBuffs = new();
    private static List<HeadhunterBuffEntry> _increasedBuffs = new();

    public static void GenerateBuffs()
    {
        int nbBuff = Random.Range(
            Save_Manager.instance.data.Items.Headhunter.MinGenerated,
            Save_Manager.instance.data.Items.Headhunter.MaxGenerated + 1
        );
        for (int i = 0; i < nbBuff; i++)
        {
            Actor playerActor = PlayerFinder.getPlayerActor();
            Buff randomBuff = GenerateRandomBuff();
            int maxAddValue = MaxStack;
            int maxIncreasedValue = MaxStack;
            bool found = false;
            foreach (HeadhunterBuffEntry entry in HeadhunterConfig.BuffConfig)
            {
                if (randomBuff.name.Contains(entry.Property))
                {
                    if ((entry.MaxAdded > 0) && (entry.MaxAdded < maxAddValue))
                    {
                        maxAddValue = entry.MaxAdded;
                    }
                    if ((entry.MaxIncreased > 0) && (entry.MaxIncreased < maxIncreasedValue))
                    {
                        maxIncreasedValue -= entry.MaxIncreased;
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                Main.logger_instance?.Msg("Error : Property " + randomBuff.name + " Not Found");
            }
            if (randomBuff != null)
            {
                float oldValue;
                string buffToRemove = "";
                foreach (Buff playerBuff in playerActor.statBuffs.buffs)
                {
                    if (playerBuff.name.Contains(randomBuff.name))
                    {
                        buffToRemove = playerBuff.name;
                        float newValue;
                        if (!GetIsIncrease(playerBuff))
                        {
                            oldValue = playerBuff.stat.addedValue;
                            if (oldValue < maxAddValue)
                            {
                                newValue =
                                    oldValue + Save_Manager.instance.data.Items.Headhunter.AddValue;
                            }
                            else
                            {
                                newValue = maxAddValue;
                            }
                            randomBuff.stat.addedValue = newValue;
                        }
                        else
                        {
                            oldValue = playerBuff.stat.increasedValue;
                            if (oldValue < maxIncreasedValue)
                            {
                                newValue =
                                    oldValue
                                    + Save_Manager.instance.data.Items.Headhunter.IncreasedValue;
                            }
                            else
                            {
                                newValue = maxIncreasedValue;
                            }
                            randomBuff.stat.increasedValue = newValue;
                        }
                        break;
                    }
                }
                if (buffToRemove != null)
                {
                    playerActor.statBuffs.removeBuffsWithName(buffToRemove);
                }
                playerActor.statBuffs.addBuff(
                    randomBuff.remainingDuration,
                    randomBuff.stat.property,
                    randomBuff.stat.addedValue,
                    randomBuff.stat.increasedValue,
                    randomBuff.stat.moreValues,
                    randomBuff.stat.tags,
                    randomBuff.stat.specialTag,
                    0,
                    randomBuff.name
                );
            }
        }
    }

    public static void GenerateBuffsList()
    {
        _addBuffs = new List<HeadhunterBuffEntry>();
        _increasedBuffs = new List<HeadhunterBuffEntry>();
        for (int i = 0; i < System.Enum.GetValues(typeof(SP)).Length; i++)
        {
            var property = (SP)i;
            foreach (HeadhunterBuffEntry entry in HeadhunterConfig.BuffConfig)
            {
                if (entry.Property == property.ToString())
                {
                    if (entry.Added)
                    {
                        _addBuffs.Add(entry);
                    }
                    if (entry.Increased)
                    {
                        _increasedBuffs.Add(entry);
                    }
                    break;
                }
            }
        }
    }

    private static SP GetPropertyFromName(string name)
    {
        SP result = SP.None;
        for (int i = 0; i < System.Enum.GetValues(typeof(SP)).Length; i++)
        {
            var property = (SP)i;
            if (name == property.ToString())
            {
                result = property;
                break;
            }
        }

        return result;
    }

    private static Buff GenerateRandomBuff()
    {
        Buff result = null;
        byte specialTag = 0;
        AT tags = AT.None;
        float addedValue = 0;
        float increasedValue = 0;
        string name = BuffNamePrefix + " : ";
        SP property;

        if (_addBuffs.Count == 0 && _increasedBuffs.Count == 0)
        {
            return result;
        }
        bool added = ChooseAdded();
        if (added)
        {
            int random = Random.Range(0, _addBuffs.Count);
            if (random > _addBuffs.Count)
            {
                random = _addBuffs.Count - 1;
            }
            string propertyName = _addBuffs[random].Property;
            property = GetPropertyFromName(propertyName);
            name += "Add ";
            addedValue = Save_Manager.instance.data.Items.Headhunter.AddValue;
        }
        else
        {
            int random = Random.Range(0, _increasedBuffs.Count);
            if (random > _increasedBuffs.Count)
            {
                random = _increasedBuffs.Count - 1;
            }
            string propertyName = _increasedBuffs[random].Property;
            property = GetPropertyFromName(propertyName);
            name += "Increased ";
            increasedValue = Save_Manager.instance.data.Items.Headhunter.IncreasedValue;
        }
        if (property != SP.None)
        {
            name += property.ToString();
            result = new Buff
            {
                name = name,
                remainingDuration = Save_Manager.instance.data.Items.Headhunter.BuffDuration,
                stat = new Stats.Stat
                {
                    addedValue = addedValue,
                    increasedValue = increasedValue,
                    moreValues = null,
                    property = property,
                    specialTag = specialTag,
                    tags = tags,
                },
            };
        }

        return result;
    }

    private static bool ChooseAdded()
    {
        if (_addBuffs.Count == 0)
        {
            return false;
        }
        if (_increasedBuffs.Count == 0)
        {
            return true;
        }

        return Random.Range(0, 2) == 0;
    }

    private static bool GetIsIncrease(Buff b)
    {
        return b.name.Contains("Increased");
    }
}
