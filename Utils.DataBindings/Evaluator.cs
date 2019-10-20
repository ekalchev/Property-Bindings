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
        public static bool TryEvalExpression(Expression expr, out object result)
        {
            if(expr.NodeType == ExpressionType.MemberAccess)
            {
                var value = TryEvalExpression(((MemberExpression)expr).Expression, out var res);
                if(value == false || res == null)
                {
                    result = null;
                    return false;
                }
            }
            //
            // Easy case
            //
            if (expr.NodeType == ExpressionType.Constant)
            {
                result = ((ConstantExpression)expr).Value;
                return true;
            }

            //
            // General case
            //
            // Debug.WriteLine ("WARNING EVAL COMPILED {0}", expr);
            var lambda = Expression.Lambda(expr, Enumerable.Empty<ParameterExpression>());

            result = lambda.Compile().DynamicInvoke();
            return true;
        }
    }
}
