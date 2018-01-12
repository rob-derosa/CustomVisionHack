using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public partial class PredictionListPage : ContentPage
	{
		PredictionListViewModel _viewModel = new PredictionListViewModel();

		public PredictionListPage()
		{
			InitializeComponent();

			//Here we set the BindingContext (each ContentPage has one) to our ViewModel
			//Then we can bind data directly to our UI from our ViewModel - yay!
			BindingContext = _viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			//Lets make a request to get these items if the list is empty
			if(_viewModel.Items.Count == 0)
				_viewModel.LoadItemsCommand.Execute(null);
		}
	}
}
