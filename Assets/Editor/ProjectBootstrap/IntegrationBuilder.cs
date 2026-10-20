using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRBat;

public static class IntegrationBuilder
{
    const string AScene = "Assets/VRBat/Scenes/VRBat.unity";
    const string HomeScene = "Assets/Scenes/HomeScene.unity";
    const string GameScene = "Assets/Scenes/GameScene.unity";

    [MenuItem("VR Bat/Build Yuki Integration")]
    public static void Apply()
    {
        var gameScene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        var aScene = EditorSceneManager.OpenScene(AScene, OpenSceneMode.Additive);
        var source = aScene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<StageView>(true)).FirstOrDefault();
        if (!source) throw new Exception("A StageView could not be found");
        var manager = gameScene.GetRootGameObjects().FirstOrDefault(x => x.name == "GameManager");
        if (!manager) throw new Exception("B GameManager could not be found");
        var view = manager.GetComponent<StageView>();
        if (!view) view = manager.AddComponent<StageView>();
        EditorUtility.CopySerialized(source, view);
        view.importedField = true;
        var batGame = manager.GetComponent<BatGame>();
        if (!batGame) batGame = manager.AddComponent<BatGame>();
        batGame.view = view;
        batGame.integratedMode = true;
        batGame.roundDuration = GameSettings.GAME_TIME_LIMIT;
        var gameplay = manager.GetComponent<GamePlayModule>();
        if (!gameplay) throw new Exception("B GamePlayModule could not be found");
        gameplay.SetBattingGame(batGame);
        batGame.integratedGameplay = gameplay;
        var tutorial = manager.GetComponent<TutorialModule>();
        if (tutorial) tutorial.SetSkipTutorialForDebug(false);
        EditorSceneManager.CloseScene(aScene, true);
        SceneManager.SetActiveScene(gameScene);

