using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Utils.DataBindings
{
    /// <summary>
    /// Methods that can evaluate Linq expressions.
    /// </summary>
    static class Evaluator
    {
        /// <summary>
        /// Gets the value of a Linq expression.
        /// </summary>
        /// <param name="expr">The expresssion.</param>
        public static object EvalExpression(Expression expr)
        {
            if(expr.NodeType == ExpressionType.MemberAccess)
            {
                var value = EvalExpression(((MemberExpression)expr).Expression);
                if(value == null)
                {
                    return null;
                }
            }
            //
            // Easy case
            //
            if (expr.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expr).Value;
            }

            //
            // General case
            //
            // Debug.WriteLine ("WARNING EVAL COMPILED {0}", expr);
            var lambda = Expression.Lambda(expr, Enumerable.Empty<ParameterExpression>());

            return lambda.Compile().DynamicInvoke();
        }
    }
}
