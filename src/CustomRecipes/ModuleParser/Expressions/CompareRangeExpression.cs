using System;
using System.Collections.Generic;

namespace CustomRecipes.Python
{
    public class CompareRangeExpression
    {
        private CompareRangeExpression() { }

        public static IExpression CreateFrom(IExpression left, Type leftOperator, IExpression middle, Type rightOperator, IExpression right)
        {
            return new AndExpression(
                Create(leftOperator, left, middle),
                Create(rightOperator, middle, right)
            );
        }


        private static IExpression Create(Type op, IExpression left, IExpression right)
        {
            var constructor = op.GetConstructor(new Type[] { typeof(IExpression), typeof(IExpression) });
            return (IExpression)constructor.Invoke(new object[] { left, right });
        }
    }
}