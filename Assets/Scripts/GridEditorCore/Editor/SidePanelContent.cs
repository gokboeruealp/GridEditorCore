using System;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Borboerue
{
    public class SidePanelContent : EditorWindow
    {
        private Vector2 scrollPosition;

        private GUIStyle[] nodeStyles;

        private void OnEnable()
        {
            nodeStyles = new GUIStyle[Enum.GetValues(typeof(ETileType)).Length];

            for (int i = 0; i < nodeStyles.Length; i++)
            {
                nodeStyles[i] = new GUIStyle();
                nodeStyles[i].normal.background = EditorGUIUtility.Load($"builtin skins/darkskin/images/node{i + 1}.png") as Texture2D;
            }
        }

        public void Draw()
        {
            GUILayout.Label("Grids", EditorStyles.boldLabel);

            string gridsPath = "Assets/Scripts/GridEditorCore/GridData";
            var files = System.IO.Directory.GetFiles(gridsPath, "*.json");
            files = files.OrderBy(f => new System.IO.FileInfo(f).CreationTime).ToArray();

            var NodeSize = 40;

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                foreach (string file in files)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    string gridName = System.IO.Path.GetFileNameWithoutExtension(file);
                    GUILayout.Label(gridName, EditorStyles.boldLabel);

                    // TODO: Added Foldout

                    string jsonMatrix = System.IO.File.ReadAllText(file);
                    GridData gridData = JsonConvert.DeserializeObject<GridData>(jsonMatrix);
                    GridEditor editorInstance = GridEditor.Instance;

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("Load"))
                    {
                        editorInstance.LoadGrid(gridData);
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(50)))
                    {
                        if (EditorUtility.DisplayDialog("Delete File", "Are you sure you want to delete this file?", "Yes", "No"))
                        {
                            System.IO.File.Delete(file);
                            AssetDatabase.Refresh();
                        }
                    }
                    GUILayout.EndHorizontal();

                    for (int y = 0; y < gridData.Matrix.GetLength(1); y++)
                    {
                        GUILayout.BeginHorizontal();

                        for (int x = 0; x < gridData.Matrix.GetLength(0); x++)
                        {
                            ETileType currentStyleType = gridData.Matrix[x, y];
                            GUIStyle currentStyle = nodeStyles[(int)currentStyleType];
                            currentStyle.alignment = TextAnchor.MiddleCenter;

                            GUILayout.Box(currentStyleType.ToString()[0].ToString(), currentStyle, GUILayout.Width(NodeSize), GUILayout.Height(NodeSize));
                        }

                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
            }
        }
    }
}