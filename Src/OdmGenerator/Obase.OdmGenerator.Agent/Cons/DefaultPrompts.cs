using System;
using System.Text;

namespace Obase.OdmGenerator.Agent.Cons;

/// <summary>
///     默认的提示词集合
/// </summary>
public static class DefaultPrompts
{
    /// <summary>
    ///     获取默认的领域类型分析知识
    /// </summary>
    /// <returns>默认的领域类型分析知识</returns>
    public static string GetDefaultDomainInfoKnowledge()
    {
        var knowledgeBuilder = new StringBuilder("领域类型包括三种,分别是实体型,显式关联型和隐式关联型.");
        knowledgeBuilder.AppendLine("你需要按照如下的方式根据数据源提供的领域类信息来分析具体的领域类型:");
        knowledgeBuilder.AppendLine(
            "1. 实体型是领域中具有唯一确定主键为表示实体而定义的类型,如果类内的属性有类似于code,id,或者自身类名+code,自身类名+id的属性,或者注释中明确为主键的属性,应当判定为实体型;或者当类内属性虽然不符合如上规则,但自身的注释或命名表达了实体的含义,也应当判定为实体型.");
        knowledgeBuilder.AppendLine(
            "2. 显式关联型是领域中用于表示具有独属于关联的属性时而定义的类型,如果类内有引用了其他实体型,并且自身没有类似于code,id,或者自身类名+code,自身类名+id的属性,或者注释中不存在明确为主键的属性,应当判定为显式关联型;或者当类内属性虽然不符合如上规则,但自身的命名表达了关联的含义,也应当判定为显式关联型;如果类内存在每个引用的实体型的主键属性,也应当判定为显式关联型.");
        knowledgeBuilder.AppendLine(
            "3. 隐式关联型是领域中用于表示不具有独属于关联的属性的类型,隐式关联型不会在领域中定义类,而是表现为一个实体型引用其他的实体型,如果实体型内存在引用其他实体型的属性,应当判定这两个类之间存在隐式关联型.");
        knowledgeBuilder.AppendLine(
            "4. **注意:如果一个类被判定为显式关联型,但类内引用了大于2个实体型或者显式关联型,此情况通常是因为此类型是由显式关联型转成实体型的,其主键为由多个主键组成的联合主键,所以不应判定为显式关联型,而是判定为实体型.**");
        var knowledge = knowledgeBuilder.ToString();
        return knowledge;
    }

    /// <summary>
    ///     获取默认的实体型ODM配置生成规则(C#)
    /// </summary>
    /// <returns>默认的实体型ODM配置生成规则  </returns>
    public static string GetDefaultOdmGenerateCSharpEntityRule()
    {
        //构建实体型的配置规则
        var entityRuleBuilder = new StringBuilder();
        entityRuleBuilder.AppendLine("实体型的配置规则和步骤如下:");
        entityRuleBuilder.AppendLine(
            "1. 调用modelBuilder的Entity<>()函数,生成此实体型的配置对象.此函数的类型参数即为实体型的类型,返回值应存储于一个变量中以便后续配置使用,此变量通常命名为实体型的名称加上EntityConfiguration后缀.");
        entityRuleBuilder.AppendLine(
            "   如为实体型User生成ODM实体配置,则生成配置对象的代码为:var userEntityConfiguration = modelBuilder.Entity<User>();");
        entityRuleBuilder.AppendLine(
            "2. 调用实体型配置对象的HasKeyAttribute()函数,指定实体型的主键属性.此函数的参数为一个Lambda表达式,用于指定主键属性.如果有多个主键,则多次调用HasKeyAttribute()函数.");
        entityRuleBuilder.AppendLine("   如为实体型User指定主键属性为Id,则代码为:userEntityConfiguration.HasKeyAttribute(p => p.Id);");
        entityRuleBuilder.AppendLine(
            "   如为实体型User指定主键属性为Id和Code,则代码为:userEntityConfiguration.HasKeyAttribute(p => p.Id).HasKeyAttribute(p => p.Code);");
        entityRuleBuilder.AppendLine(
            "3. 调用实体型配置对象的HasKeyIsSelfIncreased函数,指定实体型的主键是否自增.此函数的参数为一个布尔值,表示主键是否自增.");
        entityRuleBuilder.AppendLine("   如果实体型的主键是int,short,long类型,则可以设置为自增.如果为string类型,则应当设置为不自增.如果有多个主键,应当设置为不自增.");
        entityRuleBuilder.AppendLine(
            "   如为实体型User的主键属性为Id类型为long,则代码为:userEntityConfiguration.HasKeyIsSelfIncreased(true);");
        entityRuleBuilder.AppendLine("4. 调用实体型配置对象的ToTable()函数,指定实体型的映射表,此函数的参数为字符串类型,通常为实体型的类型名称.");
        entityRuleBuilder.AppendLine("   如为实体型User指定映射表为User,则代码为:userEntityConfiguration.ToTable(nameof(User));");

        return entityRuleBuilder.ToString();
    }

