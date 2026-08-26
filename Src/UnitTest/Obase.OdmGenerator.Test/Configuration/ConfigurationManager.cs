using System.IO;
using Microsoft.Extensions.Configuration;
using Obase.OdmGenerator.Agent.Config;

namespace Obase.OdmGenerator.Test.Configuration;

/// <summary>
///     配置管理器
/// </summary>
public static class ConfigurationManager
{
    /// <summary>
    ///     配置
    /// </summary>
    private static IConfiguration _configuration;

    /// <summary>
    ///     配置
    /// </summary>
    private static IConfiguration Configuration =>
        _configuration ??= BuildRdbConfig();

    /// <summary>
    ///     获取LLM配置
    /// </summary>
    public static IApikeyConfiguration ApikeyLlmConfiguration =>
        new ApikeyConfiguration
        {
            ApiKey = Configuration["ApiKey"],
            Endpoint = Configuration["Endpoint"],
            ModelId = Configuration["ModelId"]
        };

    /// <summary>
    ///     域类文件夹
    /// </summary>
    public static string DomainClassPath => Configuration["DomainClassDir"];

    /// <summary>
    ///     创建配置
    ///     使用当前文件夹上层目录下的Obase.Test.Config.json文件作为配置源
    /// </summary>
    /// <returns></returns>
    private static IConfiguration BuildRdbConfig()
    {
        //固定寻找当前路径上级的Obase.Test.Config.json 文件
        //代码库中不提供此文件 请参考Obase.Test.Config.example.json 创建此文件
        // 示例文件位于Obase.OdmGenerator.Test项目文件夹下
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"{Directory.GetCurrentDirectory()}/../Obase.Test.Config.json", false);
        var config = builder.Build();

        return config;
    }
}