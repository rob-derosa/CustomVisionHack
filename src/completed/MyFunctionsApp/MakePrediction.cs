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
using Microsoft.Cognitive.CustomVision.Prediction;
using Microsoft.Cognitive.CustomVision.Prediction.Models;
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
					ProjectId = "YOUR_PROJECT_ID", //This is the custom vision project we are predicting against
					PredictionKey = "YOUR_PREDICTION_KEY", //This is the prediction key we are predicting against
					TimeStamp = DateTime.UtcNow,
					UserId = Guid.NewGuid().ToString(),
					ImageUrl = await UploadImageToBlobStorage(stream),
					Results = new Dictionary<string, decimal>()
				};

				var endpoint = new PredictionEndpoint { ApiKey = prediction.PredictionKey };
				//This is where we run our prediction against the default iteration
				var result = endpoint.PredictImageUrl(new Guid(prediction.ProjectId), new ImageUrl(prediction.ImageUrl));

				// Loop over each prediction and write out the results
				foreach(var outcome in result.Predictions)
				{
					if(outcome.Probability > .70)
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
				blockBlob.Metadata.Add("createdFor", "This Custom Vision Hackathon");

				//Upload and return the new URL associated w/ this blob content
				await blockBlob.UploadFromStreamAsync(stream);
				return blockBlob.StorageUri.PrimaryUri.ToString();
			}
		}
	}
}
