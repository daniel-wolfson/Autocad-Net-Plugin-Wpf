// Copyright (c) Microsoft Corporation. All rights reserved. See License.txt in the project root for license information.

using System.Windows;
using System.Windows.Controls;

namespace Intellidesk.AcadNet.Components
{
    /// <summary>
    /// Interaction logic for CustomPopupView.xaml
    /// This view will inherit the DataContext of the hosting Window, which will be the notification passed
    /// as a parameter in the InteractionRequest.
    /// </summary>
    public partial class CustomPopupView : UserControl
    {
        public CustomPopupView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
        }

    }
}
