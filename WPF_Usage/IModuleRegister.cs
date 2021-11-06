using System;
using System.Collections.Generic;
using System.Text;
using Unity;

namespace WPF_Usage
{
    public interface IModuleRegister
    {
        void Register(IUnityContainer container);
    }
}
