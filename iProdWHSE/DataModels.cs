using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


//update al 01/03/21 12.26 (Italian Time Zone)
namespace LirecoAgent
{
    #region JOURNEY-PHASES

    [BsonIgnoreExtraElements]
    public class Journey
    {
        [BsonElement("lastupdate")]
        public DateTime Lastupdate { get; set; }

        [BsonElement("instances")]
        public List<PhaseInstance> Instances { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class PhaseInstance
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("priority")]
        public int priority { get; set; } = 0;

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Iprodcustomerid { get; set; }

        [BsonElement("machineid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Machineid { get; set; }

        [BsonElement("phaseid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Phaseid { get; set; }

        [BsonElement("creationdate")]
        public DateTime Creationdate { get; set; }

        [BsonElement("authorid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Authorid { get; set; } = "000000000000000000000000";

        [BsonElement("linkeddata")]
        public Linkeddata Linkeddata { get; set; }

        [BsonElement("performancedata")]
        public Performancedata Performancedata { get; set; }

        [BsonElement("summarydata")]
        public Summarydata Summarydata { get; set; }

        [BsonElement("currentstatus")]
        public int Currentstatus { get; set; }

        [BsonElement("lastupdate")]
        public DateTime Lastupdate { get; set; }

        [BsonElement("status")]
        public string status { get; set; }

        [BsonElement("type")]
        public string type { get; set; }

        [BsonElement("additionalproperties")]
        public Dictionary<string, AdditionalProperties> AdditionalProperties { get; set; }

        [BsonElement("schedulingdata")]
        public istanceschedulingdata schedulingdata { get; set; } = new istanceschedulingdata();
    }

    [BsonIgnoreExtraElements]
    public class istanceschedulingdata
    {
        [BsonElement("rescheduledstart")]
        public DateTime? Rescheduledstart { get; set; }

        [BsonElement("scheduledstart")]
        public DateTime? Scheduledstart { get; set; }

        [BsonElement("scheduledend")]
        public DateTime? Scheduledend { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class Linkeddata
    {
        [BsonElement("itemid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Itemid { get; set; }

        [BsonElement("workorderid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Workorderid { get; set; }

        [BsonElement("customerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Customerid { get; set; }

        [BsonElement("salesorderid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Salesorderid { get; set; }

        [BsonElement("phaseinstances")]
        public List<Phaseid> phaseinstances { get; set; }
    }

    /// <summary>
    /// Stores various kind of information about a phase instance.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Performancedata
    {
        /// <summary>
        /// Total number of parts that should be done.
        /// </summary>
        [BsonElement("totalqty")]
        public int Totalqty { get; set; }

        /// <summary>
        /// Number of parts effectively made.
        /// </summary>
        [BsonElement("currentqty")]
        public int Currentqty { get; set; }

        /// <summary>
        /// Number of discarded parts.
        /// </summary>
        [BsonElement("discarded")]
        public int Discarded { get; set; }

        /// <summary>
        /// Start time of the phase instance in UTC.
        /// </summary>
        [BsonElement("startingtime")]
        public DateTime startingtime { get; set; } = Convert.ToDateTime("1990-01-01");

        /// <summary>
        /// End time of the phase instance in UTC.
        /// </summary>
        [BsonElement("endingtime")]
        public DateTime endingtime { get; set; } = Convert.ToDateTime("1990-01-01");

        /// <summary>
        /// Total duration of the phase instance since it started, expressed in minutes.
        /// </summary>
        [BsonElement("totaltime")]
        public double Totaltime { get; set; }

        /// <summary>
        /// Average time spent to make one part.
        /// </summary>
        [BsonElement("avgtime")]
        public double Avgtime { get; set; }

        /// <summary>
        /// The current status of the machine (e.g. "production", "pause"...).
        /// </summary>
        [BsonElement("currentstatus ")]
        public string Currentstatus { get; set; }

        /// <summary>
        /// List containing information about the time spent on each phase (e.g. "production", "pause"...).
        /// </summary>
        [BsonElement("timesheets")]
        public List<Timesheets> Timesheets { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class Timesheets
    {
        /// <summary>
        /// The status of the machine (e.g. "production", "pause"...).
        /// </summary>
        [BsonElement("timetype")]
        public string Timetype { get; set; }

        /// <summary>
        /// Time expressed in seconds.
        /// </summary>
        [BsonElement("time")]
        public double Time { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class Summarydata
    {
        [BsonElement("itemname")]
        public string Itemname { get; set; }

        [BsonElement("wordordername")]
        public string Wordordername { get; set; }

        [BsonElement("salesordername")]
        public string Salesordername { get; set; }

        [BsonElement("phasename")]
        public string Phasename { get; set; }

        [BsonElement("customername")]
        public string Customername { get; set; }
    }

    #endregion


    #region CUSTOMERDATA


    [BsonIgnoreExtraElements]
    public class iProdTenantSummary
    {
        [BsonId]
        public ObjectId ID { get; set; }

        [BsonElement("lastupdate")]
        public DateTime dateTime { get; set; }

        [BsonElement("iprodcustomerid")]
        public ObjectId iprodcustomerid { get; set; }

        [BsonElement("space")]
        public long space { get; set; }

        [BsonElement("workordernumber")]
        public long workordernumber { get; set; }

        [BsonElement("remainspace")]
        public long remainspace { get; set; }

        [BsonElement("remainworkordernumber")]
        public long remainworkordernumber { get; set; }


        [BsonElement("expiringdate")]
        public DateTime expiringdate { get; set; }

        [BsonElement("status")]
        public int status { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class iProdTenantSituation
    {
        public double spaceleft { get; set; }

        public double spaceleftpercent { get; set; }

        public double workordeleft { get; set; }
        public double workordeleftpercent { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class customerstatistics
    {
        public double totalplayhours { get; set; } = 0;
        public double totalpausehours { get; set; } = 0;
        public double totalproductionhours { get; set; } = 0;

        public double totalhours { get; set; } = 0;
        public double totalpartsdone { get; set; } = 0;
        public double totalpartsscheduled { get; set; } = 0;
    }

    [BsonIgnoreExtraElements]
    public class customerFilterstatistics
    {
        public double programming { get; set; } = 0;
        public double setting { get; set; } = 0;
        public double production { get; set; } = 0;
        public double preproduction { get; set; } = 0;
        public double cleaning { get; set; } = 0;
        public double pause { get; set; } = 0;
    }

    [BsonIgnoreExtraElements]
    public class CustomerGraphData
    {
        public customerFilterstatistics customerFilterstatistics1 { get; set; }
        public List<customerFilterstatistics> customerFilterstatistics2 { get; set; }
        public List<string> Labels { get; set; }

        public string Types { get; set; }
    }



    [BsonIgnoreExtraElements]
    public class iProdCustomers
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("status")]
        public int Status { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("customerdata")]
        public Customerdata Customerdata { get; set; }

        [BsonElement("customermachine")]
        public List<Customermachine> Customermachine { get; set; }

        [BsonElement("customerusers")]
        public List<Customerusers> Customerusers { get; set; }

        [BsonElement("warehouses")]
        public List<warehouse> warehouses { get; set; }


        [BsonElement("customercustomization")]
        public Customization customization { get; set; }

        [BsonElement("creation_date")]
        public DateTime creation_date { get; set; }

        [BsonElement("expiring_date")]
        public DateTime expiring_date { get; set; }

        [BsonElement("last_update")]
        public DateTime last_update { get; set; }

        [BsonElement("additionaldata")]
        public Dictionary<string, string> additionaldata { get; set; }

    }





    [BsonIgnoreExtraElements]
    public class warehouse
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.String)]
        [BsonElement("type")]
        public string type { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("linkedid")]
        public string linkedid { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("code")]
        public string code { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("name")]
        public string name { get; set; }

        [BsonElement("additionalproperties")]
        public Dictionary<string, string> additionalproperties { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("status")]
        public string status { get; set; }
    }



    [BsonIgnoreExtraElements]
    public class Customization
    {
        [BsonElement("datacategories")]
        public Dictionary<string, string> datacategories { get; set; }

        // e queste le categoprie documentali
        [BsonElement("datatypes")]
        public List<DataType> datatypes { get; set; }
    }



    [BsonIgnoreExtraElements]
    public class DataType
    {
        [BsonElement("id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string ID { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.String)]
        public string type { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string name { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class documenttemplates
    {
        [BsonElement("id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; }


        [BsonElement("postid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string postid { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Customerdata
    {
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("VAT")]
        public string VAT { get; set; }

        [BsonElement("Logo")]
        public string logo { get; set; }

        [BsonElement("LegalName")]
        public string legalname { get; set; }

        [BsonElement("Address")]
        public string address { get; set; }

        [BsonElement("CountryCode")]
        public string country { get; set; }

    }
    #endregion

    #region MACHINES

    [BsonIgnoreExtraElements]
    public class Customermachine
    {
        [BsonElement("id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public String ID { get; set; }

        [BsonElement("guid")]
        public string Guid { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("token")]
        public string Token { get; set; }

        [BsonElement("expiringdate")]
        public DateTime Expiringdate { get; set; }

        [BsonElement("licensed")]
        public bool Licensed { get; set; }

        [BsonElement("creationdate")]
        public DateTime CreationDate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime LastUpdate { get; set; }

        [BsonElement("machinedate")]
        public DateTime machinedate { get; set; }

        [BsonElement("machinesn")]
        public string machinesn { get; set; }

        [BsonElement("machinebrand")]
        public string brand { get; set; }

        [BsonElement("machinemodel")]
        public string model { get; set; }

        [BsonElement("machinenote")]
        public string notes { get; set; }

        [BsonElement("machinehours")]
        public decimal hours { get; set; }

        [BsonElement("worktimestart")]
        public double worktimestart { get; set; }

        [BsonElement("worktimeend")]
        public double worktimeend { get; set; }

        [BsonElement("machinecost")]
        public decimal machinecost { get; set; }

        [BsonElement("status")]
        public string status { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class MachineStatus
    {
        public Customermachine Machine { get; set; }
        public PhaseInstance Currentphase { get; set; }
        public Trackingdata Trackingdata { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class UserStatus
    {
        public Customerusers user { get; set; }
        public PhaseInstance Currentphase { get; set; }
        public Trackingdata Trackingdata { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Customerusers
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("surname")]
        public string Surname { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("username")]
        public string Username { get; set; }

        [BsonElement("fiscalcode")]
        public string fiscalcode { get; set; }

        [BsonElement("mobile")]
        public string mobile { get; set; }

        [BsonElement("pwd")]
        public string Pwd { get; set; }

        [BsonElement("token")]
        public string Token { get; set; }

        [BsonElement("lasttoken")]
        public DateTime Lasttoken { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("socialtoken")]
        public List<string> Socialtoken { get; set; }

        [BsonElement("imgurl")]
        public string Imgurl { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("lastupdate")]
        public DateTime Lastupdate { get; set; }

        public string ParentMenuCode { get; set; }

        [BsonElement("authorization")]
        public Dictionary<string, string> Authorization { get; set; }

        [BsonElement("additionalreference")]
        public Dictionary<string, string> Additionalreference { get; set; }

        /// <summary>
        /// La chiave "TimeZone" contiene la proprietà <c>TimeZoneInfo.Id</c>.<br />
        /// La chiave "CultureName" contiene la proprietà <c>CultureInfo.Name</c>.
        /// </summary>
        [BsonElement("additionalproperties")]
        public Dictionary<string, string> AdditionalProperties { get; set; }

        [BsonElement("roles")]
        public List<string> Roles { get; set; } = new List<string>();
    }

    #endregion

    #region REGISTRY

    [BsonIgnoreExtraElements]
    public class ListOfItems
    {
        public List<Items> Items { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class Items
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string iprodcustomerid { get; set; }

        [BsonElement("code")]
        public string code { get; set; }

        [BsonElement("name")]
        public string name { get; set; }

        /// <summary>
        /// Possible values: product, tool, equipment, service, container.
        /// </summary>
        [BsonElement("type")]
        public string type { get; set; }

        [BsonElement("properties")]
        public Dictionary<string, string> properties { get; set; }

        [BsonElement("versionproperties")]
        public List<versionproperties> versionedproperties { get; set; }

        [BsonElement("creationdate")]
        public DateTime creationDate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime lastUpdate { get; set; }

        [BsonElement("imgurl")]
        public string imgurl { get; set; }

        [BsonElement("phases")]
        public List<Phaseid> phases { get; set; }

        [BsonElement("status")]
        public string status { get; set; }

        [BsonElement("stock")]
        public List<stockdata> stockinformation { get; set; }

        [BsonElement("cost")]
        public double cost { get; set; }

        [BsonElement("listprices")]
        public List<listprice> listprices { get; set; }

        [BsonElement("linkedcustomers")]
        public List<string> linkedcustomers { get; set; } = new List<string>();
    }


    [BsonIgnoreExtraElements]
    public class listprice
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("creationdate")]
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Can assume value <see cref="ObjectId.Empty"/> when the price list is meant for everyone.<br />
        /// In this case, <see cref="listprice.CustomerVat"/> must be <see langword="null"/>.
        /// </summary>
        [BsonElement("customerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Customerid { get; set; }

        /// <summary>
        /// Must be <see langword="null"/> when <see cref="listprice.Customerid"/> is <see cref="ObjectId.Empty"/>.
        /// </summary>
        [BsonElement("customervat")]
        public string CustomerVat { get; set; }

        [BsonElement("customername")]
        public string CustomerName { get; set; }

        [BsonElement("marketplace")]
        public bool Marketplace { get; set; }

        [BsonElement("price")]
        public double? price { get; set; }

        [BsonElement("vat")]
        public double? vat { get; set; }

        [BsonElement("startdate")]
        public DateTime? startdate { get; set; }

        [BsonElement("enddate")]
        public DateTime? enddate { get; set; }

        [BsonElement("authorid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Authorid { get; set; }

        [BsonElement("authorname")]
        public string AuthorName { get; set; }

        /// <summary>
        /// Possible values: "active", "inactive" and "deleted".
        /// </summary>
        [BsonElement("status")]
        public string status { get; set; }

        [BsonElement("showavailability")]
        public bool showavailability { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class versionproperties
    {
        [BsonElement("versionname")]
        public string versionname { get; set; }

        [BsonElement("bomexponentid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string bomexponentid { get; set; }

        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; }

        [BsonElement("status")]
        public string status { get; set; }
    
    }

    [BsonIgnoreExtraElements]
    public class Phaseid
    {
        [BsonElement("phaseid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string phaseid { get; set; }
    }

    #endregion

    #region trackingdata

    [BsonIgnoreExtraElements]
    public class Trackingdata
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Iprodcustomerid { get; set; }

        [BsonElement("itemtracking")]
        public List<Itemtracking> Itemtracking { get; set; }

        [BsonElement("phaseistanceid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Phaseistanceid { get; set; }

        [BsonElement("timesheetdata")]
        public List<Timesheetdata> Timesheetdata { get; set; }
    }


    #region TBD
    //TBD
    [BsonIgnoreExtraElements]
    public class Trackingusers
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Iprodcustomerid { get; set; }

        [BsonElement("userid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Userid { get; set; }

        [BsonElement("registrationdate")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Registrationdate { get; set; }

        [BsonElement("referenceday")]
        [BsonRepresentation(BsonType.DateTime)]
        public DateTime Referenceday { get; set; }

        [BsonElement("timesheetdata")]
        public List<UserTimeSheet> Usertimesheets { get; set; }
    }




    [BsonIgnoreExtraElements]
    public class UserTimeSheet
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("start")]
        public DateTime Start { get; set; }

        [BsonElement("end")]
        public DateTime End { get; set; }

        [BsonElement("totaltime")]
        public double Totaltime { get; set; }

        [BsonElement("closed")]
        public bool closed { get; set; }


    }
    #endregion
    //TBD

    [BsonIgnoreExtraElements]
    public class TelemetryData
    {
        [BsonElement("customerid")]
        public ObjectId customerid { get; set; }

        [BsonElement("deviceid")]
        public ObjectId deviceid { get; set; }

        [BsonElement("deviceguid")]
        public string deviceguid { get; set; }

        [BsonElement("datetime")]
        public DateTime datetime { get; set; }

        [BsonElement("datatype")]
        public string datatype { get; set; }

        [BsonElement("data")]
        public BsonDocument data { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Itemtracking
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("timesheetid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string timesheetid { get; set; }

        [BsonElement("iduser")]
        public ObjectId iduser { get; set; }

        [BsonElement("type")]
        public string Type { get; set; }

        [BsonElement("value")]
        public int Value { get; set; }

        [BsonElement("date")]
        public DateTime Date { get; set; }
    }

    /// <summary>
    /// Represents a window of time within which an user is operating a machine.
    /// </summary>
    [BsonIgnoreExtraElements]
    public class Timesheetdata
    {
        /// <summary>
        /// The id of this window.
        /// </summary>
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string id { get; set; } = ObjectId.GenerateNewId().ToString();

        /// <summary>
        /// The id (property <see cref="Customerusers._id"/>) of the operator assigned to this window.
        /// </summary>
        [BsonElement("iduser")]
        public ObjectId iduser { get; set; }

        /// <summary>
        /// The status of the machine in this window (e.g. "production", "pause"...).
        /// </summary>
        [BsonElement("type")]
        public string Type { get; set; }

        /// <summary>
        /// Start time of the window in UTC.
        /// </summary>
        [BsonElement("start")]
        public DateTime Start { get; set; }

        /// <summary>
        /// End time of the window in UTC.<br />If the window is not closed yet, then this property assumes the value "1990-01-01".
        /// </summary>
        [BsonElement("end")]
        public DateTime End { get; set; }

        /// <summary>
        /// Window duration expressed in minutes.
        /// </summary>
        [BsonElement("totaltime")]
        public double Totaltime { get; set; }

        /// <summary>
        /// Indicates whether this window has terminated.
        /// </summary>
        [BsonElement("closed")]
        public bool closed { get; set; }
    }

    #endregion

    #region phase-post

    [BsonIgnoreExtraElements]
    public class ListOfPhase
    {
        public List<Phase> Phases { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class ListOfPost
    {
        public List<Posts> Posts { get; set; }
    }



    [BsonIgnoreExtraElements]
    public class Phase
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("compatiblemachines")]
        public List<Compatiblemachines> Compatiblemachines { get; set; }

        [BsonElement("creationdate")]
        public DateTime Creationdate { get; set; }

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Iprodcustomerid { get; set; }

        [BsonElement("itemid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Itemid { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("timeforitem")]
        public double Timeforitem { get; set; }

        [BsonElement("Timeforprogramming")]
        public double Timeforprogramming { get; set; }

        [BsonElement("Timeforpreproduction")]
        public double Timeforpreproduction { get; set; }

        [BsonElement("Timefortooling")]
        public double Timefortooling { get; set; }

        [BsonElement("Timeforcleaning")]
        public double Timeforcleaning { get; set; }

        [BsonElement("updatedate")]
        public DateTime Updatedate { get; set; }

        [BsonElement("status")]
        public string status { get; set; }

        [BsonElement("additionalproperties")]
        public Dictionary<string, string> additionalproperties { get; set; }

        [BsonElement("tag")]
        public List<string> tag { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class AdditionalProperties
    {
        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; }

        [BsonElement("value")]
        public string value { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class Compatiblemachines
    {
        [BsonElement("machineid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Machineid { get; set; }
    }





    //Post Classification by TypeProduceditem
    //    5° Event



    [BsonIgnoreExtraElements]
    public class Posts
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Iprodcustomerid { get; set; }

        [BsonElement("creationdate")]
        public DateTime Creationdate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime Lastupdate { get; set; }

        [BsonElement("authorid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string AuthorId { get; set; }

        [BsonElement("authorname")]
        public string AuthorName { get; set; }

        [BsonElement("type")]
        public int Type { get; set; }

        [BsonElement("category")]
        public string category { get; set; }

        [BsonElement("visibility")]
        public List<string> visibility { get; set; }

        [BsonElement("tag")]
        public string Tag { get; set; }

        [BsonElement("textvalue")]
        public string Textvalue { get; set; }

        [BsonElement("url")]
        public string Url { get; set; }

        [BsonElement("deleted")]
        public bool deleted { get; set; } = false;

        [BsonElement("linkeddata")]
        public Linkeddatapost Linkeddata { get; set; }

        [BsonElement("filedata")]
        public Filedata Filedata { get; set; }

        [BsonElement("Additionaldata")]
        public Dictionary<string, string> Additionaldata { get; set; }

        [BsonElement("compatibleobjects")]
        public List<string> CompatibleObjects { get; set; }

        [BsonElement("Socialdata")]
        public Dictionary<string, string> Socialdata { get; set; }

        [BsonElement("statusdeleted")]
        public string statusdeleted { get; set; }

    }


    [BsonIgnoreExtraElements]
    public class EventLog
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement("iprodcustomerid")]
        public ObjectId Iprodcustomerid { get; set; }

        [BsonElement("creationdate")]
        public DateTime Creationdate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime Lastupdate { get; set; }

        [BsonElement("authorid")]
        public ObjectId AuthorId { get; set; }

        [BsonElement("authorname")]
        public string AuthorName { get; set; }

        [BsonElement("type")]
        public int Type { get; set; }

        [BsonElement("tag")]
        public string Tag { get; set; }

        [BsonElement("title")]
        public string Title { get; set; }

        [BsonElement("textvalue")]
        public string Textvalue { get; set; }

        [BsonElement("url")]
        public string Url { get; set; }

        [BsonElement("deleted")]
        public bool deleted { get; set; } = false;

        [BsonElement("linkeddata")]
        public Linkeddatapost Linkeddata { get; set; }

        [BsonElement("filedata")]
        public Filedata Filedata { get; set; }

        [BsonElement("Additionaldata")]
        public Dictionary<string, string> Additionaldata { get; set; }

        [BsonElement("Socialdata")]
        public Dictionary<string, string> Socialdata { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class SharingData
    {
        [BsonElement("destinationcustomer")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string destcustomerid { get; set; }

        [BsonElement("destinatiouser")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string destuserid { get; set; }

        [BsonElement("status")]
        public string status { get; set; }  //Sent, Accept, Reject, Banned

        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; }

        [BsonElement("creationdate")]
        public DateTime creationdate { get; set; }

        [BsonElement("creator")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string creatorid { get; set; }
    }



    [BsonIgnoreExtraElements]
    public class Filedata
    {
        [BsonElement("creationdate")]
        public DateTime? Creationdate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime? Lastupdate { get; set; }

        [BsonElement("lastpath")]
        public string Lastpath { get; set; }


    }



    [BsonIgnoreExtraElements]
    public class Linkeddatapost
    {
        [BsonElement("phaseid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Phaseid { get; set; }

        [BsonElement("postid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Postid { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class SalesOrderModel
    {
        public SalesOrder newsalesorder { get; set; } = new SalesOrder();
        public List<SalesOrder> listofsalesorder { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class SalesOrder
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Iprodcustomerid { get; set; }

        [BsonElement("customerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Customerid { get; set; }

        [BsonElement("code")]
        public string code { get; set; }

        [BsonElement("creationdate")]
        public DateTime Creationdate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; }

        [BsonElement("status")]
        public int status { get; set; }

        [BsonElement("items")]
        public List<OrderedItems> OrderedItems { get; set; } = new List<OrderedItems>();

        [BsonElement("statusdeleted")]
        public string statusdeleted { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class OrderedItems
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("itemid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Itemid { get; set; }

        [BsonElement("qty")]
        public Double Qty { get; set; }

        [BsonElement("deliverydate")]
        public DateTime Deliverydate { get; set; }

        [BsonElement("status")]
        public string status { get; set; }


        [BsonElement("additionalreference")]
        public Dictionary<string, string> additionalreference { get; set; }

        [BsonElement("elaborationstatus")]
        public string elaborationstatus { get; set; } = "notevalutated";

        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; } = DateTime.UtcNow;

        [BsonElement("qtydelivered")]
        public double Qtydelivered { get; set; } = 0;

        [BsonElement("lastdelivered")]
        public DateTime Lastdelivered { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class CustomerDestination
    {
        [BsonElement("legalname")]
        public string legalname { get; set; }

        [BsonElement("address")]
        public string address { get; set; }

        [BsonElement("city")]
        public string city { get; set; }

        [BsonElement("zip")]
        public string zip { get; set; }

        [BsonElement("country")]
        public string country { get; set; }

        [BsonElement("additionalreference")]
        public Dictionary<string, string> AdditionalReference { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class Customers
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("iprodcustomerguid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string iprodcustomerguid { get; set; }

        [Required(ErrorMessage = "Value is required")]
        [BsonElement("name")]
        public string name { get; set; }

        [BsonElement("city")]
        public string city { get; set; }

        [BsonElement("address")]
        public string address { get; set; }

        [BsonElement("zip")]
        public string zip { get; set; }

        [BsonElement("vat")]
        public string vat { get; set; }

        [BsonElement("destinations")]
        public List<CustomerDestination> CustomerDestinations { get; set; }

        [BsonElement("contacts")]
        public List<ContactId> contacts { get; set; }

        [BsonElement("creationdate")]
        public DateTime creationdate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; }

        [BsonElement("status")]
        public string status { get; set; }

        [BsonElement("customer")]
        public bool customer { get; set; }

        [BsonElement("supplier")]
        public bool supplier { get; set; }

        [BsonElement("fiscalcode")]
        public string fiscalcode { get; set; }

        [BsonElement("warehouses")]
        public List<warehouse> warehouses { get; set; }

        [BsonElement("additionalreference")]
        public Dictionary<string, string> additionalreference { get; set; }
        //<View, write> =>  complete control
        //<View, read> =>  the View is visible, but the User can't change anything: save, delete, edit, create.
        //<View, null> =>  the View is NOT accessable
    }

    [BsonIgnoreExtraElements]
    public class ContactId
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("id")]
        public string Id { get; set; }
    }

    

    [BsonIgnoreExtraElements]
    public class Contact
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Iprodcustomerid { get; set; }

        /// <summary>
        /// The id of the company (i.e. <see cref="Customers._id"/>) this contact belongs to.<br/>
        /// If the contact does not belong to any company, then this property must be <see langword="null"/>.
        /// </summary>
        [BsonElement("companyid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Companyid { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("lastname")]
        public string Lastname { get; set; }

        [BsonElement("avatar")]
        public string Avatar { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("mobilephone")]
        public string Mobilephone { get; set; }

        [BsonElement("companyphone")]
        public string Companyphone { get; set; }

        [BsonElement("companyrole")]
        public string Companyrole { get; set; }

        [BsonElement("notes")]
        public string Notes { get; set; }

        /// <summary>
        /// Assumes value "deleted" when the contact is logically deleted.
        /// </summary>
        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("additionalreference")]
        public Dictionary<string, string> additionalreference { get; set; }

        [BsonElement("additionalproperties")]
        public Dictionary<string, string> additionalproperties { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Contactlist
    {
        [BsonElement("type")]
        public string type { get; set; }

        [BsonElement("value")]
        public string value { get; set; }

        [BsonElement("name")]
        public string name { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class WorkOrders
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonElement("iprodcustomerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Iprodcustomerid { get; set; }

        [BsonElement("customerid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string customerid { get; set; }

        [BsonElement("creationdate")]
        public DateTime Creationdate { get; set; }

        [BsonElement("updatedate")]
        public DateTime Updatedate { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("salesorderid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Salesorderid { get; set; }

        [BsonElement("itemorderedid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string itemorderedid { get; set; }

        [BsonElement("status")]
        public int Status { get; set; }

        [BsonElement("items")]
        public List<WorkOrderItems> items { get; set; }

        [BsonElement("sharingdata")]
        public List<SharingData> sharingdata { get; set; }

        [BsonElement("statusdeleted")]
        public string statusdeleted { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class WorkOrderItems
    {
        [BsonElement("itemid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Itemid { get; set; }

        [BsonElement("qty")]
        public double Qty { get; set; }

        [BsonElement("endingdate")]
        public DateTime Endingdate { get; set; }

        [BsonElement("phasesistances")]
        public List<Phasesinstances> phasesistances { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class Phasesinstances
    {
        [BsonElement("phaseistanceid")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Phaseistanceid { get; set; }
    }


    public class Link
    {
        public string title { get; set; }
        public string rel { get; set; }
        public string href { get; set; }
    }

    public class alleantiafolder
    {
        public string name { get; set; }
        public string type { get; set; }
        public int size { get; set; }
        public List<Link> links { get; set; }
        public DateTime? lastModified { get; set; }
    }
    #endregion


    [BsonIgnoreExtraElements]
    public class catalog
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("category")]
        public string category { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("note")]
        public string Note { get; set; }

        [BsonElement("vendor")]
        public string Vendor { get; set; }

        [BsonElement("price")]
        public double Price { get; set; }

        [BsonElement("imgurl")]
        public string imgurl { get; set; }
    }

    public class RegisterViewModel
    {
        [Required]
        public string Companyname { get; set; }

        public string Vat { get; set; }

        [Required]
        public string Surname { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string Mobile { get; set; }


        //public List<SelectListItem> TimeZoneList { get; set; }

        [Required]
        public string UserTimeZone { get; set; }

        [Range(typeof(bool), "true", "true", ErrorMessage = "Please check the box to continue")]
        public bool TermsAndConditions { get; set; }
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please check the box to continue")]
        public bool TermsAndConditions2 { get; set; }
        [Range(typeof(bool), "true", "true", ErrorMessage = "Please check the box to continue")]
        public bool TermsAndConditions3 { get; set; }
    }


    #region sharingdata

    public class SharingUser
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyLogo { get; set; }

        public ObjectId userid { get; set; }
        public string username { get; set; }
        public string useremail { get; set; }
    }

    #endregion

    #region Gantt
    public class GanttTask
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public decimal Progress { get; set; }
        public string ParentId { get; set; }
        public string Type { get; set; }
    }

    public class GanttLink
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string SourceTaskId { get; set; }
        public string TargetTaskId { get; set; }
    }

    public class Ganttdata
    {
        public List<GanttTask> tasklist { get; set; }
        public List<GanttLink> tasklink { get; set; }
    }

    public class searchtag
    {
        public string name { get; set; }
        public string link { get; set; }
        public string guid { get; set; }
    }

    public class SchedulerEvent
    {
        public string id { get; set; }
        public string text { get; set; }
        public string start_date { get; set; }
        public string end_date { get; set; }
        public string color { get; set; }
        public string textColor { get; set; } = "black";
    }

    public class schedulerdata
    {
        public List<SchedulerEvent> data { get; set; }
    }
    #endregion


    #region operator activity
    public class activitydata
    {
        public List<action> actions { get; set; }
        public timeline timeline { get; set; }
        public List<streams> streams { get; set; }
        public global global { get; set; }
        public string activitydate { get; set; }
    }

    public class action
    {
        public string html { get; set; }
    }
    public class timeline
    {
        public string start { get; set; }
        public string stop { get; set; }
        public int step { get; set; }
    }
    public class streams
    {
        public string title { get; set; }
        public string secondary { get; set; }
        public string icon { get; set; }
        public string cls { get; set; }
        public List<events> events { get; set; }
    }
    public class events
    {
        public string icon { get; set; }
        public string time { get; set; }
        public string title { get; set; }
        public string subtitle { get; set; }
        public string desc { get; set; }
        public int size { get; set; }
        public string cls { get; set; }
    }
    public class global
    {
        public string before { get; set; }
        public string after { get; set; }
    }



    public class AppointmentData
    {
        public string Id { get; set; }
        public string Subject { get; set; }
        public string Phaseistanceid { get; set; }
        public string Phase { get; set; }
        public string Machinename { get; set; }
        public string Detail { get; set; }
        public string MachineId { get; set; }
        public string Status { get; set; }
        public double Parts { get; set; }
        public string OwnerId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string RecurrenceRule { get; set; }
    }

    public class Owner
    {
        public string OwnerId { get; set; }
        public string Name { get; set; }
    }

    public class MachineData
    {
        public string MachineId { get; set; }
        public string Name { get; set; }
    }

    #endregion



    #region Forgot password
    public class ForgotPassword
    {
        [Required]
        public string Email { get; set; }
    }
    public class ResetPassword
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
        public string Code { get; set; }
    }
    #endregion

    #region Store

    [BsonIgnoreExtraElements]
    public class StoreOrder
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement("iprodcustomerid")]
        public ObjectId Iprodcustomerid { get; set; }

        [BsonElement("creationdate")]
        public DateTime Creationdate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime Lastupdate { get; set; }

        [BsonElement("userid")]
        public ObjectId Userid { get; set; }

        [BsonElement("status")]
        public string Status { get; set; }

        [BsonElement("total")]
        public double Total { get; set; }

        [BsonElement("totaltax")]
        public double Totaltax { get; set; }

        [BsonElement("subtotal")]
        public double Subtotal { get; set; }

        [BsonElement("reference")]
        public Dictionary<string, string> Reference { get; set; }

        [BsonElement("detail")]
        public List<StoreOrderDetail> detail { get; set; }

    }

    [BsonIgnoreExtraElements]
    public class StoreOrderDetail
    {
        [BsonId]
        public ObjectId _id { get; set; }

        [BsonElement("iprodcustomerid")]
        public ObjectId Iprodcustomerid { get; set; }

        [BsonElement("creationdate")]
        public DateTime Creationdate { get; set; }

        [BsonElement("lastupdate")]
        public DateTime Lastupdate { get; set; }

        [BsonElement("orderid")]
        public ObjectId Orderid { get; set; }

        [BsonElement("productcode")]
        public string code { get; set; }

        [BsonElement("productdescription")]
        public string description { get; set; }

        [BsonElement("um")]
        public string um { get; set; }

        [BsonElement("qty")]
        public double qty { get; set; }

        [BsonElement("total")]
        public double Total { get; set; }

        [BsonElement("totaltax")]
        public double Totaltax { get; set; }

        [BsonElement("subtotal")]
        public double Subtotal { get; set; }

        [BsonElement("status")]
        public string status { get; set; }

        [BsonElement("reference")]
        public Dictionary<string, string> Reference { get; set; }

    }

    #endregion


    #region BOM

    [BsonIgnoreExtraElements]
    public class Bom
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("iprodcustomerid")]
        public string iprodcustomerid { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("code")]
        public string code { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("name")]
        public string name { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("status")]
        public string status { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("creationdate")]
        public DateTime creationdate { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("authorid")]
        public string authorid { get; set; }

        [BsonElement("exponents")]
        public List<Exponent> exponents { get; set; }

        [BsonElement("properties")]
        public Dictionary<string, string> properties { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Exponent
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("code")]
        public string code { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("startdate")]
        public DateTime startdate { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("enddate")]
        public DateTime enddate { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("status")]
        public string status { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("creationdate")]
        public DateTime creationdate { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("authorid")]
        public string authorid { get; set; }

        [BsonElement("produceditems")]
        public List<Produceditem> produceditems { get; set; }

        [BsonElement("bomtree")]
        public List<BomTreeNode> bomtree { get; set; }

        [BsonElement("properties")]
        public Dictionary<string, string> properties { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Produceditem
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("itemid")]
        public string itemid { get; set; }

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("qty")]
        public double qty { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class BomTreeNode
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string treeid { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string fatherid { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public double qty { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public double timeconsumption { get; set; }     

        [BsonRepresentation(BsonType.Int32)]
        public int type { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string typeid { get; set; }


        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("code")]
        public string Code { get; set; }

        [BsonElement("properties")]
        public Dictionary<string, string> properties { get; set; } = new Dictionary<string, string>();

        [BsonElement("sons")]
        public List<BomTreeNode> sons { get; set; } = new List<BomTreeNode>();
    }


    #endregion


    #region ERP

    [BsonIgnoreExtraElements]
    public class document
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        [Required]
        public string _id { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("iprodcustomerid")]
        [Required]
        public string Iprodcustomerid { get; set; }

        [Required]
        [BsonRepresentation(BsonType.String)]
        public string status { get; set; }

        [Required]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("documenttype")]
        public string DocumentType { get; set; }

        [Required]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("internaldocnumber")]
        public string Internaldocnumber { get; set; }


        [Required]
        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("docnumber")]
        public long Docnumber { get; set; }


        [Required]
        [BsonRepresentation(BsonType.String)]
        [BsonElement("externaldocnumber")]
        public string Externaldocnumber { get; set; }

        [Required]
        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("registrationdate")]
        public DateTime Registrationdate { get; set; } = DateTime.UtcNow;


        [Required]
        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("documentdate")]
        public DateTime Documentdate { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("customerid")]
        public string Customerid { get; set; }

        [Required]
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("documentreasonid")]
        public string Documentreasonid { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("sourcewarehouseid")]
        public string SourceWarehouseId { get; set; } = ObjectId.Empty.ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("destinationwarehouseid")]
        public string DestinationWarehouseId { get; set; } = ObjectId.Empty.ToString();

        [BsonElement("destination")]
        public destination Destination { get; set; } = new destination();

        [BsonElement("documentdetail")]
        public List<documentdetail> Documentdetail { get; set; } = new List<documentdetail>();

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("lastupdate")]
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        [BsonRepresentation(BsonType.Boolean)]
        [BsonElement("needelaboration")]
        public bool needelaboration { get; set; } = false;

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("lastelaboration")]
        public DateTime Lastelaboration { get; set; }


        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("authorid")]
        public string AuthorId { get; set; } = ObjectId.Empty.ToString();

        [BsonElement("additionalproperties")]
        public Dictionary<string, string> additionalproperties { get; set; } = new Dictionary<string, string>();

        [BsonElement("additionalreference")]
        public Dictionary<string, string> additionalreference { get; set; } = new Dictionary<string, string>();

        [BsonElement("eventlogger")]
        public List<objectevent> events { get; set; } = new List<objectevent>();
    }


    [BsonIgnoreExtraElements]
    public class objectevent
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("eventid")]
        public string eventid { get; set; } = ObjectId.Empty.ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("authorid")]
        public string AuthorId { get; set; } = ObjectId.Empty.ToString();

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("eventdate")]
        public DateTime eventdate { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("eventtype")]
        public string eventtype { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("note")]
        public string note { get; set; }

    }

    public class documentviewmodel
    {
        public string _id { get; set; }

        public string internalnumber { get; set; }

        public DateTime registrationdate { get; set; }

        public string status { get; set; }

        public string sendername { get; set; }

        public DateTime documentdate { get; set; }

        public string documentnumber { get; set; }

        public string documenttype { get; set; }

        public string documentreason { get; set; }

        public int numline { get; set; }

        public double numqty { get; set; }

        public double numpackages { get; set; }

        public string linkeddocuments { get; set; }

        public string authorname { get; set; }

        public List<documentdetail> detail { get; set; }
    }




    [BsonIgnoreExtraElements]
    public class documentdetail
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.String)]
        [BsonElement("status")]
        public string Status { get; set; } = "ok";

        [BsonRepresentation(BsonType.Int64)]
        [BsonElement("line")]
        public int Line { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("productid")]
        public string Productid { get; set; } = ObjectId.Empty.ToString();

        [BsonRepresentation(BsonType.String)]
        [BsonElement("code")]
        public string Code { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("description")]
        public string Description { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("mu")]
        public string mu { get; set; }

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("qty")]
        public double? qty { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("price")]
        public double? Price { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("total")]
        public double? Total { get; set; } = 0;

        [BsonRepresentation(BsonType.String)]
        [BsonElement("mubilled")]
        public string mubilled { get; set; }

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("qtybilled")]
        public double? qtybilled { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("pricebilled")]
        public double? PriceBilled { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("totalbilled")]
        public double? TotalBilled { get; set; } = 0;


        [BsonRepresentation(BsonType.Double)]
        [BsonElement("vat")]
        public double? vat { get; set; } = 0;


        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("source")]
        public string Source { get; set; } = ObjectId.Empty.ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("destination")]
        public string Destination { get; set; } = ObjectId.Empty.ToString();

        [BsonElement("additionalproperties")]
        public Dictionary<string, string> AdditionalProperties { get; set; }

        [BsonElement("linkeddata")]
        public Dictionary<string, string> Linkedata { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("lastupdate")]
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

        [BsonElement("transactionlog")]
        public List<transactionmovement> TransactionLog { get; set; }

        [BsonElement("scheduleddeliverydate"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? ScheduledDeliveryDate { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class documentsaveresult
    {
        public string status { get; set; } = "";
        public Dictionary<string, string> anomaly { get; set; } = new Dictionary<string, string>();

        public document _document;

    }

    [BsonIgnoreExtraElements]
    public class transactionmovement
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("_id")]
        [BsonId]
        public string _id { get; set; } = ObjectId.Empty.ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("iprodcustomerid")]
        public string iprodcustomerid { get; set; } = ObjectId.Empty.ToString();


        [BsonRepresentation(BsonType.String)]
        [BsonElement("movtype")]
        public string MovType { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("type")]
        public string Type { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("idtype")]
        public string IdType { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("date")]
        public DateTime Date { get; set; }

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("value")]
        public double Value { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("positionwhid")]
        public string PositionWhid { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("reftype")]
        public string RefType { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("reference")]
        public string Reference { get; set; }

        [BsonRepresentation(BsonType.String)]
        [BsonElement("status")]
        public string status { get; set; }

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("lastupdate")]
        public DateTime LastUpdate { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class destination
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("destinationid")]
        public string DestinationId { get; set; } = ObjectId.Empty.ToString();

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("warehouseid")]
        public string warehouseid { get; set; } = ObjectId.Empty.ToString();
    }

    [BsonIgnoreExtraElements]
    public class stockdata
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("date")]
        public DateTime date { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        [BsonElement("warehouseid")]
        public string warehouseid { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("input")]
        public double input { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("expectedinput")]
        public double expectedinput { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("output")]
        public double output { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("expectedoutput")]
        public double expectedoutput { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("balance")]
        public double balance { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("expectedbalance")]
        public double expectedbalance { get; set; } = 0;

        [BsonRepresentation(BsonType.Double)]
        [BsonElement("expectedbalancenoinput")]
        public double expectedbalancenoinput { get; set; } = 0;


        [BsonRepresentation(BsonType.Boolean)]
        [BsonElement("inventory")]
        public bool inventory { get; set; } = false;

        [BsonRepresentation(BsonType.Boolean)]
        [BsonElement("obsolete")]
        public bool obsolete { get; set; } = false;

        [BsonRepresentation(BsonType.DateTime)]
        [BsonElement("lastupdate")]
        public DateTime lastupdate { get; set; }


        [BsonElement("properties")]
        public Dictionary<string, string> properties { get; set; }
    }


    [BsonIgnoreExtraElements]
    public class Label
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonRepresentation(BsonType.DateTime)]
        public DateTime date { get; set; } = DateTime.Now;

        [BsonRepresentation(BsonType.String)]
        public string customer { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string code { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string description { get; set; }

        [BsonRepresentation(BsonType.Double)]
        public double qty { get; set; } = 0;

        [BsonRepresentation(BsonType.String)]
        public string woname { get; set; }

        [BsonRepresentation(BsonType.String)]
        public List<string> materials { get; set; }

        [BsonRepresentation(BsonType.String)]
        public string user { get; set; }
    }

    public class ICRUDModel<T> where T : class
    {
        public string action { get; set; }

        public string table { get; set; }

        public string keyColumn { get; set; }

        public object key { get; set; }

        public T value { get; set; }

        public List<T> added { get; set; }

        public List<T> changed { get; set; }

        public List<T> deleted { get; set; }

        public IDictionary<string, object> @params { get; set; }
    }


    #endregion

    public class SearchResult
    {
        public string id { get; set; }
        public string type { get; set; }
        public string htmlvalue { get; set; }
        public Dictionary<string, string> additionalproperties { get; set; }
    }


    public class SearchData
    {
        public int take { get; set; }
        public List<Wheres> where { get; set; }
        [JsonProperty(PropertyName = "defaultValue")] public string defaultValue { get; set; }
    }

    public class Wheres
    {
        public string field { get; set; }
        public bool ignoreAccent { get; set; }

        public bool ignoreCase { get; set; }

        public bool isComplex { get; set; }

        public string value { get; set; }
        public string Operator { get; set; }

    }


    #region customproperty

    [BsonIgnoreExtraElements]
    public class ReasonProperty
    {
        public string input { get; set; }
        public string output { get; set; }
        public string inventory { get; set; }
        public string printvalue { get; set; }
        public string report { get; set; }
        public string quote { get; set; }
        public string havevalue { get; set; }
        public string extwork { get; set; }
        public string buy { get; set; }
        public string repair { get; set; }
        public string qtychange { get; set; }
        public string qualitycontrol { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Reason
    {
        [BsonRepresentation(BsonType.ObjectId)]
        public string reasonid { get; set; }
        [BsonRepresentation(BsonType.String)]
        [BsonElement("name")]
        public string Name { get; set; }
        [BsonElement("properties")]
        public List<ReasonProperty> ReasonProperties { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class Documenttype
    {
        [BsonRepresentation(BsonType.String)]
        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("reasons")]
        public List<Reason> Reasons { get; set; }
    }

    [BsonIgnoreExtraElements]
    public class customproperty
    {
        [BsonRepresentation(BsonType.ObjectId)]
        [BsonId]
        public string _id { get; set; }

        [BsonElement("documenttypes")]
        public List<Documenttype> DocumentTypes { get; set; }
    }
    #endregion


    public class cardelement
    {
        public string id { get; set; }

        public string typeid { get; set; }

        public string header { get; set; }

        public string imgurl { get; set; }
        public string content { get; set; }

        public string footer { get; set; }
    }

    public class SplineAreaChartData
    {
        public DateTime xValue;
        public double yValue;
        public double yValue1;
        public double yValue2;
    }
}


