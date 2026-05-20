#if UNITY_EDITOR

namespace UnityEngine.Rendering
{ 
    public interface IRequiredSetting
    {
        public bool state { get; }
        public string name { get; }
        public string description { get; }
    }
}
#endif