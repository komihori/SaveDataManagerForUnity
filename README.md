# SaveDataManager

SaveDataManager is a save data reading mechanism using UnityJsonUtility.\
暗号化したセーブ機能を実装します. \
セーブデータの変数や暗号鍵を設定することができます.\
非同期ロード/セーブにも対応しています.

# Requirement
* UnityEngine
* UnityEditor
* System

# Usage
1. unitypackage をインストール.
2. [+/SaveDataManagement/SaveData] を選択してセーブデータ用のスクリプトを作成し, HogeDataKeySettingsの鍵を設定.
```cs
using SaveDataManagement;
using SaveDataManagement.Encrypting;
#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class HogeData {
    // セーブデータの内容
    public string value = "none";
}

public class HogeDataKeySettings : IAesKeySettings {
    // AES鍵（keyを設定してください）
    public int GetAesKeySize() => 128;
    public int GetAesBlockSize() => 128;
    public string GetAesIv() => "16文字の文字列";
    public string GetAesKey() => "16文字の文字列";
}

public class HogeDataIO : SaveDataIO<HogeData, HogeDataKeySettings> { }

#if UNITY_EDITOR
public class HogeDataEditor : SaveDataEditorWindow<HogeDataIO, HogeData, HogeDataKeySettings> {
    [MenuItem("SaveDataManagement/HogeDataEditor")]
    static void ShowWindow() {
        EditorWindow.GetWindow<HogeDataEditor>();
    }
}
#endif
```
3. SaveDataIO の関数を呼んでセーブ/ロードを行う.\
※ データは User/Documents/アプリケーション名/ の中に保存されます.
```cs
void Main() {
    // データをロード
    HogeDataIO._IO.LoadSaveData();
    HogeData data = HogeDataIO._IO.SaveData_;
    
    // データをセーブ
    data.value = "fuga";
    HogeDataIO._IO.WriteSaveData();
}
```
# SaveDataIO<Data, KeySettings>
## Static Variable
```cs
SaveDataIO<Data, KeySettings> _IO { get; }
```
## Public Variable
```cs
Data SaveData_
```
## Public Function
```cs
void SetSaveData(Data saveData)
void InitSaveData()
async Task<bool> InitSaveDataAsync()

void WriteSaveData()
void WriteSaveData(Data saveData)
void WriteJsonSaveData(Data data)
Task<bool> WriteJsonSaveDataTask(Data data)

Data LoadSaveData()
Data LoadJsonSaveData()
Task<Data> LoadJsonSaveDataAsync()
```
## Note
* AES鍵は16文字の文字列にする必要があります. AesKeyGeneratorEditorWindowを使うと自動生成できます.
* セーブデータの変数は全て読み書き可能でシリアライズ化されたものを使ってください.

## License
"SaveDataManager" is under [MIT license](https://en.wikipedia.org/wiki/MIT_License).
