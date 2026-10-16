using bbplus.angrybirdspack.Birds;
using bbplus.angrybirdspack.Items;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using CustomMainMenusAPI;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using MTM101BaldAPI.SaveSystem;
using PlusStudioLevelFormat;
using PlusStudioLevelLoader;
using PixelInternalAPI;
using PixelInternalAPI.Classes;
using PixelInternalAPI.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace bbplus.angrybirdspack
{
    [BepInPlugin("angrygabry06.bbplus.abpack", "Angry Birds Pack", "0.1.3.0")]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pixelguy.pixelmodding.baldiplus.pixelinternalapi", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudio", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("mtm101.rulerp.baldiplus.levelstudioloader", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pixelguy.pixelmodding.baldiplus.custommainmenusapi", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("char.hpsystem", BepInDependency.DependencyFlags.SoftDependency)]

    public class ABPackPlugin : BaseUnityPlugin
    {
        public static ABPackPlugin Instance;

        public static AssetManager birdModMan = new AssetManager();
        public static string sImgDir_birds = "images/birds", sAudDir_sfx_birds = "audio/sfx/birds", sImgDir = "images";
        public static string sImgDir_Stickers = "images/stickers", sImgDir_Items = "images/items", sImgDir_Posters = "images/posters";
        public static string sImgDir_Particles = "images/particles", sImgDir_Objects = "images/objects";
        public static string sImgDir_Editor = "images/editor";
        //public static string sImgDir_Rooms = "images/rooms";
        public static string sAudDir_mfx = "audio/mfx", sAudDir_items = "audio/sfx/items", sAudDir_objects = "audio/sfx/objects";
        public static string musChase, musChaseFile = "mus_bird_chase";
        public static string musChaseN, musChaseNFile = "mus_bird_chase_nightmare";
        public static string musChaseGE, musChaseGEFile = "mus_test_goldenegg";

        internal static LayerMask onlyNpcPlayerLayers = LayerMask.GetMask("NPCs", "Player", LayerMask.LayerToName(LayerStorage.standardEntities)), onlyNpcLayers = LayerMask.GetMask("NPCs", LayerMask.LayerToName(LayerStorage.standardEntities));

        public ConfigEntry<bool> customPostersGen;

        [Header("Explosion Settings")]
        [Tooltip("Drag all explosion frame sprites here in sequential order.")]
        public List<Sprite> explosionSprites = new List<Sprite>();

        public static OBJ_Red redObjPrefab;

        public static RoomCategory birdRoomCat = EnumExtensions.ExtendEnum<RoomCategory>("BirdRoom");

        public static List<WeightedRoomAsset> potentialBirdRooms = new List<WeightedRoomAsset>();
        public static RoomAsset customBirdRoomAsset;

        public void Awake()
        {
            Instance = this;
            Harmony harmony = new Harmony("angrygabry06.bbplus.abpack");
            harmony.PatchAllConditionals();
            //harmony.PatchAll();
            customPostersGen = Config.Bind("Level Generation", "CustomPostersGen", false, "If set true would allow the mod to generate it's custom posters, note that this may cause some occasional level gen crashes.");
            SignAsset();
            LoadingEvents.RegisterOnAssetsLoaded(Info, RegisterAssets(), LoadingEventOrder.Pre);
            GeneratorManagement.Register(this, GenerationModType.Addend, AddObjects);
        }

        private void SignAsset()
        {
            MTM101BaldAPI.AssetTools.AssetLoader.LocalizationFromMod(this);

            //Bird Assets
            // Bird - Red
            birdModMan.Add<Texture2D>("Red Bird Spritesheet", AssetLoader.TextureFromMod(this, sImgDir_birds, "spr_bird_redSheet.png"));
            Texture2D texRedSheet = birdModMan.Get<Texture2D>("Red Bird Spritesheet");
            birdModMan.Add<Texture2D>("Red Bird Eating Spritesheet", AssetLoader.TextureFromMod(this, sImgDir_birds, "spr_bird_redEatingSheet.png"));
            Texture2D texRedEatSheet = birdModMan.Get<Texture2D>("Red Bird Eating Spritesheet");

            Vector2 redPivot = new Vector2(0.5f, 0.65f);
            float redPPU = 56f;

            Rect rectRedIdle = new Rect(0, 256, 256, 256);
            Rect rectRedBlink = new Rect(256, 256, 256, 256);
            Rect rectRedCollision = new Rect(512, 256, 256, 256);
            Rect rectRedSpecial = new Rect(0, 0, 256, 256);
            Rect rectRedYell = new Rect(256, 0, 256, 256);

            Rect rectRedEat1 = new Rect(0, 0, 256, 256);
            Rect rectRedEat2 = new Rect(256, 0, 256, 256);

            if (texRedSheet != null)
            {
                birdModMan.Add<Sprite>("Red Bird Sprite", Sprite.Create(texRedSheet, rectRedIdle, redPivot, redPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Red Bird Blink Sprite", Sprite.Create(texRedSheet, rectRedBlink, redPivot, redPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Red Bird Yell Sprite", Sprite.Create(texRedSheet, rectRedYell, redPivot, redPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Red Bird Special Sprite", Sprite.Create(texRedSheet, rectRedSpecial, redPivot, redPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Red Bird Collision Sprite", Sprite.Create(texRedSheet, rectRedCollision, redPivot, redPPU, 1, SpriteMeshType.FullRect));
            }

            if (texRedEatSheet != null)
            {
                birdModMan.Add<Sprite>("Red Bird Eating Sprite 1", Sprite.Create(texRedEatSheet, rectRedEat1, redPivot, redPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Red Bird Eating Sprite 2", Sprite.Create(texRedEatSheet, rectRedEat2, redPivot, redPPU, 1, SpriteMeshType.FullRect));
            }

            birdModMan.Add<Texture2D>("Red Bird Poster", AssetLoader.TextureFromMod(this, sImgDir_birds, "tex_pst_pri_red.png"));

            birdModMan.Add<SoundObject>("Red Bird Call Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_special_0.wav"), "Vfx_BRD_RED_Call1", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Call Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_special_1.wav"), "Vfx_BRD_RED_Call2", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Call Sound 3", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_special_2.wav"), "Vfx_BRD_RED_Call3", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Angry Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_angry.wav"), "Vfx_BRD_RED_AngryGrunt", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Angered Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_angered.wav"), "Vfx_BRD_RED_Angered", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Caught Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_caught.wav"), "Vfx_BRD_RED_Mumbles", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Death Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_death_0.wav"), "Vfx_BRD_RED_Exhausted1", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Death Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_death_1.wav"), "Vfx_BRD_RED_Exhausted2", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Exhausted Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_exhausted.wav"), "", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Stumped Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_stump_0.wav"), "Vfx_BRD_RED_Stumped1", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Stumped Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_stump_1.wav"), "Vfx_BRD_RED_Stumped2", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Stumped Sound 3", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_stump_2.wav"), "Vfx_BRD_RED_Stumped3", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Happy Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_happy.wav"), "Vfx_BRD_RED_Happy", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Bird Damage Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_red_damage.wav"), "Vfx_BRD_RED_Damage", SoundType.Effect, birdRedColor));

            // Bird - Chuck
            birdModMan.Add<Texture2D>("Yellow Bird Spritesheet", AssetLoader.TextureFromMod(this, sImgDir_birds, "spr_bird_chuckSheet.png"));
            Texture2D texYellowSheet = birdModMan.Get<Texture2D>("Yellow Bird Spritesheet");
            birdModMan.Add<Texture2D>("Yellow Bird Eating Spritesheet", AssetLoader.TextureFromMod(this, sImgDir_birds, "spr_bird_chuckEatingSheet.png"));
            Texture2D texYellowEatSheet = birdModMan.Get<Texture2D>("Yellow Bird Eating Spritesheet");

            Vector2 yellowPivot = new Vector2(0.5f, 0.65f);
            float yellowPPU = 50f;

            Rect rectChkIdle = new Rect(0, 268, 360, 268);
            Rect rectChkBlink = new Rect(360, 268, 360, 268);
            Rect rectChkCollision = new Rect(720, 268, 360, 268);
            Rect rectChkSpecial = new Rect(0, 0, 360, 268);
            Rect rectChkYell = new Rect(360, 0, 360, 268);

            Rect rectChkEat1 = new Rect(0, 0, 360, 268);
            Rect rectChkEat2 = new Rect(360, 0, 360, 268);

            if (texYellowSheet != null)
            {
                birdModMan.Add<Sprite>("Yellow Bird Sprite", Sprite.Create(texYellowSheet, rectChkIdle, yellowPivot, yellowPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Yellow Bird Blink Sprite", Sprite.Create(texYellowSheet, rectChkBlink, yellowPivot, yellowPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Yellow Bird Yell Sprite", Sprite.Create(texYellowSheet, rectChkYell, yellowPivot, yellowPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Yellow Bird Special Sprite", Sprite.Create(texYellowSheet, rectChkSpecial, yellowPivot, yellowPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Yellow Bird Collision Sprite", Sprite.Create(texYellowSheet, rectChkCollision, yellowPivot, yellowPPU, 1, SpriteMeshType.FullRect));
            }

            if (texYellowEatSheet != null)
            {
                birdModMan.Add<Sprite>("Yellow Bird Eating Sprite 1", Sprite.Create(texYellowEatSheet, rectChkEat1, yellowPivot, yellowPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Yellow Bird Eating Sprite 2", Sprite.Create(texYellowEatSheet, rectChkEat2, yellowPivot, yellowPPU, 1, SpriteMeshType.FullRect));
            }

            birdModMan.Add<Texture2D>("Yellow Bird Poster", AssetLoader.TextureFromMod(this, sImgDir_birds, "tex_pst_pri_chuck.png"));

            birdModMan.Add<SoundObject>("Yellow Bird Call Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_special_0.wav"), "Vfx_BRD_CHK_Call1", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Call Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_special_1.wav"), "Vfx_BRD_CHK_Call2", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Call Sound 3", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_special_2.wav"), "Vfx_BRD_CHK_Call3", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Angry Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_laughs.wav"), "Vfx_BRD_CHK_Laughs", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Angered Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_angered.wav"), "Vfx_BRD_CHK_Angered", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Caught Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_caught.wav"), "Vfx_BRD_CHK_Smirks", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Death Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_death_0.wav"), "Vfx_BRD_CHK_Exhausted1", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Death Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_death_1.wav"), "Vfx_BRD_CHK_Exhausted2", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Exhausted Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_exhausted.wav"), "", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Stumped Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_stump_0.wav"), "Vfx_BRD_CHK_Stumped1", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Stumped Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_stump_1.wav"), "Vfx_BRD_CHK_Stumped2", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Stumped Sound 3", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_stump_2.wav"), "Vfx_BRD_CHK_Stumped3", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Happy Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_happy.wav"), "Vfx_BRD_CHK_Happy", SoundType.Effect, birdYellowColor));
            birdModMan.Add<SoundObject>("Yellow Bird Damage Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_chuck_damage.wav"), "Vfx_BRD_CHK_Damage", SoundType.Effect, birdYellowColor));

            // Bird - Bomb
            birdModMan.Add<Texture2D>("Black Bird Spritesheet", AssetLoader.TextureFromMod(this, sImgDir_birds, "spr_bird_bombSheet.png"));
            Texture2D texBlackSheet = birdModMan.Get<Texture2D>("Black Bird Spritesheet");
            birdModMan.Add<Texture2D>("Black Bird Eating Spritesheet", AssetLoader.TextureFromMod(this, sImgDir_birds, "spr_bird_bombEatingSheet.png"));
            Texture2D texBlackEatSheet = birdModMan.Get<Texture2D>("Black Bird Eating Spritesheet");

            Vector2 blackPivot = new Vector2(0.5f, 0.55f);
            float blackPPU = 40f;

            Rect rectBmbIdle = new Rect(0, 256, 256, 256);
            Rect rectBmbBlink = new Rect(256, 256, 256, 256);
            Rect rectBmbCollision = new Rect(512, 256, 256, 256);
            Rect rectBmbSpecial = new Rect(768, 256, 256, 256);
            Rect rectBmbYell = new Rect(0, 0, 256, 256);
            Rect rectBmbDefuse1 = new Rect(256, 0, 256, 256);
            Rect rectBmbDefuse2 = new Rect(512, 0, 256, 256);
            Rect rectBmbDefuse3 = new Rect(768, 0, 256, 256);

            Rect rectBmbEat1 = new Rect(0, 0, 256, 256);
            Rect rectBmbEat2 = new Rect(256, 0, 256, 256);

            if (texBlackSheet != null)
            {
                birdModMan.Add<Sprite>("Black Bird Sprite", Sprite.Create(texBlackSheet, rectBmbIdle, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Black Bird Blink Sprite", Sprite.Create(texBlackSheet, rectBmbBlink, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Black Bird Yell Sprite", Sprite.Create(texBlackSheet, rectBmbYell, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Black Bird Special Sprite", Sprite.Create(texBlackSheet, rectBmbSpecial, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Black Bird Collision Sprite", Sprite.Create(texBlackSheet, rectBmbCollision, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Black Bird Defuse Sprite 1", Sprite.Create(texBlackSheet, rectBmbDefuse1, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Black Bird Defuse Sprite 2", Sprite.Create(texBlackSheet, rectBmbDefuse2, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Black Bird Defuse Sprite 3", Sprite.Create(texBlackSheet, rectBmbDefuse3, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
            }

            if (texBlackEatSheet != null)
            {
                birdModMan.Add<Sprite>("Black Bird Eating Sprite 1", Sprite.Create(texBlackEatSheet, rectBmbEat1, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
                birdModMan.Add<Sprite>("Black Bird Eating Sprite 2", Sprite.Create(texBlackEatSheet, rectBmbEat2, blackPivot, blackPPU, 1, SpriteMeshType.FullRect));
            }

            birdModMan.Add<Texture2D>("Black Bird Poster", AssetLoader.TextureFromMod(this, sImgDir_birds, "tex_pst_pri_bomb.png"));

            birdModMan.Add<SoundObject>("Black Bird Call Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_special_0.wav"), "Vfx_BRD_BMB_Call1", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Call Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_special_1.wav"), "Vfx_BRD_BMB_Call2", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Call Sound 3", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_special_2.wav"), "Vfx_BRD_BMB_Call3", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Disgrunt Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_disgrunt.wav"), "Vfx_BRD_BMB_Disgrunt", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Angered Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_angered.wav"), "Vfx_BRD_BMB_Angered", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Caught Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_caught.wav"), "Vfx_BRD_BMB_Humbles", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Death Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_death_0.wav"), "Vfx_BRD_BMB_Exhausted1", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Death Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_death_1.wav"), "Vfx_BRD_BMB_Exhausted2", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Exhausted Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_exhausted.wav"), "Sfx_BRD_BMB_Explosion", SoundType.Effect, Color.white));
            birdModMan.Add<SoundObject>("Black Bird Stumped Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_stump_0.wav"), "Vfx_BRD_BMB_Stumped1", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Stumped Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_stump_1.wav"), "Vfx_BRD_BMB_Stumped2", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Stumped Sound 3", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_stump_2.wav"), "Vfx_BRD_BMB_Stumped3", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Defusing Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_defusing.wav"), "Vfx_BRD_BMB_Defusing", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Defuse Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_defuse.wav"), "", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Happy Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_happy.wav"), "Vfx_BRD_BMB_Happy", SoundType.Effect, birdBlackColor));
            birdModMan.Add<SoundObject>("Black Bird Damage Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_sfx_birds, "snd_bird_bomb_damage.wav"), "Vfx_BRD_BMB_Damage", SoundType.Effect, birdBlackColor));

            // Particles
            birdModMan.Add<Texture2D>("Particle Explosion Spritesheet", AssetLoader.TextureFromMod(this, sImgDir_Particles, "spr_pfx_explosion_sheet.png"));
            Texture2D texPfxExplosion = birdModMan.Get<Texture2D>("Particle Explosion Spritesheet");

            Vector2 pfxExplosionPivot = new Vector2(0.5f, 0.5f);
            float pfxExplosionPPU = 24f;

            if (texPfxExplosion != null)
            {
                Rect rectPfxExplosion1 = new Rect(0, 256, 256, 256);
                Rect rectPfxExplosion2 = new Rect(256, 256, 256, 256);
                Rect rectPfxExplosion3 = new Rect(512, 256, 256, 256);
                Rect rectPfxExplosion4 = new Rect(768, 256, 256, 256);
                Rect rectPfxExplosion5 = new Rect(0, 0, 256, 256);
                Rect rectPfxExplosion6 = new Rect(256, 0, 256, 256);

                Sprite particleExplosion1 = Sprite.Create(texPfxExplosion, rectPfxExplosion1, pfxExplosionPivot, pfxExplosionPPU, 1, SpriteMeshType.FullRect);
                Sprite particleExplosion2 = Sprite.Create(texPfxExplosion, rectPfxExplosion2, pfxExplosionPivot, pfxExplosionPPU, 1, SpriteMeshType.FullRect);
                Sprite particleExplosion3 = Sprite.Create(texPfxExplosion, rectPfxExplosion3, pfxExplosionPivot, pfxExplosionPPU, 1, SpriteMeshType.FullRect);
                Sprite particleExplosion4 = Sprite.Create(texPfxExplosion, rectPfxExplosion4, pfxExplosionPivot, pfxExplosionPPU, 1, SpriteMeshType.FullRect);
                Sprite particleExplosion5 = Sprite.Create(texPfxExplosion, rectPfxExplosion5, pfxExplosionPivot, pfxExplosionPPU, 1, SpriteMeshType.FullRect);
                Sprite particleExplosion6 = Sprite.Create(texPfxExplosion, rectPfxExplosion6, pfxExplosionPivot, pfxExplosionPPU, 1, SpriteMeshType.FullRect);

                explosionSprites.AddRange(new Sprite[] { particleExplosion1, particleExplosion2, particleExplosion3, particleExplosion4, particleExplosion5, particleExplosion6 });
            }

            // Objects
            birdModMan.Add<Texture2D>("Red Bird Object", AssetLoader.TextureFromMod(this, sImgDir_Objects, "spr_obj_bird_red.png"));
            Texture2D texRedObject = birdModMan.Get<Texture2D>("Red Bird Object");
            if (texRedObject != null)
            {
                Sprite spriteRedObject = Sprite.Create(texRedObject, new Rect(0, 0, texRedObject.width, texRedObject.height), new Vector2(0.5f, 0.5f), 56f, 1, SpriteMeshType.FullRect);
                birdModMan.Add<Sprite>("Red Bird Object Sprite", spriteRedObject);
            }

            birdModMan.Add<Texture2D>("Yellow Bird Object", AssetLoader.TextureFromMod(this, sImgDir_Objects, "spr_obj_bird_chuck.png"));
            Texture2D texChuckObject = birdModMan.Get<Texture2D>("Yellow Bird Object");
            if (texChuckObject != null)
            {
                Sprite spriteChuckObject = Sprite.Create(texChuckObject, new Rect(0, 0, texChuckObject.width, texChuckObject.height), new Vector2(0.5f, 0.75f), 50f, 1, SpriteMeshType.FullRect);
                birdModMan.Add<Sprite>("Yellow Bird Object Sprite", spriteChuckObject);
            }

            birdModMan.Add<Texture2D>("Black Bird Object", AssetLoader.TextureFromMod(this, sImgDir_Objects, "spr_obj_bird_bomb.png"));
            Texture2D texBombObject = birdModMan.Get<Texture2D>("Black Bird Object");
            if (texBombObject != null)
            {
                Sprite spriteBombObject = Sprite.Create(texBombObject, new Rect(0, 0, texBombObject.width, texBombObject.height), new Vector2(0.5f, 0.65f), 40f, 1, SpriteMeshType.FullRect);
                birdModMan.Add<Sprite>("Black Bird Object Sprite", spriteBombObject);
            }

            birdModMan.Add<SoundObject>("Slingshot Select Sound", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_items, "snd_slingshot_select.wav"), "Sfx_Slingshot_Select", SoundType.Effect, Color.white));
            birdModMan.Add<SoundObject>("Slingshot Use Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_items, "snd_slingshot_use_0.wav"), "Sfx_Slingshot_Used", SoundType.Effect, Color.white));
            birdModMan.Add<SoundObject>("Slingshot Use Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_items, "snd_slingshot_use_1.wav"), "Sfx_Slingshot_Used", SoundType.Effect, Color.white));
            birdModMan.Add<SoundObject>("Slingshot Use Sound 3", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_items, "snd_slingshot_use_2.wav"), "Sfx_Slingshot_Used", SoundType.Effect, Color.white));
            birdModMan.Add<SoundObject>("Red Launch Sound 1", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_objects, "snd_bird_red_launch_0.wav"), "Sfx_BRD_RED_Flying", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Launch Sound 2", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_objects, "snd_bird_red_launch_1.wav"), "Sfx_BRD_RED_Flying", SoundType.Effect, birdRedColor));
            birdModMan.Add<SoundObject>("Red Launch Sound 3", ObjectCreators.CreateSoundObject(AssetLoader.AudioClipFromMod(this, sAudDir_objects, "snd_bird_red_launch_2.wav"), "Sfx_BRD_RED_Flying", SoundType.Effect, birdRedColor));

            GameObject redObjContainer = new GameObject("OBJ_Red_Prefab");
            DontDestroyOnLoad(redObjContainer);
            redObjContainer.SetActive(false);

            SpriteRenderer sr = redObjContainer.AddComponent<SpriteRenderer>();
            sr.sprite = birdModMan.Get<Sprite>("Red Bird Object Sprite");

            PropagatedAudioManager audioMgr = redObjContainer.AddComponent<PropagatedAudioManager>();

            OBJ_Red redScript = redObjContainer.AddComponent<OBJ_Red>();
            if (redScript.sndRedLaunch == null) redScript.sndRedLaunch = new List<SoundObject>();

            SoundObject launch1 = birdModMan.Get<SoundObject>("Red Launch Sound 1");
            SoundObject launch2 = birdModMan.Get<SoundObject>("Red Launch Sound 2");
            SoundObject launch3 = birdModMan.Get<SoundObject>("Red Launch Sound 3");
            if (launch1 != null) redScript.sndRedLaunch.Add(launch1);
            if (launch2 != null) redScript.sndRedLaunch.Add(launch2);
            if (launch3 != null) redScript.sndRedLaunch.Add(launch3);

            redScript.audioManager = audioMgr;
            redScript.baseSpeed = 35f;
            redScript.stunDuration = 19f;

            redObjPrefab = redScript;

            // Items
            birdModMan.Add<Sprite>("SuperSeedsSmall", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_superSeeds_small.png"), 25f));
            birdModMan.Add<Sprite>("SuperSeedsBig", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_superSeeds_large.png"), 50f));
            birdModMan.Add<Sprite>("SlingshotRedSmall", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_slingshotRed_small.png"), 25f));
            birdModMan.Add<Sprite>("SlingshotRedBig", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_slingshotRed_large.png"), 50f));
            birdModMan.Add<Sprite>("SlingshotChuckSmall", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_slingshotChuck_small.png"), 25f));
            birdModMan.Add<Sprite>("SlingshotChuckBig", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_slingshotChuck_large.png"), 50f));
            birdModMan.Add<Sprite>("SlingshotBombSmall", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_slingshotBomb_small.png"), 25f));
            birdModMan.Add<Sprite>("SlingshotBombBig", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_slingshotBomb_large.png"), 50f));
            birdModMan.Add<Sprite>("GoldenEggSmall", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_goldenEgg_small.png"), 25f));
            birdModMan.Add<Sprite>("GoldenEggBig", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Items, "spr_itm_goldenEgg_large.png"), 25f)); //50f

            // Gauges
            birdModMan.Add<Texture2D>("Gauge Icon 1", AssetLoader.TextureFromMod(this, sImgDir, "spr_gauge_birdStrucked.png"));
            Texture2D texGauge = birdModMan.Get<Texture2D>("Gauge Icon 1");
            if (texGauge != null) { birdModMan.Add<Sprite>("Gauge Bird Strucked", AssetLoader.SpriteFromTexture2D(texGauge, 50f)); }

            // Stickers
            birdModMan.Add<Sprite>("Sticker BirdsTrouble Sprite", AssetLoader.SpriteFromMod(this, new Vector2(0.5f, 0.5f), 1f, new string[] { sImgDir_Stickers, "spr_sticker_birdsTrouble.png" }));
            birdModMan.Add<Sprite>("Sticker NotSoAngry Sprite", AssetLoader.SpriteFromMod(this, new Vector2(0.5f, 0.5f), 1f, new string[] { sImgDir_Stickers, "spr_sticker_notSoAngry.png" }));
            birdModMan.Add<Sprite>("Sticker LessBirds Sprite", AssetLoader.SpriteFromMod(this, new Vector2(0.5f, 0.5f), 1f, new string[] { sImgDir_Stickers, "spr_sticker_lessBirds.png" }));

            // Rooms
            //birdModMan.Add<Texture2D>("BirdFloor", AssetLoader.TextureFromMod(this, sImgDir_Rooms, "tex_r_bird_floor.png"));
            //birdModMan.Add<Texture2D>("BirdWall", AssetLoader.TextureFromMod(this, sImgDir_Rooms, "tex_r_bird_wall.png"));
            //birdModMan.Add<Texture2D>("BirdCeiling", AssetLoader.TextureFromMod(this, sImgDir_Rooms, "tex_r_bird_ceiling.png"));

            // Music
            musChase = AssetLoader.MidiFromMod("musChase", this, sAudDir_mfx, "mus_bird_chase.mid");
            birdModMan.Add<string>("musChase", musChase);
            musChaseN = AssetLoader.MidiFromMod("musChaseN", this, sAudDir_mfx, "mus_bird_chase_nightmare.mid");
            birdModMan.Add<string>("musChaseN", musChaseN);
            musChaseGE = AssetLoader.MidiFromMod("musChaseGE", this, sAudDir_mfx, "mus_bird_chase_goldenegg.mid");
            birdModMan.Add<string>("musChaseGE", musChaseGE);

            // Menu
            if (Chainloader.PluginInfos.ContainsKey("pixelguy.pixelmodding.baldiplus.custommainmenusapi"))
            {
                birdModMan.Add<Texture2D>("mainMenu", AssetLoader.TextureFromMod(this, sImgDir, "spr_men_plusTitle.png"));
                Texture2D texMenu = birdModMan.Get<Texture2D>("mainMenu");
                Sprite menSprite = texMenu != null ? AssetLoader.SpriteFromTexture2D(texMenu, 1f) : null;

                string musMenu = AssetLoader.MidiFromMod("musMenu", this, sAudDir_mfx, "mus_abTitle.mid");

                MainMenuObject.CreateMenuObject("BBPlus AB Pack", menSprite, null, musMenu);
            }

            // Editor
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio"))
            {
                birdModMan.Add<Sprite>("Editor NPC Red", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Editor, "spr_npc_bird_red.png"), 1f));
                birdModMan.Add<Sprite>("Editor NPC Chuck", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Editor, "spr_npc_bird_chuck.png"), 1f));
                birdModMan.Add<Sprite>("Editor NPC Bomb", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Editor, "spr_npc_bird_bomb.png"), 1f));
                //birdModMan.Add<Sprite>("Editor OBJ Slingshot", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Editor, "spr_obj_slingshot.png"), 1f));
                //birdModMan.Add<Sprite>("Editor Room Bird", AssetLoader.SpriteFromTexture2D(AssetLoader.TextureFromMod(this, sImgDir_Editor, "spr_room_bird.png"), 1f));
            }
        }

        private IEnumerator RegisterAssets()
        {
            yield return 1;
            yield return "Preparing Birds...";

            // Objects
            //GameObject slingshotBase = GameObject.Instantiate<GameObject>(Resources.FindObjectsOfTypeAll<EnvironmentObject>().First(x => x.name == "Plant" && x.GetInstanceID() >= 0 && x.transform.parent == null).gameObject, MTM101BaldiDevAPI.prefabTransform);
            //slingshotBase.GetComponentInChildren<SpriteRenderer>().sprite = AssetLoader.SpriteFromMod(this, new Vector2(0.5f, 0f), 24f, new string[] { sImgDir_Objects, "spr_obj_slingshot.png" });
            //slingshotBase.name = "Slingshot";
            //birdModMan.Add<GameObject>("Slingshot", slingshotBase);

            // Rooms
            //StandardDoorMats birdDoorMats = ObjectCreators.CreateDoorDataObject("BirdDoor", AssetLoader.TextureFromMod(this, sImgDir_Rooms, "tex_r_bird_doorOpen.png"), AssetLoader.TextureFromMod(this, sImgDir_Rooms, "tex_r_bird_doorClosed.png"));
            //birdModMan.Add<StandardDoorMats>("BirdDoorMats", birdDoorMats);

            //LevelLoaderPlugin.Instance.roomSettings.Add("birdRoom", new RoomSettings(ABPackPlugin.birdRoomCat, RoomType.Room, new Color(210f / 255f, 0f, 44f / 255f), birdDoorMats, null));
            //LevelLoaderPlugin.Instance.basicObjects.Add("obj_slingshot", birdModMan.Get<GameObject>("Slingshot"));
            //LevelLoaderPlugin.Instance.roomTextureAliases.Add("BirdFloor", birdModMan.Get<Texture2D>("BirdFloor"));
            //LevelLoaderPlugin.Instance.roomTextureAliases.Add("BirdWall", birdModMan.Get<Texture2D>("BirdWall"));
            //LevelLoaderPlugin.Instance.roomTextureAliases.Add("BirdCeiling", birdModMan.Get<Texture2D>("BirdCeiling"));

            // NPCs
            RedBird redBird = new NPCBuilder<RedBird>(Info)
                .SetName("Red Bird")
                .SetEnum("red_Bird")
                .SetMinMaxAudioDistance(10, 250)
                .IgnorePlayerOnSpawn()
                .AddLooker()
                .AddSpawnableRoomCategories(new RoomCategory[] { RoomCategory.Class, RoomCategory.Hall, birdRoomCat })
                .SetPoster(birdModMan.Get<Texture2D>("Red Bird Poster"), "PST_PRI_Red1", "PST_PRI_Red2")
                .SetForcedSubtitleColor(birdRedColor)
                .Build();

            YellowBird yellowBird = new NPCBuilder<YellowBird>(Info)
                .SetName("Yellow Bird")
                .SetEnum("yellow_Bird")
                .SetMinMaxAudioDistance(10, 250)
                .IgnorePlayerOnSpawn()
                .AddLooker()
                .AddSpawnableRoomCategories(new RoomCategory[] { RoomCategory.Class, RoomCategory.Hall, birdRoomCat })
                .SetPoster(birdModMan.Get<Texture2D>("Yellow Bird Poster"), "PST_PRI_Chuck1", "PST_PRI_Chuck2")
                .SetForcedSubtitleColor(birdYellowColor)
                .Build();

            BlackBird blackBird = new NPCBuilder<BlackBird>(Info)
                .SetName("Black Bird")
                .SetEnum("black_Bird")
                .SetMinMaxAudioDistance(10, 250)
                .IgnorePlayerOnSpawn()
                .AddLooker()
                .AddSpawnableRoomCategories(new RoomCategory[] { RoomCategory.Class, RoomCategory.Hall, birdRoomCat })
                .SetPoster(birdModMan.Get<Texture2D>("Black Bird Poster"), "PST_PRI_Bomb1", "PST_PRI_Bomb2")
                .SetForcedSubtitleColor(birdBlackColor)
                .Build();

            yield return "Adding Assigned Bird Assets...";

            if (redBird.sndRedIdle == null) redBird.sndRedIdle = new List<SoundObject>();
            AddNonEmptySounds(redBird.sndRedIdle, "Red Bird Call Sound 1", "Red Bird Call Sound 2", "Red Bird Call Sound 3");

            redBird.sndRedAngry = birdModMan.Get<SoundObject>("Red Bird Angry Sound");
            redBird.sndRedAngered = birdModMan.Get<SoundObject>("Red Bird Angered Sound");
            redBird.sndRedCaught = birdModMan.Get<SoundObject>("Red Bird Caught Sound");

            if (redBird.sndRedDeath == null) redBird.sndRedDeath = new List<SoundObject>();
            AddNonEmptySounds(redBird.sndRedDeath, "Red Bird Death Sound 1", "Red Bird Death Sound 2");

            redBird.sndRedExhausted = birdModMan.Get<SoundObject>("Red Bird Exhausted Sound");

            if (redBird.sndRedStump == null) redBird.sndRedStump = new List<SoundObject>();
            AddNonEmptySounds(redBird.sndRedStump, "Red Bird Stumped Sound 1", "Red Bird Stumped Sound 2", "Red Bird Stumped Sound 3");

            redBird.sndRedHappy = birdModMan.Get<SoundObject>("Red Bird Happy Sound");
            redBird.sndRedDamage = birdModMan.Get<SoundObject>("Red Bird Damage Sound");
            redBird.sprRedIdle = birdModMan.Get<Sprite>("Red Bird Sprite");
            redBird.sprRedBlink = birdModMan.Get<Sprite>("Red Bird Blink Sprite");
            redBird.sprRedYell = birdModMan.Get<Sprite>("Red Bird Yell Sprite");
            redBird.sprRedSpecial = birdModMan.Get<Sprite>("Red Bird Special Sprite");
            redBird.sprRedCollision = birdModMan.Get<Sprite>("Red Bird Collision Sprite");
            redBird.sprRedEat1 = birdModMan.Get<Sprite>("Red Bird Eating Sprite 1");
            redBird.sprRedEat2 = birdModMan.Get<Sprite>("Red Bird Eating Sprite 2");
            redBird.sprGauge = birdModMan.Get<Sprite>("Gauge Bird Strucked");

            if (yellowBird.sndChuckIdle == null) yellowBird.sndChuckIdle = new List<SoundObject>();
            AddNonEmptySounds(yellowBird.sndChuckIdle, "Yellow Bird Call Sound 1", "Yellow Bird Call Sound 2", "Yellow Bird Call Sound 3");

            yellowBird.sndChuckAngry = birdModMan.Get<SoundObject>("Yellow Bird Angry Sound");
            yellowBird.sndChuckAngered = birdModMan.Get<SoundObject>("Yellow Bird Angered Sound");
            yellowBird.sndChuckCaught = birdModMan.Get<SoundObject>("Yellow Bird Caught Sound");

            if (yellowBird.sndChuckDeath == null) yellowBird.sndChuckDeath = new List<SoundObject>();
            AddNonEmptySounds(yellowBird.sndChuckDeath, "Yellow Bird Death Sound 1", "Yellow Bird Death Sound 2");

            yellowBird.sndChuckExhausted = birdModMan.Get<SoundObject>("Yellow Bird Exhausted Sound");

            if (yellowBird.sndChuckStump == null) yellowBird.sndChuckStump = new List<SoundObject>();
            AddNonEmptySounds(yellowBird.sndChuckStump, "Yellow Bird Stumped Sound 1", "Yellow Bird Stumped Sound 2", "Yellow Bird Stumped Sound 3");

            yellowBird.sndChuckHappy = birdModMan.Get<SoundObject>("Yellow Bird Happy Sound");
            yellowBird.sndChuckDamage = birdModMan.Get<SoundObject>("Yellow Bird Damage Sound");
            yellowBird.sprChuckIdle = birdModMan.Get<Sprite>("Yellow Bird Sprite");
            yellowBird.sprChuckBlink = birdModMan.Get<Sprite>("Yellow Bird Blink Sprite");
            yellowBird.sprChuckYell = birdModMan.Get<Sprite>("Yellow Bird Yell Sprite");
            yellowBird.sprChuckSpecial = birdModMan.Get<Sprite>("Yellow Bird Special Sprite");
            yellowBird.sprChuckCollision = birdModMan.Get<Sprite>("Yellow Bird Collision Sprite");
            yellowBird.sprChuckEat1 = birdModMan.Get<Sprite>("Yellow Bird Eating Sprite 1");
            yellowBird.sprChuckEat2 = birdModMan.Get<Sprite>("Yellow Bird Eating Sprite 2");
            yellowBird.sprGauge = birdModMan.Get<Sprite>("Gauge Bird Strucked");

            if (blackBird.sndBombIdle == null) blackBird.sndBombIdle = new List<SoundObject>();
            AddNonEmptySounds(blackBird.sndBombIdle, "Black Bird Call Sound 1", "Black Bird Call Sound 2", "Black Bird Call Sound 3");

            blackBird.sndBombAngry = birdModMan.Get<SoundObject>("Black Bird Disgrunt Sound");
            blackBird.sndBombAngered = birdModMan.Get<SoundObject>("Black Bird Angered Sound");
            blackBird.sndBombCaught = birdModMan.Get<SoundObject>("Black Bird Caught Sound");

            if (blackBird.sndBombDeath == null) blackBird.sndBombDeath = new List<SoundObject>();
            AddNonEmptySounds(blackBird.sndBombDeath, "Black Bird Death Sound 1", "Black Bird Death Sound 2");

            blackBird.sndBombExhausted = birdModMan.Get<SoundObject>("Black Bird Exhausted Sound");

            if (blackBird.sndBombStump == null) blackBird.sndBombStump = new List<SoundObject>();
            AddNonEmptySounds(blackBird.sndBombStump, "Black Bird Stumped Sound 1", "Black Bird Stumped Sound 2", "Black Bird Stumped Sound 3");

            blackBird.sndBombDefusing = birdModMan.Get<SoundObject>("Black Bird Defusing Sound");
            blackBird.sndBombDefuse = birdModMan.Get<SoundObject>("Black Bird Defuse Sound");
            blackBird.sndBombHappy = birdModMan.Get<SoundObject>("Black Bird Happy Sound");
            blackBird.sndBombDamage = birdModMan.Get<SoundObject>("Black Bird Damage Sound");
            blackBird.sprBombIdle = birdModMan.Get<Sprite>("Black Bird Sprite");
            blackBird.sprBombBlink = birdModMan.Get<Sprite>("Black Bird Blink Sprite");
            blackBird.sprBombYell = birdModMan.Get<Sprite>("Black Bird Yell Sprite");
            blackBird.sprBombSpecial = birdModMan.Get<Sprite>("Black Bird Special Sprite");
            blackBird.sprBombCollision = birdModMan.Get<Sprite>("Black Bird Collision Sprite");
            blackBird.sprBombDefuse1 = birdModMan.Get<Sprite>("Black Bird Defuse Sprite 1");
            blackBird.sprBombDefuse2 = birdModMan.Get<Sprite>("Black Bird Defuse Sprite 2");
            blackBird.sprBombDefuse3 = birdModMan.Get<Sprite>("Black Bird Defuse Sprite 3");
            blackBird.sprBombEat1 = birdModMan.Get<Sprite>("Black Bird Eating Sprite 1");
            blackBird.sprBombEat2 = birdModMan.Get<Sprite>("Black Bird Eating Sprite 2");
            blackBird.sprGauge = birdModMan.Get<Sprite>("Gauge Bird Strucked");

            birdModMan.Add<NPC>("Red Bird", redBird);
            birdModMan.Add<NPC>("Yellow Bird", yellowBird);
            birdModMan.Add<NPC>("Black Bird", blackBird);

            // Stickers
            Sticker birdsTroubleSticker = EnumExtensions.ExtendEnum<Sticker>("BirdsTrouble");
            stickerEnums["BirdsTrouble"] = birdsTroubleSticker;

            StickerData stickerData1 = new StickerBuilder<ExtendedStickerData>(Info)
                .SetSprite(birdModMan.Get<Sprite>("Sticker BirdsTrouble Sprite"))
                .SetDuplicateOddsMultiplier(1f)
                .SetEnum(birdsTroubleSticker)
                .Build();

            birdModMan.Add("BirdsTrouble", birdsTroubleSticker);

            Sticker notSoAngrySticker = EnumExtensions.ExtendEnum<Sticker>("NotSoAngry");
            stickerEnums["NotSoAngry"] = notSoAngrySticker;

            StickerData stickerData2 = new StickerBuilder<ExtendedStickerData>(Info)
                .SetSprite(birdModMan.Get<Sprite>("Sticker NotSoAngry Sprite"))
                .SetDuplicateOddsMultiplier(1f)
                .SetEnum(notSoAngrySticker)
                .Build();

            birdModMan.Add("NotSoAngry", notSoAngrySticker);

            Sticker lessBirdsSticker = EnumExtensions.ExtendEnum<Sticker>("LessBirds");
            stickerEnums["LessBirds"] = lessBirdsSticker;

            StickerData stickerData3 = new StickerBuilder<ExtendedStickerData>(Info)
                .SetSprite(birdModMan.Get<Sprite>("Sticker LessBirds Sprite"))
                .SetDuplicateOddsMultiplier(1f)
                .SetEnum(lessBirdsSticker)
                .Build();

            birdModMan.Add("LessBirds", lessBirdsSticker);

            // Items
            ItemObject goldenEgg = new ItemBuilder(Info)
                .SetNameAndDescription("Itm_GoldenEgg", "Desc_GoldenEgg")
                .SetSprites(birdModMan.Get<Sprite>("GoldenEggSmall"), birdModMan.Get<Sprite>("GoldenEggBig"))
                .SetEnum("GoldenEgg")
                .SetGeneratorCost(40)
                .SetItemComponent<ITM_GoldenEgg>()
                .Build();
            birdModMan.Add<ItemObject>("GoldenEgg", goldenEgg);

            ItemObject slingshotRed = new ItemBuilder(Info)
                .SetNameAndDescription("Itm_SlingshotRed", "Desc_SlingshotRed")
                .SetSprites(birdModMan.Get<Sprite>("SlingshotRedSmall"), birdModMan.Get<Sprite>("SlingshotRedBig"))
                .SetEnum("SlingshotRed")
                .SetShopPrice(450)
                .SetGeneratorCost(40)
                .SetItemComponent<ITM_Slingshot>()
                .Build();

            ITM_Slingshot slingshotRedComp = (ITM_Slingshot)slingshotRed.item;
            if (slingshotRedComp != null)
            {
                slingshotRedComp.birdType = 0;
                if (slingshotRedComp.sndUse == null) slingshotRedComp.sndUse = new List<SoundObject>();
                AddNonEmptySounds(slingshotRedComp.sndUse, "Slingshot Use Sound 1", "Slingshot Use Sound 2", "Slingshot Use Sound 3");
                slingshotRedComp.redPrefab = redObjPrefab;
            }

            birdModMan.Add<ItemObject>("SlingshotRed", slingshotRed);

            ItemObject slingshotChuck = new ItemBuilder(Info)
                .SetNameAndDescription("Itm_SlingshotChuck", "Desc_SlingshotChuck")
                .SetSprites(birdModMan.Get<Sprite>("SlingshotChuckSmall"), birdModMan.Get<Sprite>("SlingshotChuckBig"))
                .SetEnum("SlingshotChuck")
                .SetShopPrice(450)
                .SetGeneratorCost(40)
                .SetItemComponent<ITM_Slingshot>()
                .Build();

            ITM_Slingshot slingshotChuckComp = (ITM_Slingshot)slingshotChuck.item;
            if (slingshotChuckComp != null)
            {
                slingshotChuckComp.birdType = 1;
                if (slingshotChuckComp.sndUse == null) slingshotChuckComp.sndUse = new List<SoundObject>();
                AddNonEmptySounds(slingshotChuckComp.sndUse, "Slingshot Use Sound 1", "Slingshot Use Sound 2", "Slingshot Use Sound 3");
            }

            birdModMan.Add<ItemObject>("SlingshotChuck", slingshotChuck);

            ItemObject slingshotBomb = new ItemBuilder(Info)
                .SetNameAndDescription("Itm_SlingshotBomb", "Desc_SlingshotBomb")
                .SetSprites(birdModMan.Get<Sprite>("SlingshotBombSmall"), birdModMan.Get<Sprite>("SlingshotBombBig"))
                .SetEnum("SlingshotBomb")
                .SetShopPrice(450)
                .SetGeneratorCost(40)
                .SetItemComponent<ITM_Slingshot>()
                .Build();

            ITM_Slingshot slingshotBombComp = (ITM_Slingshot)slingshotBomb.item;
            if (slingshotBombComp != null)
            {
                slingshotBombComp.birdType = 2;
                if (slingshotBombComp.sndUse == null) slingshotBombComp.sndUse = new List<SoundObject>();
                AddNonEmptySounds(slingshotBombComp.sndUse, "Slingshot Use Sound 1", "Slingshot Use Sound 2", "Slingshot Use Sound 3");
            }

            birdModMan.Add<ItemObject>("SlingshotBomb", slingshotBomb);

            ItemObject superSeeds = new ItemBuilder(Info)
                .SetNameAndDescription("Itm_SuperSeeds", "Desc_SuperSeeds")
                .SetSprites(birdModMan.Get<Sprite>("SuperSeedsSmall"), birdModMan.Get<Sprite>("SuperSeedsBig"))
                .SetEnum("SuperSeeds")
                .SetShopPrice(820)
                .SetGeneratorCost(40)
                .SetItemComponent<ITM_SuperSeeds>()
                .Build();
            birdModMan.Add<ItemObject>("SuperSeeds", superSeeds);

            // Posters
            if (customPostersGen.Value) { RegisterCustomPosters(); }

            // Editor Support
            if (Chainloader.PluginInfos.ContainsKey("mtm101.rulerp.baldiplus.levelstudio"))
            {
                ABPackLoaderSupport.AddLoaderSupps();
                ABPackEditorSupport.AddEditorSupps();
            }

            yield break;
        }

        private void AddNonEmptySounds(List<SoundObject> targetList, params string[] soundKeys)
        {
            foreach (string key in soundKeys)
            {
                SoundObject snd = birdModMan.Get<SoundObject>(key);
                if (snd != null) { targetList.Add(snd); }
            }
        }

        private void RegisterCustomPosters()
        {
            string[] posterKeys = new string[]
            {
                "tex_pst_7granddad.png", "tex_pst_abvsbaldi_funvideotv.png", "tex_pst_funvideotv.png",
                "tex_pst_abvsbaldi_futuristichub.png", "tex_pst_evantubehd.png", "tex_pst_fgteev.png",
                "tex_pst_xplodinapple.png", "tex_pst_fuckgenai.png", "tex_pst_talkingred.png",
                "tex_pst_parasite.png", "tex_pst_trailerclassic.png", "tex_pst_trailermightyeagle.png",
                "tex_pst_trailersummerpignic.png", "tex_pst_trailerhamoween_0.png", "tex_pst_trailerhamoween_1.png",
                "tex_pst_trailerhamoween_2.png", "tex_pst_trailerwreckthehalls.png", "tex_pst_abspace_takeastand.png",
                "tex_pst_movie1_0.png", "tex_pst_movie1_1.png", "tex_pst_movie1_2.png", "tex_pst_movie1_3.png",
                "tex_pst_movie2_0.png", "tex_pst_movie2_1.png", "tex_pst_movie2_2.png", "tex_pst_movie2_3.png",
                "tex_pst_sonicandred.png"
            };

            string[] registeredNames = new string[]
            {
                "Poster 7 Grand Dad", "Poster FunVideoTV", "Poster FunVideoTV Baldi",
                "Poster FuturisticHub Baldi", "Poster Evan Tube", "Poster FGTeev",
                "Poster XplodinApple", "Poster Gen AI", "Poster Talking Red",
                "Poster Parasite", "Poster Trailer Cinematic", "Poster Trailer Mighty Eagle",
                "Poster Trailer Hal", "Poster Trailer Hamoween 1", "Poster Trailer Hamoween 2",
                "Poster Trailer Hamoween 3", "Poster Trailer Wreckthehalls", "Poster Space",
                "Poster Movie 1 1", "Poster Movie 1 2", "Poster Movie 1 3", "Poster Movie 1 4",
                "Poster Movie 2 1", "Poster Movie 2 2", "Poster Movie 2 3", "Poster Movie 2 4",
                "Poster Sonic"
            };

            for (int i = 0; i < posterKeys.Length; i++)
            {
                Texture2D pstTex = AssetLoader.TextureFromMod(this, sImgDir_Posters, posterKeys[i]);
                if (pstTex != null)
                {
                    PosterObject poster = ScriptableObject.CreateInstance<PosterObject>();
                    poster.baseTexture = pstTex;
                    poster.textData = new PosterTextData[0];
                    birdModMan.Add<PosterObject>(registeredNames[i], poster);
                }
            }
        }

        private void AddObjects(string floor, int floorNumber, SceneObject floorObject)
        {
            List<WeightedSticker> potentialStickersToAdd = new List<WeightedSticker>
            {
                new WeightedSticker(birdModMan.Get<Sticker>("BirdsTrouble"), 520),
                new WeightedSticker(birdModMan.Get<Sticker>("NotSoAngry"), 450),
                new WeightedSticker(birdModMan.Get<Sticker>("LessBirds"), 320)
            };
            floorObject.potentialStickers = CollectionExtensions.AddRangeToArray<WeightedSticker>(
                floorObject.potentialStickers,
                potentialStickersToAdd.ToArray()
            );

            CustomLevelObject[] itemObjects = floorObject.GetCustomLevelObjects();

            bool goldenEggSpawnedOnFloor = false;
            ItemObject goldenEggItem = birdModMan.Get<ItemObject>("GoldenEgg");

            int goldenEggTargetIndex = itemObjects.Length > 0 ? UnityEngine.Random.Range(0, itemObjects.Length) : -1;

            for (int i = 0; i < itemObjects.Length; i++)
            {
                if (itemObjects[i].IsModifiedByMod(Info)) continue;

                itemObjects[i].potentialItems = itemObjects[i].potentialItems.AddItem(new WeightedItemObject() { selection = birdModMan.Get<ItemObject>("SuperSeeds"), weight = 25 }).ToArray();
                itemObjects[i].potentialItems = itemObjects[i].potentialItems.AddItem(new WeightedItemObject() { selection = birdModMan.Get<ItemObject>("SlingshotRed"), weight = 52 }).ToArray();

                if (!goldenEggSpawnedOnFloor && i == goldenEggTargetIndex)
                {
                    itemObjects[i].potentialItems = itemObjects[i].potentialItems.AddItem(new WeightedItemObject() { selection = goldenEggItem, weight = 250 }).ToArray();
                    goldenEggSpawnedOnFloor = true;
                }

                itemObjects[i].MarkAsModifiedByMod(Info);
            }

            floorObject.shopItems = floorObject.shopItems.AddItem(new WeightedItemObject() { selection = birdModMan.Get<ItemObject>("SuperSeeds"), weight = 60 }).ToArray();
            floorObject.shopItems = floorObject.shopItems.AddItem(new WeightedItemObject() { selection = birdModMan.Get<ItemObject>("SlingshotRed"), weight = 72 }).ToArray();

            var redBird = birdModMan.Get<NPC>("Red Bird");
            var yellowBird = birdModMan.Get<NPC>("Yellow Bird");
            var blackBird = birdModMan.Get<NPC>("Black Bird");

            List<WeightedNPC> potentialBirds = new List<WeightedNPC>();

            if (BoolChecks.isLessBirds)
            {
                List<NPC> availableBirds = new List<NPC> { redBird, yellowBird, blackBird };

                NPC chosenBird = availableBirds[UnityEngine.Random.Range(0, availableBirds.Count)];
                
                int weight = (floor.StartsWith("F")) ? 100 : 180;
                potentialBirds.Add(new WeightedNPC() { selection = chosenBird, weight = weight });
            }
            else
            {
                int weight = (floor.StartsWith("F")) ? 250 : 280;
                potentialBirds.Add(new WeightedNPC() { selection = redBird, weight = weight });
                potentialBirds.Add(new WeightedNPC() { selection = yellowBird, weight = weight });
                potentialBirds.Add(new WeightedNPC() { selection = blackBird, weight = weight });
            }

            if (floor.StartsWith("F") || floor == "END") { floorObject.potentialNPCs.AddRange(potentialBirds); }

            floorObject.MarkAsNeverUnload();
        }

        public class ABPackSave : ModdedSaveGameIOBinary
        {
            public override BepInEx.PluginInfo pluginInfo => ABPackPlugin.Instance.Info;

            public override void Load(BinaryReader reader) { reader.ReadByte(); }

            public override void Reset()
            {
            }

            public override void Save(BinaryWriter writer) { writer.Write((byte)0); }

            public override string[] GenerateTags()
            {
                List<string> generatedTags = new List<string>();
                if (ABPackPlugin.Instance.customPostersGen.Value) { generatedTags.Add("CustomPostersGen"); }
                return generatedTags.ToArray();
            }

            public override string DisplayTags(string[] tags) { return tags.Contains("CustomPostersGen") ? "Custom Posters Generation" : "Standard Mode"; }
        }

        public static Dictionary<string, Sticker> stickerEnums = new Dictionary<string, Sticker>();

        static Color birdRedColor = new Color(214f / 255f, 0f / 255f, 45f / 255f);
        static Color birdYellowColor = new Color(241f / 255f, 219f / 255f, 32f / 255f);
        static Color birdBlackColor = new Color(91f / 255f, 91f / 255f, 91f / 255f);
    }

    [HarmonyPatch(typeof(MainMenu), "Start")]
    public class MainMenuPatcher { public static void Postfix() { BoolChecksPatcher.ResetOnReturn(); } }

    public class BoolChecksPatcher
    {
        public static void ResetOnReturn()
        {
            BoolChecks.musicPlaying = false;
            BoolChecks.isNerfed = false;
            BoolChecks.isAngryBaldi = false;
            BoolChecks.isAngryGolden = false;
            BoolChecks.isLessBirds = false;
            BoolChecks.playerIsSquashed = false;
            BoolChecks.isMusic = 0;
        }
    }

    [HarmonyPatch(typeof(StickerManager), "ApplySticker")]
    public class StickerManagerPatcher
    {
        public static void Postfix(StickerStateData sticker)
        {
            if (ABPackPlugin.stickerEnums.TryGetValue("NotSoAngry", out Sticker nerfedSticker)) { if (sticker.sticker == nerfedSticker) { BoolChecks.isNerfed = true; } }
            if (ABPackPlugin.stickerEnums.TryGetValue("BirdsTrouble", out Sticker angryBaldiSticker)) { if (sticker.sticker == angryBaldiSticker) { BoolChecks.isAngryBaldi = true; } }
            if (ABPackPlugin.stickerEnums.TryGetValue("LessBirds", out Sticker birdsLessSticker)) { if (sticker.sticker == birdsLessSticker) { BoolChecks.isLessBirds = true; } }
        }
    }
}