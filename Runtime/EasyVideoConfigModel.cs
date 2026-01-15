using System.Collections.Generic;

[System.Serializable]
public class VideoConfig
{
    public IdleConfig idle;
    public ActionConfig actions;
}

[System.Serializable]
public class IdleConfig
{
    public string folder;
    public bool loop;
    public bool returnAfterAction;
    public bool allowExternalTrigger;
}

[System.Serializable]
public class ActionConfig
{
    public string folder;
    public List<EasyVideoModel> videos;
}
