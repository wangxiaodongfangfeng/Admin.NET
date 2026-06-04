# Admin.NET Entity 类生成总结

## 已生成的Entity类

### ✅ 共生成7个Entity类文件

| 序号 | 文件名 | 表名 | 说明 |
|------|--------|------|------|
| 1 | LkShift.cs | Lk_Shift | 班次表 |
| 2 | LkProductType.cs | Lk_ProductType | 产品类型表 |
| 3 | LkNgPosition.cs | Lk_NgPosition | NG位置表 |
| 4 | LkLeakageSeverity.cs | Lk_LeakageSeverity | 泄露程度表 |
| 5 | LkDictionary.cs | Lk_Dictionary | 数据字典表 |
| 6 | LkInspectionRecord.cs | Lk_InspectionRecord | 检测记录表（核心） |
| 7 | LkInspectionNgPosition.cs | Lk_InspectionNgPosition | 检测NG位置详情表 |

## 特性对比

### 与示例模板的对比

| 特性 | 示例(LkSchedule) | 本项目Entity |
|------|-----------------|-------------|
| 继承基类 | ✅ EntityBase | ✅ EntityBase |
| 租户隔离 | ✅ [Tenant] | ✅ [Tenant] |
| 表名标注 | ✅ [SugarTable] | ✅ [SugarTable] |
| 列描述 | ✅ [SugarColumn] | ✅ [SugarColumn] |
| 可为空标识 | ✅ virtual string? | ✅ virtual string? |
| 字段长度 | ✅ Length | ✅ Length |
| 索引支持 | ❌ | ✅ [SugarIndex] |
| 导航属性 | ❌ | ✅ [Navigate] |

## 增强功能

本项目的Entity类相比示例模板增加了以下功能：

### 1. 索引定义
```csharp
[SugarIndex("Index_InspectionRecord_Date", nameof(Date), OrderByType.Asc)]
```

### 2. 导航属性
```csharp
[Navigate(NavigateType.OneToOne, nameof(ShiftId))]
public virtual LkShift Shift { get; set; }
```

### 3. 更丰富的数据类型
- `decimal` - 用于压力值等精确数值
- `long` - 用于ID主键
- `bool` - 用于状态标识
- `text` - 用于长文本字段

### 4. 默认值设置
```csharp
public virtual int SortOrder { get; set; } = 0;
public virtual bool IsActive { get; set; } = true;
```

## 表关系图

```
┌─────────────────┐
│  LkShift        │
│  (班次表)        │
└─────────┬───────┘
          │ 1
          │
          │ N
┌─────────▼──────────────────┐
│  LkInspectionRecord        │
│  (检测记录表-核心)          │
└─────────┬──────────────────┘
          │ 1
          │
    ┌─────┼─────┐
    │     │     │
    │ N   │ N   │ 1
    │     │     │
┌───▼───┐ │  ┌──▼──────────────┐
│Product│ │  │LkInspectionNg   │
│Type   │ │  │Position         │
└───────┘ │  └──┬───────────┬──┘
          │     │ 1         │ 1
          │     │ N         │ N
          │  ┌──▼─────┐ ┌──▼─────────────┐
          │  │LkNg    │ │LkLeakage       │
          │  │Position│ │Severity        │
          │  └────────┘ └────────────────┘
          │
          │ N
     ┌────▼────────┐
     │LkDictionary │
     │(字典表)      │
     └─────────────┘
```

## 核心设计理念

### 1. 分离原则
- 基础数据表：独立管理（班次、产品类型、NG位置、泄露程度）
- 业务数据表：检测记录及其详情
- 字典表：通用配置数据

### 2. 扩展性
- 使用字典表支持动态配置
- 导航属性支持延迟加载和预加载
- 软删除支持数据恢复

### 3. 性能优化
- 关键字段添加索引
- 使用适当的数据类型
- 支持分页查询

## 使用场景

### 场景1: 创建检测记录
```csharp
var record = new LkInspectionRecord
{
    Operator = "张三",
    Date = "2026-06-03",
    ShiftId = 1,
    Pressure = 12.5m,
    ProductTypeId = 1,
    TestResult = "NG",
    UserId = 1,
    CreateTime = "14:30:00"
};

await _db.Insertable(record).ExecuteCommandAsync();
```

### 场景2: 查询带关联数据
```csharp
var records = await _db.Queryable<LkInspectionRecord>()
    .Includes(x => x.Shift)
    .Includes(x => x.ProductType)
    .Includes(x => x.NgPositions)
    .Where(x => x.Date == "2026-06-03")
    .ToListAsync();
```

### 场景3: 统计分析
```csharp
var stats = await _db.Queryable<LkInspectionRecord>()
    .GroupBy(x => x.TestResult)
    .Select(x => new 
    {
        Result = x.TestResult,
        Count = SqlFunc.AggregateCount(x.Id)
    })
    .ToListAsync();
```

## 数据库迁移

### 初始化所有表
```csharp
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

### 单表迁移
```csharp
// 只迁移检测记录表
db.CodeFirst.InitTables(typeof(LkInspectionRecord));
```

## 最佳实践

### 1. DTO转换
```csharp
// 建议创建对应的DTO类
public class InspectionRecordDto
{
    public long Id { get; set; }
    public string Operator { get; set; }
    public string Date { get; set; }
    public string ShiftName { get; set; } // 从导航属性获取
    // ... 其他字段
}
```

### 2. 查询优化
```csharp
// 只查询需要的字段
var list = await _db.Queryable<LkInspectionRecord>()
    .Select(x => new { x.Id, x.Operator, x.Date })
    .ToListAsync();
```

### 3. 批量操作
```csharp
// 批量插入
await _db.Insertable(recordList).ExecuteCommandAsync();

// 批量更新
await _db.Updateable(recordList).ExecuteCommandAsync();
```

## 文件结构

```
AdminNET-Entities/
├── LkShift.cs                      # 班次表
├── LkProductType.cs                # 产品类型表
├── LkNgPosition.cs                 # NG位置表
├── LkLeakageSeverity.cs           # 泄露程度表
├── LkDictionary.cs                 # 数据字典表
├── LkInspectionRecord.cs           # 检测记录表（核心）
├── LkInspectionNgPosition.cs      # NG位置详情表
├── README.md                       # 详细文档
└── ENTITY_SUMMARY.md              # 本文件
```

## 注意事项

### ⚠️ 重要提示

1. **租户ID**: 所有Entity的租户ID为 `"1300000000001"`，请根据实际项目修改
2. **命名空间**: 使用 `Admin.NET.Application.Entity`，请根据实际项目调整
3. **基类**: 所有Entity继承 `EntityBase`，确保Admin.NET.Core已引用
4. **数据库**: 可支持SQLite、SQL Server、MySQL等多种数据库

### 🔧 配置建议

1. 在 `appsettings.json` 中配置数据库连接字符串
2. 在启动类中配置SqlSugar数据库类型
3. 根据需要启用CodeFirst自动建表功能
4. 配置软删除过滤器

## 下一步工作

- [ ] 创建对应的Service服务层
- [ ] 创建对应的DTO类
- [ ] 创建对应的Controller控制器
- [ ] 添加数据验证特性
- [ ] 编写单元测试
- [ ] 配置权限控制
- [ ] 添加数据种子

## 版本信息

- **创建日期**: 2026-06-03
- **版本**: v1.0.0
- **框架**: Admin.NET
- **ORM**: SqlSugar
- **作者**: Kiro AI Assistant

---

✅ **所有Entity类已成功生成，可以直接用于Admin.NET项目！**
