using System;
using Newtonsoft.Json;

namespace test.Models
{
    public class DetailsJson
    {                        
        public DateTime date { get; set; }        
        public float factValue { get; set; }        
        public float planValue { get; set; }
        [JsonIgnore]
        public int count { get; set; }
        [JsonIgnore]
        public int notCommitedCount { get; set; }
        [JsonIgnore]
        public float factValueInUserCurrency { get; set; }
        [JsonIgnore]
        public float planValueInUserCurrency { get; set; }
        [JsonIgnore]
        public float accountCurrencyUserCurrencyRate { get; set; }        
    }
}
