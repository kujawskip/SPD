﻿#pragma checksum "..\..\..\View\InitialCondition.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "7517F45A2FA3DCD2069142107418D426"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace SpacialPrisonerDilemma.View {
    
    
    /// <summary>
    /// InitialCondition
    /// </summary>
    public partial class InitialCondition : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 6 "..\..\..\View\InitialCondition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas Canvas;
        
        #line default
        #line hidden
        
        
        #line 7 "..\..\..\View\InitialCondition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.CheckBox ChangeCell;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\..\View\InitialCondition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas Legend;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\View\InitialCondition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider RandomSize;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\View\InitialCondition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComboBox;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\View\InitialCondition.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox ComboBox_Copy;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SpacialPrisonerDilemma;component/view/initialcondition.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\InitialCondition.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.Canvas = ((System.Windows.Controls.Canvas)(target));
            
            #line 6 "..\..\..\View\InitialCondition.xaml"
            this.Canvas.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.Canvas_OnMouseWheel);
            
            #line default
            #line hidden
            
            #line 6 "..\..\..\View\InitialCondition.xaml"
            this.Canvas.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.Canvas_OnMouseDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.ChangeCell = ((System.Windows.Controls.CheckBox)(target));
            return;
            case 3:
            
            #line 8 "..\..\..\View\InitialCondition.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 9 "..\..\..\View\InitialCondition.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.Button_Click_1);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Legend = ((System.Windows.Controls.Canvas)(target));
            return;
            case 6:
            this.RandomSize = ((System.Windows.Controls.Slider)(target));
            return;
            case 7:
            this.ComboBox = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 8:
            this.ComboBox_Copy = ((System.Windows.Controls.ComboBox)(target));
            
            #line 16 "..\..\..\View\InitialCondition.xaml"
            this.ComboBox_Copy.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.ComboBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

