using System.Runtime.Serialization;

namespace IdentityMicroservice
{
    [DataContract]
    public class ServiceContract
    {
        public ServiceContract(int statusCode, ResultModel resultModel, string message)
        {
            StatusCode = statusCode;
            ResultModel = resultModel;
            Message = message;
        }

        [DataMember]
        public int StatusCode { get; set; }

        [DataMember]
        public ResultModel ResultModel { get; set; }

        [DataMember]
        public string Message { get; set; }

    }
}
