using System;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

namespace Kaiko
{
    public class SidePanelContent : EditorWindow
    {
        string gridsPath = "Assets/Scripts/WorldGenerator/GridData";
        string[] files;
        int NodeSize = 40;

        private Vector2 scrollPosition;
        private bool[] foldouts; // An array to track foldout states for each grid.

        private GUIStyle[] nodeStyles;

        private void OnEnable()
        {
            files = System.IO.Directory.GetFiles(gridsPath, "*.json");
            files = files.OrderBy(f => new System.IO.FileInfo(f).CreationTime).ToArray();

            nodeStyles = new GUIStyle[Enum.GetValues(typeof(ETileType)).Length];
            foldouts = new bool[files.Length];

            for (int i = 0; i < nodeStyles.Length; i++)
            {
                nodeStyles[i] = new GUIStyle();
                nodeStyles[i].normal.background = EditorGUIUtility.Load($"builtin skins/darkskin/images/node{i + 1}.png") as Texture2D;
            }
        }

        public void Draw()
        {
            GUILayout.Label("Grids", EditorStyles.boldLabel);

            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollView.scrollPosition;

                for (int fileIndex = 0; fileIndex < files.Length; fileIndex++)
                {
                    GUILayout.BeginVertical(EditorStyles.helpBox);

                    string file = files[fileIndex];
                    string gridName = System.IO.Path.GetFileNameWithoutExtension(file);

                    // Use foldout for each grid.
                    foldouts[fileIndex] = EditorGUILayout.Foldout(foldouts[fileIndex], gridName, true, EditorStyles.boldLabel);

                    if (foldouts[fileIndex])
                    {
                        // Content of the grid when it's expanded.
                        string jsonMatrix = System.IO.File.ReadAllText(file);
                        GridData gridData = JsonConvert.DeserializeObject<GridData>(jsonMatrix);
                        WorldGeneratorEditor editorInstance = WorldGeneratorEditor.Instance;

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
                    }

                    GUILayout.EndVertical();
                }
            }
        }
    }
}
