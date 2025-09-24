using ListaComprasApp.ViewModels;

namespace ListaComprasApp.Views;

public partial class ListaDetalhesPage : ContentPage
{
    public ListaDetalhesPage(ListaDetalhesViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private void OnCheckBoxTapped(object sender, EventArgs e)
    {
        if (sender is Label label && label.Parent is StackLayout parent)
        {
            var checkBox = parent.Children.OfType<CheckBox>().FirstOrDefault();
            if (checkBox != null)
            {
                checkBox.IsChecked = !checkBox.IsChecked;
            }
        }
    }
}