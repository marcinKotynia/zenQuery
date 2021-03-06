﻿using System.Data;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SQLite;
using System.Collections.Specialized;
using System;
using System.Windows.Forms;

namespace crycore
{

    /// <summary>
    /// Klasa importuje dane do tabeli tblSnippets
    /// Zarzadza akcjami uzytkownika
    /// </summary>
    class actions
    {
        static Regex regexITEM = new Regex("<zenITEM>(.*?)</zenITEM>",
RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static Regex regexAUTHOR = new Regex("<zenAUTHOR>(.*?)</zenAUTHOR>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static Regex regexDESCRIPTION = new Regex("<zenDESCRIPTION>(.*?)</zenDESCRIPTION>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static Regex regexDATABASE = new Regex("<zenDATABASE>(.*?)</zenDATABASE>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static Regex regexPROVIDER = new Regex("<zenPROVIDER>(.*?)</zenPROVIDER>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static Regex regexOBJECTTYPE = new Regex("<zenOBJECTTYPE>(.*?)</zenOBJECTTYPE>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static Regex regexOBJECTMASK = new Regex("<zenOBJECTMASK>(.*?)</zenOBJECTMASK>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static Regex regexACTION = new Regex("<zenACTION>(.*?)</zenACTION>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

        static Regex regexTYPE = new Regex("<zenTYPE>(.*?)</zenTYPE>",
        RegexOptions.Singleline | RegexOptions.CultureInvariant | RegexOptions.Compiled);


        public static void fillActions()
        {
            mk.Logic.simpleDebug.dump();
            string appPath =  System.IO.Directory.GetCurrentDirectory();
            ParseFiles(appPath, "xml");

        }

        /// <summary>
        /// Wypelnia snippety dla okna scintilli
        /// </summary>
        /// <param name="doc">biezace okno scintilli</param>
        /// <param name="_type"> 
        /// 1	zwykle snippety	Query window Snippets
        /// 2	browser snippets Browser Snippets
        /// </param>
        /// <param name="_provider">ORACLE,MSSQL,ALL </param>
        public static void fillSnippets(ref ScintillaNET.Scintilla doc, int _type, string _provider)
        {
            mk.Logic.simpleDebug.dump();


            doc.Snippets.List.Clear();
            IDataReader r = mk.msqllite.GetDataReader("select description,strsql from tblsnipitem where type = '" + _type + "' and (provider = 'ALL' or provider= '" + _provider + "')   order by description;");
            while (r.Read())
            {

                doc.Snippets.List.Add(r[0].ToString(), r[1].ToString(), char.Parse("@"), false);
            }
            r.Close();
        }

      
        /// <summary>
        /// Wywolanie
        /// string appPath =  System.IO.Directory.GetCurrentDirectory();
        /// </summary>
        /// <param name="path"></param>
        /// <param name="optionalparam"></param>
         static void ParseFiles(string path, string fileMask)
        {
            mk.Logic.simpleDebug.dump();
             //usuniecie wsyztskiego
            mk.msqllite.mSQLLiteExecute(false, "delete from tblsnipitem  ", null, null);


            StringBuilder sb = new StringBuilder();


            System.Collections.ArrayList files = new System.Collections.ArrayList();
            DirSearch(path, ref files, fileMask);

            //czytanie pliku
            string content;

            foreach (string f in files)
            {
                if (File.Exists(f))
                {
                    FileInfo t = new FileInfo(f);
                    System.Diagnostics.Debug.Print(f + " " + t.Length.ToString());
                  //  if (t.Length < 10024)
                   // {
                        using (System.IO.StreamReader sr = new System.IO.StreamReader(f))
                        {
                            content = sr.ReadToEnd();
                            ParseRegex(content); //przetwarzanie pojedynczego pliku
                        }
                    //}
                }
            }

        }
        /// <summary>
        /// Znalezeinie Item, gdyz jeden plik moze zawierac wiecej itemow
        /// </summary>
        static void ParseRegex(string content)
         {
             mk.Logic.simpleDebug.dump();
            if (content.Length < 10) return;

            MatchCollection mc;
            mc = regexITEM.Matches(content);
            foreach (Match m in mc) //<zenITEM>
            {
                ParseRegexItem(m.Groups[0].Value);

            }

        }


        /// <summary>
        /// Dodanie Pozycji
        /// </summary>
        /// <param name="content"></param>
         static void ParseRegexItem(string content)
        {
            mk.Logic.simpleDebug.dump();
            if (content.Length < 1) return;


            //Action a = new Action(regexACTION.Match(content).Value,
            //    regexDESCRIPTION.Match(content).Value,
            //    regexDATABASE.Match(content).Value,
            //    regexPROVIDER.Match(content).Value,
            //    regexOBJECTTYPE.Match(content).Value,
            //    regexOBJECTMASK.Match(content).Value,
            //    regexACTION.Match(content).Value,
            //    regexTYPE.Match(content).Value
            //     );

             string  description= regexDESCRIPTION.Match(content).Groups[1].Value;
             string strsql=regexACTION.Match(content).Groups[1].Value ;
             string type =regexTYPE.Match(content).Groups[1].Value ;
             string database = regexDATABASE.Match(content).Groups[1].Value ;
             string provider=regexPROVIDER.Match(content).Groups[1].Value ;
             string objecttype=regexOBJECTTYPE.Match(content).Groups[1].Value ;
             string objectmask=regexOBJECTMASK.Match(content).Groups[1].Value ;

            List<SQLiteParameter> paramss = new List<SQLiteParameter>();
            paramss.Add(new SQLiteParameter("@Description", description ));

             //sprawdzenie czy istnieje 
            bool check;
            check =  mk.msqllite.ifExist("select 1 from tblsnipitem where description = '" + description +  "' ");

            paramss.Add(new SQLiteParameter("@strsql", strsql ));
            paramss.Add(new SQLiteParameter("@type", type));
            paramss.Add(new SQLiteParameter("@database", database ));
            paramss.Add(new SQLiteParameter("@provider", provider ));
            paramss.Add(new SQLiteParameter("@objecttype", objecttype));
            paramss.Add(new SQLiteParameter("@objectmask", objectmask));
            if (check) //istnieje
            {
                mk.msqllite.mSQLLiteExecute(false, "update tblsnipitem  set description = @description,strsql=@strsql,type=@type,database=@database,provider=@provider,objecttype=@objecttype,objectmask=@objectmask where snipitemID = @snipitemID", paramss, null);
            }
            else
            {
                mk.msqllite.mSQLLiteExecute(false, "insert into tblsnipitem  (Description,strsql,type,database,provider,objecttype,objectmask ) values (@Description,@strsql,@type,@database,@provider,@objecttype,@objectmask)", paramss, null);
            }
            
        }

        //Method to Search Directory for specified file Type.Paste it outside the function
         static void DirSearch(string sDir, ref System.Collections.ArrayList files, string mask)
        {
            try
            {
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    // System.Diagnostics.Debug.Print(sDir);
                    foreach (string f in Directory.GetFiles(d))
                    {
                        files.Add(f);
                        //System.Diagnostics.Debug.Print(f);
                    }
                    DirSearch(d, ref files, mask);
                }
            }
            catch (System.Exception excpt)
            {
                System.Diagnostics.Debug.Print(excpt.Message);
            }
        }

      public  static void getText(string Text, string startText, string endText, out string TextPart, out int endPoint)
         {
             mk.Logic.simpleDebug.dump();
             int startPoint;
             TextPart = "";
             endPoint = -1;

             startPoint = Text.IndexOf(startText);
             if (startPoint > -1)
             {
                 endPoint = Text.IndexOf(endText, startPoint);

                 //wyechstrachowanie i oczyszczenei komendy do wykonanaia
                 TextPart = Text.Substring(startPoint + startText.Length, endPoint - startPoint - startText.Length); //ekstrakcja komendy
             }

         }



         static string NZ(object input)
         {
             string text = input.ToString();
             if (string.IsNullOrEmpty(text))
                 return "";
             else
                 return text;
         }

         /// <summary>
         /// Komenda do tekstu
         /// </summary>
         /// <param name="sqlcommand"></param>
         /// <returns></returns>
        public static string getactionTextSqlParam(string sqlcommand, string options,ref zenQuery.DbClient _dbClient)
         {
             mk.Logic.simpleDebug.dump();
             try
             {


                 string prefix, sufix;
                 bool removePrefix, removeSufix;
                 string[] strArr = options.Split('|');


                 prefix = NZ(strArr[0]);
                 sufix = NZ(strArr[1]);
                 removePrefix = Convert.ToBoolean(strArr[2]);
                 removeSufix = Convert.ToBoolean(strArr[3]);



                 DataSet ds = _dbClient.Execute(sqlcommand, 7);
                 if (ds == null || ds.Tables.Count == 0) return "/* \n No rows returned from Action. Prepared command \n " + sqlcommand + "\n*/";

                 StringBuilder sb = new StringBuilder();


                 int items = ds.Tables[0].Rows.Count;
                 int numerator = 0;

                 string pattern;
                 foreach (DataRow r in ds.Tables[0].Rows)
                 {
                     numerator++;
                     pattern = "";

                     if (numerator == 1 && removePrefix)
                         pattern = "";
                     else
                         pattern = "{0}";


                     pattern += "{1}";

                     if (items == numerator && removeSufix)
                         pattern += "";
                     else
                         pattern += "{2}";

                     sb.Append(string.Format(pattern, prefix, r[0].ToString(), sufix));
                 }
                 ds.Dispose();


                 if (items == 0)
                     sb.Append("/* \n No rows returned from Action. Prepared command \n " + sqlcommand + "\n*/");

                 sb.Replace("\n", Environment.NewLine);

                 return sb.ToString().Trim();
             }
             catch (Exception e)
             {

                 return "ERROR " + e.Message.ToString();
             }
         }

    }


}
