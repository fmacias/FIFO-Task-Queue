using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace FifoTaskQueue
{
    public class Operations
    {
        public static bool IsAsync(Action action)
        {
            return IsAsycn(action);
        }
        public static bool RefuseAsync(Action<object> action)
        {
            return IsAsycn(action);
        }
        private static bool IsAsycn(Action<object> action)
        {
            return action.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
        }
        private static bool IsAsycn(Action action)
        {
            return action.Method.IsDefined(typeof(AsyncStateMachineAttribute), false);
        }
    }
}