    /// <summary>
    ///     获取默认的显式关联型ODM配置生成规则(C#)
    /// </summary>
    /// <returns>默认的显式关联型ODM配置生成规则</returns>
    public static string GetDefaultOdmGenerateCSharpExplicitlyRule()
    {
        //构建显式关联型的配置规则
        var explicitlyRuleBuilder = new StringBuilder();
        explicitlyRuleBuilder.AppendLine("显式关联型的配置规则和步骤如下:");
        explicitlyRuleBuilder.AppendLine(
            "1. 调用modelBuilder的Association<>()函数,生成此显式关联型的配置对象.此函数的类型参数即为显式关联型的类型,返回值应存储于一个变量中以便后续配置使用,此变量通常命名为显式关联型的名称加上Configuration后缀.");
        explicitlyRuleBuilder.AppendLine(
            "   如为显式关联型UserRole生成ODM显式关联型配置,则生成配置对象的代码为:var userRoleConfiguration = modelBuilder.Association<UserRole>();");
        explicitlyRuleBuilder.AppendLine(
            "2. 调用显式关联型型配置对象的AssociationEnd()函数,指定此显式关联型的关联端.此函数的参数为一个Lambda表达式,用于指定关联端,关联端是显式关联型中引用的其他实体型类型的属性.");
        explicitlyRuleBuilder.AppendLine(
            "   然后在AssociationEnd()函数的返回值上调用HasMapping()函数指定此关联端的主键映射关系,HasMapping()函数的参数是两个字符串,第一个是当前关联端的主键,第二个是此关联端在当前显式关联型中的映射属性名称.显式关联型至少有两个关联端,需要为每个关联端指定每个主键映射关系.比如有一个主键,则此关联端应当调用一次HasMapping,有两个主键此关联端应当调用两次HasMapping方法");
        explicitlyRuleBuilder.AppendLine(
            "   如为显式关联型UserRole引用的两个实体型User和Role(属性名也为User和Role,User主键为Id,Role的主键为Code,UserRole中定义了User主键属性UserId,Role主键属性RoleCode)指定关联端和关联端映射,则代码为:userRoleConfiguration.AssociationEnd(p => p.User).HasMapping(\"Id\",\"UserId\");\nuserRoleConfiguration.AssociationEnd(p => p.Role).HasMapping(\"Code\",\"RoleCode\");");
        explicitlyRuleBuilder.AppendLine(
            "   如为显式关联型UserApplet引用的两个实体型User和Applet(属性名也为User和Applet,User主键为Id,Applet的主键为AppletId和CreatorId,UserApplet中定义了User主键属性UserId,Applet主键属性AppletId和CreatorId)指定关联端和关联端映射,则代码为:userAppletConfiguration.AssociationEnd(p => p.User).HasMapping(\"Id\",\"UserId\");\nuserAppletConfiguration.AssociationEnd(p => p.Applet).HasMapping(\"AppletId\",\"AppletId\").HasMapping(\"CreatorId\",\"CreatorId\");");
        explicitlyRuleBuilder.AppendLine("3. 调用显式关联型配置对象的ToTable()函数,指定显式关联型的映射表,此函数的参数为字符串类型,通常为显式关联型的类型名称.");
        explicitlyRuleBuilder.AppendLine(
            "   如为显式关联型UserRole指定映射表为UserRole,则代码为:userRoleConfiguration.ToTable(nameof(UserRole));");
        return explicitlyRuleBuilder.ToString();
    }

