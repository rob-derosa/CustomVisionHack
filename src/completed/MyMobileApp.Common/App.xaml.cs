using System;

using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			DependencyService.Register<CloudDataStore>();
			MainPage = new MainPage();
		}

		public static string AzureFunctionsUrl;
	}
}
