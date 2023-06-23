using System.Text.Json;

namespace LibSirius.Utils;

public static class Serializer
{
    public async static Task<T?> ReadFromJsonAsync<T>(FileInfo file)
        where T : new()
    {
        using Stream reader = file.OpenRead();
        return await JsonSerializer.DeserializeAsync<T>(reader);
    }

    public async static Task WriteToJsonAsync<T>(FileInfo file, T objectToWrite)
    {
        JsonSerializerOptions jso = new JsonSerializerOptions
        {
            WriteIndented = true,
        };
        file.Refresh();
        if (file.Exists)
            file.Delete();
        using Stream writer = file.OpenWrite();
        await JsonSerializer.SerializeAsync(writer, objectToWrite, jso);
    }
}
