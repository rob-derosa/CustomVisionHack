using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using MyCommonLibrary;
using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public class PredictionListViewModel : BaseViewModel
	{
		public ObservableCollection<Prediction> Items { get; set; }
		public Command LoadItemsCommand { get; set; }

		public PredictionListViewModel()
		{
			Title = "Past Predictions";
			Items = new ObservableCollection<Prediction>();
			LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
		}

		async Task ExecuteLoadItemsCommand()
		{
			if(IsBusy)
				return;

			IsBusy = true;

			try
			{
				Items.Clear();
				var items = await DataStore.Instance.GetPredictionsAsync(true);
				foreach(var item in items)
				{
					Items.Add(item);
				}
			}
			catch(Exception ex)
			{
				Debug.WriteLine(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}
	}
}
