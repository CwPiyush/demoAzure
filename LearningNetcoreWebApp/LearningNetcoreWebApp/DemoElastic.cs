using System;
using System.Collections.Generic;
using System.Linq;
using Nest;

namespace ElasticSearchDemo
{
    public class Employee
    {
        public int EmpId { set; get; }

        public string Name { set; get; }

        public string Department { set; get; }

        public int Salary { set; get; }
    }
    class ElasticDemo
    {
        public static Uri EsNode;
        public static ConnectionSettings EsConfig;
        public static ElasticClient EsClient;
        //public static void demo()
        //{
        //    EsNode = new Uri("http://localhost:9200/");
        //    EsConfig = new ConnectionSettings(EsNode);
        //    EsClient = new ElasticClient(EsConfig);

        //    var settings = new IndexSettings { NumberOfReplicas = 1, NumberOfShards = 2 };

        //    var indexConfig = new IndexState
        //    {
        //        Settings = settings
        //    };

        //    if (!EsClient.IndexExists("employee").Exists)
        //    {
        //        EsClient.CreateIndex("employee", c => c
        //        .InitializeUsing(indexConfig)
        //        .Mappings(m => m.Map<Employee>(mp => mp.AutoMap())));
        //    }

        //    InsertDocument();
        //}

        public static void InsertDocument()
        {
            var lst = PopulateEmployees();

            foreach (var obj in lst.Select((value, counter) => new { counter, value }))
            {
                EsClient.Index(obj.value, i => i
                    .Index("employee")
                    .Type("myEmployee")
                    .Id(obj.counter)                    
                    );
            }
        }

        public static List<Employee> PopulateEmployees()
        {
            return new List<Employee>
            {
                new Employee {EmpId = 1, Name = "John", Department = "IT", Salary = 45000},
                new Employee {EmpId = 2, Name = "Will", Department = "Dev", Salary = 35000},
                new Employee {EmpId = 3, Name = "Henry", Department = "Dev", Salary = 25000},
                new Employee {EmpId = 4, Name = "Eric", Department = "Dev", Salary = 15000},
                new Employee {EmpId = 5, Name = "Steve", Department = "Dev", Salary = 65000},
                new Employee {EmpId = 6, Name = "Mike", Department = "QA", Salary = 75000},
                new Employee {EmpId = 7, Name = "Mark", Department = "QA", Salary = 55000},
                new Employee {EmpId = 8, Name = "Kevin", Department = "QA", Salary = 45000},
                new Employee {EmpId = 9, Name = "Haddin", Department = "Dev", Salary = 25000},
                new Employee {EmpId = 10, Name = "Smith", Department = "Dev", Salary = 15000}
            };
        }
    }
}