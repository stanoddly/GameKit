using System.Runtime.InteropServices;

namespace Pixely;

public static class ExtensionMethods
{
    public static TValue GetValueOrNew<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
        where TValue : class, new()
    {
        ref TValue? value = ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out bool exists);

        if (!exists)
        {
            value = new TValue();
        }

        return value!;
    }
}