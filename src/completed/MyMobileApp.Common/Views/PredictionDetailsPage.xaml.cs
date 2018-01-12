using System;

using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public partial class PredictionDetailsPage : ContentPage
	{
		PredictionDetailsViewModel _viewModel = new PredictionDetailsViewModel();

		public PredictionDetailsPage()
		{
			BindingContext = _viewModel;
			InitializeComponent();
		}
	}
}
