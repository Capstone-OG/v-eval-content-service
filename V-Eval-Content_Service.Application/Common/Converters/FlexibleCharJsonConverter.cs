using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace V_Eval_Content_Service.Application.Common.Converters;

/// <summary>
/// Converter giúp chuyển đổi an toàn mọi giá trị JSON (chuỗi nhiều ký tự "A.", "Đáp án A", "0", số nguyên 0..3, null)
/// sang kiểu char chuẩn mực ('A', 'B', 'C', 'D') mà không bị ném lỗi 'could not be converted to System.Char'.
/// Hỗ trợ đầy đủ mọi dạng dữ liệu bóc tách từ AI OCR, trích xuất tài liệu hoặc người dùng dán vào.
/// </summary>
public class FlexibleCharJsonConverter : JsonConverter<char>
{
    public override char Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            string? str = reader.GetString();
            if (string.IsNullOrWhiteSpace(str)) return 'A';

            str = str.Trim();

            // 1. Nếu chỉ là 1 ký tự hoặc có dấu bao quanh như "A.", "(B)", "[C]", "D:"
            string stripped = str.Trim(" \t\r\n.:();,[]'\"".ToCharArray());
            if (stripped.Length == 1)
            {
                char c = char.ToUpperInvariant(stripped[0]);
                if (c >= 'A' && c <= 'D') return c;
                if (c >= '0' && c <= '3') return (char)('A' + (c - '0'));
                if (c >= '1' && c <= '4') return (char)('A' + (c - '1'));
                return c;
            }

            // 2. Tìm khớp từ khóa (Đáp án A, Phương án B, Option C, Answer D, Chọn A...)
            var match = Regex.Match(str, @"(?:(?:đáp\s*án|phương\s*án|chọn|câu|answer|choice|option)\s*[:\-\.]?\s*)?([A-D])\b", RegexOptions.IgnoreCase);
            if (match.Success && match.Groups[1].Success)
            {
                return char.ToUpperInvariant(match.Groups[1].Value[0]);
            }

            // 3. Quét tìm ký tự A, B, C, D đầu tiên
            foreach (char c in str)
            {
                char upper = char.ToUpperInvariant(c);
                if (upper >= 'A' && upper <= 'D') return upper;
            }

            // 4. Nếu có số 0..3 trong chuỗi
            foreach (char c in str)
            {
                if (c >= '0' && c <= '3') return (char)('A' + (c - '0'));
                if (c >= '1' && c <= '4') return (char)('A' + (c - '1'));
            }

            return char.ToUpperInvariant(str[0]);
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out int num))
            {
                // Ánh xạ 0 -> A, 1 -> B, 2 -> C, 3 -> D
                if (num >= 0 && num <= 3) return (char)('A' + num);
                // Ánh xạ 1 -> A, 2 -> B, 3 -> C, 4 -> D
                if (num >= 1 && num <= 4) return (char)('A' + (num - 1));
                // Hoặc mã ASCII 65..68 ('A'..'D')
                if (num >= 65 && num <= 68) return (char)num;
                // Mã ASCII 97..100 ('a'..'d')
                if (num >= 97 && num <= 100) return (char)(num - 32);
            }
            return 'A';
        }

        if (reader.TokenType is JsonTokenType.Null or JsonTokenType.None)
        {
            return 'A';
        }

        return 'A';
    }

    public override void Write(Utf8JsonWriter writer, char value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}

/// <summary>
/// Converter an toàn cho kiểu Nullable char (char?)
/// </summary>
public class FlexibleNullableCharJsonConverter : JsonConverter<char?>
{
    private static readonly FlexibleCharJsonConverter _inner = new();

    public override char? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType is JsonTokenType.Null or JsonTokenType.None) return null;
        if (reader.TokenType == JsonTokenType.String && string.IsNullOrWhiteSpace(reader.GetString())) return null;

        return _inner.Read(ref reader, typeof(char), options);
    }

    public override void Write(Utf8JsonWriter writer, char? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteStringValue(value.Value.ToString());
        else
            writer.WriteNullValue();
    }
}