        FitGameField(gameScene, gameplay);
        RefineGameVisuals(gameScene);
        DisableRoot(gameScene, "XR Origin (XR Rig)");
        DisableRoot(gameScene, "XR Interaction Simulator");
        DisableRoot(gameScene, "Post Process Volume");
        DisableRoot(gameScene, "Interactables");
        DisableRoot(gameScene, "UI");
        DisableRoot(gameScene, "Teleport Area Setup");
        DisableRoot(gameScene, "Teleport Area");
        DisableRoot(gameScene, "Sphere");
        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene);

        var homeScene = EditorSceneManager.OpenScene(HomeScene, OpenSceneMode.Single);
        var home = homeScene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<HomeManager>(true)).FirstOrDefault();
        if (!home) throw new Exception("B HomeManager could not be found");
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Fonts/NotoSansJP-Medium.ttf");
        home.SetDisplayFont(font);
        var result = home.GetComponent<ResultUIManager>();
        if (result) result.SetDisplayFont(font);
        foreach (var root in homeScene.GetRootGameObjects()) {
            if (root.name.Contains("XR Origin")) root.tag = "Player";
        }
        RefineHomeVisuals(homeScene);
        EditorSceneManager.MarkSceneDirty(homeScene);
        EditorSceneManager.SaveScene(homeScene);
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene(HomeScene, true),
            new EditorBuildSettingsScene(GameScene, true)
        };
        AssetDatabase.SaveAssets();
        Debug.Log("INTEGRATION_BUILDER_COMPLETE");
    }

    static void FitGameField(Scene scene, GamePlayModule gameplay)
    {
        var roots = scene.GetRootGameObjects();
        GameObject Find(string name) => roots.FirstOrDefault(x => x.name == name);
        var terrainObject = Find("Terrain");
        if (terrainObject) {
            var terrain = terrainObject.GetComponent<Terrain>();
            if (terrain && terrain.terrainData) {
                var size = terrain.terrainData.size;
                Debug.Log($"OBJECT_SIZE Terrain original={size}");
                terrain.terrainData.size = new Vector3(210, Mathf.Max(10, size.y), 190);
                EditorUtility.SetDirty(terrain.terrainData);
                terrain.transform.position = new Vector3(-105, -.18f, -12);
                Debug.Log($"OBJECT_SIZE Terrain final={terrain.terrainData.size} position={terrain.transform.position}");
            }
        }
        SetBox(Find("Floor"), new Vector3(0, -.018f, 13.5f), new Vector3(16, .01f, 27));
        SetBox(Find("HomeBase"), new Vector3(0, .007f, .42f), new Vector3(.46f, .012f, .46f));
        SetBox(Find("HomeBase2"), new Vector3(0, .007f, -.5f), new Vector3(.46f, .012f, .46f));
        SetBox(Find("WakuLeft"), new Vector3(-8.4f, -.009f, 14), new Vector3(.42f, .012f, 27));
        SetBox(Find("WakuRight"), new Vector3(8.4f, -.009f, 14), new Vector3(.42f, .012f, 27));

        var pitcher = roots.SelectMany(x => x.GetComponentsInChildren<PitcherController>(true))
            .FirstOrDefault(x => x.name == "B Pitcher Center") ??
            roots.SelectMany(x => x.GetComponentsInChildren<PitcherController>(true)).FirstOrDefault();
        if (!pitcher) throw new Exception("B pitcher prefab could not be found");
        var pitchers = new List<PitcherController>();
        var parent = pitcher.transform.parent;
        for (int lane = -1; lane <= 1; lane++) {
            string actorName = lane == 0 ? "B Pitcher Center" : lane < 0 ? "B Pitcher Left" : "B Pitcher Right";
            var actor = scene.GetRootGameObjects().FirstOrDefault(x => x.name == actorName);
            if (!actor) actor = lane == 0 ? pitcher.gameObject : UnityEngine.Object.Instantiate(pitcher.gameObject, parent);
            actor.name = actorName;
            FitCharacter(actor, new Vector3(lane * 3.8f, 0, lane == 0 ? 8.15f : 7.85f));
            var controller = actor.GetComponent<PitcherController>();
            controller.Configure(actor.name, lane);
            pitchers.Add(controller);
        }
        gameplay.SetPitchers(pitchers);

        FitImportedNetWalls(scene);
    }

    static void SetBox(GameObject go, Vector3 position, Vector3 scale)
    {
        if (!go) return;
        Debug.Log($"OBJECT_SIZE {go.name} original={go.transform.localScale} position={go.transform.position}");
        go.transform.position = position;
        go.transform.localScale = scale;
        Debug.Log($"OBJECT_SIZE {go.name} final={go.transform.localScale} position={go.transform.position}");
    }

    static Bounds? RenderBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>(true).Where(r => r.enabled).ToArray();
        if (renderers.Length == 0) return null;
        var bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static void FitCharacter(GameObject go, Vector3 position)
    {
        go.transform.position = position;
        var before = RenderBounds(go);
        if (before.HasValue && before.Value.size.y > .01f) {
            go.transform.localScale *= 1.8f / before.Value.size.y;
            var after = RenderBounds(go);
            if (after.HasValue) go.transform.position += Vector3.up * (.015f - after.Value.min.y);
        }
        Debug.Log($"OBJECT_SIZE {go.name} finalBounds={RenderBounds(go)} position={go.transform.position}");
    }

    [MenuItem("VR Bat/Refit Imported Net Walls")]
    public static void RefitNets()
    {
        var scene = SceneManager.GetActiveScene();
        if (scene.path != GameScene) throw new Exception("Open B GameScene first");
        FitImportedNetWalls(scene);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    static void FitImportedNetWalls(Scene scene)
    {
        var walls = scene.GetRootGameObjects().Where(x =>
            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(x).Contains("/Interactables/Cube.prefab") ||
            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(x).Contains("/OverNet.prefab")).ToArray();
        if (walls.Length != 4) throw new Exception("Expected four B net walls; found " + walls.Length);
        foreach (var wall in walls) {
            var b = RenderBounds(wall);
            if (!b.HasValue) throw new Exception("B net wall has no renderer: " + wall.name);
            if (b.Value.size.z > b.Value.size.x) {
                bool left = b.Value.center.x < 0;
                FitNetPanel(wall, new Vector3(left ? -104.9f : 104.9f, 6.02f, 83f),
                    new Vector3(.2f, 12f, 190f));
                wall.name = left ? "B Net Left" : "B Net Right";
            } else {
                bool back = b.Value.center.z < 0;
                FitNetPanel(wall, new Vector3(0, 6.02f, back ? -11.9f : 177.9f),
                    new Vector3(210f, 12f, .2f));
                wall.name = back ? "B Net Back" : "B Net Outfield";
            }
            foreach (var component in wall.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>(true))
                UnityEngine.Object.DestroyImmediate(component, true);
        }
    }

    static void FitNetPanel(GameObject go, Vector3 targetCenter, Vector3 targetSize)
    {
        var before = RenderBounds(go);
        if (!before.HasValue) return;
        var b = before.Value;
        // The front/back imported prefab is rotated 90 degrees, so local X and Z
        // affect the opposite world axes.
        bool rotated = b.size.x > b.size.z;
        go.transform.localScale = Vector3.Scale(go.transform.localScale,
            rotated
                ? new Vector3(targetSize.z / b.size.z, targetSize.y / b.size.y, targetSize.x / b.size.x)
                : new Vector3(targetSize.x / b.size.x, targetSize.y / b.size.y, targetSize.z / b.size.z));
        var after = RenderBounds(go);
        if (after.HasValue) go.transform.position += targetCenter - after.Value.center;
        Debug.Log($"OBJECT_SIZE {go.name} originalBounds={b} finalBounds={RenderBounds(go)}");
    }

    static void DisableRoot(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects()) {
            if (root.name == name || root.name.StartsWith(name + " (")) {
                root.SetActive(false);
                Debug.Log("OBJECT_DISABLED " + root.name);
            }
        }
    }

    [MenuItem("VR Bat/Refine Imported Field and Lobby")]
    public static void RefineVisuals()
    {
        var gameScene = EditorSceneManager.OpenScene(GameScene, OpenSceneMode.Single);
        RefineGameVisuals(gameScene);
        EditorSceneManager.MarkSceneDirty(gameScene);
        EditorSceneManager.SaveScene(gameScene);
        var homeScene = EditorSceneManager.OpenScene(HomeScene, OpenSceneMode.Single);
        RefineHomeVisuals(homeScene);
        EditorSceneManager.MarkSceneDirty(homeScene);
        EditorSceneManager.SaveScene(homeScene);
        AssetDatabase.SaveAssets();
    }

    static Material TintAsset(string path, string hex)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (!material) throw new Exception("Missing B material " + path);
        var color = ColorUtility.TryParseHtmlString(hex, out var parsed) ? parsed : Color.white;
        material.SetColor("_BaseColor", color);
        material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    static void ShapeInfield(GameObject floor)
    {
        if (!floor) return;
        const string path = "Assets/MyAsetts/IntegratedInfield.asset";
        var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (!mesh) {
            mesh = new Mesh { name = "B Infield Diamond" };
            mesh.vertices = new[] {
                new Vector3(0, 0, .35f), new Vector3(13, 0, 20),
                new Vector3(0, 0, 40), new Vector3(-13, 0, 20)
            };
            mesh.triangles = new[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path);
        }
        floor.GetComponent<MeshFilter>().sharedMesh = mesh;
        floor.transform.position = new Vector3(0, -.012f, 0);
        floor.transform.rotation = Quaternion.identity;
        floor.transform.localScale = Vector3.one;
        var collider = floor.GetComponent<Collider>();
        if (collider) collider.enabled = false;
    }

    static void RefineGameVisuals(Scene scene)
    {
        var roots = scene.GetRootGameObjects();
        var floor = roots.FirstOrDefault(x => x.name == "Floor");
        var left = roots.FirstOrDefault(x => x.name == "WakuLeft");
        var right = roots.FirstOrDefault(x => x.name == "WakuRight");
        SetBox(floor, new Vector3(0, -.018f, 13.5f), new Vector3(16, .01f, 27));
        ShapeInfield(floor);
        SetBox(left, new Vector3(-8.4f, -.009f, 14), new Vector3(.42f, .012f, 27));
        SetBox(right, new Vector3(8.4f, -.009f, 14), new Vector3(.42f, .012f, 27));
        var ground = TintAsset("Assets/MyAsetts/Materials/Ground.mat", "#2B713D");
        TintAsset("Assets/MyAsetts/Materials/FloorBase.mat", "#705944");
        TintAsset("Assets/MyAsetts/Materials/FloorBase 2.mat", "#91714E");
        var white = AssetDatabase.LoadAssetAtPath<Material>("Assets/MyAsetts/Materials/White.mat");
        const string stripePath = "Assets/MyAsetts/Materials/IntegratedGrassStripe.mat";
        var stripe = AssetDatabase.LoadAssetAtPath<Material>(stripePath);
        if (!stripe) {
            stripe = new Material(ground);
            AssetDatabase.CreateAsset(stripe, stripePath);
        }
        stripe.SetColor("_BaseColor", new Color(.16f, .37f, .23f, 1));
        stripe.color = new Color(.16f, .37f, .23f, 1);
        EditorUtility.SetDirty(stripe);

        var old = roots.FirstOrDefault(x => x.name == "B Field Markings");
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var marks = new GameObject("B Field Markings");
        for (int z = 38; z < 178; z += 20)
            ThinBox("Mowed stripe " + z, new Vector3(0, -.169f, z),
                new Vector3(208, .005f, 9), stripe, marks.transform);
        Line("Left foul line", new Vector3(-.35f, .025f, .6f), new Vector3(-78, .025f, 112), .045f, white, marks.transform);
        Line("Right foul line", new Vector3(.35f, .025f, .6f), new Vector3(78, .025f, 112), .045f, white, marks.transform);
        Line("Left batter box", new Vector3(-.8f, .025f, -.5f), new Vector3(-.8f, .025f, 1), .028f, white, marks.transform);
        Line("Right batter box", new Vector3(.8f, .025f, -.5f), new Vector3(.8f, .025f, 1), .028f, white, marks.transform);
        var homeBase = roots.FirstOrDefault(x => x.name == "HomeBase");
        if (homeBase) {
            foreach (var location in new[] { new Vector3(-9, .007f, 19), new Vector3(9, .007f, 19), new Vector3(0, .007f, 38) }) {
                var baseObject = UnityEngine.Object.Instantiate(homeBase, marks.transform);
                baseObject.name = "B Field Base";
                baseObject.transform.position = location;
                baseObject.transform.localScale = new Vector3(.42f, .012f, .42f);
            }
        }
        Debug.Log("OBJECT_ADDED B Field Markings: mowed stripes, foul lines, batter boxes, three bases");
    }

    static void RefineHomeVisuals(Scene scene)
    {
        TintAsset("Assets/MyAsetts/Materials/HomeScene/Wall.mat", "#244257");
        TintAsset("Assets/MyAsetts/Materials/HomeScene/Floor.mat", "#425863");
        TintAsset("Assets/MyAsetts/Materials/HomeScene/TPFloor.mat", "#2E6571");
        var light = scene.GetRootGameObjects().FirstOrDefault(x => x.name == "Light")?.GetComponent<Light>();
        if (light) {
            light.color = new Color(.84f, .92f, 1f, 1f);
            light.intensity = 1.4f;
        }
        var old = scene.GetRootGameObjects().FirstOrDefault(x => x.name == "B Lobby Accents");
        if (old) UnityEngine.Object.DestroyImmediate(old);
        var accents = new GameObject("B Lobby Accents");
        var white = AssetDatabase.LoadAssetAtPath<Material>("Assets/MyAsetts/Materials/White.mat");
        const string accentPath = "Assets/MyAsetts/Materials/IntegratedHomeAccent.mat";
        var accent = AssetDatabase.LoadAssetAtPath<Material>(accentPath);
        if (!accent) {
            accent = new Material(white);
            AssetDatabase.CreateAsset(accent, accentPath);
        }
        accent.SetColor("_BaseColor", new Color(.13f, .76f, .83f, 1f));
        accent.color = new Color(.13f, .76f, .83f, 1f);
        EditorUtility.SetDirty(accent);
        ThinBox("Entry header light", new Vector3(0, 4.2f, 9.94f), new Vector3(12, .065f, .05f), accent, accents.transform);
        ThinBox("Entry left light", new Vector3(-6, 2.7f, 9.94f), new Vector3(.065f, 3, .05f), accent, accents.transform);
        ThinBox("Entry right light", new Vector3(6, 2.7f, 9.94f), new Vector3(.065f, 3, .05f), accent, accents.transform);
        Debug.Log("OBJECT_ADDED B Lobby Accents: three wall-mounted light strips");
    }

    static GameObject ThinBox(string name, Vector3 position, Vector3 size, Material material, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = size;
        go.GetComponent<Renderer>().sharedMaterial = material;
        go.GetComponent<Collider>().enabled = false;
        return go;
    }

    static void Line(string name, Vector3 from, Vector3 to, float width, Material material, Transform parent)
    {
        var go = ThinBox(name, (from + to) * .5f,
            new Vector3(width, .005f, Vector3.Distance(from, to)), material, parent);
        go.transform.rotation = Quaternion.FromToRotation(Vector3.forward, (to - from).normalized);
    }
}
