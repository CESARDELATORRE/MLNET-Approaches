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
using Microsoft.ML.LightGBM;
using SampleRegression.Model.DataModels;
using Microsoft.ML.Transforms;

namespace Sample
{
    class Program
    {
        private const string TRAIN_DATA_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-train.csv";
        private const string TEST_DATA_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-test.csv";

        // The trained model file will be generated in the app's execution folder
        private const string MODEL_RELATIVE_PATH = @"../../../../SampleRegression.Model/MLModel.zip";

        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 1);

            // Load Data
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<SampleObservation>(TRAIN_DATA_PATH, hasHeader: true, separatorChar: ',');
            IDataView testDataView = mlContext.Data.LoadFromTextFile<SampleObservation>(TEST_DATA_PATH, hasHeader: true, separatorChar: ',');

            // Train Model
            (ITransformer model, string trainerName) = TrainModel(mlContext, trainingDataView);

            // Evaluate quality of Model
            EvaluateModel(mlContext, model, testDataView, trainerName);

            // (Optional) Try a single prediction (Only one case with first row of DataSet)
            TrySinglePrediction(mlContext, model, testDataView);

            // Save model
            SaveModel(mlContext, model, MODEL_RELATIVE_PATH);

            ConsoleHelper.ConsoleWriteHeader("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        public static (ITransformer model, string trainerName) TrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            // Data process configuration with pipeline data transformations 
            IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[] { new OneHotEncodingEstimator.ColumnOptions("vendor_id", "vendor_id"), new OneHotEncodingEstimator.ColumnOptions("payment_type", "payment_type") })
                                      .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features, new[] { "vendor_id", "payment_type", "rate_code", "passenger_count", "trip_time_in_secs", "trip_distance" }));

            // Set the training algorithm
            IEstimator<ITransformer> trainer = mlContext.Regression.Trainers.LightGbm(new Options() { NumBoostRound = 200, LearningRate = 0.02864992f, NumLeaves = 57, MinDataPerLeaf = 1, UseSoftmax = false, UseCat = false, UseMissing = true, MinDataPerGroup = 100, MaxCatThreshold = 16, CatSmooth = 20, CatL2 = 10, LabelColumn = "fare_amount", FeatureColumn = "Features" });
            IEstimator<ITransformer> trainingPipeline = dataProcessPipeline.Append(trainer);

            ConsoleHelper.ConsoleWriteHeader("=============== Training model ===============");

            ITransformer model = trainingPipeline.Fit(trainingDataView);

            ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");
            return (model, trainer.ToString());
        }

        private static void EvaluateModel(MLContext mlContext, ITransformer model, IDataView testDataView, string trainerName)
        {
            // Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = model.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, "fare_amount", "Score");
            ConsoleHelper.PrintRegressionMetrics(trainerName, metrics);
        }

        // (OPTIONAL) Try/test a single prediction with the trained model and any test data
        private static void TrySinglePrediction(MLContext mlContext, ITransformer model, IDataView dataView)
        {
            // Load data to test. Could be any test data. Since this is generated code, a row from a dataView is used
            // But here you can try with any sample data to make a prediction
            var sample = mlContext.Data.CreateEnumerable<SampleObservation>(dataView, false).First();

            // Create prediction engine related to the loaded trained model
            var predEngine = model.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);

            //Score
            var resultprediction = predEngine.Predict(sample);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sample.Fare_amount} | Predicted value: {resultprediction.Score}");
            Console.WriteLine($"==================================================");
        }

        private static void SaveModel(MLContext mlContext, ITransformer model, string modelRelativePath)
        {
            // Save/persist the trained model to a .ZIP file
            Console.WriteLine($"=============== Saving the model  ===============");
            using (var fs = new FileStream(GetAbsolutePath(modelRelativePath), FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(model, fs);

            Console.WriteLine("The model is saved to {0}", GetAbsolutePath(modelRelativePath));
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}

