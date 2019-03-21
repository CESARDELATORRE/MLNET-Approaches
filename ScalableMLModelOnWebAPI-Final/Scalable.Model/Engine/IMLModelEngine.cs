using Microsoft.Data.DataView;
using Microsoft.ML;

namespace Scalable.Model.Engine
{
    public interface IMLModelEngine<TData, TPrediction>
                    where TData : class
                    where TPrediction : class

    {
        ITransformer MLModel
        {
            get;
        }

        TPrediction Predict(TData dataSample);
    }
}
