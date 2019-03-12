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
using SampleRegression.Common.DataModels;
using System.Collections.Generic;

using SampleRegression.Common;

namespace SampleRegression.Predict
{
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

            //============= OBJECT APPROACH ======================================
            var mlModelEngine = new MLModelScorer<SampleObservation, SamplePrediction>(modelFilePath);

            // Make a single prediction
            var resultprediction = mlModelEngine.Predict(sampleForPrediction);
            //====================================================================

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sampleForPrediction.Fare_amount} | Predicted value: {resultprediction.Score}");
            Console.WriteLine($"==================================================");


            // (REMOVE PROBABLY)
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





}
