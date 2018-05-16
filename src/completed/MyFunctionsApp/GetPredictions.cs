using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MyCommonLibrary;

namespace MyFunctionsApp
{
	public static class GetPredictions
	{
		[FunctionName(nameof(GetPredictions))]
		public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{databaseId}/{collectionId}")]
		HttpRequestMessage req, string databaseId, string collectionId, TraceWriter log)
		{
			try
			{
				//Using a Route, the databaseId and collectionId params will automatically be populated with the data in the URL
				//We aren't actually doing anything with these IDs at this point other than writing them out
				//but you could potentially pass them into a dataservice to process.
				//In our case, the CosmosDataService has these values hardcoded

				Console.WriteLine($"GetPredictions called [databaseID:{databaseId}, collectionID: {collectionId}]");
				var list = CosmosDataService.Instance.GetItemsAsync<Prediction>();
				return req.CreateResponse(HttpStatusCode.OK, list);
			}
			catch(Exception e)
			{
				return req.CreateErrorResponse(HttpStatusCode.BadRequest, e.GetBaseException().Message);
			}
		}
	}
}
