using System;
using UnityEngine;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace SaveDataManagement.Encrypting {
    public abstract class EncryptBase<Data> where Data : class, new() {
        // AES設定値
        //===================================
        protected abstract int aesKeySize { get; }
        protected abstract int aesBlockSize { get; }
        protected abstract string aesIv { get; }
        protected abstract string aesKey { get; }
        //===================================
        /// <summary>
        /// AES暗号化セーブデータ保存
        /// </summary>
        public void AesEncryptDataSave(Data save, string path) {
            // クラスをJSON文字列に変換
            //string json = save.GetJsonData();
            string json = JsonUtility.ToJson(save);
            // byte配列に変換
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(json);
            // AES暗号化
            byte[] arrEncrypted = AesEncrypt(arr, aesKeySize, aesBlockSize, aesIv, aesKey);
            // ファイル書き込み
            System.IO.File.WriteAllBytes(path, arrEncrypted);
        }
        public Task<bool> AesEncryptDataSaveTask(Data save, string path) {
            bool isSuccess;
            // クラスをJSON文字列に変換
            //string json = save.GetJsonData();
            string json = JsonUtility.ToJson(save);
            // byte配列に変換
            byte[] arr = System.Text.Encoding.UTF8.GetBytes(json);
            // AES暗号化
            byte[] arrEncrypted = AesEncrypt(arr, aesKeySize, aesBlockSize, aesIv, aesKey);
            // ファイル書き込み
            try {
                System.IO.File.WriteAllBytes(path, arrEncrypted);
                isSuccess = true;
            } catch (Exception ex) {
                Debug.LogError(ex);
                isSuccess = false;
            }
            return Task.FromResult(isSuccess);

        }
        /// <summary>
        /// AES暗号化セーブデータロード
        /// </summary>
        public string AesDecryptSaveData(string path) {
            // ファイル読み込み
            byte[] arrRead = System.IO.File.ReadAllBytes(path);
            // 復号化
            byte[] arrDecrypt = AesDecrypt(arrRead, aesKeySize, aesBlockSize, aesIv, aesKey);
            // byte配列を文字列に変換
            return System.Text.Encoding.UTF8.GetString(arrDecrypt);
        }
        public Task<bool> AesDecryptSaveDataTask(string path, out string aesDecryptSaveData) {
            bool isSuccess = false;
            aesDecryptSaveData = null;
            byte[] arrRead;
            // ファイル読み込み
            try {
                arrRead = System.IO.File.ReadAllBytes(path);
            } catch (Exception ex) {
                isSuccess = false;
                Debug.LogError(ex);
#if UNITY_EDITOR
                bool b = UnityEditor.EditorUtility.DisplayDialog("Error", ex.ToString() + "\nAesDecryptSaveDataTask", "OK");
                if (b) Application.Quit();
#endif
                return Task.FromResult(isSuccess);
            }

            // 復号化
            byte[] arrDecrypt = AesDecrypt(arrRead, aesKeySize, aesBlockSize, aesIv, aesKey);

            // byte配列を文字列に変換
            aesDecryptSaveData = System.Text.Encoding.UTF8.GetString(arrDecrypt);
            isSuccess = true;
            return Task.FromResult(isSuccess);
        }
        /// <summary>
        /// AES暗号化
        /// </summary>
        public byte[] AesEncrypt(byte[] byteText, int aesKeySize, int aesBlockSize, string aesIv, string aesKey) {
            // AESマネージャー取得
            var aes = GetAesManager(aesKeySize, aesBlockSize, aesIv, aesKey);
            // 暗号化
            byte[] encryptText = aes.CreateEncryptor().TransformFinalBlock(byteText, 0, byteText.Length);
            return encryptText;
        }
        /// <summary>
        /// AES復号化
        /// </summary>
        public byte[] AesDecrypt(byte[] byteText, int aesKeySize, int aesBlockSize, string aesIv, string aesKey) {
            // AESマネージャー取得
            var aes = GetAesManager(aesKeySize, aesBlockSize, aesIv, aesKey);
            // 復号化
            byte[] decryptText = aes.CreateDecryptor().TransformFinalBlock(byteText, 0, byteText.Length);

            return decryptText;
        }
        /// <summary>
        /// AesManagedを取得
        /// </summary>
        /// <param name="keySize">暗号化鍵の長さ</param>
        /// <param name="blockSize">ブロックサイズ</param>
        /// <param name="iv">初期化ベクトル(半角X文字（8bit * X = [keySize]bit))</param>
        /// <param name="key">暗号化鍵 (半X文字（8bit * X文字 = [keySize]bit）)</param>
        private AesManaged GetAesManager(int keySize, int blockSize, string iv, string key) {
            Debug.Log($"key{key}:{key.Length} iv{iv}:{iv.Length} keySize{keySize} blockSize{blockSize}");
            AesManaged aes = new AesManaged();
            aes.KeySize = keySize;
            aes.BlockSize = blockSize;
            aes.Mode = CipherMode.CBC;
            aes.IV = System.Text.Encoding.UTF8.GetBytes(iv);
            aes.Key = System.Text.Encoding.UTF8.GetBytes(key);
            aes.Padding = PaddingMode.PKCS7;
            return aes;
        }
    }
}