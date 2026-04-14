using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Tools;

public partial class UserManager : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
    private string _username = string.Empty;
    public bool IsLoggedIn => !string.IsNullOrEmpty(Username);
}