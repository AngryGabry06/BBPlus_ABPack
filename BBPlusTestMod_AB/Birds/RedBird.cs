using HarmonyLib;
using HPSystem;
using MTM101BaldAPI;
using MTM101BaldAPI.Components.Animation;
using MTM101BaldAPI.Reflection;
using MTM101BaldAPI.Registers;
using Rewired;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace bbplus.angrybirdspack.Birds
{
    public class RedBird : NPC
    {
        public SoundObject sndRedAngry, sndRedAngered, sndRedCaught, sndRedExhausted, sndRedDamage;
        public SoundObject sndRedHappy;
        public List<SoundObject> sndRedIdle = new List<SoundObject>();
        public List<SoundObject> sndRedDeath = new List<SoundObject>();
        public List<SoundObject> sndRedStump = new List<SoundObject>();
        public Sprite sprRedIdle, sprRedYell, sprRedSpecial, sprGauge;
        public Sprite sprRedBlink, sprRedCollision;
        public Sprite sprRedEat1, sprRedEat2;
        public PropagatedAudioManager audManager;
        public AudioManager audManFar;

        private bool isIdleAnimationActive = false;
        private Coroutine idleAnim;
        private float cSpeed = 0f, nSpeed = 0f, aSpeed = 0f;

        private Coroutine hopCoroutine;
        private Vector3 originalSpritePos = Vector3.zero;
        private bool isHopping = false;
        private bool isWeak = false;
        private bool isEating = false;
        private bool isProcessingTrigger = false;

        private PlayerManager angryTargetPlayer;
        private Baldi angryTargetBaldi;
        readonly MovementModifier freezeMod = new MovementModifier(Vector3.zero, 0f);
        private float sDelay, cDelay, aDelay, fDelay;
        private bool hasCalled = false;
        public bool isAngry = false;
        public bool isGoldenAngry = false;
        public int aCount = 0, aCountMax, aCountMin;
        public int eCount = 0, eCountBuild = 0, eCountMax, eCountMin;
        public int ebCount = 0, ebCountBuild = 0, ebCountMax, ebCountMin;
        private float chance;
        //private bool hadItem;

        public override void Initialize()
        {
            base.Initialize();

            eCountMax = 4;
            eCountMin = 3;
            eCountBuild = UnityEngine.Random.Range(eCountMin, eCountMax);

            if (BoolChecks.isAngryBaldi)
            {
                ebCountMax = 4;
                ebCountMin = 3;
                ebCountBuild = UnityEngine.Random.Range(ebCountMin, ebCountMax);
            }

            chance = UnityEngine.Random.Range(1f, 100f);

            if (spriteRenderer.Length > 0) { spriteRenderer[0].sprite = sprRedIdle; }
            StayStill(false);

            AudioManager audM = GetComponent<AudioManager>();
            audManager = gameObject.GetComponent<PropagatedAudioManager>() ?? gameObject.AddComponent<PropagatedAudioManager>();

            AudioSource newSource = gameObject.GetComponent<AudioSource>() ?? gameObject.AddComponent<AudioSource>();
            newSource.spatialBlend = 0f;
            audManager.audioDevice = newSource;

            audManFar = GetComponent<AudioManager>();

            if (!BoolChecks.isNerfed)
            {
                aCountMax = 3;
                aCountMin = aCountMax - 1;
                nSpeed = 24f;
                aSpeed = 32f;
            }
            else
            {
                aCountMax = UnityEngine.Random.Range(6, 8);
                aCountMin = aCountMax - 1;
                nSpeed = 18f;
                aSpeed = 26f;
            }

            cSpeed = nSpeed;
            navigator.maxSpeed = cSpeed;
            navigator.SetSpeed(cSpeed);

            behaviorStateMachine.ChangeState(new RedBird_Wander(this));
            StartIdleAnimation();
        }

        public bool PlayerHasGoldenEgg(PlayerManager pm)
        {
            if (pm == null || pm.itm == null) return false;

            global::Items goldenEggType = EnumExtensions.GetFromExtendedName<global::Items>("GoldenEgg");
            for (int i = 0; i < pm.itm.maxItem; i++)
            {
                ItemObject item = pm.itm.items[i];
                if (item != null && item.itemType == goldenEggType) { return true; }
            }
            return false;
        }

        public void TriggerGoldenEggAnger(PlayerManager pm)
        {
            if (!isGoldenAngry)
            {
                if (isAngry || isWeak || isEating) return;

                isGoldenAngry = true;
                BoolChecks.isAngryGolden = true;

                angryTargetPlayer = pm;
                aCount = aCountMax;
                Angered();

                if (ec == null || ec.Npcs == null) return;
                foreach (NPC npc in ec.Npcs)
                {
                    if (npc is YellowBird yellow)
                    {
                        yellow.Angered();
                        yellow.SetAngered();
                    }
                    else if (npc is BlackBird black)
                    {
                        black.Angered();
                        black.SetAngered();
                    }
                }
            }
        }

        public void PlayRandomSound(List<SoundObject> sounds) { audManager.PlayRandomAudio(sounds.ToArray()); }

        public void PlayRandomSoundFar(List<SoundObject> sounds) { audManFar.PlayRandomAudio(sounds.ToArray()); }

        public IEnumerator DoHop(float height = 0.75f, float duration = 0.3f)
        {
            if (spriteRenderer == null || spriteRenderer.Length == 0) yield break;

            Transform spriteTransform = spriteRenderer[0].transform;

            if (!isHopping) { originalSpritePos = spriteTransform.localPosition; }

            isHopping = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += ec.NpcTimeScale * Time.deltaTime;
                float progress = Mathf.Clamp01(elapsed / duration);

                float yOffset = Mathf.Sin(progress * Mathf.PI) * height;
                spriteTransform.localPosition = new Vector3(originalSpritePos.x, originalSpritePos.y + yOffset, originalSpritePos.z);

                yield return null;
            }

            spriteTransform.localPosition = originalSpritePos;
            isHopping = false;
            hopCoroutine = null;
        }

        public void TriggerHop(float height = 0.75f, float duration = 0.3f) { if (!isHopping && gameObject.activeInHierarchy) { hopCoroutine = StartCoroutine(DoHop(height, duration)); } }

        public void ResetHop()
        {
            if (hopCoroutine != null)
            {
                StopCoroutine(hopCoroutine);
                hopCoroutine = null;
            }
            if (spriteRenderer != null && spriteRenderer.Length > 0 && isHopping) { spriteRenderer[0].transform.localPosition = originalSpritePos; }
            isHopping = false;
        }

        private IEnumerator AnimationIdle(float sDelay)
        {
            yield return new WaitForSeconds(sDelay);

            while (isIdleAnimationActive)
            {
                if (spriteRenderer.Length > 0) { spriteRenderer[0].sprite = sprRedBlink; }
                yield return new WaitForSeconds(0.1f);
                if (spriteRenderer.Length > 0) { spriteRenderer[0].sprite = sprRedIdle; }

                if (UnityEngine.Random.value < 0.60f) { TriggerHop(0.6f, 0.35f); }

                float waitTime = UnityEngine.Random.Range(2f, 4f);
                float elapsed = 0f;

                while (elapsed < waitTime && isIdleAnimationActive)
                {
                    elapsed += ec.NpcTimeScale * Time.deltaTime;
                    yield return null;
                }
            }

            idleAnim = null;
        }

        private void StopIdleAnimation()
        {
            if (idleAnim != null)
            {
                StopCoroutine(idleAnim);
                idleAnim = null;
            }
            isIdleAnimationActive = false;
        }

        private void StartIdleAnimation()
        {
            if (idleAnim == null && !isIdleAnimationActive)
            {
                isIdleAnimationActive = true;
                idleAnim = StartCoroutine(AnimationIdle(UnityEngine.Random.Range(1f, 2.5f)));
            }
        }

        protected override void VirtualOnTriggerEnter(Entity otherEntity, Collider other)
        {
            base.VirtualOnTriggerEnter(otherEntity, other);
            if (otherEntity == null || isWeak || isEating || isProcessingTrigger) return;

            PlayerManager playerManager = otherEntity.GetComponent<PlayerManager>();
            if (playerManager != null)
            {
                if (!playerManager.Tagged || !BoolChecks.isNerfed)
                {
                    ResetHop();
                    if (aCount <= aCountMin) { aCount++; }

                    if (!isAngry && !hasCalled && aCount <= aCountMin)
                    {
                        PlayRandomSound(sndRedIdle);
                        ec.MakeNoise(transform.position, 85);

                        StopIdleAnimation();
                        StartCoroutine(SpriteYell(0.8f));
                        StartCoroutine(CallCooldown(40f));
                    }
                    else if (!isAngry && hasCalled && aCount <= aCountMin)
                    {
                        if (aCount <= aCountMax) { audManager.PlaySingle(sndRedAngry); }
                        StopIdleAnimation();
                        StartCoroutine(SpriteAnger(1.25f));
                    }
                    else if (!isAngry && aCount == aCountMax)
                    {
                        angryTargetPlayer = playerManager;
                        if (PlayerHasGoldenEgg(playerManager)) { TriggerGoldenEggAnger(playerManager); }
                        else { Angered(); }
                    }
                    else if (isAngry || BoolChecks.isAngryGolden) //else if (isAngry && angryTargetPlayer != null)
                    {
                        aCount = 0;
                        StartCoroutine(Caught(1.5f, playerManager));
                    }
                }
                return;
            }

            if (BoolChecks.isAngryBaldi)
            {
                Baldi baldi = otherEntity.GetComponent<Baldi>();
                if (baldi != null)
                {
                    ResetHop();
                    if (ebCount < ebCountBuild) { ebCount++; }

                    if (!isAngry && !hasCalled && ebCount < ebCountBuild)
                    {
                        PlayRandomSound(sndRedIdle);

                        StopIdleAnimation();
                        StartCoroutine(SpriteYell(0.8f));
                        StartCoroutine(CallCooldown(40f));
                    }
                    else if (!isAngry && hasCalled && ebCount < ebCountBuild)
                    {
                        if (ebCount <= ebCountMax) { audManager.PlaySingle(sndRedAngry); }
                        StopIdleAnimation();
                        StartCoroutine(SpriteAnger(1.25f));
                    }
                    else if (!isAngry && ebCount >= ebCountBuild)
                    {
                        angryTargetBaldi = baldi;
                        AngeredBaldi();
                    }
                    else if (isAngry && angryTargetBaldi == baldi)
                    {
                        ebCount = 0;
                        StartCoroutine(CaughtBaldi(1.5f, baldi));
                    }
                }
            }
        }

        public void SetAngered() { aCount = aCountMax; }

        public void Angered()
        {
            if (isAngry || isProcessingTrigger) return;
            ResetHop();

            BoolChecks.isMusic++;
            int totalBirdsOnFloor = ec.Npcs.OfType<NPC>().Count(npc => npc is RedBird || npc is BlackBird || npc is YellowBird);
            if (totalBirdsOnFloor > 0 && BoolChecks.isMusic >= totalBirdsOnFloor)
            {
                if (!BoolChecks.isAngryGolden) { MusicManaging.PlayLoopMidiMusic("musChaseN"); }
                else { MusicManaging.PlayLoopMidiMusic("musChaseGE"); }
            }
            else if (!BoolChecks.musicPlaying)
            {
                BoolChecks.musicPlaying = true;
                MusicManaging.PlayLoopMidiMusic("musChase");
            }

            audManager.PlaySingle(sndRedAngered);
            StopIdleAnimation();
            spriteRenderer[0].sprite = sprRedSpecial;

            if (hopCoroutine != null) { StopCoroutine(hopCoroutine); hopCoroutine = null; }
            StartCoroutine(AngerDelay(3f));
        }

        private void AngeredBaldi()
        {
            if (isAngry || isProcessingTrigger) return;
            ResetHop();

            audManager.PlaySingle(sndRedAngered);
            StopIdleAnimation();
            spriteRenderer[0].sprite = sprRedSpecial;

            if (hopCoroutine != null) { StopCoroutine(hopCoroutine); hopCoroutine = null; }
            StartCoroutine(AngerDelayBaldi(3f));
        }

        private IEnumerator AngerDelay(float aFloat)
        {
            isProcessingTrigger = true;
            hasCalled = false;
            StayStill(true);
            aDelay = aFloat;
            while (aDelay > 0f)
            {
                aDelay -= ec.NpcTimeScale * Time.deltaTime;
                yield return null;
            }
            StayStill(false);
            isAngry = true;
            isProcessingTrigger = false;

            cSpeed = aSpeed;

            behaviorStateMachine.ChangeState(new RedBird_Chase(this, angryTargetPlayer));
            yield break;
        }

        private IEnumerator AngerDelayBaldi(float aFloat)
        {
            isProcessingTrigger = true;
            hasCalled = false;
            StayStill(true);
            aDelay = aFloat;
            while (aDelay > 0f)
            {
                aDelay -= ec.NpcTimeScale * Time.deltaTime;
                yield return null;
            }
            StayStill(false);
            isAngry = true;
            isProcessingTrigger = false;

            cSpeed = aSpeed;

            behaviorStateMachine.ChangeState(new RedBird_ChaseBaldi(this, angryTargetBaldi));
            yield break;
        }

        private bool CheckAndConsumeItem(PlayerManager pm, global::Items itemType)
        {
            if (pm == null || pm.itm == null) return false;
            for (int i = 0; i < pm.itm.maxItem; i++)
            {
                ItemObject currentItem = pm.itm.items[i];
                if (currentItem != null && currentItem.itemType == itemType)
                {
                    pm.itm.RemoveItem(i);
                    return true;
                }
            }
            return false;
        }

        private IEnumerator Caught(float aFloat, PlayerManager pm)
        {
            isProcessingTrigger = true;
            isAngry = false;
            angryTargetPlayer = null;

            if (PlayerHasGoldenEgg(pm)) { TakeGoldenEgg(pm); }

            BoolChecks.isMusic = Mathf.Max(0, BoolChecks.isMusic - 1);
            if (BoolChecks.isMusic <= 0)
            {
                BoolChecks.isMusic = 0;
                BoolChecks.musicPlaying = false;
                MusicManaging.StopMidiMusic();
            }

            float min, max;
            int minHP, maxHP;

            cSpeed = nSpeed;

            if (!BoolChecks.isNerfed)
            {
                min = 25f;
                max = 32f;
                minHP = 28;
                maxHP = 50;
            }
            else
            {
                min = 10f;
                max = 22f;
                minHP = 14;
                maxHP = 30;
            }
            float rFloat = UnityEngine.Random.Range(min, max);
            int hpInt = UnityEngine.Random.Range(minHP, maxHP);

            global::Items superSeedsType = EnumExtensions.GetFromExtendedName<global::Items>("SuperSeeds");
            bool hadItem;
            if (!BoolChecks.isAngryGolden) { hadItem = CheckAndConsumeItem(pm, superSeedsType); }
            else { hadItem = false; }

            if (eCount <= eCountBuild)
            {
                if (hadItem) // && !BoolChecks.isAngryGolden
                {
                    isEating = true;
                    StayStill(true);
                    yield return StartCoroutine(GetEating(3f, 12f, 0.5f));
                }
                else if (!BoolChecks.playerIsSquashed)
                {
                    StayStill(true);

                    ec.MakeNoise(transform.position, 85);

                    if (IsHPSystemLoaded()) { TryApplyHPDamage(hpInt, sndRedDamage); }

                    audManager.PlaySingle(sndRedExhausted);

                    StartCoroutine(PlayerStruck(rFloat, pm));

                    eCount++;
                    audManager.PlaySingle(sndRedCaught);

                    aDelay = aFloat;
                    while (aDelay > 0f)
                    {
                        aDelay -= ec.NpcTimeScale * Time.deltaTime;
                        yield return null;
                    }

                    StayStill(false);

                    spriteRenderer[0].sprite = sprRedIdle;
                    StartIdleAnimation();

                    behaviorStateMachine.ChangeState(new RedBird_Wander(this));
                }
                else
                {
                    PlayRandomSound(sndRedStump);

                    spriteRenderer[0].sprite = sprRedIdle;
                    StartIdleAnimation();

                    behaviorStateMachine.ChangeState(new RedBird_Wander(this));
                }
            }
            else
            {
                behaviorStateMachine.ChangeState(new RedBird_Wander(this));
                StayStill(true);
                PlayRandomSound(sndRedDeath);
                spriteRenderer[0].sprite = sprRedCollision;
                isWeak = true;
            }

            if (BoolChecks.isAngryGolden)
            {
                if (ec == null || ec.Npcs == null) { isProcessingTrigger = false; yield break; }

                BoolChecks.isAngryGolden = false;

                foreach (NPC npc in ec.Npcs)
                {
                    if (npc is YellowBird yellow)
                    {
                        yellow.isGoldenAngry = false;
                        StartCoroutine(yellow.SetCaught(pm));
                    }
                    if (npc is BlackBird black)
                    {
                        black.isGoldenAngry = false;
                        StartCoroutine(black.SetCaught(pm));
                    }
                }

                if (chance > 80f)
                {
                    BoolChecks.isAngryGolden = false;
                    if (Singleton<CoreGameManager>.Instance != null)
                    {
                        Baldi baldiInstance = ec != null ? ec.GetBaldi() : UnityEngine.Object.FindObjectOfType<Baldi>();

                        if (baldiInstance != null) { Singleton<CoreGameManager>.Instance.EndGame(pm.transform, baldiInstance); }
                        isProcessingTrigger = false;
                        yield break;
                    }
                }
            }

            isProcessingTrigger = false;
            yield break;
        }

        public IEnumerator SetCaught(PlayerManager pm)
        {
            isAngry = false;
            isGoldenAngry = false;
            BoolChecks.isAngryGolden = false;
            angryTargetPlayer = null;
            aCount = 0;
            isProcessingTrigger = false;

            BoolChecks.isMusic = Mathf.Max(0, BoolChecks.isMusic - 1);
            if (BoolChecks.isMusic <= 0)
            {
                BoolChecks.isMusic = 0;
                BoolChecks.musicPlaying = false;
                MusicManaging.StopMidiMusic();
            }

            cSpeed = nSpeed;

            PlayRandomSound(sndRedStump);

            spriteRenderer[0].sprite = sprRedIdle;
            StartIdleAnimation();

            behaviorStateMachine.ChangeState(new RedBird_Wander(this));

            yield break;
        }

        private IEnumerator CaughtBaldi(float aFloat, Baldi baldi)
        {
            isProcessingTrigger = true;
            isAngry = false;
            angryTargetBaldi = null;

            cSpeed = nSpeed;

            StayStill(true);
            ec.MakeNoise(transform.position, 85);

            audManager.PlaySingle(sndRedExhausted);

            float min = 20f;
            float max = 28f;
            float rFloat = UnityEngine.Random.Range(min, max);
            StartCoroutine(BaldiStruck(rFloat, baldi));

            audManager.PlaySingle(sndRedCaught);

            aDelay = aFloat;
            while (aDelay > 0f)
            {
                aDelay -= ec.NpcTimeScale * Time.deltaTime;
                yield return null;
            }

            StayStill(false);
            spriteRenderer[0].sprite = sprRedIdle;
            StartIdleAnimation();

            behaviorStateMachine.ChangeState(new RedBird_Wander(this));
            isProcessingTrigger = false;
            yield break;
        }

        private IEnumerator GetEating(float sTime, float eTime, float eDelay)
        {
            ItemObject zestyBar = ItemMetaStorage.Instance.FindByEnum(global::Items.ZestyBar)?.value;
            SoundObject eatSound = null;

            if (zestyBar != null && zestyBar.item != null)
            {
                var soundField = zestyBar.item.GetType().GetFields(
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance
                ).FirstOrDefault(f => f.FieldType == typeof(SoundObject));

                if (soundField != null) { eatSound = soundField.GetValue(zestyBar.item) as SoundObject; }
            }

            spriteRenderer[0].sprite = sprRedEat1;
            audManager.PlaySingle(sndRedHappy);
            yield return new WaitForSeconds(sTime);
            while (eTime > 0f)
            {
                audManager.PlaySingle(eatSound);

                spriteRenderer[0].sprite = sprRedEat2;
                yield return new WaitForSeconds(eDelay);
                eTime -= ec.NpcTimeScale * eDelay;

                if (eTime <= 0f) break;

                spriteRenderer[0].sprite = sprRedEat1;
                yield return new WaitForSeconds(eDelay);
                eTime -= ec.NpcTimeScale * eDelay;
            }
            isEating = false;
            StayStill(false);
            spriteRenderer[0].sprite = sprRedIdle;
            StartIdleAnimation();

            behaviorStateMachine.ChangeState(new RedBird_Wander(this));
            isProcessingTrigger = false;
            yield break;
        }

        private IEnumerator PlayerStruck(float fFloat, PlayerManager pm)
        {
            if (pm == null) yield break;
            pm.Am.moveMods.Add(freezeMod);

            BoolChecks.playerIsSquashed = true;
            float duration = fFloat;
            HudGauge gauge = null;
            var hud = Singleton<CoreGameManager>.Instance.GetHud(0);
            if (hud != null)
            {
                var gaugeManager = hud.GetComponentInChildren<HudGaugeManager>();
                if (gaugeManager != null) gauge = gaugeManager.ActivateNewGauge(sprGauge, duration);
            }
            fDelay = fFloat;
            while (fDelay > 0f)
            {
                fDelay -= ec.NpcTimeScale * Time.deltaTime;
                if (gauge != null) gauge.SetValue(duration, fDelay);
                yield return null;
            }
            pm.Am.moveMods.Remove(freezeMod);
            BoolChecks.playerIsSquashed = false;
            if (gauge != null) gauge.Deactivate();
            yield break;
        }

        private IEnumerator BaldiStruck(float fFloat, Baldi baldi)
        {
            if (baldi == null) yield break;

            baldi.Navigator.Am.moveMods.Add(freezeMod);

            fDelay = fFloat;
            while (fDelay > 0f)
            {
                fDelay -= ec.NpcTimeScale * Time.deltaTime;
                yield return null;
            }

            baldi.Navigator.Am.moveMods.Remove(freezeMod);
            yield break;
        }

        private IEnumerator SpriteYell(float dFloat)
        {
            isProcessingTrigger = true;
            StayStill(true);

            StopIdleAnimation();
            spriteRenderer[0].sprite = sprRedYell;
            sDelay = dFloat;
            while (sDelay > 0f)
            {
                sDelay -= ec.NpcTimeScale * Time.deltaTime;
                yield return null;
            }
            StayStill(false);
            spriteRenderer[0].sprite = sprRedIdle;

            StartIdleAnimation();
            isProcessingTrigger = false;
            yield break;
        }

        private IEnumerator SpriteAnger(float dFloat)
        {
            isProcessingTrigger = true;
            StayStill(true);
            StopIdleAnimation();
            spriteRenderer[0].sprite = sprRedSpecial;
            sDelay = dFloat;
            while (sDelay > 0f)
            {
                sDelay -= ec.NpcTimeScale * Time.deltaTime;
                yield return null;
            }
            StayStill(false);
            spriteRenderer[0].sprite = sprRedIdle;

            StartIdleAnimation();
            isProcessingTrigger = false;
            yield break;
        }

        private IEnumerator CallCooldown(float sFloat)
        {
            hasCalled = true;
            cDelay = sFloat;
            while (cDelay > 0f)
            {
                cDelay -= ec.NpcTimeScale * Time.deltaTime;
                yield return null;
            }
            hasCalled = false;
            yield break;
        }

        public void StayStill(bool stop)
        {
            navigator.maxSpeed = stop ? 0f : cSpeed;
            navigator.SetSpeed(stop ? 0f : cSpeed);
        }

        public void BirdCallFar()
        {
            PlayRandomSoundFar(sndRedIdle);
            ec.MakeNoise(transform.position, 85);
            AlertOtherAngryBirds(transform.position);
        }

        public void BirdCallFarBaldi()
        {
            PlayRandomSoundFar(sndRedIdle);
            //AlertOtherAngryBirds(transform.position);
        }

        public void AlertOtherAngryBirds(Vector3 callPosition)
        {
            if (ec == null || ec.Npcs == null) return;

            foreach (NPC npc in ec.Npcs)
            {
                if (npc == null || npc == this) continue;

                if (npc is YellowBird yellow) { if (yellow.isAngry) { yellow.RespondToCall(callPosition, angryTargetPlayer); } }
                if (npc is BlackBird black) { if (black.isAngry) { black.RespondToCall(callPosition, angryTargetPlayer); } }
            }
        }

        public void RespondToCall(Vector3 callPosition, PlayerManager targetPlayer)
        {
            if (behaviorStateMachine == null) return;

            if (!isAngry) { angryTargetPlayer = targetPlayer; }

            if (behaviorStateMachine.currentState is NpcState currentState) { currentState.ChangeNavigationState(new NavigationState_TargetPosition(this, 63, callPosition)); }
        }

        public void TakeGoldenEgg(PlayerManager player)
        {
            if (player == null || player.itm == null) return;

            global::Items goldenEggType = EnumExtensions.GetFromExtendedName<global::Items>("GoldenEgg");
            for (int i = 0; i < player.itm.items.Length; i++)
            {
                if (player.itm.items[i] != null && player.itm.items[i].itemType == goldenEggType)
                {
                    player.itm.RemoveItem(i);
                    break;
                }
            }
        }

        private static bool IsHPSystemLoaded() { return BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("char.hpsystem"); }

        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private void TryApplyHPDamage(int hpInt, SoundObject damageSound) { HPSystemPlugin.Instance.DamagePlayer(hpInt, null, damageSound); }
    }

    internal class RedBird_Wander : RedBird_StateBase
    {
        public RedBird_Wander(RedBird birdModMan) : base(birdModMan) { }

        public override void Enter()
        {
            base.Enter();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (redBird.PlayerHasGoldenEgg(player)) { redBird.TriggerGoldenEggAnger(player); }
        }

        public override void PlayerSighted(PlayerManager player)
        {
            base.PlayerSighted(player);
            if (redBird.PlayerHasGoldenEgg(player)) { redBird.TriggerGoldenEggAnger(player); }
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
        }
    }

    internal class RedBird_Chase : RedBird_StateBase
    {
        readonly PlayerManager pm;
        private bool hasCalledOnSight = false;

        public RedBird_Chase(RedBird birdModMan, PlayerManager targetPlayer) : base(birdModMan) { pm = targetPlayer; }

        public override void Enter()
        {
            base.Enter();
            hasCalledOnSight = false;
            if (pm != null) { ChangeNavigationState(new NavigationState_TargetPlayer(npc, 63, pm.transform.position)); }
        }

        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            if (!hasCalledOnSight)
            {
                redBird.BirdCallFar();
                hasCalledOnSight = true;
            }

            if (!(currentNavigationState is NavigationState_TargetPlayer)) { ChangeNavigationState(new NavigationState_TargetPlayer(npc, 63, player.transform.position)); }

            currentNavigationState.UpdatePosition(player.transform.position);
        }

        public override void PlayerSighted(PlayerManager player)
        {
            base.PlayerSighted(player);
            if (!(currentNavigationState is NavigationState_TargetPlayer))
            {
                ChangeNavigationState(new NavigationState_TargetPlayer(npc, 63, player.transform.position));
            }
            currentNavigationState.UpdatePosition(player.transform.position);
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (redBird.isAngry && pm != null)
            {
                ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
                hasCalledOnSight = false;
            }
            else
            {
                ChangeNavigationState(new NavigationState_WanderRandom(npc, 0));
                hasCalledOnSight = false;
            }
        }
    }

    internal class RedBird_ChaseBaldi : RedBird_StateBase
    {
        readonly Baldi baldi;
        private bool hasCalledOnSight = false;

        public RedBird_ChaseBaldi(RedBird birdModMan, Baldi targetBaldi) : base(birdModMan) { baldi = targetBaldi; }

        public override void Enter()
        {
            base.Enter();
            hasCalledOnSight = false;
            if (baldi != null) { ChangeNavigationState(new NavigationState_TargetPosition(npc, 63, baldi.transform.position)); }
        }

        public override void Update()
        {
            base.Update();
            if (baldi != null && currentNavigationState is NavigationState_TargetPosition)
            {
                if (!hasCalledOnSight)
                {
                    redBird.BirdCallFarBaldi();
                    hasCalledOnSight = true;
                }
                currentNavigationState.UpdatePosition(baldi.transform.position);
            }
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            if (baldi != null) { ChangeNavigationState(new NavigationState_TargetPosition(npc, 63, baldi.transform.position)); }
            else { npc.behaviorStateMachine.ChangeState(new RedBird_Wander(redBird)); }
        }
    }

    internal class RedBird_StateBase : NpcState
    {
        protected RedBird redBird;
        public RedBird_StateBase(RedBird birdModMan) : base(birdModMan) { redBird = birdModMan; }
    }
}