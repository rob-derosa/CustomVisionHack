using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public class SettingsViewModel : BaseViewModel
	{
		public SettingsViewModel()
		{
			if(App.Current.Properties.ContainsKey(nameof(AzureFunctionsUrl)))
				AzureFunctionsUrl = App.AzureFunctionsUrl = (string)App.Current.Properties[nameof(AzureFunctionsUrl)];
		}

		public ICommand SaveCommand => new Command(SaveSettings);

		string _azureFunctionsUrl;
		public string AzureFunctionsUrl
		{
			get { return _azureFunctionsUrl; }
			set { SetProperty(ref _azureFunctionsUrl, value); }
		}

		async public void SaveSettings()
		{
			//Persist to disk
			App.Current.Properties[nameof(AzureFunctionsUrl)] = App.AzureFunctionsUrl = AzureFunctionsUrl;
			await App.Current.SavePropertiesAsync();

			DataStore.Instance.Reset();
		}
	}
}
