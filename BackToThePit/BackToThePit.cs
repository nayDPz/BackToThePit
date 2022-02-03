using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using RoR2.UI;
using On;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using RiskOfOptions;

namespace BackToThePit
{
    [BepInDependency(R2API.R2API.PluginGUID)]
    [BepInDependency("com.rune580.riskofoptions")]
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [R2APISubmoduleDependency(nameof(SoundAPI), nameof(LanguageAPI))]
    public class BackToThePit : BaseUnityPlugin
	{
        public const string PluginGUID = PluginAuthor + "." + PluginName;
        public const string PluginAuthor = "badeepz";
        public const string PluginName = "BackToThePit";
        public const string PluginVersion = "1.0.0";

        internal static GameObject narratorController;
        public static BackToThePit instance;
        public static AudioManager.VolumeConVar cvNarratorVolume = new AudioManager.VolumeConVar("volume_narrator", ConVarFlags.Archive | ConVarFlags.Engine, "100", "The volume of the narrator, from 0 to 100.", "Volume_Narrator");


        public void Awake()
        {
            instance = this;
            Log.Init(Logger);

            ModSettingsManager.addOption(new ModOption(ModOption.OptionType.Slider, "Narrator Volume", "Volume of the narrator.", "100"));
            ModSettingsManager.setPanelTitle("BackToThePit Settings");
            ModSettingsManager.setPanelDescription("Configure settings for the BackToThePit mod");
            ModSettingsManager.addListener(ModSettingsManager.getOption("Narrator Volume"), new UnityEngine.Events.UnityAction<float>(floatEvent));

            Modules.Config.ReadConfig();

            using (Stream manifestResourceStream2 = Assembly.GetExecutingAssembly().GetManifestResourceStream("BackToThePit.NarratorBank.bnk"))
            {
                byte[] array = new byte[manifestResourceStream2.Length];
                manifestResourceStream2.Read(array, 0, array.Length);
                SoundAPI.SoundBanks.Add(array);
            }

            narratorController = new GameObject();
            narratorController.name = "NarratorController";
            narratorController.AddComponent<NarratorController>();
            NarratorController.Init();


            On.RoR2.UI.CharacterSelectController.SelectSurvivor += CharacterSelectController_SelectSurvivor;
            On.RoR2.HealthComponent.TriggerOneShotProtection += HealthComponent_TriggerOneShotProtection;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            On.RoR2.CharacterMaster.RespawnExtraLife += CharacterMaster_RespawnExtraLife;
            On.RoR2.GenericPickupController.GrantItem += GenericPickupController_GrantItem;
            On.RoR2.GenericPickupController.GrantEquipment += GenericPickupController_GrantEquipment;
            TeleporterInteraction.onTeleporterFinishGlobal += TeleporterInteraction_onTeleporterFinishGlobal;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt; 
            

            Log.LogInfo(nameof(Awake) + " done.");
        }

        private void Update()
        {
            Log.LogInfo(cvNarratorVolume.ToString());
        }

        private void TeleporterInteraction_onTeleporterFinishGlobal(TeleporterInteraction obj)
        {
            NarratorController.instance.RequestNarration(new NarrationRequest
            {
                soundString = "AnnounceTeleporterActivation",
                cooldown = 7f,
                priority = NarratorController.InterruptPriority.High
            });
        }

        public void floatEvent(float f)
        {
            cvNarratorVolume.AttemptSetString(f.ToString());
        }

        private void GenericPickupController_GrantEquipment(On.RoR2.GenericPickupController.orig_GrantEquipment orig, GenericPickupController self, CharacterBody body, Inventory inventory)
        {
            if (body.isPlayerControlled)
            {
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnouncePlayerPickup",
                    cooldown = 7f,
                    priority = NarratorController.InterruptPriority.Mid
                });
            }

