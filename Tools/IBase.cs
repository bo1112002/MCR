
namespace Tools
{
    /// <summary>所有业务对象的基接口</summary>
    public interface IBase
    {
        /// <summary>验证</summary>
        Result Validate(ObjectChangedTag tag);
    }

    /// <summary>对象发生改变时的标识</summary>
    public enum ObjectChangedTag
    {
        [EnumDescription("无效")]
        None,
        [EnumDescription("创建")]
        Insert,
        [EnumDescription("修改")]
        Update,
        [EnumDescription("删除")]
        Delete,
        [EnumDescription("临时对象")]
        Temp,
    }



}
