using System.Windows;
using System.Windows.Input;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        var vm = DataContext as LoginViewModel;
        (DataContext as LoginViewModel)?.SetPasswordBinding(PasswordBox);
        (DataContext as LoginViewModel)?.SetClose(Close);
        vm.OnLoginSuccess += () =>
        {
            
            var mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        };
    }
}