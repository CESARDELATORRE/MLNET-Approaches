﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using ScalableMLModelWebAPI.DataModels;
using ScalableMLModelWebAPI.MLModel;

namespace ScalableMLModelWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PredictorController : ControllerBase
    {
        private readonly IMLModelEngine<SampleObservation, SamplePrediction> _modelEngine;

        public PredictorController(IMLModelEngine<SampleObservation, SamplePrediction> modelEngine)
        {
            // Get the ML Model Engine injected, for scoring
            _modelEngine = modelEngine;
        }

        // GET api/predictor/sentimentprediction?sentimentText=ML.NET is awesome!
        [HttpGet]
        [Route("sentimentprediction")]
        public ActionResult<string> PredictSentiment([FromQuery]string sentimentText)
        {
            SampleObservation sampleData = new SampleObservation() { SentimentText = sentimentText };

            //Predict sentiment
            SamplePrediction prediction = _modelEngine.Predict(sampleData);

            bool isToxic = prediction.IsToxic;
            float probability = CalculatePercentage(prediction.Score);
            string retVal = $"Prediction: Is Toxic?: '{isToxic.ToString()}' with {probability.ToString()}% probability of toxicity  for the text '{sentimentText}'";

            return retVal;

        }

        public static float CalculatePercentage(double value)
        {
            return 100 * (1.0f / (1.0f + (float)Math.Exp(-value)));
        }

    }


}