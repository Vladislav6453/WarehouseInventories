using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class MovementsWindow : Window
{
    public MovementsWindow()
    {
        InitializeComponent();
        var vm = DataContext as MovementViewModel;
        vm.SetCurrentWindow(this);
    }
}