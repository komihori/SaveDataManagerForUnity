#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using SaveDataManagement.Encrypting;

namespace SaveDataManagement {
    public class SaveDataEditorWindow<IO, Data, KeySettings> : EditorWindow
        where IO : SaveDataIO<Data, KeySettings>, new()
        where KeySettings : IAesKeySettings, new()
        where Data : class, new()
        {
        private static IO _SaveDataIO {
            get {
                if (_saveDataIO == null) _saveDataIO = new IO();
                return _saveDataIO;
            }
        }
        private static IO _saveDataIO;

        private static string _DocumentFolderPath => _SaveDataIO._DocumentFolderPath;
        private static string _FullPath => _SaveDataIO._FullPath;

        [SerializeField] Data _saveData;
        private bool _saveDataWritting = false;

        /*
        [MenuItem("Debugger/.../SaveDataEditor")]
        static void ShotWindow() {
             EditorWindow.GetWindow<SaveDataEditor<S, D>>();
        }
        */

        protected virtual void OnGUI() {
            EditorGUI.BeginDisabledGroup(_saveDataWritting);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load")) {
                LoadAsync();
            }
            if (GUILayout.Button("Save")) {
                SaveAsync();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(20);

            if (GUILayout.Button("SaveData Initialize!!")) {
                Init();
            }
            EditorGUILayout.Space(20);
            if (_saveData == null) return;

            var data = new SerializedObject(this);
            data.Update();
            EditorGUILayout.PropertyField(data.FindProperty("_saveData"), true);
            data.ApplyModifiedProperties();

            EditorGUI.EndDisabledGroup();
        }

        private async void Init() {
            _saveDataWritting = true;
            Repaint();
            _saveData = new Data();
            _SaveDataIO.WriteSaveData(new Data());
            bool success = await _SaveDataIO.WriteSaveDataAsync((new Data()));
            if (success) Debug.Log($"SaveDataEditor/init success!!");
            else Debug.Log("SaveDataEditor/init failed...");
            _saveDataWritting = false;
            Repaint();
            
        }
        private void Load() {
            _saveData = _SaveDataIO.SaveData_;
            Data loadData;
            if (!Directory.Exists(_DocumentFolderPath)) {
                Directory.CreateDirectory(_DocumentFolderPath);
            }
            if (File.Exists(_FullPath)) {
                loadData = JsonUtility.FromJson<Data>(_SaveDataIO.GenerateEncrypt().AesDecryptSaveData(_FullPath));
                Debug.Log($"SaveDataEditor/Load: {_FullPath}");
            } else {
                loadData = new Data();
                _saveData = loadData;
                _SaveDataIO.WriteSaveData(_saveData);
                Debug.Log($"SaveDataEditor/Load: {_FullPath}, {loadData.ToString()}");
            }
            _saveData = loadData;
            Repaint();
            Debug.Log($"SaveDataEditor/load finish. Load: {_saveData.ToString()}");
        }
        private async void LoadAsync() {
            _saveDataWritting = true;
            Repaint();
            _saveData = await _SaveDataIO.LoadSaveDataAsync();
            if (_saveData == null) {
                Debug.LogError("SaveDataEditor/load failed...");
            } else {
                Debug.Log($"SaveDataEditor/load success!! Load:{_saveData.ToString()}");
            }
            _saveDataWritting = false;
            Repaint();
        }
        private void Save() {
            _SaveDataIO.WriteSaveData(_saveData);
            Repaint();
            Debug.Log($"SaveDataEditor/save success!! Save:{_saveData.ToString()}");
        }
        private async void SaveAsync() {
            _saveDataWritting = true;
            Repaint();
            if (!await _SaveDataIO.WriteSaveDataAsync(_saveData)) {
                Debug.LogError("SaveDataEditor/save async failed...");
            } else {
                Debug.Log($"SaveDataEditor/save async success!! Save:{_saveData.ToString()}");
            }
            _saveDataWritting = false;
            Repaint();
        }
    }
}
#endif