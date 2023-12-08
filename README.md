# BooleanSearching
Boolean search algorithm for C#

**Supports the following search types:**
- Exact search -> "searchTerm"
- Normal search -> searchTerm
- AND -> searchTerm AND searchTerm2
- OR -> searchTerm OR searchTerm2
- NOT -> searchTerm NOT searchTerm2
- Nested search queries -> searchTerm AND searchTerm2 OR (searchTerm3 AND searchTerm4)

**Usage:**
```
static void Test() {
  string SearchQuery = "test AND genemuiden";

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
```
**Which results in:**
```
Results:
test1 Slot Genemuiden 0691191100 kakhoofd lazy

+- test AND genemuiden (Evaluate (Results: 1) (Solved: True | Time: 1702022397743))
   +- test (SearchString (Results: 2) (Solved: True | Time: 1702022397741))
   +- AND (BooleanOperatorAND (Results: 1) (Solved: True | Time: 1702022397743))
   +- genemuiden (SearchString (Results: 1) (Solved: True | Time: 1702022397741))
```
