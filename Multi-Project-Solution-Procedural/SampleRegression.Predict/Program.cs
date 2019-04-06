
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
using System.Collections.Generic;


namespace SampleRegression.Predict
{
    class Program
    {
        //Machine Learning model to load and use for predictions
        private const string MODEL_FILEPATH = @"MLModel.zip";

        //Dataset to use for predictions 
        private const string DATA_FILEPATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-test.csv";

        //File to save multiple predictions 
        private const string PREDICTIONS_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-predictions.csv";

        static void Main(string[] args)
        {
            MLContext mlContext = new MLContext();

            //Load ML Model from .zip file
            ITransformer mlModel = LoadModelFromFile(mlContext, MODEL_FILEPATH);

            // Create sample data to do a single prediction with it 
            SampleObservation sampleData = CreateSingleDataSample(mlContext, DATA_FILEPATH);

            // Test a single prediction
            Predict(mlContext, mlModel, sampleData);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void Predict(MLContext mlContext, ITransformer mlModel, SampleObservation sampleData)
        {
            // Create prediction engine related to the loaded ML model          
            

            // Measuring CreatePredictionengine() time
            var watch = System.Diagnostics.Stopwatch.StartNew();

            var predEngine = mlModel.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);

            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"");

            Console.WriteLine($"++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"CreatePredictionEngine() takes: {elapsedMs.ToString()}");
            Console.WriteLine($"++++++++++++++++++++++++++++++++++++++++++");

            Console.WriteLine($"");

            // Measuring CreatePredictionengine() time
            var watch2 = System.Diagnostics.Stopwatch.StartNew();

            var predEngine2 = mlModel.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);

            watch.Stop();
            long elapsedMs2 = watch2.ElapsedMilliseconds;

            Console.WriteLine($"");

            Console.WriteLine($"++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"CreatePredictionEngine() takes: {elapsedMs2.ToString()}");
            Console.WriteLine($"++++++++++++++++++++++++++++++++++++++++++");

            Console.WriteLine($"");

            // Try a single prediction
            SamplePrediction predictionResult = predEngine.Predict(sampleData);

            Console.WriteLine($"Single Prediction --> Actual value: {sampleData.Fare_amount} | Predicted value: {predictionResult.PredictedFareAmount}");
        }

        private static ITransformer LoadModelFromFile(MLContext mlContext, string modelFilePath)
        {
            ITransformer mlModel;

            // Measuring CreatePredictionengine() time
            var watch = System.Diagnostics.Stopwatch.StartNew();
            using (var stream = new FileStream(modelFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                mlModel = mlContext.Model.Load(stream);
            }

            watch.Stop();
            long elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"");

            Console.WriteLine($"++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"Load model takes: {elapsedMs.ToString()}");
            Console.WriteLine($"++++++++++++++++++++++++++++++++++++++++++");

            Console.WriteLine($"");

            // Measuring CreatePredictionengine() time
            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            using (var stream = new FileStream(modelFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                mlModel = mlContext.Model.Load(stream);
            }

            watch.Stop();
            long elapsedMs2 = watch2.ElapsedMilliseconds;

            Console.WriteLine($"");

            Console.WriteLine($"++++++++++++++++++++++++++++++++++++++++++");
            Console.WriteLine($"Load model takes: {elapsedMs2.ToString()}");
            Console.WriteLine($"++++++++++++++++++++++++++++++++++++++++++");

            Console.WriteLine($"");

            return mlModel;
        }

        // Method to load single row of data to try a single prediction
        // You can change this code and create your own sample data here (Hardcoded or from any source)
        private static SampleObservation CreateSingleDataSample(MLContext mlContext, string dataFilePath)
        {
            // Read dataset to get a single row for trying a prediction          
            IDataView dataView = mlContext.Data.LoadFromTextFile<SampleObservation>(
                                            path: dataFilePath,
                                            hasHeader: true,
                                            separatorChar: ',');

            // Here (SampleObservation object) you could provide new test data, hardcoded or from the end-user application, instead of the row from the file.
            SampleObservation sampleForPrediction = mlContext.Data.CreateEnumerable<SampleObservation>(dataView, false)
                                                                        .First();
            return sampleForPrediction;
        }
    }
}

