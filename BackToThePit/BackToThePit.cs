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
        public const string PluginGUID = "com." + PluginAuthor + "." + PluginName;
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


            Modules.Config.ReadConfig();

            

            ModSettingsManager.AddOption(new RiskOfOptions.Options.SliderOption(Modules.Config.volume));

            Modules.Config.volume.SettingChanged += (object sender, EventArgs args) => { cvNarratorVolume.AttemptSetString(Modules.Config.volume.Value.ToString()); };

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

            On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.RebuildMannequinInstance += SurvivorMannequinSlotController_RebuildMannequinInstance;
            On.RoR2.HealthComponent.TriggerOneShotProtection += HealthComponent_TriggerOneShotProtection;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            On.RoR2.CharacterMaster.RespawnExtraLife += CharacterMaster_RespawnExtraLife;
            On.RoR2.ItemDef.AttemptGrant += ItemDef_AttemptGrant;
            On.RoR2.EquipmentDef.AttemptGrant += EquipmentDef_AttemptGrant;
            TeleporterInteraction.onTeleporterFinishGlobal += TeleporterInteraction_onTeleporterFinishGlobal;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt; 
            

            Log.LogInfo("Remind yourself that overconfidence is a slow and insidious killer.");
        }

        public void Start()
        {
            cvNarratorVolume.AttemptSetString(Modules.Config.volume.Value.ToString());
        }

        private void SurvivorMannequinSlotController_RebuildMannequinInstance(On.RoR2.SurvivorMannequins.SurvivorMannequinSlotController.orig_RebuildMannequinInstance orig, RoR2.SurvivorMannequins.SurvivorMannequinSlotController self)
        {
            GameObject bodyPrefab = self.currentSurvivorDef.bodyPrefab;

            NarratorController.instance.RequestNarration(new NarrationRequest
            {
                soundString = "Announce" + bodyPrefab.name,
                cooldown = 0f,
                priority = NarratorController.InterruptPriority.CharacterSelect
            });

            orig(self);
        }

        private void EquipmentDef_AttemptGrant(On.RoR2.EquipmentDef.orig_AttemptGrant orig, ref PickupDef.GrantContext context)
        {
            if (context.body.isPlayerControlled)
            {
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnouncePlayerPickup",
                    cooldown = 12f,
                    priority = NarratorController.InterruptPriority.High
                });
            }
            orig(ref context);
        }

        private void ItemDef_AttemptGrant(On.RoR2.ItemDef.orig_AttemptGrant orig, ref PickupDef.GrantContext context)
        {
            if (context.body.isPlayerControlled)
            {
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnouncePlayerPickup",
                    cooldown = 12f,
                    priority = NarratorController.InterruptPriority.High
                });
            }
            orig(ref context);
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
            if(NarratorController.instance)
            {
                if (scene.name == "arena" || scene.name == "bazaar" || scene.name == "dampcavesimple" || scene.name == "foggyswamp" || scene.name == "frozenwall" ||
                                scene.name == "goldshores" || scene.name == "golemplains" || scene.name == "golemplains2" || scene.name == "goolake" || scene.name == "moon2" || scene.name == "rootjungle" ||
                                scene.name == "shipgraveyard" || scene.name == "wispgraveyard")
                {
                    float i = UnityEngine.Random.value;
                    if (i > 0.2f)
                    {
                        string name = scene.name;
                        if(scene.name == "golemplains2")
                            name = "golemplains";
                        NarratorController.instance.RequestNarration(new NarrationRequest
                        {
                            soundString = "Announce" + name,
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
                else if (scene.name != "lobby")
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "Buffer",
                        cooldown = 2f,
                        priority = NarratorController.InterruptPriority.High
                    });
                }
            }
            
        }

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            if(self.body.isPlayerControlled)
            {
                if (amount > self.fullCombinedHealth * 0.6f)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceBigHeal",
                        cooldown = 10f,
                        priority = NarratorController.InterruptPriority.Damage
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
            if(report.attackerBody)
            {
                if (report.attackerBody.isPlayerControlled && report.victimBody)
                {
                    if (report.damageDealt > report.attackerBody.damage * 7.5f)
                    {
                        NarratorController.instance.RequestNarration(new NarrationRequest
                        {
                            soundString = "AnnounceBigHit",
                            cooldown = 15f,
                            priority = NarratorController.InterruptPriority.Low
                        });
                        return;
                    }
                }
            }
            
            if (report.victimBody && report.victimBody.isPlayerControlled && !report.isFallDamage)
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

        private void GlobalEventManager_onCharacterDeathGlobal(DamageReport report)
        {
            if (report.victimBody.isPlayerControlled)
            {
                
                NarratorController.instance.RequestNarration(new NarrationRequest
                {
                    soundString = "AnnouncePlayerDeath",
                    cooldown = 10f,
                    priority = NarratorController.InterruptPriority.Death
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
                        cooldown = 15f,
                        priority = NarratorController.InterruptPriority.High
                    });
                    return;
                }
                if (report.victimBody.name.Contains("SuperRoboBallBoss"))
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillAWU",
                        cooldown = 15f,
                        priority = NarratorController.InterruptPriority.High
                    });
                    return;
                }
                if (report.victimIsBoss && report.victimIsChampion)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillBig",
                        cooldown = 15f,
                        priority = NarratorController.InterruptPriority.High
                    });
                    return;
                }

                if(report.damageInfo.damageType == DamageType.DoT)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillDot",
                        cooldown = 15f,
                        priority = NarratorController.InterruptPriority.Mid
                    });
                    return;
                }

                if (report.damageInfo.damage > report.victimBody.maxHealth)
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceOverkill",
                        cooldown = 15f,
                        priority = NarratorController.InterruptPriority.Mid
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
                        cooldown = 16f,
                        priority = NarratorController.InterruptPriority.Mid
                    });
                    return;
                }
                else
                {
                    NarratorController.instance.RequestNarration(new NarrationRequest
                    {
                        soundString = "AnnounceKillWeak",
                        cooldown = 16f,
                        priority = NarratorController.InterruptPriority.Mid
                    });
                    return;
                }


            }
        }

        
        

        
    }
}
