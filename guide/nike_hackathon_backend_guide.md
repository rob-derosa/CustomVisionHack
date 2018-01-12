# Custom Vision Hack

In this hack, we're going to show how to spin up your own Functions App backend and a Xamarin Mobile App front-end in a few steps with the intent of using Cognitive Services Custom Vision AI to identify custom-trained objects, blob storage to store our images, and Cosmos DB to track the results.

Visual Studio 2017 on Windows is required to build and publish the backend Azure Functions App. The mobile app project can be built and run using either an iOS or Android device. A device is preferred because we intend to use the camera to take pictures but an emulator/simulator will work as well. If you are not able to build and run the mobile app project due to technical limitations, you can download and install a pre-built version of the app to your device (see step 12.b) and enter your Functions app endpoint URL to utilize your own edpoint.

__A few notes on Custom Vision__
- We're going to use the web interface to train our classifiers for this hack but this can and should be done programatically using an SDK or the REST API
- To increase the accuracy of your predictions:
   - Add more images of your object in different angles, lighting, environments and situations where you anticipate your object may be tested against
   - Add more images of things that are NOT your object and tag them with other data - the classifier needs to understand what your object is not, too)
   - Only test against objects/models that exist within your project - you may get misfires if you test against an object you have not trained


#### This hackathon excercises the following platforms or services

