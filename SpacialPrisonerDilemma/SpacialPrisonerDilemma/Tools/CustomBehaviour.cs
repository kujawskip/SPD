using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SpacialPrisonerDilemma.Tools
{
    class CustomBehaviour : DependencyObject
    {

        public static readonly DependencyProperty OnMouseEnterProperty =
            DependencyProperty.RegisterAttached("OnMouseEnterProperty", typeof(ICommand), typeof(CustomBehaviour), new PropertyMetadata(null, OnMouseEnterPropertyChanged));

        public static ICommand GetOnMouseEnterProperty(DependencyObject e)
        {
            return (ICommand)e.GetValue(OnMouseEnterProperty);
        }

        public static void SetOnMouseEnterProperty(UIElement e, ICommand value)
        {
            e.SetValue(OnMouseEnterProperty, value);
        }

        private static void OnMouseEnterPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ui = d as UIElement;
            if (ui == null) return; // albo throw exception
            if (e.OldValue != null)
            {
                ui.MouseEnter -= HandleMouseEnter;
            }
            if (e.NewValue != null)
            {
                ui.MouseEnter += HandleMouseEnter;
            }
        }

        private static void HandleMouseEnter(object sender, MouseEventArgs e)
        {
            var command = GetOnMouseEnterProperty((DependencyObject)sender);
            if (command != null) command.Execute(e);
        }
    }
}
