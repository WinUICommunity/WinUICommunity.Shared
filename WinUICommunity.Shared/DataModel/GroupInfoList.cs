﻿using System.Collections.Generic;

namespace WinUICommunity.Shared.DataModel;
public class GroupInfoList : List<object>
{
    public GroupInfoList(IEnumerable<object> items) : base(items) { }

    public object Key { get; set; }

    public string Title { get; set; }

    public override string ToString()
    {
        return Title;
    }
}
