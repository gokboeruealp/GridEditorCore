using System;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Borboerue
{
    public class GridEditor : EditorWindow
    {
        private const float NodeSize = 75;

        private const int gridSizeX = 12;
        private const int gridSizeY = 12; 
        private Vector2 panOffset = Vector2.zero;

        private GUIStyle[] nodeStyles;
        private ETileType[,] nodeStyleTypes;

        private bool isPanning = false;
        private Vector2 panStart;
        private Vector2 drag;

        private SidePanelContent sidePanelContent;
        private SidePanelContent _sidePanelContent;

        public static GridEditor Instance { get { return GetWindow<GridEditor>(); } }
        public void LoadGrid(GridData gridData)
        {
            nodeStyleTypes = new ETileType[gridSizeX, gridSizeY];

            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    nodeStyleTypes[x, y] = gridData.Matrix[x, y];
                }
            }

            Repaint();
        }

        [MenuItem("Tools/Grid Editor")]
        public static void ShowWindow()
        {
            GridEditor window = GetWindow<GridEditor>("Grid Editor");
        }

        private void OnEnable()
        {
            nodeStyleTypes = new ETileType[gridSizeX, gridSizeY];
            nodeStyles = new GUIStyle[Enum.GetValues(typeof(ETileType)).Length];

            _sidePanelContent = new SidePanelContent();

            for (int i = 0; i < nodeStyles.Length; i++)
            {
                nodeStyles[i] = new GUIStyle();
                nodeStyles[i].normal.background = EditorGUIUtility.Load($"builtin skins/darkskin/images/node{i + 1}.png") as Texture2D;
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            InitializeEditorWindow();
            HandleInputEvents();

            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical("box", GUILayout.Width(515));

            if (sidePanelContent != null)
            {
                sidePanelContent.Draw();
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
            GenerateGridNode();
        }

        private void InitializeEditorWindow()
        {
            GUILayoutOption[] options = { GUILayout.Height(20), GUILayout.Width(40) };

            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(40 * 3));
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_Refresh@2x"), options))
            {
                OnClickRefresh();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_TreeEditor.Trash"), options))
            {
                OnClickTrash();
            }

            if (GUILayout.Button(EditorGUIUtility.IconContent("d_SaveAs@2x"), options))
            {
                OnClickSave();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(sidePanelContent != null ? ">" : "<", options))
            {
                sidePanelContent = sidePanelContent == null ? _sidePanelContent : null;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnClickRefresh()
        {
            Vector2 totalNodePosition = Vector2.zero;
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    totalNodePosition += new Vector2(x * NodeSize, y * NodeSize);
                }
            }

            Vector2 averageNodePosition = totalNodePosition / (gridSizeX * gridSizeY);
            panOffset = -averageNodePosition + new Vector2(position.width, position.height) * 0.5f;
            panOffset.x = Mathf.Clamp(panOffset.x, -gridSizeX * NodeSize + position.width, 0) / 2;
            panOffset.y = Mathf.Clamp(panOffset.y, -gridSizeY * NodeSize + position.height, 0) / 2;
        }

        private void OnClickTrash()
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    nodeStyleTypes[x, y] = ETileType.Zero;
                }
            }
        }

        private void OnClickSave()
        {
            var data = new ETileType[gridSizeX, gridSizeY];

            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    data[x, y] = nodeStyleTypes[x, y];
                }
            }

            GridData gridData = new GridData { Matrix = data };
            string jsonMatrix = JsonConvert.SerializeObject(gridData);
            string path = EditorUtility.SaveFilePanelInProject("Save Grid Data", "GridData", "json", "Please enter a file name to save the grid data to", "Assets/Scripts/GridEditorCore/GridData");
            if (path.Length != 0)
            {
                System.IO.File.WriteAllText(path, jsonMatrix);
                AssetDatabase.Refresh();
            }
        }

        private void HandleInputEvents()
        {
            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 2) // Middle mouse button
            {
                isPanning = true;
                panStart = e.mousePosition;
                e.Use();
            }

            if (isPanning && e.type == EventType.MouseDrag)
            {
                panOffset += e.mousePosition - panStart;
                panStart = e.mousePosition;
                Repaint();
            }

            if (e.rawType == EventType.MouseUp && e.button == 2)
            {
                isPanning = false;
            }
        }

        private void GenerateGridNode()
        {
            Event e = Event.current;
            Vector2 mousePos = e.mousePosition;

            GUILayoutUtility.GetRect(gridSizeX * NodeSize, gridSizeY * NodeSize);

            Handles.BeginGUI();

            for (int y = 0; y < gridSizeY; y++)
            {
                for (int x = 0; x < gridSizeX; x++)
                {
                    Rect nodeRect = new Rect(x * NodeSize, y * NodeSize, NodeSize, NodeSize);
                    nodeRect.position += panOffset;

                    ETileType currentStyleType = nodeStyleTypes[x, y];
                    GUIStyle currentStyle = nodeStyles[(int)currentStyleType];

                    GUI.Box(nodeRect, GUIContent.none, currentStyle);

                    if (e.type == EventType.MouseDown && nodeRect.Contains(mousePos))
                    {
                        if (e.button == 0) // Left click
                        {
                            nodeStyleTypes[x, y] = (ETileType)(((int)currentStyleType + 1) % nodeStyles.Length);
                            e.Use();
                        }
                        else if (e.button == 1) // Right click
                        {
                            ShowContextMenu(x, y);
                            e.Use();
                        }
                    }

                    GUI.Label(nodeRect, currentStyleType.ToString(), new GUIStyle(GUI.skin.label)
                    {
                        alignment = TextAnchor.MiddleCenter
                    });
                }
            }

            Handles.EndGUI();
        }

        private void ShowContextMenu(int x, int y)
        {
            GenericMenu menu = new GenericMenu();

            for (int i = 0; i < Enum.GetValues(typeof(ETileType)).Length; i++)
            {
                ETileType styleType = (ETileType)i;
                menu.AddItem(new GUIContent(styleType.ToString()), nodeStyleTypes[x, y] == styleType,
                    () =>
                    {
                        nodeStyleTypes[x, y] = styleType;
                        Repaint();
                    });
            }

            menu.ShowAsContext();
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

            panOffset += drag * 0.5f;
            Vector3 newOffset = new Vector3(panOffset.x % gridSpacing, panOffset.y % gridSpacing, 0);

            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }

            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }

            Handles.color = Color.white;
            Handles.EndGUI();
        }
    }
}