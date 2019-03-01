//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using System;
using System.IO;
using System.Linq;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using Microsoft.Data.DataView;
using SampleRegression.Common;
using System.Collections.Generic;

namespace SampleRegression.Predict
{
    class Program
    {       
        //Machine Learning model to load and use for predictions
        private const string MODEL01_RELATIVE_PATH = @"Models/model01.zip";
        private const string MODEL02_RELATIVE_PATH = @"Models/model02.zip";
        private const string MODEL03_RELATIVE_PATH = @"Models/model03.zip";

        //Dataset used just for testing predictions with some data 
        private const string DATA_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-test.csv";

        //File to save multiple predictions 
        private const string PREDICTIONS01_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-predictions-01.csv";
        private const string PREDICTIONS02_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-predictions-01.csv";
        private const string PREDICTIONS03_PATH = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-predictions-01.csv";

        static void Main(string[] args)
        {
            var mlContext = new MLContext();

            // 1. Load the models from .ZIP files
            ITransformer model01 = LoadModel(mlContext, MODEL01_RELATIVE_PATH);
            ITransformer model02 = LoadModel(mlContext, MODEL02_RELATIVE_PATH);
            ITransformer model03 = LoadModel(mlContext, MODEL03_RELATIVE_PATH);

            // 2. Test single sample prediction
            TrySinglePrediction(mlContext, model01, DATA_PATH);
            TrySinglePrediction(mlContext, model02, DATA_PATH);
            TrySinglePrediction(mlContext, model03, DATA_PATH);

            // 3. Perform multiple predictions and save them to a file
            PerformMultiplePredictionsAndSaveToFile(mlContext, model01, DATA_PATH, PREDICTIONS01_PATH);
            PerformMultiplePredictionsAndSaveToFile(mlContext, model02, DATA_PATH, PREDICTIONS02_PATH);
            PerformMultiplePredictionsAndSaveToFile(mlContext, model03, DATA_PATH, PREDICTIONS03_PATH);

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
        }

        private static ITransformer LoadModel(MLContext mlContext, string modelRelativePath)
        {
            // Loading the model from the .ZIP model file
            Console.WriteLine($"Loading model from .ZIP file..");
            Console.WriteLine($" ");
            ITransformer model;
            using (var stream = new FileStream(modelRelativePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                model = mlContext.Model.Load(stream);
            }
            return model;
        }

        private static void TrySinglePrediction(MLContext mlContext, ITransformer model, string dataPath)
        {         
            //Load data to test. Could be any test data. For demonstration purpose train data is used here.
            IDataView dataView = mlContext.Data.ReadFromTextFile<SampleObservation>(
                                            path: dataPath,
                                            hasHeader: true,
                                            separatorChar: ',');

            // Create a single sample from the first row of the dataset
            // IMPORTANT: Here you could provide new test data, hardcoded or from the end-user application
            var sampleForPrediction = mlContext.CreateEnumerable<SampleObservation>(dataView, false).First();

            // Create prediction engine needed to perform a single prediction
            var predEngine = model.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);

            // Make a single prediction
            var resultprediction = predEngine.Predict(sampleForPrediction);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sampleForPrediction.Fare_amount} | Predicted value: {resultprediction.Score}");
            Console.WriteLine($"==================================================");

        }

        public static void PerformMultiplePredictionsAndSaveToFile(MLContext mlContext, ITransformer model, string testDataFile, string predictionsFile)
        {
            IDataView testDataView = mlContext.Data.ReadFromTextFile<SampleObservation>(
                                            path: testDataFile,
                                            hasHeader: true,
                                            separatorChar: ',');

            Console.WriteLine($"");
            Console.WriteLine($"=============== Multiple Predictions  ===============");
            var predictions = PerformMultiplePredictions(model, testDataView);

            Console.WriteLine(string.Format("Peek a few rows from Predictions: Showing {0} rows", 4));
            PeekDataViewInConsole(predictions, 4);
            Console.WriteLine($"");

            using (var fs = new FileStream(predictionsFile, FileMode.Create, FileAccess.Write, FileShare.Write))
                mlContext.Data.SaveAsText(predictions, fs);

            Console.WriteLine($"Predictions file saved here: {predictionsFile}");
            Console.WriteLine($"==================================================");
        }

        public static IDataView PerformMultiplePredictions(ITransformer model, IDataView testDataView)
        {
            IDataView predictions = model.Transform(testDataView);
            return predictions;
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
