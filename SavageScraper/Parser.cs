using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SavageScraper
{
    class Parser
    {
        private string inputFilePath, outputFilePath;

        public Parser(string inputFilePath, string outputFilePath)
        {
            this.inputFilePath = inputFilePath;
            this.outputFilePath = outputFilePath;
            File.Delete(outputFilePath);
        }

        //Start by scanning all the topic tags and later all abandoned category tags.
        public void Parse()
        {
            string findTagString = "(<.*?>)|(.+?(?=<|$))";
            Regex findTag = new Regex(findTagString);

            List<string> words = findTag.Split(File.ReadAllText(inputFilePath)).ToList();

            LookForTopicTags(words);
            LookForCategoryTags(words);
        }


        //deals with topic tag and every category tag within it. --LIKE-A-BOSS--
        private void ProcessTopicTag(List<string> tagContents)
        {
            int numberOfCategoryTags = DetermineNumberOfCategoryTags(tagContents);

            int start = tagContents[0].IndexOf("\"") + 1;
            int end = tagContents[0].LastIndexOf("\"");

            //name attribute of the topic tag
            string topicName = tagContents[0].Substring(start, end - start);

            //now finding category tags  
            for (int i = 1; i <= numberOfCategoryTags; i++)
            {
                start = GetNthOccurance(tagContents, "<category>", i);
                end = GetNthOccurance(tagContents, "</category>", i);


                var categoryContent = tagContents.GetRange(start, end - start);

                ProcessCategoryTag(categoryContent, topicName);
            }

        }


        private void ProcessCategoryTag(List<string> category, string topicName)
        {
            string patternContent = "", templateContent = "", thatContent = "";

            //look for pattern tag
            int start = category.IndexOf("<pattern>") + 1;
            int end = category.IndexOf("</pattern>");
            for (int i = start; i < end; i++)
            {
                patternContent += category[i];
            }

            //for that tag
            start = category.IndexOf("<that>") + 1;
            end = category.IndexOf("</that>");
            for (int i = start; i < end; i++)
            {
                thatContent += category[i];
            }

            //for template tag
            start = category.IndexOf("<template>") + 1;
            end = category.IndexOf("</template>");
            for (int i = start; i < end; i++)
            {
                templateContent += category[i];
            }

            Console.WriteLine("pattern: {0}\ntemplate: {1}\nthat: {2}\ntopic: {3}", patternContent, templateContent, thatContent, topicName);

            WriteToCsvFile(patternContent, thatContent, templateContent, topicName);
        }


        private int DetermineNumberOfCategoryTags(List<string> tagContents)
        {
            int count = 0;
            foreach (string tag in tagContents)
            {
                if (tag == "<category>")
                    count++;
            }
            return count;
        }


        private int GetNthOccurance(List<string> list, string word, int n)
        {
            int count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == word)
                {
                    count++;
                    if (count == n)
                        return i;
                }
            }
            return -1;
        }


        private void LookForTopicTags(List<string> words)
        {
            List<string> tagContents = new List<string>();

            if (!words.Contains("</topic>"))                  //changed here
                return;

            for (int i = 0; i < words.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(words[i]) || string.IsNullOrEmpty(words[i]))
                    words.Remove(words[i]);
            }

            //check for topic tags
            for (int i = 0; i < words.Count; i++)
            {
                if (words[i].Contains("<topic"))
                {
                    tagContents.Add(words[i]);
                    while (words[i] != "</topic>")
                    {
                        i++;
                        tagContents.Add(words[i]);
                    }
                }
            }

            ProcessTopicTag(tagContents);
        }


        private void LookForCategoryTags(List<string> words)
        {
            bool toProcess = false;
            int start, end;
            List<string> tagContents = new List<string>();

            for (int i = 0; i < words.Count; i++)
            {
                if (words[i] == "<category>")
                {
                    toProcess = isIndependent(words, i);
                    if (toProcess)
                    {
                        start = i;
                        i++;
                        while (words[i] != "</category>" && i < words.Count - 1)
                            i++;
                        end = i;

                        tagContents = words.GetRange(start, end - start);
                        ProcessCategoryTag(tagContents, "");
                    }
                }

            }
        }


        private bool isIndependent(List<string> words, int index)
        {
            for (int i = index; i > 0; i--)
            {
                if (words[i].Contains("</topic>"))
                    return true;
                else if (words[i].Contains("<topic"))     //changed here words[i] == topic
                    return false;
            }
            return true;

        }


        private void WriteToCsvFile(string patternContent, string thatContent, string templateContent, string topicName)
        {
            if (topicName == "") topicName = "*";
            if (thatContent == "") thatContent = "*";

            int start = (inputFilePath.LastIndexOf("\\")) + 1;
            int end = inputFilePath.Length - start;
            string fileName = inputFilePath.Substring(start, end);            

            MakeCsvCompatible(ref patternContent, ref thatContent, ref templateContent, ref topicName, ref fileName);

            string row = "0, " + patternContent + ", " + thatContent + ", " + topicName + ", " + templateContent + ", " + fileName + Environment.NewLine;
            File.AppendAllText(outputFilePath, row);
        }


        private void MakeCsvCompatible(ref string patternContent, ref string thatContent, ref string templateContent, ref string topicName, ref string fileName)
        {
            patternContent = patternContent.Trim().Replace(Environment.NewLine, String.Empty).Replace(", ", "#Comma ").Replace("\n", String.Empty)
                .Replace(",", "#Comma ").Replace("\t", " ");

            thatContent = thatContent.Trim().Replace(Environment.NewLine, String.Empty).Replace(", ", "#Comma ").Replace("\n", String.Empty)
                .Replace(",", "#Comma ").Replace("\t", " ");

            templateContent = templateContent.Trim().Replace(Environment.NewLine, String.Empty).Replace(", ", "#Comma ").Replace("\n", String.Empty)
                .Replace(",", "#Comma ").Replace("\t", " ");

            topicName = topicName.Trim().Replace(Environment.NewLine, String.Empty).Replace(", ", "#Comma ").Replace("\n", String.Empty)
                .Replace(",", "#Comma ").Replace("\t", " ");

            fileName = fileName.Trim().Replace(Environment.NewLine, String.Empty).Replace(", ", "#Comma ").Replace("\n", String.Empty)
                .Replace(",", "#Comma ").Replace("\t", " ");
        }
    }
}
