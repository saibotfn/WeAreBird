#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

namespace AlderBird.Editor
{
    /// <summary>Builds genuine native Unity assets from the delivered FBX.</summary>
    public static class BirdSetup
    {
        private static readonly string[] Names = {
            "Bird_Idle", "Bird_Flap", "Bird_Glide", "Bird_Takeoff",
            "Bird_Land", "Bird_Hop", "Bird_LookAround", "Bird_WingFold"
        };
        private static readonly HashSet<string> Loops = new HashSet<string> {
            "Bird_Idle", "Bird_Flap", "Bird_Glide", "Bird_LookAround"
        };

        private static string RootFolder()
        {
            foreach (string guid in AssetDatabase.FindAssets("BirdSetup t:MonoScript"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith("/Scripts/Editor/BirdSetup.cs", StringComparison.Ordinal)) continue;
                string root = path.Substring(0, path.Length - "/Scripts/Editor/BirdSetup.cs".Length);
                if (AssetDatabase.LoadAssetAtPath<GameObject>(root + "/Bird.fbx") != null) return root;
            }
            throw new InvalidOperationException("Copy the complete Unity folder into Assets/Unity_VR_Bird, preserving its Scripts and Textures folders.");
        }

        private static string ClipName(string source)
        {
            return Names.FirstOrDefault(n => source.EndsWith(n, StringComparison.Ordinal));
        }

