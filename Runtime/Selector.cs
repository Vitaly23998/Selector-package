using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	public class TextureMaterialExporter : MonoBehaviour
{
	public Material materialToSave; // Assign the material you want to save in the Inspector
		public Terrain terrain;
		public string savePath = "Assets/SavedSplatmap.tga";

		public Texture2D colorTexture; // Ваша основная текстура
		public Texture2D alphaTexture; // Черно-белая текстура для альфа-канала
		public void SaveMaterial()
	{
		if (materialToSave == null)
		{
			Debug.LogWarning("Please assign a material to the 'materialToSave' field in the inspector.");
			return;
		}

		// Save the material itself
		string materialPath = "Assets/" + materialToSave.name + ".mat";
#if UNITY_EDITOR
			UnityEditor.AssetDatabase.CreateAsset(materialToSave, materialPath);

		// Refresh the AssetDatabase to ensure the new assets are recognized
		UnityEditor.AssetDatabase.Refresh();
#endif
			Debug.Log("Material and Texture saved to project folder.");
	}

		public void CreateTexture()
		{
			// Создаем новую текстуру с таким же размером, как и у основной текстуры
			Texture2D outputTexture = new Texture2D(colorTexture.width, colorTexture.height);

			// Проходим по каждому пикселю основной текстуры
			for (int y = 0; y < outputTexture.height; y++)
			{
				for (int x = 0; x < outputTexture.width; x++)
				{
					// Берем цвет из основной текстуры
					Color color = colorTexture.GetPixel(x, y);
					// Берем значение альфа-канала из черно-белой текстуры
					float alpha = alphaTexture.GetPixel(x, y).grayscale;

					// Устанавливаем новый цвет с измененным альфа-каналом
					outputTexture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
				}
			}
			outputTexture.Apply();

			// Сохраняем результат в PNG
			System.IO.File.WriteAllBytes(Application.dataPath + "/CombinedTexture.png", outputTexture.EncodeToPNG());

			// Уничтожаем временную текстуру
			Destroy(outputTexture);
		}

		public void SaveTexture()
		{
			TerrainData terrainData = terrain.terrainData;
			int splatmapResolution = terrainData.alphamapResolution;
			int heightmapResolution = terrainData.heightmapResolution;

			// Create a new Texture2D to store the splatmap data
			Texture2D splatmapTexture = new Texture2D(splatmapResolution, splatmapResolution, TextureFormat.RGBA32, false);

			// Read splatmap data and fill the Texture2D
			float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, splatmapResolution, splatmapResolution);
			Color[] colors = new Color[splatmapResolution * splatmapResolution];

			for (int y = 0; y < splatmapResolution; y++)
			{
				for (int x = 0; x < splatmapResolution; x++)
				{
					// You may need to adjust the mapping depending on your terrain's textures
					Color pixelColor = new Color(splatmapData[x, y, 0], splatmapData[x, y, 1], splatmapData[x, y, 2], splatmapData[x, y, 3]);
					colors[y * splatmapResolution + x] = pixelColor;
				}
			}

			// Flip the texture vertically
			colors = FlipTextureVertical(colors, splatmapResolution);

			// Rotate the texture by -90 degrees
			colors = RotateTexture(colors, splatmapResolution);

			splatmapTexture.SetPixels(colors);
			splatmapTexture.Apply();

			// Save the Texture2D as an asset
			byte[] bytes = splatmapTexture.EncodeToTGA();
			System.IO.File.WriteAllBytes(savePath, bytes);

			// Optional: Refresh the Asset Database to see the saved texture in the Unity Editor
#if UNITY_EDITOR
			UnityEditor.AssetDatabase.Refresh();
#endif
		}

		private Color[] FlipTextureVertical(Color[] colors, int resolution)
		{
			Color[] flippedColors = new Color[colors.Length];

			for (int y = 0; y < resolution; y++)
			{
				for (int x = 0; x < resolution; x++)
				{
					flippedColors[x + (resolution - y - 1) * resolution] = colors[x + y * resolution];
				}
			}

			return flippedColors;
		}

		private Color[] RotateTexture(Color[] colors, int resolution)
		{
			Color[] rotatedColors = new Color[colors.Length];

			for (int y = 0; y < resolution; y++)
			{
				for (int x = 0; x < resolution; x++)
				{
					rotatedColors[(resolution - y - 1) + x * resolution] = colors[x + y * resolution];
				}
			}

			return rotatedColors;
		}
	}
