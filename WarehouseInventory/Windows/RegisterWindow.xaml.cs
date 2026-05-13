using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class RegisterWindow : Window
{
    public RegisterWindow()
    {
        InitializeComponent();
        var vm = DataContext as RegisterWindow;
        (DataContext as RegisterViewModel)?.SetPasswordBinding(PasswordBox, ConfirmPasswordBox);
        (DataContext as RegisterViewModel)?.SetClose(Close);
    }
}