1. Visual Studio 2017
1. Xamarin (Xamarin.Forms)
1. Azure Functions (C#/.NET)
1. Azure Storage Account (blob)
1. Azure Cosmos DB (SQL/DocumentDB)
1. Cognitive Services Custom Vision

#### Recommended tools

1. (Required) [Visual Studio 2017](https://www.visualstudio.com/downloads/) with the following packages installed:
   - Mobile Development with .NET
   - Azure Development
   - (Extension) Azure Functions and Web Jobs Tools version 15.0.31201.0 or higher
1. Visual Studio for Mac (optional for Android, required for iOS)
1. [Postman](https://www.getpostman.com/)


### Step 1: Clone the Repo

Make a local directory, and then clone the repo from [https://github.com/rob-derosa/customvisionhack](https://github.com/rob-derosa/customvisionhack)

```git clone https://github.com/rob-derosa/customvisionhack.git```


### Step 2: Create a new Azure Function App

1. Browse to [https://portal.azure.com](https://portal.azure.com)
1. In the top left, click Create Resource > Compute > Function App
<br/><img src="resources/portal_create_new_functions_app.png" width="75%" />
1. Enter in a name for the app (e.g. `myfunctionsapp` - this must be unique but don't worry, the portal will tell you if it's not)
1. Choose your Azure Subscription
1. Choose "Create new" for the resource group and copy the same value you used for your app name into the Resource Group Name field (we'll create all of our resources under this same Resource Group)
1. Choose "Use existing", and select the resource group you created in the previous section
1. Choose a region (any region is fine)
1. Leave the rest of the settings as default
1. Optionally, for easy access, click the "Pin to dashboard" checkbox
1. Click "Create" to create your Function App
<br/><img src="resources/portal_create_new_functions_app_settings.png" width="75%" />
1. It can take a few minutes before this process completes but you should see some notifications updating you on status 
<br/><img src="resources/portal_notifications_in_progress.png" width="40%" />
1. You can always check out all incoming notifications by clicking on the Alert icon in the top toolbar 
<br/><img src="resources/portal_notifications.png" width="40%" />


### Step 3: Create a Function App solution in Visual Studio 2017

1. Launch Visual Studio and ensure your Azure Functions and Web Jobs Tools are updated to at version 15.0.31201.0
  1. Tools > Extensions and Updates > Expand the Updates node and select Visual Studio Marketplace
  1. Select Azure Functions and Web Jobs Tools and click the "Update" button
  <br/><img src="resources/vs_update_azure_tools.jpg" width="75%" />
  1. Close out and exit Visual Studio so the update can install
     1. Click the "Modify" button when the prompt shows
  1. Once the update completes, restart Visual Studio
1. In Visual Studio, click File > Open > Project/Solution... and select the empty solution located in the cloned repo `NikeHack/src/StartHere/MyBackendApp.sln`
1. Right-click on the Solution node in Solution Explorer and select Add > New Project...
<br/><img src="resources/vs_add_new_project_menu.png" width="75%" />
1. Under Visual C#, choose Cloud > Azure Functions
<br/><img src="resources/vs_new_functions_app_project.png" width="75%" />
1. Select Azure Functions v1 (.NET Framework) from the dropdown and Http Trigger as the trigger type
1. Select "Anonymous" from Access rights dropdown and click OK
<br/><img src="resources/vs_new_functions_app_project_settings.png" width="75%" />
   1. this will create a boilerplate function called `Function1` 1. we will replace this function with a real one shortly
1. Test out the function by clicking the Run/Debug button in Visual Studio
   1. You may be prompted to download and install the Azure Functions CLI tools 1. click the "Yes" button to accept
1. Use Postman or a browser to make a GET request to the function (e.g. `http://localhost:7071/api/Function1?name=Rob`) and verify the ouput `Hello, Rob`, for example
<br/><img src="resources/postman_verify_function1.png" width="75%" />


### Step 4: Publish your app to the cloud and verify

1. Right-click on your Functions project and select "Publish..."
1. Select "Azure Function App" and "Select Existing"
1. Click the Settings icon and select "Create Profile"
1. Click the "Create Profile" button
<br/><img src="resources/vs_new_publish_profile.png" width="75%"/>
1. Select your Azure Subscription from the dropdown
1. Expand your the Resource Group and select the Function App you created in step 2
1. Click "OK"
<br/><img src="resources/vs_create_publish_profile_select_appservice.png" width="75%" />
1. (If you plan to attach a remote debugger)
   1. Click "Manage Profile Settings"
   <br/><img src="resources/vs_publish_profile_settings_link.png" width="65%" />
   1. Change the Configuration to "Debug" and click "Save"
   <br/><img src="resources/vs_publish_profile_settings.png" width="50%" />
1. Click the "Publish" button
   1. If you get a warning indicating the version remotely doesn't match the local version, accept by clicking "Yes"
1. Copy the site URL and verify the function is running by using Postman to send that same GET request against the remote instance (e.g. `http://myfunctionsapp.azurewebsites.net/api/Function1?name=Rob`) and verify the ouput `Hello, Rob`
<br/><img src="resources/vs_publish_profile_site_link.png" width="65%" />


### Step 5: Create your Storage Account (Blob)

1. Once again, browse to [https://portal.azure.com](https://portal.azure.com)
1. Click "Create Resource" from the menu on the left of the page, then search for "Storage Account" and click "Create"
<br/><img src="resources/portal_create_new_storage_account.png" width="75%" />
1. Enter in a name for the storage account (e.g. `mystorageaccount`)
1. From th "Account Kind" dropdown, select "Blob"
1. Choose your Azure Subscription
1. Choose "Use existing", and select the resource group you created in the previous section
1. Choose a region (any region is fine)
1. Click "Create" to create the Storage Account
<br/><img src="resources/portal_create_new_storage_account_settings.png" width="75%" />
1. Once your storage account has been created, we need to create a new container to house our images
   1. Jump back into it and select "Browse blobs"
   1. Click the "+ Container" button to create a new container to house the image blobs we'll upload
   1. Name the container `images` and set the Public access level to __Blob (anonymous read access for blobs only)__
   <br/><img src="resources/portal_storage_account_create_container.png" width="75%" />


### Step 6: Generate a Shared Access Signature for your Storage Account

> __Note:__ We use a SAS URL to grant read/write access for a specified amount of time. We'll be using this from within a function.

1. Go to your storage account 1. you can search for this or go to Resource Groups > select yours > click on your storage account in the list
1. Click on Shared Access Signature
1. Leave the default settings as is but ensure the "End Date" is set to 24hrs from now or however long you want the Functions App to have access
1. Click the "Generate SAS" button
<br/><img src="resources/portal_storage_account_generate_sas_url.png" width="75%" />
1. Copy the "Blob service SAS URL" to somewhere safe - we'll be using this to allow our function limited access to read and write to our storage account (e.g. `https://mynewstorageaccountblob.blob.core.windows.net/?sv=2017-04-17&ss=b&srt=sco&sp=rwdlac&se=2018-01-05T23:03:17Z&st=2018-01-04T23:03:17Z&spr=https&sig=HIE2i2WSliZkiQ8lAw0qjykZm1VbywZdAGbUhrWfl%2BI%3D`)

> __Note:__ This can also be done programatically to generate the URLs on demand. Check out [this code](https://github.com/rob-derosa/Hunt/blob/master/Hunt.Backend.Functions/Functions/GetStorageToken.cs#L81) as a reference.


### Step 7: Upload an image/byte[] to your blob storage account

1. Back in Visual Studio, change the method name and file name of `Function1` to `MakePrediction`
1. Also change the value of the `FunctionName` attribute to `nameof(MakePrediction)`
1. In the method signature, change the HttpTrigger methods params from `"post", "get"` to just `"post"`
1. Right-click on the project's "Dependencies" node and choose "Manage Nuget Packages..."
1. Click on the "Browse" tab and search for "Azure Storage"
1. Select the "WindowsAzure.Storage" and change the version dropdown to v8.3.0 and click "Install"
1. Follow the instructions in [this gist](https://gist.github.com/rob-derosa/87e59e3dac93882f29f8fd4fa246ff3d)
1. Add the missing using statements
1. Back in Visual Studio, update line 43 to incorporate both your storage SAS URL and the container name
1. Build and run the project locally
1. Verify this by using Postman to send a POST request to your local endpoint
    1. Set the method dropdown to `POST`
    1. Set your endpoint in the address bar
    1. Set the body content by selecting the "Body" tab and choose the "binary" option
    1. Choose a local image file to send
    1. Send the request and verify a URL comes back
    1. Copy and past this URL into a browser and validate the image is the one you posted
      1. Note: you can also verify in the Azure portal by clicking "Browse blobs" and selecting the "images" container
1. Publish to the cloud and verify this remotely following the steps outlined above


### Step 8: Create a Azure Cosmos DB

1. Once again, browse to [https://portal.azure.com](https://portal.azure.com)
1. In the top left, click Create Resource > Databases > Azure Cosmos DB
<br/><img src="resources/portal_create_new_cosmos_database.png" />
1. Enter in a name for the database (e.g. `mycosmosdatabase`)
1. Choose the `SQL` from the API dropdown 1. this will use DocumentDB under the hood
1. Choose your Azure Subscription
1. Choose "Use existing", and select the resource group you created in the previous section
1. Choose a region (any region is fine)
1. Click "Create" to create the Cosmos DB
<br/><img src="resources/portal_create_new_cosmos_database_settings.png" />
1. It can take a few minutes before this process completes


### Step 9: Write a document to your Cosmos SQL Database

1. Back in Visual Studio, right-click on the project's "Dependencies" node and choose "Manage Nuget Packages..."
1. Click on the "Browse" tab and search for "DocumentDB"
1. Select the "Microsoft.Azure.DocumentDB.Core" and ensure version 1.7.1 is selected and click "Install"
1. Add the MyCommonLibrary project to your solution
   1. Right-click on the solution and select Add > Existing Project... and navigate to the parent folder and select MyCommonLibrary.csproj
1. Add a reference to the MyCommonLibrary project to your Function App project
   - __Note:__ This is a .NET Standard library that contains classes/models shared between the front and back ends
   1. Right click on the Dependencies node of the Functions app and select Add Reference...
   1. On the left side, choose Projects and check the box next to MyCommonLibrary
   1. Click the "OK" button
   <br/><img src="resources/vs_add_reference_common.png" width="75%" />
1. Right-click on your Functions app project and select Add > New Class... and name it "CosmosDataService.cs"
1. Replace the empty class with the code from [this gist](https://gist.github.com/rob-derosa/d38e6a7c1bdba90b101a3e9ad7b6dfb6)
1. Add the missing using statements
1. The `_databaseId` is the name of your database and the `_collectionId` is then name of the table/collection - feel free to change these
1. Check out the method `EnsureDatabaseConfigured` - it's responsible for creating your database and collection if either do not exist
1. Now we need to get some values from the Azure Portal, such as our database URL and SAS key
   1. In the Azure portal, navigate to your Cosmos DB
   1. Click on Keys
   1. In the Read-write Keys tab, copy the URI and paste it as the value of `_databaseUrl` in CosmosDataService.cs
   1. In the Read-write Keys tab, copy the Primary Key and paste it as the value of `_databaseKey` in CosmosDataService.cs
<br/><img src="resources/portal_cosmos_keys.png" />
1. In MakePrediction.cs, replace the code in the `try` clause with the code in [this gist](https://gist.github.com/rob-derosa/240dd977d41ebc58f633dbdf009d5e63)
1. Build and run your project locally
1. Use Postman to make the same local POST request as in the previous step
1. Verify the data was inserted by inspecting the Prediction JSON object that is returned.
   - You can also see the documents in your database by navigating to the Data Explorer from your Cosmos DB account and expanding down to your collection documents
1. Publish to the cloud and re-verify


### Step 10: Set up your own Custom Vision project and train a classifier

In this step, we will create a new Custom Vision project and classify a few different models (objects) by uploading various photos of the models with associated tags. Then we will train the classifier to identify/predict those objects with a probability against each tag.

1. Browse to http://customvision.ai and sign-in to create a new project
1. Click the "New Project" button
1. Complete the form by choosing a name for your project, such as "ShoesProject"
1. You can leave the Domain as General and click the "Create project" button
<br/><img src="resources/portal_cv_new_project.png" width="75%" />
1. Once your project has been created, it's time to upload some images of a model/shoe you intend to train.
    - __Tip:__ While the minimum photos-per-model count is 5, the classifier becomes more accurate as you add more photos - it is recommended to add at least 10 photos or more if you have the time
    - __Tip:__ Add varying photos of the object in different angles, perspective, lighting and environments
   1. Using your phone's camera, take at least 10 images of the object you wish to train and transfer them to your computer for upload
   1. Click the "Add images" button
   <br/><img src="resources/portal_cv_add_images.png" width="75%" />
   1. Click the "Browse local files" button and select the images you snapped in the previous step
   1. You will be asked to add some tags to the images you are uploading - enter at least 2 tags
   1. Click the "Upload files" button and verify your model was tagged properly by checking the boxes of those specific tags and validating the image count
   <br/><img src="resources/portal_cv_add_tags.png" width="75%" />
   1. Repeat this process to until you have at least 4 total models with at least 2 tag each
1. Once you have at least 4 models, click the Train Iteration button at the top
1. This will train your first iteration of the classifier (iterations are versioned and each project can have a default iteration)
<br/><img src="resources/portal_cv_train_iteration.png" width="75%" />
1. Let's test out this iteration running a quick test - start by clicking the "Quick Test" button at the top
<br/><img src="resources/portal_cv_train_iteration_results.png" width="75%" />
1. Take another photo of one of the objects and transfer it to your computer
1. Select the photo your just snapped and choose it for upload
1. Once the photo has been uploaded, you will see the results of the test below - ideally, you'll have the correct set of tags with a high probability
<br/><img src="resources/portal_cv_quick_test_results.png" width="75%" />
1. As you add additional images with tags, you'll need to re-train your classifier by repeating the steps above. It is also recommended to set the latest iteration of your trained classifier to be the default iteration for the project. You can do this by:
   1. Click on the "Performance" tab at the top
   1. Select the iteration on the left you want to make the default
   1. Click on the "Make default" button towards the top to set it as default
   <br/><img src="resources/portal_cv_default_iteration.png" width="75%" />


### Step 11: Make a prediction with Custom Vision and log the results

Now that we have a custom vision project with at least 4 trained models, we can begin to make predictions against the classifier. For this, we will need the custom vision project ID as well as the training key.

1. In Visual Studio, right-click on the project's "Dependencies" node and choose "Manage Nuget Packages..."
1. Click on the "Browse" tab and search for "CustomVision"
1. Select the "Microsoft.Cognitive.CustomVision.Prediction" and change the version dropdown to v1.0.0 and click "Install"
1. Select the "Microsoft.Cognitive.CustomVision.Training" and change the version dropdown to v1.0.0 and click "Install"
1. In MakePrediction.cs, replace the code within the `try` clause with the code in [this gist](https://gist.github.com/rob-derosa/07952c26d0bc7e78b974cae5ddec1485)
1. Add the missing using statements
1. Add your project ID and training key to lines 30 and 31
   1. To get your custom vision project ID and training key, go to your project at http://customvision.ai
   1. Navigate to the project settings and you'll find your project ID and training key
   <br/><img src="resources/portal_cv_settings.png" width="75%" />
1. You've just added a chunk of code that will now run a prediction against the image you upload and return a list of resulting tags and their probability
1. Build and run locally
1. Use Postman to make the same local POST request as in the previous step
1. Verify the data was inserted by inspecting the Prediction JSON object that is returned which should include some matching tags
1. Publish to the cloud and re-verify
<br/><img src="resources/postman_get_predictions_result.png" width="75%" />


### Step 12: Connect the mobile front-end to the Functions backend

__Note:__ If you are unable to build and deploy the mobile app, skip to Step 12.b to install the app directly on your iOS or Android device.

__Note:__ You can build the mobile front end using either Visual Studio for Windows or Visual Studio for Mac. If you are targeting iOS, you must have a Mac - either as the main IDE or bridged to your PC running Visual Studio for Windows.

1. In another instance of Visual Studio, open the MyMobileApp.sln file sitting in the root folder - this will open a Xamarin Forms project targeting iOS and Android
1. Open the App.xaml.cs file and set your Azure Function endpoint on line 17.
1. Right-click on either the iOS or Android project and select "Set as Startup Project", then click "Rebuild"
1. Select a device or emulator from the drop-down list in the top toolbar and click the Start Debug button
   - It can take several minutes to compile and begin the debugging session
1. Once your debugging session begins, if using an actual device:
   1. Tap the "Take Photo" button to take a photo and snap a pic of one of the 4 models you have trainied your classifier on
   1. Tap the "Make Prediction" button
1. If you are using an emulator and cannot use a camera:
      1. Using the browser in the emulator, search for one of the 4 models you have trainied your classifier on
      1. Save the image to your local photo library
      1. Launch the mobile app and tap on the "Take Picture" button
      1. Select the image you downloaded in the previous step
1. Ideally, a match will be found and the matching tags will be displayed
1. Click the Reset button in the top right to reset the data and make another prediction


#### Step 12.b: Install a prebuilt version of the mobile app

__Note:__ This step is for those that cannot build the mobile app - it is configurable to point to any Azure endpoint

1. On the device you want to install the app on, navigate to http://install.appcenter.ms/nikehackathon
1. Click on the "Install" button
1. If the device is iOS:
   1. You will need to trust the enterprise signing certificate by by opening the Settings App
   1. Tap on General and scroll down to tap on Profiles & Device Management
   1. Locate the "Xamarin Inc." profile under the Enterprise Apps section and tap on it
   1. Tap the "Trust Xamarin Inc" button and confirm by tapping "Trust" again
   1. Go to the home screen and you should see the app installing/installed
1. Launch the app and click on the Settings tab
1. Enter your Functions App endpoint url in the textbox and tap Save (e.g. `http://myfunctionsapp.azurewebsites.net`)


### Step 13: Add another function that returns a list of past predictions

1. In Visual Studio in the MyFunctionsApp solution, add a new function to your functions project
   1. Right-click on your functions project and selct Add > Class... > Azure Function
   1. Name the function "GetPredictions" and click the "Add" button
   1. Leave the trigger type as Http trigger but change the Access rights to "Anonymous"
   1. Click the "OK" button
1. Replace the class code with the contents in [this gist](https://gist.github.com/rob-derosa/6c0857be11ac0c66cf62386fa33c5de8)
1. Add the missing `using` statements and note the use of a `Route` to automatically pull data in from the URL, in this case the `databaseId` and `collectionId`
1. Because we are using a RESTful approach to the URLs, we need to modify the host.json file to remove the default route prefix of `api/`
   1. Open the "host.json" file and add the following code from [this gist](https://gist.github.com/rob-derosa/3505273e2948429397d23c87b2ea34c8)
   1. This will make it so our `GetPredictions` function does not have to have the api prefix (eg `api/123456-7890-...`)
   1. But now that our route prefix no longer has the default `api/`, we need to update our MakePrediction.cs function route to make up for that.
      1. Open the MakePrediction.cs file and change the `Route` property in the `HttpTrigger` attribute from `null` to `"api/MakePrediction"`
1. Build and run your project locally
1. Verify the `GetPredictions` function using Postman by making a `GET` request to your function
1. Set your endpoint in the address bar (e.g. `http://localhost:7071/myDatabaseId/myCollectionId`)
1. Verify an array of `Prediction` JSON data is returned
1. Publish to the cloud and re-verify


### Step 14: Add a new tabbed page to the mobile app and display a list of past predictions

1. Steps to come...


#### Attaching a remote debugger to your Azure Functions App

1. Before we can attach a remote debugger, we need to ensure the "Enable Just My Code" option in the Debugging section is disabled
   1. In Visual Studio 2017, click on Tools > Options... > Click on the Debugging node
   1. Look for the checkbox labeled "Enable Just My Code" and ensure it is UNCHECKED
   <br/><img src="resources/vs_disable_justmycode.png" width="75%" />
1. In Visual Studio 2017, open the Server Explorer window: View > Server Explorer
1. Expand your Azure subscription node > App Service > YourResourceGroup > YourFunctionsApp
1. Right Click on YourFunctionsApp node and select Attach Debugger
<br/><img src="resources/vs_attach_debugger.png" width="50%" />
1. Set a breakpoint in code where you want the debugger to pause


#### References

- Azure Functions
- Cosmos DB
- Custom Vision
   - [Web Portal](http://customvision.ai)
   - [C# API](https://docs.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/csharp-tutorial)
   - [REST API - Prediction](https://go.microsoft.com/fwlink/?linkid=865445)
   - [REST API - Training](https://go.microsoft.com/fwlink/?linkid=865446)
   - [Tips on improving classifiers](https://docs.microsoft.com/en-us/azure/cognitive-services/custom-vision-service/getting-started-improving-your-classifier)