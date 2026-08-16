# MCR Mods 领域模型层架构介绍(本地缓存架构)

## 0. 架构定位（一句话）
> 这套机制的作用是：**用"进程内单例对象 + 原地更新 + 变更点主动刷新"的模式，让高频读、低频改的数据彻底脱离数据库查询**。
> 它的价值不在单次优化，而在于提供了一个**可复用的模式**——只要新数据符合"读多写少、变更点可枚举"的特征，就能照搬这套结构快速接入。

据此，Mods 不是"一堆数据库表的映射类"，而是**一套以进程内全局缓存为底座的领域模型层**：
普通缓存优化的是"单对象读取"，这套架构优化的是"**对象图的遍历**"——对象之间的导航、关系、派生统计全部在内存里完成，数据库只在首次加载和变更持久化时出现。

---

## 1. 三个核心机制

### 机制一：进程内单例对象

每个业务对象（以 AutoID 标识）在进程内**只有一个实例**，存放于全局缓存（`EntityBase.GetMyICache()`）：

- Key 的构成 = 对象类型前缀 + 唯一 ID（`GetPrefixName()` 返回 `"SCL"`，`AutoID` 形如 `SCL-xxxx`）；
- `EntityBase` 把 `Equals/GetHashCode/CompareTo` 全部定义在 AutoID 上，保证"同一 ID = 同一对象"在整个系统成立；
- 所有读取路径（单查、列表、导航）最终都收敛到"用 Key 从全局缓存取对象"，取不到才回源数据库并放入缓存。

### 机制二：原地更新

修改一个对象时**不 new 新对象、不重新 Set 缓存**：

```csharp
// School.Update —— 先持久化，成功后直接改字段
Result rs = this.EntityMaping_Excute("Update", ps);
if (rs.IsOK == true)
{
    this.Name = name;      // 直接改字段
    this.Remark = remark;
}
```

`this` 本身就是缓存里那个对象的引用，改字段即改缓存，所有持有该引用的代码立刻看到新值——连 `Set` 都省了。这是"不 new 新对象"的精髓，也是引用稳定性带来的最大红利。

### 机制三：变更点主动刷新

所有会改变数据的地方（Update_*/Insert/Delete）都是**可枚举的、封闭的入口**，每个入口在持久化成功后主动维护缓存：

| 变更点 | 缓存动作 |
|---|---|
| `Insert` | 持久化成功 → `Set(AutoID, theNew)` 入缓存 |
| `Update_*` | 持久化成功 → 原地改字段（引用即缓存） |
| `Delete` | 持久化成功 → `Clear(AutoID)` 出缓存 |

因为写入口只有这几个，所以"缓存与数据库一致"不需要复杂机制，靠约定就能守住。

---

## 2. 七条设计规范（理念 → Mods 代码对应）

### 规范一：字段二分法

每个实体的字段严格分为两类，并用 `#region 持久属性` 显式隔离：

| 类别 | 特征 | 例 |
|---|---|---|
| 持久属性 | 落库、参与映射 | `School.Name / Remark / IsDisable / CTime` |
| 逻辑字段 | 不落库、由持久属性派生、懒加载 | `School._FileTypeOfStatistical`（各分类文档数量统计） |

逻辑字段是"对象自带的二级缓存"，它的维护规则见规范六。

### 规范二：Key = 对象名 + 唯一 ID

`GetPrefixName()` 声明类型前缀（School→`"SCL"`、Subject→`"SBJ"`），`AutoID` 由 `EntityBase.CreateTagID()` 本地生成（时间戳 + 随机码，不依赖数据库自增）。Key 全局唯一、自带类型语义，任何模块拿到一个 ID 字符串就能定位对象。

### 规范三：首次查询入缓存；条件查询"先查 ID 再 load"

三种读路径全部收敛到同一个缓存协议：

```csharp
// 1) 单查：GetByID —— 缓存未命中才查库，查到即 Set
School the = EntityBase.GetMyICache().Get(autoID) as School;
if (the == null) { /* 查库 → ToEntity → Set */ }

// 2) 列表：GetAll —— 数据库只负责给出"目标键值集合"，对象逐个按 Key load
string autoID = r.GetValue(0).ToString();          // reader 只取 ID
School the = EntityBase.GetMyICache().Get(autoID); // 从缓存取，没有才填充

// 3) 分页条件查询：GetAllByKeyWord —— 直接复用 GetByID(autoID) load
```

数据库在列表查询中退化为"ID 筛选器"，对象本体始终从缓存来。

### 规范四：修改 = 先持久化，再原地改字段

见机制二。顺序不可颠倒：先 `EntityMaping_Excute` 写库，`IsOK` 之后才更新内存字段，保证失败不污染缓存。

### 规范五：NONE 空对象 / 原型

