using System.Collections.Generic;
using System.Threading.Tasks;
using MyCommonLibrary;
using Xamarin.Forms;

namespace MyMobileApp.Common
{
	public interface IDataStore
	{
		Task<IEnumerable<Prediction>> GetPredictionsAsync(bool forceRefresh = false);
		Task<Prediction> MakePredictionAsync(byte[] image);
		void Reset();
		//Task<Prediction> GetPredictionAsync(string id);
		//Task<bool> DeleteItemAsync(string id);
		//Task<bool> UpdateItemAsync(T item);
	}

	public static class DataStore
	{
		static IDataStore _instance;
		public static IDataStore Instance => _instance ?? (_instance = DependencyService.Get<IDataStore>());
	}
}
