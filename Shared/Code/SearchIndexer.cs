using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using WebUtility = System.Net.WebUtility;
using System.Text;
using Lawspot.Backend;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;
using System.Data.Linq;

namespace Lawspot.Shared
{
    public static class SearchIndexer
    {
        private static object indexLock = new object();
        private static StandardAnalyzer analyzer = new StandardAnalyzer(Version.LUCENE_29);
        private static IndexSearcher searcher;

        /// <summary>
        /// Gets the directory containing the search index.
        /// </summary>
        private static FSDirectory AppData
        {
            get
            {
                return FSDirectory.Open(new DirectoryInfo(System.Web.Hosting.HostingEnvironment.MapPath(@"~/App_Data")));
            }
        }

        /// <summary>
        /// Rebuilds the search index from scratch.
        /// </summary>
        public static void RebuildIndex()
        {
            lock (indexLock)
            {
                // Open the search index for writing.
                using (var writer = new IndexWriter(AppData, analyzer, true, IndexWriter.MaxFieldLength.UNLIMITED))
                using (var dataContext = new LawspotDataContext())
                {
                    // Delete all existing documents.
                    writer.DeleteAll();

                    dataContext.CommandTimeout = int.MaxValue;
                    var dataOptions = new DataLoadOptions();
                    dataOptions.LoadWith<Question>(q => q.ReviewedByUser);
                    dataOptions.LoadWith<Question>(q => q.Answers);
                    dataOptions.LoadWith<Answer>(a => a.CreatedByUser);
                    dataOptions.LoadWith<Answer>(a => a.ReviewedByUser);
                    dataContext.LoadOptions = dataOptions;

                    foreach (var question in dataContext.Questions)
                    {
                        // Create a new document.
                        var document = CreateDocument(question);

                        // Add the new document.
                        if (document != null)
                            writer.AddDocument(document);
                    }
                }

                // Invalidate the searcher.
                searcher = null;
            }
        }

        //utility class for output of TextWriter for the Visual Sudio Debug window
        class DebugTextWriter : System.IO.TextWriter
        {
            public override void Write(char[] buffer, int index, int count)
            {
                System.Diagnostics.Debug.Write(new String(buffer, index, count));
            }

            public override void Write(string value)
            {
                System.Diagnostics.Debug.Write(value);
            }

            public override Encoding Encoding
            {
                get { return System.Text.Encoding.Default; }
            }
        }

        /// <summary>
        /// Updates a question in the search index.
        /// </summary>
        /// <param name="q"> The question to update. </param>
        public static void UpdateQuestion(Question q)
        {
            try
            {
                lock (indexLock)
                {
                    using (var writer = new IndexWriter(AppData, analyzer, false, IndexWriter.MaxFieldLength.UNLIMITED))
                    {
                        // Remove older documents.
                        writer.DeleteDocuments(new Term[] { new Term("ID", q.QuestionId.ToString()) });

                        // Create a new document.
                        var document = CreateDocument(q);

                        // Add the new document.
                        if (document != null)
                            writer.AddDocument(document);
                    }

                    // Invalidate the searcher.
                    searcher = null;
                }
            }
            catch (FileNotFoundException)
            {
                // The index file doesn't exist.
                RebuildIndex();
            }
        }

        /// <summary>
        /// Creates a document in the search index.
        /// </summary>
        /// <param name="q"> The question to use as the data source for the document. </param>
        /// <returns> A populated document. </returns>
        private static Document CreateDocument(Question q)
        {
            var doc = new Document();
            doc.Add(new Field("Title", q.Title, Field.Store.NO, Field.Index.ANALYZED));
            var details = new StringBuilder(q.Details);
            var publicDetails = new StringBuilder(q.Details);
            int approvedAnswerCount = 0;
            if (q.ReviewedByUser != null)
            {
                details.Append(" ... ");
                details.Append(q.ReviewedByUser.EmailDisplayName);
            }
            foreach (var answer in q.Answers)
            {
                details.Append(" ... ");
                details.Append(answer.Details);
                details.Append(" ... ");
                details.Append(answer.CreatedByUser.EmailDisplayName);
                if (answer.ReviewedByUser != null)
                {
                    details.Append(" ... ");
                    details.Append(answer.ReviewedByUser.EmailDisplayName);
                }
                if (answer.Status == AnswerStatus.Approved)
                {
                    approvedAnswerCount++;
                    publicDetails.Append(" ... ");
                    publicDetails.Append(answer.Details);
                }
            }
            doc.Add(new Field("Details", details.ToString(), Field.Store.NO, Field.Index.ANALYZED));
            doc.Add(new Field("PublicDetails", publicDetails.ToString(), Field.Store.NO, Field.Index.ANALYZED));
            doc.Add(new Field("ID", q.QuestionId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED));
            doc.Add(new Field("Public", (q.Status == QuestionStatus.Approved && approvedAnswerCount >= 1).ToString(), Field.Store.NO, Field.Index.NOT_ANALYZED));
            return doc;
        }

        /// <summary>
        /// A search hit, containing the ID of the question and a snippet of HTML.
        /// </summary>
        public class SearchHit
        {
            /// <summary>
            /// The query that resulted in this hit.
            /// </summary>
            public Query Query { get; set; }

