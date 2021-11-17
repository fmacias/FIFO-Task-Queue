using System;

namespace fmacias.Components.MVPVMModule.Abstract
{
    public interface IModuleRegister
    {
        string GetUnicRegistrationName(Type type);
        void Register<TViewType>();
    }
}