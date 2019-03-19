using Microsoft.Data.DataView;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScalableMLModelWebAPI.MLModel
{
    public interface IMLModelEngine<TData, TPrediction>
                    where TData : class
                    where TPrediction : class

    {
        TPrediction Predict(TData dataSample);

        IDataView PredictMany(IDataView testDataView);
    }
}
