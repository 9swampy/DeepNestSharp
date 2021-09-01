namespace DeepNestSharp.Ui.Services
{
  using DeepNestSharp.Domain.Services;

  public sealed class SettingsService : ISettingsService
  {
    /// <inheritdoc/>
    public void SetValue<T>(string key, T value)
    {
      //if (!SettingsStorage.ContainsKey(key)) SettingsStorage.Add(key, value);
      //else SettingsStorage[key] = value;

      //Properties.Settings.Default["ClipByHull"] = value;
      //Properties.Settings.Default.Save();
      //Properties.Settings.Default.Upgrade();
    }

    /// <inheritdoc/>
    public T GetValue<T>(string key)
    {
      //return (bool)Properties.Settings.Default["ClipByHull"];
      //if (SettingsStorage.TryGetValue(key, out object value))
      //{
      //  return (T)value;
      //}

      return default;
    }
  }
}
