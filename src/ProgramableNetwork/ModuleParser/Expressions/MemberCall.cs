using System;
using System.Linq.Expressions;
using System.Reflection;

namespace ProgramableNetwork.Python
{
    public class MemberCall
    {
        private object value;
        private MethodInfo[] memberInfos;

        public MemberCall(object value, MethodInfo[] memberInfos)
        {
            this.value = value;
            this.memberInfos = memberInfos;
        }

        public MethodInfo[] Type => memberInfos;
        public object Target => value;

        internal static object Create(object value, MethodInfo[] methodInfos)
        {
            return new MemberCall(value, methodInfos);
        }
    }
}