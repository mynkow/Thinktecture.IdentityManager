﻿/*
 * Copyright (c) Dominick Baier, Brock Allen.  All rights reserved.
 * see license
 */
using System;
using System.Collections.Generic;
using System.Linq;
using Thinktecture.IdentityManager.Resources;

namespace Thinktecture.IdentityManager
{
    public static class PropertyMetadataExtensions
    {
        public static IEnumerable<string> Validate(
            this IEnumerable<PropertyMetadata> propertiesMetadata, 
            IEnumerable<Property> properties)
        {
            if (propertiesMetadata == null) throw new ArgumentNullException("propertiesMetadata");
            properties = properties ?? Enumerable.Empty<Property>();

            var errors = new List<string>();

            var crossQuery =
                from m in propertiesMetadata
                from p in properties
                where m.Type == p.Type
                let e = m.Validate(p.Value)
                where e != null
                select e;
            
            errors.AddRange(crossQuery);

            var metaTypes = propertiesMetadata.Select(x => x.Type);
            var propTypes = properties.Select(x => x.Type);
            
            var more = propTypes.Except(metaTypes);
            if (more.Any())
            {
                var types = more.Aggregate((x, y) => x + ", " + y);
                errors.Add(String.Format(Messages.UnrecognizedProperties, types));
            }

            var less = metaTypes.Except(propTypes);
            if (less.Any())
            {
                var types = less.Aggregate((x,y)=>x + ", " + y);
                errors.Add(String.Format(Messages.MissingRequiredProperties, types));
            }
            
            return errors;
        }

        public static string Validate(this PropertyMetadata property, string value)
        {
            if (property == null) throw new ArgumentNullException("property");

            if (property.Required && String.IsNullOrWhiteSpace(value))
            {
                return String.Format(Messages.IsRequired, property.Name);
            }
            else if (!String.IsNullOrWhiteSpace(value))
            {
                if (property.DataType == PropertyDataType.Boolean)
                {
                    bool val;
                    if (!Boolean.TryParse(value, out val))
                    {
                        return String.Format(Messages.InvalidBoolean, property.Name);
                    }
                }

                if (property.DataType == PropertyDataType.Email)
                {
                    if (!value.Contains("@"))
                    {
                        return String.Format(Messages.InvalidEmail, property.Name);
                    }
                }

                if (property.DataType == PropertyDataType.Number)
                {
                    double d;
                    if (!Double.TryParse(value, out d))
                    {
                        return String.Format(Messages.InvalidNumber, property.Name);
                    }
                }

                if (property.DataType == PropertyDataType.Url)
                {
                    Uri uri;
                    if (!Uri.TryCreate(value, UriKind.Absolute, out uri) ||
                        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    {
                        return String.Format(Messages.InvalidUrl, property.Name);
                    }
                }
            }

            return null;
        }

        public static bool TrySet(this IEnumerable<PropertyMetadata> properties, object instance, string type, string value)
        {
            if (properties == null) throw new ArgumentNullException("properties");

            var executableProperty = properties.Where(x => x.Type == type).SingleOrDefault() as ExecutablePropertyMetadata;
            if (executableProperty != null)
            {
                return executableProperty.TrySet(instance, value);
            }
            
            return false;
        }

        public static bool TrySet(this PropertyMetadata property, object instance, string value)
        {
            if (property == null) throw new ArgumentNullException("property");

            var executableProperty = property as ExecutablePropertyMetadata;
            if (executableProperty != null)
            {
                executableProperty.Set(instance, value);
                return true;
            }

            return false;
        }

        public static bool TryGet(this PropertyMetadata property, object instance, out string value)
        {
            if (property == null) throw new ArgumentNullException("property");

            var executableProperty = property as ExecutablePropertyMetadata;
            if (executableProperty != null)
            {
                value = executableProperty.Get(instance);
                return true;
            }

            value = null;
            return false;
        }

        public static object Convert(this PropertyMetadata property, string value)
        {
            if (property == null) throw new ArgumentNullException("property");
            
            if (String.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            if (property.DataType == PropertyDataType.Boolean)
            {
                return Boolean.Parse(value);
            }

            if (property.DataType == PropertyDataType.Number)
            {
                return Double.Parse(value);
            }

            return value;
        }
    }
}
