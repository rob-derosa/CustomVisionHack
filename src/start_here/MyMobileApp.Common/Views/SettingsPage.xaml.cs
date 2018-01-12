using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public partial class SettingsPage : ContentPage
	{
		SettingsViewModel _viewModel = new SettingsViewModel();
		public SettingsPage()
		{
			InitializeComponent();
			BindingContext = _viewModel;
		}
	}
}
