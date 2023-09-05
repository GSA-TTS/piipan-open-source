using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Piipan.Components.Helpers
{
    /// <summary>
    /// Helper methods for evaluating whether or not a field has an attribute, and receiving that attribute
    /// </summary>
    public static class ExpressionHelpers
    {
        /// <summary>
        /// Gets an attribute from the property, if it exists
        /// </summary>
        /// <typeparam name="T">The underlying property type</typeparam>
        /// <typeparam name="A">The attribute we are searching for</typeparam>
        /// <param name="expression">An expression for the underlying property</param>
        /// <returns>The attribute we are checking for</returns>
        /// <exception cref="InvalidOperationException">Thrown if Expression is not a member expression</exception>
        public static string GetExpressionName<T>(this Expression<Func<T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new InvalidOperationException("Expression must be a member expression");

            StringBuilder sb = new StringBuilder();
            sb.Append(memberExpression.Member.Name);
            var currentMemberExpression = memberExpression.Expression as MemberExpression;
            while (currentMemberExpression != null)
            {
                sb.Insert(0, currentMemberExpression.Member.Name + "_");
                currentMemberExpression = currentMemberExpression.Expression as MemberExpression;
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets an attribute from the property, if it exists
        /// </summary>
        /// <typeparam name="T">The underlying property type</typeparam>
        /// <typeparam name="A">The attribute we are searching for</typeparam>
        /// <param name="expression">An expression for the underlying property</param>
        /// <returns>The attribute we are checking for</returns>
        /// <exception cref="InvalidOperationException">Thrown if Expression is not a member expression</exception>
        public static A GetAttribute<T, A>(this Expression<Func<T>> expression) where A : Attribute
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new InvalidOperationException("Expression must be a member expression");

            return memberExpression.Member.GetAttribute<A>();
        }

        /// <summary>
        /// Checks to see if an attribute exists on the property
        /// </summary>
        /// <typeparam name="T">The underlying property type</typeparam>
        /// <typeparam name="A">The attribute we are searching for</typeparam>
        /// <param name="expression">An expression for the underlying property</param>
        /// <returns>True if the attribute exists on the property, otherwise false</returns>
        /// <exception cref="InvalidOperationException">Thrown if Expression is not a member expression</exception>
        public static bool HasAttribute<T, A>(this Expression<Func<T>> expression) where A : Attribute
        {
            return GetAttribute<T, A>(expression) != null;
        }

        private static T GetAttribute<T>(this ICustomAttributeProvider provider)
            where T : Attribute
        {
            var attributes = provider.GetCustomAttributes(typeof(T), true);
            return attributes.Length > 0 ? attributes[0] as T : null;
        }
    }
}
