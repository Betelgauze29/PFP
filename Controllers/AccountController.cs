using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using test.Models;

namespace test.Controllers
{
    [Route("api/accounts")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly ILogger<AccountController> _logger;

        public AccountController(ILogger<AccountController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public string Get(DateTime dateBegin, DateTime dateEnd)
        {              
            DateTime start = DateTime.Now;
            
            var accountsInfo = new List<Account>();    
            using (var httpClient = new HttpClient())
            {
                
                httpClient.DefaultRequestHeaders.Add("X-ApiKey", "rETjmCehuYmszbyijI1GgBtOe3rdAynEjFxGbi06dn2Z_ClMZmKTSzFVIx80_KMq4tdtsEVtD4BYohbiiIaheese_I9KbyAYAVdKHKo-QrYZC3HB5ireqyWpQUW09kWmcWTObSOIG9nTsqSZo4UIX8RgFBf60Wzdm3iB44DqEAzy-wm_qzKUXi8BpqkpQOhWbh6ePp3GTdAnuSLcNZ2VCH8Qt1AzJJ74VULBS-89rWxBZ2BxRSlz6GCd7jnk5XmcwDxm2XPvS0W6mbY28rUE5AdAA1rjpwa4ynnfp2pxTrGBAwtaKCQfpLPlcynrSSrlqlsm_NktCm38D6Rpo6eEQbchi06eS2zbGWjom1Ou9DcVnL7mbxlVtBK6uyFReSV_IsOBSNmXdb6iBCNsoShSu91IeoY2jDjqjxBUR2pqyN8Uzix8_GKS1TS6mW49QdteljvInu7EbWGp2p6Wo0SOwOPDqVjv5uoSzQWeALYyz_1Gylc7gXdA7304-AE17wkbWVs0iy5-Zjaw45VLlMRR3xDdbSwrJqgqqx2fdlIQHj17iqr1a3oN-tFJXKCIrH5ENgv2brlApzCRrcQGvgTu6OO9wzzX5kXB69_Jw7Af36pN4GWC");
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                List<AccountJson> accounts = new List<AccountJson>();  
                using (var response = httpClient.GetAsync("https://api.planfact.io/api/v1/bizinfos/accountshistory"))
                {                    
                    var apiResponse = JObject.Parse(response.Result.Content.ReadAsStringAsync().Result);
                    accounts = apiResponse.SelectToken("data").ToObject<List<AccountJson>>();                    
                }

                accounts.Remove(accounts.Find(acc => acc.accountId == 252302));
                
               
                foreach (var acc in accounts)
                {
                    var account = new Account();
                    account.Remainders = new List<float>();

                    for (int i = 0; i < (dateEnd - dateBegin).TotalDays + 1; i++)
                    {
                        account.Remainders.Add(0);
                    }

                    using (var accountInfo = httpClient.GetAsync($"https://api.planfact.io/api/v1/accounts/{acc.accountId}"))
                    {
                        var apiResponse = JObject.Parse(accountInfo.Result.Content.ReadAsStringAsync().Result);
                        account.Title = apiResponse.SelectToken("data.title").ToString();
                        var startingRemainderDate = DateTime.Parse(apiResponse.SelectToken("data.startingRemainderDate").ToString());
                        var startingRemainderValue = Convert.ToSingle(apiResponse.SelectToken("data.startingRemainderValue").ToString());
                        
                        if (startingRemainderDate > dateBegin)
                        {
                            int i = (int)(startingRemainderDate - dateBegin).TotalDays;
                            account.Remainders[i] = startingRemainderValue;                            
                        }
                        else
                        {                            
                            account.Remainders[0] = startingRemainderValue;
                        }
                    }

                    
                    int index = 0;
                                      
                    for (int j = 0; j < acc.details.Length; j++)
                    {
                        index = j;
                        if (acc.details[j].date > dateBegin) break;

                        float valueToAppend;
                        var detailsDayNum = (acc.details[j].date - dateBegin).TotalDays;
                        if (detailsDayNum < (DateTime.Today - dateBegin).TotalDays)
                        {
                            valueToAppend = acc.details[j].factValue;
                        }
                        else
                        {
                            valueToAppend = acc.details[j].planValue;
                        }
                        account.Remainders[0] += valueToAppend;                        
                    }                                        

                    for (int i = 1; i <= (dateEnd - dateBegin).TotalDays; i++)
                    {
                        var details = acc.details[index];

                        if (account.Remainders[i] == 0)
                        {
                            account.Remainders[i] = account.Remainders[i - 1];
                        }
                                             
                        var detailsDayNum = (details.date - dateBegin).TotalDays;
                        if (i - 1 == detailsDayNum)
                        {
                            float valueToAppend;
                            if (detailsDayNum < (DateTime.Today - dateBegin).TotalDays)
                            {
                                valueToAppend = details.factValue;
                            }
                            else
                            {
                                valueToAppend = details.planValue;
                            }                           
                            account.Remainders[i] = account.Remainders[i - 1] + valueToAppend;

                            if (index < acc.details.Count() - 1)
                            {
                                index++;
                            }
                        }
                    }
                    accountsInfo.Add(account);
                }

                
                var totalRemainder = new Account()
                {
                    Title = "Общий остаток",
                    Remainders = new List<float>()
                };
                for (int i = 0; i < (dateEnd - dateBegin).TotalDays + 1; i++)
                {
                    totalRemainder.Remainders.Add(0);
                }

                foreach (var acc in accountsInfo)
                {
                    for (int i = 0; i < acc.Remainders.Count; i++)
                    {
                        totalRemainder.Remainders[i] += acc.Remainders[i];
                    }
                }
                accountsInfo.Add(totalRemainder);
            }             
            Console.WriteLine((DateTime.Now - start).Milliseconds);

            return JsonConvert.SerializeObject(accountsInfo);
        }
    }
}
