using System;
using System.IO;
using UnityEngine;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using SaveDataManagement.Encrypting;

namespace SaveDataManagement
{
    /// <summary>
    /// セーブデータの読み書きを行う。
    /// </summary>
    ///
    public class SaveDataIO<Data, KeySettings> where Data : class, new() where KeySettings : IAesKeySettings, new()
    {
        private static SaveDataIO<Data, KeySettings> _io = null;
        public static SaveDataIO<Data, KeySettings> _IO { get {
                if (_io == null) _io = new SaveDataIO<Data, KeySettings>();
                return _io;
            }
        }

        public Encrypt GenerateEncrypt() {
            var encrypt = new Encrypt();
            var keySettings = new KeySettings();
            encrypt.LoadAes(keySettings);
            return encrypt;
        }

        public string _HashName {
            get {
                byte[] hashBytes;
                using (var csp = SHA256.Create()) {
                    var targetBytes = System.Text.Encoding.UTF8.GetBytes(typeof(Data).ToString());
                    hashBytes = csp.ComputeHash(targetBytes);
                }
                var hashStr = new StringBuilder();
                foreach (var hashByte in hashBytes) {
                    hashStr.Append(hashByte.ToString("x2"));
                }
                return hashStr.ToString();
            }
        }
        public string _DocumentFolderPath => System.IO.Path.Combine(System.Environment.GetFolderPath(Environment.SpecialFolder.Personal), Application.productName);
        public string _DefaultSaveDataName => $"DefaultData.{Application.productName}";
        public string _FileName {
            get {
#if UNITY_EDITOR
                return $"T{_HashName}.{Application.productName}";
#else
                return $"U{_HashName}.{Application.productName}";
#endif
            }
        }
        public virtual string _PlayDataVersion => "0.0.1";
        public string _FullPath => System.IO.Path.Combine(_DocumentFolderPath, _FileName);
        public string _JsonFileName => $"U{_HashName}.Json";
        public string _FullJsonPath => System.IO.Path.Combine(_DocumentFolderPath, _JsonFileName);
        private Data saveData_;
        public Data SaveData_ {
            get {
                if (saveData_ == null) {
                    InitSaveData();
                    Debug.Log("Try Init");
                }
                return saveData_;
            }
            private set {
                saveData_ = value;
            }
        }

        public void SetSaveData(Data saveData) => SaveData_ = saveData;