        private static void ConfigureModel(string path, string singleClip = null)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null) throw new InvalidOperationException("Missing model: " + path);
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            importer.motionNodeName = string.Empty;
            importer.importAnimation = true;
            importer.globalScale = 1f;
            importer.useFileScale = true;
            importer.useFileUnits = true;
            importer.bakeAxisConversion = true;
            importer.importCameras = false;
            importer.importLights = false;
            importer.importBlendShapes = false;
            importer.importConstraints = false;
            importer.optimizeGameObjects = false; // Head remains available for optional aim.
            importer.optimizeBones = false;       // Preserve the complete shared hierarchy.
            importer.preserveHierarchy = true;
            importer.animationCompression = ModelImporterAnimationCompression.Off;
            importer.resampleCurves = true;
            importer.skinWeights = ModelImporterSkinWeights.Custom;
            importer.maxBonesPerVertex = 4;
            importer.minBoneWeight = 0.0001f;
            importer.meshCompression = ModelImporterMeshCompression.Off;
            importer.importNormals = ModelImporterNormals.Import;
            importer.importTangents = ModelImporterTangents.None;
            importer.materialImportMode = ModelImporterMaterialImportMode.None;
            importer.SaveAndReimport();
            string rootPath = importer.transformPaths.FirstOrDefault(p => p == "Root" || p.EndsWith("/Root", StringComparison.Ordinal));
            if (rootPath == null) throw new InvalidOperationException("Could not resolve the imported Root transform path.");
            importer.motionNodeName = rootPath;
            var clips = importer.defaultClipAnimations;
            foreach (var clip in clips)
            {
                string name = singleClip ?? ClipName(clip.name);
                if (name == null) continue;
                clip.name = name;
                clip.loopTime = Loops.Contains(name);
                clip.loopPose = false; // Source endpoint poses already agree exactly.
                clip.lockRootRotation = true;
                clip.lockRootHeightY = true;
                clip.lockRootPositionXZ = true;
                clip.keepOriginalOrientation = true;
                clip.keepOriginalPositionY = true;
                clip.keepOriginalPositionXZ = true;
            }
            importer.clipAnimations = clips;
            importer.SaveAndReimport();
        }

        private static Dictionary<string, AnimationClip> LoadClips(string modelPath)
        {
            var result = new Dictionary<string, AnimationClip>();
            foreach (var clip in AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>())
            {
                if (clip.name.StartsWith("__preview__", StringComparison.Ordinal)) continue;
                string key = ClipName(clip.name);
                if (key != null) result[key] = clip;
            }
            return result;
        }

        private static Transform Bone(GameObject model, string name)
        {
            Transform result = model.GetComponentsInChildren<Transform>(true).FirstOrDefault(t => t.name == name);
            if (result == null) throw new InvalidOperationException("Imported skeleton is missing " + name);
            return result;
        }

        [MenuItem("Tools/Alder Bird/Create Bird Prefab")]
        public static void Build()
        {
            GameObject wrapper = null;
            try
            {
                string root = RootFolder();
                string modelPath = root + "/Bird.fbx";
                EditorUtility.DisplayProgressBar("Alder Bird", "Importing Generic rig and animation clips", .1f);
                ConfigureModel(modelPath);
                var clips = LoadClips(modelPath);
                // Only use the redundant files if the primary import did not expose a take.
                foreach (string name in Names.Where(n => !clips.ContainsKey(n)))
                {
                    string path = root + "/Animations/" + name + ".fbx";
                    ConfigureModel(path, name);
                    foreach (var pair in LoadClips(path)) clips[pair.Key] = pair.Value;
                }
                if (Names.Any(n => !clips.ContainsKey(n))) throw new InvalidOperationException("One or more required clips could not be imported.");
                string output = root + "/Generated";
                if (!AssetDatabase.IsValidFolder(output)) AssetDatabase.CreateFolder(root, "Generated");
                Material material = MakeMaterial(root, output);
                var controller = MakeController(output, clips);
                var modelAsset = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
                wrapper = new GameObject("Bird");
                var orientation = new GameObject("Orientation");
                orientation.transform.SetParent(wrapper.transform, false);
                var cameraObject = new GameObject("BirdCamera");
                cameraObject.transform.SetParent(wrapper.transform, false);
                cameraObject.transform.localPosition = new Vector3(0f, 1.5f, -4f);
                cameraObject.transform.localRotation = Quaternion.Euler(10f, 0f, 0f);
                cameraObject.AddComponent<Camera>();
                var model = (GameObject)PrefabUtility.InstantiatePrefab(modelAsset);
                model.transform.SetParent(orientation.transform, false);
                model.name = "Bird_Model";
                // Measure the actual imported bind pose before sampling any clip.
                string basisReport = NormalizeImportedBasis(model, orientation.transform);
                var animator = model.GetComponent<Animator>() ?? model.AddComponent<Animator>();
                if (animator.avatar == null || !animator.avatar.isValid)
                    throw new InvalidOperationException("Unity did not create a valid Generic Avatar.");
                animator.runtimeAnimatorController = controller;
                animator.applyRootMotion = false;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    renderer.sharedMaterials = new[] { material };
                    renderer.quality = SkinQuality.Bone4;
                    renderer.shadowCastingMode = ShadowCastingMode.On;
                    renderer.receiveShadows = true;
                    renderer.updateWhenOffscreen = false;
                }
                EditorUtility.DisplayProgressBar("Alder Bird", "Checking clips, bounds and root drift", .65f);
                string validation = ValidateAndSetBounds(model, clips);
                clips["Bird_Idle"].SampleAnimation(model, 0f);
                float groundOffset = GroundIdle(model, orientation.transform);
                validation += "Idle ground correction: " + groundOffset.ToString("F6") + " m.\n";
                var head = Bone(model, "Head");
                var behaviour = wrapper.AddComponent<BirdController>();
                behaviour.Configure(animator, head, head.InverseTransformDirection(wrapper.transform.forward));
                wrapper.AddComponent<BirdFlightController>();
                foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
                    PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
                foreach (var transform in model.GetComponentsInChildren<Transform>(true))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(transform);
                PrefabUtility.RecordPrefabInstancePropertyModifications(animator);
                // Rerunning creates numbered copies and never overwrites edited prefabs/controllers.
                string prefabPath = AssetDatabase.GenerateUniqueAssetPath(output + "/Bird.prefab");
                GameObject prefab = PrefabUtility.SaveAsPrefabAsset(wrapper, prefabPath);
                string reportPath = AssetDatabase.GenerateUniqueAssetPath(output + "/Unity_Import_Report.txt");
                File.WriteAllText(reportPath, "ALDER BIRD / UNITY IMPORT CHECK\nUnity " + Application.unityVersion +
                    "\n" + basisReport + "\n" + validation + "\nPrefab: " + prefabPath +
                    "\nQuest frame time and visual quality still require testing in your game.\n");
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                Selection.activeObject = prefab;
                EditorGUIUtility.PingObject(prefab);
                Debug.Log("Alder Bird is ready: " + prefabPath + "\n" + basisReport + "\n" + validation, prefab);
            }
            catch (Exception e) { Debug.LogException(e); }
            finally
            {
                if (wrapper != null) Object.DestroyImmediate(wrapper);
                EditorUtility.ClearProgressBar();
            }
        }

        private static Material MakeMaterial(string root, string output)
        {
            string texturePath = root + "/Textures/Bird_Albedo.png";
            var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
            if (importer == null) throw new InvalidOperationException("Missing Bird_Albedo.png");
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.None;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 1024;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings {
                name = "Android", overridden = true, maxTextureSize = 1024,
                format = TextureImporterFormat.ASTC_6x6, compressionQuality = 50
            });
            importer.SaveAndReimport();
            var pipeline = GraphicsSettings.currentRenderPipeline;
            if (pipeline != null && pipeline.GetType().Name.IndexOf("Universal", StringComparison.OrdinalIgnoreCase) < 0)
                throw new InvalidOperationException("The supplied material setup supports URP or the Built-in Render Pipeline.");
            Shader shader = Shader.Find(pipeline != null ? "Universal Render Pipeline/Lit" : "Standard");
            if (shader == null) throw new InvalidOperationException("Use a URP or Built-in Render Pipeline project.");
            var material = new Material(shader) { name = "Bird_Material" };
            Texture texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
            if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
            if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", 0f);
            if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", .22f);
            if (material.HasProperty("_Glossiness")) material.SetFloat("_Glossiness", .22f);
            material.enableInstancing = false;
            AssetDatabase.CreateAsset(material, AssetDatabase.GenerateUniqueAssetPath(output + "/Bird_Material.mat"));
            return material;
        }

        private static AnimatorController MakeController(string output, Dictionary<string, AnimationClip> clips)
        {
            string path = AssetDatabase.GenerateUniqueAssetPath(output + "/Bird_Animator.controller");
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            foreach (string trigger in new[] { "Idle", "Takeoff", "Land", "Hop", "LookAround", "WingFold" })
                controller.AddParameter(trigger, AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Gliding", AnimatorControllerParameterType.Bool);
            AnimatorStateMachine machine = controller.layers[0].stateMachine;
            var states = new Dictionary<string, AnimatorState>();
            for (int i = 0; i < Names.Length; i++)
            {
                var state = machine.AddState(Names[i], new Vector3(300 * (i % 3), 85 * (i / 3), 0));
                state.motion = clips[Names[i]];
                state.writeDefaultValues = false;
                states[Names[i]] = state;
            }
            machine.defaultState = states["Bird_Idle"];
            foreach (string trigger in new[] { "Idle", "Takeoff", "Land", "Hop", "LookAround", "WingFold" })
            {
                var transition = machine.AddAnyStateTransition(states["Bird_" + trigger]);
                transition.AddCondition(AnimatorConditionMode.If, 0, trigger);
                transition.hasExitTime = false;
                transition.hasFixedDuration = true;
                transition.duration = .12f;
                transition.canTransitionToSelf = false;
            }
            Exit(states["Bird_Takeoff"], states["Bird_Flap"]);
            Exit(states["Bird_Land"], states["Bird_Idle"]);
            Exit(states["Bird_Hop"], states["Bird_Idle"]);
            Exit(states["Bird_WingFold"], states["Bird_Idle"]);
            var glide = states["Bird_Flap"].AddTransition(states["Bird_Glide"]);
            glide.AddCondition(AnimatorConditionMode.If, 0, "Gliding");
            glide.hasExitTime = false; glide.hasFixedDuration = true; glide.duration = .2f;
            var flap = states["Bird_Glide"].AddTransition(states["Bird_Flap"]);
            flap.AddCondition(AnimatorConditionMode.IfNot, 0, "Gliding");
            flap.hasExitTime = false; flap.hasFixedDuration = true; flap.duration = .2f;
            return controller;
        }

        private static void Exit(AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = true; transition.exitTime = 1f;
            transition.hasFixedDuration = true; transition.duration = .12f;
        }

        private static string NormalizeImportedBasis(GameObject model, Transform orientation)
        {
            Vector3 origin = Bone(model, "Root").position;
            Vector3 left = Bone(model, "Shoulder_L").position;
            Vector3 right = Bone(model, "Shoulder_R").position;
            Vector3 a = (left + right) * .5f - origin;
            Vector3 b = Bone(model, "TailBase").position - origin;
            // Known bind-pose coordinates reconstruct the source Y/Z directions
            // without assuming which FBX axis conversion Unity applied.
            const float determinant = .105f * .231f - .015f * .176f;
            Vector3 up = (.105f * a - .015f * b) / determinant;
            Vector3 forward = -(.231f * b - .176f * a) / determinant;
            float width = Vector3.Distance(left, right);
            if (width < .000001f || up.sqrMagnitude < .000001f || forward.sqrMagnitude < .000001f)
                throw new InvalidOperationException("Invalid imported bind-pose landmarks.");
            Quaternion basis = Quaternion.LookRotation(forward.normalized, up.normalized);
            float scale = .122f / width;
            orientation.localRotation = Quaternion.Inverse(basis);
            orientation.localScale = Vector3.one * scale;
            orientation.position -= Bone(model, "Root").position;
            Vector3 actualUp = orientation.TransformDirection(up.normalized);
            Vector3 actualForward = orientation.TransformDirection(forward.normalized);
            float importedWidth = Vector3.Distance(Bone(model, "Shoulder_L").position, Bone(model, "Shoulder_R").position);
            if (Vector3.Dot(actualUp, Vector3.up) < .999f || Vector3.Dot(actualForward, Vector3.forward) < .999f || Mathf.Abs(importedWidth - .122f) > .0001f)
                throw new InvalidOperationException("Imported orientation or scale could not be normalized.");
            return "Measured bind pose: Y up, +Z forward, meters.\nOrientation correction: " +
                orientation.localEulerAngles.ToString("F4") + "; scale correction: " + scale.ToString("F6") +
                ".\nShoulder spacing after correction: " + importedWidth.ToString("F6") + " m.";
        }

        private static string ValidateAndSetBounds(GameObject model, Dictionary<string, AnimationClip> clips)
        {
            var report = new StringBuilder();
            var renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length != 1) throw new InvalidOperationException("Expected one skinned renderer.");
            int triangles = renderers.Sum(r => (int)r.sharedMesh.GetIndexCount(0) / 3);
            if (triangles > 8000 || triangles < 4000) throw new InvalidOperationException("Unexpected triangle count: " + triangles);
            int bones = renderers[0].bones.Length;
            report.AppendLine("Triangles: " + triangles + "; skinned renderers: " + renderers.Length + "; skin bone references: " + bones);
            var bounds = new Dictionary<SkinnedMeshRenderer, Bounds>();
            var started = new HashSet<SkinnedMeshRenderer>();
            var baked = new Mesh();
            float rootDrift = 0f;
            try
            {
                foreach (var pair in clips)
                {
                    AnimationClip clip = pair.Value;
                    if (clip.length <= 0 || !Mathf.Approximately(clip.frameRate, 30f))
                        throw new InvalidOperationException("Invalid clip duration/frame rate: " + pair.Key);
                    Vector3 initial = Vector3.zero;
                    for (int f = 0; f <= 12; f++)
                    {
                        clip.SampleAnimation(model, clip.length * f / 12f);
                        Vector3 position = model.transform.InverseTransformPoint(Bone(model, "Root").position);
                        if (f == 0) initial = position;
                        rootDrift = Mathf.Max(rootDrift, Vector3.Distance(initial, position));
                        foreach (var renderer in renderers)
                        {
                            renderer.BakeMesh(baked);
                            baked.RecalculateBounds();
                            if (started.Add(renderer)) bounds[renderer] = baked.bounds;
                            else
                            {
                                Bounds b = bounds[renderer];
                                b.Encapsulate(baked.bounds.min); b.Encapsulate(baked.bounds.max);
                                bounds[renderer] = b;
                            }
                        }
                    }
                    report.AppendLine(pair.Key + ": " + clip.length.ToString("F3") + " s, " + clip.frameRate + " fps");
                }
                if (rootDrift > .0001f) throw new InvalidOperationException("Unexpected animated root movement: " + rootDrift);
                foreach (var renderer in renderers)
                {
                    Bounds b = bounds[renderer]; b.Expand(.05f); renderer.localBounds = b;
                }
                report.AppendLine("Root drift: " + rootDrift.ToString("F8") + ". Clip bounds sampled; culling bounds expanded.");
            }
            finally { Object.DestroyImmediate(baked); }
            return report.ToString();
        }

        private static float GroundIdle(GameObject model, Transform orientation)
        {
            var mesh = new Mesh();
            float minimum = float.PositiveInfinity;
            try
            {
                foreach (var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    renderer.BakeMesh(mesh);
                    foreach (Vector3 vertex in mesh.vertices)
                    {
                        Vector3 world = renderer.transform.TransformPoint(vertex);
                        minimum = Mathf.Min(minimum, orientation.parent.InverseTransformPoint(world).y);
                    }
                }
            }
            finally { Object.DestroyImmediate(mesh); }
            if (float.IsInfinity(minimum)) throw new InvalidOperationException("Could not measure the idle foot position.");
            orientation.localPosition -= Vector3.up * minimum;
            return minimum;
        }
    }

    [CustomEditor(typeof(BirdController))]
    public sealed class BirdControllerInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation controls", EditorStyles.boldLabel);
            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                var bird = (BirdController)target;
                if (GUILayout.Button("Idle")) bird.Idle();
                if (GUILayout.Button("Take off")) bird.TakeOff();
                if (GUILayout.Button("Glide")) bird.SetGliding(true);
                if (GUILayout.Button("Flap")) bird.SetGliding(false);
                if (GUILayout.Button("Land")) bird.Land();
                if (GUILayout.Button("Hop")) bird.Hop();
                if (GUILayout.Button("Look around")) bird.LookAround();
                if (GUILayout.Button("Fold wings")) bird.FoldWings();
            }
            if (!Application.isPlaying) EditorGUILayout.HelpBox("Enter Play mode to use these buttons. Motion through the scene is controlled by your own script.", MessageType.Info);
        }
    }
}
#endif
