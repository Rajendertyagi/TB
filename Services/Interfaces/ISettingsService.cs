namespace TB.Services.Interfaces;

public interface ISettingsService
{
    T? Get<T>(string key, T? defaultValue = default);
    void Set(string key, object value);
    Dictionary<string, object> GetAll();
    string GetAllJson();
}