每个实体都有 `public static readonly School NONE = new School() { AutoID = "SCL-001", Name = "其它" }`：
既作"无效对象"占位（避免 null），又作执行映射 SQL 的**原型实例**（`School.NONE.EntityMaping_Excute("GetByID", …)`，因为执行 SQL 需要一个能报出自己类型前缀的实例）。

### 规范六：逻辑字段的事件驱动失效（置空重算）

```csharp
protected School()
{
    EntityBase.Evt_EntityChange += (entityInfo) =>
    {
        if (entityInfo is SourceDocument)
            _FileTypeOfStatistical = null;   // 置空，下次读取重新加载
    };
}
```

当相关的实体发生变更（`EntityMaping.Excute` 成功后触发 `Evt_EntityChange`），依赖它的逻辑字段被**置空**，下次访问时懒重算。
这比"事件发生时 +1 增量维护"更稳健：**哪怕漏了某次事件，下次读取也会自动从数据库修正，永远不会累积偏差**。

### 规范七：外部数据源统一适配

缓存层不只是"数据库缓存"，而是**统一的数据访问抽象**。`School_QST` 把第三方 QST 接口的学校数据也纳入同一套协议：

```csharp
EntityBase.GetMyICache().Set("School_QST::GetAll", list, DateTime.Now.AddHours(5));
```

带 5 小时 TTL 的集合级缓存，业务层拿到的仍是同一套 `GetAll/GetFindID_QST` 接口——数据来自 DB 还是外部 API，对上层透明。

---

## 3. 架构的进阶维度：从"单对象缓存"到"对象图遍历"

README 的第二部分展示了 Mods 更高层次的设计，这些在仓库代码中均有对应实现：

### 3.1 对象导航——关系在对象方法里，不在 SQL 里

```csharp
// Subject.cs
public WX_Member GetMyMember()      { return WX_Member.GetByID(this.MemberID); }
public IList<RoomClass> GetMyRoomClasses() { return RoomClass.GetListBySubject(this); }
public string CreateName            { get { return this.GetMyMember()?.Name; } }
```

实体不止存自己的字段，还能"走到"关联对象。因为底层是缓存，导航命中时是纯内存操作，**关系遍历的成本被缓存摊薄**——这是普通 ORM 的导航属性做不到的（每次都要查库或生成 JOIN）。

### 3.2 扩展类——同一对象的多视图投影

`Subject_Ext2`（老师视角：附加"我的班级集合"）、`Subject_Ext3`（学生视角：附加 `RoomClassID/CourseCount` 等联表字段，重写 `ToEntity` 多读两列）：

- **基础 `Subject`（单表字段）→ 进缓存，全局共享**；
- **扩展类（带联表统计字段）→ 不进缓存，每次查询现算**，因为派生字段依赖的关联数据会变。

这与规范六同源：派生数据要么不缓存（扩展类），要么缓存了靠事件失效（逻辑字段）。

### 3.3 `AddToList` ——列表加载的统一抽象

```csharp
EntityBase.AddToList<Subject>(list, readers, (r) => new Subject_Ext2(), (theAdd, reader2) => {…});
```

基类把"查列表 → 缓存优先填充 → 返回集合"的重复逻辑抽成泛型方法，实体只需提供"如何 new"和"如何转换"两个委托。

### 3.4 关系表也是对象

`Rel_Subject_RoomClass`、`Rel_RoomClass_Member` 把多对多关系建模成一等对象，有自己的 `GetByID/Insert/Delete`，也走缓存——关系查询同样命中内存，不必每次 JOIN。这是从关系型思维到对象思维的彻底转换。

---

## 4. 这种架构的好处

1. **读路径几乎脱离数据库**
   高频读的对象单查、列表、导航、关系遍历全部命中内存；数据库压力只剩首次加载和变更持久化。对"读多写少"的数据，这是数量级的收益。

2. **可复用的模式，而非一次性优化**
   新数据只要满足"读多写少、变更点可枚举"，照抄"持久属性 + GetByID + Update_*/Insert/Delete"模板即可接入，扩展性正是这套架构的核心价值。

3. **原地更新消灭了一大类缓存同步代码**
   引用稳定使得"改字段即更新缓存"，无需写回、无需版本号管理，所有持有引用的代码自动看到新值，不存在"缓存里的旧副本"。

4. **一致性靠收敛的变更点保证**
   写入口封闭且全部遵循"先持久化、后维护缓存"的顺序，失败不污染缓存；配合置空重算的逻辑字段，即使有遗漏也会被下次读取自动修正。

5. **领域建模在性能上变得可行**
   对象导航、关系对象、视图投影这些"面向对象"的优雅建模，通常因 JOIN 和重复查库而在性能上不可行；有了缓存底座，它们都变成了内存操作。

6. **统一的数据访问抽象**
   数据库数据、外部接口数据（QST）、派生统计，对上层呈现为同一套获取接口；调用方不需要关心数据来自哪里、是否带 TTL。

