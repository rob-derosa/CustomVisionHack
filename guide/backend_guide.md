# Custom Vision Hack

In this hack, we're going to show how to spin up your own Functions App backend and a Xamarin Mobile App front-end in a few steps with the intent of using Cognitive Services Custom Vision AI to identify custom-trained objects, blob storage to store our images, and Cosmos DB to track the results.

Visual Studio 2017 on Windows is required to build and publish the backend Azure Functions App. The mobile app project can be built and run using either an iOS or Android device. A device is preferred because we intend to use the camera to take pictures but an emulator/simulator will work as well. If you are not able to build and run the mobile app project due to technical limitations, you can download and install a pre-built version of the app to your device (see [step 12.b](#step-12b-install-a-prebuilt-version-of-the-mobile-app)) and enter your Functions app endpoint URL to utilize your own edpoint.

__A few notes on Custom Vision__
- We're going to use the web interface to train our classifiers for this hack but this can and should be done programatically using an SDK or the REST API
- To increase the accuracy of your predictions:
   - Add more images of your object in different angles, lighting, environments and situations where you anticipate your object may be tested against
   - Add more images of things that are NOT your object and tag them with other data - the classifier needs to understand what your object is not, too
   - Only test against objects/models that exist within your project - you may get misfires if you test against an object you have not trained


#### This hackathon exercises the following platforms or services

1. Visual Studio 2017
1. Xamarin (Xamarin.Forms)
1. Azure Functions (C#/.NET)
1. Azure Storage Account (blob)
1. Azure Cosmos DB (SQL/DocumentDB)
1. Cognitive Services Custom Vision

#### Recommended tools

1. (Required) [Visual Studio 2017](https://www.visualstudio.com/downloads/) with the following packages installed:
   - Mobile Development with .NET (Xamarin)
   - Azure Development
   - (Extension) Azure Functions and Web Jobs Tools version 15.0.31201.0 or higher
   * If you run into issues with Visual Studio, please see [this guide](https://github.com/rob-derosa/CustomVisionHack/blob/master/guide/xamarin_setup_guide.pdf) to assist with using a Virtual Machine with Visual Studio 2017 pre-installed (MSDN subscription required)
1. [Visual Studio for Mac](https://www.visualstudio.com/downloads/) (optional for Android, required for iOS if you want to build out the mobile client which is optional)
1. [Postman](https://www.getpostman.com/)


### Table of Contents

1. [Clone the repo](#step-1-clone-the-repo)
1. [Create a new Azure Function App](#step-2-create-a-new-azure-function-app)
1. [Create a Function App solution in Visual Studio 2017](#step-3-create-a-function-app-solution-in-visual-studio-2017)
1. [Publish your app to the cloud and verify](#step-4-publish-your-app-to-the-cloud-and-verify)
1. [Create your Storage Account (Blob)](#step-5-create-your-storage-account-blob)
1. [Generate a Shared Access Signature for your Storage Account](#step-6-generate-a-shared-access-signature-for-your-storage-account)
1. [Upload an image/byte[] to your blob storage account](#step-7-upload-an-imagebyte-to-your-blob-storage-account)
1. [Create an Azure Cosmos DB account](#step-8-create-an-azure-cosmos-db)
1. [Write a document to your Cosmos DocumentDB Database](#step-9-write-a-document-to-your-cosmos-documentdb-database)
1. [Set up your own Custom Vision project and train a classifier](#step-10-set-up-your-own-custom-vision-project-and-train-a-classifier)
1. [Make a prediction with Custom Vision and log the results](#step-11-make-a-prediction-with-custom-vision-and-log-the-results)
1. [Connect the mobile front-end to the Functions backend](#step-12-connect-the-mobile-front-end-to-the-functions-backend)
   1. [Install a prebuilt version of the mobile app](#step-12b-install-a-prebuilt-version-of-the-mobile-app)
1. [Add another function that returns a list of past predictions](#step-13-add-another-function-that-returns-a-list-of-past-predictions)
1. [Add a new tabbed page to the mobile app and display a list of past predictions](#step-14-add-a-new-tabbed-page-to-the-mobile-app-and-display-a-list-of-past-predictions)
1. [Make predictions locally (edge) using CoreML/TensorFlow](#step-15-make-predictions-on-the-device-edge-using-coremltensorflow)

#### Azure Portal Common Fields

- __Name__: All resources will require a unique name and almost always impose some limitations on the characters allowed. In many cases, this also serves as part of the host name. For example, the name `myfunctionsapp` would equate to `http://myfunctionsapp.azurewebsites.net` 
- __Subscription__: Select your Azure subscription in the dropdown list - this is where any services you use will be billed.
- __Resource Groups__: Resource groups provide a way to monitor, control access, provision and manage billing for collections of assets that are required to run an application. Each resource you create in Azure will require a parent resource group. For this hack, you should use the same resource group for all your resources. If this is the first resource you are creating, select __Create New__, otherwise choose __Use Existing__ and select the original resource group you created.
- __Location__: Select a region to deploy your resources. Typically this would be a region closest to your intended user base.


### Step 1: Clone the repo

Make a local directory, and then clone the repo from [https://github.com/rob-derosa/customvisionhack](https://github.com/rob-derosa/customvisionhack)

```git clone https://github.com/rob-derosa/customvisionhack.git```


### Step 2: Create a new Azure Function App

1. Browse to [https://portal.azure.com](https://portal.azure.com)
1. In the top left, click Create Resource > Compute > Function App
<br/><img src="resources/portal_create_new_functions_app.png" width="75%" />
1. Enter in a name for the app (e.g. `myfunctionsapp` - this must be unique but don't worry, the portal will tell you if it's not)
1. Because this is the very first resource we are creating, we need to create a new resource group
1. Complete the new resource [common form fields](#azure-portal-common-fields)
1. Leave the rest of the settings as default
1. Optionally, for easy access, click the __Pin to dashboard__ checkbox
1. Click __Create__ to create your Function App
<br/><img src="resources/portal_create_new_functions_app_settings.png" width="75%" />
1. It can take a few minutes before this process completes but you should see some notifications updating you on status 
<br/><img src="resources/portal_notifications_in_progress.png" width="40%" />
1. You can always check out all incoming notifications by clicking on the Alert icon in the top toolbar 
<br/><img src="resources/portal_notifications.png" width="40%" />


### Step 3: Create a Function App solution in Visual Studio 2017

1. Launch Visual Studio and ensure your Azure Functions and Web Jobs Tools are updated to at version 15.0.31201.0
  1. Tools > Extensions and Updates > Expand the Updates node and select Visual Studio Marketplace
  1. Select Azure Functions and Web Jobs Tools and click the __Update__ button
  <br/><img src="resources/vs_update_azure_tools.jpg" width="75%" />
  1. Close out and exit Visual Studio so the update can install
     1. Click the __Modify__ button when the prompt shows
  1. Once the update completes, restart Visual Studio
1. In Visual Studio, click File > Open > Project/Solution... and select the empty solution located in the cloned repo `/src/start_here/MyBackendApp.sln`
1. Right-click on the Solution node in Solution Explorer and select Add > New Project...
<br/><img src="resources/vs_add_new_project_menu.png" width="75%" />
1. Under Visual C#, choose Cloud > Azure Functions
<br/><img src="resources/vs_new_functions_app_project.png" width="75%" />
1. Select Azure Functions v1 (.NET Framework) from the dropdown and Http Trigger as the trigger type
1. Select __Anonymous__ from Access rights dropdown and click OK
<br/><img src="resources/vs_new_functions_app_project_settings.png" width="75%" />
   1. this will create a boilerplate function called `Function1` 1. we will replace this function with a real one shortly
1. Test out the function by clicking the Run/Debug button in Visual Studio
   1. You may be prompted to download and install the Azure Functions CLI tools 1. click the __Yes__ button to accept
1. Use Postman or a browser to make a GET request to the function (e.g. `http://localhost:7071/api/Function1?name=Rob`) and verify the ouput (e.g. `Hello, Rob`)
<br/><img src="resources/postman_verify_function1.png" width="75%" />

### Step 4: Publish your app to the cloud and verify

1. Right-click on your Functions project and select __Publish...__ then click the __Start__ button
1. Select __Azure Function App__ and __Select Existing__
1. Click the Settings icon and select __Create Profile__
1. Click the __Create Profile__ button
<br/><img src="resources/vs_new_publish_profile.png" width="75%"/>
1. Select your Azure Subscription from the dropdown
1. Expand your the Resource Group and select the Function App you created in step 2
1. Click __OK__
<br/><img src="resources/vs_create_publish_profile_select_appservice.png" width="75%" />
1. (If you plan to attach a remote debugger)
   1. Click __Manage Profile Settings__
   <br/><img src="resources/vs_publish_profile_settings_link.png" width="65%" />
   1. Change the Configuration to __Debug__ and click __Save__
   <br/><img src="resources/vs_publish_profile_settings.png" width="50%" />
1. Click the __Publish/Save__ button
   1. If you get a warning indicating the version remotely doesn't match the local version, accept by clicking __Yes__
1. Copy the site URL and verify the function is running by using Postman to send that same GET request against the remote instance (e.g. `http://myfunctionsapp.azurewebsites.net/api/Function1?name=Rob`) and verify the ouput (e.g. `Hello, Rob`)
<br/><img src="resources/vs_publish_profile_site_link.png" width="65%" />


### Step 5: Create your Storage Account (Blob)

1. Once again, browse to [https://portal.azure.com](https://portal.azure.com)
1. Click __Create Resource__ from the menu on the left of the page, then search for __Storage Account__ and click __Create__
<br/><img src="resources/portal_create_new_storage_account.png" width="75%" />
1. Complete the new resource [common form fields](#azure-portal-common-fields)
1. From the __Account Kind__ dropdown, select __Blob__
1. Click __Create__ to create the Storage Account
<br/><img src="resources/portal_create_new_storage_account_settings.png" width="75%" />
1. Once your storage account has been created, we need to create a new container to house our images
   1. Jump back into it and select __Browse blobs__
   1. Click the __+ Container__ button to create a new container to house the image blobs we'll upload
   1. Name the container `images` and set the Public access level to __Blob (anonymous read access for blobs only)__
   <br/><img src="resources/portal_storage_account_create_container.png" width="75%" />


### Step 6: Generate a Shared Access Signature for your Storage Account

> __Note:__ We use a SAS URL to grant read/write access for a specified amount of time. We'll be using this from within a function.

1. Go to your storage account
   1. you can search for this or go to Resource Groups > select yours > click on your storage account in the list
1. Click on Shared Access Signature
1. Leave the default settings as is but ensure the __End Date__ is set to 24hrs from now or however long you want the Functions App to have access
1. Click the __Generate SAS__ button
<br/><img src="resources/portal_storage_account_generate_sas_url.png" width="75%" />
1. Copy the __Blob service SAS URL__ to somewhere safe - we'll be using this to allow our function limited access to read and write to our storage account (e.g. `https://mynewstorageaccountblob.blob.core.windows.net/?sv=2017-04-17&ss=b&srt=sco&sp=rwdlac&se=2018-01-05T23:03:17Z&st=2018-01-04T23:03:17Z&spr=https&sig=HIE2i2WSliZkiQ8lAw0qjykZm1VbywZdAGbUhrWfl%2BI%3D`)

> __Note:__ This can also be done programatically to generate the URLs on demand. Check out [this code](https://github.com/rob-derosa/Hunt/blob/master/Hunt.Backend.Functions/Functions/GetStorageToken.cs#L81) as a reference.


### Step 7: Upload an image/byte[] to your blob storage account

1. Back in Visual Studio, change the method name and file name of `Function1` to `MakePrediction`
1. Also change the value of the `FunctionName` attribute to `nameof(MakePrediction)` - this essentially just passes in the name of the method as a string to define our endpoint equivalant to `"MakePrediction"`
1. In the method signature, change the HttpTrigger methods params from `"post", "get"` to just `"post"`
1. Right-click on the project's __Dependencies__ node and choose __Manage Nuget Packages...__
1. Click on the __Browse__ tab and search for `Newtonsoft`
1. Select the __Newtonsoft.Json__ v11.0.2 package and click __Install__
1. Click on the __Browse__ tab and search for `Azure Storage`
1. Select the __WindowsAzure.Storage__ v9.1.1 package and click __Install__
1. Follow the instructions in [this gist](https://gist.github.com/rob-derosa/87e59e3dac93882f29f8fd4fa246ff3d)
   1. If your function returns `IActionResult`, you likely created a Functions v2 (.NET Standard Preview) instead of a Functions v1 (.NET Framework) - see step 3.10
1. Add the missing using statements by hovering over the red squiggly-lined class until a light bulb appears. Click on the light bulb and select the corresponding namespace 
<br/><img src="resources/vs_add_using_statements.png" width="50%">
1. Back in Visual Studio, update the value of the `endpoint` variable to incorporate both your storage SAS URL __AND THE CONTAINER NAME__ - you need to ensure the URL you copy and paste incorporates the `{containerName}` code after the `.net/` and before the `?sv=2018...`, otherwise you'll likely see a 404 error (e.g. `https://mynewstorageaccountblob.blob.core.windows.net/{containerName}?sv=2017-07-29&ss=b&srt=sco`...)
1. Build and run the project locally
1. Verify this by using Postman to send a POST request to your local endpoint
    1. Set the method dropdown to `POST`
    1. Set your endpoint in the address bar (e.g. `http://localhost:7071/api/MakePrediction`)
    1. Set the body content by selecting the __Body__ tab and choose the __binary__ option
    1. Choose a local image file to send
    1. Send the request and verify a URL comes back
    1. Copy and paste this URL into a browser and validate the image is the one you posted
      1. Note: you can also verify in the Azure portal by clicking __Browse blobs__ and selecting the __images__ container
1. Publish to the cloud and verify this remotely following the steps outlined above


### Step 8: Create an Azure Cosmos DB account

1. Once again, browse to [https://portal.azure.com](https://portal.azure.com)
1. In the top left, click __Create Resource > Databases > Azure Cosmos DB__
<br/><img src="resources/portal_create_new_cosmos_database.png" />
1. Complete the new resource [common form fields](#azure-portal-common-fields)
1. Choose the `SQL` from the API dropdown - this will use DocumentDB under the hood
1. Click __Create__ to create the Cosmos DB
<br/><img src="resources/portal_create_new_cosmos_database_settings.png" />
1. It can take a few minutes before this process completes


### Step 9: Write a document to your Cosmos DocumentDB Database

1. Back in Visual Studio, right-click on the project's __Dependencies__ node and choose __Manage Nuget Packages...__
1. Click on the __Browse__ tab and search for __DocumentDB__
1. Select the __Microsoft.Azure.DocumentDB.Core__ v1.9.1 package and click __Install__
1. Add the `MyCommonLibrary` project to your solution - this project contains model classes that we will share between our front (mobile app) and back (Functions app) ends
   1. Right-click on the solution and select __Add > Existing Project...__ and navigate to the parent folder and select `MyCommonLibrary.csproj`
1. Add a reference to the `MyCommonLibrary` project to your Function App project
   - __Note:__ This is a .NET Standard library that contains classes/models shared between the front and back ends
   1. Right click on the __Dependencies__ node of the Functions app and select __Add Reference...__
   1. On the left side, choose Projects and check the box next to `MyCommonLibrary`
   1. Click the __OK__ button
   <br/><img src="resources/vs_add_reference_common.png" width="75%" />
1. Right-click on the `MyCommonLibrary` project and select __Rebuild__
1. Right-click on your Functions app project and select __Add > Class...__ and name it `CosmosDataService.cs`
1. Replace the empty class with the code from [this gist](https://gist.github.com/rob-derosa/d38e6a7c1bdba90b101a3e9ad7b6dfb6)
1. Add the missing using statements
1. The `_databaseId` is the name of your database and the `_collectionId` is then name of the table/collection - feel free to change these
1. Check out the method `EnsureDatabaseConfigured` - it's responsible for creating your database and collection if either do not exist
1. Now we need to get some values from the Azure Portal, such as our database URL and SAS key
   1. In the Azure portal, navigate to your Cosmos DB
   1. Click on __Keys__
   1. In the Read-write Keys tab, copy the URI and paste it as the value of `_databaseUrl` in `CosmosDataService.cs`
   1. In the Read-write Keys tab, copy the Primary Key and paste it as the value of `_databaseKey` in `CosmosDataService.cs`
<br/><img src="resources/portal_cosmos_keys.png" />
1. In MakePrediction.cs, replace the code in the `try` clause with the code in [this gist](https://gist.github.com/rob-derosa/240dd977d41ebc58f633dbdf009d5e63)
1. Build and run your project locally
1. Use Postman to make the same local POST request as in the previous step
1. Verify the data was inserted by inspecting the Prediction JSON object that is returned.
   - You can also see the documents in your database by navigating to the Data Explorer from your Cosmos DB account and expanding down to your collection documents
1. Publish to the cloud and re-verify


### Step 10: Set up your own Custom Vision project and train a classifier

In this step, we will create a new Custom Vision project and classify a few different models (objects) by uploading various photos of the models with associated tags. Then we will train the classifier to identify/predict those objects with a probability against each tag.

1. Browse to [http://customvision.ai](http://customvision.ai) and sign-in to create a new project
1. Click the __New Project__ button
1. Complete the form by choosing a name for your project, such as __ShoesProject__
1. You can leave the Domain as General and click the __Create project__ button
<br/><img src="resources/portal_cv_new_project.png" width="75%" />
1. Once your project has been created, it's time to upload some images of a model/shoe you intend to train.
    - __Tip:__ While the minimum photos-per-model count is 5, the classifier becomes more accurate as you add more photos - it is recommended to add at least 10 photos or more if you have the time
    - __Tip:__ Add varying photos of the object in different angles, perspective, lighting and environments
   1. Using your phone's camera, take at least 10 images of the object you wish to train and transfer them to your computer for upload
   1. Click the __Add images__ button
   <br/><img src="resources/portal_cv_add_images.png" width="75%" />
   1. Click the __Browse local files__ button and select the images you snapped in the previous step
   1. You will be asked to add some tags to the images you are uploading - enter at least 2 tags
   1. Click the __Upload files__ button and verify your model was tagged properly by checking the boxes of those specific tags and validating the image count
   <br/><img src="resources/portal_cv_add_tags.png" width="75%" />
   1. Repeat this process to until you have at least 4 total models with at least 2 tags each - the classifier gets smarter as you add more models
1. Once you have at least 4 models, click the __Train Iteration__ button at the top
1. This will train your first iteration of the classifier (iterations are versioned and each project can have a default iteration)
<br/><img src="resources/portal_cv_train_iteration.png" width="75%" />
1. Let's test out this iteration running a quick test - start by clicking the __Quick Test__ button at the top
<br/><img src="resources/portal_cv_train_iteration_results.png" width="75%" />
1. Take another photo of one of the objects and transfer it to your computer
1. Select the photo your just snapped and choose it for upload
1. Once the photo has been uploaded, you will see the results of the test below - ideally, you'll have the correct set of tags with a high probability
<br/><img src="resources/portal_cv_quick_test_results.png" width="75%" />
1. As you add additional images with tags, you'll need to re-train your classifier by repeating the steps above. It is also recommended to set the latest iteration of your trained classifier to be the default iteration for the project. You can do this by:
   1. Click on the __Performance__ tab at the top
   1. Select the iteration on the left you want to make the default
   1. Click on the __Make default__ button towards the top to set it as default
   <br/><img src="resources/portal_cv_default_iteration.png" width="75%" />


### Step 11: Make a prediction with Custom Vision and log the results

Now that we have a custom vision project with at least 4 trained models, we can begin to make predictions against the classifier. For this, we will need the custom vision project ID as well as the training key.

1. In Visual Studio, right-click on your Azure Functions project's __Dependencies__ node and choose __Manage Nuget Packages...__
1. Click on the __Browse__ tab and search for __CustomVision__
1. Select the __Microsoft.Cognitive.CustomVision.Prediction__ v1.2.0 package and click __Install__
1. Select the __Microsoft.Cognitive.CustomVision.Training__ v1.2.0 package and click __Install__
1. Search for __Microsoft.Rest.ClientRuntime__ and install package v2.3.11
1. In `MakePrediction.cs`, replace the code within the `try` clause with the code in [this gist](https://gist.github.com/rob-derosa/07952c26d0bc7e78b974cae5ddec1485)
1. Add the missing using statements
1. Set the value of the `prediction.ProjectId` to your custom vision project ID and the value of `prediction.PredictionKey` to your custom vision project's prediction key
   1. To get your custom vision project ID and prediction key, go to your project at http://customvision.ai
   1. Navigate to the project settings and you'll find your project ID and prediction key
   <br/><img src="resources/portal_cv_settings.png" width="75%" />
1. You've just added a chunk of code that will now run a prediction against the image you upload and return a list of resulting tags and their probability
1. Build and run locally
1. Use Postman to make the same local POST request as in the previous step
1. Verify the data was inserted by inspecting the Prediction JSON object that is returned which should include some matching tags
1. Publish to the cloud and re-verify
<br/><img src="resources/postman_get_predictions_result.png" width="75%" />


### Step 12: Connect the mobile front-end to the Functions backend

__Note:__ If you are unable to build and deploy the mobile app, skip to Step [12.b](#step-12b-install-a-prebuilt-version-of-the-mobile-app) to install the app directly on your iOS or Android device.

__Note:__ You can build the mobile front end using either Visual Studio for Windows or Visual Studio for Mac. If you are targeting iOS, you must have a Mac - either as the main IDE or bridged to your PC running Visual Studio for Windows.

1. In another instance of Visual Studio, open the __MyMobileApp.sln__ file sitting in the root folder - this will open a Xamarin Forms project targeting iOS and Android
1. Open the `App.xaml.cs` file in the __MyMobileApp.Common__ project and set your Azure Function endpoint on line 17.
1. Right-click on either the iOS or Android project and select __Set as Startup Project__, then click __Rebuild__
1. Select a device or emulator from the drop-down list in the top toolbar and click the Start Debug button
   - It can take several minutes to compile and begin the debugging session
1. Once your debugging session begins, if using an actual device:
   1. Tap the __Take Photo__ button to take a photo and snap a pic of one of the 4 models you have trainied your classifier against
   1. Tap the __Make Prediction__ button
1. If you are using an emulator and cannot use a camera:
      1. Using the browser in the emulator, search for one of the 4 models you have trainied your classifier on
      1. Save the image to your local photo library
      1. Launch the mobile app and tap on the __Take Picture__ button
      1. Select the image you downloaded in the previous step
1. Ideally, you'll see something similar to the following:
<br/><img src="resources/ios_prediction_result.png" width="300" /> <img src="resources/android_prediction_result.png" width="300" />
1. Click the Reset button in the top right to reset the data and make another prediction


#### Step 12.b: Install a prebuilt version of the mobile app

__Note:__ This step is for those that cannot build the mobile app - it is configurable to point to any Azure endpoint

1. On the device you want to install the app, navigate [here for iOS](https://install.appcenter.ms/users/robd/apps/custom-vision-hack/distribution_groups/public) or [here for Android](https://install.appcenter.ms/users/robd/apps/custom-vision-hack-1/distribution_groups/public)
1. Click on the __Install__ button
1. If the device is iOS:
   1. You will need to trust the enterprise signing certificate by by opening the Settings App
   1. Tap on General and scroll down to tap on Profiles & Device Management
   1. Locate the __Xamarin Inc.__ profile under the Enterprise Apps section and tap on it
   1. Tap the __Trust Xamarin Inc__ button and confirm by tapping __Trust__ again
   1. Go to the home screen and you should see the app installing/installed
1. Launch the app and click on the Settings tab
1. Enter your Functions App endpoint url in the textbox and tap Save (e.g. `http://myfunctionsapp.azurewebsites.net`)
   1. Tap the __Take Photo__ button to take a photo and snap a pic of one of the 4 models you have trainied your classifier against
   1. Tap the __Make Prediction__ button
1. Ideally, you'll see something similar to the following:
<br/><img src="resources/ios_prediction_result.png" width="300" /> <img src="resources/android_prediction_result.png" width="300" />
1. Click the Reset button in the top right to reset the data and make another prediction

### Step 13: Add another function that returns a list of past predictions

1. In Visual Studio, right-click on your functions project and selct __Add > Class... > Azure Function__
   1. Name the function __GetPredictions__ and click the __Add__ button
   1. Leave the trigger type as Http trigger but change the Access rights to __Anonymous__
   1. Click the __OK__ button
1. Replace the class code with the contents in [this gist](https://gist.github.com/rob-derosa/6c0857be11ac0c66cf62386fa33c5de8)
1. Add the missing `using` statements and note the use of a `Route` to automatically pull data in from the URL, in this case the `databaseId` and `collectionId`
1. Because we are using a RESTful approach to the URLs, we need to modify the host.json file to remove the default route prefix of `api/`
   1. Open the __host.json__ file and add the following code from [this gist](https://gist.github.com/rob-derosa/3505273e2948429397d23c87b2ea34c8)
   1. This will make it so our `GetPredictions` function does not have to have the api prefix (eg `api/123456-7890-...`)
   1. But now that our route prefix no longer has the default `api/`, we need to update our `MakePrediction.cs` function route to make up for that.
      1. Open the `MakePrediction.cs` file and change the `Route` property in the `HttpTrigger` attribute from `null` to `"api/MakePrediction"`
1. Build and run your project locally
1. Verify the `GetPredictions` function using Postman by making a `GET` request to your function
1. Set your endpoint in the address bar (e.g. `http://localhost:7071/myDatabaseId/myCollectionId`)
1. Verify an array of `Prediction` JSON data is returned
1. Publish to the cloud and re-verify


### Step 14: Add a new tabbed page to the mobile app and display a list of past predictions

1. In Visual Studio, right-click on the __Views__ folder within the `MyMobileApp.Common` project.
1. Select __Add > New Item > Xamarin.Forms > ContentPage__ and name it `PredictionListPage.xaml` - this will create a new UI page in .xaml as well as a backing .cs file
1. Replace the code in `PredictionListPage.xaml` with [this code](https://gist.github.com/rob-derosa/e43dad741adf8614db9ea5ba655d6334) - this is our UI code expressed in XAML and we use bindings to automatically tie data to our UI
1. Replace the code in `PredictionListPage.xaml.cs` with [this code](https://gist.github.com/rob-derosa/a35e06554e670da6a7159e6046a35255) - this is our code-behind or backing class and can respond to UI events like a button click or resume-from-background
1. Right-click on the `ViewModels` folder and select __Add > New Item > Class__ and name it `PredictionListViewModel.cs`
1. Replace the code in `PredictionListViewModel` with the [this code](https://gist.github.com/rob-derosa/81c54ac40e6b3e603d26bb2b3d6c789a) - this is the ViewModel/binding context that will be associated with `PredictionListPage` and will contain the data the UI will display
1. Now we need to add a new tab so we can switch to the new list page
   1. Open `/Views/MainPage.cs` and notice the pattern of the two pages already present, `predictPage` and `settingsPage`
   1. Follow the pattern to create a new page of type `PredictionListPage` and add it to the `Children` of `MainPage`
      - If you run into issues, you can reference [this code](https://github.com/rob-derosa/CustomVisionHack/blob/master/src/completed/MyMobileApp.Common/Views/MainPage.cs) as an example
1. Rebuild the project, deploy to your device and select the new tab you just created
1. You should see a list of photos with their matching tags 


### Step 15: Make predictions on the device (edge) using CoreML/TensorFlow

__Note:__ In this step, we'll cut the cord to Azure and bring the models down to the bare metal. You will need iOS 11+ or Android API 21+.

1. In the Custom Vision portal, click into your project then click on the __Settings__ button and change the Domain from __General__ to __General (compact)__ and click __Save__
<br/><img src="resources/portal_cv_domain_compact.png" width="300" />
1. Now that the domain has changed, we need to train the classifier again so click the green __Train__ button at the top
1. Once the training has completed, click on the __Performance__ tab and then click the __Export__ link
<br/><img src="resources/portal_cv_export.png" width="75%" />
1. Download the corresponding model for your mobile OS (__CoreML__ for iOS, __TensorFlow__ for Android)
<br/><img src="resources/portal_cv_coreml_tensorflow.png" width="75%" />
1. Back in Visual Studio, right-click on your MyMobileApp.Common project's __Dependencies__ node and choose __Manage Nuget Packages...__
1. Click on the __Browse__ tab and search for __Xamarin CustomVision__
1. Select the __Xam.Plugins.OnDeviceCustomVision__ v1.0.0 package and click __Install__ - this package will allow us to classify an image from a common API while still using the native frameworks to do so
1. Repeat this process for the iOS and/or Android projects
1. Add the exported model data to your mobile project(s)
   - iOS: rename the .mlmodel file you downloaded to something like `mymodel.mlmodel` and add it to the `Resources` folder in your iOS project
   - Android: unzip the file you exported and add the `labels.txt` and `model.pb` file to the `Assets` folder in your Android project
1. Initialize the Custom Vision package in your mobile project(s)
   - iOS: Add this line ```CrossImageClassifier.Current.Init("<model_name>", ModelType.General);``` to the `AppDelegate.cs` file just before the line ```return base.FinishedLaunching(app, options);``` where `<model_name>` is the name of the .mlmodel file without the extension
   - Android: Add this line ```CrossImageClassifier.Current.Init("model.pb", ModelType.General);``` to the `MainActivity.cs` file just before the line ```LoadApplication(new App());```
1. In the `MyMobileApp.Common` project, open the `/ViewModels/PredictionDetailsViewModel.cs` file and replace the code inside the `try` block with [this code](https://gist.github.com/rob-derosa/a19e392bfe1d0670a7cb43a5116cd21c)
   - This chunk of code will now employ the local device frameworks for ML against the compact model exported previously
1. Add the missing using statements
1. Build and deploy to the device/OS of your choice
1. Disable your cell/wifi connectivity to ensure you're not being sneakily tricked, snap a pic make a prediction
1. Ideally, you'll see something similar to the following:
<br/><img src="resources/ios_prediction_result.png" width="300" /> <img src="resources/android_prediction_result.png" width="300" />


#### Attaching a remote debugger to your Azure Functions App

1. Before we can attach a remote debugger, we need to ensure the __Enable Just My Code__ option in the Debugging section is disabled
   1. In Visual Studio 2017, click on Tools > Options... > Click on the Debugging node
   1. Look for the checkbox labeled __Enable Just My Code__ and ensure it is UNCHECKED
   <br/><img src="resources/vs_disable_justmycode.png" width="75%" />
1. In Visual Studio 2017, open the Server Explorer window: View > Server Explorer
1. Expand your Azure subscription node > App Service > YourResourceGroup > YourFunctionsApp
1. Right Click on __YourFunctionsApp__ node and select Attach Debugger
<br/><img src="resources/vs_attach_debugger.png" width="50%" />
1. Set a breakpoint in code where you want the debugger to pause
1. From here, you can either hover over the code you want to see values for or add the variable to the __Watch__ pad


#### References

- [Azure Functions](https://azure.microsoft.com/en-us/services/functions/)
- [Cosmos DB](https://azure.microsoft.com/en-us/services/cosmos-db/)
- [Xamarin.Forms](http://xamarin.com/forms)
- [Custom Vision](https://azure.microsoft.com/en-us/services/cognitive-services/custom-vision-service/)
   - [Web Portal](http://customvision.ai)
   - [C# API](https://docs.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/csharp-tutorial)
   - [REST API - Prediction](https://go.microsoft.com/fwlink/?linkid=865445)
   - [REST API - Training](https://go.microsoft.com/fwlink/?linkid=865446)
   - [Tips on improving classifiers](https://docs.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/getting-started-improving-your-classifier)
- [Azure Storage Explorer](https://azure.microsoft.com/en-us/features/storage-explorer/)
- [Custom Vision On Device Xamarin Plugin](https://github.com/jimbobbennett/Xam.Plugins.OnDeviceCustomVision)
- [Xamarin Mobile App Hack](https://github.com/dwhathaway/CustomVisionHack/blob/mobile-app-hack/guide/mobile_app_guide.md)
