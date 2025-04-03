using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ExplorerTabUtility.Models;

namespace ExplorerTabUtility.Helpers;

public class HotKeyActionJsonConverter : JsonConverter<HotKeyAction>
{
    // Map old numeric values to their corresponding enum values after reordering
    private static readonly Dictionary<int, HotKeyAction> LegacyMapping = new()
    {
        { 0, HotKeyAction.Open },
        { 1, HotKeyAction.Duplicate },
        { 2, HotKeyAction.ReopenClosed },
        { 3, HotKeyAction.SetTargetWindow },
        { 4, HotKeyAction.ToggleWinHook },
        { 5, HotKeyAction.ToggleReuseTabs },
        { 6, HotKeyAction.ToggleVisibility },
        { 7, HotKeyAction.NavigateBack },
        { 8, HotKeyAction.NavigateUp },
        { 9, HotKeyAction.DetachTab },
        { 10, HotKeyAction.SnapRight },
        { 11, HotKeyAction.SnapLeft },
        { 12, HotKeyAction.SnapUp },
        { 13, HotKeyAction.SnapDown },
        { 14, HotKeyAction.TabSearch }
    };

    public override HotKeyAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Number)
        {
            // For backward compatibility - read the numeric value
            var enumValue = reader.GetInt32();

            // Use the legacy mapping to get the correct enum value
            if (LegacyMapping.TryGetValue(enumValue, out var mappedValue))
            {
                return mappedValue;
            }

            // Fallback for values not in the mapping
            if (Enum.IsDefined(typeof(HotKeyAction), enumValue))
            {
                return (HotKeyAction)enumValue;
            }

            return HotKeyAction.Open; // Default value if the number is not valid
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            // New approach - read the string value
            var enumString = reader.GetString()!;

            // Try to parse the string as an enum value
            if (Enum.TryParse<HotKeyAction>(enumString, true, out var result))
            {
                return result;
            }

            return HotKeyAction.Open; // Default value if the string is not valid
        }

        throw new JsonException($"Unexpected token {reader.TokenType} when parsing HotKeyAction.");
    }

    public override void Write(Utf8JsonWriter writer, HotKeyAction value, JsonSerializerOptions options)
    {
        // Always write as a string
        writer.WriteStringValue(value.ToString());
    }
}