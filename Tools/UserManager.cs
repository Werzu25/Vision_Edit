using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tools;

public partial class UserManager : ObservableObject
{
    [ObservableProperty]
    private string _username;

    public bool IsLoggedIn => !string.IsNullOrEmpty(Username);
}