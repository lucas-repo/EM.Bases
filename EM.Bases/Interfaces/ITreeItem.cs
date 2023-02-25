using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace EM.Bases
{
    /// <summary>
    /// 树节点元素接口
    /// </summary>
    public interface ITreeItem : IGroupItem, IParent<ITreeItem>, ItreeItemInfo
    {
    }
}
