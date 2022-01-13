using System.Collections.Generic;

using EFCoreTable = Panosen.CodeDom.EFCore.Table;

namespace EFGen.Generation
{
    /// <summary>
    /// 上下文
    /// </summary>
    public class Context
    {
        public Param Param { get; set; }

        public Dictionary<string, EFCoreTable> TableMap { get; set; }
    }
}
