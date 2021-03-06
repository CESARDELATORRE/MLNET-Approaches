//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using SampleBinaryClassification.Model.DataModels;


namespace SampleBinaryClassification.ConsoleApp
{
    class Program
    {
        //Machine Learning model to load and use for predictions
        private const string MODEL_FILEPATH = @"MLModel.zip";

        //Dataset to use for predictions 
        private const string DATA_FILEPATH = @"D:\DevSpikes\AutoML-CLI\train.csv";

        static void Main(string[] args)
        {
            MLContext mlContext = new MLContext();

            ITransformer mlModel = mlContext.Model.Load(MODEL_FILEPATH, out DataViewSchema inputSchema);
            var predEngine = mlContext.Model.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlModel);

            // Create sample data to do a single prediction with it 
            SampleObservation sampleData = CreateSingleDataSample(mlContext, DATA_FILEPATH);

            // Try a single prediction
            SamplePrediction predictionResult = predEngine.Predict(sampleData);
            Console.WriteLine($"Single Prediction --> Actual value: {sampleData.Label} | Predicted value: {predictionResult.Prediction}");

            //Training code used by ML.NET CLI and AutoML to generate the model
            //ModelBuilder.CreateModel();

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        // Method to load single row of data to try a single prediction
        // You can change this code and create your own sample data here (Hardcoded or from any source)
        private static SampleObservation CreateSingleDataSample(MLContext mlContext, string dataFilePath)
        {
            // Read dataset to get a single row for trying a prediction          
            IDataView dataView = mlContext.Data.LoadFromTextFile<SampleObservation>(
                                            path: dataFilePath,
                                            hasHeader: true,
                                            separatorChar: ',',
                                            allowQuoting: true,
                                            allowSparse: false);

            // Here (SampleObservation object) you could provide new test data, hardcoded or from the end-user application, instead of the row from the file.
            SampleObservation sampleForPrediction = mlContext.Data.CreateEnumerable<SampleObservation>(dataView, false)
                                                                        .First();
            return sampleForPrediction;
        }
    }
}
