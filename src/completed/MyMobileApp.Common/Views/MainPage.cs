using System;

using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public class MainPage : TabbedPage
	{
		public MainPage()
		{
			var predictPage = new NavigationPage(new PredictionDetailsPage())
			{
				Title = "Predict"
			};

			var itemsPage = new NavigationPage(new PredictionListPage())
			{
				Title = "History"
			};

			var settingsPage = new NavigationPage(new SettingsPage())
			{
				Title = "Settings"
			};

			switch(Device.RuntimePlatform)
			{
				case Device.iOS:
					itemsPage.Icon = "tab_feed.png";
					predictPage.Icon = "tab_about.png";
					settingsPage.Icon = "tab_settings.png";
					break;
			}

			Children.Add(predictPage);
			Children.Add(itemsPage);
			Children.Add(settingsPage);

			Title = Children[0].Title;
		}

		protected override void OnCurrentPageChanged()
		{
			base.OnCurrentPageChanged();
			Title = CurrentPage?.Title ?? string.Empty;
		}
	}
}
