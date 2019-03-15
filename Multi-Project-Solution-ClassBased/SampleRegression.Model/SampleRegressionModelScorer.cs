//*****************************************************************************************
//*                                                                                       *
//* This is an auto-generated file by Microsoft ML.NET CLI (Command-Line Interface) tool. *
//*                                                                                       *
//*****************************************************************************************

using Microsoft.Data.DataView;
using Microsoft.ML;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SampleRegression.Model.DataModels;

namespace SampleRegression.Model
{
    public class SampleRegressionModelScorer
    {
        private MLContext _mlContext;
        private ITransformer _mlModel;

        // The Prediction engine is not thread safe so it needs to be [ThreadStatic]: Unique for each thread (https://docs.microsoft.com/en-us/dotnet/api/system.threadstaticattribute)
        [ThreadStatic]
        private static PredictionEngine<SampleObservation, SamplePrediction> _predictionEngine;

        public PredictionEngine<SampleObservation, SamplePrediction> PredictionEngine
        {
            get
            {
                // _predictionEngine can be null the first time or if coming from a different thread, since it is [ThreadStatic]
                if (_predictionEngine == null)
                    _predictionEngine = _mlModel.CreatePredictionEngine<SampleObservation, SamplePrediction>(_mlContext);

                return _predictionEngine;            
            }
        }

        public ITransformer MLModel
        {
            get => _mlModel;
        }

        public MLContext MLContext
        {
            get => _mlContext;
        }

        //Constructor loading model from serialized .ZIP file
        public SampleRegressionModelScorer(string modelFilePathName, MLContext mlContext = null)
        {
            //Create or use provided MLContext
            if (mlContext != null)
                _mlContext = mlContext;
            else 
                _mlContext = new MLContext();

            //Load the model from the .ZIP file
            using (var fileStream = File.OpenRead(modelFilePathName))
            {
                _mlModel = _mlContext.Model.Load(fileStream);
            }
        }
    }


}


/* DELETE 

// FROM BLAZOR SAMPLE...

public class Engine
{
    static MLContext context = new MLContext();
    static ITransformer model
        = context.Model.Load(File.Open("model.zip", FileMode.Open));

    [ThreadStatic]
    static PredictionEngine<SourceData, Prediction> t_engine;

    public static PredictionEngine<SourceData, Prediction> GetPredictionEngine(string modelfile)  //(CDLTLL) Why modelfile path here?
    {
        if (t_engine != null)
            return t_engine;

        return t_engine = model.CreatePredictionEngine<SourceData, Prediction>(context);
    }

    public static float CalculatePercentage(double value)
    {
        return 100 * (1.0f / (1.0f + (float)Math.Exp(-value)));
    }
}
*/
