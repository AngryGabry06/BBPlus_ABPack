using System;
using System.Collections.Generic;
using System.Text;
using PlusStudioLevelLoader;
using PlusLevelStudio;
using UnityEngine;
using MTM101BaldAPI.AssetTools;
using PlusLevelStudio.Editor;
using PlusStudioLevelFormat;
using PlusLevelStudio.Editor.Tools;
using MTM101BaldAPI;

namespace bbplus.angrybirdspack
{
    public static class ABPackEditorSupport
    {
        public static void AddEditorSupps()
        {
            AssetManager editorModMan = ABPackPlugin.birdModMan;

            //LevelStudioPlugin.Instance.selectableTextures.Add("BirdFloor");
            //LevelStudioPlugin.Instance.selectableTextures.Add("BirdWall");
            //LevelStudioPlugin.Instance.selectableTextures.Add("BirdCeiling");

            Sprite npcRedSprite = editorModMan.Get<Sprite>("Red Bird Sprite");
            NPC redBirdNPC = editorModMan.Get<NPC>("Red Bird");
            SpriteRenderer rendRed = redBirdNPC.GetComponentInChildren<SpriteRenderer>();
            if (rendRed != null) { rendRed.sprite = npcRedSprite; }
            EditorInterface.AddNPCVisual("redBird", redBirdNPC);

            Sprite npcChuckSprite = editorModMan.Get<Sprite>("Yellow Bird Sprite");
            NPC yellowBirdNPC = editorModMan.Get<NPC>("Yellow Bird");
            SpriteRenderer rendChuck = yellowBirdNPC.GetComponentInChildren<SpriteRenderer>();
            if (rendChuck != null) { rendChuck.sprite = npcChuckSprite; }
            EditorInterface.AddNPCVisual("yellowBird", yellowBirdNPC);

            Sprite npcBombSprite = editorModMan.Get<Sprite>("Black Bird Sprite");
            NPC blackBirdNPC = editorModMan.Get<NPC>("Black Bird");
            SpriteRenderer rendBomb = blackBirdNPC.GetComponentInChildren<SpriteRenderer>();
            if (rendBomb != null) { rendBomb.sprite = npcBombSprite; }
            EditorInterface.AddNPCVisual("blackBird", blackBirdNPC);

            //EditorInterface.AddObjectVisualWithCustomCapsuleCollider("obj_slingshot", LevelLoaderPlugin.Instance.basicObjects["obj_slingshot"], 1f, 7f, 1, Vector3.up * 3.5f);
            //LevelStudioPlugin.Instance.defaultRoomTextures.Add("birdRoom", new TextureContainer("BirdFloor", "BirdWall", "BirdCeiling"));

            EditorInterfaceModes.AddModeCallback(AddContent);
        }

        public static void AddContent(EditorMode mode, bool vanillaCompliant)
        {
            AssetManager editorModMan = ABPackPlugin.birdModMan;
            //EditorInterfaceModes.AddToolToCategory(mode, "rooms", new RoomTool("birdRoom", editorModMan.Get<Sprite>("Editor Room Bird")));
            EditorInterfaceModes.AddToolToCategory(mode, "npcs", new NPCTool("redBird", editorModMan.Get<Sprite>("Editor NPC Red")));
            EditorInterfaceModes.AddToolToCategory(mode, "npcs", new NPCTool("yellowBird", editorModMan.Get<Sprite>("Editor NPC Chuck")));
            EditorInterfaceModes.AddToolToCategory(mode, "npcs", new NPCTool("blackBird", editorModMan.Get<Sprite>("Editor NPC Bomb")));
            //EditorInterfaceModes.AddToolToCategory(mode, "objects", new ObjectToolNoRotation("obj_slingshot", editorModMan.Get<Sprite>("Editor OBJ Slingshot")));
            EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("GoldenEgg"));
            EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("SuperSeeds"));
            EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("SlingshotRed"));
            EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("SlingshotChuck"));
            EditorInterfaceModes.AddToolToCategory(mode, "items", new ItemTool("SlingshotBomb"));
        }
    }

    public static class ABPackLoaderSupport
    {
        public static void AddLoaderSupps()
        {
            AssetManager editorModMan = ABPackPlugin.birdModMan;
            LevelLoaderPlugin.Instance.npcAliases.Add("redBird", editorModMan.Get<NPC>("Red Bird"));
            LevelLoaderPlugin.Instance.npcAliases.Add("yellowBird", editorModMan.Get<NPC>("Yellow Bird"));
            LevelLoaderPlugin.Instance.npcAliases.Add("blackBird", editorModMan.Get<NPC>("Black Bird"));
            LevelLoaderPlugin.Instance.itemObjects.Add("GoldenEgg", editorModMan.Get<ItemObject>("GoldenEgg"));
            LevelLoaderPlugin.Instance.itemObjects.Add("SuperSeeds", editorModMan.Get<ItemObject>("SuperSeeds"));
            LevelLoaderPlugin.Instance.itemObjects.Add("SlingshotRed", editorModMan.Get<ItemObject>("SlingshotRed"));
            LevelLoaderPlugin.Instance.itemObjects.Add("SlingshotChuck", editorModMan.Get<ItemObject>("SlingshotChuck"));
            LevelLoaderPlugin.Instance.itemObjects.Add("SlingshotBomb", editorModMan.Get<ItemObject>("SlingshotBomb"));
        }
    }
}