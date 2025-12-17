using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vision_Edit.ViewModels;

namespace Vision_Edit.Views;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage(RegistrationViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}