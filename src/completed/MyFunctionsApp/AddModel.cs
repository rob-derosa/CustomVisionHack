using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Cognitive.CustomVision;
using Microsoft.Cognitive.CustomVision.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using MyCommonLibrary;
using Newtonsoft.Json.Linq;

namespace MyFunctionsApp
{
	public static class AddModel
	{
		[FunctionName(nameof(AddModel))]
		public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequestMessage req, TraceWriter log)
		{
			try
			{
				var allTags = new List<string>();
				var json = req.Content.ReadAsStringAsync().Result;
				var jobj = JObject.Parse(json);
				var tags = (JArray)jobj["tags"];
				var term = jobj["term"].ToString();
				var projectId = jobj["projectId"].ToString();
				var trainingKey = jobj["trainingKey"].ToString();
				var offset = 0;

				if(jobj["offset"] != null)
					offset = (int)jobj["offset"];

				var imageUrls = await SearchForImages(term, offset);
				var api = new TrainingApi(new TrainingApiCredentials(trainingKey));
				var project = api.GetProject(Guid.Parse(projectId));

				var tagModels = new List<ImageTagModel>();
				var existingTags = api.GetTags(project.Id);
				foreach(string tag in tags)
				{
					ImageTagModel model = existingTags.Tags.SingleOrDefault(t => t.Name == tag);

					if(model == null)
						model = api.CreateTag(project.Id, tag.Trim());

					tagModels.Add(model);
				}

				var batch = new ImageUrlCreateBatch(tagModels.Select(m => m.Id).ToList(), imageUrls);
				var summary = api.CreateImagesFromUrls(project.Id, batch);

				//if(!summary.IsBatchSuccessful)
				//	return req.CreateErrorResponse(HttpStatusCode.BadRequest, "Image batch was unsuccessful");

				//Traing the classifier and generate a new iteration, that we'll set as the default
				var iteration = api.TrainProject(project.Id);

				while(iteration.Status == "Training")
				{
					Thread.Sleep(1000);
					iteration = api.GetIteration(project.Id, iteration.Id);
				}

				iteration.IsDefault = true;
				api.UpdateIteration(project.Id, iteration.Id, iteration);

				return req.CreateResponse(HttpStatusCode.OK, iteration.Id);
			}
			catch(Exception e)
			{
				var exception = e.GetBaseException();
				return req.CreateErrorResponse(HttpStatusCode.BadRequest, exception.Message);
			}

			async Task<List<string>> SearchForImages(string term, int offset)
			{
				var client = new HttpClient();
				client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "c2adf0e5c057447ea9e0f50cc5202251");
				var uri = $"https://api.cognitive.microsoft.com/bing/v7.0/images/search?count=50&q={term}&offset={offset}";

				var json = await client.GetStringAsync(uri);
				var jobj = JObject.Parse(json);
				var arr = (JArray)jobj["value"];

				var list = new List<string>();
				foreach(var result in arr)
					list.Add(result["contentUrl"].ToString());

				return list;
			}
		}
	}
}
