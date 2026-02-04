namespace GAG.EasyVideo
{
    public interface IStorageProvider
    {
        string RootPath { get; }
        bool IsReady { get; }

        void Initialize(UnityEngine.MonoBehaviour runner);

        string GetVideoPath(string relativePath);
        bool FileExists(string relativePath);
    }
}
