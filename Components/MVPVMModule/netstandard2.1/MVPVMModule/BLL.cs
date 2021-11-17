using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.MVPVMModule
{
    abstract public class BLL : IBLL
    {
        private readonly IDAL dal;

        protected BLL(IDAL dal)
        {
            this.dal = dal;
        }

        public IDAL DAL => dal;
    }
}
