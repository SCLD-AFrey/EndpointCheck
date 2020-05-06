using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.POCO;
using Grpc.Core;
using EndpointSvc;
using Newtonsoft.Json;

namespace EndpointCheckClient.ViewModels
{
    [MetadataType(typeof(MetaData))]
    public class MainViewModel
    {
        public class MetaData : IMetadataProvider<MainViewModel>
        {
            void IMetadataProvider<MainViewModel>.BuildMetadata
                (MetadataBuilder<MainViewModel> p_builder)
            {
                p_builder.CommandFromMethod(p_x => p_x.OnProcessButtonCommand()).CommandName("ProcessButtonCommand");
            }
        }

        #region Constructors

        protected MainViewModel()
        {
            //Config
            Host = "127.0.0.1";
            Port = "50051";
            string jsonFile = "../../../../EndpointChecker/endpoint-list.json";

            //Load initial json file
            using (StreamReader r = new StreamReader(jsonFile))
            {
                endpointJson = r.ReadToEnd();
            }
            CreateChannel();

        }

        public static MainViewModel Create()
        {
            return ViewModelSource.Create(() => new MainViewModel());
        }

        #endregion

        #region Fields and Properties

        public virtual string endpointJson { get; set; }
        public virtual string outText { get; set; }
        public virtual EndpointChecker.EndpointCheckerClient client { get; set; }
        public virtual Channel channel { get; set; }
        public virtual List<EndpointCheck> items { get; set; }
        public virtual string Host { get; set; }
        public virtual string Port { get; set; }

        #endregion

        public void CreateChannel()
        {
            channel = new Channel(string.Concat(Host, ":", Port), ChannelCredentials.Insecure); //--- without SSL
            client = new EndpointChecker.EndpointCheckerClient(channel);
        }

        #region Methods
        public void OnProcessButtonCommand()
        {
            items = JsonConvert.DeserializeObject<List<EndpointCheck>>(endpointJson);
            outText = "Processing..." + Environment.NewLine;
            ProcessEndpointListByEntry();
            outText += "Complete" + Environment.NewLine;
        }

        private async void ProcessEndpointListByEntry()
        {
            foreach (var _endpoint in items)
            {
                var endpointTask = ProcessEndpoint(_endpoint);
                var endpoint = await endpointTask;
                outText += String.Format("Checking: {0} @ {1} - Passed={2}", endpoint.Name, endpoint.IPaddress, endpoint.Success.ToString()) + Environment.NewLine;
            }
        }

        private async Task<EndpointCheck> ProcessEndpoint(EndpointCheck endpointCheck)
        {
            var request = new EndpointCheckRequest
            {
                Json = JsonConvert.SerializeObject(endpointCheck)
            };
            var reply = client.CheckEndpointAsync(request).ResponseAsync.Result;

            endpointCheck = JsonConvert.DeserializeObject<EndpointCheck>(reply.Json);

            return endpointCheck;
        }

        #endregion
    }
}