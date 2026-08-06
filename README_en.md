# Full English Translation (Code reserved, all text/annotations converted to standard tech English)
## README.md English Version
I originally did not plan to open-source this set of code. However, developers working on Go projects wanted AI to reference my architectural pattern, and feedback has been positive. I’ve decided to share my years of experience, hoping it can bring value in the AI development era.

### Core Mechanism Overview
This architecture adopts a design of **in-process singleton objects + in-place state updates + proactive refresh on data mutations**. It eliminates frequent database queries for read-heavy, write-light datasets, while ensuring thread safety and data consistency via dual-layer locking.

Its core value lies in being a reusable template: any data entity matching the trait "high read frequency, low write frequency, enumerable mutation points" can be quickly integrated by copying this structure. This aligns with the requirement to identify which data objects can be registered into the global cache layer later.

```csharp
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;
using Tools.AccessDB;

namespace MCR.Mods
{
    /// <summary>School Domain Entity (Prefix: SCL)</summary>
    [Serializable]
    public class School : EntityBase
    {
        #region Persistent Database Fields
        private string _Name = string.Empty;
        /// <summary>School Display Name</summary>
        public string Name
        {
            get => _Name;
            set => _Name = value;
        }

        private string _Bind_Key = string.Empty;
        /// <summary>External binding identifier for third-party API school data retrieval</summary>
        public string Bind_Key
        {
            get => _Bind_Key;
            set => _Bind_Key = value;
        }

        private string _Remark = string.Empty;
        /// <summary>Manual Remarks</summary>
        public string Remark
        {
            get => _Remark;
            set => _Remark = value;
        }

        private bool _IsDisable = false;
        /// <summary>Soft Delete / Disabled Flag</summary>
        public bool IsDisable
        {
            get => _IsDisable;
            set => _IsDisable = value;
        }

        private DateTime _CTime = DateTime.Now;
        /// <summary>Record Creation Timestamp</summary>
        public DateTime CTime
        {
            get => _CTime;
            set => _CTime = value;
        }
        #endregion

        protected School()
        {
            EntityBase.Evt_EntityChange += (entityInfo) =>
            {
                if (entityInfo is SourceDocument)
                {
                    _FileTypeOfStatistical = null;
                }
            };
        }

        #region Abstract Base Class Overrides
        public override Type GetTypeBase()
        {
            return typeof(School);
        }

        protected override string GetPrefixName()
        {
            return "SCL";
        }

        protected override void ToEntity(EntityReader reader)
        {
            this.AutoID = reader.GetValue<string>(this, "AutoID");
            this.Name = reader.GetValue<string>(this, "Name");
            this.Remark = reader.GetValue<string>(this, "Remark");
            this.IsDisable = reader.GetValue<bool>(this, "IsDisable");
            this.Bind_Key = reader.GetValue<string>(this, "Bind_Key");
        }
        #endregion

        private Dictionary<string, int> _FileTypeOfStatistical = null;
        /// <summary>Get categorized document count statistics for current school</summary>
        public Dictionary<string, int> GetFileTypeOfStatistical()
        {
            int count = 0;
            if (_FileTypeOfStatistical == null)
            {
                _FileTypeOfStatistical = new Dictionary<string, int>();

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.Courseware);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.Courseware.ToString(), count);

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.Discuss);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.Discuss.ToString(), count);

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.Nofity);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.Nofity.ToString(), count);

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.Question);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.Question.ToString(), count);

                count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.VoteQuestions);
                _FileTypeOfStatistical.Add(VSTO.PPT_FileType.VoteQuestions.ToString(), count);

                //count = SourceDocument.GetSourceCountBySchoolAndFType(this, VSTO.PPT_FileType.NONE);
                //_FileTypeOfStatistical.Add(VSTO.PPT_FileType.NONE.ToString(), count);
            }
            return _FileTypeOfStatistical;
        }

        //==================== Update Mutators ====================
        /// <summary>Update school display name and remarks</summary>
        public Result Update(string name, string remark)
        {
            name = name.Trim();
            if (string.IsNullOrEmpty(name))
            {
                return new Result(false, "Operation aborted: School name cannot be empty");
            }

            ParameterTag[] ps =
            {
                new ParameterTag("@AutoID", this.AutoID, E_DbType.VarChar, 50),
                new ParameterTag("@Name", name, E_DbType.VarChar, 50),
                new ParameterTag("@Remark", remark, E_DbType.VarChar, 200)
            };

            Result rs = this.EntityMaping_Excute("Update", ps);
            if (rs.IsOK)
            {
                this.Name = name;
                this.Remark = remark;
            }
            return rs;
        }

        /// <summary>Toggle school disabled status flag</summary>
        public Result Update_IsDisable(bool isDisable)
        {
            ParameterTag[] ps =
            {
                new ParameterTag("@AutoID", this.AutoID, E_DbType.VarChar, 50),
                new ParameterTag("@IsDisable", isDisable, E_DbType.Bit, 1)
            };

            Result rs = this.EntityMaping_Excute("Update_IsDisable", ps);
            if (rs.IsOK)
            {
                this.IsDisable = isDisable;
            }
            return rs;
        }

        #region Static Global Accessors
        /// <summary>Fallback Null School Instance</summary>
        public static readonly School NONE = new School() { AutoID = "SCL-001", Name = "Default" };

        /// <summary>Load single school entity by unique AutoID</summary>
        public static School GetByID(string autoID)
        {
            if (string.IsNullOrEmpty(autoID))
                return null;

            School the = EntityBase.GetMyICache().Get(autoID) as School;
            if (the == null)
            {
                ParameterTag[] ps =
                {
                    new ParameterTag("@AutoID", autoID, E_DbType.VarChar, 50)
                };
                Result rs = School.NONE.EntityMaping_Excute("GetByID", ps, (readers) =>
                {
                    if (readers.Count > 0)
                    {
                        the = new School();
                        the.ToEntity(readers[0]);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                });
            }
            return the;
        }

        /// <summary>Retrieve full collection of all school entities</summary>
        public static IList<School> GetAll()
        {
            List<School> list = new List<School>();
            Result rs = School.NONE.EntityMaping_Excute("GetList_All", null, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string autoID = r.GetValue(0).ToString();
                    School the = EntityBase.GetMyICache().Get(autoID) as School;
                    if (the == null)
                    {
                        the = new School();
                        the.ToEntity(r);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    list.Add(the);
                }
            });
            return list;
        }

        /// <summary>Paginated filtered school search with keyword matching</summary>
        public static IList<School> GetAllByKeyWord(string schoolName, string schoolEmail, string schoolPhone, int pageSize, int pageNo, ref int totalCount, int disable = -1)
        {
            List<School> list = new List<School>();
            string keyWord = string.Empty;

            if (!string.IsNullOrEmpty(schoolName))
            {
                keyWord += " And Name like '% " + schoolName + " %' ";
            }
            if (!string.IsNullOrEmpty(schoolEmail))
            {
                keyWord += " And Email like '% " + schoolEmail + " %' ";
            }
            if (!string.IsNullOrEmpty(schoolPhone))
            {
                keyWord += " And Phone like '% " + schoolPhone + " %' ";
            }
            if (disable > -1)
            {
                keyWord += " And IsDisable = " + disable;
            }

            ParameterTag[] ps =
            {
                new ParameterTag("@keyWord", keyWord, E_DbType.VarChar, 500),
                new ParameterTag("@pageSize", pageSize, E_DbType.Int, 4),
                new ParameterTag("@pageNo", pageNo, E_DbType.Int, 4)
            };

            Result rs = School.NONE.EntityMaping_Excute("GetAllByKeyWord", ps, (readers) =>
            {
                foreach (EntityReader r in readers)
                {
                    string autoID = r.GetValue(0).ToString();
                    School the = GetByID(autoID);
                    if (the == null)
                    {
                        the = new School();
                        the.ToEntity(r);
                        EntityBase.GetMyICache().Set(the.AutoID, the);
                    }
                    if (the != null)
                    {
                        list.Add(the);
                    }
                }
            });

            int sum = 0;
            Result rs2 = School.NONE.EntityMaping_Excute("GetAllByKeyWord", ps, (readers) =>
            {
                if (readers.Count > 0)
                {
                    sum = Convert.ToInt32(readers[0]);
                }
            });
            totalCount = sum;
            return list;
        }

        /// <summary>Create new school record & persist to database</summary>
        public static Result Insert(string name, string remark, string bind_key = "")
        {
            name = name.Trim();
            if (string.IsNullOrEmpty(name))
            {
                return new Result(false, "Operation aborted: School name cannot be empty");
            }

            School theNew = new School();
            theNew.Name = name;
            theNew.Remark = remark;
            theNew.Bind_Key = bind_key;

            ParameterTag[] ps =
            {
                new ParameterTag("@AutoID", theNew.AutoID, E_DbType.VarChar, 50),
                new ParameterTag("@Name", theNew.Name, E_DbType.VarChar, 50),
                new ParameterTag("@CTime", theNew.CTime, E_DbType.DateTime, 0),
                new ParameterTag("@Bind_Key", theNew.Bind_Key, E_DbType.VarChar, 50),
                new ParameterTag("@IsDisable", theNew.IsDisable, E_DbType.Bit, 1),
                new ParameterTag("@Remark", theNew.Remark, E_DbType.VarChar, 200)
            };

            Result rs = theNew.EntityMaping_Excute("Insert", ps);
            if (rs.IsOK)
            {
                rs.Data = theNew;
                EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
            }
            return rs;
        }

        /// <summary>Delete school entity & evict from global cache</summary>
        public static Result Delete(School the)
        {
            ParameterTag[] ps =
            {
                new ParameterTag("@AutoID", the.AutoID, E_DbType.VarChar, 50)
            };

            Result rs = the.EntityMaping_Excute("Delete", ps);
            if (rs.IsOK)
            {
                GetMyICache().Clear(the.AutoID);
            }
            return rs;
        }
        #endregion
    }

    /// <summary>External third-party QST API School Data Adapter Subclass</summary>
    public class School_QST : School
    {
        protected School_QST() { }

        /// <summary>Cache-wrapped full school list from QST external API (5-hour TTL)</summary>
        public static IList<School> GetAll()
        {
            List<School> list = EntityBase.GetMyICache().Get("School_QST::GetAll") as List<School>;
            if (list == null)
            {
                list = new List<School>();
                Dictionary<string, object> dicData = QST_Interface.GetList_School();
                if (!dicData.ContainsKey("ERR") && dicData["code"].ToString() == "200")
                {
                    IList infos = dicData["data"] as IList;
                    foreach (object obj in infos)
                    {
                        Dictionary<string, object> info = obj as Dictionary<string, object>;
                        if (info != null)
                        {
                            School_QST theSchool = new School_QST();
                            theSchool.AutoID = info["id"].ToString();
                            theSchool.Bind_Key = info["code"].ToString();
                            theSchool.CTime = Convert.ToDateTime(info["createTime"]);
                            theSchool.Name = info["name"].ToString();
                            theSchool.Remark = info["remark"].ToString();
                            theSchool.IsDisable = true;
                            list.Add(theSchool);
                        }
                    }
                    EntityBase.GetMyICache().Set("School_QST::GetAll", list, DateTime.Now.AddHours(5));
                }
            }
            return list;
        }

        /// <summary>Fetch single school entity from QST API by external ID</summary>
        public static School GetFindID_QST(string id)
        {
            School_QST theSchool = null;
            Dictionary<string, object> dicData = QST_Interface.GetSchoolByID(id);
            if (!dicData.ContainsKey("ERR") && dicData["code"].ToString() == "200")
            {
                Dictionary<string, object> info = dicData["data"] as Dictionary<string, object>;
                if (info != null)
                {
                    theSchool = new School_QST();
                    theSchool.AutoID = info["id"].ToString();
                    theSchool.Bind_Key = info["code"].ToString();
                    theSchool.CTime = Convert.ToDateTime(info["createTime"]);
                    theSchool.Name = info["name"].ToString();
                    theSchool.Remark = info["remark"].ToString();
                    theSchool.IsDisable = true;
                    return theSchool;
                }
            }

            if (theSchool == null)
            {
                IList<School> list = GetAll();
                foreach (School the in list)
                {
                    if (the.AutoID == id)
                        return the;
                }
            }
            return null;
        }
    }
}
```

