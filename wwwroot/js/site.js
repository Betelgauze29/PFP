am4core.ready(() => {
    $("#date-begin").change(datePickersChange);
    $("#date-end").change(datePickersChange);

    $("#date-begin").val("2021-03-01");
    $("#date-end").val("2022-03-31");

    $("#date-begin").trigger("change"); 
});

function datePickersChange() {
    var dateBegin = new Date($("#date-begin").val());
    var dateEnd = new Date($("#date-end").val());
    if (dateBegin > dateEnd) {
        $("#date-begin").val("2021-03-01");
        $("#date-end").val("2022-03-31");
    }

    var accountsInfo = [];  
    console.time("Request");
    $.ajax({
        type: "get",
        url: "/api/accounts",  
        data: {
            dateBegin: $("#date-begin").val(),
            dateEnd: $("#date-end").val()
        },
        success: function (response) {   
            console.timeEnd("Request");
            accountsInfo = JSON.parse(response);                           
                        
            am4core.useTheme(am4themes_animated);            
            var chart = am4core.create("account-balance-chart", am4charts.XYChart);
            
            var dateBegin =  Date.parse($("#date-begin").val());
            var dateEnd =  Date.parse($("#date-end").val());
            var numOfDays = (dateEnd - dateBegin)/86400000;            
            var data = [];  

            for (var i = 0; i <= numOfDays; i++) {
                var date = new Date(dateBegin);   
                date.setHours(0,0,0,0); 
                date.setMonth(0);
                date.setDate(moment(dateBegin).dayOfYear() + i);                                   

                data.push({
                    date: date, 
                    value0: accountsInfo[0].Remainders[i], 
                    value1: accountsInfo[1].Remainders[i], 
                    value2: accountsInfo[2].Remainders[i], 
                    value3: accountsInfo[3].Remainders[i]
                });
            }            
            chart.data = data;
                  
            var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
            dateAxis.renderer.minGridDistance = 72;
            var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
            
            accountsInfo.forEach((acc, index) => {
                var series = chart.series.push(new am4charts.LineSeries());
                series.name = acc.Title;
                series.dataFields.valueY = `value${index}`;
                series.dataFields.dateX = "date";
                series.tooltipText = series.name + `: {value${index}}`
                series.tooltip.pointerOrientation = "left";
                series.strokeWidth = 2;
                series.tensionX = 0.7;
                series.hidden = true;                                
                if (series.name === "Общий остаток") {
                    series.fillOpacity = 0.2;
                    series.hidden = false;
                }
            });            

            chart.legend = new am4charts.Legend();
            chart.cursor = new am4charts.XYCursor();            
            chart.scrollbarX = new am4core.Scrollbar();
        }
    });      
}