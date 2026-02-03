namespace GAG.EasyVideo
{

    public static class EasyVideoStorageFactory
    {
        public static IStorageProvider Create()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidMediaStorageProvider();
#elif UNITY_EDITOR
            return new EditorStreamingAssetsProvider();
#else
            return new StandaloneExternalStorageProvider();
#endif
        }

        //HttpStorageProvider
        //USBStorageProvider
        //NASStorageProvider
    }
}