            orig(self, body, inventory);
        }

        private void GenericPickupController_GrantItem(On.RoR2.GenericPickupController.orig_GrantItem orig, GenericPickupController self, CharacterBody body, Inventory inventory)
        {
            if(body.isPlayerControlled)
            {
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnouncePlayerPickup",
                    cooldown = 7f,
                    priority = NarratorController.InterruptPriority.Mid
                });
            }
            
            orig(self, body, inventory);
        }

        private void CharacterMaster_RespawnExtraLife(On.RoR2.CharacterMaster.orig_RespawnExtraLife orig, CharacterMaster self)
        {
            if(self.teamIndex == TeamIndex.Player)
            {
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnounceRevive",
                    cooldown = 7f,
                    priority = NarratorController.InterruptPriority.High
                });
            }         
            orig(self);
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {

            if (scene.name == "arena" || scene.name == "bazaar" || scene.name == "dampcavesimple" || scene.name == "foggyswamp" || scene.name == "frozenwall" ||
                scene.name == "goldshores" || scene.name == "golemplains" || scene.name == "golemplains2" || scene.name == "goolake" || scene.name == "moon2" || scene.name == "rootjungle" ||
                scene.name == "shipgraveyard" || scene.name == "wispgraveyard")
            {
                float i = UnityEngine.Random.value;
                if (i > 0.2f)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "Announce" + scene.name,
                        cooldown = 7f,
                        priority = NarratorController.InterruptPriority.High
                    });
                }
                else
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceGenericLevel",
                        cooldown = 7f,
                        priority = NarratorController.InterruptPriority.High
                    });
                }
            }
            else if (scene.name == "mysteryspace" || scene.name == "limbo" || scene.name == "artifactworld")
            {
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnounceHiddenRealm",
                    cooldown = 7f,
                    priority = NarratorController.InterruptPriority.High
                });
            }    
            else if (scene.name == "blackbeach" || scene.name == "blackbeach2" || scene.name == "skymeadow")
            {
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnounceGenericLevel",
                    cooldown = 7f,
                    priority = NarratorController.InterruptPriority.High
                });
            }
            else if(scene.name != "lobby")
            {
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "Buffer",
                    cooldown = 7f,
                    priority = NarratorController.InterruptPriority.High
                });
            }
        }

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            if(self.body && self.body.teamComponent && self.body.teamComponent.teamIndex == TeamIndex.Player)
            {
                if (amount > self.fullCombinedHealth * 0.6f)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceBigHeal",
                        cooldown = 10f,
                        priority = NarratorController.InterruptPriority.Low
                    });
                    return orig(self, amount, procChainMask, nonRegen);
                }
                else if (amount > self.fullCombinedHealth * 0.1f && nonRegen)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceHeal",
                        cooldown = 10f,
                        priority = NarratorController.InterruptPriority.Damage
                    });
                    return orig(self, amount, procChainMask, nonRegen);
                }
            }
            

            return orig(self, amount, procChainMask, nonRegen);
        }

        private void HealthComponent_TriggerOneShotProtection(On.RoR2.HealthComponent.orig_TriggerOneShotProtection orig, HealthComponent self)
        {
            NarratorController.instance.RequestNarration(new NarrationRequest
            {
                soundString = "AnnounceTriggerOSP",
                cooldown = 10f,
                priority = NarratorController.InterruptPriority.High
            });
            orig(self);
        }

        private void GlobalEventManager_onServerDamageDealt(DamageReport report)
        {
            if (report.attackerTeamIndex == TeamIndex.Player && report.victimBody)
            {
                if(report.damageDealt > report.attackerBody.damage * 7.5f)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceBigHit",
                        cooldown = 10f,
                        priority = NarratorController.InterruptPriority.Damage
                    });
                    return;
                }
            }
            if (report.victimTeamIndex == TeamIndex.Player && report.victimBody && !report.isFallDamage)
            {
                if (report.victim.combinedHealthFraction < 0.15f)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceLowHP",
                        cooldown = 10f,
                        priority = NarratorController.InterruptPriority.Mid
                    });
                    return;
                }
            }
        }

        private void CharacterSelectController_SelectSurvivor(On.RoR2.UI.CharacterSelectController.orig_SelectSurvivor orig, CharacterSelectController self, SurvivorIndex survivor)
        {
            GameObject bodyPrefab = SurvivorCatalog.GetSurvivorDef(survivor).bodyPrefab;

            NarratorController.instance.RequestNarration(new NarrationRequest 
            { 
                soundString = "Announce" + bodyPrefab.name, 
                cooldown = 0f, 
                priority = NarratorController.InterruptPriority.CharacterSelect 
            });

            orig(self, survivor);
        }

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            if (report.victimBody.isPlayerControlled)
            {
                
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnouncePlayerDeath",
                    cooldown = 10f,
                    priority = NarratorController.InterruptPriority.High
                });
                return;
            }
            if(report.attackerTeamIndex == TeamIndex.Player && report.victimBody)
            {
                if(report.victimBody.name.Contains("WormBody"))
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillWorm",
                        cooldown = 3f,
                        priority = NarratorController.InterruptPriority.High
                    });
                    return;
                }
                if (report.victimBody.name.Contains("SuperRoboBallBoss"))
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillAWU",
                        cooldown = 3f,
                        priority = NarratorController.InterruptPriority.High
                    });
                    return;
                }
                if (report.victimIsBoss && report.victimIsChampion)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillBig",
                        cooldown = 3f,
                        priority = NarratorController.InterruptPriority.High
                    });
                    return;
                }

                if(report.damageInfo.damageType == DamageType.DoT)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillDot",
                        cooldown = 10f,
                        priority = NarratorController.InterruptPriority.Low
                    });
                    return;
                }

                if (report.damageInfo.damage > report.victimBody.maxHealth)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceOverkill",
                        cooldown = 8f,
                        priority = NarratorController.InterruptPriority.Low
                    });
                    return;
                }

                float mass = 50f;
                CharacterMotor motor = report.victimBody.GetComponent<CharacterMotor>();
                Rigidbody rigidBody = report.victimBody.GetComponent<Rigidbody>();
                if (motor) mass = motor.mass;
                else if (rigidBody) mass = rigidBody.mass;

                if(mass > 200f)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillMedium",
                        cooldown = 8f,
                        priority = NarratorController.InterruptPriority.Low
                    });
                    return;
                }
                else
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillWeak",
                        cooldown = 10f,
                        priority = NarratorController.InterruptPriority.Low
                    });
                    return;
                }


            }
        }

        
        

        
    }
}
