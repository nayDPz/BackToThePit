using BepInEx;
using R2API;
using R2API.Utils;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using RoR2.UI;
using On;
using System;
using System.Collections.Generic;

namespace BackToThePit
{
    public class NarratorController : NetworkBehaviour
    {
        public static NarratorController instance;

        public uint currentSoundId = 0;
        public NarrationRequest currentNarration;
        private float cooldownTimer;

        public enum InterruptPriority
        {
            CharacterSelect,
            Damage,
            Low, // Kills, Item Pickup, Healed, Damaged
            Mid, // Crits, Interactibles
            High, // Boss kill, Boss spawn, Death, Revive, Enter stage
            Death,
        }

        public void RequestNarration(NarrationRequest request)
        {
            RequestNarration(request, base.gameObject);
        }

        public void RequestNarration(NarrationRequest request, GameObject gameObject)
        {
            //Log.LogInfo("Requesting " + request.soundString);
            string soundString = request.soundString;
            InterruptPriority overridePriority = request.priority;
            float cooldown = request.cooldown;

            if(true)//UnityEngine.Random.value < Modules.Config.speechChance.Value)
            {
                if (this.currentNarration == null)
                {
                    Log.LogInfo("Playing " + soundString);
                    AkSoundEngine.StopPlayingID(this.currentSoundId);
                    this.currentSoundId = Util.PlaySound(soundString, gameObject);

                    this.cooldownTimer = cooldown;
                    this.currentNarration = request;
                }
                else if (overridePriority > this.currentNarration.priority || this.cooldownTimer <= 0f)
                {
                    Log.LogInfo("Playing " + soundString);

                    AkSoundEngine.StopPlayingID(this.currentSoundId);

                    this.currentSoundId = Util.PlaySound(soundString, gameObject);

                    this.cooldownTimer = cooldown * Modules.Config.cooldownCoefficient.Value;
                    this.currentNarration = request;

                }
            }
            else
            {
                Log.LogInfo("Request for " + request.soundString + " rolled to skip");
                this.cooldownTimer = 1f;
            }
            
        }

        private void OnDisable()
        {
            AkSoundEngine.StopPlayingID(this.currentSoundId);
        }
        private void OnDestroy()
        {
            AkSoundEngine.StopPlayingID(this.currentSoundId);
        }
        private void FixedUpdate()
        {
            if (this.cooldownTimer > 0f)
                this.cooldownTimer -= Time.fixedDeltaTime;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init()
        {
            RoR2Application.onLoad = (Action)Delegate.Combine(RoR2Application.onLoad, new Action(delegate ()
            {
                NarratorController.instance = UnityEngine.Object.Instantiate<GameObject>(BackToThePit.narratorController, RoR2Application.instance.transform).GetComponent<NarratorController>();
            }));
        }

    }

    public class NarrationRequest
    {
        public string soundString = "";
        public NarratorController.InterruptPriority priority = NarratorController.InterruptPriority.CharacterSelect;
        public float cooldown = 0f;
    }
}