    /// <summary>
    ///     获取默认的隐式关联型ODM配置生成规则(C#)
    /// </summary>
    /// <returns>默认的隐式关联型ODM配置生成规则</returns>
    public static string GetDefaultOdmGenerateCSharpImplicitRule()
    {
        //构建隐式关联型的配置规则
        var implicitlyRuleBuilder = new StringBuilder();
        implicitlyRuleBuilder.AppendLine("隐式关联型的配置规则和步骤如下:");
        implicitlyRuleBuilder.AppendLine(
            "1. 调用modelBuilder的Association()函数,生成此隐式关联型的配置器对象.返回值应存储于一个变量中以便后续配置使用,此变量通常命名为参与隐式关联型的各个类型名称加上Configuration后缀.");
        implicitlyRuleBuilder.AppendLine(
            "   如为实体型Role和Permission参与的隐式关联型生成ODM隐式关联型配置,则生成配置对象的代码为:var rolePermissionConfiguration = modelBuilder.Association();");
        implicitlyRuleBuilder.AppendLine(
            "2. 调用隐式关联型配置器对象的AssociationEnd<>()函数,指定此隐式关联型的关联端.此函数的类型参数用于指定关联端的类型,关联端的是隐式关联型中参与关联的类型.");
        implicitlyRuleBuilder.AppendLine(
            "   然后在AssociationEnd<>()函数的返回值上调用HasMapping()函数指定此关联端的主键映射关系,HasMapping()函数的参数是两个字符串,第一个是当前关联端的主键,第二个是此关联端在当前显式关联型中的映射属性名称.隐式关联型至少有两个关联端,需要为每个关联端指定每个主键映射关系.");
        implicitlyRuleBuilder.AppendLine(
            "   隐式关联型的关联端映射往往与此关联关系存储于参与关联的哪个关联端类型有关系,如果存储于参与关联的某一个关联端类型中,那么此类型自己的关联端主键映射即为自己的主键属性,其他关联端的主键映射则为存储在此类型中的属性.比如有一个主键,则此关联端应当调用一次HasMapping,有两个主键此关联端应当调用两次HasMapping方法.");
        implicitlyRuleBuilder.AppendLine(
            "   如果参与隐式关联的所有类型都没有存储其他关联端主键属性,则关联端映射设置为类名+主键名.");
        implicitlyRuleBuilder.AppendLine(
            "   如为两个实体Role和Permission参与(Role的主键为Code,Permission的主键为Id,并且在Permission中定义了Role的主键属性RoleCode)参与的隐式关联型指定关联端和关联端映射,则代码为:rolePermissionConfiguration.AssociationEnd<Role>().HasMapping(\"Code\",\"RoleCode\");\nrolePermissionConfiguration.AssociationEnd<Permission>().HasMapping(\"Id\",\"Id\");");
        implicitlyRuleBuilder.AppendLine(
            "   如为两个实体型User和Role(User和Role中都没有存储其他关联端主键属性)参与的隐式关联型指定关联端和关联端映射,则代码为:userRoleConfiguration.AssociationEnd<User>().HasMapping(\"Id\",\"UserId\");\nuserRoleConfiguration.AssociationEnd<Role>().HasMapping(\"Code\",\"RoleCode\");");
        implicitlyRuleBuilder.AppendLine(
            "   如为两个实两个实体型User和Applet(User主键为Id,Applet的主键为AppletId和CreatorId,并且在Applet中定义了User主键属性UserId)参与的隐式关联型指定关联端和关联端映射,,则代码为:userAppletConfiguration.AssociationEnd<User>().HasMapping(\"Id\",\"UserId\");\nuserAppletConfiguration.AssociationEnd<Applet>().HasMapping(\"AppletId\",\"AppletId\").HasMapping(\"CreatorId\",\"CreatorId\");");
        implicitlyRuleBuilder.AppendLine(
            "3. 调用隐式关联型配置器对象的ToTable()函数,指定隐式关联型的映射表,此函数的参数为字符串类型.此映射表通常为参与隐式关联型的类型中存储了其他关联端主键属性的类型名称的类型名称.");
        implicitlyRuleBuilder.AppendLine(
            "   如果参与隐式关联的所有类型都没有存储其他关联端主键属性,则直接将映射表命名为参与隐式关联类型名称组合.");
        implicitlyRuleBuilder.AppendLine(
            "   如为两个实体型Role和Permission(Permission中定义了Role的主键属性RoleCode)参与的隐式关联型指定映射表,则映射表应当为Permission,代码为:rolePermissionConfiguration.ToTable(nameof(Permission));");
        implicitlyRuleBuilder.AppendLine(
            "   如为两个实体型User和Role(User和Role中都没有存储其他关联端主键属性)参与的隐式关联型指定映射表,那么就将映射表命名为UserRole,代码为:userRoleConfiguration.ToTable(\"UserRole\");");
        return implicitlyRuleBuilder.ToString();
    }

    /// <summary>
    ///     获取默认的实体型ODM配置生成规则(Java)
    /// </summary>
    /// <returns>默认的实体型ODM配置生成规则  </returns>
    public static string GetDefaultOdmGenerateJavaEntityRule()
    {
        //构建实体型的配置规则
        var entityRuleBuilder = new StringBuilder();
        entityRuleBuilder.AppendLine("实体型的配置规则和步骤如下:");
        throw new NotImplementedException();
    }

    /// <summary>
    ///     获取默认的显式关联型ODM配置生成规则(Java)
    /// </summary>
    /// <returns>默认的显式关联型ODM配置生成规则</returns>
    public static string GetDefaultOdmGenerateJavaExplicitlyRule()
    {
        //构建显式关联型的配置规则
        var explicitlyRuleBuilder = new StringBuilder();
        explicitlyRuleBuilder.AppendLine("显式关联型的配置规则和步骤如下:");
        throw new NotImplementedException();
    }

    /// <summary>
    ///     获取默认的隐式关联型ODM配置生成规则(Java)
    /// </summary>
    /// <returns>默认的隐式关联型ODM配置生成规则</returns>
    public static string GetDefaultOdmGenerateJavaImplicitRule()
    {
        //构建隐式关联型的配置规则
        var implicitlyRuleBuilder = new StringBuilder();
        implicitlyRuleBuilder.AppendLine("隐式关联型的配置规则和步骤如下:");
        throw new NotImplementedException();
    }
}