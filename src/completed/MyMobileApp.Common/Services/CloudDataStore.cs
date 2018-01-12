using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MyCommonLibrary;
using Newtonsoft.Json;
using Plugin.Connectivity;

namespace MyMobileApp.Common
{
	public class CloudDataStore : IDataStore
	{
		HttpClient _client;
		IEnumerable<Prediction> _predictions;

		public CloudDataStore()
		{
			Reset();
			_predictions = new List<Prediction>();
		}

		public void Reset()
		{
			_client = new HttpClient();
			_client.BaseAddress = new Uri($"{App.AzureFunctionsUrl.TrimEnd('/')}/");
		}

		public async Task<IEnumerable<Prediction>> GetPredictionsAsync(bool forceRefresh = false)
		{
			if(forceRefresh && CrossConnectivity.Current.IsConnected)
			{
				var json = await _client.GetStringAsync($"databaseId/collectionId");
				_predictions = await Task.Run(() => JsonConvert.DeserializeObject<IEnumerable<Prediction>>(json));
			}

			return _predictions;
		}

		//public async Task<Prediction> GetPredictionAsync(string id)
		//{
		//	if(id != null && CrossConnectivity.Current.IsConnected)
		//	{
		//		var json = await client.GetStringAsync($"{databaseId}/{collectionId}/{id}");
		//		return await Task.Run(() => JsonConvert.DeserializeObject<Prediction>(json));
		//	}

		//	return null;
		//}

		public async Task<Prediction> MakePredictionAsync(byte[] image)
		{
			if(image == null || !CrossConnectivity.Current.IsConnected)
				return null;

			HttpResponseMessage response = null;
			try
			{
				var imageContent = new ByteArrayContent(image);
				response = await _client.PostAsync("api/MakePrediction", imageContent);
				var result = JsonConvert.DeserializeObject<Prediction>(response.Content.ReadAsStringAsync().Result);
				return result;
			}
			catch(Exception e)
			{
				var error = response.Content.ReadAsStringAsync().Result;

				if(error == null)
					error = e.Message;

				Console.WriteLine($"Error making prediction: {error}");
				return null;
			}
		}

		/*
		public async Task<bool> UpdateItemAsync(Item item)
		{
			if(item == null || item.Id == null || !CrossConnectivity.Current.IsConnected)
				return false;

			var serializedItem = JsonConvert.SerializeObject(item);
			var buffer = Encoding.UTF8.GetBytes(serializedItem);
			var byteContent = new ByteArrayContent(buffer);

			var response = await client.PutAsync(new Uri($"api/item/{item.Id}"), byteContent);

			return response.IsSuccessStatusCode;
		}

		public async Task<bool> DeleteItemAsync(string id)
		{
			if(string.IsNullOrEmpty(id) && !CrossConnectivity.Current.IsConnected)
				return false;

			var response = await client.DeleteAsync($"api/item/{id}");

			return response.IsSuccessStatusCode;
		}
		*/
	}
}