---
## AI Analysis Section English Version
### AI Full Analysis of This Domain Cache Architecture Sample
This `School` C# class fully implements your domain caching architectural philosophy. Below is a breakdown of its core design mechanisms:
1. **Inherit from `EntityBase` Base Class**
`EntityBase` provides unified global cache infrastructure via `GetMyICache()`.

2. **Dual Partition Field Design Pattern**
- Persistent fields: `Name`, `Bind_Key`, `Remark`, `IsDisable`, `CTime` — persisted to relational database tables.
- Derived logical fields: `_FileTypeOfStatistical` — runtime statistics, never saved to DB, lazy loaded on first access.

3. **Cache Key Generation Rule**
- `GetPrefixName()` returns entity type prefix `"SCL"`
- Unique identity: `AutoID`
- Final cache key format: `{Prefix}-{AutoID}` e.g. `SCL-001`
Matches your rule: combine entity type identifier + unique primary ID as global cache lookup key.

4. **Single Entity Lazy Load (`GetByID`)**
```csharp
School the = EntityBase.GetMyICache().Get(autoID) as School;
if (the == null)
{
    // Query database for missing entity
    // Map database row to domain object
    EntityBase.GetMyICache().Set(the.AutoID, the);
}
return the;
```
Implements your rule: load entity from DB & insert into global cache on first miss.

