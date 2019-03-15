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
using SampleRegression.Model;
using System.Collections.Generic;

namespace SampleRegression.Predict
{
    class Program
    {
        //Machine Learning model to load and use for predictions
        private const string MODEL_FILE_PATH = @"MLModel.zip";

        //Dataset used just for trying a single prediction with some data 
        private const string DATA_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-test.csv";

        //File to save multiple predictions 
        private const string PREDICTIONS_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-predictions.csv";

        static void Main(string[] args)
        {
            // Create ML model scorer object
            SampleRegressionModelScorer modelScorer = new SampleRegressionModelScorer(MODEL_FILE_PATH);

            // 1. Test single sample prediction
            PredictSingle(modelScorer);

            // 2. Perform multiple predictions and save them to a file
            BulkPredict(modelScorer, DATA_PATH, PREDICTIONS_PATH);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void PredictSingle(SampleRegressionModelScorer modelScorer)
        {
            // Create sample data to do a single prediction with it 
            SampleObservation sampleData = CreateSingleDataSample();

            // Try a single prediction
            var predictionResult = modelScorer.PredictionEngine.Predict(sampleData);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sampleData.Fare_amount} | Predicted value: {predictionResult.Score}");
            Console.WriteLine($"==================================================");
        }

        public static void BulkPredict(SampleRegressionModelScorer modelScorer, string dataFile, string predictionsFile)
        {
            MLContext mlContext = modelScorer.MLContext;

            //Load bulk sample data for many predictions
            IDataView bulkData = mlContext.Data.LoadFromTextFile<SampleObservation>(
                                            path: dataFile,
                                            hasHeader: true,
                                            separatorChar: ',');

            Console.WriteLine($"=============== Multiple Predictions  ===============");

            // Get predictions in bulk
            IDataView predictions = modelScorer.MLModel.Transform(bulkData);

            Console.WriteLine(string.Format("Peek a few rows from Predictions: Showing {0} rows", 4));
            PeekDataViewInConsole(predictions, 4);
            Console.WriteLine($"");

            // Save predictions in a file
            using (var fs = new FileStream(predictionsFile, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Data.SaveAsText(predictions, fs);

            Console.WriteLine($"Predictions file saved here: {predictionsFile}");
            Console.WriteLine($"==================================================");
        }

        // Method to load single row of data to try a single prediction
        // You can change this code and create your own sample data here (Hardcoded or from any source)
        private static SampleObservation CreateSingleDataSample()
        {
            var mlContext = new MLContext();

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

        public static void PeekDataViewInConsole(IDataView dataView, int numberOfRows = 5)
        {
            var preViewTransformedData = dataView.Preview(maxRows: numberOfRows);

            foreach (var row in preViewTransformedData.RowView)
            {
                var ColumnCollection = row.Values;
                string lineToPrint = "Row--> ";
                foreach (KeyValuePair<string, object> column in ColumnCollection)
                {
                    lineToPrint += $"| {column.Key}:{column.Value}";
                }
                Console.WriteLine(lineToPrint + "\n");
            }
        }

    }



    /*
    class Program
    {       
        //Machine Learning model to load and use for predictions
        private const string MODEL_RELATIVE_PATH = @"model.zip";

        //Dataset used just for testing predictions with some data 
        private const string DATA_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-test.csv";

        //File to save multiple predictions 
        private const string PREDICTIONS_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-predictions.csv";

        static void Main(string[] args)
        {
            // 1. Test single sample prediction
            TrySinglePrediction(MODEL_RELATIVE_PATH, DATA_PATH);

            // 3. Perform multiple predictions and save them to a file
            PerformMultiplePredictionsAndSaveToFile(MODEL_RELATIVE_PATH, DATA_PATH, PREDICTIONS_PATH);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static void TrySinglePrediction(string modelFilePath, string datasetFilePath)
        {
            MLContext mlContext = new MLContext(seed:1);

            //Load data to test. Could be any test data. For demonstration purpose train data is used here.
            IDataView dataView = mlContext.Data.LoadFromTextFile<SampleObservation>(
                                            path: datasetFilePath,
                                            hasHeader: true,
                                            separatorChar: ',');

            // Create a single sample from the first row of the dataset
            // IMPORTANT: Here you could provide new test data, hardcoded or from the end-user application
            var sampleForPrediction = mlContext.Data.CreateEnumerable<SampleObservation>(dataView, false).First();

            //============= [ThreadStatic] OBJECT AND APPROACH ======================================
            //var mlModelScorer = new MLModelScorer<SampleObservation, SamplePrediction>(modelFilePath);
            // Make a single prediction
            //var resultprediction = mlModelScorer.Predict(sampleForPrediction);
            //====================================================================
            
            //============= OBJECT POOLING APPROACH ======================================
            var mlModelScorer = new MLModelScorerObjPool<SampleObservation, SamplePrediction>(modelFilePath);
            // Make a single prediction
            var resultprediction = mlModelScorer.Predict(sampleForPrediction);
            //====================================================================

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sampleForPrediction.Fare_amount} | Predicted value: {resultprediction.Score}");
            Console.WriteLine($"==================================================");


            // (REMOVE/DELETE..)
            //============= STATIC CLASS APPROACH ================================
            // Load the model from serialized file
            // MLModelStatic<SampleObservation, SamplePrediction>.LoadMLModelFromFile(modelFilePath);
            //
            // Make a single prediction
            // var resultprediction = MLModelStatic<SampleObservation, SamplePrediction>.Predict(sampleForPrediction);
            //====================================================================
        }

        public static void PerformMultiplePredictionsAndSaveToFile(string modelFilePath, string testDataFile, string predictionsFile)
        {
            MLContext mlContext = new MLContext(seed: 1);

            IDataView testDataView = mlContext.Data.LoadFromTextFile<SampleObservation>(
                                            path: testDataFile,
                                            hasHeader: true,
                                            separatorChar: ',');

            Console.WriteLine($"=============== Multiple Predictions  ===============");

            var mlModelEngine = new MLModelScorer<SampleObservation, SamplePrediction>(modelFilePath);

            IDataView predictions = mlModelEngine.PredictMany(testDataView);

            Console.WriteLine(string.Format("Peek a few rows from Predictions: Showing {0} rows", 4));
            PeekDataViewInConsole(predictions, 4);
            Console.WriteLine($"");

            using (var fs = new FileStream(predictionsFile, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Data.SaveAsText(predictions, fs);

            Console.WriteLine($"Predictions file saved here: {predictionsFile}");
            Console.WriteLine($"==================================================");
        }

        

        public static void PeekDataViewInConsole(IDataView dataView, int numberOfRows = 5)
        {
            var preViewTransformedData = dataView.Preview(maxRows: numberOfRows);

            foreach (var row in preViewTransformedData.RowView)
            {
                var ColumnCollection = row.Values;
                string lineToPrint = "Row--> ";
                foreach (KeyValuePair<string, object> column in ColumnCollection)
                {
                    lineToPrint += $"| {column.Key}:{column.Value}";
                }
                Console.WriteLine(lineToPrint + "\n");
            }
        }

    }
    */

}
