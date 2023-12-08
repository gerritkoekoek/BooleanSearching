using System.Runtime.CompilerServices;

namespace BooleanSearching
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Test();

            Console.ReadLine();
        }

        static void Test()
        {
            //string SearchQuery = "(gerrit AND koop NOT henk) OR (gerrit AND henk) OR (test1 OR test2 OR test3 AND (test4 AND test5 OR (gerritIsDik)) OR test6) OR \"koop\" AND kaas eieren";
            //string SearchQuery = "Gerrit AND 0623456789 AND kaas";

            //string SearchQuery = "test OR genemuiden"; //WERKT
            //string SearchQuery = "PHP AND koekoek AND zwolle"; //WERKT
            //string SearchQuery = "lazy AND 0691191100 NOT zwartsluis"; //WERKT
            //string SearchQuery = "test OR genemuiden AND (0691191100 OR 0691191100)"; //WERKT
            //string SearchQuery = "lazy AND 0691191100 AND zwartsluis"; //WERKT
            //string SearchQuery = "lazy AND sluis"; //WERKT
            //string SearchQuery = "lazy AND \"genemuiden\""; //WERKT
            //string SearchQuery = "lazy AND \"enemuiden\""; //WERKT

            //string SearchQuery = "\"verkoopt\"";
            //string SearchQuery = "Hoorn AND .NET OR C#";
            //string SearchQuery = "(Hoorn AND .NET OR C#)";
            //string SearchQuery = "Javascript AND .React AND .NET OR Angular";

            List<Candidate> OriginalDataset = new List<Candidate>()
            {
                new Candidate() { FirstName = "Gerrit", LastName = "Koekoek", Town = "Zwolle", PhoneNumber = "0623456789", Description = "This candidate is very good with PHP and Javascript" },
                new Candidate() { FirstName = "verkoopt", LastName = "Koekoek", Town = "Wezep", PhoneNumber = "0611223344", Description = "This candidate is very lazy" },
                new Candidate() { FirstName = "Henk", LastName = "Slot", Town = "Zwartsluis", PhoneNumber = "0691191100", Description = "Never heard of anyone this lazy" },
                new Candidate() { FirstName = "test1", LastName = "Slot", Town = "Genemuiden", PhoneNumber = "0691191100", Description = "kakhoofd lazy" },
                new Candidate() { FirstName = "test5", LastName = "Slot", Town = "Zwartsluis", PhoneNumber = "0623469230", Description = "Never heard of anyone this lazy" },
            };

            // Let's perform a boolean search
            // First we need to create a tree like structure based on our searchQuery
            BooleanSearchItem<Candidate> root = new BooleanSearchItem<Candidate>() { Parent = null, OriginalDataset = OriginalDataset, Children = new List<BooleanSearchItem<Candidate>>(), Type = BooleanSearchItemType.Evaluate, SearchQuery = SearchQuery };
            // Build our tree based on the search query
            root.BuildTree();
            // Get our boolean search result
            List<Candidate> results = root.GetResults();

            Console.WriteLine(root.ToStringNice());
        }
    }
}