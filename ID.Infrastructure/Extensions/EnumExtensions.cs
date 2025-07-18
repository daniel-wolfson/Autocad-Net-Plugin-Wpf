using ID.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web.Mvc;

namespace ID.Infrastructure.Extensions
{
    public static class EnumExtensions
    {
        public static DataInfoAttribute GetDataInfo(this Enum enumValue)
        {
            return enumValue.GetType()
                .GetMember(enumValue.ToString())
                .First()
                .GetCustomAttribute<DataInfoAttribute>();
        }

        /// <summary> GetAttribute </summary>
        public static TAttribute GetAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<TAttribute>();
        }

        public static Dictionary<short, TAttribute> GetAttributes<TAttribute>(this Type enumType)
            where TAttribute : Attribute
        {
            Dictionary<short, TAttribute> _dict = new Dictionary<short, TAttribute>();
            //Type enumType = typeof(TEnum);
            //Type enumUnderlyingType = Enum.GetUnderlyingType(enumType);
            Array enumValues = Enum.GetValues(enumType);
            foreach (Enum enumVal in enumValues)
            {
                _dict.Add((short)(int)(object)enumVal, enumVal.GetAttribute<TAttribute>());
            }
            return _dict;
        }

        public static TAttribute XGetAttribute<TAttribute>(this Enum value) where TAttribute : Attribute
        {
            var enumType = value.GetType();
            var name = Enum.GetName(enumType, value);
            return enumType.GetField(name).GetCustomAttributes(false).OfType<TAttribute>().SingleOrDefault();
        }

        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()
                            .GetName();
        }

        public static string GetDescription(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DescriptionAttribute>()
                            .Description;
        }

        public static T GetDefaultValue<T>(this Enum enumValue)
        {
            return (T)enumValue.GetType()
                    .GetMember(enumValue.ToString())
                    .First()
                    .GetCustomAttribute<DefaultValueAttribute>()
                    .Value;
        }

        public static bool IsBadStatusCode(this HttpStatusCode statusCode)
        {
            return (int)statusCode >= 400 && (int)statusCode <= 499;
        }
        public static bool IsSuccessStatusCode(this HttpStatusCode statusCode)
        {
            return (int)statusCode >= 200 && (int)statusCode <= 299;
        }
        public static bool IsSuccessStatusCode(this int? statusCode)
        {
            statusCode = statusCode ?? 500;
            return statusCode >= 200 && statusCode <= 299;
        }

        public static IEnumerable<SelectListItem> EnumToSelectList<T>()
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new SelectListItem()
                {
                    Text = e.ToString(),
                    Value = Convert.ChangeType(e, typeof(int)).ToString()
                })
                .ToList();
        }

        /// returns the localized Name, if a [Display(Name="Localised Name")] attribute is applied to the enum member
        /// returns null if there isnt an attribute
        public static string DisplayNameOrEnumName(this Enum value)
        // => value.DisplayNameOrDefault() ?? value.ToString()
        {
            // More efficient form of ^ based on http://stackoverflow.com/a/17034624/11635
            var enumType = value.GetType();
            var enumMemberName = Enum.GetName(enumType, value);
            return enumType
                .GetEnumMemberAttribute<DisplayAttribute>(enumMemberName)
                ?.GetName() // Potentially localized
                ?? enumMemberName; // Or fall back to the enum name
        }

        /// returns the localized Name, if a [Display] attribute is applied to the enum member
        /// returns null if there is no attribute
        public static string DisplayNameOrDefault(this Enum value) =>
            value.GetEnumMemberAttribute<DisplayAttribute>()?.GetName();

        static TAttribute GetEnumMemberAttribute<TAttribute>(this Enum value) where TAttribute : Attribute =>
            value.GetType().GetEnumMemberAttribute<TAttribute>(value.ToString());

        static TAttribute GetEnumMemberAttribute<TAttribute>(this Type enumType, string enumMemberName) where TAttribute : Attribute =>
            enumType.GetMember(enumMemberName).Single().GetCustomAttribute<TAttribute>();

    }
}