            /// <summary>
            /// The ID of the question.
            /// </summary>
            public int ID { get; set; }
        }

        /// <summary>
        /// Searches for the given text.
        /// </summary>
        /// <param name="queryText"> The text to search for. </param>
        /// <param name="publicOnly"> Indicates whether only published questions should be searched. </param>
        /// <returns> A list of search hits. </returns>
        public static IEnumerable<SearchHit> Search(string queryText, bool publicOnly = true)
        {
            if (queryText == null)
                throw new ArgumentNullException("queryText");
            queryText = queryText.Replace("*", "");
            queryText = queryText.Replace("?", "");
            queryText = queryText.Trim();
            if (queryText == string.Empty)
                return new SearchHit[0];

            var results = new List<SearchHit>();
            var parser = new MultiFieldQueryParser(Version.LUCENE_29, new string[] { "Title", publicOnly ? "PublicDetails" : "Details" }, analyzer);

            Query query;
            try
            {
                query = parser.Parse(queryText);
            }
            catch (ParseException)
            {
                query = parser.Parse(QueryParser.Escape(queryText));
            }

            if (publicOnly)
            {
                var publicOnlyQuery = new BooleanQuery();
                publicOnlyQuery.Add(new TermQuery(new Term("Public", true.ToString())), BooleanClause.Occur.MUST);
                publicOnlyQuery.Add(query, BooleanClause.Occur.MUST);
                query = publicOnlyQuery;
            }

            lock (indexLock)
            {
                if (searcher == null)
                {
                    try
                    {
                        searcher = new IndexSearcher(AppData, false);
                    }
                    catch
                    {
                        RebuildIndex();
                        searcher = new IndexSearcher(AppData, false);
                    }
                }
                var hits = searcher.Search(query, null, 100);
                foreach (var hit in hits.ScoreDocs)
                {
                    var document = searcher.Doc(hit.Doc);
                    results.Add(new SearchHit {
                        Query = query,
                        ID = int.Parse(document.Get("ID")),
                    });
                }
            }
            return results;
        }

        /// <summary>
        /// Creates a snippet highlighting the search terms.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="document"></param>
        /// <param name="maxLength"></param>
        /// <param name="startHtml"></param>
        /// <param name="endHtml"></param>
        /// <returns></returns>
        public static string CreateSnippetHtml(Query query, string document, int maxLength, string startHtml, string endHtml)
        {
            // Extract the list of search terms.
            var searchTerms = new List<Term>();
            query.ExtractTerms(searchTerms);
            var terms = searchTerms.Select(st => st.Text()).Distinct().ToList();
            
            // Count the number of terms existing in the document.
            int start = 0;
            int termCount = 0;
            foreach (var term in terms)
            {
                while (true)
                {
                    var termIndex = document.IndexOf(term, start, StringComparison.InvariantCultureIgnoreCase);
                    if (termIndex == -1)
                        break;
                    if ((termIndex == 0 || document[termIndex - 1] == ' ') &&
                        (termIndex + term.Length >= document.Length || char.IsWhiteSpace(document[termIndex + term.Length]) || char.IsPunctuation(document[termIndex + term.Length])))
                    {
                        termCount++;
                    }
                    start = termIndex + term.Length;
                }
            }

            if (termCount == 0)
                return WebUtility.HtmlEncode(StringUtilities.SummarizeText(document, maxLength));

            var result = new StringBuilder();
            start = 0;
            int snippetLength = maxLength / termCount / 2;

            while (true)
            {
                // Find the next search term.
                int foundIndex = document.Length;
                string foundTerm = string.Empty;
                foreach (var term in terms)
                {
                    var termIndex = document.IndexOf(term, start, StringComparison.InvariantCultureIgnoreCase);
                    if (termIndex >= 0 && termIndex < foundIndex)
                    {
                        if ((termIndex == 0 || document[termIndex - 1] == ' ') &&
                            (termIndex + term.Length >= document.Length || char.IsWhiteSpace(document[termIndex + term.Length]) || char.IsPunctuation(document[termIndex + term.Length])))
                        {
                            foundIndex = termIndex;
                            foundTerm = term;
                        }
                    }
                }

                if (foundIndex == document.Length)
                {
                    string text = document.Substring(start);
                    if (text.Length > snippetLength)
                        text = text.Substring(0, snippetLength) + " ...";
                    result.Append(WebUtility.HtmlEncode(text));
                    break;
                }
                else if (start == 0)
                {
                    string text = document.Substring(0, foundIndex);
                    if (text.Length > snippetLength)
                        text = "... " + text.Substring(text.Length - snippetLength, snippetLength);
                    result.Append(WebUtility.HtmlEncode(text));
                }
                else
                {
                    string inBetween = document.Substring(start, foundIndex - start);
                    if (inBetween.Length > snippetLength * 2)
                        inBetween = inBetween.Substring(0, snippetLength) + " ... " +
                            inBetween.Substring(inBetween.Length - snippetLength);
                    result.Append(WebUtility.HtmlEncode(inBetween));
                }

                result.Append(startHtml);
                result.Append(WebUtility.HtmlEncode(document.Substring(foundIndex, foundTerm.Length)));
                result.Append(endHtml);

                start = foundIndex + foundTerm.Length;
            }
            return result.ToString();
        }
    }
}