#if UNITY_EDITOR
	[CustomEditor(typeof(TextureMaterialExporter))]
	public class TextureMaterialExporterEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			TextureMaterialExporter exporter = (TextureMaterialExporter) target;

			if (GUILayout.Button("Export Material"))
			{
				exporter.SaveMaterial();
			}
			if (GUILayout.Button("Export Texture"))
			{
				exporter.SaveTexture();
			}
						if (GUILayout.Button("CreateTexture"))
			{
				exporter.CreateTexture();
			}
		}
	}

[ExecuteInEditMode]
	public class AutoUVUnwrapping : MonoBehaviour
	{
		[MenuItem("Utils/Generate Lightmap UVs for Selection")]
		public static void GenerateLightmapUVs()
		{
			GameObject[] selectedObjects = Selection.gameObjects;
			foreach (GameObject selectedObject in selectedObjects)
			{
				MeshFilter meshFilter = selectedObject.GetComponent<MeshFilter>();
				if (meshFilter != null)
				{
					Unwrapping.GenerateSecondaryUVSet(meshFilter.sharedMesh);
				}
			}
		}
	}

	public class SelectMeshByName : EditorWindow
	{
		string meshName = "";

		[MenuItem("Utils/Select Mesh By Name")]
		public static void ShowWindow()
		{
			GetWindow<SelectMeshByName>(false, "Select Meshes");
		}

		void OnGUI()
		{
			GUILayout.Label("Enter part of the Mesh name", EditorStyles.boldLabel);
			meshName = EditorGUILayout.TextField("Mesh Name", meshName);

			if (GUILayout.Button("Select Meshes"))
			{
				SelectMeshes();
			}

			if (GUILayout.Button("Get Mesh Name from Selected Object"))
			{
				GetMeshNameFromSelectedObject();
			}
		}

		void SelectMeshes()
		{
			List<GameObject> selectedObjects = new List<GameObject>();
			MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();

			foreach (MeshFilter meshFilter in meshFilters)
			{
				if (meshFilter.sharedMesh != null && meshFilter.sharedMesh.name.Contains(meshName))
				{
					selectedObjects.Add(meshFilter.gameObject);
				}
			}

			Selection.objects = selectedObjects.ToArray();
		}

		void GetMeshNameFromSelectedObject()
		{
			if (Selection.activeGameObject != null)
			{
				MeshFilter meshFilter = Selection.activeGameObject.GetComponent<MeshFilter>();
				if (meshFilter != null && meshFilter.sharedMesh != null)
				{
					meshName = meshFilter.sharedMesh.name;
					Repaint();
				}
			}
		}
	}

	public class AutoFocusSettings : EditorWindow
	{
		private static bool autoFocusEnabled = false;

		[MenuItem("ReactionGames/Utils/Auto Focus Settings")]
		public static void ShowWindow()
		{
			GetWindow<AutoFocusSettings>(false, "Auto Focus Settings", true);
		}

		private void OnGUI()
		{
			GUILayout.Label("Auto Focus Settings", EditorStyles.boldLabel);
			autoFocusEnabled = EditorGUILayout.Toggle("Enable Auto Focus", autoFocusEnabled);
		}

		[InitializeOnLoadMethod]
		private static void Init()
		{
			Selection.selectionChanged -= OnSelectionChanged; // Prevent multiple subscriptions
			Selection.selectionChanged += OnSelectionChanged;
		}

		private static void OnSelectionChanged()
		{
			if (autoFocusEnabled && Selection.activeGameObject != null)
			{
				SceneView.lastActiveSceneView.FrameSelected();
			}
		}
	}
#endif
}
