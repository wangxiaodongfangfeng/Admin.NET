# Admin.NET Entity 类文档

## 概述
本目录包含绿控检测系统的所有业务表实体类，遵循Admin.NET框架规范。这些Entity类可以直接用于Admin.NET后端项目中。

## Entity 类列表

### 基础数据表

#### 1. LkShift.cs - 班次表
**表名**: `Lk_Shift`  
**说明**: 存储班次信息（早班、中班、晚班）

**字段**:
- `Name` - 班次名称（如：早班 (08:00-16:00)）
- `StartTime` - 开始时间（如：08:00）
- `EndTime` - 结束时间（如：16:00）

#### 2. LkProductType.cs - 产品类型表
**表名**: `Lk_ProductType`  
**说明**: 存储产品类型信息

**字段**:
- `Name` - 产品类型名称
- `Description` - 描述

#### 3. LkNgPosition.cs - NG位置表
**表名**: `Lk_NgPosition`  
**说明**: 存储产品NG（不合格）位置信息

**字段**:
- `Name` - NG位置名称（如：位置1-顶部）
- `Description` - 描述

#### 4. LkLeakageSeverity.cs - 泄露程度表
**表名**: `Lk_LeakageSeverity`  
**说明**: 存储泄露严重程度信息

**字段**:
- `Name` - 泄露程度名称（如：轻微泄露）
- `Color` - 显示颜色（如：#e6a23c）
- `Level` - 严重等级（数字越大越严重）

#### 5. LkDictionary.cs - 数据字典表
**表名**: `Lk_Dictionary`  
**说明**: 存储系统通用数据字典

**字段**:
- `DictionaryName` - 字典名称（英文标识，如：product_status）
- `DisplayName` - 显示名称（中文名称，如：产品状态）
- `ItemName` - 项目名称（如：待检测）
- `ItemValue` - 项目值（可选代码，如：pending）
- `SortOrder` - 排序顺序
- `IsActive` - 是否启用
- `Description` - 描述

**索引**:
- `Index_Dictionary_Name` - 字典名称索引
- `Index_Dictionary_IsActive` - 启用状态索引

### 核心业务表

#### 6. LkInspectionRecord.cs - 检测记录表
**表名**: `Lk_InspectionRecord`  
**说明**: 存储检测记录主表信息

**字段**:
- `Operator` - 操作员
- `Date` - 检测日期
- `ShiftId` - 班次ID（外键）
- `Pressure` - 压力值（MPa）
- `ProductTypeId` - 产品类型ID（外键）
- `ProductModel` - 产品型号
- `BatchNumber` - 批次号
- `Specification` - 规格
- `SteelStamp` - 钢印号
- `TestResult` - 检测结果（OK/NG）
- `Images` - 图片URL列表（JSON数组）
- `UserId` - 用户ID
- `CreateTime` - 创建时间（时分秒）
- `Remarks` - 备注

**导航属性**:
- `Shift` - 班次信息
- `ProductType` - 产品类型信息
- `NgPositions` - NG位置详情列表

**索引**:
- `Index_InspectionRecord_Date` - 日期索引
- `Index_InspectionRecord_UserId` - 用户索引
- `Index_InspectionRecord_TestResult` - 检测结果索引
- `Index_InspectionRecord_ProductTypeId` - 产品类型索引

#### 7. LkInspectionNgPosition.cs - 检测NG位置详情表
**表名**: `Lk_InspectionNgPosition`  
**说明**: 存储检测记录的NG位置详细信息

**字段**:
- `InspectionId` - 检测记录ID（外键）
- `PositionId` - NG位置ID（外键）
- `ImageUrl` - 图片URL
- `LeakageSeverityId` - 泄露程度ID（外键）

**导航属性**:
- `InspectionRecord` - 检测记录
- `Position` - NG位置信息
- `LeakageSeverity` - 泄露程度信息

**索引**:
- `Index_InspectionNgPosition_InspectionId` - 检测记录索引

## 使用说明

### 1. 引入命名空间
```csharp
using Admin.NET.Application.Entity;
using Admin.NET.Core;
using SqlSugar;
```

### 2. 数据库配置
所有Entity都配置了租户标识 `[Tenant("1300000000001")]`，请根据实际情况修改。

