using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TweetSharp;

namespace Atom_Bandung.Controllers
{
    public class HomeController : Controller
    {
        public static string mention_search;
        public static string PDAM_search;
        public static string PJS_search;
        public static string PJU_search;
        public static string Disdik_search;
        public static string Diskominfo_search;
        public static bool use_BM;
        public static List<TwitterStatus>[] stat;

        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult atom_form()
        {
            return View();
        }

        [HttpPost]
        public ActionResult atom_result(string Mention, string PDAM, string PJS, string PJU, string Disdik, string Diskominfo, string Algorithm)
        {
            /* Variabel Global Untuk Search*/
            mention_search = string.Copy(Mention);
            PDAM_search = string.Copy(PDAM);
            PJS_search = string.Copy(PJS);
            PJU_search = string.Copy(PJU);
            Disdik_search = string.Copy(Disdik);
            Diskominfo_search = string.Copy(Diskominfo);

            /*Local Variable*/
            string[] tes = new string[5];
            tes[0] = PDAM_search.ToLower();
            tes[1] = PJS_search.ToLower();
            tes[2] = PJU_search.ToLower();
            tes[3] = Disdik_search.ToLower();
            tes[4] = Diskominfo_search.ToLower();
            List<string[]> pemkot = new List<string[]>();
            List<string> menti = mention_search.Split(';').ToList<string>();

            /*Use wether BM or KMB Algorithm*/
            if (Algorithm.Equals("BM"))
            {
                use_BM = true;
            }
            else
            {
                use_BM = false;
            }

            //TwitterService
            var service = new TwitterService("5q4cQb8Q9YyOl35vPRZLx8Jjq", "UmFtUuVDIsrOrLI3g4qs8HqYGWjIQ3O4ghfhAu6Sb5Y9QkN2lx");
            //Authenticate
            service.AuthenticateWith("178535721-rRphja9pNDIDK3p1JOx55Joly19vgzRTFBivuNmB", "XU4EFfVN4tlwbGu75POv4FurI571cznMIw3IBPrqIgWSG");

            List<TwitterStatus> tweet = new List<TwitterStatus>();
            
            //Search by key
            if (menti.Count != 0)
            {
                foreach (string ment in menti)
                {
                    TwitterSearchResult searchRes = service.Search(new SearchOptions { Q = ment.ToLower(), Count = 100, });
                    var tweets = searchRes.Statuses;

                    if (tweets != null)
                    {
                        foreach (TwitterStatus t in tweets)
                        {
                            tweet.Add(t);
                        }
                    }
                }
            }

            

            /*Beginning of iti*/

            //Seperate semicolom (;)
            for (int i = 0; i < 5; i++)
            {
                pemkot.Add(tes[i].Split(';'));
            }

            /**============FILTER===========**/
            stat = new List<TwitterStatus>[6];
            for (int i = 0; i < 6; i++)
            {
                stat[i] = new List<TwitterStatus>();
            }
            KMP kmp = new KMP();
            BM bm = new BM();
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < pemkot[j].Length; i++)
                {
                    string x = pemkot[j][i];
                    List<TwitterStatus> tweetx = new List<TwitterStatus>(tweet);
                    foreach (TwitterStatus t in tweetx)
                    {
                        if (use_BM == false)
                        {
                            //KMP Algorithm
                            if (kmp.KMPSearch(t.Text, x))
                            {
                                stat[j].Add(t);
                                tweet.Remove(t);
                            }
                        }
                        else
                        {
                            //BM ALgorithm
                            if (bm.BMSearch(t.Text, x))
                            {
                                stat[j].Add(t);
                                tweet.Remove(t);
                            }
                        }
                    }


                }
            }



            stat[5] = new List<TwitterStatus>(tweet);
            tweet.Clear();

            LocationAnalyzer la = new LocationAnalyzer();
            List<string>[] location = new List<string>[6];
            for(int o = 0 ; o < 6 ; o++)
            {
                location[o] = new List<string>();
                foreach(TwitterStatus tx in stat[0])
                {
                    string temp = la.getTempat(tx.Text);
                    if (temp != "")
                    {
                        location[o].Add(temp);
                    }
                }
            }
            
            ViewBag.PDAM = stat[0];
            ViewBag.PJU = stat[1];
            ViewBag.Dissos = stat[2];
            ViewBag.Disdik = stat[3];
            ViewBag.Diskominfo = stat[4];
            ViewBag.Other = stat[5];

            ViewBag.PDAMloc = location[0];
            ViewBag.PJUloc = location[1];
            ViewBag.Dissosloc = location[2];
            ViewBag.Disdikloc = location[3];
            ViewBag.Diskominfloc = location[4];
            ViewBag.Otherloc = location[5];
            ViewBag.OtherlocCount = location[5].Count;
            
