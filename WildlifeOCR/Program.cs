// <snippet_imports>
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
// </snippet_imports>

/*
Prerequisites:
1. Install the Custom Vision SDK. See:
https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training/
https://www.nuget.org/packages/Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction/
2. Download the images used by this sample from:
https://github.com/Azure-Samples/cognitive-services-sample-data-files/tree/master/CustomVision/ImageClassification/Images
3. Copy the Images folder from the Github repo to your project's output directory.
This sample looks for images in the following paths:
<your project's output directory>/Images/Type 1/coyote
<your project's output directory>/Images/Type 1/bobcat
<your project's output directory>/Images/Test
For example, if your project targets .NET Core 3.1 and your build type is Debug, copy the Images folder to <your project directory>/bin/Debug/netcoreapp3.1.
*/

namespace ImageClassification
{
    class Program
    {
        // <snippet_creds>
        // You can obtain these values from the Keys and Endpoint page for your Custom Vision resource in the Azure Portal.
        private static string trainingEndpoint = "https://wildlifeocrcustomvision2.cognitiveservices.azure.com/";
        private static string trainingKey = "211b6e9b4b184032bace329ae8952ddd";
        // You can obtain these values from the Keys and Endpoint page for your Custom Vision Prediction resource in the Azure Portal.
        private static string predictionEndpoint = "https://wildlifeocrcustomvision2-prediction.cognitiveservices.azure.com/";
        private static string predictionKey = "30e83494a3594beea35bd7208e67e076";
        // You can obtain this value from the Properties page for your Custom Vision Prediction resource in the Azure Portal. See the "Resource ID" field. This typically has a value such as:
        // /subscriptions/<your subscription ID>/resourceGroups/<your resource group>/providers/Microsoft.CognitiveServices/accounts/<your Custom Vision prediction resource name>
        private static string predictionResourceId = "/subscriptions/cfd4703a-689b-4df7-9d89-da3229b26c28/resourceGroups/MyResourceGroup/providers/Microsoft.CognitiveServices/accounts/Wildlifeocrcustomvision2-Prediction";

        private static List<string> coyoteImages;
        private static List<string> bobcatImages;
        private static Tag coyoteTag;
        private static Tag bobcatTag;
        private static Iteration iteration;
        private static string publishedModelName = "coyoteAndBobcatModel";
        private static MemoryStream testImage;
        // </snippet_creds>

        static void Main(string[] args)
        {
            // <snippet_maincalls>
            CustomVisionTrainingClient trainingApi = AuthenticateTraining(trainingEndpoint, trainingKey);
            CustomVisionPredictionClient predictionApi = AuthenticatePrediction(predictionEndpoint, predictionKey);

            //Project project = CreateProject(trainingApi);

            //get the projectguid from the URL in customVision
            Guid projectId = new Guid("d7f0bedb-b57c-47b4-9b8a-0d43f2a85fab");

            //AddTags(trainingApi, project);
            //UploadImages(trainingApi, project);
            //TrainProject(trainingApi, project);
            //PublishIteration(trainingApi, project);
            TestIteration(predictionApi, projectId);
            //DeleteProject(trainingApi, project);
            // </snippet_maincalls>
        }

        // <snippet_auth>
        private static CustomVisionTrainingClient AuthenticateTraining(string endpoint, string trainingKey)
        {
            // Create the Api, passing in the training key
            CustomVisionTrainingClient trainingApi = new CustomVisionTrainingClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training.ApiKeyServiceClientCredentials(trainingKey))
            {
                Endpoint = endpoint
            };
            return trainingApi;
        }
        private static CustomVisionPredictionClient AuthenticatePrediction(string endpoint, string predictionKey)
        {
            // Create a prediction endpoint, passing in the obtained prediction key
            CustomVisionPredictionClient predictionApi = new CustomVisionPredictionClient(new Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.ApiKeyServiceClientCredentials(predictionKey))
            {
                Endpoint = endpoint
            };
            return predictionApi;
        }
        // </snippet_auth>

        // <snippet_create>
        private static Project CreateProject(CustomVisionTrainingClient trainingApi)
        {

            // Create a new project
            Console.WriteLine("Creating new project:");
            return trainingApi.CreateProject("Gwen Type 1 Images - b");
            //return trainingApi.GetProject(new Guid("c05de83e-52db-4c1e-abb8-8a48c7fbda57"e.CognitiveServices.Vision.CustomVision.Training.Models.CustomVisionErrorException has ));
        }
        // </snippet_create>
        // <snippet_addtags>
        private static void AddTags(CustomVisionTrainingClient trainingApi, Project project)
        {
            // Make two tags in the new project
            coyoteTag = trainingApi.CreateTag(project.Id, "coyote");
            bobcatTag = trainingApi.CreateTag(project.Id, "bobcat");
        }
        // </snippet_addtags>

