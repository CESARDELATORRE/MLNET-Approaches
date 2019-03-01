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


namespace SampleRegression.Predict
{
    class Program
    {
        private static string DataPath = @"D:\GitRepos\MLNET-Approaches\Data\taxi-fare-train.csv";
        private static string ModelPath = @".\model.zip";

        static void Main(string[] args)
        {
            var mlContext = new MLContext();

            // 1. Load the model from .ZIP file
            ITransformer model = LoadModel(mlContext);

            // 2. Test single sample prediction
            TestSinglePrediction(mlContext, model);
        }

        private static ITransformer LoadModel(MLContext mlContext)
        {
            // Loading the model from the .ZIP model file
            ITransformer model;
            using (var stream = new FileStream(ModelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                model = mlContext.Model.Load(stream);
            }
            return model;
        }

        private static void TestSinglePrediction(MLContext mlContext, ITransformer model)
        {         
            //Load data to test. Could be any test data. For demonstration purpose train data is used here.
            IDataView dataView = mlContext.Data.ReadFromTextFile<SampleObservation>(
                                            path: DataPath,
                                            hasHeader: true,
                                            separatorChar: ',');

            // Create a single sample from the first row of the dataset
            // But here you could provide new test data, hardcoded or from the end-user application
            var sampleForPrediction = mlContext.CreateEnumerable<SampleObservation>(dataView, false).First();

            // Create prediction engine for predicting a single
            var predEngine = model.CreatePredictionEngine<SampleObservation, SamplePrediction>(mlContext);

            // Make a single prediction
            var resultprediction = predEngine.Predict(sampleForPrediction);

            Console.WriteLine($"=============== Single Prediction  ===============");
            Console.WriteLine($"Actual value: {sampleForPrediction.Fare_amount} | Predicted value: {resultprediction.Score}");
            Console.WriteLine($"==================================================");

            Console.WriteLine("=============== End of process, hit any key to finish ===============");
            Console.ReadKey();
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
