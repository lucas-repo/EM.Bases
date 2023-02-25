namespace EM.Bases
{
    /// <summary>
    /// 分组元素
    /// </summary>
    public class GroupItem: SelectableItem, IGroupItem
    {
        /// <inheritdoc/>
        public virtual IItemCollection<IBaseItem> Children { get; protected set; }
    }
    /// <summary>
    /// 分组元素
    /// </summary>
    /// <typeparam name="TItem">元素类型</typeparam>
    /// <typeparam name="TChildren">子元素类型</typeparam>
    public class GroupItem<TItem, TChildren> : SelectableItem<TItem>, IGroupItem<TItem, TChildren> where TItem : IGroupItem
    {
        /// <inheritdoc/>
        public virtual IItemCollection<TChildren> Children { get; protected set; }
    }
}