7. **约定一致、心智负担低**
   字段二分、Key 规则、读写顺序、NONE 原型，全部项目统一；读任何一个实体类，结构都是可预期的。

---

## 5. 这种架构的不足

1. **一致性依赖"变更点可枚举"这一前提，且靠人工守护**
   一旦出现绕过实体方法直接改库的路径（运维脚本、其它系统、批量 SQL），缓存不会感知，产生脏数据。这是该模式适用边界的根本约束——README 也明确它只适合"变更点可枚举"的数据。

2. **原地更新缺少字段级并发保护**
   缓存容器层有读写锁，但实体字段的原地修改本身没有锁；README 所说的"双层锁"在本 C# 实现中只落地了一层（容器锁），字段级并发写入在多线程宿主下存在竞态。

3. **事件失效存在订阅泄漏**
   `School` 在构造函数里订阅静态事件 `EntityBase.Evt_EntityChange` 且从不退订：每 new 一个 School 就多挂一个订阅，实例被静态事件引用而无法释放，且一次变更会触发所有存活实例的回调。对象数量大时是内存与性能的双重成本。

4. **手工模板重复，已滋生复制粘贴错误**
   每个实体重复 `ToEntity` 逐字段读取、每个操作重复 `ParameterTag` 数组。走读中发现的实际 Bug：`WX_Member.Update_MType` 中 `this.MType = MType` 自我赋值、`Update_IsFollowed` 实际改的是 `IsDisable`——这类错误正是手工字段映射的典型代价。

5. **实现细节瑕疵**
   `School.GetAllByKeyWord` 中 `GetByID(autoID)` 返回 null 时直接调用 `the.ToEntity(r)` 会空引用（README 的讨论中也已指出该瑕疵）。

6. **共享可变引用的隐性耦合**
   全局唯一实例意味着任何模块拿到对象后都能改其内存状态（包括逻辑字段）；没有只读视图约束时，"谁改的、改了没持久化"只靠纪律。

7. **适用面有明确边界**
   写频繁、变更点无法枚举、或对象量远超内存的数据不适合直接套用；这类数据仍需传统查询路径。扩展接入前必须先判断数据特征，否则收益不成立。

---

## 6. 总结

| 维度 | 结论 |
|---|---|
| 架构本质 | 以进程内全局缓存为底座的领域模型层，不是 ORM，也不是"对象缓存 + 写穿透" |
| 三大机制 | 进程内单例对象 / 原地更新 / 变更点主动刷新 |
| 关键区分 | 普通缓存优化"单对象读取"；这套架构优化"对象图的遍历" |
| 适用数据 | 读多写少、变更点可枚举 |
| 最大价值 | 可复用的模式——符合条件的新数据可照搬结构快速接入 |
| 主要代价 | 一致性靠人工守护变更点、字段级并发与事件订阅需额外注意、手工模板易错 |

Mods 目录是这套理念在 C# 上的完整落地：从最简单的 `School`（单对象 + 逻辑字段 + 事件失效 + 外部源适配），到进阶的 `Subject`（对象导航 + 多视图投影 + 关系对象），构成了一个层层递进的范本。它的核心资产不是某个类的实现，而是那套"单例对象 + 原地更新 + 变更点刷新"的可复制模式本身。

---

## 附录：理念与代码对照速查

| 理念（README 原文概念） | Mods 代码对应 |
|---|---|
| 字段二分法 | `#region 持久属性` vs `_FileTypeOfStatistical` 等逻辑字段 |
| GetKey = 对象名 + 唯一 ID | `GetPrefixName()`（"SCL"/"SBJ"…）+ `AutoID` |
| 第一次查询丢进全局缓存 | 各实体 `GetByID`：Get 未命中 → 查库 → ToEntity → Set |
| 条件查询先查 ID 再 load | `GetAll` / `GetAllByKeyWord`：reader 只取 autoID，再 GetByID |
| 修改先持久化再改字段 | 各实体 `Update_*`：EntityMaping_Excute 成功后才赋值，无 Set |
| 原地更新（不 new 新对象） | Update 成功直接 `this.X = x`，引用即缓存 |
| 变更点主动刷新 | Insert→Set、Delete→Clear、Update→原地改 |
| 逻辑字段事件驱动失效 | `EntityBase.Evt_EntityChange` 置空 + 懒重算 |
| 外部数据源统一适配 | `School_QST::GetAll` 集合缓存（5 小时 TTL） |
| 对象图遍历 | `Subject.GetMyMember/GetMyRoomClasses/CreateName` |
| 多视图投影 | `Subject_Ext2`（老师视角）/ `Subject_Ext3`（学生视角） |
| 关系也是对象 | `Rel_Subject_RoomClass` / `Rel_RoomClass_Member` |
| 列表加载统一抽象 | `EntityBase.AddToList<T>` |
| NONE 空对象 / 原型 | `public static readonly XxxEntity NONE` |
