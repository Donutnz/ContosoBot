using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.MobileServices;
using ContosoBot.DataModels;
using System.Text;
using System.Collections.Generic;
using System.Reflection;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace ContosoBot {
    [BotAuthentication]
    public class MessagesController: ApiController {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity) {
            if(activity.Type == ActivityTypes.Message) {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                StateClient stacli = activity.GetStateClient();
                BotData udat = await stacli.BotState.GetUserDataAsync(activity.ChannelId,activity.From.Id);

                var umess = activity.Text;
                string outp = "Test prob";

                if(!udat.GetProperty<bool>("repcust")) {
                    udat.SetProperty("repcust",true);
                    await stacli.BotState.SetUserDataAsync(activity.ChannelId,activity.From.Id,udat);
                    //outp = "Hello, I'm Contosobot!\n\nPlease choose a name by typing \"setname Firstname Lastname\" or login with \"login Username Password\"";
                    Activity greeting = activity.CreateReply("Welcome to Contoso Online Service");
                    greeting.Recipient = activity.From;
                    greeting.Type = "message";
                    greeting.Attachments = new List<Attachment>();

                    List<CardImage> ims = new List<CardImage>();
                    ims.Add(new CardImage(url: "https://msdnshared.blob.core.windows.net/media/2016/03/Contoso_logo.png"));

                    List<CardAction> buttons = new List<CardAction>();
                    CardAction helpbut = new CardAction() {
                        Value = "help",
                        Type = "imBack",
                        Title = "Help"
                    };
                    buttons.Add(helpbut);
                    CardAction sitebut = new CardAction() {
                        Value = "http://contoso.com",
                        Type = "openUrl",
                        Title = "Contoso.com"
                    };
                    buttons.Add(sitebut);

                    ThumbnailCard greetcard = new ThumbnailCard() {
                        Title = "Contoso Online Bank Service",
                        Subtitle = "My name is ContosoBot",
                        Text = "Note: for security reasons, this bot cannot make transactions. To manage your account please go to http://contoso.com",
                        Images = ims,
                        Buttons = buttons
                    };

                    Attachment plattach = greetcard.ToAttachment();
                    greeting.Attachments.Add(plattach);
                    await connector.Conversations.SendToConversationAsync(greeting);
                    return Request.CreateResponse(HttpStatusCode.OK);
                }
                else {
                    if(udat.GetProperty<string>("fname") == null && udat.GetProperty<string>("lname") == null) {
                        outp = "You haven\'t set a name or logged in yet, please set a name by typing in \"setname Firstname Lastname\" of login with \"login Username Password\"";
                    }
                    else {
                        outp = "Sorry, " + udat.GetProperty<string>("fname") + ", I missed that.";
                    }
                }

                if(umess.ToLower().Contains("login")) {
                    if(umess.ToLower().Substring(0,5) == "login") {
                        string user = "";
                        string pass = "";
                        string fname = "";
                        string lname="";
                        float balance;
                        bool u = true;
                        for(int x = 6; x < umess.Length; x++) {
                            if(umess[x].ToString()==" ") {
                                u = false;
                                continue;
                            }
                            if(u) {
                                user += umess[x];
                            }
                            else if(!u){
                                pass += umess[x];
                            }
                        }
                        List<custdat> cdat = await azuremanager.AzureManagerInstance.GetCustDat();
                        //outp = "";
                        string retdat=null;
                        try {
                            retdat = cdat.First(item => item.username == user).password;
                        }
                        catch {
                            outp = $"Sorry, I couldn't find the username |{user}|.";
                        }
                        if(retdat != null) {
                            if(pass == retdat) {
                                try {
                                    fname = cdat.First(item => item.username == user).firstname;
                                }
                                catch {
                                    fname = "";
                                }

                                try {
                                    lname = cdat.First(item => item.username == user).lastname;
                                }
                                catch {
                                    lname = "";
                                }
                                try {
                                    balance = cdat.First(item => item.username == user).balance;
                                }
                                catch {
                                    balance = 0;
                                }
                                udat.SetProperty<string>("fname",fname);
                                udat.SetProperty<string>("lname",lname);
                                udat.SetProperty<float>("balance",balance);
                                udat.SetProperty<bool>("loggedin",true);
                                await stacli.BotState.SetUserDataAsync(activity.ChannelId,activity.From.Id,udat);
                                outp = $"Hello {fname} {lname}, welcome back.";
                            }
                        }
                    }
                }

                if(umess.ToLower().Equals("newaccount")) {
                    //Coming soon
                }

                if(umess.ToLower().Contains("balance")) {
                    if(udat.GetProperty<bool>("loggedin")) {
                        float bal = udat.GetProperty<float>("balance");
                        outp=$"Your balance is: ${bal}";
                    }
                    else {
                        outp = "Sorry, you're not logged in yet, please login with the command \"login Username Password\"";
                    }
                }
                if(umess.ToLower().Contains("setname")) {
                    if((umess.Length - 7)<=0){
                        outp = "Sorry, you must put a name.";
                    }
                    else if(umess.ToLower().Substring(0,7) == "setname") {
                        string fname = "";
                        string lname = "";
                        bool f = true;
                        for(int x = 7; x < umess.Length; x++) {
                            if(umess[x].Equals(" ")) {
                                f = false;
                                continue;
                            }
                            if(f) {
                                fname += umess[x];
                            }
                            else {
                                lname += umess[x];
                            }
                            
                        }
                        udat.SetProperty<string>("fname",fname);
                        udat.SetProperty<string>("lname",lname);
                        await stacli.BotState.SetUserDataAsync(activity.ChannelId,activity.From.Id,udat);
                        outp = "Set your name to " + udat.GetProperty<string>("fname")+" "+udat.GetProperty<string>("lname");
                    }
                }

                if(umess.ToLower().Contains("myname")) {
                    outp = "Your name is: " + udat.GetProperty<string>("fname") + " " + udat.GetProperty<string>("lname");
                }

                bool willclear = udat.GetProperty<bool>("willclear");

                if(umess.ToLower() == "clear") {
                    if(!willclear) {
                        outp = "Are you sure you want to clear your data?\nType \"clear\" again to confirm";
                        udat.SetProperty("willclear",true);
                        await stacli.BotState.SetUserDataAsync(activity.ChannelId,activity.From.Id,udat);
                    }
                    else if(willclear){
                        await stacli.BotState.DeleteStateForUserAsync(activity.ChannelId,activity.From.Id);
                        outp = "Cleared data...";
                    }
                }
                else {
                    if(willclear) {
                        udat.SetProperty<bool>("willclear",false);
                        await stacli.BotState.SetUserDataAsync(activity.ChannelId,activity.From.Id,udat);
                    }
                }

                if(umess.ToLower().Contains("exchange")) {
                    if(umess.ToLower().Substring(0,8) == "exchange" && umess.ToLower().Substring(8)!=null) {
                        string curr1 = "GBP";
                        string curr2 = "USD";
                        //exchangeobject.Rates exch=new exchangeobject.Rates();
                        List<string> denoms=new List<string>();
                        //var ratesset = exch.GetType().GetProperties();
                        foreach(PropertyInfo item in typeof(exchangeobject.Rates).GetProperties()) {
                            Debug.WriteLine($"Item: {item.Name}");
                            denoms.Add(item.Name.ToString());
                        }
                        Debug.WriteLine($"Denoms list: {denoms.Count}");
                        bool fden = true;
                        int pos = 0;
                        int prevpos = 0;
                        for(int x = 0; x < denoms.Count; x++) {
                            if(umess.ToUpper().Contains(denoms[x])) {
                                pos = umess.ToUpper().IndexOf(denoms[x]);
                                if(fden) {
                                    curr1 = denoms[x];
                                    prevpos = pos;
                                    fden = false;
                                }
                                else {
                                    curr2 = denoms[x];
                                }
                                Debug.WriteLine($"Found: {denoms[x]} at {pos}");
                                continue;
                            }
                        }
                        if(prevpos > pos) { //puts the order of currencys back to how the user set them
                            //Debug.WriteLine($"Before temp thing: curr 1: {curr1} and curr2: {curr2}");
                            string temp = curr1;
                            curr1 = curr2;
                            curr2 = temp;
                            //Debug.WriteLine($"After temp thing: curr 1: {curr1} and curr2: {curr2}");
                        }
                        //Regex onlydigits = new Regex(@"[^0-9.]");
                        //string regstr = Regex.Replace(umess,@"[^\.\d]","");
                        float amount;
                        try {
                            amount = float.Parse(Regex.Replace(umess,@"[^\.\d]",""));
                        }
                        catch {
                            amount = 1;
                        }
                        Debug.WriteLine($"Amount: ${amount}");
                        Debug.WriteLine($"Curr is |{curr1}| and Curr2 is: |{curr2}|");
                        exchangeobject.RootObject root;
                        HttpClient fixer = new HttpClient();
                        string rate = await fixer.GetStringAsync(new Uri($"http://api.fixer.io/latest?base="+curr2));
                        root = JsonConvert.DeserializeObject<exchangeobject.RootObject>(rate);
                        //exch = JsonConvert.DeserializeObject<exchangeobject.Rates>(rate);
                        //var compin = root.rates;
                        //PropertyInfo denom = typeof(exchangeobject.RootObject).GetProperty("rates.AUD");
                        //object val = denom.GetValue(root,null);
                        //var proptype = compin.GetType();
                        //Debug.WriteLine("Got to get type stuff");
                        //outp = $"Rate is: {propinf.GetValue(exch)}";
                        var propinf = root.rates.GetType().GetProperty(curr1);
                        string conprop = propinf.GetValue(root.rates).ToString();
                        outp = $"{curr1} {float.Parse(conprop) * amount} is worth {curr2} {amount}";
                        //Debug.WriteLine("Got past get type stuff");
                    }
                }

                /*
                if(umess.ToLower().Equals("hello")) {
                    outp = "Hi!";
                    Debug.WriteLine("Hi!");
                }
                */

                /*
                if(umess.ToLower().Equals("getdata")) { //test azure API call
                    List<custdat> cdat = await azuremanager.AzureManagerInstance.GetCustDat();
                    outp = "";
                    try {
                        outp = cdat.First(item => item.firstname == "Rumkin").username;
                    }
                    catch {
                        outp = "Not found";
                    }
                    //foreach(custdat c in cdat) {
                        //outp += c.firstname;
                    //}
                }
                */

                if(umess.ToLower().Contains("help")) {
                    outp = "Help:\n\n\nlogin Username Password - Log in\n\nexchange Amount Currency(e.g. NZD) Currency - Get currency exchange info\n\nhelp - This help\n\nbalance - Show your balance if logged in\n\ngreeting - Show title card\n\nsetname Firstname Lastname - Set name\n\nclear - Logout\n\nMore info can be found on our homepage http://contoso.com";
                }

                if(umess.ToLower().Equals("greeting")) {
                    udat.SetProperty("repcust",false);
                    await stacli.BotState.SetUserDataAsync(activity.ChannelId,activity.From.Id,udat);
                    outp = "You will see the title card on your next message";
                }
                /*// calculate something for us to return
                int length = (activity.Text ?? string.Empty).Length;

                // return our reply to the user
                Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");*/

                //Activity reply = activity.CreateReply($"You sent: {umess}");
                Activity reply = activity.CreateReply(outp);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message) {
            if(message.Type == ActivityTypes.DeleteUserData) {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if(message.Type == ActivityTypes.ConversationUpdate) {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if(message.Type == ActivityTypes.ContactRelationUpdate) {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if(message.Type == ActivityTypes.Typing) {
                // Handle knowing tha the user is typing
            }
            else if(message.Type == ActivityTypes.Ping) {
            }

            return null;
        }

        //Written by Donutnz
    }
}