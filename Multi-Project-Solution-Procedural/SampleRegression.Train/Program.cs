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
using Microsoft.ML.LightGBM;
using Microsoft.Data.DataView;
using SampleRegression.Model.DataModels;
using Microsoft.ML.Transforms;

namespace Sample
{
    class Program
    {
        private const string TRAIN_DATA_FILEPATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-train.csv";
        private const string TEST_DATA_FILEPATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-test.csv";

        // The trained model file will be generated in the app's execution folder
        private const string MODEL_FILEPATH = @"../../../../SampleRegression.Model/MLModel.zip";

        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            MLContext mlContext = new MLContext(seed: 1);

            // Load Data
            IDataView trainingDataView = mlContext.Data.LoadFromTextFile<SampleObservation>(TRAIN_DATA_FILEPATH, hasHeader: true, separatorChar: ',');
            IDataView testDataView = mlContext.Data.LoadFromTextFile<SampleObservation>(TEST_DATA_FILEPATH, hasHeader: true, separatorChar: ',');

            // Train Model
            ITransformer mlModel = TrainModel(mlContext, trainingDataView);

            // Evaluate quality of Model
            EvaluateModel(mlContext, mlModel, testDataView);

            // Save model
            SaveModel(mlContext, mlModel, MODEL_FILEPATH);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        public static ITransformer TrainModel(MLContext mlContext, IDataView trainingDataView)
        {
            // Data process configuration with pipeline data transformations 
            IEstimator<ITransformer> dataProcessPipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[] { new OneHotEncodingEstimator.ColumnOptions("vendor_id", "vendor_id"), new OneHotEncodingEstimator.ColumnOptions("payment_type", "payment_type") })
                                      .Append(mlContext.Transforms.Concatenate(DefaultColumnNames.Features, new[] { "vendor_id", "payment_type", "rate_code", "passenger_count", "trip_time_in_secs", "trip_distance" }));

            // Set the training algorithm
            IEstimator<ITransformer> trainer = mlContext.Regression.Trainers.LightGbm(new Options() { NumBoostRound = 200, LearningRate = 0.02864992f, NumLeaves = 57, MinDataPerLeaf = 1, UseSoftmax = false, UseCat = false, UseMissing = true, MinDataPerGroup = 100, MaxCatThreshold = 16, CatSmooth = 20, CatL2 = 10, LabelColumn = "fare_amount", FeatureColumn = "Features" });
            IEstimator<ITransformer> trainingPipeline = dataProcessPipeline.Append(trainer);

            Console.WriteLine("=============== Training " + trainer.ToString() + " model ===============");

            ITransformer model = trainingPipeline.Fit(trainingDataView);

            Console.WriteLine("=============== End of training process ===============");
            return model;
        }

        private static void EvaluateModel(MLContext mlContext, ITransformer mlModel, IDataView testDataView)
        {
            // Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            IDataView predictions = mlModel.Transform(testDataView);
            RegressionMetrics metrics = mlContext.Regression.Evaluate(predictions, "fare_amount", "Score");
            ConsoleHelper.PrintRegressionMetrics(metrics);
        }

        private static void SaveModel(MLContext mlContext, ITransformer mlModel, string modelRelativePath)
        {
            // Save/persist the trained model to a .ZIP file
            Console.WriteLine($"=============== Saving the model  ===============");
            using (var fs = new FileStream(GetAbsolutePath(modelRelativePath), FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(mlModel, fs);

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