5. **Batched List Loading (`GetAll`)**
```csharp
foreach (EntityReader r in readers)
{
    string autoID = r.GetValue(0).ToString();
    School the = EntityBase.GetMyICache().Get(autoID) as School;
    if (the == null)
    {
        the = new School();
        the.ToEntity(r);
        EntityBase.GetMyICache().Set(the.AutoID, the);
    }
    list.Add(the);
}
```
Follows your specification: filter queries only fetch primary ID column from DB, then fully hydrate entities via cache lookup.

6. **Paginated Filter Query (`GetAllByKeyWord`)**
Explicitly calls `GetByID(autoID)` for each primary ID returned by SQL, pure implementation of "fetch ID first, resolve full entity via cache" logic.

7. Mutate Workflow: Persist First, In-Place Object Update
```csharp
public Result Update(string name, string remark)
{
    // Validate input parameters
    Result rs = this.EntityMaping_Excute("Update", ps); // Write changes to DB first
    if (rs.IsOK)
    {
        this.Name = name;
        this.Remark = remark;
    }
    return rs;
}
```
Aligns with your workflow: retrieve cached entity by key, validate input parameters, persist changes to database, then mutate object fields directly.
Critical optimization: no extra `Set()` cache call required. The variable `this` references the live cached singleton instance — field edits modify cache state directly (in-place update).

