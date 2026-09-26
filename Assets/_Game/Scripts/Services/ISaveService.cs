namespace Summit.Game.Services
{
    public interface ISaveService
    {
        float GetFloat(string key, float defaultValue = 0f);
        void SetFloat(string key, float value);
    }
}
