using UnityEngine;
using UnityEditor;
using System.IO;

public class GameSetupEditor : EditorWindow
{
    [MenuItem("游戏配置/一键创建角色数据")]
    public static void CreateCharacterData()
    {
        string folderPath = "Assets/CharacterData";
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "CharacterData");
        }
        
        CreateAthleticZombie(folderPath);
        CreateChargeZombie(folderPath);
        CreateTankZombie(folderPath);
        CreateJumpZombie(folderPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[游戏配置] 4个角色数据资源创建完成！路径: " + folderPath);
        EditorUtility.DisplayDialog("完成", "4个角色数据资源已创建完成！\n路径: Assets/CharacterData", "确定");
    }
    
    [MenuItem("游戏配置/一键创建游戏模式数据")]
    public static void CreateGameModeData()
    {
        string folderPath = "Assets/GameModeData";
        
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "GameModeData");
        }
        
        CreateEndlessMode(folderPath);
        CreateHellMode(folderPath);
        CreateNightMode(folderPath);
        CreateStormMode(folderPath);
        CreateIceMode(folderPath);
        CreateTimeLimitMode(folderPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("[游戏配置] 游戏模式数据资源创建完成！路径: " + folderPath);
        EditorUtility.DisplayDialog("完成", "游戏模式数据资源已创建完成！\n路径: Assets/GameModeData", "确定");
    }
    
    static void CreateAthleticZombie(string folderPath)
    {
        string assetPath = folderPath + "/AthleticZombie.asset";
        
        if (AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath) != null)
        {
            Debug.Log("[游戏配置] AthleticZombie 已存在，跳过创建");
            return;
        }
        
        CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
        data.characterName = "运动僵尸";
        data.characterID = "athletic_zombie";
        data.minForwardSpeed = 15f;
        data.forwardSpeed = 26f;
        data.maxForwardSpeed = 43f;
        data.speedChangeRate = 2f;
        data.horizontalSpeed = 8f;
        data.jumpForce = 9f;
        data.gravity = 20f;
        data.maxHealth = 200f;
        data.regenDelay = 3f;
        data.regenRate = 15f;
        
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[游戏配置] 创建角色数据: 运动僵尸");
    }
    
    static void CreateChargeZombie(string folderPath)
    {
        string assetPath = folderPath + "/ChargeZombie.asset";
        
        if (AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath) != null)
        {
            Debug.Log("[游戏配置] ChargeZombie 已存在，跳过创建");
            return;
        }
        
        CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
        data.characterName = "冲锋僵尸";
        data.characterID = "charge_zombie";
        data.minForwardSpeed = 15f;
        data.forwardSpeed = 28f;
        data.maxForwardSpeed = 50f;
        data.speedChangeRate = 3f;
        data.horizontalSpeed = 10f;
        data.jumpForce = 9f;
        data.gravity = 20f;
        data.maxHealth = 150f;
        data.regenDelay = 3f;
        data.regenRate = 20f;
        
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[游戏配置] 创建角色数据: 冲锋僵尸");
    }
    
    static void CreateTankZombie(string folderPath)
    {
        string assetPath = folderPath + "/TankZombie.asset";
        
        if (AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath) != null)
        {
            Debug.Log("[游戏配置] TankZombie 已存在，跳过创建");
            return;
        }
        
        CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
        data.characterName = "坦克僵尸";
        data.characterID = "tank_zombie";
        data.minForwardSpeed = 13f;
        data.forwardSpeed = 24f;
        data.maxForwardSpeed = 40f;
        data.speedChangeRate = 1.5f;
        data.horizontalSpeed = 7f;
        data.jumpForce = 8f;
        data.gravity = 25f;
        data.maxHealth = 250f;
        data.regenDelay = 4f;
        data.regenRate = 15f;
        
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[游戏配置] 创建角色数据: 坦克僵尸");
    }
    
    static void CreateJumpZombie(string folderPath)
    {
        string assetPath = folderPath + "/JumpZombie.asset";
        
        if (AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath) != null)
        {
            Debug.Log("[游戏配置] JumpZombie 已存在，跳过创建");
            return;
        }
        
        CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
        data.characterName = "蹦蹦僵尸";
        data.characterID = "jump_zombie";
        data.minForwardSpeed = 15f;
        data.forwardSpeed = 26f;
        data.maxForwardSpeed = 43f;
        data.speedChangeRate = 2f;
        data.horizontalSpeed = 7f;
        data.jumpForce = 11f;
        data.gravity = 15f;
        data.maxHealth = 180f;
        data.regenDelay = 4f;
        data.regenRate = 15f;
        
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[游戏配置] 创建角色数据: 蹦蹦僵尸");
    }
    
    [MenuItem("游戏配置/自动配置CharacterManager")]
    public static void AutoConfigureCharacterManager()
    {
        CharacterManager manager = FindObjectOfType<CharacterManager>();
        
        if (manager == null)
        {
            GameObject go = new GameObject("CharacterManager");
            manager = go.AddComponent<CharacterManager>();
            Debug.Log("[游戏配置] 创建了新的 CharacterManager 对象");
        }
        
        CharacterData[] characters = new CharacterData[4];
        characters[0] = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/CharacterData/AthleticZombie.asset");
        characters[1] = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/CharacterData/ChargeZombie.asset");
        characters[2] = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/CharacterData/TankZombie.asset");
        characters[3] = AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/CharacterData/JumpZombie.asset");
        
        bool allLoaded = true;
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null)
            {
                allLoaded = false;
                Debug.LogError($"[游戏配置] 角色数据 {i} 加载失败！请先运行'一键创建角色数据'");
            }
        }
        
        if (!allLoaded)
        {
            EditorUtility.DisplayDialog("错误", "角色数据加载失败！\n请先运行菜单: 游戏配置 > 一键创建角色数据", "确定");
            return;
        }
        
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty charactersProp = so.FindProperty("characters");
        charactersProp.arraySize = 4;
        
        for (int i = 0; i < 4; i++)
        {
            charactersProp.GetArrayElementAtIndex(i).objectReferenceValue = characters[i];
        }
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
        
        Debug.Log("[游戏配置] CharacterManager 配置完成！已加载4个角色数据");
        EditorUtility.DisplayDialog("完成", "CharacterManager 已自动配置完成！\n已加载4个角色数据", "确定");
        
        Selection.activeGameObject = manager.gameObject;
    }
    
    [MenuItem("游戏配置/自动配置MainMenuUI")]
    public static void AutoConfigureMainMenuUI()
    {
        MainMenuUI menuUI = FindObjectOfType<MainMenuUI>();
        
        if (menuUI == null)
        {
            EditorUtility.DisplayDialog("错误", "未找到 MainMenuUI 组件！\n请确保场景中有挂载了 MainMenuUI 脚本的对象", "确定");
            return;
        }
        
        SerializedObject so = new SerializedObject(menuUI);
        
        UnityEngine.UI.Button[] allButtons = FindObjectsOfType<UnityEngine.UI.Button>();
        
        UnityEngine.UI.Button[] characterButtons = new UnityEngine.UI.Button[4];
        UnityEngine.UI.Button endlessMode = null;
        UnityEngine.UI.Button hellMode = null;
        UnityEngine.UI.Button nightMode = null;
        UnityEngine.UI.Button stormMode = null;
        UnityEngine.UI.Button timeLimit = null;
        UnityEngine.UI.Button iceMode = null;
        UnityEngine.UI.Button getNewChar = null;
        UnityEngine.UI.Button gameRecord = null;
        
        foreach (var btn in allButtons)
        {
            string name = btn.gameObject.name;
            
            if (name.Contains("CharacterButton1") || name.Contains("运动僵尸"))
                characterButtons[0] = btn;
            else if (name.Contains("CharacterButton2") || name.Contains("冲锋僵尸"))
                characterButtons[1] = btn;
            else if (name.Contains("CharacterButton3") || name.Contains("坦克僵尸"))
                characterButtons[2] = btn;
            else if (name.Contains("CharacterButton4") || name.Contains("蹦蹦僵尸"))
                characterButtons[3] = btn;
            else if (name.Contains("EndlessMode") || name.Contains("无尽模式"))
                endlessMode = btn;
            else if (name.Contains("HellMode") || name.Contains("地狱模式"))
                hellMode = btn;
            else if (name.Contains("NightMode") || name.Contains("黑夜模式"))
                nightMode = btn;
            else if (name.Contains("StormMode") || name.Contains("雷暴模式"))
                stormMode = btn;
            else if (name.Contains("TimeLimit") || name.Contains("限时模式"))
                timeLimit = btn;
            else if (name.Contains("IceMode") || name.Contains("冰封模式"))
                iceMode = btn;
            else if (name.Contains("GetNewCharacter") || name.Contains("获取新角色"))
                getNewChar = btn;
            else if (name.Contains("GameRecord") || name.Contains("游戏记录"))
                gameRecord = btn;
        }
        
        SerializedProperty charButtonsProp = so.FindProperty("characterButtons");
        charButtonsProp.arraySize = 4;
        for (int i = 0; i < 4; i++)
        {
            charButtonsProp.GetArrayElementAtIndex(i).objectReferenceValue = characterButtons[i];
        }
        
        so.FindProperty("endlessModeButton").objectReferenceValue = endlessMode;
        so.FindProperty("hellModeButton").objectReferenceValue = hellMode;
        so.FindProperty("nightModeButton").objectReferenceValue = nightMode;
        so.FindProperty("stormModeButton").objectReferenceValue = stormMode;
        so.FindProperty("timeLimitModeButton").objectReferenceValue = timeLimit;
        so.FindProperty("iceModeButton").objectReferenceValue = iceMode;
        so.FindProperty("getNewCharacterButton").objectReferenceValue = getNewChar;
        so.FindProperty("gameRecordButton").objectReferenceValue = gameRecord;
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(menuUI);
        
        int foundCount = 0;
        for (int i = 0; i < 4; i++)
            if (characterButtons[i] != null) foundCount++;
        
        Debug.Log($"[游戏配置] MainMenuUI 自动配置完成！找到 {foundCount}/4 个角色按钮");
        EditorUtility.DisplayDialog("完成", $"MainMenuUI 已自动配置！\n找到 {foundCount}/4 个角色按钮\n\n如果有按钮未找到，请检查按钮命名", "确定");
        
        Selection.activeGameObject = menuUI.gameObject;
    }
    
    [MenuItem("游戏配置/检查UI按钮命名")]
    public static void CheckButtonNames()
    {
        UnityEngine.UI.Button[] allButtons = FindObjectsOfType<UnityEngine.UI.Button>();
        
        if (allButtons.Length == 0)
        {
            EditorUtility.DisplayDialog("提示", "场景中没有找到任何按钮！", "确定");
            return;
        }
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine("=== 场景中的所有按钮 ===");
        sb.AppendLine();
        
        int characterCount = 0;
        int modeCount = 0;
        int otherCount = 0;
        
        foreach (var btn in allButtons)
        {
            string name = btn.gameObject.name;
            string category = "未分类";
            
            if (name.Contains("CharacterButton") || name.Contains("运动僵尸") || 
                name.Contains("冲锋僵尸") || name.Contains("坦克僵尸") || name.Contains("蹦蹦僵尸"))
            {
                category = "✅ 角色按钮";
                characterCount++;
            }
            else if (name.Contains("EndlessMode") || name.Contains("无尽模式"))
            {
                category = "✅ 无尽模式";
                modeCount++;
            }
            else if (name.Contains("HellMode") || name.Contains("地狱模式"))
            {
                category = "✅ 地狱模式";
                modeCount++;
            }
            else if (name.Contains("NightMode") || name.Contains("黑夜模式"))
            {
                category = "✅ 黑夜模式";
                modeCount++;
            }
            else if (name.Contains("StormMode") || name.Contains("雷暴模式"))
            {
                category = "✅ 雷暴模式";
                modeCount++;
            }
            else if (name.Contains("TimeLimit") || name.Contains("限时模式"))
            {
                category = "✅ 限时模式";
                modeCount++;
            }
            else if (name.Contains("GetNewCharacter") || name.Contains("获取新角色"))
            {
                category = "✅ 获取新角色";
                otherCount++;
            }
            else if (name.Contains("GameRecord") || name.Contains("游戏记录"))
            {
                category = "✅ 游戏记录";
                otherCount++;
            }
            else
            {
                category = "❌ 未识别";
            }
            
            sb.AppendLine($"[{category}] {name}");
        }
        
        sb.AppendLine();
        sb.AppendLine("=== 统计 ===");
        sb.AppendLine($"角色按钮: {characterCount}/4");
        sb.AppendLine($"模式按钮: {modeCount}/5");
        sb.AppendLine($"其他按钮: {otherCount}/2");
        sb.AppendLine($"总按钮数: {allButtons.Length}");
        
        Debug.Log(sb.ToString());
        
        string message = $"按钮检查完成！\n\n" +
                        $"角色按钮: {characterCount}/4\n" +
                        $"模式按钮: {modeCount}/5\n" +
                        $"其他按钮: {otherCount}/2\n\n" +
                        $"详细信息请查看Console窗口";
        
        EditorUtility.DisplayDialog("按钮检查结果", message, "确定");
    }
    
    [MenuItem("游戏配置/完整配置流程")]
    public static void ShowSetupGuide()
    {
        string guide = "=== 完整配置流程 ===\n\n" +
                      "1. 点击菜单: 游戏配置 > 一键创建角色数据\n" +
                      "   → 自动创建4个角色数据资源\n\n" +
                      "2. 创建主菜单场景并配置UI\n" +
                      "   → 确保按钮命名包含关键字\n" +
                      "   → 可用'检查UI按钮命名'验证\n\n" +
                      "3. 点击菜单: 游戏配置 > 自动配置CharacterManager\n" +
                      "   → 自动加载角色数据\n\n" +
                      "4. 点击菜单: 游戏配置 > 自动配置MainMenuUI\n" +
                      "   → 自动连接按钮引用\n\n" +
                      "5. 配置Build Settings\n" +
                      "   → 添加MainMenu和GameScene场景\n\n" +
                      "详细说明请查看: 快速配置指南.md";
        
        EditorUtility.DisplayDialog("配置流程", guide, "确定");
    }
    
    static void CreateEndlessMode(string folderPath)
    {
        string assetPath = folderPath + "/EndlessMode.asset";
        
        if (AssetDatabase.LoadAssetAtPath<GameModeData>(assetPath) != null)
        {
            Debug.Log("[游戏配置] EndlessMode 已存在，跳过创建");
            return;
        }
        
        GameModeData data = ScriptableObject.CreateInstance<GameModeData>();
        data.modeName = "无尽模式";
        data.modeType = GameMode.Endless;
        data.initialHealthMultiplier = 1f;
        data.regenRateMultiplier = 1f;
        data.useDynamicDifficulty = false;
        data.ambientLightMultiplier = 1f;
        data.addSpecialLight = false;
        data.isTimeLimited = false;
        data.scoreMultiplier = 1f;
        
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[游戏配置] 创建游戏模式: 无尽模式");
    }
    
    static void CreateHellMode(string folderPath)
    {
        string assetPath = folderPath + "/HellMode.asset";
        
        if (AssetDatabase.LoadAssetAtPath<GameModeData>(assetPath) != null)
        {
            Debug.Log("[游戏配置] HellMode 已存在，跳过创建");
            return;
        }
        
        GameModeData data = ScriptableObject.CreateInstance<GameModeData>();
        data.modeName = "地狱模式";
        data.modeType = GameMode.Hell;
        
        data.initialHealthMultiplier = 2f;
        data.regenRateMultiplier = 2f;
        
        data.useDynamicDifficulty = true;
        data.difficultyStages = new DifficultyStage[8];
        
        data.difficultyStages[0] = new DifficultyStage { distanceStart = 0f, distanceEnd = 200f, difficultyStart = 1.0f, difficultyEnd = 1.6f };
        data.difficultyStages[1] = new DifficultyStage { distanceStart = 200f, distanceEnd = 400f, difficultyStart = 1.6f, difficultyEnd = 2.4f };
        data.difficultyStages[2] = new DifficultyStage { distanceStart = 400f, distanceEnd = 800f, difficultyStart = 2.4f, difficultyEnd = 2.8f };
        data.difficultyStages[3] = new DifficultyStage { distanceStart = 800f, distanceEnd = 1600f, difficultyStart = 2.8f, difficultyEnd = 3.3f };
        data.difficultyStages[4] = new DifficultyStage { distanceStart = 1600f, distanceEnd = 3200f, difficultyStart = 3.3f, difficultyEnd = 3.8f };
        data.difficultyStages[5] = new DifficultyStage { distanceStart = 3200f, distanceEnd = 6400f, difficultyStart = 3.8f, difficultyEnd = 4.5f };
        data.difficultyStages[6] = new DifficultyStage { distanceStart = 6400f, distanceEnd = 12800f, difficultyStart = 4.5f, difficultyEnd = 5.0f };
        data.difficultyStages[7] = new DifficultyStage { distanceStart = 12800f, distanceEnd = 999999f, difficultyStart = 5.0f, difficultyEnd = 5.0f };
        
        data.minCountMultiplierStart = 1.0f;
        data.minCountMultiplierEnd = 1.4f;
        data.maxCountMultiplierStart = 1.0f;
        data.maxCountMultiplierEnd = 1.41f;
        data.spacingMultiplierStart = 1.2f;
        data.spacingMultiplierEnd = 0.1f;
        
        data.ambientLightMultiplier = 0.6f;
        data.addSpecialLight = true;
        data.specialLightColor = new Color(228f/255f, 18f/255f, 51f/255f);
        data.specialLightIntensity = 0.5f;
        
        data.isTimeLimited = false;
        data.scoreMultiplier = 1f;
        
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[游戏配置] 创建游戏模式: 地狱模式");
        Debug.Log("  - 血量倍率: 2x");
        Debug.Log("  - 回血倍率: 2x");
        Debug.Log("  - 动态难度: 8个阶段");
        Debug.Log("  - 环境光照: 0.6x (暗红色灯光)");
    }
    
    static void CreateNightMode(string folderPath)
    {
        string assetPath = folderPath + "/NightMode.asset";
        
        if (AssetDatabase.LoadAssetAtPath<GameModeData>(assetPath) != null)
        {
            Debug.Log("[游戏配置] NightMode 已存在，跳过创建");
            return;
        }
        
        GameModeData data = ScriptableObject.CreateInstance<GameModeData>();
        data.modeName = "黑夜模式";
        data.modeType = GameMode.Night;
        
        data.initialHealthMultiplier = 1f;
        data.regenRateMultiplier = 1f;
        
        data.useDynamicDifficulty = false;
        
        data.minCountMultiplierStart = 1.0f;
        data.minCountMultiplierEnd = 1.0f;
        data.maxCountMultiplierStart = 1.0f;
        data.maxCountMultiplierEnd = 1.0f;
        data.spacingMultiplierStart = 1.0f;
        data.spacingMultiplierEnd = 1.0f;
        
        data.ambientLightMultiplier = 0.45f;
        data.addSpecialLight = true;
        data.specialLightColor = new Color(228f/255f, 18f/255f, 51f/255f);
        data.specialLightIntensity = 1.5f;
        
        data.isTimeLimited = false;
        data.scoreMultiplier = 1f;
        
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[游戏配置] 创建游戏模式: 黑夜模式");
        Debug.Log("  - 环境光照: 0.45x");
        Debug.Log("  - 特殊灯光: 启用, 强度 1.5");
    }
    
    static void CreateStormMode(string folderPath)
    {
        string assetPath = folderPath + "/StormMode.asset";

        if (AssetDatabase.LoadAssetAtPath<GameModeData>(assetPath) != null)
        {
            Debug.Log("[GameSetup] StormMode already exists, skipping.");
            return;
        }

        GameModeData data = ScriptableObject.CreateInstance<GameModeData>();
        data.modeName = "雷暴模式";
        data.modeType = GameMode.Storm;

        data.initialHealthMultiplier = 1f;
        data.regenRateMultiplier = 1f;

        data.useDynamicDifficulty = false;

        data.minCountMultiplierStart = 1.0f;
        data.minCountMultiplierEnd = 1.0f;
        data.maxCountMultiplierStart = 1.0f;
        data.maxCountMultiplierEnd = 1.0f;
        data.spacingMultiplierStart = 1.0f;
        data.spacingMultiplierEnd = 1.0f;

        data.ambientLightMultiplier = 0.1f;
        data.addSpecialLight = true;
        data.specialLightColor = new Color(0.16470589f, 0.14387326f, 0.2509804f);
        data.specialLightIntensity = 6f;

        data.isTimeLimited = false;
        data.scoreMultiplier = 1f;

        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[GameSetup] Created game mode: StormMode");
    }

    static void CreateIceMode(string folderPath)
    {
        string assetPath = folderPath + "/IceMode.asset";

        if (AssetDatabase.LoadAssetAtPath<GameModeData>(assetPath) != null)
        {
            Debug.Log("[GameSetup] IceMode already exists, skipping.");
            return;
        }

        GameModeData data = ScriptableObject.CreateInstance<GameModeData>();
        data.modeName = "冰封模式";
        data.modeType = GameMode.Ice;

        data.initialHealthMultiplier = 1f;
        data.regenRateMultiplier = 1f;

        data.useDynamicDifficulty = false;

        data.minCountMultiplierStart = 1.0f;
        data.minCountMultiplierEnd = 1.0f;
        data.maxCountMultiplierStart = 1.0f;
        data.maxCountMultiplierEnd = 1.0f;
        data.spacingMultiplierStart = 1.0f;
        data.spacingMultiplierEnd = 1.0f;

        data.ambientLightMultiplier = 0.85f;
        data.addSpecialLight = false;

        data.iceShooterWeightMultiplier = 3.0f;
        data.iceMushroomWeightMultiplier = 3.0f;
        data.iceSnowPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Epic Toon FX/Prefabs/Environment/Weather/Snow/SnowSpinny.prefab");
        data.iceSnowOffset = new Vector3(0f, 12f, 0f);
        data.iceSnowFollowPlayer = true;
        data.iceSnowWorldPosition = new Vector3(0f, 12f, 0f);
        data.iceSnowAreaSize = new Vector3(120f, 40f, 120f);

        data.isTimeLimited = false;
        data.scoreMultiplier = 1f;

        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[GameSetup] Created game mode: IceMode");
    }

    static void CreateTimeLimitMode(string folderPath)
    {
        string assetPath = folderPath + "/TimeLimitMode.asset";
        
        if (AssetDatabase.LoadAssetAtPath<GameModeData>(assetPath) != null)
        {
            Debug.Log("[游戏配置] TimeLimitMode 已存在，跳过创建");
            return;
        }
        
        GameModeData data = ScriptableObject.CreateInstance<GameModeData>();
        data.modeName = "限时模式";
        data.modeType = GameMode.TimeLimit;
        
        data.initialHealthMultiplier = 1f;
        data.regenRateMultiplier = 1f;
        
        data.useDynamicDifficulty = false;
        
        data.minCountMultiplierStart = 1.0f;
        data.minCountMultiplierEnd = 1.0f;
        data.maxCountMultiplierStart = 1.0f;
        data.maxCountMultiplierEnd = 1.0f;
        data.spacingMultiplierStart = 1.0f;
        data.spacingMultiplierEnd = 1.0f;
        
        data.ambientLightMultiplier = 1f;
        data.addSpecialLight = false;
        
        data.isTimeLimited = true;
        data.scoreMultiplier = 1f;
        
        AssetDatabase.CreateAsset(data, assetPath);
        Debug.Log("[游戏配置] 创建游戏模式: 限时模式");
        Debug.Log("  - 每秒减少5点血量上限");
    }
    
    [MenuItem("游戏配置/自动配置GameModeManager")]
    public static void AutoConfigureGameModeManager()
    {
        GameModeManager manager = FindObjectOfType<GameModeManager>();
        
        if (manager == null)
        {
            GameObject go = new GameObject("GameModeManager");
            manager = go.AddComponent<GameModeManager>();
            Debug.Log("[游戏配置] 创建了新的 GameModeManager 对象");
        }
        
        GameModeData endlessMode = AssetDatabase.LoadAssetAtPath<GameModeData>("Assets/GameModeData/EndlessMode.asset");
        GameModeData hellMode = AssetDatabase.LoadAssetAtPath<GameModeData>("Assets/GameModeData/HellMode.asset");
        GameModeData nightMode = AssetDatabase.LoadAssetAtPath<GameModeData>("Assets/GameModeData/NightMode.asset");
        GameModeData stormMode = AssetDatabase.LoadAssetAtPath<GameModeData>("Assets/GameModeData/StormMode.asset");
        GameModeData iceMode = AssetDatabase.LoadAssetAtPath<GameModeData>("Assets/GameModeData/IceMode.asset");
        GameModeData timeLimitMode = AssetDatabase.LoadAssetAtPath<GameModeData>("Assets/GameModeData/TimeLimitMode.asset");
        
        if (endlessMode == null || hellMode == null || nightMode == null || stormMode == null || iceMode == null || timeLimitMode == null)
        {
            EditorUtility.DisplayDialog("错误", "游戏模式数据加载失败！\n请先运行菜单: 游戏配置 > 一键创建游戏模式数据", "确定");
            return;
        }
        
        SerializedObject so = new SerializedObject(manager);
        SerializedProperty modesProp = so.FindProperty("gameModes");
        modesProp.arraySize = 6;
        modesProp.GetArrayElementAtIndex(0).objectReferenceValue = endlessMode;
        modesProp.GetArrayElementAtIndex(1).objectReferenceValue = hellMode;
        modesProp.GetArrayElementAtIndex(2).objectReferenceValue = nightMode;
        modesProp.GetArrayElementAtIndex(3).objectReferenceValue = stormMode;
        modesProp.GetArrayElementAtIndex(4).objectReferenceValue = iceMode;
        modesProp.GetArrayElementAtIndex(5).objectReferenceValue = timeLimitMode;
        
        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(manager);
        
        Debug.Log("[渨戏配置] GameModeManager 配置完成！已加载4个渨戏模式");
        EditorUtility.DisplayDialog("完成", "GameModeManager 已自动配置完成！\n已加载4个渨戏模式", "确定");
        
        Selection.activeGameObject = manager.gameObject;
    }
}
