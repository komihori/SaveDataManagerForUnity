#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace SaveDataManagement {
    public class AesKeyGeneratorEditorWindow : EditorWindow {
        [SerializeField] string _aesKey = "";

        [MenuItem("SaveDataManagement/AesKeyGenerater")]
        static void ShowWindow() {
            EditorWindow.GetWindow<AesKeyGeneratorEditorWindow>();
        }
        void OnGUI() {
            if (GUILayout.Button("Generate Key!!")) {
                _aesKey = GeneratePassword(16);
            }
            GUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.TextField("Aes Key", _aesKey);
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Copy")) EditorGUIUtility.systemCopyBuffer = _aesKey;
            GUILayout.EndHorizontal();
        }
        static string GeneratePassword(int length) {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()_+=-"; // 使用したい文字の範囲を指定します
            StringBuilder passwordBuilder = new StringBuilder();
            var random = new System.Random();
            for (int i = 0; i < length; i++) {
                int randomIndex = random.Next(0, validChars.Length);
                char randomChar = validChars[randomIndex];
                passwordBuilder.Append(randomChar);
            }
            return passwordBuilder.ToString();
        }
    }
}
#endif