8. Create Workflow: Insert DB then Cache Upsert
```csharp
if (rs.IsOK)
{
    rs.Data = theNew;
    EntityBase.GetMyICache().Set(theNew.AutoID, theNew);
}
```
9. Delete Workflow: Remove Cache After DB Erase
```csharp
if (rs.IsOK)
{
    GetMyICache().Clear(the.AutoID);
}
```
10. Event-Derived Logical Field Invalidation
```csharp
protected School()
{
    EntityBase.Evt_EntityChange += (entityInfo) =>
    {
        if (entityInfo is SourceDocument)
        {
            _FileTypeOfStatistical = null;
        }
    };
}
```
Elegant event pattern: when dependent `SourceDocument` entities mutate, mark local statistical cache as null to trigger recalculation on next read.

11. `School_QST`: External API Data Adapter Subclass
```csharp
EntityBase.GetMyICache().Set("School_QST::GetAll", list, DateTime.Now.AddHours(5));
```
Extends the unified cache abstraction to third-party HTTP API data with fixed 5-hour TTL; business logic consumes external API data via identical `GetByID` / `GetAll` interfaces as database entities.

12. Null Object Pattern Static Instance
```csharp
public static readonly School NONE = new School() { AutoID = "SCL-001", Name = "Default" };
```
Fallback empty entity used for executing generic database mapping logic without null reference exceptions.

### Overall Design Review
This implementation perfectly materializes your caching architecture, with four standout innovative patterns:
1. Event-driven lazy invalidation for computed statistics
2. Null object fallback singleton
3. Universal cache abstraction supporting both SQL and external API data sources
4. Reference-stable singleton design enabling zero-overhead in-place cache mutation (no repeated cache write calls)

