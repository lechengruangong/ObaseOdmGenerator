/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：智能体输出结果处理助手.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 18:26:38
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Obase.OdmGenerator.WorkFlow.Common;

/// <summary>
///     智能体输出结果处理助手
///     用于从智能体的输出文本中稳定地提取并反序列化Json数据
///     即使模型偶尔输出Markdown代码块、附加说明文字或单个Json对象,也能尽可能解析出期望的结果
/// </summary>
internal static class AgentOutputHelper
{
    /// <summary>
    ///     Json反序列化选项
    ///     在Web默认选项(忽略属性名大小写)的基础上,允许尾随逗号并跳过注释,以提高对智能体输出的兼容性
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerOptions.Web)
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    /// <summary>
    ///     匹配Markdown代码块的正则表达式
    /// </summary>
    private static readonly Regex MarkdownFenceRegex = new(@"```(?:json|JSON)?\s*([\s\S]*?)```", RegexOptions.Compiled);

    /// <summary>
    ///     从智能体的输出文本中提取Json数组并反序列化为对象集合
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="text">智能体的输出文本</param>
    /// <returns>反序列化后的对象集合</returns>
    public static List<T> DeserializeJsonArray<T>(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("反序列化结果失败,智能体输出内容为空.");

        //提取Json内容
        var json = ExtractJsonArray(text);

        try
        {
            return JsonSerializer.Deserialize<List<T>>(json, JsonOptions) ??
                   throw new InvalidOperationException("反序列化结果失败");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"反序列化结果失败,智能体输出无法解析为Json数组,原始输出如下:{Environment.NewLine}{text}", ex);
        }
    }

    /// <summary>
    ///     从智能体的输出文本中提取Json对象并反序列化
    /// </summary>
    /// <typeparam name="T">对象类型</typeparam>
    /// <param name="text">智能体的输出文本</param>
    /// <returns>反序列化后的对象</returns>
    public static T DeserializeJsonObject<T>(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException("反序列化结果失败,智能体输出内容为空.");

        //提取Json内容
        var json = ExtractJsonObject(text);

        try
        {
            return JsonSerializer.Deserialize<T>(json, JsonOptions) ??
                   throw new InvalidOperationException("反序列化结果失败");
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException(
                $"反序列化结果失败,智能体输出无法解析为Json对象,原始输出如下:{Environment.NewLine}{text}", ex);
        }
    }

    /// <summary>
    ///     从智能体的输出文本中提取Json数组
    ///     依次处理Markdown代码块包裹、前后附加文字、单个Json对象等情况
    /// </summary>
    /// <param name="text">智能体的输出文本</param>
    /// <returns>提取出的Json数组内容</returns>
    private static string ExtractJsonArray(string text)
    {
        var content = StripMarkdownFence(text);

        //提取Json数组区域:从第一个'['到最后一个']',跳过数组前后可能附加的任何说明文字
        var start = content.IndexOf('[');
        var end = content.LastIndexOf(']');
        if (start >= 0 && end > start)
            return content.Substring(start, end - start + 1);

        //如果输出的是单个Json对象,则将其包装为Json数组
        start = content.IndexOf('{');
        end = content.LastIndexOf('}');
        if (start >= 0 && end > start)
            return $"[{content.Substring(start, end - start + 1)}]";

        return content;
    }

    /// <summary>
    ///     从智能体的输出文本中提取Json对象
    ///     依次处理Markdown代码块包裹、前后附加文字、被包装成单元素数组等情况
    /// </summary>
    /// <param name="text">智能体的输出文本</param>
    /// <returns>提取出的Json对象内容</returns>
    private static string ExtractJsonObject(string text)
    {
        var content = StripMarkdownFence(text);

        //如果输出被包装成了Json数组,则解包取出数组内的Json对象
        if (content.StartsWith('[') && content.EndsWith(']'))
        {
            var innerStart = content.IndexOf('{');
            var innerEnd = content.LastIndexOf('}');
            if (innerStart >= 0 && innerEnd > innerStart)
                return content.Substring(innerStart, innerEnd - innerStart + 1);
        }

        //提取Json对象区域:从第一个'{'到最后一个'}',跳过对象前后可能附加的任何说明文字
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        if (start >= 0 && end > start)
            return content.Substring(start, end - start + 1);

        return content;
    }

    /// <summary>
    ///     去除文本中的Markdown代码块包裹,例如```json ... ```
    /// </summary>
    /// <param name="text">智能体的输出文本</param>
    /// <returns>去除代码块包裹后的文本</returns>
    private static string StripMarkdownFence(string text)
    {
        var content = text.Trim();
        var fenceMatch = MarkdownFenceRegex.Match(content);
        return fenceMatch.Success ? fenceMatch.Groups[1].Value.Trim() : content;
    }
}