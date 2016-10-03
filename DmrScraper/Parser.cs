using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlAgilityPack;

namespace Dmr
{


    public class Parser
    {

        const string HIDDEN_TOKEN_NAME = "dmrFormToken";

        private HtmlDocument _doc = new HtmlDocument();
       
        public Parser(string html = "")
        {
            if(!string.IsNullOrEmpty(html))
                _doc.LoadHtml(html);
        }

        public void LoadHtml(string html)
        {
            _doc.LoadHtml(html);
        }

        public string GetAuthenticationToken()
        {
            return _doc.DocumentNode
                .SelectSingleNode("//input[@name='" + HIDDEN_TOKEN_NAME + "']")
                .GetAttributeValue("value", null);
        }

        public List<Entity> GetVehicle()
        {
            if (_doc.DocumentNode.InnerText.Contains("Ingen køretøjer fundet"))
                return null;

            var list = new List<Entity>();

            // sæt rod node: body -> .h-tab-content-inner
            var root =_doc.DocumentNode.SelectSingleNode("//div[@class='h-tab-content-inner']");

            if (root == null)
                return null;

            // nu har(biurde) vi en liste der består af -> h2 #text div[id='id*']...
            // de tre første noder er ikke nøvendige, 
            // et h tag, en tekst node og et div med en form: h2 #text div
            var rows = root.ChildNodes.Where(x => x.Name == "div").Skip(1).ToList();

            bool isFirst = true;
            foreach (var row in rows)
            {
                IEnumerable<Entity> entities;
                if (isFirst)
                {
                    isFirst = false;
                    entities = GetFirstRowEntities(row);
                    list.AddRange(entities);
                }
                else
                {
                    entities = GetRowEntities(row);
                    list.AddRange(entities);
                }
            }

            return list;
        }

        private IEnumerable<HtmlNode> GetUnits(HtmlNode node)
        {
            return node.ChildNodes.Where(x => x.GetAttributeValue("class", "").Contains("unit"));
        }

        private IEnumerable<HtmlNode> GetLines(HtmlNode node)
        {
            return node.ChildNodes.Where(x => x.GetAttributeValue("class", "").Contains("line"));
        }

        private HtmlNode GetLine(HtmlNode node)
        {
            return GetLines(node).FirstOrDefault();
        }

        private string GetCategory(HtmlNode node)
        {
            return GetPrettyString(node.SelectSingleNode("h3").InnerText);
        }

        private IEnumerable<Entity> GetFirstRowEntities(HtmlNode row)
        {

            var line = GetLine(row);

            var units = GetUnits(line).ToList();

            var unitsSpecial = 
                GetUnits(line.ChildNodes.First(x => x.Name == "div")).ToList();

            if (units == null || units.Count() != 2)
                throw new Exception("Found an unexpected number of units. Expected 2 saw " + units.Count());

            if (unitsSpecial == null || unitsSpecial.Count() != 2)
                throw new Exception("Found an unexpected number of special units. Expected 2 saw " + units.Count());

            HtmlNode unitA = units.First();
            HtmlNode unitB = units.Last(); 

            HtmlNode unitSpecialA = unitsSpecial.First();
            HtmlNode unitSpecialB = unitsSpecial.Last();

            string categoryA = GetCategory(unitSpecialA);
            string categoryB = GetCategory(unitSpecialB);

            // result
            List<Entity> result = new List<Entity>();

            var divsA =
                unitSpecialA.ChildNodes
                .First(x => x.GetAttributeValue("class", "") == "bluebox").ChildNodes
                .Where(y => y.GetAttributeValue("class", "") == "notrequired keyvalue singleDouble");

            foreach (HtmlNode divA in divsA)
            {
                var model = GetModelFromSpecialUnits(divA, categoryA);
                result.Add(model);
            }

            foreach (var lineA in GetLines(unitA)) 
            {
                var innerUnitsAA = GetUnits(lineA).ToList();
                var modelAA = GetModelFromInnerUnits(innerUnitsAA, categoryA);
                result.Add(modelAA);
            }

            IEnumerable<HtmlNode> divsB =
                unitSpecialB.ChildNodes
                .First(x => x.GetAttributeValue("class", "") == "bluebox").ChildNodes
                .Where(y => y.GetAttributeValue("class", "") == "notrequired keyvalue singleDouble")
                .ToList();
            
            foreach (var divB in divsB)
            {
                var model = GetModelFromSpecialUnits(divB, categoryB);
                result.Add(model);
            }

            foreach (var lineB in GetLines(unitB))
            {
                var innerUnitsBB = GetUnits(lineB).ToList();
                var modelBB = GetModelFromInnerUnits(innerUnitsBB, categoryB);
                result.Add(modelBB);
            }

            return result;
        }

        private List<Entity> GetRowEntities(HtmlNode row)
        {
            // result
            var result = new List<Entity>();

            var category = GetCategory(row);

            // here we loop throgh 1 or 2 units
            foreach (var unit in GetUnits(GetLine(row)))
            {
                var lines = GetLines(unit);
                foreach (var line in lines)
                {
                    // key (label) / value er også .unit 
                    var units = GetUnits(line);
                    var model = GetModelFromInnerUnits(units, category);
                    result.Add(model);
                }
            }
            return result;
        }

        private IEnumerable<HtmlNode> GetAllHvrContainers(HtmlNode node)
        {
            return node.ChildNodes
                .First(x => x.GetAttributeValue("class", "") == "bluebox").ChildNodes
                .Where(y => y.GetAttributeValue("class", "") == "notrequired keyvalue singleDouble")
                .ToList(); 
        }

        private Entity GetModelFromInnerUnits(IEnumerable<HtmlNode> innerUnits, string category = "")
        {
            var label = GetPrettyString(innerUnits.First().InnerText);
            var slug = GetSlug(label);
            var path = GetSlug(category) + "/" + slug;
            var value = innerUnits.Last().SelectSingleNode("span").InnerText;
            var model = new Entity()
            {
                Path = path,
                Category = category,
                Slug = slug,
                Label = label,
                Value = value
            };
            return model;        
        }

        private Entity GetModelFromSpecialUnits(HtmlNode innerUnits, string category = "")
        {
            var divsBTrimmed = innerUnits.ChildNodes.Where(da => da.Name == "span");

            var label = GetPrettyString(divsBTrimmed.First().InnerText);
            var slug = GetSlug(label);
            var value = GetPrettyString(divsBTrimmed.Last().InnerText);
            var path = GetSlug(category) + "/" + slug;
            var model = new Entity()
            {
                Path = path,
                Category = category,
                Slug = slug,
                Label = label,
                Value = value
            };
            return model;
        }

        private char[] _trim = "\r\n\t:;,. ".ToCharArray();
        private char[] _replaceWithEmpty = ":;,. ()".ToCharArray();
        private char[] _replaceWithDash = "/\\ ".ToCharArray();

        private string GetSlug(string s)
        {
            string result = GetPrettyString(s);
            result = result.Replace("æ", "ae");
            result = result.Replace("ø", "oe");
            result = result.Replace("å", "aa");            
            result = Encoding.ASCII.GetString(Encoding.GetEncoding("Cyrillic").GetBytes(result));
            foreach (var c in _replaceWithEmpty)
                result = result.Replace(c.ToString(), string.Empty);
            foreach (var c in _replaceWithDash)
                result = result.Replace(c.ToString(), "-");
            return result.ToLower().Trim(_trim);
        }

        private string GetPrettyString(string s)
        {
            string result = s.Trim(_trim);
            return result.Replace("&shy;", " ");
        }

    }
}
