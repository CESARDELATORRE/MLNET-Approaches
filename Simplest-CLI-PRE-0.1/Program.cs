// This is an auto generated file by ML.NET CLI

using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using Microsoft.Data.DataView;
using Microsoft.ML.LightGBM;
using Microsoft.ML.Transforms.Categorical;


namespace Sample
{
    class Program
    {
        private static string TrainDataPath = @"D:\DevSpikes\AutoML-CLI\taxi-fare-train.csv";
        private static string TestDataPath = @"D:\DevSpikes\AutoML-CLI\taxi-fare-test.csv";
        private static string ModelPath = @"..\..\..\model.zip";

        // Set this flag to enable the training process.
        private static bool EnableTraining = false;

        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // Set a random seed for repeatable/deterministic results across multiple trainings.
            var mlContext = new MLContext(seed: 1);

            if (EnableTraining)
            {
                // Create, Train, Evaluate and Save a model
                BuildTrainEvaluateAndSaveModel(mlContext);
                ConsoleHelper.ConsoleWriteHeader("=============== End of training process ===============");
            }
            else
            {
                ConsoleHelper.ConsoleWriteHeader("Skipping the training process. Please set the flag : 'EnableTraining' to 'true' to enable the training process.");
            }

            // Make a single test prediction loading the model from .ZIP file
            TestSinglePrediction(mlContext);

            ConsoleHelper.ConsoleWriteHeader("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();

        }

        private static ITransformer BuildTrainEvaluateAndSaveModel(MLContext mlContext)
        {
            // Data loading
            IDataView trainingDataView = mlContext.Data.ReadFromTextFile<SampleObservation>(
                                            path: TrainDataPath,
                                            hasHeader: true,
                                            separatorChar: ',',
                                            allowQuotedStrings: true,
                                            trimWhitespace: false,
                                            supportSparse: true);
            IDataView testDataView = mlContext.Data.ReadFromTextFile<SampleObservation>(
                                            path: TestDataPath,
                                            hasHeader: true,
                                            separatorChar: ',',
                                            allowQuotedStrings: true,
                                            trimWhitespace: false,
                                            supportSparse: true);

            // Common data process configuration with pipeline data transformations
            var dataProcessPipeline = mlContext.Transforms.Categorical.OneHotEncoding(new[] { new OneHotEncodingEstimator.ColumnInfo("vendor_id", "vendor_id"), new OneHotEncodingEstimator.ColumnInfo("payment_type", "payment_type") })
                                      .Append(mlContext.Transforms.Concatenate("Features", new[] { "vendor_id", "payment_type", "rate_code", "passenger_count", "trip_time_in_secs", "trip_distance" }));

            // Set the training algorithm, then create and config the modelBuilder  
            var trainer = mlContext.Regression.Trainers.LightGbm(new Options() { NumBoostRound = 200, LearningRate = 0.02864992f, NumLeaves = 57, MinDataPerLeaf = 1, UseSoftmax = false, UseCat = false, UseMissing = true, MinDataPerGroup = 100, MaxCatThreshold = 16, CatSmooth = 20, CatL2 = 10, Booster = new Options.TreeBooster.Arguments() { RegLambda = 0.5, RegAlpha = 1 }, LabelColumn = "fare_amount", FeatureColumn = "Features" });
            var trainingPipeline = dataProcessPipeline.Append(trainer);

            // Train the model fitting to the DataSet
            Console.WriteLine("=============== Training the model ===============");
            var trainedModel = trainingPipeline.Fit(trainingDataView);

            // Evaluate the model and show accuracy stats
            Console.WriteLine("===== Evaluating Model's accuracy with Test data =====");
            var predictions = trainedModel.Transform(testDataView);
            var metrics = mlContext.Regression.Evaluate(predictions, "fare_amount", "Score");
            ConsoleHelper.PrintRegressionMetrics(trainer.ToString(), metrics);

            // Save/persist the trained model to a .ZIP file
            Console.WriteLine($"=============== Saving the model  ===============");
            using (var fs = new FileStream(ModelPath, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Model.Save(trainedModel, fs);

            Console.WriteLine("The model is saved to {0}", ModelPath);

            return trainedModel;
        }

        // (OPTIONAL) Try/test a single prediction by loading the model from the file, first.
        private static void TestSinglePrediction(MLContext mlContext)
        {
            //Load data to test. Could be any test data. For demonstration purpose train data is used here.
            IDataView trainingDataView = mlContext.Data.ReadFromTextFile<SampleObservation>(
                                            path: TrainDataPath,
                                            hasHeader: true,
                                            separatorChar: ',',
                                            allowQuotedStrings: true,
                                            trimWhitespace: false,
                                            supportSparse: true);

            var sample = mlContext.CreateEnumerable<SampleObservation>(trainingDataView, false).First();

            ITransformer trainedModel;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                trainedModel = mlContext.Model.Load(stream);
            }

            // Create prediction engine related to the loaded trained model
            var predEngine = trainedModel.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);

            //Score
            var resultprediction = predEngine.Predict(sample);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sample.Fare_amount} | Predicted value: {resultprediction.Score}");
            Console.WriteLine($"==================================================");
        }

    }

    public class SampleObservation
    {
        [ColumnName("vendor_id"), LoadColumn(0)]
        public string Vendor_id { get; set; }


        [ColumnName("rate_code"), LoadColumn(1)]
        public float Rate_code { get; set; }


        [ColumnName("passenger_count"), LoadColumn(2)]
        public float Passenger_count { get; set; }


        [ColumnName("trip_time_in_secs"), LoadColumn(3)]
        public float Trip_time_in_secs { get; set; }


        [ColumnName("trip_distance"), LoadColumn(4)]
        public float Trip_distance { get; set; }


        [ColumnName("payment_type"), LoadColumn(5)]
        public string Payment_type { get; set; }


        [ColumnName("fare_amount"), LoadColumn(6)]
        public float Fare_amount { get; set; }


    }

    public class SamplePrediction
    {
        public float Score { get; set; }
    }

}
