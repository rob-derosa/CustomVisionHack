using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Cognitive.CustomVision;
using Microsoft.Cognitive.CustomVision.Models;
using Microsoft.Rest;
using Microsoft.WindowsAzure.Storage.Blob;
using MyCommonLibrary;
using Newtonsoft.Json.Linq;

namespace MyFunctionsApp
{
	public static class MakePrediction
	{
		static CloudBlobContainer _blobContainer;

		[FunctionName(nameof(MakePrediction))]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "api/MakePrediction")]HttpRequestMessage req, TraceWriter log)
		{
			try
			{
				var stream = await req.Content.ReadAsStreamAsync();
				var prediction = new Prediction
				{
					ProjectId = "YOURCUSTOMVISIONPROJECTID",  //36f167d1-82e0-45f4-8ddc-...
					TrainingKey = "YOURCUSTOMVISIONTRAININGKEY",  //c63201c1e627428fb5c5d6...
					TimeStamp = DateTime.UtcNow,
					UserId = Guid.NewGuid().ToString(),
					ImageUrl = await UploadImageToBlobStorage(stream)
				};

				var api = new TrainingApi(new TrainingApiCredentials(prediction.TrainingKey));
				var account = api.GetAccountInfo();
				var predictionKey = account.Keys.PredictionKeys.PrimaryKey;

				var creds = new PredictionEndpointCredentials(predictionKey);
				var endpoint = new PredictionEndpoint(creds);

				//This is where we run our prediction against the default iteration
				var result = endpoint.PredictImageUrl(new Guid(prediction.ProjectId), new ImageUrl(prediction.ImageUrl));
				prediction.Results = new Dictionary<string, decimal>();
				
				// Loop over each prediction and write out the results
				foreach(var outcome in result.Predictions)
				{
					if(outcome.Probability >= 0.0010)
						prediction.Results.Add(outcome.Tag, (decimal)outcome.Probability);
				}

				await CosmosDataService.Instance.InsertItemAsync(prediction);
				return req.CreateResponse(HttpStatusCode.OK, prediction);
			}
			catch(Exception e)
			{
				var baseException = e.GetBaseException();
				var operationException = baseException as HttpOperationException;
				var reason = baseException.Message;

				if(operationException != null)
				{
					var jobj = JObject.Parse(operationException.Response.Content);
					var code = jobj.GetValue("Code");

					if(code != null && !string.IsNullOrWhiteSpace(code.ToString()))
						reason = code.ToString();
				}
				
				return req.CreateErrorResponse(HttpStatusCode.BadRequest, reason);
			}

			async Task<string> UploadImageToBlobStorage(Stream stream)
			{
				//Create a new blob block Id
				var blobId = Guid.NewGuid().ToString() + ".jpg";

				if(_blobContainer == null)
				{
					//You can set your endpoint here as a string in code or just set it to pull from your App Settings
					var containerName = "images";
					var endpoint = $"https://YOURSTORAGEACCOUNT.blob.core.windows.net/{containerName}/?sv=2017-04-17&ss=MAKE_SURE_TO_GET_A_SAS_TOKEN-01-06T04:57:40Z&st=2018-01-05T20:57:40Z&spr=https&sig=YE2ZWYTvRax4jRUmBpZSzaCFDd8ZwM3pxSDHYWVn0dY%3D";
					_blobContainer = new CloudBlobContainer(new Uri(endpoint));
				}

				//Create a new block to store this uploaded image data
				var blockBlob = _blobContainer.GetBlockBlobReference(blobId);
				blockBlob.Properties.ContentType = "image/jpg";

				//You can even store metadata with the content
				blockBlob.Metadata.Add("createdFor", "This Awesome Hackathon");

				//Upload and return the new URL associated w/ this blob content
				await blockBlob.UploadFromStreamAsync(stream);
				return blockBlob.StorageUri.PrimaryUri.ToString();
			}
		}
	}
}