        // <snippet_upload>
        private static void UploadImages(CustomVisionTrainingClient trainingApi, Project project)
        {
            // Add some images to the tags
            Console.WriteLine("\tUploading images");
            LoadImagesFromDisk();

            // Images can be uploaded one at a time
            foreach (var image in coyoteImages)
            {
                using (var stream = new MemoryStream(File.ReadAllBytes(image)))
                {
                    trainingApi.CreateImagesFromData(project.Id, stream, new List<Guid>() { coyoteTag.Id });
                }
            }

            // Or uploaded in a single batch 
            var imageFiles = bobcatImages.Select(img => new ImageFileCreateEntry(Path.GetFileName(img), File.ReadAllBytes(img))).ToList();
            trainingApi.CreateImagesFromFiles(project.Id, new ImageFileCreateBatch(imageFiles, new List<Guid>() { bobcatTag.Id }));

        }
        // </snippet_upload>

        // <snippet_train>
        private static void TrainProject(CustomVisionTrainingClient trainingApi, Project project)
        {
            // Now there are images with tags start training the project
            Console.WriteLine("\tTraining");
            iteration = trainingApi.TrainProject(project.Id);

            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Console.WriteLine("Waiting 10 seconds for training to complete...");
                Thread.Sleep(10000);

                // Re-query the iteration to get it's updated status
                iteration = trainingApi.GetIteration(project.Id, iteration.Id);
            }
        }
        // </snippet_train>

        // <snippet_publish>Microsoft.Azure.CognitiveServices.Vision.CustomVisio
        private static void PublishIteration(CustomVisionTrainingClient trainingApi, Project project)
        {
            trainingApi.PublishIteration(project.Id, iteration.Id, publishedModelName, predictionResourceId);
            Console.WriteLine("Done!\n");

            // Now there is a trained endpoint, it can be used to make a prediction
        }
        // </snippet_publish>

        // <snippet_test>
        private static void TestIteration(CustomVisionPredictionClient predictionApi, Guid projectId)
        {

            // Make a prediction against the new project
            Console.WriteLine("Making a prediction:");

            var testImage = new MemoryStream(File.ReadAllBytes(Path.Combine("Images", "Test", "MFDC0687.jpg")));
            var result = predictionApi.ClassifyImage(projectId, publishedModelName, testImage);

            // Loop over each prediction and write out the results
            foreach (var c in result.Predictions)
            {
                Console.WriteLine($"\t{c.TagName}: {c.Probability:P1}");
                DataTable dt = createDataTable(c);
                StringBuilder sb = new StringBuilder();
                foreach (DataRow dr in dt.Rows)
                {
                    foreach (DataColumn dc in dt.Columns)
                        sb.Append(FormatCSV(dr[dc.ColumnName].ToString()) + ",");
                        sb.Remove(sb.Length - 1, 1);
                        sb.AppendLine();
                }
                File.AppendAllTextAsync("first-sample.csv", sb.ToString());

            }
        }
        // </snippet_test>

        private static DataTable createDataTable(Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction.Models.PredictionModel c)
        {
            DataTable table = new DataTable();
            //columns  
            table.Columns.Add("Tag Name", typeof(string));
            table.Columns.Add("Probability", typeof(double));
            table.Columns.Add("Greater then 96%", typeof(double));

            //data  
            table.Rows.Add(c.TagName, c.Probability, c.Probability.CompareTo(96));

            return table;
        }

        public static string FormatCSV(string input)
        {
            try
            {
                if (input == null)
                    return string.Empty;

                bool containsQuote = false;
                bool containsComma = false;
                int len = input.Length;
                for (int i = 0; i < len && (containsComma == false || containsQuote == false); i++)
                {
                    char ch = input[i];
                    if (ch == '"')
                        containsQuote = true;
                    else if (ch == ',')
                        containsComma = true;
                }

                if (containsQuote && containsComma)
                    input = input.Replace("\"", "\"\"");

                if (containsComma)
                    return "\"" + input + "\"";
                else
                    return input;
            }
            catch
            {
                throw;
            }
        }

        // <snippet_loadimages>
        private static void LoadImagesFromDisk()
        {
            // this loads the images to be uploaded from disk into memory
            coyoteImages = Directory.GetFiles(Path.Combine("Images/Type 1", "Coyote")).ToList();
            bobcatImages = Directory.GetFiles(Path.Combine("Images/Type 1", "Bobcat")).ToList();
            testImage = new MemoryStream(File.ReadAllBytes(Path.Combine("Images", "Test", "MFDC0687.jpg")));
        }
        // </snippet_loadimages>
        // <snippet_delete>
        private static void DeleteProject(CustomVisionTrainingClient trainingApi, Project project)
        {
            // Delete project. Note you cannot delete a project with a published iteration; you must unpublish the iteration first.
            Console.WriteLine("Unpublishing iteration.");
            trainingApi.UnpublishIteration(project.Id, iteration.Id);
            Console.WriteLine("Deleting project.");
            trainingApi.DeleteProject(project.Id);
        }
        // </snippet_create>
    }
}