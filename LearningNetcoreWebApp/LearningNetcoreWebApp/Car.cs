using System.Collections.Generic;

namespace LearningNetcoreWebApp
{
    public class make
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public int Wt { get; set; }
        public int SoldCount { get; set; }
    }

    public class CarList
    {
        public string Id { get; set; }
        public CarSuggestion mm_suggest { get; set; }
        public string name { get; set; }
        public Payload payload { get; set; }
        public string output { get; set; }
    }

    public class CarSuggestion
    {
        public List<string> input { get; set; }
        public int weight { get; set; }
    }
    public class Payload
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public int SoldCount { get; set; }
        public int Year { get; set; }
    }
}