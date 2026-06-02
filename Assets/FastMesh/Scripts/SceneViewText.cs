using UnityEngine;

// Chỉ gọi namespace UnityEditor khi đang chạy trong môi trường Editor của Unity
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FastMesh_Example
{
    [ExecuteInEditMode]
    public class SceneViewText : MonoBehaviour
    {
        public bool isShow = true;
        string text2 = "These 3D models, all created with \"Fast Mesh - 3D Asset Creation Tool\" (click)";
        Color backgroundColor = Color.white;
        Color textColor = Color.black;

        private void OnEnable()
        {
            // Bọc logic đăng ký sự kiện Editor để tránh lỗi khi Build
#if UNITY_EDITOR
            SceneView.duringSceneGui += OnSceneGUI;
#endif
        }

        private void OnDisable()
        {
            // Bọc logic hủy đăng ký sự kiện Editor
#if UNITY_EDITOR
            SceneView.duringSceneGui -= OnSceneGUI;
#endif
        }

#if UNITY_EDITOR
        // Bọc toàn bộ hàm xử lý giao diện Scene View bên trong #if UNITY_EDITOR
        private void OnSceneGUI(SceneView sceneView)
        {
            if (isShow == false) return;

            Handles.BeginGUI();
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 20;
            style.normal.textColor = textColor;
            style.wordWrap = true;

            float width = 420f;
            float height = 50f;
            float x = (sceneView.position.width - width) / 2f;
            float y = 10f;

            GUI.color = backgroundColor;
            GUI.DrawTexture(new Rect(x - 10, y - 10, width + 20, height + 20), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (GUI.Button(new Rect(x, y, width, height), text2, style))
            {
                Application.OpenURL("https://assetstore.unity.com/packages/slug/288711");
            }

            Handles.EndGUI();
        }
#endif
    }
}