namespace SaveDataManagement.Encrypting {
    public interface IAesKeySettings {
        int GetAesKeySize();
        int GetAesBlockSize();
        string GetAesIv();
        string GetAesKey();
    }
    public abstract class AesKeySettingsBase : IAesKeySettings {
        public int GetAesKeySize() => GetAesKey().Length * 8;
        public int GetAesBlockSize() => GetAesIv().Length * 8;
        public abstract string GetAesIv();
        public abstract string GetAesKey();
    }
}