### 3. 代码生成
Admin.NET支持根据Entity自动生成CRUD代码，包括：
- Service服务层
- Controller控制器
- DTO数据传输对象

### 4. 导航属性查询示例
```csharp
// 查询检测记录并包含关联数据
var records = await _db.Queryable<LkInspectionRecord>()
    .Includes(x => x.Shift)
    .Includes(x => x.ProductType)
    .Includes(x => x.NgPositions, ng => ng.Position)
    .Includes(x => x.NgPositions, ng => ng.LeakageSeverity)
    .Where(x => x.TestResult == "NG")
    .ToListAsync();
```

### 5. 分页查询示例
```csharp
// 分页查询检测记录
var pageData = await _db.Queryable<LkInspectionRecord>()
    .Includes(x => x.Shift)
    .Includes(x => x.ProductType)
    .OrderByDescending(x => x.Date)
    .ThenByDescending(x => x.CreateTime)
    .ToPagedListAsync(pageIndex, pageSize);
```

## EntityBase 基类

所有Entity都继承自 `EntityBase`，包含以下公共字段：
- `Id` - 主键（long）
- `CreateTime` - 创建时间（DateTime）
- `UpdateTime` - 更新时间（DateTime）
- `CreateUserId` - 创建用户ID
- `UpdateUserId` - 更新用户ID
- `IsDelete` - 是否删除（软删除标记）

## 命名规范

### 表命名
- 前缀：`Lk_`（绿控简写）
- 格式：Pascal命名法
- 示例：`Lk_InspectionRecord`

### 字段命名
- 格式：Pascal命名法
- 外键：以`Id`结尾，如`ShiftId`
- 布尔值：以`Is`开头，如`IsActive`

### 导航属性
- 单对象：使用实体名称
- 集合：使用复数形式
- 标注：使用 `[Navigate]` 特性

## 数据库迁移

### 使用CodeFirst创建表
```csharp
// 在Admin.NET中，通过以下方式创建数据库表
db.CodeFirst.InitTables(
    typeof(LkShift),
    typeof(LkProductType),
    typeof(LkNgPosition),
    typeof(LkLeakageSeverity),
    typeof(LkDictionary),
    typeof(LkInspectionRecord),
    typeof(LkInspectionNgPosition)
);
```

### 更新表结构
```csharp
// 备份数据后更新表结构
db.CodeFirst.SetStringDefaultLength(200).BackupTable().InitTables(typeof(LkInspectionRecord));
```

## 特性说明

### SugarColumn 常用参数
- `ColumnName` - 数据库列名
- `ColumnDescription` - 列描述
- `Length` - 字符串长度
- `IsNullable` - 是否可为空
- `DecimalDigits` - 小数位数
- `ColumnDataType` - 列数据类型（如：text）

### SugarIndex 索引
- 语法：`[SugarIndex("索引名", nameof(字段名), OrderByType)]`
- 用途：提高查询性能

### Navigate 导航属性
- `NavigateType.OneToOne` - 一对一关系
- `NavigateType.OneToMany` - 一对多关系
- `NavigateType.ManyToMany` - 多对多关系

## 注意事项

1. **租户隔离**: 所有表都设置了租户ID，确保多租户数据隔离
2. **软删除**: 继承EntityBase后自动支持软删除
3. **审计字段**: 创建人、更新人、创建时间、更新时间自动填充
4. **外键约束**: 数据库层面需要手动添加外键约束
5. **JSON字段**: Images字段存储JSON数组，查询时需要反序列化

## 扩展功能

### 添加自定义验证
```csharp
public partial class LkInspectionRecord
{
    [SugarColumn(IsIgnore = true)]
    public bool IsQualified => TestResult == "OK";
}
```

### 添加计算属性
```csharp
public partial class LkInspectionRecord
{
    [SugarColumn(IsIgnore = true)]
    public string DisplayDate => DateTime.Parse(Date).ToString("yyyy年MM月dd日");
}
```

## 技术栈

- **框架**: Admin.NET
- **ORM**: SqlSugar
- **.NET版本**: .NET 6.0+
- **数据库**: SQLite / SQL Server / MySQL（可配置）

## 版本历史

- **v1.0.0** - 初始版本，包含7个核心Entity类
- 创建时间：2026-06-03

## 联系方式

如有问题或建议，请联系开发团队。