        public void InitSaveData() {
            Data _LoadData;
            if (!Directory.Exists(_DocumentFolderPath))
                Directory.CreateDirectory(_DocumentFolderPath);
            if (File.Exists(_FullPath)) {
                var encrypt = GenerateEncrypt();
                _LoadData = JsonUtility.FromJson<Data>(encrypt.AesDecryptSaveData(_FullPath));
                Debug.Log("Load : " + _FullPath);
            } else {
                string defaultSaveDataPath = System.IO.Path.Combine(Application.streamingAssetsPath, _DefaultSaveDataName);
                if (File.Exists(defaultSaveDataPath)) {
                    var encrypt = GenerateEncrypt();
                    _LoadData = JsonUtility.FromJson<Data>(encrypt.AesDecryptSaveData(defaultSaveDataPath));
                } else {
                    Debug.LogError("DefaultSaveData NotFound!");
                    _LoadData = new Data();
                }

                saveData_ = _LoadData;
                WriteSaveData();
                Debug.Log($"Load : {defaultSaveDataPath}, {_LoadData.ToString()}");
            }
            SaveData_ = _LoadData;
            return;
        }
        public async Task<bool> InitSaveDataAsync() {
            Data _LoadData;

            bool directoryExist = false;

            try {
                directoryExist = Directory.Exists(_DocumentFolderPath);
            } catch (Exception ex) {
                UnityEngine.Debug.LogError(ex);
                return false;
            }

            if (!directoryExist) {
                try {
                    Directory.CreateDirectory(_DocumentFolderPath);
                } catch (Exception ex) {
                    UnityEngine.Debug.LogError(ex);
                    return false;
                }
            }

            bool fileExist = false;

            try {
                fileExist = File.Exists(_FullPath);
            } catch (Exception ex) {
                UnityEngine.Debug.LogError(ex);
                return false;
            }

            if (fileExist) {
                var encrypt = GenerateEncrypt();

                string data = null;

                if(!(await encrypt.AesDecryptSaveDataTask(_FullPath, out data))) {
                    UnityEngine.Debug.LogError("Faild to save the SaveData.");
                    return false;
                }

                _LoadData = JsonUtility.FromJson<Data>(data);
                UnityEngine.Debug.Log("Load : " + _FullPath);
            } else {
                _LoadData = new Data();
                saveData_ = _LoadData;
                if(!(await WriteSaveDataAsync(_LoadData))) {
                    UnityEngine.Debug.LogError("Faild to save the SaveData.");
                    return false;
                }

                UnityEngine.Debug.Log("Load : " + _FullPath + ", " + _LoadData.ToString());
            }
            SaveData_ = _LoadData;
            return true;
        }
        /// <summary>
        /// セーブデータを出力する。
        /// </summary>
        /// <param name="data">SaveData</param>
        public void WriteSaveData()
        {
            if (!Directory.Exists(_DocumentFolderPath)) 
                Directory.CreateDirectory(_DocumentFolderPath);
            var encrypt = GenerateEncrypt();
            encrypt.AesEncryptDataSave(SaveData_, _FullPath);
            Debug.Log("Save Path : " + _FullPath);

        }
        public void WriteSaveData(Data saveData) {
            if (!Directory.Exists(_DocumentFolderPath))
                Directory.CreateDirectory(_DocumentFolderPath);
            var encrypt = GenerateEncrypt();
            encrypt.AesEncryptDataSave(saveData, _FullPath);
            Debug.Log("Save Path : " + _FullPath);

        }
        public async Task<bool> WriteSaveDataAsync(Data saveData) {
            bool isSuccess = false;

            bool directoryExist = false;

            try {
                directoryExist = Directory.Exists(_DocumentFolderPath);
            } catch (Exception ex) {
                isSuccess = false;
                UnityEngine.Debug.LogError(ex);
                return isSuccess;
            }

            if (!directoryExist) {
                try {
                    Directory.CreateDirectory(_DocumentFolderPath);
                } catch (Exception ex) {
                    isSuccess = false;
                    UnityEngine.Debug.LogError(ex);
                    return isSuccess;
                }
            }

            bool fileExist = false;

            try {
                fileExist = File.Exists(_FullPath);
            } catch (Exception ex) {
                isSuccess = false;
                UnityEngine.Debug.LogError(ex);
                return isSuccess;
            }

            var encrypt = GenerateEncrypt();
            if(!(await encrypt.AesEncryptDataSaveTask(saveData, _FullPath))) {
                isSuccess = false;
                UnityEngine.Debug.LogError("Faild to save the SaveData.");
                return isSuccess;
            }
            if (!(await WriteJsonSaveDataTask(saveData))) {
                isSuccess = false;
                UnityEngine.Debug.LogError("Faild to save as the SaveData.");
                //bool b = UnityEditor.EditorUtility.DisplayDialog("Error", "Faild to save the SaveData." + "\nWriteSaveDataAsync", "OK");
                //if (b) Application.Quit();
                return isSuccess;
            }
            UnityEngine.Debug.Log("Save Path : " + _FullPath);
            isSuccess = true;

            return isSuccess;
        }
        /// <summary>
        /// Jsonデータとしてセーブデータを出力する。
        /// </summary>
        /// <param name="data">SaveData</param>
        public void WriteJsonSaveData(Data data)
        {
            string json = JsonUtility.ToJson(data);
            UnityEngine.Debug.Log("WriteJsonSaveData : " + json);
            if (!Directory.Exists(_DocumentFolderPath))
                Directory.CreateDirectory(_DocumentFolderPath);
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(json);
            System.IO.File.WriteAllBytes(_FullJsonPath, arr);
            Debug.Log("WriteJasonSaveDate " + _FullPath + ", " + data.ToString());
        }
        public Task<bool> WriteJsonSaveDataTask(Data data) {
            bool isSuccess = false;
            string json = JsonUtility.ToJson(data);
            UnityEngine.Debug.Log("WriteJsonSaveData : " + json);

            bool directoryExist = false;

            try {
                directoryExist = Directory.Exists(_DocumentFolderPath);
            } catch (Exception ex) {
                isSuccess = false;
                UnityEngine.Debug.LogError(ex);
                return Task.FromResult(isSuccess);
            }

            if (!directoryExist) {
                try {
                    Directory.CreateDirectory(_DocumentFolderPath);
                } catch (Exception ex) {
                    isSuccess = false;
                    UnityEngine.Debug.LogError(ex);
                    return Task.FromResult(isSuccess);
                }
            }

            byte[] arr = System.Text.Encoding.UTF8.GetBytes(json);
            try {
                System.IO.File.WriteAllBytes(_FullJsonPath, arr);
                UnityEngine.Debug.Log("WriteJasonSaveDate " + _FullPath + ", " + data.ToString());
                isSuccess = true;
            }catch(Exception ex) {
                UnityEngine.Debug.LogError(ex);
                isSuccess = false;
            }

            return Task.FromResult(isSuccess);
        }
        /// <summary>
        /// セーブデータを読み込む。
        /// 存在しない場合は初期データを出力し、読み込む。
        /// </summary>
        /// <returns>SaveData</returns>
        public Data LoadSaveData() {
            Data _LoadData;
            if (!Directory.Exists(_DocumentFolderPath))
                Directory.CreateDirectory(_DocumentFolderPath);
            if (File.Exists(_FullPath)) {
                var encrypt = GenerateEncrypt();
                _LoadData = JsonUtility.FromJson<Data>(encrypt.AesDecryptSaveData(_FullPath));
                Debug.Log("Load : " + _FullPath);
            } else {
                _LoadData = new Data();
                WriteSaveData();
                Debug.Log("Load : " + _FullPath + ", " + _LoadData.ToString());
            }
            return _LoadData;
        }
        public async Task<Data> LoadSaveDataAsync() {
            Data _LoadData = null;
            bool directoryExist = false;
            try {
                directoryExist = Directory.Exists(_DocumentFolderPath);
            } catch (Exception ex) {
                UnityEngine.Debug.LogError(ex);
                return _LoadData;
            }
            if (!directoryExist) {
                try {
                    Directory.CreateDirectory(_DocumentFolderPath);
                } catch (Exception ex) {
                    UnityEngine.Debug.LogError(ex);
                    return _LoadData;
                }
            }
            bool fileExist = false;
            try {
                fileExist = File.Exists(_FullPath);
            } catch (Exception ex) {
                UnityEngine.Debug.LogError(ex);
                return _LoadData;
            }
            if (fileExist) {
                var encrypt = GenerateEncrypt();
                string data = null;
                if (!await encrypt.AesDecryptSaveDataTask(_FullPath, out data)) {
                    UnityEngine.Debug.LogError("Faild to Load DecryptedSaveData.");
                    return _LoadData;
                }

                _LoadData = JsonUtility.FromJson<Data>(data);
                UnityEngine.Debug.Log("Load : " + _FullPath);
            } else {
                _LoadData = new Data();
                bool writeSaveDataSuccess = await WriteSaveDataAsync(_LoadData);
                if (!writeSaveDataSuccess) {
                    UnityEngine.Debug.LogError("Faild to save the New SaveData.");
                    return _LoadData;
                }
                UnityEngine.Debug.Log("Load : " + _FullPath + ", " + _LoadData.ToString());
            }
            return _LoadData;
        }
        /// <summary>
        /// Jsonデータを読み込む。
        /// </summary>
        /// <returns>SaveData</returns>
        public Data LoadJsonSaveData() {
            Data _LoadData;
            if (!Directory.Exists(_DocumentFolderPath))
                Directory.CreateDirectory(_DocumentFolderPath);
            if (File.Exists(_FullJsonPath)) {
                byte[] arrRead = System.IO.File.ReadAllBytes(_FullJsonPath);
                _LoadData = JsonUtility.FromJson<Data>(System.Text.Encoding.UTF8.GetString(arrRead));
                Debug.Log("Load : " + _FullJsonPath);
            } else {
                _LoadData = new Data();
                WriteJsonSaveData(_LoadData);
                Debug.Log("Load : " + _FullJsonPath);
            }
            return _LoadData;
        }
        public async Task<Data> LoadJsonSaveDataAsync() {
            Data _LoadData = null;
            bool directoryExist = false;
            try {
                directoryExist = Directory.Exists(_DocumentFolderPath);
            } catch (Exception ex) {
                UnityEngine.Debug.LogError(ex);
                return _LoadData;
            }
            if (!directoryExist) {
                try {
                    Directory.CreateDirectory(_DocumentFolderPath);
                } catch (Exception ex) {
                    UnityEngine.Debug.LogError(ex);
                    return _LoadData;
                }
            }
            bool fileExist = false;
            try {
                fileExist = File.Exists(_FullJsonPath);
            } catch (Exception ex) {
                UnityEngine.Debug.LogError(ex);
                return _LoadData;
            }
            if (fileExist) {
                byte[] arrRead = null;
                try {
                    arrRead = System.IO.File.ReadAllBytes(_FullJsonPath);
                } catch (Exception ex) {
                    UnityEngine.Debug.LogError(ex);
                    return _LoadData;
                }
                _LoadData = JsonUtility.FromJson<Data>(System.Text.Encoding.UTF8.GetString(arrRead));
                UnityEngine.Debug.Log("Load : " + _FullJsonPath);
            } else {
                _LoadData = new Data();
                bool success = await WriteJsonSaveDataTask(_LoadData);
                if (!success) {
                    UnityEngine.Debug.LogError("Faild to write New SaveData as JsonFile");
                }
                UnityEngine.Debug.Log("Load : " + _FullJsonPath);
            }
            return _LoadData;
        }

        public class Encrypt : EncryptBase<Data> {
            // AES設定値
            //===================================
            private IAesKeySettings _aesKeySettings = null;
            protected override int aesKeySize => _aesKeySettings.GetAesKeySize();
            protected override int aesBlockSize => _aesKeySettings.GetAesBlockSize();
            protected override string aesIv => _aesKeySettings.GetAesIv();
            protected override string aesKey => _aesKeySettings.GetAesKey();
            //===================================
            public void LoadAes(IAesKeySettings aesKeySettings) {
                _aesKeySettings = aesKeySettings;
            }
        }
    }
}
