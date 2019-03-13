//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.Data.DataView;
using SampleRegression.Model.DataModels;

namespace SampleRegression.Predict
{
    class Program
    {       
        //Machine Learning model to load and use for predictions
        private const string MODEL_RELATIVE_PATH = @"MLModel.zip";

        //Dataset used just for trying a single prediction with some data 
        private const string DATA_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-test.csv";

        static void Main(string[] args)
        {
            var mlContext = new MLContext();

            // Create sample data to do a single prediction with it 
            SampleObservation sampleData = CreateSampleData(mlContext);

            // Load the ML model .zip file
            ITransformer mlModel;
            using (var stream = new FileStream(MODEL_RELATIVE_PATH, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                mlModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded ML model          
            var predEngine = mlModel.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);

            // Try a single prediction
            var predictionResult = predEngine.Predict(sampleData);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sampleData.Fare_amount} | Predicted value: {predictionResult.Score}");
            Console.WriteLine($"==================================================");

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        // Method to load single row of data to try a single prediction
        // You can change this code and create your own sample data here (Hardcoded or from any source)
        private static SampleObservation CreateSampleData(MLContext mlContext)
        {          
            // Read dataset to get a single row for trying a prediction          
            IDataView dataView = mlContext.Data.LoadFromTextFile<SampleObservation>(
                                            path: DATA_PATH,
                                            hasHeader: true,
                                            separatorChar: ',');

            // Here you could provide new test data, hardcoded or from the end-user application, instead of the row from the file.
            SampleObservation sampleForPrediction = mlContext.Data.CreateEnumerable<SampleObservation>(dataView, false)
                                                                        .First();
            return sampleForPrediction;
        }

    }
}