            ViewBag.PDAM_Count = stat[0].Count;
            ViewBag.PJU_Count = stat[1].Count;
            ViewBag.Dissos_Count = stat[2].Count;
            ViewBag.Disdik_Count = stat[3].Count;
            ViewBag.Diskominfo_Count = stat[4].Count;
            ViewBag.Other_Count = stat[5].Count;

            /*END of ITI*/
            return View();
        }
    }

    /*Algoritma String Search Booyer - Moore*/
    class BM
    {
        public bool BMSearch(string texts, string pats)
        {
            bool contain;
            string text = texts.ToLower();
            string pat = pats.ToLower();
            List<int> loc = new List<int>();
            int m = pat.Length;
            int n = text.Length;

            int[] badChar = BadChars(pat, m);

            int s = 0;
            while (s <= (n - m))
            {
                int j = m - 1;

                while (j >= 0 && pat[j] == text[s + j])
                    --j;

                if (j < 0)
                {
                    loc.Add(s);
                    s += (s + m < n) ? m - badChar[text[s + m]] : 1;
                }
                else
                {
                    if (text[s + j] <= 255)
                    {
                        s += Math.Max(1, j - badChar[text[s + j]]);
                    }
                    else
                    {
                        s++;
                    }
                }
            }
            if (loc.Count == 0)
            {
                contain = false;
            }
            else
            {
                contain = true;
            }
            return contain;
        }
        private int[] BadChars(string tex, int size)
        {
            string text = tex.ToLower();
            int i;
            List<int> badChar = new List<int>();
            for (i = 0; i < 256; i++)
            {
                badChar.Add(-1);
            }
            for (i = 0; i < size; i++)
            {
                badChar[(int)text[i]] = i;
            }
            return badChar.ToArray();
        }
    }

    /*Algoritma String Search Booyer Moore*/
    class KMP
    {
        public bool KMPSearch(string textx, string patternx)
        {
            string text = textx.ToLower();
            string pattern = patternx.ToLower();
            List<int> loc = new List<int>();
            bool x;
            int M = pattern.Length;
            int N = text.Length;
            int i = 0;
            int j = 0;
            int[] KMPTab = new int[M];

            CreateKMPTab(pattern, KMPTab);

            while (i < N)
            {
                if (pattern[j] == text[i])
                {
                    j++;
                    i++;
                }

                if (j == M)
                {
                    loc.Add(i - j);
                    j = KMPTab[j - 1];
                }

                else if (i < N && pattern[j] != text[i])
                {
                    if (j != 0)
                        j = KMPTab[j - 1];
                    else
                        i = i + 1;
                }
            }
            if (loc.Count == 0)
            {
                x = false;
            }
            else
            {
                x = true;
            }
            return x;
        }

        private void CreateKMPTab(string patternx, int[] KMPTab)
        {
            string pattern = patternx.ToLower();
            int l = 0; int i = 1; int x = pattern.Length; KMPTab[0] = 0;
            while (i < x)
            {
                if (pattern[i] == pattern[l])
                {
                    l++; KMPTab[i] = l; i++;
                }
                else
                {
                    if (l != 0)
                    {
                        l = KMPTab[l - 1];
                    }
                    else
                    {
                        KMPTab[i] = 0; i++;
                    }
                }
            }
        }
    }

    class LocationAnalyzer
    {
        /*Location Analyzer*/
        public string getTempat(string input)
        {
            List<String> kamustempat = "dago dubai india dipatiukur tamansari bandung jakarta yogyakarta medan indonesia singapura bali lembang".Split(' ').ToList<String>();
            String text = input.ToLower();
            List<String> token = text.Split(' ').ToList<String>();
            int indeks = -1;
            foreach (String tmp in kamustempat)
            {
                if (text.ToLower().Contains(tmp))
                {
                    indeks = token.IndexOf(tmp);
                }
            }
            if (indeks == -1)
            {
                List<String> katadepan = new List<String>();
                katadepan.Add("di");
                katadepan.Add("ke");
                katadepan.Add("dari");
                bool foundkdep = false;
                int i = 0;
                while ((!foundkdep) && (i < katadepan.Count))
                {
                    if (text.ToLower().Contains(katadepan[i]))
                    {
                        foundkdep = true;
                    }
                    else
                    {
                        i++;
                    }
                }
                if (foundkdep)
                {
                    String katdeppak = katadepan[i];
                    int indekskatdep = token.IndexOf(katdeppak);
                    String tempat = "";
                    indekskatdep++;
                    while ((indekskatdep < token.Count) && (!token[indekskatdep].EndsWith(".") || !token[indekskatdep].EndsWith(",") || !token[indekskatdep].EndsWith("!") || !token[indekskatdep].EndsWith("?")))
                    {
                        tempat = tempat + token[indekskatdep] + " ";
                        indekskatdep++;
                    }
                    tempat = tempat.Substring(0, tempat.Length - 2);
                    return tempat;
                }
                else
                {
                    return "";
                }
            }
            else
            {
                return token[indeks];
            }
        }
    }




}
