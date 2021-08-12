﻿using CJG.Core.Entities;
using CJG.Web.External.Areas.Ext.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;

namespace CJG.Web.External.Helpers.Converters
{
    public class UnderRepresentedGroupsTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (typeof(IEnumerable<UnderRepresentedGroupSelectionViewModel>).IsAssignableFrom(sourceType))
                return true;

            return base.CanConvertFrom(context, sourceType);
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (typeof(IEnumerable<UnderRepresentedGroup>).IsAssignableFrom(destinationType))
                return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
                return null;

            if (typeof(IEnumerable<UnderRepresentedGroup>).IsAssignableFrom(destinationType)
                && typeof(IEnumerable<UnderRepresentedGroupSelectionViewModel>).IsAssignableFrom(value.GetType()))
            {
                var results = new List<UnderRepresentedGroup>();

                var source = (IEnumerable)value;

                foreach (UnderRepresentedGroupSelectionViewModel item in source)
                {
                    if (item.IsSelected)
                        results.Add(new UnderRepresentedGroup() { Id = item.Id });
                }

                return results;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}