### Mapping Between Your C# Architecture & Go Implementation
| Design Dimension | C# `EntityBase` System | Current Go Cache Implementation |
|------------------|------------------------|---------------------------------|
| Global Cache Store | `GetMyICache()` | `configStore` / `deviceOrderCountStore` |
| Cache Key Generation | Prefix + AutoID string | Hardcoded string keys per domain type |
| Live Reference Update | Managed object references | Pointer structs + internal mutex locks |
| Derived Stat Invalidation | Event trigger set-to-null | Incremental counter updates |
| Filtered List Loading | Generic base abstraction fetch IDs first | Separate duplicated logic per cache type |

Major gap: Your C# code abstracts list loading logic into reusable base methods shared across all entities, while the Go codebase duplicates cache read/write logic for every distinct data type without a shared generic `EntityBase` equivalent. For future Go expansion with more domain models, building a generic domain cache base layer matching this pattern will eliminate repetitive boilerplate.

### Minor Code Observation
```csharp
School the = GetByID(autoID);
if (the == null)
{
    the.ToEntity(r);
    EntityBase.GetMyICache().Set(the.AutoID, the);
}
```
Potential null reference risk: If `GetByID()` returns null, invoking `the.ToEntity()` will throw an exception. This branch is theoretically unreachable in valid runtime flow due to internal cache hydration logic inside `GetByID()`.

---
## Extended Analysis (Subject Multi-View Entity Pattern)
The subsequent `Subject` entity code exposes additional advanced layers beyond the basic `School` implementation:
### 1. Domain Object Graph Navigation
```csharp
public WX_Member GetMyMember() { return WX_Member.GetByID(this.MemberID); }
public IList<RoomClass> GetMyRoomClasses() { return RoomClass.GetListBySubject(this); }
public string CreateName { get { return this.GetMyMember()?.Name; } }
```
Pure DDD Aggregate Root navigation design. All cross-entity lookups resolve via cached `GetByID()` calls, avoiding repeated JOIN database queries and reducing graph traversal latency drastically compared to standard ORM lazy loading.

### 2. Extended View Subclasses (`Subject_Ext2` / `Subject_Ext3`)
This multi-view projection pattern is the most sophisticated upgrade:
- `Subject_Ext2`: Teacher business view with associated classroom collection
- `Subject_Ext3`: Student business view with aggregated JOIN statistics (`CourseCount`, `NotDoneCourse`)
```csharp
protected override void ToEntity(EntityReader reader)
{
    base.ToEntity(reader);
    this.RoomClassID = reader.GetValue("RoomClassID").ToString();
    this.CourseCount = Convert.ToInt32(reader.GetValue("CourseCount"));
}
```
Core separation rule: Base domain entities (single-table columns) are cached long-term; extended projection objects with JOIN aggregated stats are constructed per query and never persisted to cache, as their derived values are volatile.

### 3. Generic List Hydration Factory Method
```csharp
EntityBase.AddToList<Subject>(list, readers, (r) => new Subject_Ext2(), (theAdd, reader2) => {...});
```
`EntityBase` exposes a generic utility accepting factory delegates for entity construction & row mapping, eliminating duplicated loop/hydrate logic across all domain list APIs.

### 4. Many-to-Many Relationships Modelled as Independent Entities
```csharp
Rel_Subject_RoomClass.Insert(this, theRoomClass);
Rel_Subject_RoomClass.GetByID(this, theRoomClass);
```
Cross-table junction tables are implemented as full cache-backed entities rather than raw SQL JOINs, enabling cached relationship lookups without repeated relational queries.

### Updated High-Level Architecture Definition
This is not merely a simple row cache layer — it is a **cache-first domain modeling framework**:
Domain objects encapsulate business rules, cross-entity graph navigation, and multi-view projections, with the global cache layer removing database round-trips for object graph traversal.
Ordinary key-value caching optimizes single-row reads only; your architecture optimizes full domain graph traversal performance via unified entity caching rules enforced by the base class.

### Confirmation Question
You infer extended projection classes (`Subject_Ext2`/`Subject_Ext3`) are never cached, only base single-table entities persist in memory cache — is this design choice intentional in your system?