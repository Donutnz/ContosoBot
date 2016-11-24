using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using ContosoBot.DataModels;
using System.Threading.Tasks;
using System.Text;

namespace ContosoBot {
    public class azuremanager {
        private static azuremanager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<custdat> customerdata;

        private azuremanager() {
            this.client = new MobileServiceClient("https://databasestest.azurewebsites.net");
            this.customerdata = this.client.GetTable<custdat>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static azuremanager AzureManagerInstance {
            get {
                if(instance == null) {
                    instance = new azuremanager();
                }
                return instance;
            }
        }

        public async Task<List<custdat>> GetCustDat() {
            return await this.customerdata.ToListAsync();
        }
    }
}