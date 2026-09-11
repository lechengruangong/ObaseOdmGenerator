/*
┌──────────────────────────────────────────────────────────────┐
│　描   述：C#代码的领域类信息分析器.
│　作   者：Obase开发团队
│　版权所有：武汉乐程软工科技有限公司
│　创建时间：2026-8-19 11:37:14
└──────────────────────────────────────────────────────────────┘
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TreeSitter;

namespace Obase.OdmGenerator.Agent.Analyzer;

/// <summary>
///     C#代码的领域类信息分析器
///     负责解析C#代码并提取领域类信息
/// </summary>
public class DotNetDomainInfoAnalyzer : IDomainInfoAnalyzer
{
    /// <summary>
    ///     领域代码文件字典，键为文件名，值为文件内容
    /// </summary>
    private readonly Dictionary<string, string> _codeFiles = new();

    /// <summary>
    ///     初始化分析器，读取指定路径下的C#代码文件并存储其内容
    /// </summary>
    /// <param name="path">指定路径</param>
    public DotNetDomainInfoAnalyzer(string path)
    {
        //读取目录内的.cs文件 包括子目录
        var dir = new DirectoryInfo(path);
        var files = dir.GetFiles("*.cs", SearchOption.AllDirectories)
            //有些文件可能在bin或obj目录下 这些文件是编译生成的 不需要分析 所以过滤掉
            .Where(f => !f.FullName.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar)
                        && !f.FullName.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
            .ToList();

        //制作一个文件内容字典 键为文件名 值为文件内容 方便后续分析
        foreach (var file in files)
        {
            using var reader = new StreamReader(file.OpenRead());
            var content = reader.ReadToEnd();
            if (!_codeFiles.TryAdd(file.Name, content))
                throw new NotSupportedException("暂不支持解析不同命名空间下的同名文件.");
        }
    }

    /// <summary>
    ///     清理文档注释的行内容 去掉注释符号与summary标签 并去掉前后空格 只保留有内容的行
    /// </summary>
    /// <param name="commentLines">注释的原始行集合</param>
    /// <returns>清理后的注释内容</returns>
    private static string CleanSummaryComment(IEnumerable<string> commentLines)
    {
        return string.Join(Environment.NewLine, commentLines
            .Select(line => line.Replace("///", "").Replace("<summary>", "").Replace("</summary>", "").Trim())
            .Where(line => !string.IsNullOrEmpty(line)));
    }

    /// <summary>
    ///     分析代码文件，提取领域类信息
    /// </summary>
    /// <returns>领域类信息</returns>
    public List<DomainClassInfo> Analyze()
    {
        var allClasses = new List<DomainClassInfo>();

        //解析每个文件
        foreach (var (filePath, code) in _codeFiles)
        {
            //构造C#的解析器 解析AST语法树
            using var language = new Language("C#");
            using var parser = new Parser(language);
            using var tree = parser.Parse(code);
            if (tree == null)
                throw new ArgumentException($"无法抽取{filePath}的语法树.");

            //提取领域类信息

            //查询语法树 所有 class_declaration 节点
            //得到一个列表 每两个为一组 分别是类名和类体
            var query = new Query(tree.Language, @"
                    (class_declaration
                        name: (identifier) @class-name
                        body: (declaration_list) @body
                    )
                ");

            //一个数组列表 每个数组有2个元素 分别是类名 类体
            var classGroupList = query.Execute(tree.RootNode).Captures.Chunk(2).ToList();

            //查询类声明前的文档注释 某个类的注释一定在类声明节点之前 可能有数行 只取从/// <summary> 到 /// </summary> 的注释内容
            //此处按类名作为键存储其注释 以避免没有注释的类导致注释与类错位
            var classCommentQuery = new Query(tree.Language, @"
                    (
                       (comment)* @class.summary.lines
                       .
                       (class_declaration) @class-declaration
                    )
                ");
            var classCommentList = classCommentQuery.Execute(tree.RootNode).Captures.ToList();

            //类名与类注释的字典
            var classCommentDict = new Dictionary<string, string>();

            //记录当前类之前的注释与当前类声明节点 成对处理
            var pendingCommentLines = new List<string>();
            foreach (var capture in classCommentList)
            {
                if (capture.Name == "class.summary.lines")
                {
                    //注释节点 暂存
                    pendingCommentLines.Add(capture.Node.Text);
                    continue;
                }

                //类声明节点 取类名作为键
                var declaredNameNode = capture.Node.Children.FirstOrDefault(n => n.Type == "identifier");
                if (declaredNameNode != null)
                    classCommentDict[declaredNameNode.Text] = CleanSummaryComment(pendingCommentLines);
                //清空暂存的注释 继续处理下一个类
                pendingCommentLines.Clear();
            }

            foreach (var group in classGroupList)
            {
                //取类名节点的文本 即类名
                var className = group[0].Node.Text;

                //取此类的注释
                classCommentDict.TryGetValue(className, out var classComment);
                classComment ??= string.Empty;

                //继续查询类体节点 取其中的属性声明节点
                query = new Query(tree.Language, @"
                    (property_declaration
                        type: (identifier) @type
                        name: (identifier) @name    
                    )
                ");

                //得到一个列表 每两个元素一组 是一个属性的类型节点和名称
                var referencedTypePropertyList = query.Execute(group[1].Node).Captures.Chunk(2).ToList();
                //引用列表
                var referencedTypes = new List<Tuple<string, string>>();

                //先将引用类型的属性加入引用类型列表
                foreach (var referencedTypeProperty in referencedTypePropertyList)
                {
                    //取类型节点的文本 即属性类型
                    var propertyType = referencedTypeProperty[0].Node.Text;
                    //取名称节点的文本 即属性名称
                    var propertyName = referencedTypeProperty[1].Node.Text;
                    //加入引用类型列表
                    referencedTypes.Add(new Tuple<string, string>(propertyType, propertyName));
                }


                //继续查询类体节点 取其中的系统类型的属性声明节点
                query = new Query(tree.Language, @"
                    (property_declaration
                        type: (predefined_type) @type
                        name: (identifier) @name
                    )
                ");

                //得到一个列表 每两个元素一组 是一个属性的类型节点和名称
                var propertyList = query.Execute(group[1].Node).Captures.Chunk(2).ToList();

                //系统类型属性列表
                var propertyTypes = new List<Tuple<string, string>>();

                foreach (var property in propertyList)
                {
                    //取类型节点的文本 即属性类型
                    var propertyType = property[0].Node.Text;
                    //取名称节点的文本 即属性名称
                    var propertyName = property[1].Node.Text;
                    //加入属性列表
                    propertyTypes.Add(new Tuple<string, string>(propertyType, propertyName));
                }

                //继续查询类体节点 取其中的泛型类型声明节点
                query = new Query(tree.Language, @"
                    (property_declaration
                        type: (generic_name) @type
                        name: (identifier) @name 
                    )
                ");

                //得到一个列表 每两个元素一组 是一个泛型属性的类型节点和名称
                var genericTypeList = query.Execute(group[1].Node).Captures.Chunk(2).ToList();
                //循环此节点列表 第一个元素即为泛型的定义 要从中出去泛型的类型名称和泛型参数类型名称 例如 List<User> 的类型名称为 List 泛型参数类型名称为 User
                //第二个元素为属性名称
                foreach (var genericType in genericTypeList)
                {
                    //取identifier子节点和type_argument_list子节点
                    var identifierNode = genericType[0].Node.Children.FirstOrDefault(n => n.Type == "identifier");
                    var typeArgumentNode =
                        genericType[0].Node.Children.FirstOrDefault(n => n.Type == "type_argument_list");
                    //如果定义是List 说明这是一个List<T>类型 取出泛型参数类型名称加入引用的类型列表
                    if (identifierNode?.Text == "List" && typeArgumentNode != null)
                        referencedTypes.Add(new Tuple<string, string>(typeArgumentNode.Children[1].Text,
                            genericType[1].Node.Text));
                }

                //继续查询类体节点 取其中的属性和注释节点
                //某个属性的注释一定是在属性声明节点的前面 可能有数行 只取从/// <summary> 到 /// </summary> 的注释内容
                query = new Query(tree.Language, @"
                       (
                          (comment)* @summary.lines
                          .
                          (property_declaration
                            name: (identifier) @prop.name
                          )
                        )
                ");

                //得到一个列表 列表中若干的元素为一组 以取到属性名称prop.name为标识 此前的所有comment节点为一组 即为该属性的注释内容
                var propertyCommentList = query.Execute(group[1].Node).Captures.ToList();

                var dict = new Dictionary<string, string>();

                //上一个属性名称节点的索引
                var lastProp = 0;
                //开始处理注释和属性
                for (var i = 0; i < propertyCommentList.Count; i++)
                {
                    //当前节点
                    var capture = propertyCommentList[i];
                    //prop.name的是属性节点 其他的都是comment节点 只处理prop.name节点
                    if (capture.Name == "prop.name")
                    {
                        //找到属性名称节点
                        var propName = capture.Node.Text;
                        //向前查找所有的comment节点 直到遇到第一个不是comment的节点为止
                        //考虑到有些属性可能没有注释 所以只查找到上一个属性名称节点为止
                        var commentLines = new List<string>();
                        for (var j = i - 1; j >= lastProp; j--)
                        {
                            var prevCapture = propertyCommentList[j];
                            if (prevCapture.Name == "summary.lines")
                            {
                                //取出注释内容 去掉/// <summary> </summary> 并去掉前后空格
                                var text = CleanSummaryComment([prevCapture.Node.Text]);
                                if (!string.IsNullOrEmpty(text))
                                    commentLines.Add(text);
                            }
                            else
                            {
                                break;
                            }
                        }

                        //将注释内容反转 因为是从后往前查找的
                        commentLines.Reverse();
                        //将注释内容加入字典
                        dict.Add(propName, string.Join(Environment.NewLine, commentLines));
                        //记录上一个属性名称节点的索引
                        lastProp = i;
                    }
                }

                //加入信息
                allClasses.Add(new DomainClassInfo
                {
                    ClassName = className,
                    ClassComment = classComment,
                    ReferencedTypes = referencedTypes,
                    PropertyList = propertyTypes,
                    PropertyComments = dict
                });
            }
        }

        //过滤 引用类型只保留引用了 其他已解析类 的类型 而 属性则相反 只保留没有引用 其他已解析类 的类
        var allClassNames = allClasses.Select(c => c.ClassName).ToHashSet();
        foreach (var cls in allClasses)
        {
            cls.ReferencedTypes = cls.ReferencedTypes
                .Where(t => allClassNames.Contains(t.Item1))
                .ToList();
            cls.PropertyList = cls.PropertyList
                .Where(t => !allClassNames.Contains(t.Item1))
                .ToList();
            cls.PropertyComments = cls.PropertyComments
                .Where(t => !allClassNames.Contains(t.Key))
                .ToDictionary(p => p.Key, p => p.Value);
        }


        return allClasses;
    }
}