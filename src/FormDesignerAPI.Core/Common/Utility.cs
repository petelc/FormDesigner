using System.Text.Json;
using System.Collections.Generic;

namespace FormDesignerAPI.Core.Common;

public static class Utility
{
    // Flattens JSON, collects all values for each key for type inference
    static void FlattenJsonAdvanced(JsonElement element, string prefix, Dictionary<string, List<JsonElement>> flat)
    {
        foreach (var prop in element.EnumerateObject())
        {
            string key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}_{prop.Name}";
            if (prop.Value.ValueKind == JsonValueKind.Object)
            {
                FlattenJsonAdvanced(prop.Value, key, flat);
            }
            else if (prop.Value.ValueKind == JsonValueKind.Array)
            {
                // If array of primitives, store as JSON string
                if (prop.Value.EnumerateArray().All(e => e.ValueKind != JsonValueKind.Object))
                {
                    if (!flat.ContainsKey(key))
                        flat[key] = new List<JsonElement>();
                    flat[key].Add(prop.Value);
                }
                // If array of objects, handle separately (see above)
            }
            else
            {
                if (!flat.ContainsKey(key))
                    flat[key] = new List<JsonElement>();
                flat[key].Add(prop.Value);
            }
        }

        // Infers type from all values for a key
        // static string InferSqlTypeAdvanced(List<JsonElement> values)
        // {
        //     bool hasString = false, hasInt = false, hasFloat = false, hasBool = false, hasNull = false;
        //     foreach (var v in values)
        //     {
        //         switch (v.ValueKind)
        //         {
        //             case JsonValueKind.String:
        //                 hasString = true;
        //                 break;
        //             case JsonValueKind.Number:
        //                 if (v.TryGetInt32(out _)) hasInt = true;
        //                 else hasFloat = true;
        //                 break;
        //             case JsonValueKind.True:
        //             case JsonValueKind.False:
        //                 hasBool = true;
        //                 break;
        //             case JsonValueKind.Null:
        //                 hasNull = true;
        //                 break;
        //             default:
        //                 hasString = true; // fallback for objects/arrays
        //                 break;
        //         }
        //     }
        //     if (hasString) return "NVARCHAR(MAX)";
        //     if (hasFloat) return "FLOAT";
        //     if (hasInt) return "INT";
        //     if (hasBool) return "BIT";
        //     return "NVARCHAR(MAX)";
        // }
    }
}
