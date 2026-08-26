using Obase.OdmGenerator.Agent.Config;

namespace Obase.OdmGenerator.Test.Configuration;

/// <summary>
///     LLM配置
/// </summary>
public class ApikeyConfiguration : IApikeyConfiguration
{
    /// <summary>
    ///     API KEY
    ///     鉴权凭证
    /// </summary>
    public string ApiKey { get; init; }

    /// <summary>
    ///     API接入点
    /// </summary>
    public string Endpoint { get; init; }

    /// <summary>
    ///     模型ID
    /// </summary>
    public string ModelId { get; init; }
}