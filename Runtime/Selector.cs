using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor
{
	public class SelectMeshByName : EditorWindow
	{
		string meshName = "";

		[MenuItem("Utils/Selector Window/Select Mesh By Name")]
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
#endif
}
