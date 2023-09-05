using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Piipan.Shared.Helpers
{
	public static class ObjectProperty
    {
        /// <summary>
        /// Get the Attribute information using reflection
        /// </summary>
        public static string GetDisplayNameAttribute(Type type, string column)
        {
            var property = type.GetProperty(column);
            var displayNameAttributeValue = (property ?? throw new InvalidOperationException()).GetCustomAttributes(typeof(DisplayAttribute), true).Cast<DisplayAttribute>().SingleOrDefault()?.Name;
            return displayNameAttributeValue;
        }


    }
}
