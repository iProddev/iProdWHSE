using iProdDataModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UT = iProdWHSE.utility;

namespace iProdWHSE
{
    public static class MockHelper
    {

        static Form1 mainForm { get; set; }
        
        public static bool Slow { get; set; }

        public static void Init()
        {
            if (mainForm is null) mainForm = UT.mainForm; 
        }

        
        static string log(string st)
        {
            return UT.Log(st);
        }

        public static bool Ping()
        {
            Init();

            log("Pinging Web Service..");
            UT.Sleep(1000);   // 1 secondo

            return true;
        }



        public static myNameSpace.sendJobsV01Response GetPickResponse(myNameSpace.sendJobsV01Request req)
        {
            var resp = new myNameSpace.sendJobsV01Response();
            resp.@return.returnValue = 1;

            // in realta della richiesta non ce ne facciamo nulla perche nel servizio va inviato e restituisce solo return = 1
            var par = req.param.ToList();

            foreach(var p in par)
            {
                UT.Log($"PICK - MOCK                .... invio jobNumber {p.jobNumber}, JobPosition {p.JobPosition}");
                if (Slow) UT.Sleep(700);
            }


            return resp;

        }

        public static myNameSpace.readAllAMDV01Response GetStockResponse(List<Items> items, int num)
        {



            var resp = new myNameSpace.readAllAMDV01Response();
            resp.@return = new myNameSpace.RetReadAllAMDV01();

            var ret = resp.@return;



            var arts = new List<myNameSpace.AMDTypeV01>();


            
            for (int i = 0; i <= num; i++)
            {
                if (Slow) UT.Sleep(700);
                var rand = new Random();

                int j = rand.Next(items.Count-1);

                var a = new myNameSpace.AMDTypeV01();

                a.articleName = items[j].name;
                a.articleNumber = items[j].code;
                a.compartmentDepthNumber = 3;
                a.compartmentNumber = 1;
                a.containerSize = 1000;
                a.fifo = 0;
                a.inventoryAtStorageLocation = "5";
                a.liftNumber = 4;
                a.shelfNumber = 5;
                a.minimumInventory = "500";

                arts.Add(a);
                UT.Sleep(200);
            }

            ret.returnValue = 1;
            ret.article = arts.ToArray();
          


            return resp;
        }

    }
}
