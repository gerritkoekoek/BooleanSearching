using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BooleanSearching
{
    public class BooleanSearchItem<T> where T : Entity
    {
        public BooleanSearchItem<T> Parent { get; set; }
        public List<BooleanSearchItem<T>> Children { get; set; }
        public BooleanSearchItemType Type { get; set; }
        public List<T> OriginalDataset { get; set; }
        private IEnumerable<T> Results { get; set; }
        public string SearchQuery { get; set; }
        private bool Solved = false;
        private long SolvedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        public BooleanSearchItem()
        {
            OriginalDataset = new List<T>();
            Results = new List<T>();
        }

        /// <summary>
        /// Build a booleanSearch tree based on a search query
        /// 
        /// </summary>
        /// <param name="searchQuery"></param>
        public void BuildTree()
        {
            if (Type == BooleanSearchItemType.SearchString ||
                Type == BooleanSearchItemType.ExactSearchString ||
                Type == BooleanSearchItemType.BooleanOperatorAND ||
                Type == BooleanSearchItemType.BooleanOperatorOR ||
                Type == BooleanSearchItemType.BooleanOperatorNOT)
            {
                return;
            }

            // Generate tokens from Search Query string
            List<string> tokens = GenerateTokensFromString();

            // Generate children from tokens
            GenerateChildrenFromTokens(tokens);

            // Complete the tree by letting each child build their trees
            foreach (BooleanSearchItem<T> child in Children)
            {
                child.BuildTree();
            }
        }

        private List<string> GenerateTokensFromString()
        {
            List<string> tokens = new List<string>();
            // Make sure we dont have trailing and leading whitespaces
            SearchQuery = SearchQuery.Trim();

            string token = string.Empty;
            bool found = false;
            for (int i = 0; i < SearchQuery.Length; i++)
            {
                found = false;
                char currentChar = SearchQuery[i];

                if (currentChar == ' ')
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        token += ' ';
                    }
                    else
                    {
                        token = string.Empty;
                    }

                    continue;
                }
                else if (currentChar == '(')
                {
                    if (!string.IsNullOrEmpty(token)) { tokens.Add(token); }
                    // Loop till we find our other )
                    token = "(";
                    int nestCounter = 1;
                    int closerFound = 0;
                    for (int a = i + 1; a < SearchQuery.Length; a++)
                    {
                        token += SearchQuery[a];
                        if (SearchQuery[a] == '(')
                        {
                            nestCounter += 1;
                        }
                        if (SearchQuery[a] == ')')
                        {
                            closerFound += 1;
                            // TODO check if it's either i = a OR i = a + 1
                            i = a;
                            found = true;

                            if (closerFound == nestCounter)
                            {
                                break;
                            }
                        }
                    }
                    if (found)
                    {
                        tokens.Add(token);
                        token = string.Empty;
                        continue;
                    }
                    else
                    {
                        // We didn't find an exiting quote! User error or ?
                        break;
                    }

                }
                else if (currentChar == '"')
                {
                    if (!string.IsNullOrEmpty(token)) { tokens.Add(token); }
                    // Loop till we find our other quote
                    token = "\"";
                    for (int b = i + 1; b < SearchQuery.Length; b++)
                    {
                        token += SearchQuery[b];
                        if (SearchQuery[b] == '"')
                        {
                            // Make sure to main loop continues after the last quote we just found
                            // TODO check if it's either i = b OR i = b + 1
                            i = b;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        tokens.Add(token);
                        token = string.Empty;
                        continue;
                    }
                    else
                    {
                        // We didn't find an exiting quote! User error or ?
                        break;
                    }
                }
                else
                {
                    // TODO i + 1 of i + 2 of i + 3 kan uiteraard buiten index liggen...
                    // OR
                    if (currentChar == 'O' && SearchQuery[i + 1] == 'R' && SearchQuery[i + 2] == ' ')
                    {
                        if (!string.IsNullOrEmpty(token)) { tokens.Add(token); }
                        token = "OR";
                        tokens.Add(token);
                        token = string.Empty;
                        i = i + 2;
                        continue;
                    }
                    // AND
                    else if (currentChar == 'A' && SearchQuery[i + 1] == 'N' && SearchQuery[i + 2] == 'D' && SearchQuery[i + 3] == ' ')
                    {
                        if (!string.IsNullOrEmpty(token)) { tokens.Add(token); }
                        token = "AND";
                        tokens.Add(token);
                        token = string.Empty;
                        i = i + 3;
                        continue;
                    }
                    // NOT
                    else if (currentChar == 'N' && SearchQuery[i + 1] == 'O' && SearchQuery[i + 2] == 'T' && SearchQuery[i + 3] == ' ')
                    {
                        if (!string.IsNullOrEmpty(token)) { tokens.Add(token); }
                        token = "NOT";
                        tokens.Add(token);
                        token = string.Empty;
                        i = i + 3;
                        continue;
                    }
                    // Mmh something else, most likely a regular search term ;)
                    else
                    {
                        token += SearchQuery[i];
                    }
                }
            }

            if (!string.IsNullOrEmpty(token))
            {
                tokens.Add(token);
            }

            return tokens;
        }

        private void GenerateChildrenFromTokens(List<string> tokens)
        {
            foreach (string token in tokens)
            {
                if (Children == null)
                {
                    Children = new List<BooleanSearchItem<T>>();
                }

                // Remove leading and trailing spaces
                string newToken = token.Trim();

                if (newToken.StartsWith("(") && newToken.EndsWith(")"))
                {
                    //Removes first ( and last )
                    newToken = newToken.Substring(1, newToken.Length - 2);
                    Children.Add(new BooleanSearchItem<T>() { OriginalDataset = this.OriginalDataset, Parent = this, Children = new List<BooleanSearchItem<T>>(), SearchQuery = newToken, Type = BooleanSearchItemType.Evaluate });
                }
                else if (newToken.StartsWith("\"") && newToken.EndsWith("\""))
                {
                    newToken = newToken.Substring(1, newToken.Length - 2);
                    Children.Add(new BooleanSearchItem<T>() { OriginalDataset = this.OriginalDataset, Parent = this, Children = null, SearchQuery = newToken, Type = BooleanSearchItemType.ExactSearchString });
                }
                else if (newToken == "OR")
                {
                    Children.Add(new BooleanSearchItem<T>() { OriginalDataset = this.OriginalDataset, Parent = this, Children = null, SearchQuery = newToken, Type = BooleanSearchItemType.BooleanOperatorOR });
                }
                else if (newToken == "AND")
                {
                    Children.Add(new BooleanSearchItem<T>() { OriginalDataset = this.OriginalDataset, Parent = this, Children = null, SearchQuery = newToken, Type = BooleanSearchItemType.BooleanOperatorAND });
                }
                else if (newToken == "NOT")
                {
                    Children.Add(new BooleanSearchItem<T>() { OriginalDataset = this.OriginalDataset, Parent = this, Children = null, SearchQuery = newToken, Type = BooleanSearchItemType.BooleanOperatorNOT });
                }
                else
                {
                    Children.Add(new BooleanSearchItem<T>() { OriginalDataset = this.OriginalDataset, Parent = this, Children = null, SearchQuery = newToken, Type = BooleanSearchItemType.SearchString });
                }
            }
        }

        public List<T> GetResults()
        {
            FillLists();

            //Results = PerformBooleanOperations();

            //while there is something not solved perform FillListsB???
            int iteraties = 0;
            while (TreeContainsUnsolvedNode())
            {
                iteraties += 1;
                FillListsB();
            }

            Console.WriteLine($"Aantal iteraties voor oplossen hele boom: " + iteraties.ToString());

            Console.WriteLine();
            foreach (var item in Results)
            {
                Console.WriteLine(item.ToString());
            }
            Console.WriteLine();

            return Results.ToList();
        }

        private bool TreeContainsUnsolvedNode()
        {
            if (!Solved)
            {
                return true;
            }

            if (Type == BooleanSearchItemType.Evaluate)
            {
                foreach (var child in Children)
                {
                    child.TreeContainsUnsolvedNode();
                }
            }

            return false;
        }

        private IEnumerable<T> FillLists()
        {
            if (Type == BooleanSearchItemType.SearchString)
            {
                Results = OriginalDataset.Where(x => x.ContainsSearchQuery(SearchQuery));
                Solved = true;
                SolvedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            else if (Type == BooleanSearchItemType.ExactSearchString)
            {
                Results = OriginalDataset.Where(x => x.ContainsSearchQuery(SearchQuery, true));
                Solved = true;
                SolvedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            else if (Type == BooleanSearchItemType.Evaluate)
            {
                foreach (var child in Children)
                {
                    Results = Results.Union(child.FillLists());
                }
            }

            return Results;
        }

        private IEnumerable<T> FillListsB()
        {
            if (Type == BooleanSearchItemType.SearchString || Type == BooleanSearchItemType.ExactSearchString)
            {
                return Results;
            }

            if (Type == BooleanSearchItemType.Evaluate)
            {
                foreach (var child in Children)
                {
                    //Results.Union(child.FillListsB());
                    if (child.TreeContainsUnsolvedNode())
                    {
                        Results = child.FillListsB();
                    }
                }

                Solved = true;
                SolvedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                foreach (var child in Children)
                {
                    if (child.TreeContainsUnsolvedNode())
                    {
                        Solved = false;
                    }
                }

                // Set parent solved = true when elements child are all solved
                // What happens if any of the child is a Evaluate which has children that are not solved yet??

                //if (!(Children.Any(x => !x.Solved)))
                //{
                //    Solved = true;
                //}
                //if (TreeContainsUnsolvedNode())
                //{
                //    Solved = false;
                //} else
                //{
                //    Solved = true;
                //}
            }

            if (Type == BooleanSearchItemType.BooleanOperatorAND || Type == BooleanSearchItemType.BooleanOperatorOR || Type == BooleanSearchItemType.BooleanOperatorNOT)
            {
                // If next sibling is not Solved we cannot perform this action
                var next = GetNextSibling();
                var previous = GetPreviousSibling();

                if (previous.Solved && next.Solved)
                {
                    bool nextIsExact = next.Type == BooleanSearchItemType.ExactSearchString;
                    bool previousIsExact = previous.Type == BooleanSearchItemType.ExactSearchString;

                    if (Type == BooleanSearchItemType.BooleanOperatorAND)
                    {
                        if (previous.Type == BooleanSearchItemType.Evaluate && next.Type == BooleanSearchItemType.Evaluate)
                        {
                            // Two datasets (previous and next) returns the elements which exists in both datasets
                            Results = next.Results.Intersect(previous.Results);
                        }
                        else if (previous.Type != BooleanSearchItemType.Evaluate && next.Type != BooleanSearchItemType.Evaluate)
                        {
                            // TODO DUBBELCHECK
                            //Results = OriginalDataset.Where(x => x.ContainsSearchQuery(previous.SearchQuery) && x.ContainsSearchQuery(next.SearchQuery));
                            Results = Parent.Results.Where(x => x.ContainsSearchQuery(previous.SearchQuery, previousIsExact) && x.ContainsSearchQuery(next.SearchQuery, nextIsExact));
                        }
                        else if (previous.Type == BooleanSearchItemType.Evaluate && next.Type != BooleanSearchItemType.Evaluate)
                        {
                            Results = previous.Results.Where(x => x.ContainsSearchQuery(next.SearchQuery, nextIsExact));
                        }
                        else if (previous.Type != BooleanSearchItemType.Evaluate && next.Type == BooleanSearchItemType.Evaluate)
                        {
                            Results = next.Results.Where(x => x.ContainsSearchQuery(previous.SearchQuery, previousIsExact));
                        }
                    }
                    else if (Type == BooleanSearchItemType.BooleanOperatorOR)
                    {
                        if (previous.Type == BooleanSearchItemType.Evaluate && next.Type == BooleanSearchItemType.Evaluate)
                        {
                            // Two datasets (previous and next) returns the elements which exists in one of the datasets
                            Results = next.Results.Union(previous.Results);
                        }
                        else if (previous.Type != BooleanSearchItemType.Evaluate && next.Type != BooleanSearchItemType.Evaluate)
                        {
                            Results = Parent.Results.Where(x => x.ContainsSearchQuery(previous.SearchQuery, previousIsExact) || x.ContainsSearchQuery(next.SearchQuery, nextIsExact));
                        }
                        else if (previous.Type == BooleanSearchItemType.Evaluate && next.Type != BooleanSearchItemType.Evaluate)
                        {
                            Results = previous.Results.Union(next.Results);
                        }
                        else if (previous.Type != BooleanSearchItemType.Evaluate && next.Type == BooleanSearchItemType.Evaluate)
                        {
                            Results = next.Results.Union(previous.Results);
                        }
                    }
                    else if (Type == BooleanSearchItemType.BooleanOperatorNOT)
                    {
                        if (previous.Type == BooleanSearchItemType.Evaluate && next.Type == BooleanSearchItemType.Evaluate)
                        {
                            // Elements in previous.Results that don't exist in next.Result
                            Results = previous.Results.Except(next.Results);
                        }
                        else if (previous.Type != BooleanSearchItemType.Evaluate && next.Type != BooleanSearchItemType.Evaluate)
                        {
                            Results = Parent.Results.Where(x => !x.ContainsSearchQuery(next.SearchQuery, nextIsExact));
                        }
                        else if (previous.Type == BooleanSearchItemType.Evaluate && next.Type != BooleanSearchItemType.Evaluate)
                        {
                            Results = previous.Results.Where(x => !x.ContainsSearchQuery(next.SearchQuery, nextIsExact));
                        }
                        else if (previous.Type != BooleanSearchItemType.Evaluate && next.Type == BooleanSearchItemType.Evaluate)
                        {
                            // TODO DUBBELCHECK
                            //Results = previous.Results.Where(x => !x.ContainsSearchQuery(next.SearchQuery));
                            Results = Parent.Results.Where(x => !x.ContainsSearchQuery(next.SearchQuery, nextIsExact));
                        }
                    }

                    Solved = true;
                    SolvedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
                else
                {
                    Solved = false;
                }
            }

            return Results;
        }

        //private IEnumerable<T> PerformBooleanOperations()
        //{
        //    if (Type == BooleanSearchItemType.SearchString || Type == BooleanSearchItemType.ExactSearchString)
        //    {
        //        return Results;
        //    }

        //    if (Type == BooleanSearchItemType.Evaluate)
        //    {
        //        for (int i = 0; i < Children.Count; i++)
        //        {
        //            var currentChild = Children[i];
        //            Results = currentChild.PerformBooleanOperations();
        //        }
        //    }
        //}

        //private List<T> PerformBooleanOperations()
        //{
        //    for (int i = 0; i < Children.Count; i++)
        //    {
        //        var currentChild = Children[i];

        //        if (Type == BooleanSearchItemType.Evaluate)
        //        {
        //            Results = currentChild.PerformBooleanOperations();
        //        } else
        //        {
        //            if(Type == BooleanSearchItemType.SearchString || Type == BooleanSearchItemType.ExactSearchString)
        //            {
        //                continue;
        //            } else
        //            {
        //                var childLeadingSibling = Children[i - 1];
        //                var childTrailingSibling = Children[i + 1];

        //                if (Type == BooleanSearchItemType.BooleanOperatorAND)
        //                {
        //                    Results = currentChild.OriginalDataset.Where(x => childLeadingSibling.Results.Contains(x) && childTrailingSibling.Results.Contains(x)).ToList();
        //                }
        //                else if (Type == BooleanSearchItemType.BooleanOperatorOR)
        //                {
        //                    Results = currentChild.OriginalDataset.Where(x => childLeadingSibling.Results.Contains(x) || childTrailingSibling.Results.Contains(x)).ToList();
        //                }
        //                else if (Type == BooleanSearchItemType.BooleanOperatorNOT)
        //                {

        //                }
        //            }
        //        }
        //    }
        //}

        public BooleanSearchItem<T> GetNextSibling()
        {
            if (Parent == null)
            {
                return null; // Er is geen volgende broer als er geen ouder is of als de knoop zelf null is.
            }

            int index = Parent.Children.IndexOf(this);

            if (index < 0 || index == Parent.Children.Count - 1)
            {
                return null; // Er is geen volgende broer als de knoop de laatste is in de lijst met kinderen.
            }

            return Parent.Children[index + 1];
        }

        public BooleanSearchItem<T> GetPreviousSibling()
        {
            if (Parent == null)
            {
                return null; // Er is geen volgende broer als er geen ouder is of als de knoop zelf null is.
            }

            int index = Parent.Children.IndexOf(this);

            if (index < 0 || index == Parent.Children.Count - 1)
            {
                return null; // Er is geen volgende broer als de knoop de laatste is in de lijst met kinderen.
            }

            return Parent.Children[index - 1];
        }

        public override string ToString()
        {
            string temp = $"{(Parent == null ? "Parent is null" : $"Parent({Parent.Type.ToString()}) heeft {Parent.Children.Count} children")} | {(Children == null ? "Mijn children zijn null" : $"Ik heb {Children.Count} children")} en zelf ben ik een {Type.ToString()} \n";

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    temp += child.ToString();
                }
            }

            return temp;
        }

        public string ToStringNice(int level = 0)
        {
            string indent = new string(' ', level * 3); // Aanpassen van het aantal spaties per niveau

            string result = $"{indent}+- {SearchQuery} ({Type.ToString()} (Results: {(Results.Count())}) (Solved: {Solved} | Time: {SolvedAt}))\n";

            if (Children != null)
            {
                foreach (var child in Children)
                {
                    result += child.ToStringNice(level + 1); // Recursief aanroepen met een verhoogd niveau
                }
            }

            return result;
        }
    }
}
