using System;

namespace fmacias.Components.MVPVMModule
{
    /// <summary>
    /// Business Logik Layer
    /// </summary>
    public interface IBLL
    {
        IDAL DAL { get; }
    }
}
