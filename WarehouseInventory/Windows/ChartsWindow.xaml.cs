using System.Windows;
using WarehouseInventory.ViewModels;

namespace WarehouseInventory.Windows;

public partial class ChartsWindow : Window
{
    public ChartsWindow()
    {
        InitializeComponent();
        var vm = DataContext as ChartsViewModel;
        vm.SetCurrentWindow(this);
    }
}