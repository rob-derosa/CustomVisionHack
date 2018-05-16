using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using MyCommonLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyFunctionsApp
{
    public partial class CosmosDataService
    {
        static string _databaseId = "my_database";
        static string _collectionId = "predictions";

		static string _databaseUrl = "https://YOURDATABASEURL.documents.azure.com:443/";  //https://yourcosmosdatabase.documents.azure.com:443/";
		static string _databaseKey = "YOURDATABASEKEYgCfsnYBA9qFNz1oLgSR33tAxbFvOA=="; //MnipfCxKxaOZAZJCl6QRRRaJUHm...

        DocumentClient _client;

        Uri _collectionLink = UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);

        public CosmosDataService()
        {
            _client = new DocumentClient(new Uri(_databaseUrl), _databaseKey, ConnectionPolicy.Default);
        }

        static CosmosDataService _instance;
        public static CosmosDataService Instance
        {
            get { return _instance ?? (_instance = new CosmosDataService()); }
        }

        Uri GetCollectionUri()
        {
            return UriFactory.CreateDocumentCollectionUri(_databaseId, _collectionId);
        }

        Uri GetDocumentUri(string id)
        {
            return UriFactory.CreateDocumentUri(_databaseId, _collectionId, id);
        }

        /// <summary>
        /// Ensures the database and collection are created
        /// </summary>
        async Task EnsureDatabaseConfigured()
        {
            var db = new Database { Id = _databaseId };
            var collection = new DocumentCollection { Id = _collectionId };

            var result = await _client.CreateDatabaseIfNotExistsAsync(db);

            if (result.StatusCode == HttpStatusCode.Created || result.StatusCode == HttpStatusCode.OK)
            {
                var dbLink = UriFactory.CreateDatabaseUri(_databaseId);
                var colResult = await _client.CreateDocumentCollectionIfNotExistsAsync(dbLink, collection);
            }
        }

        /// <summary>
        /// Fetches an item based on it's Id
        /// </summary>
        /// <returns>The serialized item object</returns>
        /// <param name="id">The Id of the item to retrieve</param>
        public async Task<T> GetItemAsync<T>(string id) where T : BaseModel, new()
        {
            try
            {
                var docUri = GetDocumentUri(id);
                var result = await _client.ReadDocumentAsync<T>(docUri);

                if (result.StatusCode == HttpStatusCode.OK)
                {
                    return result.Document;
                }

                return null;
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                    await EnsureDatabaseConfigured();

                return null;
            }
        }

        /// <summary>
        /// Fetches all items in the collection
        /// </summary>
        /// <returns>The serialized item object list</returns>
        public List<T> GetItemsAsync<T>() where T : BaseModel, new()
        {
            try
            {
                var result = _client.CreateDocumentQuery<T>(GetCollectionUri()).ToList();
                return result;
            }
            catch (DocumentClientException)
            {
                return null;
            }
        }


        /// <summary>
        /// Inserts the document into the collection and creates the database and collection if it has not yet been created
        /// </summary>
        /// <param name="item">The item to add</param>
        public async Task InsertItemAsync<T>(T item) where T : BaseModel
        {
            try
            {
                var result = await _client.CreateDocumentAsync(_collectionLink, item);
                item.Id = result.Resource.Id;
            }
            catch (DocumentClientException dce)
            {
                if (dce.StatusCode == HttpStatusCode.NotFound)
                {
                    await EnsureDatabaseConfigured();
                    await InsertItemAsync(item);
                }
            }
        }

        /// <summary>
        /// Updates the document
        /// </summary>
        /// <param name="item">The item to update</param>
        public async Task UpdateItemAsync<T>(T item) where T : BaseModel
        {
            await _client.ReplaceDocumentAsync(GetDocumentUri(item.Id), item);
        }
    }
}
