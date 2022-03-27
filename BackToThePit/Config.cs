using BepInEx.Configuration;
using UnityEngine;
using System;

namespace BackToThePit.Modules
{
    public static class Config
    {
        public static ConfigEntry<float> cooldownCoefficient;
        public static ConfigEntry<float> speechChance;
        public static ConfigEntry<float> volume;
        public static void ReadConfig()
        {
            volume = BackToThePit.instance.Config.Bind<float>(new ConfigDefinition("Narrator", "Volume"), 100f, new ConfigDescription("Volume of the narrator, 0-100"));
            cooldownCoefficient = BackToThePit.instance.Config.Bind<float>(new ConfigDefinition("Narrator", "Cooldown"), 1f, new ConfigDescription("Multiplier for the Narrator's speech cooldown. A value of 5 would mean 5x less speech, and a value of would 0.2 mean 5x more speech"));
            if (cooldownCoefficient.Value < 0)
                cooldownCoefficient.Value = 0;
        }
    }
}