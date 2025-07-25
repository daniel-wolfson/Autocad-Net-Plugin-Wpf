﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace ID.AcadNet.Infrastructure.Core
{
    public class RowDefinitionExtended : RowDefinition
    {
        // Variables
        private static readonly DependencyProperty VisibleProperty;

        // Properties
        private Boolean Visible
        {
            get => (Boolean)GetValue(VisibleProperty);
            set => SetValue(VisibleProperty, value);
        }

        // Constructors
        static RowDefinitionExtended()
        {
            VisibleProperty = DependencyProperty.Register("Visible",
                typeof(Boolean),
                typeof(RowDefinitionExtended),
                new PropertyMetadata(true, OnVisibleChanged));

            RowDefinition.HeightProperty.OverrideMetadata(typeof(RowDefinitionExtended),
                new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null,
                    new CoerceValueCallback(CoerceWidth)));

            RowDefinition.MinHeightProperty.OverrideMetadata(typeof(RowDefinitionExtended),
                new FrameworkPropertyMetadata((Double)0, null,
                    new CoerceValueCallback(CoerceMinWidth)));
        }

        // Get/Set
        public static void SetVisible(DependencyObject obj, Boolean nVisible)
        {
            obj.SetValue(VisibleProperty, nVisible);
        }
        public static Boolean GetVisible(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(VisibleProperty);
        }

        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            obj.CoerceValue(RowDefinition.HeightProperty);
            obj.CoerceValue(RowDefinition.MinHeightProperty);
        }
        static Object CoerceWidth(DependencyObject obj, Object nValue)
        {
            return ((RowDefinitionExtended)obj).Visible ? nValue : new GridLength(0);
        }
        static Object CoerceMinWidth(DependencyObject obj, Object nValue)
        {
            return ((RowDefinitionExtended)obj).Visible ? nValue : (Double)0;
        }
    }
}
